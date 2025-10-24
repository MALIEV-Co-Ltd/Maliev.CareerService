namespace Maliev.CareerService.Api.Models.DevelopmentGoals;

/// <summary>
/// Response model for Development Goal
/// </summary>
public class DevelopmentGoalResponse
{
    public Guid Id { get; set; }
    public Guid IdpId { get; set; }
    public string GoalTitle { get; set; } = string.Empty;
    public string GoalDescription { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CompletionDate { get; set; }
    public string? ActionItems { get; set; }
    public string? ProgressNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public string RowVersion { get; set; } = string.Empty;
}
