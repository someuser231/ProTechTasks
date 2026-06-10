using ProTechTasks.Models;

namespace ProTechTasks.Algorithms;

public class LinearSortAlgorithm
{
    public List<Driver> FindNearest(List<Driver> drivers, int orderX, int orderY, int count = 5)
    {
        var driversWithDistance = new List<(Driver driver, double distance)>();

        for (int i = 0; i < drivers.Count; i++)
        {
            Driver driver = drivers[i];
            double dx = driver.X - orderX;
            double dy = driver.Y - orderY;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            driversWithDistance.Add((driver, distance));
        }

        driversWithDistance.Sort((a, b) => a.distance.CompareTo(b.distance));

        var result = new List<Driver>();
        for (int i = 0; i < count && i < driversWithDistance.Count; i++)
        {
            result.Add(driversWithDistance[i].driver);
        }

        return result;
    }
}
