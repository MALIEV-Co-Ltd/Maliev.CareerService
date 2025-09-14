using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Data.Entities;

public class JobPositionSkill
{
    public int JobPositionId { get; set; }
    public int SkillId { get; set; }

    [Required]
    [MaxLength(50)]
    public required string RequiredLevel { get; set; }

    public bool IsRequired { get; set; } = true;

    // Navigation properties
    public JobPosition JobPosition { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}