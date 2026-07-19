using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Domain.Entities;

public class Skill : BaseEntity
{
    [Required]
    public Guid EmployeeId { get; set; }

    [Required]
    [MaxLength(100)]
    public string SkillName { get; set; } = string.Empty;

    [Required]
    [Range(1, 5)]
    public ProficiencyLevel ProficiencyLevel { get; set; }

    [Required]
    public DateTime LastAssessedDate { get; set; }

    public bool IsDevelopmentArea { get; set; } = false;

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

public enum ProficiencyLevel
{
    Beginner = 1,
    Elementary = 2,
    Intermediate = 3,
    Advanced = 4,
    Expert = 5
}
