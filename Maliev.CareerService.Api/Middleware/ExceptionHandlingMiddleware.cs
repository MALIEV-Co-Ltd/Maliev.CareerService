using System.Net;
using System.Text.Json;

namespace Maliev.CareerService.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Get correlation ID from the context
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? 
                               context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ??
                               Guid.NewGuid().ToString();
            
            _logger.LogError(ex, "An unhandled exception has occurred: {Message}. Correlation ID: {CorrelationId}", 
                ex.Message, correlationId);
            
            await HandleExceptionAsync(context, ex, correlationId);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, string correlationId)
    {
        context.Response.ContentType = "application/json";
        
        var response = context.Response;
        var errorResponse = new ErrorResponse
        {
            CorrelationId = correlationId
        };

        switch (exception)
        {
            case ArgumentException argEx:
                errorResponse.Message = argEx.Message;
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.ErrorType = "ArgumentException";
                break;
            case KeyNotFoundException:
                errorResponse.Message = "Resource not found.";
                errorResponse.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.ErrorType = "NotFound";
                break;
            case UnauthorizedAccessException:
                errorResponse.Message = "Unauthorized access.";
                errorResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.ErrorType = "Unauthorized";
                break;
            case InvalidOperationException invalidOpEx:
                errorResponse.Message = invalidOpEx.Message;
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.ErrorType = "InvalidOperation";
                break;
            default:
                // In development, include exception details for debugging
                if (context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
                {
                    errorResponse.Message = exception.Message;
                    errorResponse.StackTrace = exception.StackTrace;
                }
                else
                {
                    errorResponse.Message = "An internal server error occurred.";
                }
                errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.ErrorType = "InternalServerError";
                break;
        }

        response.StatusCode = errorResponse.StatusCode;
        
        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        await response.WriteAsync(jsonResponse);
    }

    private class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string ErrorType { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
    }
}