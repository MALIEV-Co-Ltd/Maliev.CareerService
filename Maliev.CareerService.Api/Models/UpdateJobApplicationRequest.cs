using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;

public class UpdateJobApplicationRequest
{
    [Required]
    [MaxLength(50)]
    public required string Status { get; set; }

    public string? Notes { get; set; }
}