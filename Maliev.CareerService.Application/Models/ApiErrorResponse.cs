namespace Maliev.CareerService.Application.Models;
/// <summary>
/// Response model for apierror
/// </summary>

public class ApiErrorResponse
{
    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    public int StatusCode { get; set; }
    /// <summary>
    /// Gets or sets the type of error.
    /// </summary>
    public string ErrorType { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the correlation identifier for request tracking.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the validation errors dictionary.
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
    /// <summary>
    /// Gets or sets the stack trace (if included).
    /// </summary>
    public string? StackTrace { get; set; }
    /// <summary>
    /// Gets or sets the error timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates an ApiErrorResponse from an exception.
    /// </summary>
    /// <param name="ex">The exception.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    /// <param name="includeStackTrace">If true, includes stack trace in response.</param>
    /// <returns>An ApiErrorResponse instance.</returns>
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

    /// <summary>
    /// Creates an ApiErrorResponse from validation errors.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    /// <returns>An ApiErrorResponse instance.</returns>
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
