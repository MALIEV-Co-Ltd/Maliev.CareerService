namespace Maliev.CareerService.Data.Models;

/// <summary>
/// Application status state machine constants
/// </summary>
public static class ApplicationStatus
{
    public const string Submitted = "submitted";
    public const string UnderReview = "under_review";
    public const string Interviewing = "interviewing";
    public const string Offered = "offered";
    public const string Accepted = "accepted";
    public const string Rejected = "rejected";
    public const string Withdrawn = "withdrawn";

    /// <summary>
    /// All valid status values
    /// </summary>
    public static readonly string[] ValidStatuses =
    [
        Submitted,
        UnderReview,
        Interviewing,
        Offered,
        Accepted,
        Rejected,
        Withdrawn
    ];

    /// <summary>
    /// Terminal statuses that cannot be changed
    /// </summary>
    public static readonly string[] TerminalStatuses =
    [
        Accepted,
        Rejected,
        Withdrawn
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
