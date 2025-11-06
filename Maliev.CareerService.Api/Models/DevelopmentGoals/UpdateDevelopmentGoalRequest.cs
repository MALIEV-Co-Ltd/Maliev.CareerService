namespace Maliev.CareerService.Api.Models.DevelopmentGoals;

/// <summary>
/// Request model for updating a Development Goal
/// </summary>
public class UpdateDevelopmentGoalRequest
{
    public string GoalTitle { get; set; } = string.Empty;
    public string GoalDescription { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public string? ActionItems { get; set; }
    public string? ProgressNotes { get; set; }
    public string RowVersion { get; set; } = string.Empty;
}
