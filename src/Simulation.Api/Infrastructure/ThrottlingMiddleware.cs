using System.Collections.Concurrent;

namespace SimulationServer.Api.Infrastructure;
public class ThrottlingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<Guid, (DateTime lastRequestTime, int requestCount)> _requestLog = new();
    private readonly TimeSpan _throttlingTimeSpan = TimeSpan.FromSeconds(1); // Throttle window
    private readonly int _requestLimit = 5; // Maximum allowed requests within time span

    public ThrottlingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/upload"))
        {
            // Extract the actorId from the route
            if (context.Request.RouteValues.TryGetValue("actorId", out var actorIdValue) && Guid.TryParse(actorIdValue?.ToString(), out var actorId))
            {
                // Throttling logic
                var now = DateTime.UtcNow;

                if (_requestLog.TryGetValue(actorId, out var log))
                {
                    // Check if the last request was within the throttling window
                    if (now - log.lastRequestTime < _throttlingTimeSpan)
                    {
                        // Increment request count
                        if (log.requestCount >= _requestLimit)
                        {
                            // Throttle the request
                            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                            await context.Response.WriteAsync("Too many requests, please try again later.");
                            return;
                        }

                        // Update request count
                        _requestLog[actorId] = (log.lastRequestTime, log.requestCount + 1);
                    }
                    else
                    {
                        // Reset request count if outside throttling window
                        _requestLog[actorId] = (now, 1);
                    }
                }
                else
                {
                    // Add actorId to the log for the first request
                    _requestLog[actorId] = (now, 1);
                }
            }
        }

        // Call the next middleware in the pipeline
        await _next(context);
    }
}
