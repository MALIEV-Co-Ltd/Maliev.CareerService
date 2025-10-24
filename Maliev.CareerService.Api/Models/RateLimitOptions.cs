using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;

public class RateLimitOptions
{
    public const string SectionName = "RateLimit";

    [Required]
    public required CareerEndpointOptions CareerEndpoint { get; set; }

    [Required]
    public required GlobalOptions Global { get; set; }

    [Required]
    public required UserOptions User { get; set; }

    public class CareerEndpointOptions
    {
        [Range(1, int.MaxValue)]
        public int PermitLimit { get; set; } = 100;

        public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);

        [Range(1, int.MaxValue)]
        public int QueueLimit { get; set; } = 10;
    }

    public class GlobalOptions
    {
        [Range(1, int.MaxValue)]
        public int PermitLimit { get; set; } = 200;

        public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);

        [Range(1, int.MaxValue)]
        public int QueueLimit { get; set; } = 20;
    }

    public class UserOptions
    {
        [Range(1, int.MaxValue)]
        public int PermitLimit { get; set; } = 50;

        public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);

        [Range(1, int.MaxValue)]
        public int QueueLimit { get; set; } = 5;
    }
}