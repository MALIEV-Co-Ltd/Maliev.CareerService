namespace Maliev.CareerService.Api.Models;

public class ApiErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string ErrorType { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
    public string? StackTrace { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiErrorResponse FromException(Exception ex, string correlationId, bool includeStackTrace = false)
    {
        return new ApiErrorResponse
        {
            Message = ex.Message,
            StatusCode = ex switch
            {
                ArgumentException => 400,
                KeyNotFoundException => 404,
                UnauthorizedAccessException => 401,
                InvalidOperationException => 400,
                _ => 500
            },
            ErrorType = ex.GetType().Name,
            CorrelationId = correlationId,
            StackTrace = includeStackTrace ? ex.StackTrace : null,
            Timestamp = DateTime.UtcNow
        };
    }

    public static ApiErrorResponse FromValidationErrors(Dictionary<string, string[]> errors, string correlationId)
    {
        return new ApiErrorResponse
        {
            Message = "Validation failed",
            StatusCode = 400,
            ErrorType = "ValidationError",
            CorrelationId = correlationId,
            ValidationErrors = errors,
            Timestamp = DateTime.UtcNow
        };
    }
}