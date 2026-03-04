namespace Maliev.CareerService.Api.Models.Applications;

/// <summary>
/// Represents a single status change record in the audit trail
/// </summary>
public class StatusChangeRecord
{
    /// <summary>
    /// Status change record ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Previous status (null for initial submission)
    /// </summary>
    public string? FromStatus { get; set; }

    /// <summary>
    /// New status after the change
    /// </summary>
    public string ToStatus { get; set; } = string.Empty;

    /// <summary>
    /// User ID who made the change
    /// </summary>
    public Guid ChangedBy { get; set; }

    /// <summary>
    /// Name of the user who made the change
    /// </summary>
    public string ChangedByName { get; set; } = string.Empty;

    /// <summary>
    /// When the change occurred
    /// </summary>
    public DateTime ChangedAt { get; set; }

    /// <summary>
    /// Reason for the status change
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Whether this is a reversal of a previous change
    /// </summary>
    public bool IsReversal { get; set; }
}
