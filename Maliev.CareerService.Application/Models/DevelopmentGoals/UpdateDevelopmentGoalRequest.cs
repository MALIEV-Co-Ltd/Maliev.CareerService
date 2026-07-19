namespace Maliev.CareerService.Application.Models.DevelopmentGoals;

/// <summary>
/// Request model for updating a Development Goal
/// </summary>
public class UpdateDevelopmentGoalRequest
{
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
    /// Gets or sets the action items required to achieve the goal.
    /// </summary>
    public string? ActionItems { get; set; }
    /// <summary>
    /// Gets or sets the progress notes for the goal.
    /// </summary>
    public string? ProgressNotes { get; set; }
    /// <summary>
    /// Gets or sets the row version for optimistic concurrency control.
    /// </summary>
    public string RowVersion { get; set; } = string.Empty;
}
