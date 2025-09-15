using Maliev.CareerService.Api.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Maliev.CareerService.Api.Services;

public interface IFallbackCacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions? options = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}

public class FallbackCacheService : IFallbackCacheService
{
    private readonly IRedisCacheService _redisCacheService;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<FallbackCacheService> _logger;
    private readonly CacheOptions _cacheOptions;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public FallbackCacheService(
        IRedisCacheService redisCacheService,
        IMemoryCache memoryCache,
        ILogger<FallbackCacheService> logger,
        IOptions<CacheOptions> cacheOptions)
    {
        _redisCacheService = redisCacheService;
        _memoryCache = memoryCache;
        _logger = logger;
        _cacheOptions = cacheOptions.Value;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        // If Redis is disabled, use in-memory cache only
        if (!_cacheOptions.RedisEnabled)
        {
            if (_memoryCache.TryGetValue(key, out T? memoryValue))
            {
                _logger.LogDebug("Cache hit in in-memory cache (Redis disabled) for key: {Key}", key);
                return memoryValue;
            }
            
            _logger.LogDebug("Cache miss in in-memory cache (Redis disabled) for key: {Key}", key);
            return default(T);
        }

        // Try to get from Redis first
        try
        {
            var result = await _redisCacheService.GetAsync<T>(key, cancellationToken);
            if (result != null)
            {
                _logger.LogDebug("Cache hit in Redis for key: {Key}", key);
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get value from Redis cache for key: {Key}. Falling back to in-memory cache.", key);
        }

        // Fall back to in-memory cache if enabled
        if (_cacheOptions.FallbackEnabled && _memoryCache.TryGetValue(key, out T? memoryValue))
        {
            _logger.LogDebug("Cache hit in in-memory cache for key: {Key}", key);
            return memoryValue;
        }

        _logger.LogDebug("Cache miss for key: {Key}", key);
        return default(T);
    }

    public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions? options = null, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            // Set in Redis if enabled
            if (_cacheOptions.RedisEnabled)
            {
                await _redisCacheService.SetAsync(key, value, options, cancellationToken);
            }

            // Also set in in-memory cache for faster access (if enabled or if Redis is disabled)
            if (_cacheOptions.FallbackEnabled || !_cacheOptions.RedisEnabled)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(options?.AbsoluteExpirationRelativeToNow ?? _cacheOptions.DefaultExpiration)
                    .SetSize(CalculateCacheSize(value));

                _memoryCache.Set(key, value, cacheEntryOptions);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set value in cache for key: {Key}.", key);

            // Fall back to in-memory cache only if enabled
            if (_cacheOptions.FallbackEnabled)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(options?.AbsoluteExpirationRelativeToNow ?? _cacheOptions.DefaultExpiration)
                    .SetSize(CalculateCacheSize(value));

                _memoryCache.Set(key, value, cacheEntryOptions);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            // Remove from Redis if enabled
            if (_cacheOptions.RedisEnabled)
            {
                await _redisCacheService.RemoveAsync(key, cancellationToken);
            }

            // Also remove from in-memory cache
            _memoryCache.Remove(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove value from cache for key: {Key}.", key);

            // Remove from in-memory cache regardless
            _memoryCache.Remove(key);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        // If Redis is disabled, check in-memory cache only
        if (!_cacheOptions.RedisEnabled)
        {
            return _memoryCache.TryGetValue(key, out _);
        }

        // Check Redis first
        try
        {
            var existsInRedis = await _redisCacheService.ExistsAsync(key, cancellationToken);
            if (existsInRedis)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check existence in Redis cache for key: {Key}. Checking in in-memory cache.", key);
        }

        // Fall back to in-memory cache if enabled
        if (_cacheOptions.FallbackEnabled)
        {
            return _memoryCache.TryGetValue(key, out _);
        }

        return false;
    }

    private static long CalculateCacheSize<T>(T value)
    {
        if (value == null)
            return 1;

        // Simple size calculation - in a real implementation, you might want to use a more sophisticated approach
        try
        {
            var json = JsonSerializer.Serialize(value);
            return json.Length;
        }
        catch
        {
            // If serialization fails, return a default size
            return 1024; // 1KB default
        }
    }
}