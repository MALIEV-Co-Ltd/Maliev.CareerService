namespace Maliev.CareerService.Api.Models.DevelopmentGoals;

/// <summary>
/// Request model for creating a new Development Goal
/// </summary>
public class CreateDevelopmentGoalRequest
{
    public string GoalTitle { get; set; } = string.Empty;
    public string GoalDescription { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public string? ActionItems { get; set; }
}
