using Maliev.CareerService.Application.Models.JobPostings;

namespace Maliev.CareerService.Application.Services;

/// <summary>
/// Service interface for managing job postings
/// </summary>
public interface IJobPostingService
{
    /// <summary>
    /// Gets active job postings with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of active job postings</returns>
    Task<JobPostingListResponse> GetActivePostingsAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific job posting by ID
    /// </summary>
    /// <param name="id">Job posting ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Job posting details or null if not found</returns>
    Task<JobPostingResponse?> GetPostingByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches job postings with filters and pagination
    /// </summary>
    /// <param name="searchTerm">Search term for position title, description, requirements, or responsibilities</param>
    /// <param name="department">Filter by department</param>
    /// <param name="location">Filter by location</param>
    /// <param name="employmentType">Filter by employment type</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of matching job postings</returns>
    Task<JobPostingListResponse> SearchPostingsAsync(
        string? searchTerm,
        string? department,
        string? location,
        string? employmentType,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new job posting
    /// </summary>
    /// <param name="request">Job posting creation request</param>
    /// <param name="createdBy">User ID of the creator</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created job posting details</returns>
    Task<JobPostingResponse> CreatePostingAsync(
        CreateJobPostingRequest request,
        Guid createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing job posting
    /// </summary>
    /// <param name="id">Job posting ID</param>
    /// <param name="request">Job posting update request</param>
    /// <param name="updatedBy">User ID of the updater</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated job posting details or null if not found</returns>
    Task<JobPostingResponse?> UpdatePostingAsync(
        Guid id,
        UpdateJobPostingRequest request,
        Guid updatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a job posting
    /// </summary>
    /// <param name="id">Job posting ID</param>
    /// <param name="deletedBy">User ID of the deleter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully, false if not found</returns>
    Task<bool> DeletePostingAsync(
        Guid id,
        Guid deletedBy,
        CancellationToken cancellationToken = default);
}
