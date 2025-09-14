using Microsoft.Extensions.Caching.Memory;

namespace Maliev.CareerService.Api.Services;

public interface ICacheInvalidationService
{
    void InvalidateJobPositionCache(int? jobPositionId = null);
    void InvalidateSkillCache(int? skillId = null);
    void InvalidateWorkLocationCache(int? workLocationId = null);
    void InvalidateJobApplicationCache(int? jobApplicationId = null, int? jobPositionId = null);
    void InvalidateRelatedCachesForJobPosition(int jobPositionId);
    void InvalidateRelatedCachesForSkill(int skillId);
    void InvalidateRelatedCachesForWorkLocation(int workLocationId);
}

public class CacheInvalidationService : ICacheInvalidationService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheInvalidationService> _logger;

    public CacheInvalidationService(IMemoryCache cache, ILogger<CacheInvalidationService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public void InvalidateJobPositionCache(int? jobPositionId = null)
    {
        if (jobPositionId.HasValue)
        {
            var cacheKey = $"jobposition_{jobPositionId.Value}";
            _cache.Remove(cacheKey);
            _logger.LogDebug("Invalidated job position cache for ID {JobPositionId}", jobPositionId.Value);
        }
        else
        {
            // Invalidate all job position caches (more sophisticated approach would use tags)
            // For now, we'll rely on natural expiration
            _logger.LogDebug("Requested bulk job position cache invalidation (will expire naturally)");
        }

        // Also invalidate related caches that might be affected
        _cache.Remove("jobposition_departments");
        _logger.LogDebug("Invalidated job position departments cache");
    }

    public void InvalidateSkillCache(int? skillId = null)
    {
        if (skillId.HasValue)
        {
            // Individual skill caches are not keyed separately in current implementation,
            // so we invalidate the broader caches
            _logger.LogDebug("Requested skill cache invalidation for ID {SkillId} (will be handled by broader invalidation)", skillId.Value);
        }

        // Invalidate all skill-related caches
        _cache.Remove("skills_all_true");
        _cache.Remove("skills_all_false");
        _cache.Remove("skill_categories");
        _logger.LogDebug("Invalidated all skill caches");
    }

    public void InvalidateWorkLocationCache(int? workLocationId = null)
    {
        if (workLocationId.HasValue)
        {
            // Individual work location caches are not keyed separately in current implementation,
            // so we invalidate the broader caches
            _logger.LogDebug("Requested work location cache invalidation for ID {WorkLocationId} (will be handled by broader invalidation)", workLocationId.Value);
        }

        // Invalidate all work location-related caches
        _cache.Remove("worklocations_all_true");
        _cache.Remove("worklocations_all_false");
        _cache.Remove("worklocation_cities");
        _logger.LogDebug("Invalidated all work location caches");
    }

    public void InvalidateJobApplicationCache(int? jobApplicationId = null, int? jobPositionId = null)
    {
        if (jobApplicationId.HasValue)
        {
            var cacheKey = $"jobapplication_{jobApplicationId.Value}";
            _cache.Remove(cacheKey);
            _logger.LogDebug("Invalidated job application cache for ID {JobApplicationId}", jobApplicationId.Value);
        }
        else
        {
            _logger.LogDebug("Requested bulk job application cache invalidation (will expire naturally)");
        }

        // If we know the job position ID, invalidate that as well since it affects the count
        if (jobPositionId.HasValue)
        {
            InvalidateJobPositionCache(jobPositionId.Value);
        }
    }

    public void InvalidateRelatedCachesForJobPosition(int jobPositionId)
    {
        // When a job position is updated/deleted, invalidate:
        // 1. The job position itself
        InvalidateJobPositionCache(jobPositionId);
        
        // 2. Related skill caches (if needed)
        // Note: In current implementation, skills are not cached individually
        
        // 3. Related work location caches (if needed)
        // Note: In current implementation, work locations are not cached individually
        
        // 4. Departments cache (affected by job positions)
        _cache.Remove("jobposition_departments");
        _logger.LogDebug("Invalidated job position departments cache due to job position {JobPositionId} change", jobPositionId);
    }

    public void InvalidateRelatedCachesForSkill(int skillId)
    {
        // When a skill is updated/deleted, invalidate:
        // 1. All skill caches (since skills might be referenced in job positions)
        InvalidateSkillCache();
        
        // 2. Related job position caches (more sophisticated implementation would invalidate only affected job positions)
        // For now, we'll rely on natural expiration of job position caches
        _logger.LogDebug("Invalidated all skill caches due to skill {SkillId} change", skillId);
    }

    public void InvalidateRelatedCachesForWorkLocation(int workLocationId)
    {
        // When a work location is updated/deleted, invalidate:
        // 1. All work location caches
        InvalidateWorkLocationCache();
        
        // 2. Related job position caches (more sophisticated implementation would invalidate only affected job positions)
        // For now, we'll rely on natural expiration of job position caches
        _logger.LogDebug("Invalidated all work location caches due to work location {WorkLocationId} change", workLocationId);
    }
}