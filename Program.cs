using ProTechTasks.Algorithms;
using ProTechTasks.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var drivers = new List<Driver>();

var random = new Random();
for (int i = 1; i <= 20; i++)
{
    var driver = new Driver();
    driver.Id = i;
    driver.X = random.Next(0, 100);
    driver.Y = random.Next(0, 100);
    drivers.Add(driver);
}

var linearAlgorithm = new LinearSortAlgorithm();
var priorityQueueAlgorithm = new PriorityQueueAlgorithm();
var gridAlgorithm = new GridPartitionAlgorithm(cellSize: 10);

app.MapGet("/", () => "ProTechTasks Driver Search API\n");

app.MapGet("/drivers", () =>
{
    return drivers;
});

app.MapGet("/search", (int x, int y, HttpContext http) =>
{
    int count = 5;
    if (http.Request.Query.ContainsKey("count"))
    {
        count = int.Parse(http.Request.Query["count"]!);
    }

    var linearResult = linearAlgorithm.FindNearest(drivers, x, y, count);
    var priorityQueueResult = priorityQueueAlgorithm.FindNearest(drivers, x, y, count);
    var gridResult = gridAlgorithm.FindNearest(drivers, x, y, count);

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
