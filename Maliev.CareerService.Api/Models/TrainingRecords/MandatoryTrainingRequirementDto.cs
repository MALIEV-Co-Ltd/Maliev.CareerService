namespace Maliev.CareerService.Api.Models.TrainingRecords;

/// <summary>
/// Data transfer object for mandatory training requirement (Feature 003)
/// </summary>
public class MandatoryTrainingRequirementDto
{
    /// <summary>
    /// Gets or sets the requirement identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the training program identifier
    /// </summary>
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
    public int CompletionDeadlineDays { get; set; }

    /// <summary>
    /// Gets or sets the number of months until recertification is required
    /// </summary>
    public int? RecertificationMonths { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the requirement is active
    /// </summary>
    public bool IsActive { get; set; }
}
