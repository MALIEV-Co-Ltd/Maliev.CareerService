using Maliev.CareerService.Api.Models;
using Maliev.CareerService.Data.DbContexts;
using Maliev.CareerService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Maliev.CareerService.Api.Services;

public class SkillService : ISkillService
{
    private readonly CareerDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly CacheOptions _cacheOptions;
    private readonly ILogger<SkillService> _logger;
    private readonly ICacheInvalidationService _cacheInvalidationService;

    public SkillService(
        CareerDbContext context,
        IMemoryCache cache,
        CacheOptions cacheOptions,
        ILogger<SkillService> logger,
        ICacheInvalidationService cacheInvalidationService)
    {
        _context = context;
        _cache = cache;
        _cacheOptions = cacheOptions;
        _logger = logger;
        _cacheInvalidationService = cacheInvalidationService;
    }

    public async Task<SkillDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"skill_{id}";
        
        if (_cache.TryGetValue(cacheKey, out SkillDto? cachedSkill))
        {
            return cachedSkill;
        }

        var skill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (skill == null)
            return null;

        var dto = MapToDto(skill);
        
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(_cacheOptions.DefaultExpiration)
            .SetSize(1); // Simple size for single object
        
        _cache.Set(cacheKey, dto, cacheEntryOptions);
        
        return dto;
    }

    public async Task<IEnumerable<SkillDto>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"skills_all_{activeOnly}";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<SkillDto>? cachedSkills))
        {
            return cachedSkills!;
        }

        var query = _context.Skills.AsQueryable();
        
        if (activeOnly)
        {
            query = query.Where(s => s.IsActive);
        }

        var skills = await query
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToListAsync(cancellationToken);

        var dtos = skills.Select(MapToDto).ToList();
        
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(_cacheOptions.LongExpiration)
            .SetSize(dtos.Count > 0 ? dtos.Count : 1); // Size based on number of skills
        
        _cache.Set(cacheKey, dtos, cacheEntryOptions);
        
        return dtos;
    }

    public async Task<IEnumerable<SkillDto>> GetByCategoryAsync(string category, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"skills_category_{category}_{activeOnly}";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<SkillDto>? cachedSkills))
        {
            return cachedSkills!;
        }

        var query = _context.Skills.AsQueryable();
        
        if (activeOnly)
        {
            query = query.Where(s => s.IsActive);
        }

        var skills = await query
            .Where(s => s.Category == category)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        var dtos = skills.Select(MapToDto).ToList();
        
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(_cacheOptions.DefaultExpiration)
            .SetSize(dtos.Count > 0 ? dtos.Count : 1); // Size based on number of skills
        
        _cache.Set(cacheKey, dtos, cacheEntryOptions);
        
        return dtos;
    }

    public async Task<SkillDto> CreateAsync(CreateSkillRequest request, CancellationToken cancellationToken = default)
    {
        var skill = new Skill
        {
            Name = request.Name,
            Category = request.Category,
            IsActive = request.IsActive
        };

        _context.Skills.Add(skill);
        await _context.SaveChangesAsync(cancellationToken);

        // Clear caches using the cache invalidation service
        _cacheInvalidationService.InvalidateRelatedCachesForSkill(id);

        _logger.LogInformation("Created skill: {Name} in category {Category} with ID {Id}", skill.Name, skill.Category, skill.Id);

        return MapToDto(skill);
    }

    public async Task<SkillDto?> UpdateAsync(int id, UpdateSkillRequest request, CancellationToken cancellationToken = default)
    {
        var skill = await _context.Skills.FindAsync(new object[] { id }, cancellationToken);

        if (skill == null)
            return null;

        skill.Name = request.Name;
        skill.Category = request.Category;
        skill.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        // Clear caches using the cache invalidation service
        _cacheInvalidationService.InvalidateRelatedCachesForSkill(id);

        _logger.LogInformation("Updated skill: {Name} in category {Category} with ID {Id}", skill.Name, skill.Category, skill.Id);

        return MapToDto(skill);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var skill = await _context.Skills.FindAsync(new object[] { id }, cancellationToken);
        
        if (skill == null)
            return false;

        // Check if skill is used by any job positions
        var hasJobPositions = await _context.JobPositionSkills
            .AnyAsync(jps => jps.SkillId == id, cancellationToken);

        if (hasJobPositions)
        {
            // Soft delete - mark as inactive instead of removing
            skill.IsActive = false;
            await _context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            // Hard delete if not referenced
            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Clear caches
        ClearSkillCaches();
        var cacheKey = $"skill_{id}";
        _cache.Remove(cacheKey);

        _logger.LogInformation("Deleted skill with ID {Id}", id);

        return true;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Skills.AnyAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Skills.AsQueryable();
        
        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return await query.AnyAsync(s => s.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "skill_categories";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<string>? cachedCategories))
        {
            return cachedCategories!;
        }

        var categories = await _context.Skills
            .Where(s => s.IsActive && s.Category != null)
            .Select(s => s.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(_cacheOptions.LongExpiration)
            .SetSize(categories.Count > 0 ? categories.Count : 1); // Size based on number of categories
        
        _cache.Set(cacheKey, categories, cacheEntryOptions);

        return categories;
    }

    public async Task<IEnumerable<SkillDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllAsync(true, cancellationToken);
        }

        var lowerSearchTerm = searchTerm.ToLower();
        
        var skills = await _context.Skills
            .Where(s => s.IsActive)
            .Where(s => s.Name.ToLower().Contains(lowerSearchTerm) || 
                       (s.Category != null && s.Category.ToLower().Contains(lowerSearchTerm)))
            .OrderBy(s => s.Name.ToLower().StartsWith(lowerSearchTerm) ? 0 : 1) // Exact matches first
            .ThenBy(s => s.Name)
            .Take(50) // Limit results for performance
            .ToListAsync(cancellationToken);

        return skills.Select(MapToDto);
    }

    private void ClearSkillCaches()
    {
        _cache.Remove("skills_all_true");
        _cache.Remove("skills_all_false");
        _cache.Remove("skill_categories");
        
        // Clear category-specific caches (this is a simplified approach - in production you might want a more sophisticated cache invalidation)
        // For now, we'll let them expire naturally
    }

    private static SkillDto MapToDto(Skill skill)
    {
        return new SkillDto
        {
            Id = skill.Id,
            Name = skill.Name,
            Category = skill.Category,
            IsActive = skill.IsActive,
            CreatedDate = skill.CreatedDate,
            ModifiedDate = skill.ModifiedDate
        };
    }
}