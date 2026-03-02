using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Domain.Entities;

public class MandatoryTrainingRequirement : BaseEntity
{
    public Guid TrainingProgramId { get; set; }

    public Guid? DepartmentId { get; set; }

    public Guid? PositionId { get; set; }

    public int CompletionDeadlineDays { get; set; } = 30;

    public int? RecertificationMonths { get; set; }

    public bool IsActive { get; set; } = true;

    public TrainingProgram TrainingProgram { get; set; } = null!;
}
