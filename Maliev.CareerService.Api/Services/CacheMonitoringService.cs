using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace Maliev.CareerService.Api.Services;

public interface ICacheMonitoringService
{
    void RecordCacheHit(string cacheKey);
    void RecordCacheMiss(string cacheKey);
    void RecordCacheSet(string cacheKey);
    void RecordCacheRemove(string cacheKey);
    Task<CacheMetrics> GetCacheMetricsAsync();
    Task<Dictionary<string, CacheItemInfo>> GetCacheItemsInfoAsync();
}

public class CacheMonitoringService : ICacheMonitoringService, IHealthCheck
{
    private readonly ILogger<CacheMonitoringService> _logger;
    private readonly Meter _meter;
    private readonly Counter<long> _cacheHitsCounter;
    private readonly Counter<long> _cacheMissesCounter;
    private readonly Counter<long> _cacheSetsCounter;
    private readonly Counter<long> _cacheRemovesCounter;
    private readonly Histogram<double> _cacheOperationDuration;

    // Cache metrics tracking
    private readonly ConcurrentDictionary<string, CacheItemTracking> _cacheItems = new();
    private long _totalHits = 0;
    private long _totalMisses = 0;
    private long _totalSets = 0;
    private long _totalRemoves = 0;

    public CacheMonitoringService(ILogger<CacheMonitoringService> logger)
    {
        _logger = logger;
        
        // Initialize metrics
        _meter = new Meter("Maliev.CareerService.Cache", "1.0.0");
        _cacheHitsCounter = _meter.CreateCounter<long>("cache.hits", "hits", "Number of cache hits");
        _cacheMissesCounter = _meter.CreateCounter<long>("cache.misses", "misses", "Number of cache misses");
        _cacheSetsCounter = _meter.CreateCounter<long>("cache.sets", "sets", "Number of cache sets");
        _cacheRemovesCounter = _meter.CreateCounter<long>("cache.removes", "removes", "Number of cache removes");
        _cacheOperationDuration = _meter.CreateHistogram<double>("cache.operation.duration", "milliseconds", "Cache operation duration");
    }

    public void RecordCacheHit(string cacheKey)
    {
        Interlocked.Increment(ref _totalHits);
        _cacheHitsCounter.Add(1, new KeyValuePair<string, object?>("key", cacheKey));

        // Track individual cache item
        _cacheItems.AddOrUpdate(
            cacheKey,
            _ => new CacheItemTracking { Hits = 1, LastAccess = DateTimeOffset.UtcNow },
            (_, existing) => { existing.Hits++; existing.LastAccess = DateTimeOffset.UtcNow; return existing; });

        _logger.LogDebug("Cache hit recorded for key: {CacheKey}", cacheKey);
    }

    public void RecordCacheMiss(string cacheKey)
    {
        Interlocked.Increment(ref _totalMisses);
        _cacheMissesCounter.Add(1, new KeyValuePair<string, object?>("key", cacheKey));

        _logger.LogDebug("Cache miss recorded for key: {CacheKey}", cacheKey);
    }

    public void RecordCacheSet(string cacheKey)
    {
        Interlocked.Increment(ref _totalSets);
        _cacheSetsCounter.Add(1, new KeyValuePair<string, object?>("key", cacheKey));

        // Track individual cache item
        _cacheItems.AddOrUpdate(
            cacheKey,
            _ => new CacheItemTracking { Sets = 1, Created = DateTimeOffset.UtcNow, LastAccess = DateTimeOffset.UtcNow },
            (_, existing) => { existing.Sets++; existing.LastAccess = DateTimeOffset.UtcNow; return existing; });

        _logger.LogDebug("Cache set recorded for key: {CacheKey}", cacheKey);
    }

    public void RecordCacheRemove(string cacheKey)
    {
        Interlocked.Increment(ref _totalRemoves);
        _cacheRemovesCounter.Add(1, new KeyValuePair<string, object?>("key", cacheKey));

        // Remove tracking for this item
        _cacheItems.TryRemove(cacheKey, out _);

        _logger.LogDebug("Cache remove recorded for key: {CacheKey}", cacheKey);
    }

    public Task<CacheMetrics> GetCacheMetricsAsync()
    {
        var totalRequests = _totalHits + _totalMisses;
        var hitRate = totalRequests > 0 ? (double)_totalHits / totalRequests : 0;

        var metrics = new CacheMetrics
        {
            TotalHits = _totalHits,
            TotalMisses = _totalMisses,
            TotalSets = _totalSets,
            TotalRemoves = _totalRemoves,
            HitRate = Math.Round(hitRate * 100, 2),
            TotalItems = _cacheItems.Count,
            CacheItems = _cacheItems.Select(kvp => new CacheItemMetrics
            {
                Key = kvp.Key,
                Hits = kvp.Value.Hits,
                Sets = kvp.Value.Sets,
                LastAccess = kvp.Value.LastAccess,
                Created = kvp.Value.Created
            }).ToList()
        };

        return Task.FromResult(metrics);
    }

    public Task<Dictionary<string, CacheItemInfo>> GetCacheItemsInfoAsync()
    {
        var result = new Dictionary<string, CacheItemInfo>();

        foreach (var item in _cacheItems)
        {
            result[item.Key] = new CacheItemInfo
            {
                Hits = item.Value.Hits,
                Sets = item.Value.Sets,
                LastAccess = item.Value.LastAccess,
                Created = item.Value.Created
            };
        }

        return Task.FromResult(result);
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var totalRequests = _totalHits + _totalMisses;
            var hitRate = totalRequests > 0 ? (double)_totalHits / totalRequests : 0;

            var data = new Dictionary<string, object>
            {
                ["totalHits"] = _totalHits,
                ["totalMisses"] = _totalMisses,
                ["totalSets"] = _totalSets,
                ["totalRemoves"] = _totalRemoves,
                ["hitRatePercentage"] = Math.Round(hitRate * 100, 2),
                ["totalItems"] = _cacheItems.Count
            };

            // Determine health status based on hit rate
            if (hitRate >= 0.8) // 80% hit rate is considered healthy
            {
                return Task.FromResult(HealthCheckResult.Healthy(
                    $"Cache is performing well with {Math.Round(hitRate * 100, 2)}% hit rate", data));
            }
            else if (hitRate >= 0.5) // 50% hit rate is considered degraded
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Cache performance is degraded with {Math.Round(hitRate * 100, 2)}% hit rate"));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Cache performance is poor with {Math.Round(hitRate * 100, 2)}% hit rate"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy($"Cache health check failed: {ex.Message}", ex));
        }
    }

    public void Dispose()
    {
        _meter?.Dispose();
    }
}

public class CacheItemTracking
{
    public long Hits { get; set; } = 0;
    public long Sets { get; set; } = 0;
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastAccess { get; set; } = DateTimeOffset.UtcNow;
}

public class CacheMetrics
{
    public long TotalHits { get; set; }
    public long TotalMisses { get; set; }
    public long TotalSets { get; set; }
    public long TotalRemoves { get; set; }
    public double HitRate { get; set; }
    public int TotalItems { get; set; }
    public List<CacheItemMetrics> CacheItems { get; set; } = new();
}

public class CacheItemMetrics
{
    public string Key { get; set; } = string.Empty;
    public long Hits { get; set; }
    public long Sets { get; set; }
    public DateTimeOffset LastAccess { get; set; }
    public DateTimeOffset Created { get; set; }
}

public class CacheItemInfo
{
    public long Hits { get; set; }
    public long Sets { get; set; }
    public DateTimeOffset LastAccess { get; set; }
    public DateTimeOffset Created { get; set; }
}