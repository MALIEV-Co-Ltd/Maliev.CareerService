using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Application.Models;
/// <summary>
/// Configuration options for Jwt
/// </summary>

public class JwtOptions
{
    /// <summary>
    /// Gets the configuration section name for JWT settings.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Gets or sets the JWT token issuer.
    /// </summary>
    [Required]
    public required string Issuer { get; set; }

    /// <summary>
    /// Gets or sets the JWT token audience.
    /// </summary>
    [Required]
    public required string Audience { get; set; }

    /// <summary>
    /// Gets or sets the security key used for signing JWT tokens.
    /// </summary>
    [Required]
    public required string SecurityKey { get; set; }
}
