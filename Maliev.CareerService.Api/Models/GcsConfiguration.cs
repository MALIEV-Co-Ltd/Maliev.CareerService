using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;
/// <summary>
/// Represents a GcsConfiguration
/// </summary>

public class GcsConfiguration
{
    /// <summary>
    /// Gets the configuration section name for GCS settings.
    /// </summary>
    public const string SectionName = "Gcs";

    /// <summary>
    /// Gets or sets the base path for GCS storage.
    /// </summary>
    [Required]
    public required string BasePath { get; set; }
}
