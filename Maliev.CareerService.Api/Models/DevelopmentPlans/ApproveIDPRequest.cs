namespace Maliev.CareerService.Api.Models.DevelopmentPlans;

/// <summary>
/// Request model for approving an Individual Development Plan
/// </summary>
public class ApproveIDPRequest
{
    /// <summary>
    /// Gets or sets optional notes from the approver.
    /// </summary>
    public string? ApprovalNotes { get; set; }
    /// <summary>
    /// Gets or sets the row version for optimistic concurrency control.
    /// </summary>
    public string RowVersion { get; set; } = string.Empty;
}
