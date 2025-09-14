using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;

public class UpdateSkillRequest
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    public bool IsActive { get; set; } = true;
}