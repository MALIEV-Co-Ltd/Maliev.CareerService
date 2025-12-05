using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;
/// <summary>
/// Configuration options for UploadService
/// </summary>

public class UploadServiceOptions
{
    /// <summary>
    /// Gets the configuration section name for the upload service.
    /// </summary>
    public const string SectionName = "ExternalServices:UploadService";

    /// <summary>
    /// Gets or sets the base URL for the upload service.
    /// </summary>
    [Required]
    [Url]
    public required string BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the timeout duration in seconds for upload operations.
    /// </summary>
    [Range(1, 300)]
    public int TimeoutInSeconds { get; set; } = 180;
}
