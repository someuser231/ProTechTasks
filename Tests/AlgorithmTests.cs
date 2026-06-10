using NUnit.Framework;
using ProTechTasks.Algorithms;
using ProTechTasks.Models;

namespace ProTechTasks.Tests;

[TestFixture]
public class AlgorithmTests
{
    private LinearSortAlgorithm _linear;
    private PriorityQueueAlgorithm _priorityQueue;
    private GridPartitionAlgorithm _grid;

    [SetUp]
    public void SetUp()
    {
        _linear = new LinearSortAlgorithm();
        _priorityQueue = new PriorityQueueAlgorithm();
        _grid = new GridPartitionAlgorithm(cellSize: 10);
    }

    private void BuildGrid(List<Driver> drivers)
    {
        _grid.BuildIndex(drivers);
    }

    [Test]
    public void LinearSort_ReturnsCorrectNearest()
    {
        List<Driver> drivers = new List<Driver>();
        drivers.Add(new Driver { Id = 1, X = 0, Y = 0 });
        drivers.Add(new Driver { Id = 2, X = 1, Y = 1 });
        drivers.Add(new Driver { Id = 3, X = 5, Y = 5 });
        drivers.Add(new Driver { Id = 4, X = 10, Y = 10 });
        drivers.Add(new Driver { Id = 5, X = 20, Y = 20 });
        drivers.Add(new Driver { Id = 6, X = 50, Y = 50 });

        List<Driver> result = _linear.FindNearest(drivers, 0, 0, 5);

        Assert.That(result.Count, Is.EqualTo(5));
        Assert.That(result[0].Id, Is.EqualTo(1));
        Assert.That(result[1].Id, Is.EqualTo(2));
        Assert.That(result[2].Id, Is.EqualTo(3));
    }

    [Test]
    public void PriorityQueue_ReturnsCorrectNearest()
    {
        List<Driver> drivers = new List<Driver>();
        drivers.Add(new Driver { Id = 1, X = 0, Y = 0 });
        drivers.Add(new Driver { Id = 2, X = 1, Y = 1 });
        drivers.Add(new Driver { Id = 3, X = 5, Y = 5 });
        drivers.Add(new Driver { Id = 4, X = 10, Y = 10 });
        drivers.Add(new Driver { Id = 5, X = 20, Y = 20 });
        drivers.Add(new Driver { Id = 6, X = 50, Y = 50 });

        List<Driver> result = _priorityQueue.FindNearest(drivers, 0, 0, 5);

        Assert.That(result.Count, Is.EqualTo(5));
        Assert.That(result[0].Id, Is.EqualTo(1));
        Assert.That(result[1].Id, Is.EqualTo(2));
        Assert.That(result[2].Id, Is.EqualTo(3));
    }

    [Test]
    public void Grid_ReturnsCorrectNearest()
    {
        List<Driver> drivers = new List<Driver>();
        drivers.Add(new Driver { Id = 1, X = 0, Y = 0 });
        drivers.Add(new Driver { Id = 2, X = 1, Y = 1 });
        drivers.Add(new Driver { Id = 3, X = 5, Y = 5 });
        drivers.Add(new Driver { Id = 4, X = 10, Y = 10 });
        drivers.Add(new Driver { Id = 5, X = 20, Y = 20 });
        drivers.Add(new Driver { Id = 6, X = 50, Y = 50 });

        BuildGrid(drivers);
        List<Driver> result = _grid.FindNearest(0, 0, 5);

        Assert.That(result.Count, Is.EqualTo(5));
        Assert.That(result[0].Id, Is.EqualTo(1));
        Assert.That(result[1].Id, Is.EqualTo(2));
        Assert.That(result[2].Id, Is.EqualTo(3));
    }

    [Test]
    public void LinearSort_WhenLessDriversThanCount_ReturnsAll()
    {
        List<Driver> drivers = new List<Driver>();
        drivers.Add(new Driver { Id = 1, X = 5, Y = 5 });
        drivers.Add(new Driver { Id = 2, X = 10, Y = 10 });

        List<Driver> result = _linear.FindNearest(drivers, 0, 0, 5);

        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public void PriorityQueue_WhenLessDriversThanCount_ReturnsAll()
    {
        List<Driver> drivers = new List<Driver>();
        drivers.Add(new Driver { Id = 1, X = 5, Y = 5 });
        drivers.Add(new Driver { Id = 2, X = 10, Y = 10 });

        List<Driver> result = _priorityQueue.FindNearest(drivers, 0, 0, 5);

        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public void Grid_WhenLessDriversThanCount_ReturnsAll()
    {
        List<Driver> drivers = new List<Driver>();
        drivers.Add(new Driver { Id = 1, X = 5, Y = 5 });
        drivers.Add(new Driver { Id = 2, X = 10, Y = 10 });

        BuildGrid(drivers);
        List<Driver> result = _grid.FindNearest(0, 0, 5);

        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public void LinearSort_EmptyDrivers_ReturnsEmpty()
    {
        List<Driver> drivers = new List<Driver>();
        List<Driver> result = _linear.FindNearest(drivers, 0, 0, 5);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void PriorityQueue_EmptyDrivers_ReturnsEmpty()
    {
        List<Driver> drivers = new List<Driver>();
        List<Driver> result = _priorityQueue.FindNearest(drivers, 0, 0, 5);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Grid_EmptyDrivers_ReturnsEmpty()
    {
        List<Driver> drivers = new List<Driver>();
        BuildGrid(drivers);
        List<Driver> result = _grid.FindNearest(0, 0, 5);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void AllAlgorithms_ReturnSameDrivers()
    {
        Random random = new Random(42);
        List<Driver> drivers = new List<Driver>();

        for (int i = 1; i <= 50; i++)
        {
            Driver driver = new Driver();
            driver.Id = i;
            driver.X = random.Next(0, 100);
            driver.Y = random.Next(0, 100);
            drivers.Add(driver);
        }

        BuildGrid(drivers);

        List<Driver> linearResult = _linear.FindNearest(drivers, 50, 50, 5);
        List<Driver> priorityResult = _priorityQueue.FindNearest(drivers, 50, 50, 5);
        List<Driver> gridResult = _grid.FindNearest(50, 50, 5);

        List<int> linearIds = new List<int>();
        for (int i = 0; i < linearResult.Count; i++)
        {
            linearIds.Add(linearResult[i].Id);
        }
        linearIds.Sort();

        List<int> priorityIds = new List<int>();
        for (int i = 0; i < priorityResult.Count; i++)
        {
            priorityIds.Add(priorityResult[i].Id);
        }
        priorityIds.Sort();

        List<int> gridIds = new List<int>();
        for (int i = 0; i < gridResult.Count; i++)
        {
            gridIds.Add(gridResult[i].Id);
        }
        gridIds.Sort();

        Assert.That(priorityIds, Is.EqualTo(linearIds));
        Assert.That(gridIds, Is.EqualTo(linearIds));
    }
}
