using BenchmarkDotNet.Running;
using ProTechTasks.Algorithms;
using ProTechTasks.Benchmarks;
using ProTechTasks.Models;

if (args.Length > 0 && args[0] == "--benchmark")
{
    BenchmarkRunner.Run<SearchBenchmarks>();
    return;
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

int parallelLimit = app.Configuration.GetSection("Settings").GetValue<int>("ParallelLimit");
app.UseMiddleware<ProTechTasks.Middleware.ParallelLimitMiddleware>(parallelLimit);

app.UseSwagger();
app.UseSwaggerUI();

int N = app.Configuration.GetSection("MapSettings").GetValue<int>("N");
int M = app.Configuration.GetSection("MapSettings").GetValue<int>("M");
bool allowMultiplePerCell = app.Configuration.GetSection("Settings").GetValue<bool>("AllowMultipleDriversPerCell");

var drivers = new List<Driver>();
var occupiedPositions = new HashSet<(int, int)>();

var linearAlgorithm = new LinearSortAlgorithm();
var priorityQueueAlgorithm = new PriorityQueueAlgorithm();
var gridAlgorithm = new GridPartitionAlgorithm(cellSize: 10);
gridAlgorithm.BuildIndex(drivers);

app.MapGet("/", () => "ProTechTasks Driver Search API\n");

app.MapGet("/drivers", () =>
{
    return drivers;
});

app.MapPut("/drivers", (Driver request) =>
{
    Driver existing = drivers.FirstOrDefault(d => d.Id == request.Id);

    if (request.X < 0 || request.X >= N || request.Y < 0 || request.Y >= M)
    {
        if (existing != null)
        {
            occupiedPositions.Remove((existing.X, existing.Y));
            drivers.Remove(existing);
            gridAlgorithm.BuildIndex(drivers);
            app.Logger.LogInformation("Водитель {Id} удалён из-за выхода за пределы карты ({X}, {Y})", request.Id, request.X, request.Y);
        }
        app.Logger.LogWarning("Некорректные координаты для водителя {Id}: ({X}, {Y})", request.Id, request.X, request.Y);
        return Results.BadRequest("Координаты некорректны");
    }

    if (!allowMultiplePerCell && occupiedPositions.Contains((request.X, request.Y)) && !(existing != null && existing.X == request.X && existing.Y == request.Y))
    {
        app.Logger.LogWarning("Координаты ({X}, {Y}) заняты, запрос водителя {Id} отклонён", request.X, request.Y, request.Id);
        return Results.BadRequest("Здесь уже находится другой водитель");
    }

    if (existing != null)
    {
        occupiedPositions.Remove((existing.X, existing.Y));
        existing.X = request.X;
        existing.Y = request.Y;
        occupiedPositions.Add((request.X, request.Y));
        gridAlgorithm.BuildIndex(drivers);
        app.Logger.LogInformation("Координаты водителя {Id} изменены на ({X}, {Y})", request.Id, request.X, request.Y);
        return Results.Ok("Координаты успешно изменены");
    }

    occupiedPositions.Add((request.X, request.Y));
    Driver driver = new Driver();
    driver.Id = request.Id;
    driver.X = request.X;
    driver.Y = request.Y;
    drivers.Add(driver);
    gridAlgorithm.BuildIndex(drivers);
    app.Logger.LogInformation("Добавлен водитель {Id} с координатами ({X}, {Y})", request.Id, request.X, request.Y);
    return Results.Ok("Координаты успешно добавлены");
});

app.MapGet("/search", (int x, int y, HttpContext http) =>
{
    if (x < 0 || x >= N || y < 0 || y >= M)
    {
        return Results.BadRequest($"Coordinates out of bounds. Valid range: 0 <= x < {N}, 0 <= y < {M}");
    }

    int count = 5;
    if (http.Request.Query.ContainsKey("count"))
    {
        count = int.Parse(http.Request.Query["count"]!);
    }

    var linearResult = linearAlgorithm.FindNearest(drivers, x, y, count);
    var priorityQueueResult = priorityQueueAlgorithm.FindNearest(drivers, x, y, count);
    var gridResult = gridAlgorithm.FindNearest(x, y, count);

    var response = new
    {
        orderX = x,
        orderY = y,
        count = count,
        results = new
        {
            linear = linearResult,
            priorityQueue = priorityQueueResult,
            grid = gridResult
        }
    };

    return Results.Ok(response);
});

app.MapPost("/orders", async (OrderRequest request, IHttpClientFactory httpClientFactory) =>
{
    if (request.X < 0 || request.X >= N || request.Y < 0 || request.Y >= M)
    {
        app.Logger.LogWarning("Некорректные координаты заказа {Id}: ({X}, {Y})", request.Id, request.X, request.Y);
        return Results.BadRequest("Координаты некорректны");
    }

    if (drivers.Count == 0)
    {
        app.Logger.LogWarning("Нет свободных водителей для заказа {Id}", request.Id);
        return Results.BadRequest("Свободных водителей нет");
    }

    List<Driver> nearest = gridAlgorithm.FindNearest(request.X, request.Y, 5);

    if (nearest.Count == 0)
    {
        app.Logger.LogWarning("Нет свободных водителей для заказа {Id}", request.Id);
        return Results.BadRequest("Свободных водителей нет");
    }

    int index = 0;
    try
    {
        HttpClient client = httpClientFactory.CreateClient();
        string url = $"http://www.randomnumberapi.com/api/v1.0/random?min=0&max={nearest.Count - 1}&count=1";
        string response = await client.GetStringAsync(url);
        int[] numbers = System.Text.Json.JsonSerializer.Deserialize<int[]>(response)!;
        index = numbers[0];
    }
    catch
    {
        app.Logger.LogWarning("Удалённый API недоступен, используется локальный генератор случайных чисел");
        Random rng = new Random();
        index = rng.Next(0, nearest.Count);
    }

    Driver selectedDriver = nearest[index];

    List<object> route = new List<object>();
    int cx = selectedDriver.X;
    int cy = selectedDriver.Y;

    route.Add(new { x = cx, y = cy });

    int stepX = request.X > cx ? 1 : -1;
    while (cx != request.X)
    {
        cx += stepX;
        route.Add(new { x = cx, y = cy });
    }

    int stepY = request.Y > cy ? 1 : -1;
    while (cy != request.Y)
    {
        cy += stepY;
        route.Add(new { x = cx, y = cy });
    }

    int routeLength = Math.Abs(selectedDriver.X - request.X) + Math.Abs(selectedDriver.Y - request.Y);

    var result = new
    {
        driverId = selectedDriver.Id,
        driverX = selectedDriver.X,
        driverY = selectedDriver.Y,
        routeLength = routeLength,
        route = route
    };

    app.Logger.LogInformation("Заказу {Id} назначен водитель {DriverId}, длина маршрута {Length}", request.Id, selectedDriver.Id, routeLength);
    return Results.Ok(result);
});

app.Run();
