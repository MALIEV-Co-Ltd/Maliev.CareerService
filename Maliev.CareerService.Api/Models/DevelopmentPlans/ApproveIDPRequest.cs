namespace Maliev.CareerService.Api.Models.DevelopmentPlans;

/// <summary>
/// Request model for approving an Individual Development Plan
/// </summary>
public class ApproveIDPRequest
{
    public string? ApprovalNotes { get; set; }
    public string RowVersion { get; set; } = string.Empty;
}
