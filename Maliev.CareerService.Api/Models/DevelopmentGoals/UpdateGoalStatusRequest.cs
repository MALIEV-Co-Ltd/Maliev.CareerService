namespace Maliev.CareerService.Api.Models.DevelopmentGoals;

/// <summary>
/// Request model for updating Development Goal status
/// </summary>
public class UpdateGoalStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public DateTime? CompletionDate { get; set; }
    public string? ProgressNotes { get; set; }
    public string RowVersion { get; set; } = string.Empty;
}
