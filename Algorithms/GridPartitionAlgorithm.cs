using ProTechTasks.Models;

namespace ProTechTasks.Algorithms;

public class GridPartitionAlgorithm
{
    private readonly int _cellSize;

    public GridPartitionAlgorithm(int cellSize = 50)
    {
        _cellSize = cellSize;
    }

    public List<Driver> FindNearest(List<Driver> drivers, int orderX, int orderY, int count = 5)
    {
        var grid = new Dictionary<(int, int), List<Driver>>();

        for (int i = 0; i < drivers.Count; i++)
        {
            Driver driver = drivers[i];
            int cellX = driver.X / _cellSize;
            int cellY = driver.Y / _cellSize;
            var cellKey = (cellX, cellY);

            if (!grid.ContainsKey(cellKey))
            {
                grid[cellKey] = new List<Driver>();
            }

            grid[cellKey].Add(driver);
        }

        int orderCellX = orderX / _cellSize;
        int orderCellY = orderY / _cellSize;

        var candidates = new List<Driver>();
        int radius = 0;

        while (true)
        {
            if (radius == 0)
            {
                CollectCell(grid, orderCellX, orderCellY, candidates);
            }
            else
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        if (Math.Abs(dx) == radius || Math.Abs(dy) == radius)
                        {
                            CollectCell(grid, orderCellX + dx, orderCellY + dy, candidates);
                        }
                    }
                }
            }

            if (candidates.Count >= count)
            {
                double minNextRingDist = Math.Max(0.0, (radius - 1) * _cellSize);

                candidates.Sort((a, b) =>
                {
                    double distA = GetDistance(a, orderX, orderY);
                    double distB = GetDistance(b, orderX, orderY);
                    return distA.CompareTo(distB);
                });

                if (candidates.Count > count)
                {
                    candidates = candidates.Take(count).ToList();
                }

                double kthDistance = GetDistance(candidates[count - 1], orderX, orderY);

                if (kthDistance <= minNextRingDist)
                {
                    return candidates;
                }
            }

            radius++;

            if (radius > 2000)
            {
                break;
            }
        }

        candidates.Sort((a, b) => GetDistance(a, orderX, orderY).CompareTo(GetDistance(b, orderX, orderY)));
        return candidates.Take(count).ToList();
    }

    private static void CollectCell(Dictionary<(int, int), List<Driver>> grid, int cx, int cy, List<Driver> result)
    {
        var cellKey = (cx, cy);
        if (grid.ContainsKey(cellKey))
        {
            result.AddRange(grid[cellKey]);
        }
    }

    private static double GetDistance(Driver driver, int x, int y)
    {
        double dx = driver.X - x;
        double dy = driver.Y - y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
