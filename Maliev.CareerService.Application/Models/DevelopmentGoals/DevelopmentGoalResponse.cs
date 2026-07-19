namespace Maliev.CareerService.Application.Models.DevelopmentGoals;

/// <summary>
/// Response model for Development Goal
/// </summary>
public class DevelopmentGoalResponse
{
    /// <summary>
    /// Gets or sets the unique identifier for the development goal.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Gets or sets the identifier of the associated Individual Development Plan (IDP).
    /// </summary>
    public Guid IdpId { get; set; }
    /// <summary>
    /// Gets or sets the title of the development goal.
    /// </summary>
    public string GoalTitle { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the detailed description of the development goal.
    /// </summary>
    public string GoalDescription { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the category of the development goal.
    /// </summary>
    public string Category { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the target completion date for the goal.
    /// </summary>
    public DateTime TargetDate { get; set; }
    /// <summary>
    /// Gets or sets the current status of the goal (e.g., InProgress, Completed).
    /// </summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the date when the goal was completed.
    /// </summary>
    public DateTime? CompletionDate { get; set; }
    /// <summary>
    /// Gets or sets the action items required to achieve the goal.
    /// </summary>
    public string? ActionItems { get; set; }
    /// <summary>
    /// Gets or sets the progress notes for tracking goal advancement.
    /// </summary>
    public string? ProgressNotes { get; set; }
    /// <summary>
    /// Gets or sets the timestamp when the goal was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// Gets or sets the timestamp when the goal was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    /// <summary>
    /// Gets or sets the identifier of the user who created the goal.
    /// </summary>
    public Guid? CreatedBy { get; set; }
    /// <summary>
    /// Gets or sets the identifier of the user who last updated the goal.
    /// </summary>
    public Guid? UpdatedBy { get; set; }
    /// <summary>
    /// Gets or sets the row version for optimistic concurrency control.
    /// </summary>
    public string RowVersion { get; set; } = string.Empty;
}
