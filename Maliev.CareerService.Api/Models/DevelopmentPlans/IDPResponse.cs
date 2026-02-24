using Maliev.CareerService.Api.Models.DevelopmentGoals;

namespace Maliev.CareerService.Api.Models.DevelopmentPlans;

/// <summary>
/// Response model for Individual Development Plan
/// </summary>
public class IDPResponse
{
    /// <summary>
    /// Gets or sets the unique identifier for the IDP.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Gets or sets the employee identifier associated with this IDP.
    /// </summary>
    public Guid EmployeeId { get; set; }
    /// <summary>
    /// Gets or sets the year for which the IDP is created.
    /// </summary>
    public int PlanYear { get; set; }
    /// <summary>
    /// Gets or sets the current status of the IDP (e.g., Draft, Submitted, Approved).
    /// </summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the timestamp when the IDP was submitted.
    /// </summary>
    public DateTime? SubmittedAt { get; set; }
    /// <summary>
    /// Gets or sets the timestamp when the IDP was approved.
    /// </summary>
    public DateTime? ApprovedAt { get; set; }
    /// <summary>
    /// Gets or sets the identifier of the user who approved the IDP.
    /// </summary>
    public Guid? ApprovedBy { get; set; }
    /// <summary>
    /// Gets or sets the list of development goals associated with this IDP.
    /// </summary>
    public List<DevelopmentGoalResponse> Goals { get; set; } = [];
    /// <summary>
    /// Gets or sets the timestamp when the IDP was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// Gets or sets the timestamp when the IDP was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    /// <summary>
    /// Gets or sets the identifier of the user who created the IDP.
    /// </summary>
    public Guid? CreatedBy { get; set; }
    /// <summary>
    /// Gets or sets the identifier of the user who last updated the IDP.
    /// </summary>
    public Guid? UpdatedBy { get; set; }
    /// <summary>
    /// Gets or sets the row version for optimistic concurrency control.
    /// </summary>
    public string RowVersion { get; set; } = string.Empty;
}
