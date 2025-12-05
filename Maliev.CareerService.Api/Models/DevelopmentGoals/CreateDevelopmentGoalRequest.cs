namespace Maliev.CareerService.Api.Models.DevelopmentGoals;

/// <summary>
/// Request model for creating a new Development Goal
/// </summary>
public class CreateDevelopmentGoalRequest
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
    /// Gets or sets the category of the development goal (e.g., Technical, Leadership).
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
}
