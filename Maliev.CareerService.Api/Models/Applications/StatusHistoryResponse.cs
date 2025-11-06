namespace Maliev.CareerService.Api.Models.Applications;

/// <summary>
/// Response containing full status change history for an application
/// </summary>
public class StatusHistoryResponse
{
    /// <summary>
    /// Application ID
    /// </summary>
    public Guid ApplicationId { get; set; }

    /// <summary>
    /// List of status changes ordered by ChangedAt DESC (newest first)
    /// </summary>
    public List<StatusChangeRecord> Changes { get; set; } = [];
}
