using Serilog.Context;

namespace Maliev.CareerService.Api.Middleware;

public class LoggingContextMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add request context information to all logs for this request
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        using (LogContext.PushProperty("RequestPath", context.Request.Path))
        using (LogContext.PushProperty("RequestQueryString", context.Request.QueryString.HasValue ? context.Request.QueryString.Value : ""))
        using (LogContext.PushProperty("UserAgent", context.Request.Headers.UserAgent.ToString()))
        using (LogContext.PushProperty("ClientIP", context.Connection.RemoteIpAddress?.ToString()))
        {
            await _next(context);
        }
    }
}

public static class LoggingContextMiddlewareExtensions
{
    public static IApplicationBuilder UseLoggingContext(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LoggingContextMiddleware>();
    }
}