using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models.TrainingRecords;

/// <summary>
/// Request to create a new mandatory training requirement (Feature 003)
/// </summary>
public class CreateMandatoryRequirementRequest
{
    /// <summary>
    /// Gets or sets the training program identifier
    /// </summary>
    [Required(ErrorMessage = "Training program ID is required")]
    public Guid TrainingProgramId { get; set; }

    /// <summary>
    /// Gets or sets the target department identifier (null = all)
    /// </summary>
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// Gets or sets the target position identifier (null = all)
    /// </summary>
    public Guid? PositionId { get; set; }

    /// <summary>
    /// Gets or sets the number of days to complete the training from hire date
    /// </summary>
    [Required(ErrorMessage = "Completion deadline days is required")]
    [Range(1, 365, ErrorMessage = "Deadline must be between 1 and 365 days")]
    public int CompletionDeadlineDays { get; set; } = 30;

    /// <summary>
    /// Gets or sets the number of months until recertification is required
    /// </summary>
    [Range(1, 120, ErrorMessage = "Recertification months must be between 1 and 120")]
    public int? RecertificationMonths { get; set; }
}
