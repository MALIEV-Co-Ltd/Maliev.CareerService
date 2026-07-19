using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Application.Models.TrainingRecords;

/// <summary>
/// Request to update an existing mandatory training requirement (Feature 003)
/// </summary>
public class UpdateMandatoryRequirementRequest
{
    /// <summary>
    /// Gets or sets the number of days to complete the training from hire date
    /// </summary>
    [Required(ErrorMessage = "Completion deadline days is required")]
    [Range(1, 365, ErrorMessage = "Deadline must be between 1 and 365 days")]
    public int CompletionDeadlineDays { get; set; }

    /// <summary>
    /// Gets or sets the number of months until recertification is required
    /// </summary>
    [Range(1, 120, ErrorMessage = "Recertification months must be between 1 and 120")]
    public int? RecertificationMonths { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the requirement is active
    /// </summary>
    [Required(ErrorMessage = "IsActive status is required")]
    public bool IsActive { get; set; }
}
