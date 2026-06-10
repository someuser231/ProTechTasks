using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using ProTechTasks.Algorithms;
using ProTechTasks.Models;

namespace ProTechTasks.Benchmarks;

public class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        WithOptions(ConfigOptions.DisableOptimizationsValidator);
        AddLogger(DefaultConfig.Instance.GetLoggers().ToArray());
        AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().ToArray());
        AddExporter(DefaultConfig.Instance.GetExporters().ToArray());
    }
}

[MemoryDiagnoser]
[Config(typeof(BenchmarkConfig))]
public class SearchBenchmarks
{
    private List<Driver> _drivers = new List<Driver>();

    private LinearSortAlgorithm _linear = new LinearSortAlgorithm();
    private PriorityQueueAlgorithm _priorityQueue = new PriorityQueueAlgorithm();
    private GridPartitionAlgorithm _grid = new GridPartitionAlgorithm(cellSize: 10);

    [Params(100, 1000, 10000)]
    public int DriverCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _drivers = new List<Driver>();

        for (int i = 1; i <= DriverCount; i++)
        {
            var driver = new Driver();
            driver.Id = i;
            driver.X = random.Next(0, 1000);
            driver.Y = random.Next(0, 1000);
            _drivers.Add(driver);
        }

        _grid.BuildIndex(_drivers);
    }

    [Benchmark]
    public List<Driver> LinearSort()
    {
        return _linear.FindNearest(_drivers, 500, 500, 5);
    }

    [Benchmark]
    public List<Driver> PriorityQueue()
    {
        return _priorityQueue.FindNearest(_drivers, 500, 500, 5);
    }

    [Benchmark]
    public List<Driver> GridPartition()
    {
        return _grid.FindNearest(500, 500, 5);
    }
}
