using Maliev.CareerService.Api.Models.DevelopmentGoals;

namespace Maliev.CareerService.Api.Models.DevelopmentPlans;

/// <summary>
/// Response model for Individual Development Plan
/// </summary>
public class IDPResponse
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public int PlanYear { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public List<DevelopmentGoalResponse> Goals { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public string RowVersion { get; set; } = string.Empty;
}
