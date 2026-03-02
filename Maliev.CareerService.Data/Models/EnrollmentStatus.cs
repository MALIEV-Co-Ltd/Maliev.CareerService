namespace Maliev.CareerService.Data.Models;

/// <summary>
/// Training enrollment status constants
/// </summary>
public static class EnrollmentStatus
{
    public const string Enrolled = "enrolled";
    public const string InProgress = "in_progress";
    public const string Completed = "completed";
    public const string Cancelled = "cancelled";

    /// <summary>
    /// All valid enrollment status values
    /// </summary>
    public static readonly string[] ValidStatuses =
    [
        Enrolled,
        InProgress,
        Completed,
        Cancelled
    ];

    /// <summary>
    /// Terminal statuses that cannot be changed
    /// </summary>
    public static readonly string[] TerminalStatuses =
    [
        Completed,
        Cancelled
    ];

    /// <summary>
    /// Validates if a status is valid
    /// </summary>
    public static bool IsValid(string status) => ValidStatuses.Contains(status);

    /// <summary>
    /// Checks if a status is terminal
    /// </summary>
    public static bool IsTerminal(string status) => TerminalStatuses.Contains(status);
}
