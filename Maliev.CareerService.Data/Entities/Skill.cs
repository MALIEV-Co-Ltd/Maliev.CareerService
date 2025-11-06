using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Data.Entities;

public class Skill
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<JobPositionSkill> JobPositionSkills { get; set; } = [];
}