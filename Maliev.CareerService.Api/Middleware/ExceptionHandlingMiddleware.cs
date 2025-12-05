using System.Net;
using System.Text.Json;
using Maliev.CareerService.Api.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Api.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IWebHostEnvironment environment)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;
    private readonly IWebHostEnvironment _environment = environment;
    /// <summary>
    /// Performs  I n v o k e asynchronously
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the asynchronous operation</returns>

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = exception switch
        {
            DbUpdateConcurrencyException => new ErrorResponse
            {
                Error = "Conflict",
                Message = "The resource was modified by another user. Please refresh and try again.",
                StatusCode = (int)HttpStatusCode.Conflict,
                TraceId = context.TraceIdentifier
            },
            ArgumentException or ArgumentNullException => new ErrorResponse
            {
                Error = "Bad Request",
                Message = exception.Message,
                StatusCode = (int)HttpStatusCode.BadRequest,
                TraceId = context.TraceIdentifier
            },
            UnauthorizedAccessException => new ErrorResponse
            {
                Error = "Unauthorized",
                Message = "You are not authorized to perform this action.",
                StatusCode = (int)HttpStatusCode.Unauthorized,
                TraceId = context.TraceIdentifier
            },
            KeyNotFoundException => new ErrorResponse
            {
                Error = "Not Found",
                Message = exception.Message,
                StatusCode = (int)HttpStatusCode.NotFound,
                TraceId = context.TraceIdentifier
            },
            _ => new ErrorResponse
            {
                Error = "Internal Server Error",
                Message = _environment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred. Please try again later.",
                StatusCode = (int)HttpStatusCode.InternalServerError,
                TraceId = context.TraceIdentifier,
                Details = _environment.IsDevelopment() ? exception.StackTrace : null
            }
        };

        response.StatusCode = errorResponse.StatusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }
}
