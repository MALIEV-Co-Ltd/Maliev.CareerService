using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Domain.Entities;

public class TrainingProgram : BaseEntity
{
    public string ProgramCode { get; set; } = string.Empty;

    public string ProgramName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? Category { get; set; }

    public decimal DurationHours { get; set; }

    public string? Provider { get; set; }

    public string? ExternalLmsUrl { get; set; }

    public bool IsMandatory { get; set; }

    public string[] TargetRoles { get; set; } = [];

    public int? MaxParticipants { get; set; }

    public bool IsActive { get; set; } = true;

    public int? ValidityMonths { get; set; }

    public ICollection<EmployeeTrainingEnrollment> Enrollments { get; set; } = [];
}
