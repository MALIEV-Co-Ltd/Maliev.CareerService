using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Maliev.CareerService.Api.Services;

public interface ICacheVersioningService
{
    string GenerateCacheKey<T>(string baseKey, params object[] parameters);
    string GenerateVersionedCacheKey<T>(string baseKey, params object[] parameters);
    string GenerateCacheVersion(params object[] data);
    Task InvalidateCacheByVersionAsync(string versionPrefix, CancellationToken cancellationToken = default);
    Task<string> GetCurrentVersionAsync(string key, CancellationToken cancellationToken = default);
    Task SetCurrentVersionAsync(string key, string version, CancellationToken cancellationToken = default);
}

public class CacheVersioningService : ICacheVersioningService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<CacheVersioningService> _logger;

    public CacheVersioningService(
        IDistributedCache distributedCache,
        ILogger<CacheVersioningService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public string GenerateCacheKey<T>(string baseKey, params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0)
        {
            return $"{typeof(T).Name}:{baseKey}";
        }

        var keyBuilder = new StringBuilder($"{typeof(T).Name}:{baseKey}:");
        foreach (var param in parameters)
        {
            keyBuilder.Append($"{param?.ToString() ?? "null"}:");
        }

        // Remove trailing colon
        if (keyBuilder.Length > 0 && keyBuilder[keyBuilder.Length - 1] == ':')
        {
            keyBuilder.Length--;
        }

        return keyBuilder.ToString();
    }

    public string GenerateVersionedCacheKey<T>(string baseKey, params object[] parameters)
    {
        var cacheKey = GenerateCacheKey<T>(baseKey, parameters);
        return $"{cacheKey}:v1"; // Default version
    }

    public string GenerateCacheVersion(params object[] data)
    {
        if (data == null || data.Length == 0)
        {
            return "default";
        }

        // Serialize all data to create a consistent version string
        var dataString = string.Join("|", data.Select(d => d?.ToString() ?? "null"));

        // Create a hash of the data to use as the version
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(dataString));
        return Convert.ToBase64String(hashBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public Task InvalidateCacheByVersionAsync(string versionPrefix, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, you would invalidate all cache entries with the given version prefix
            // For now, we'll just log the invalidation
            _logger.LogInformation("Invalidating cache entries with version prefix: {VersionPrefix}", versionPrefix);
            
            // You might want to implement a more sophisticated invalidation mechanism
            // such as maintaining a registry of cache keys by version and removing them
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invalidate cache by version prefix: {VersionPrefix}", versionPrefix);
        }
        
        return Task.CompletedTask;
    }

    public async Task<string> GetCurrentVersionAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var versionKey = $"version:{key}";
            var version = await _distributedCache.GetStringAsync(versionKey, cancellationToken);
            return version ?? "default";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current version for key: {Key}", key);
            return "default";
        }
    }

    public async Task SetCurrentVersionAsync(string key, string version, CancellationToken cancellationToken = default)
    {
        try
        {
            var versionKey = $"version:{key}";
            await _distributedCache.SetStringAsync(versionKey, version, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30) // Keep versions for 30 days
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set current version for key: {Key} to version: {Version}", key, version);
        }
    }
}