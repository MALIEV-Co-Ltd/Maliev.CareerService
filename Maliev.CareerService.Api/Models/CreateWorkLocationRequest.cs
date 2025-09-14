using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;

public class CreateWorkLocationRequest
{
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [Required]
    [MaxLength(100)]
    public required string City { get; set; }

    public int? CountryId { get; set; }

    public bool IsRemoteAllowed { get; set; } = false;

    public bool IsHybrid { get; set; } = false;

    public bool IsActive { get; set; } = true;
}