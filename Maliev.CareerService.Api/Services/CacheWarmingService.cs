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
    private readonly IJobPositionService _jobPositionService;
    private readonly ISkillService _skillService;
    private readonly IWorkLocationService _workLocationService;
    private readonly ILogger<CacheWarmingService> _logger;
    private readonly CacheOptions _cacheOptions;
    private Timer? _timer;

    public CacheWarmingService(
        IJobPositionService jobPositionService,
        ISkillService skillService,
        IWorkLocationService workLocationService,
        ILogger<CacheWarmingService> logger,
        IOptions<CacheOptions> cacheOptions)
    {
        _jobPositionService = jobPositionService;
        _skillService = skillService;
        _workLocationService = workLocationService;
        _logger = logger;
        _cacheOptions = cacheOptions.Value;
    }

    public async Task WarmUpCachesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting cache warming process");

            // Warm up job positions cache
            await WarmUpJobPositionsCacheAsync(cancellationToken);

            // Warm up skills cache
            await WarmUpSkillsCacheAsync(cancellationToken);

            // Warm up work locations cache
            await WarmUpWorkLocationsCacheAsync(cancellationToken);

            _logger.LogInformation("Cache warming process completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during cache warming process");
        }
    }

    private async Task WarmUpJobPositionsCacheAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Warming up job positions cache");

            // Load frequently accessed job positions
            var searchRequest = new JobPositionSearchRequest
            {
                Page = 1,
                PageSize = 10, // Load top 10 job positions
                SortField = "CreatedDate",
                SortDirection = "Descending"
            };

            var recentPositions = await _jobPositionService.SearchAsync(searchRequest, cancellationToken);
            _logger.LogInformation("Warmed up {Count} recent job positions", recentPositions.Items.Count());

            // Load departments cache
            await _jobPositionService.GetDepartmentsAsync(cancellationToken);
            _logger.LogInformation("Warmed up departments cache");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error warming up job positions cache");
        }
    }

    private async Task WarmUpSkillsCacheAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Warming up skills cache");

            // Load all skills
            await _skillService.GetAllAsync(true, cancellationToken);
            _logger.LogInformation("Warmed up all skills cache");

            // Load skill categories
            await _skillService.GetCategoriesAsync(cancellationToken);
            _logger.LogInformation("Warmed up skill categories cache");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error warming up skills cache");
        }
    }

    private async Task WarmUpWorkLocationsCacheAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Warming up work locations cache");

            // Load all work locations
            await _workLocationService.GetAllAsync(true, cancellationToken);
            _logger.LogInformation("Warmed up all work locations cache");

            // Load cities
            await _workLocationService.GetCitiesAsync(cancellationToken);
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