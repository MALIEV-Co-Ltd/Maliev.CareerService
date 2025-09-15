using System.Diagnostics;
using Maliev.CareerService.Api.HealthChecks;

namespace Maliev.CareerService.Api.Middleware;

public class ResponseTimeMiddleware
{
    private readonly RequestDelegate _next;

    public ResponseTimeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            ResponseTimeHealthCheck.RecordResponseTime(stopwatch.ElapsedMilliseconds);
        }
    }
}

public static class ResponseTimeMiddlewareExtensions
{
    public static IApplicationBuilder UseResponseTimeMonitoring(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ResponseTimeMiddleware>();
    }
}