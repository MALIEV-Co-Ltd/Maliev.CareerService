namespace Maliev.CareerService.Application.Models;
/// <summary>
/// Configuration options for Cors
/// </summary>

public class CorsOptions
{
    /// <summary>
    /// Gets the configuration section name for CORS settings.
    /// </summary>
    public const string SectionName = "Cors";

    /// <summary>
    /// Gets or sets the array of allowed origins for CORS.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = [];
}
