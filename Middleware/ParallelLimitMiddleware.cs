namespace ProTechTasks.Middleware;

public class ParallelLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly int _limit;
    private int _currentCount = 0;

    public ParallelLimitMiddleware(RequestDelegate next, int limit)
    {
        _next = next;
        _limit = limit;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        int current = Interlocked.Increment(ref _currentCount);

        if (current > _limit)
        {
            Interlocked.Decrement(ref _currentCount);
            context.Response.StatusCode = 503;
            await context.Response.WriteAsync("Service Unavailable");
            return;
        }

        try
        {
            await _next(context);
        }
        finally
        {
            Interlocked.Decrement(ref _currentCount);
        }
    }
}
