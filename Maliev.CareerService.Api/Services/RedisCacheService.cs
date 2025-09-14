using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace Maliev.CareerService.Api.Services;

public interface IRedisCacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions? options = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}

public class RedisCacheService : IRedisCacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly CacheOptions _cacheOptions;

    public RedisCacheService(
        IDistributedCache distributedCache,
        ILogger<RedisCacheService> logger,
        IOptions<CacheOptions> cacheOptions)
    {
        _distributedCache = distributedCache;
        _logger = logger;
        _cacheOptions = cacheOptions.Value;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrEmpty(cachedValue))
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default(T);
            }

            var value = JsonSerializer.Deserialize<T>(cachedValue);
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving value from Redis cache for key: {Key}", key);
            return default(T);
        }
    }

    public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions? options = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value);
            
            // Use provided options or create default options
            var cacheOptions = options ?? new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheOptions.DefaultExpiration
            };

            await _distributedCache.SetStringAsync(key, serializedValue, cacheOptions, cancellationToken);
            _logger.LogDebug("Value set in Redis cache for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in Redis cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Value removed from Redis cache for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from Redis cache for key: {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedValue = await _distributedCache.GetAsync(key, cancellationToken);
            return cachedValue != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence in Redis cache for key: {Key}", key);
            return false;
        }
    }
}

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<RedisHealthCheck> _logger;

    public RedisHealthCheck(
        IConnectionMultiplexer connectionMultiplexer,
        ILogger<RedisHealthCheck> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _connectionMultiplexer.GetDatabase();
            
            // Perform a simple ping to check connectivity
            var pong = await database.PingAsync();
            
            var data = new Dictionary<string, object>
            {
                ["endpoint"] = _connectionMultiplexer.Configuration,
                ["pingResponseTime"] = pong.TotalMilliseconds
            };

            return HealthCheckResult.Healthy($"Redis is reachable. Ping response time: {pong.TotalMilliseconds}ms", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            return HealthCheckResult.Unhealthy($"Redis health check failed: {ex.Message}", ex);
        }
    }
}