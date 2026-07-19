using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Application.Models;
/// <summary>
/// Request model for updatejobapplication
/// </summary>

public class UpdateJobApplicationRequest
{
    /// <summary>
    /// Gets or sets the new application status.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public required string Status { get; set; }

    /// <summary>
    /// Gets or sets optional notes about the status change.
    /// </summary>
    public string? Notes { get; set; }
}
