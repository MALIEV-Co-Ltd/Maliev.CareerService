namespace Maliev.CareerService.Application.Models.DevelopmentGoals;

/// <summary>
/// Request model for updating Development Goal status
/// </summary>
public class UpdateGoalStatusRequest
{
    /// <summary>
    /// Gets or sets the new status for the goal.
    /// </summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the date when the goal was completed.
    /// </summary>
    public DateTime? CompletionDate { get; set; }
    /// <summary>
    /// Gets or sets the progress notes for the goal.
    /// </summary>
    public string? ProgressNotes { get; set; }
    /// <summary>
    /// Gets or sets the row version for optimistic concurrency control.
    /// </summary>
    public string RowVersion { get; set; } = string.Empty;
}
