using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;

public class CacheOptions
{
    public const string SectionName = "Cache";

    [Range(1, int.MaxValue)]
    public int MaxCacheSize { get; set; } = 1000;

    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(30);

    public TimeSpan LongExpiration { get; set; } = TimeSpan.FromHours(2);
}