using Maliev.CareerService.Api.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Maliev.CareerService.Api.Services;

public interface ICacheWarmingService
{
    Task WarmUpCachesAsync(CancellationToken cancellationToken = default);
}

public class CacheWarmingService : ICacheWarmingService, IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<CacheWarmingService> _logger;
    private readonly CacheOptions _cacheOptions;
    private Timer? _timer;

    public CacheWarmingService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<CacheWarmingService> logger,
        IOptions<CacheOptions> cacheOptions)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _cacheOptions = cacheOptions.Value;
    }

    public async Task WarmUpCachesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting cache warming process");

            using var scope = _serviceScopeFactory.CreateScope();
            var jobPositionService = scope.ServiceProvider.GetRequiredService<IJobPositionService>();
            var skillService = scope.ServiceProvider.GetRequiredService<ISkillService>();
            var workLocationService = scope.ServiceProvider.GetRequiredService<IWorkLocationService>();

            // Warm up job positions cache
            await WarmUpJobPositionsCacheAsync(jobPositionService, cancellationToken);

            // Warm up skills cache
            await WarmUpSkillsCacheAsync(skillService, cancellationToken);

            // Warm up work locations cache
            await WarmUpWorkLocationsCacheAsync(workLocationService, cancellationToken);

            _logger.LogInformation("Cache warming process completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during cache warming process");
        }
    }

    private async Task WarmUpJobPositionsCacheAsync(IJobPositionService jobPositionService, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Warming up job positions cache");

            // Load frequently accessed job positions
            var searchRequest = new JobPositionSearchRequest
            {
                Page = 1,
                PageSize = 10, // Load top 10 job positions
                SortBy = "CreatedDate",
                SortDescending = true
            };

            var recentPositions = await jobPositionService.SearchAsync(searchRequest, cancellationToken);
            _logger.LogInformation("Warmed up {Count} recent job positions", recentPositions.Items.Count());

            // Load departments cache
            await jobPositionService.GetDepartmentsAsync(cancellationToken);
            _logger.LogInformation("Warmed up departments cache");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error warming up job positions cache");
        }
    }

    private async Task WarmUpSkillsCacheAsync(ISkillService skillService, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Warming up skills cache");

            // Load all skills
            await skillService.GetAllAsync(true, cancellationToken);
            _logger.LogInformation("Warmed up all skills cache");

            // Load skill categories
            await skillService.GetCategoriesAsync(cancellationToken);
            _logger.LogInformation("Warmed up skill categories cache");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error warming up skills cache");
        }
    }

    private async Task WarmUpWorkLocationsCacheAsync(IWorkLocationService workLocationService, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Warming up work locations cache");

            // Load all work locations
            await workLocationService.GetAllAsync(true, cancellationToken);
            _logger.LogInformation("Warmed up all work locations cache");

            // Load cities
            await workLocationService.GetCitiesAsync(cancellationToken);
            _logger.LogInformation("Warmed up cities cache");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error warming up work locations cache");
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cache warming service is starting");

        // Run cache warming immediately on startup
        _ = Task.Run(() => WarmUpCachesAsync(cancellationToken), cancellationToken);

        // Schedule periodic cache warming based on cache expiration
        var warmingInterval = TimeSpan.FromMinutes(Math.Max(_cacheOptions.DefaultExpiration.TotalMinutes / 2, 30));
        _timer = new Timer(async _ => await WarmUpCachesAsync(cancellationToken), null, warmingInterval, warmingInterval);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cache warming service is stopping");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}