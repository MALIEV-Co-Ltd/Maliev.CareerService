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
            _logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = context.Response;
        var errorResponse = new ErrorResponse();

        switch (exception)
        {
            case ApplicationException:
                errorResponse.Message = exception.Message;
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;
            case KeyNotFoundException:
                errorResponse.Message = "Resource not found.";
                response.StatusCode = (int)HttpStatusCode.NotFound;
                break;
            case UnauthorizedAccessException:
                errorResponse.Message = "Unauthorized access.";
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;
            default:
                errorResponse.Message = "An internal server error occurred.";
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        errorResponse.StatusCode = response.StatusCode;
        
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
        public string TraceId { get; set; } = System.Diagnostics.Activity.Current?.Id ?? string.Empty;
    }
}