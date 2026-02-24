namespace Maliev.CareerService.Api.Models.Common;

/// <summary>
/// Standard error response model
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Error type/title
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Detailed error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Trace identifier for correlating logs
    /// </summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Additional error details (only in development)
    /// </summary>
    public string? Details { get; set; }
}
