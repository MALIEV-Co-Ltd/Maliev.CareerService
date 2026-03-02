namespace Maliev.CareerService.Data.Models;

/// <summary>
/// Tracks status changes for job applications
/// </summary>
public class ApplicationStatusChange
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to JobApplication
    /// </summary>
    public Guid ApplicationId { get; set; }

    /// <summary>
    /// Navigation property to JobApplication
    /// </summary>
    public JobApplication Application { get; set; } = null!;

    /// <summary>
    /// Previous status
    /// </summary>
    public string? FromStatus { get; set; }

    /// <summary>
    /// New status
    /// </summary>
    public string ToStatus { get; set; } = string.Empty;

    /// <summary>
    /// User ID who made the change
    /// </summary>
    public Guid ChangedBy { get; set; }

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

    /// <summary>
    /// If this is a reversal, the ID of the change being reversed
    /// </summary>
    public Guid? ReversedChangeId { get; set; }
}
