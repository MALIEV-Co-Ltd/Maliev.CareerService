using Maliev.CareerService.Application.Models.ELearningResources;

namespace Maliev.CareerService.Application.Services;

/// <summary>
/// Service interface for managing e-learning resources
/// </summary>
public interface IELearningResourceService
{
    /// <summary>
    /// Gets active e-learning resources with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of active e-learning resources</returns>
    Task<ELearningResourceListResponse> GetActiveResourcesAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific e-learning resource by ID
    /// </summary>
    /// <param name="id">E-learning resource ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>E-learning resource details or null if not found</returns>
    Task<ELearningResourceResponse?> GetResourceByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Filters e-learning resources with optional filters and pagination
    /// </summary>
    /// <param name="category">Filter by category</param>
    /// <param name="resourceType">Filter by resource type (Video, Document, Interactive, Quiz)</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of matching e-learning resources</returns>
    Task<ELearningResourceListResponse> FilterResourcesAsync(
        string? category,
        string? resourceType,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
