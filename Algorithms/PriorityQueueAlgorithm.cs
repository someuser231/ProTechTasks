using ProTechTasks.Models;

namespace ProTechTasks.Algorithms;

public class PriorityQueueAlgorithm
{
    public List<Driver> FindNearest(List<Driver> drivers, int orderX, int orderY, int count = 5)
    {
        var queue = new PriorityQueue<Driver, double>();

        for (int i = 0; i < drivers.Count; i++)
        {
            Driver driver = drivers[i];
            double dx = driver.X - orderX;
            double dy = driver.Y - orderY;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            queue.Enqueue(driver, distance);
        }

        var result = new List<Driver>();
        for (int i = 0; i < count && queue.Count > 0; i++)
        {
            result.Add(queue.Dequeue());
        }

        return result;
    }
}
