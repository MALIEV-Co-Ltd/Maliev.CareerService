
using Maliev.CareerService.Application.Mapping;
using Maliev.CareerService.Application.Models.ELearningResources;
using Maliev.CareerService.Infrastructure.Data;
using Maliev.CareerService.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Infrastructure.Services;

/// <summary>
/// Service implementation for managing e-learning resources
/// </summary>
public class ELearningResourceService(
    CareerDbContext dbContext,
    ILogger<ELearningResourceService> logger) : IELearningResourceService
{
    private readonly CareerDbContext _dbContext = dbContext;
    private readonly ILogger<ELearningResourceService> _logger = logger;

    /// <inheritdoc />
    public async Task<ELearningResourceListResponse> GetActiveResourcesAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ELearningResources
            .Where(r => r.IsActive)
            .OrderBy(r => r.Title);

        var totalCount = await query.CountAsync(cancellationToken);

        var resources = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var responses = resources.Select(r => r.ToELearningResourceResponse()).ToList();

        return new ELearningResourceListResponse
        {
            Items = responses,
            Page = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    /// <inheritdoc />
    public async Task<ELearningResourceResponse?> GetResourceByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var resource = await _dbContext.ELearningResources
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (resource == null)
        {
            return null;
        }

        return resource.ToELearningResourceResponse();
    }

    /// <inheritdoc />
    public async Task<ELearningResourceListResponse> FilterResourcesAsync(
        string? category,
        string? resourceType,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ELearningResources
            .Where(r => r.IsActive);

        // Apply category filter
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(r => r.Category == category);
        }

        // Apply resource type filter
        if (!string.IsNullOrWhiteSpace(resourceType))
        {
            query = query.Where(r => r.ResourceType == resourceType);
        }

        query = query.OrderBy(r => r.Title);

        var totalCount = await query.CountAsync(cancellationToken);

        var resources = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var responses = resources.Select(r => r.ToELearningResourceResponse()).ToList();

        return new ELearningResourceListResponse
        {
            Items = responses,
            Page = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}
