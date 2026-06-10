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
var app = builder.Build();

int N = 100;
int M = 100;

var drivers = new List<Driver>();
var random = new Random();
var occupiedPositions = new HashSet<(int, int)>();

for (int i = 1; i <= 20; i++)
{
    int x;
    int y;

    do
    {
        x = random.Next(0, N);
        y = random.Next(0, M);
    }
    while (occupiedPositions.Contains((x, y)));

    occupiedPositions.Add((x, y));

    var driver = new Driver();
    driver.Id = i;
    driver.X = x;
    driver.Y = y;
    drivers.Add(driver);
}

var linearAlgorithm = new LinearSortAlgorithm();
var priorityQueueAlgorithm = new PriorityQueueAlgorithm();
var gridAlgorithm = new GridPartitionAlgorithm(cellSize: 10);
gridAlgorithm.BuildIndex(drivers);

app.MapGet("/", () => "ProTechTasks Driver Search API\n");

app.MapGet("/drivers", () =>
{
    return drivers;
});

app.MapPut("/drivers/{id}", (int id, int x, int y) =>
{
    if (x < 0 || x >= N || y < 0 || y >= M)
    {
        return Results.BadRequest($"Coordinates out of bounds. Valid range: 0 <= x < {N}, 0 <= y < {M}");
    }

    if (occupiedPositions.Contains((x, y)) && !drivers.Any(d => d.Id == id && d.X == x && d.Y == y))
    {
        return Results.BadRequest("The position is busy");
    }

    var existing = drivers.FirstOrDefault(d => d.Id == id);

    if (existing != null)
    {
        occupiedPositions.Remove((existing.X, existing.Y));
        existing.X = x;
        existing.Y = y;
        occupiedPositions.Add((x, y));
        gridAlgorithm.BuildIndex(drivers);
        return Results.Ok(existing);
    }

    occupiedPositions.Add((x, y));
    var driver = new Driver();
    driver.Id = id;
    driver.X = x;
    driver.Y = y;
    drivers.Add(driver);
    gridAlgorithm.BuildIndex(drivers);
    return Results.Created($"/drivers/{id}", driver);
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

app.Run();
