namespace Maliev.CareerService.Data.Models;

/// <summary>
/// Individual Development Plan (IDP) status constants
/// </summary>
public static class IDPStatus
{
    public const string Draft = "draft";
    public const string Submitted = "submitted";
    public const string Approved = "approved";
    public const string InProgress = "in_progress";
    public const string Completed = "completed";

    /// <summary>
    /// All valid IDP status values
    /// </summary>
    public static readonly string[] ValidStatuses =
    [
        Draft,
        Submitted,
        Approved,
        InProgress,
        Completed
    ];

    /// <summary>
    /// Statuses that allow editing
    /// </summary>
    public static readonly string[] EditableStatuses =
    [
        Draft
    ];

    /// <summary>
    /// Terminal status
    /// </summary>
    public static readonly string[] TerminalStatuses =
    [
        Completed
    ];

    /// <summary>
    /// Validates if a status is valid
    /// </summary>
    public static bool IsValid(string status) => ValidStatuses.Contains(status);

    /// <summary>
    /// Checks if editing is allowed for this status
    /// </summary>
    public static bool IsEditable(string status) => EditableStatuses.Contains(status);

    /// <summary>
    /// Checks if a status is terminal
    /// </summary>
    public static bool IsTerminal(string status) => TerminalStatuses.Contains(status);
}
