using Maliev.CareerService.Api.Models;
using Maliev.CareerService.Data.DbContexts;
using Maliev.CareerService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Maliev.CareerService.Api.Services;

public class WorkLocationService : IWorkLocationService
{
    private readonly CareerDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WorkLocationService> _logger;
    private readonly CacheOptions _cacheOptions;

    public WorkLocationService(
        CareerDbContext context,
        IMemoryCache cache,
        ILogger<WorkLocationService> logger,
        CacheOptions cacheOptions)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
        _cacheOptions = cacheOptions;
    }

    public async Task<WorkLocationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"worklocation_{id}";
        
        if (_cache.TryGetValue(cacheKey, out WorkLocationDto? cachedLocation))
        {
            return cachedLocation;
        }

        var location = await _context.WorkLocations
            .FirstOrDefaultAsync(wl => wl.Id == id, cancellationToken);

        if (location == null)
            return null;

        var dto = MapToDto(location);
        
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(_cacheOptions.DefaultExpiration)
            .SetSize(1); // Simple size for single object
        
        _cache.Set(cacheKey, dto, cacheEntryOptions);
        
        return dto;
    }

    public async Task<IEnumerable<WorkLocationDto>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"worklocations_all_{activeOnly}";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<WorkLocationDto>? cachedLocations))
        {
            return cachedLocations!;
        }

        var query = _context.WorkLocations.AsQueryable();
        
        if (activeOnly)
        {
            query = query.Where(wl => wl.IsActive);
        }

        var locations = await query
            .OrderBy(wl => wl.Name)
            .ToListAsync(cancellationToken);

        var dtos = locations.Select(MapToDto).ToList();
        
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(_cacheOptions.DefaultExpiration)
            .SetSize(dtos.Count > 0 ? dtos.Count : 1); // Size based on number of locations
        
        _cache.Set(cacheKey, dtos, cacheEntryOptions);
        
        return dtos;
    }

    public async Task<WorkLocationDto> CreateAsync(CreateWorkLocationRequest request, CancellationToken cancellationToken = default)
    {
        var location = new WorkLocation
        {
            Name = request.Name,
            Address = request.Address,
            City = request.City,
            CountryId = request.CountryId,
            IsRemoteAllowed = request.IsRemoteAllowed,
            IsHybrid = request.IsHybrid,
            IsActive = request.IsActive
        };

        _context.WorkLocations.Add(location);
        await _context.SaveChangesAsync(cancellationToken);

        // Clear cache
        ClearLocationCaches();

        _logger.LogInformation("Created work location: {Name} in {City} with ID {Id}", location.Name, location.City, location.Id);

        return MapToDto(location);
    }

    public async Task<WorkLocationDto?> UpdateAsync(int id, UpdateWorkLocationRequest request, CancellationToken cancellationToken = default)
    {
        var location = await _context.WorkLocations.FindAsync(new object[] { id }, cancellationToken);

        if (location == null)
            return null;

        location.Name = request.Name;
        location.Address = request.Address;
        location.City = request.City;
        location.CountryId = request.CountryId;
        location.IsRemoteAllowed = request.IsRemoteAllowed;
        location.IsHybrid = request.IsHybrid;
        location.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        // Clear caches
        ClearLocationCaches();
        var cacheKey = $"worklocation_{id}";
        _cache.Remove(cacheKey);

        _logger.LogInformation("Updated work location: {Name} in {City} with ID {Id}", location.Name, location.City, location.Id);

        return MapToDto(location);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var location = await _context.WorkLocations.FindAsync(new object[] { id }, cancellationToken);
        
        if (location == null)
            return false;

        // Check if location is used by any job positions
        var hasJobPositions = await _context.JobPositionLocations
            .AnyAsync(jpl => jpl.WorkLocationId == id, cancellationToken);

        if (hasJobPositions)
        {
            // Soft delete - mark as inactive instead of removing
            location.IsActive = false;
            await _context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            // Hard delete if not referenced
            _context.WorkLocations.Remove(location);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Clear caches
        ClearLocationCaches();
        var cacheKey = $"worklocation_{id}";
        _cache.Remove(cacheKey);

        _logger.LogInformation("Deleted work location with ID {Id}", id);

        return true;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.WorkLocations.AnyAsync(wl => wl.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAndCityAsync(string name, string city, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.WorkLocations.AsQueryable();
        
        if (excludeId.HasValue)
        {
            query = query.Where(wl => wl.Id != excludeId.Value);
        }

        return await query.AnyAsync(wl => wl.Name == name && wl.City == city, cancellationToken);
    }

    public async Task<IEnumerable<string>> GetCitiesAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "worklocation_cities";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<string>? cachedCities))
        {
            return cachedCities!;
        }

        var cities = await _context.WorkLocations
            .Where(wl => wl.IsActive)
            .Select(wl => wl.City)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(_cacheOptions.LongExpiration)
            .SetSize(cities.Count > 0 ? cities.Count : 1); // Size based on number of cities
        
        _cache.Set(cacheKey, cities, cacheEntryOptions);

        return cities;
    }

    private void ClearLocationCaches()
    {
        _cache.Remove("worklocations_all_true");
        _cache.Remove("worklocations_all_false");
        _cache.Remove("worklocation_cities");
    }

    private static WorkLocationDto MapToDto(WorkLocation location)
    {
        return new WorkLocationDto
        {
            Id = location.Id,
            Name = location.Name,
            Address = location.Address,
            City = location.City,
            CountryId = location.CountryId,
            IsRemoteAllowed = location.IsRemoteAllowed,
            IsHybrid = location.IsHybrid,
            IsActive = location.IsActive,
            CreatedDate = location.CreatedDate,
            ModifiedDate = location.ModifiedDate
        };
    }
}