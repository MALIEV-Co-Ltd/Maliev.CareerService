using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;
/// <summary>
/// Configuration options for Cache
/// </summary>

public class CacheOptions
{
    /// <summary>
    /// Gets the configuration section name for cache settings.
    /// </summary>
    public const string SectionName = "Cache";

    /// <summary>
    /// Gets or sets the maximum cache size.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int MaxCacheSize { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the default cache expiration time.
    /// </summary>
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets or sets the long cache expiration time.
    /// </summary>
    public TimeSpan LongExpiration { get; set; } = TimeSpan.FromHours(2);

    /// <summary>
    /// Gets or sets a value indicating whether Redis caching is enabled.
    /// </summary>
    public bool RedisEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether fallback caching is enabled.
    /// </summary>
    public bool FallbackEnabled { get; set; } = true;
}
