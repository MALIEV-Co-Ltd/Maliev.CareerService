using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;
/// <summary>
/// Configuration options for RateLimit
/// </summary>

public class RateLimitOptions
{
    /// <summary>
    /// Gets the configuration section name for rate limit settings.
    /// </summary>
    public const string SectionName = "RateLimit";

    /// <summary>
    /// Gets or sets the rate limit options for career endpoints.
    /// </summary>
    [Required]
    public required CareerEndpointOptions CareerEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the global rate limit options.
    /// </summary>
    [Required]
    public required GlobalOptions Global { get; set; }

    /// <summary>
    /// Gets or sets the per-user rate limit options.
    /// </summary>
    [Required]
    public required UserOptions User { get; set; }
    /// <summary>
    /// Configuration options for CareerEndpoint
    /// </summary>

    public class CareerEndpointOptions
    {
        /// <summary>
        /// Gets or sets the number of permits allowed within the time window.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int PermitLimit { get; set; } = 100;

        /// <summary>
        /// Gets or sets the time window for rate limiting.
        /// </summary>
        public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets or sets the maximum number of requests that can be queued.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int QueueLimit { get; set; } = 10;
    }
    /// <summary>
    /// Configuration options for Global
    /// </summary>

    public class GlobalOptions
    {
        /// <summary>
        /// Gets or sets the number of permits allowed within the time window.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int PermitLimit { get; set; } = 200;

        /// <summary>
        /// Gets or sets the time window for rate limiting.
        /// </summary>
        public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets or sets the maximum number of requests that can be queued.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int QueueLimit { get; set; } = 20;
    }
    /// <summary>
    /// Configuration options for User
    /// </summary>

    public class UserOptions
    {
        /// <summary>
        /// Gets or sets the number of permits allowed within the time window.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int PermitLimit { get; set; } = 50;

        /// <summary>
        /// Gets or sets the time window for rate limiting.
        /// </summary>
        public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets or sets the maximum number of requests that can be queued.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int QueueLimit { get; set; } = 5;
    }
}
