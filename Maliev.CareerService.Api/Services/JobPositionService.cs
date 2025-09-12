using Maliev.CareerService.Api.Models;
using Maliev.CareerService.Data.DbContexts;
using Maliev.CareerService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Maliev.CareerService.Api.Services;

public class JobPositionService : IJobPositionService
{
    private readonly CareerDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<JobPositionService> _logger;
    private readonly CacheOptions _cacheOptions;

    public JobPositionService(
        CareerDbContext context,
        IMemoryCache cache,
        ILogger<JobPositionService> logger,
        CacheOptions cacheOptions)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
        _cacheOptions = cacheOptions;
    }

    public async Task<JobPositionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"jobposition_{id}";
        
        if (_cache.TryGetValue(cacheKey, out JobPositionDto? cachedPosition))
        {
            return cachedPosition;
        }

        var position = await _context.JobPositions
            .Include(jp => jp.JobPositionLocations)
                .ThenInclude(jpl => jpl.WorkLocation)
            .Include(jp => jp.JobPositionSkills)
                .ThenInclude(jps => jps.Skill)
            .Include(jp => jp.JobApplications)
            .FirstOrDefaultAsync(jp => jp.Id == id, cancellationToken);

        if (position == null)
            return null;

        var dto = MapToDto(position);
        
        _cache.Set(cacheKey, dto, _cacheOptions.DefaultExpiration);
        
        return dto;
    }

    public async Task<PagedResult<JobPositionDto>> SearchAsync(JobPositionSearchRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.JobPositions
            .Include(jp => jp.JobPositionLocations)
                .ThenInclude(jpl => jpl.WorkLocation)
            .Include(jp => jp.JobPositionSkills)
                .ThenInclude(jps => jps.Skill)
            .Include(jp => jp.JobApplications)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            query = query.Where(jp => jp.Title.Contains(request.Title));
        }

        if (!string.IsNullOrWhiteSpace(request.Department))
        {
            query = query.Where(jp => jp.Department.Contains(request.Department));
        }

        if (!string.IsNullOrWhiteSpace(request.EmploymentType))
        {
            query = query.Where(jp => jp.EmploymentType == request.EmploymentType);
        }

        if (!string.IsNullOrWhiteSpace(request.ExperienceLevel))
        {
            query = query.Where(jp => jp.ExperienceLevel == request.ExperienceLevel);
        }

        if (request.WorkLocationIds.Any())
        {
            query = query.Where(jp => jp.JobPositionLocations
                .Any(jpl => request.WorkLocationIds.Contains(jpl.WorkLocationId)));
        }

        if (request.SkillIds.Any())
        {
            query = query.Where(jp => jp.JobPositionSkills
                .Any(jps => request.SkillIds.Contains(jps.SkillId)));
        }

        if (request.MinSalary.HasValue)
        {
            query = query.Where(jp => jp.SalaryRangeMin >= request.MinSalary.Value);
        }

        if (request.MaxSalary.HasValue)
        {
            query = query.Where(jp => jp.SalaryRangeMax <= request.MaxSalary.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Currency))
        {
            query = query.Where(jp => jp.Currency == request.Currency);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(jp => jp.IsActive == request.IsActive.Value);
        }

        if (request.IsPublic.HasValue)
        {
            query = query.Where(jp => jp.IsPublic == request.IsPublic.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(jp => 
                jp.Title.ToLower().Contains(searchTerm) ||
                jp.Description.ToLower().Contains(searchTerm) ||
                jp.Department.ToLower().Contains(searchTerm) ||
                (jp.Requirements != null && jp.Requirements.ToLower().Contains(searchTerm)) ||
                (jp.Responsibilities != null && jp.Responsibilities.ToLower().Contains(searchTerm)));
        }

        // Apply sorting
        query = request.SortBy.ToLower() switch
        {
            "title" => request.SortDescending ? query.OrderByDescending(jp => jp.Title) : query.OrderBy(jp => jp.Title),
            "department" => request.SortDescending ? query.OrderByDescending(jp => jp.Department) : query.OrderBy(jp => jp.Department),
            "employmenttype" => request.SortDescending ? query.OrderByDescending(jp => jp.EmploymentType) : query.OrderBy(jp => jp.EmploymentType),
            "experiencelevel" => request.SortDescending ? query.OrderByDescending(jp => jp.ExperienceLevel) : query.OrderBy(jp => jp.ExperienceLevel),
            "salarymin" => request.SortDescending ? query.OrderByDescending(jp => jp.SalaryRangeMin) : query.OrderBy(jp => jp.SalaryRangeMin),
            "modifieddate" => request.SortDescending ? query.OrderByDescending(jp => jp.ModifiedDate) : query.OrderBy(jp => jp.ModifiedDate),
            _ => request.SortDescending ? query.OrderByDescending(jp => jp.CreatedDate) : query.OrderBy(jp => jp.CreatedDate)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var positions = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = positions.Select(MapToDto).ToList();

        return new PagedResult<JobPositionDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<JobPositionDto> CreateAsync(CreateJobPositionRequest request, CancellationToken cancellationToken = default)
    {
        var position = new JobPosition
        {
            Title = request.Title,
            Department = request.Department,
            Description = request.Description,
            Requirements = request.Requirements,
            Responsibilities = request.Responsibilities,
            EmploymentType = request.EmploymentType,
            ExperienceLevel = request.ExperienceLevel,
            SalaryRangeMin = request.SalaryRangeMin,
            SalaryRangeMax = request.SalaryRangeMax,
            Currency = request.Currency,
            IsActive = request.IsActive,
            IsPublic = request.IsPublic
        };

        _context.JobPositions.Add(position);
        await _context.SaveChangesAsync(cancellationToken);

        // Add work locations
        if (request.WorkLocationIds.Any())
        {
            var locationMappings = request.WorkLocationIds.Select(locationId => new JobPositionLocation
            {
                JobPositionId = position.Id,
                WorkLocationId = locationId
            }).ToList();

            _context.JobPositionLocations.AddRange(locationMappings);
        }

        // Add skills
        if (request.Skills.Any())
        {
            var skillMappings = request.Skills.Select(skill => new JobPositionSkill
            {
                JobPositionId = position.Id,
                SkillId = skill.SkillId,
                RequiredLevel = skill.RequiredLevel,
                IsRequired = skill.IsRequired
            }).ToList();

            _context.JobPositionSkills.AddRange(skillMappings);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Load the complete entity with relationships
        var createdPosition = await _context.JobPositions
            .Include(jp => jp.JobPositionLocations)
                .ThenInclude(jpl => jpl.WorkLocation)
            .Include(jp => jp.JobPositionSkills)
                .ThenInclude(jps => jps.Skill)
            .Include(jp => jp.JobApplications)
            .FirstAsync(jp => jp.Id == position.Id, cancellationToken);

        _logger.LogInformation("Created job position: {Title} with ID {Id}", position.Title, position.Id);

        return MapToDto(createdPosition);
    }

    public async Task<JobPositionDto?> UpdateAsync(int id, UpdateJobPositionRequest request, CancellationToken cancellationToken = default)
    {
        var position = await _context.JobPositions
            .Include(jp => jp.JobPositionLocations)
            .Include(jp => jp.JobPositionSkills)
            .FirstOrDefaultAsync(jp => jp.Id == id, cancellationToken);

        if (position == null)
            return null;

        // Update properties
        position.Title = request.Title;
        position.Department = request.Department;
        position.Description = request.Description;
        position.Requirements = request.Requirements;
        position.Responsibilities = request.Responsibilities;
        position.EmploymentType = request.EmploymentType;
        position.ExperienceLevel = request.ExperienceLevel;
        position.SalaryRangeMin = request.SalaryRangeMin;
        position.SalaryRangeMax = request.SalaryRangeMax;
        position.Currency = request.Currency;
        position.IsActive = request.IsActive;
        position.IsPublic = request.IsPublic;

        // Update work locations
        _context.JobPositionLocations.RemoveRange(position.JobPositionLocations);
        if (request.WorkLocationIds.Any())
        {
            var locationMappings = request.WorkLocationIds.Select(locationId => new JobPositionLocation
            {
                JobPositionId = position.Id,
                WorkLocationId = locationId
            }).ToList();

            _context.JobPositionLocations.AddRange(locationMappings);
        }

        // Update skills
        _context.JobPositionSkills.RemoveRange(position.JobPositionSkills);
        if (request.Skills.Any())
        {
            var skillMappings = request.Skills.Select(skill => new JobPositionSkill
            {
                JobPositionId = position.Id,
                SkillId = skill.SkillId,
                RequiredLevel = skill.RequiredLevel,
                IsRequired = skill.IsRequired
            }).ToList();

            _context.JobPositionSkills.AddRange(skillMappings);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Clear cache
        var cacheKey = $"jobposition_{id}";
        _cache.Remove(cacheKey);

        // Load updated entity with relationships
        var updatedPosition = await _context.JobPositions
            .Include(jp => jp.JobPositionLocations)
                .ThenInclude(jpl => jpl.WorkLocation)
            .Include(jp => jp.JobPositionSkills)
                .ThenInclude(jps => jps.Skill)
            .Include(jp => jp.JobApplications)
            .FirstAsync(jp => jp.Id == id, cancellationToken);

        _logger.LogInformation("Updated job position: {Title} with ID {Id}", position.Title, position.Id);

        return MapToDto(updatedPosition);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var position = await _context.JobPositions.FindAsync(new object[] { id }, cancellationToken);
        
        if (position == null)
            return false;

        // Check if position has related data (applications, locations, skills)
        var hasApplications = await _context.JobApplications
            .AnyAsync(ja => ja.JobPositionId == id, cancellationToken);
        var hasLocations = await _context.JobPositionLocations
            .AnyAsync(jpl => jpl.JobPositionId == id, cancellationToken);
        var hasSkills = await _context.JobPositionSkills
            .AnyAsync(jps => jps.JobPositionId == id, cancellationToken);

        if (hasApplications || hasLocations || hasSkills)
        {
            // Soft delete - mark as inactive instead of removing
            position.IsActive = false;
            await _context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            // Hard delete if not referenced
            _context.JobPositions.Remove(position);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Clear cache
        var cacheKey = $"jobposition_{id}";
        _cache.Remove(cacheKey);

        _logger.LogInformation("Deleted job position with ID {Id}", id);

        return true;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.JobPositions.AnyAsync(jp => jp.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByTitleAndDepartmentAsync(string title, string department, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.JobPositions.AsQueryable();
        
        if (excludeId.HasValue)
        {
            query = query.Where(jp => jp.Id != excludeId.Value);
        }

        return await query.AnyAsync(jp => jp.Title == title && jp.Department == department, cancellationToken);
    }

    public async Task<IEnumerable<string>> GetDepartmentsAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "departments";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<string>? cachedDepartments))
        {
            return cachedDepartments!;
        }

        var departments = await _context.JobPositions
            .Where(jp => jp.IsActive)
            .Select(jp => jp.Department)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync(cancellationToken);

        _cache.Set(cacheKey, departments, _cacheOptions.LongExpiration);

        return departments;
    }

    public async Task<PagedResult<JobPositionDto>> GetPublicPositionsAsync(JobPositionSearchRequest request, CancellationToken cancellationToken = default)
    {
        // Override to only show public and active positions
        request.IsPublic = true;
        request.IsActive = true;
        
        return await SearchAsync(request, cancellationToken);
    }

    private static JobPositionDto MapToDto(JobPosition position)
    {
        return new JobPositionDto
        {
            Id = position.Id,
            Title = position.Title,
            Department = position.Department,
            Description = position.Description,
            Requirements = position.Requirements,
            Responsibilities = position.Responsibilities,
            EmploymentType = position.EmploymentType,
            ExperienceLevel = position.ExperienceLevel,
            SalaryRangeMin = position.SalaryRangeMin,
            SalaryRangeMax = position.SalaryRangeMax,
            Currency = position.Currency,
            IsActive = position.IsActive,
            IsPublic = position.IsPublic,
            CreatedDate = position.CreatedDate,
            ModifiedDate = position.ModifiedDate,
            WorkLocations = position.JobPositionLocations.Select(jpl => new WorkLocationDto
            {
                Id = jpl.WorkLocation.Id,
                Name = jpl.WorkLocation.Name,
                Address = jpl.WorkLocation.Address,
                City = jpl.WorkLocation.City,
                CountryId = jpl.WorkLocation.CountryId,
                IsRemoteAllowed = jpl.WorkLocation.IsRemoteAllowed,
                IsHybrid = jpl.WorkLocation.IsHybrid,
                IsActive = jpl.WorkLocation.IsActive,
                CreatedDate = jpl.WorkLocation.CreatedDate,
                ModifiedDate = jpl.WorkLocation.ModifiedDate
            }).ToList(),
            Skills = position.JobPositionSkills.Select(jps => new JobPositionSkillDto
            {
                SkillId = jps.Skill.Id,
                SkillName = jps.Skill.Name,
                SkillCategory = jps.Skill.Category,
                RequiredLevel = jps.RequiredLevel,
                IsRequired = jps.IsRequired
            }).ToList(),
            ApplicationCount = position.JobApplications.Count
        };
    }
}