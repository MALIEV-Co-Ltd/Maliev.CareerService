using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;
/// <summary>
/// Request model for updateworklocation
/// </summary>

public class UpdateWorkLocationRequest
{
    /// <summary>
    /// Gets or sets the work location name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the physical address.
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public required string City { get; set; }

    /// <summary>
    /// Gets or sets the country identifier.
    /// </summary>
    public int? CountryId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether remote work is allowed.
    /// </summary>
    public bool IsRemoteAllowed { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether hybrid work is supported.
    /// </summary>
    public bool IsHybrid { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the location is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
