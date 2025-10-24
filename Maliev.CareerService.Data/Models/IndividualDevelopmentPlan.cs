using Maliev.CareerService.Data.Models.Base;

namespace Maliev.CareerService.Data.Models;

/// <summary>
/// Individual Development Plan (IDP) entity for employee career planning
/// </summary>
public class IndividualDevelopmentPlan : BaseEntity
{
    /// <summary>
    /// Employee ID from Employee Service
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Plan year (e.g., 2025)
    /// </summary>
    public int PlanYear { get; set; }

    /// <summary>
    /// Plan status (Draft, Submitted, Approved, InProgress, Completed)
    /// </summary>
    public string Status { get; set; } = IDPStatus.Draft;

    /// <summary>
    /// When the plan was submitted for approval
    /// </summary>
    public DateTime? SubmittedAt { get; set; }

    /// <summary>
    /// When the plan was approved
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// HR staff member who approved the plan
    /// </summary>
    public Guid? ApprovedBy { get; set; }

    /// <summary>
    /// Navigation property: Development goals within this plan
    /// </summary>
    public ICollection<EmployeeDevelopmentGoal> Goals { get; set; } = [];
}
