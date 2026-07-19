namespace Maliev.CareerService.Application.Models;
/// <summary>
/// Data transfer object for WorkLocation
/// </summary>

public class WorkLocationDto
{
    /// <summary>
    /// Gets or sets the work location identifier.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Gets or sets the work location name.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Gets or sets the physical address.
    /// </summary>
    public string? Address { get; set; }
    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    public required string City { get; set; }
    /// <summary>
    /// Gets or sets the country identifier.
    /// </summary>
    public int? CountryId { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether remote work is allowed.
    /// </summary>
    public bool IsRemoteAllowed { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether hybrid work is supported.
    /// </summary>
    public bool IsHybrid { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether this location is active.
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// Gets or sets the record creation date.
    /// </summary>
    public DateTime CreatedDate { get; set; }
    /// <summary>
    /// Gets or sets the last modification date.
    /// </summary>
    public DateTime ModifiedDate { get; set; }
}
