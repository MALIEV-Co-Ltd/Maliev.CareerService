using Maliev.CareerService.Api.Services;

namespace Maliev.CareerService.Api.Middleware;

/// <summary>
/// Middleware to track concurrent users accessing the API
/// </summary>
public class ConcurrentUsersMiddleware(
    RequestDelegate next,
    ILogger<ConcurrentUsersMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ConcurrentUsersMiddleware> _logger = logger;
    /// <summary>
    /// Performs  I n v o k e asynchronously
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <param name="metricsService">The metricsService</param>
    /// <returns>A task representing the asynchronous operation</returns>

    public async Task InvokeAsync(HttpContext context, IMetricsService metricsService)
    {
        // Increment concurrent users on request start
        metricsService.IncrementConcurrentUsers();

        try
        {
            // Process the request
            await _next(context);
        }
        finally
        {
            // Decrement concurrent users on request end (even if exception occurred)
            metricsService.DecrementConcurrentUsers();
        }
    }
}
