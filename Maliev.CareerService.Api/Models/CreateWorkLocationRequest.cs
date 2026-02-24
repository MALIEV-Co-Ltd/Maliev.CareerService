using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;
/// <summary>
/// Request model for createworklocation
/// </summary>

public class CreateWorkLocationRequest
{
    /// <summary>
    /// Gets or sets the work location name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the physical address of the work location.
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the city where the work location is situated.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public required string City { get; set; }

    /// <summary>
    /// Gets or sets the country identifier for the work location.
    /// </summary>
    public int? CountryId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether remote work is allowed at this location.
    /// </summary>
    public bool IsRemoteAllowed { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether this location supports hybrid work arrangements.
    /// </summary>
    public bool IsHybrid { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the work location is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
