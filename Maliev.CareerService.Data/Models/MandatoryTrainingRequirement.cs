using System.ComponentModel.DataAnnotations;
using Maliev.CareerService.Data.Models.Base;

namespace Maliev.CareerService.Data.Models;

/// <summary>
/// Defines training that must be completed by specific employee groups
/// </summary>
public class MandatoryTrainingRequirement : BaseEntity
{
    /// <summary>
    /// Training program that is mandatory
    /// </summary>
    [Required]
    public Guid TrainingProgramId { get; set; }

    /// <summary>
    /// Target department (null = all departments)
    /// </summary>
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// Target position/role (null = all positions)
    /// </summary>
    public Guid? PositionId { get; set; }

    /// <summary>
    /// Number of days from employee hire date to complete training
    /// </summary>
    [Required]
    [Range(1, 365)]
    public int CompletionDeadlineDays { get; set; } = 30;

    /// <summary>
    /// Number of months until recertification is required (if applicable)
    /// </summary>
    [Range(1, 120)]
    public int? RecertificationMonths { get; set; }

    /// <summary>
    /// Whether this requirement is currently active
    /// Inactive requirements stop new assignments but preserve historical data
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    /// <summary>
    /// Associated training program
    /// </summary>
    public TrainingProgram TrainingProgram { get; set; } = null!;
}
