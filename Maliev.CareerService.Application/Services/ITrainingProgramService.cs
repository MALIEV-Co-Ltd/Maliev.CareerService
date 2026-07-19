using Maliev.CareerService.Application.Models.TrainingPrograms;

namespace Maliev.CareerService.Application.Services;

/// <summary>
/// Service interface for managing training programs
/// </summary>
public interface ITrainingProgramService
{
    /// <summary>
    /// Gets active training programs with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of active training programs</returns>
    Task<TrainingProgramListResponse> GetActiveProgramsAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific training program by ID
    /// </summary>
    /// <param name="id">Training program ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Training program details or null if not found</returns>
    Task<TrainingProgramResponse?> GetProgramByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Filters training programs with optional filters and pagination
    /// </summary>
    /// <param name="category">Filter by category</param>
    /// <param name="isMandatory">Filter by mandatory status</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of matching training programs</returns>
    Task<TrainingProgramListResponse> FilterProgramsAsync(
        string? category,
        bool? isMandatory,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new training program
    /// </summary>
    /// <param name="request">Training program creation request</param>
    /// <param name="createdBy">User ID of the creator</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created training program details</returns>
    Task<TrainingProgramResponse> CreateProgramAsync(
        CreateTrainingProgramRequest request,
        Guid createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing training program
    /// </summary>
    /// <param name="id">Training program ID</param>
    /// <param name="request">Training program update request</param>
    /// <param name="updatedBy">User ID of the updater</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated training program details or null if not found</returns>
    Task<TrainingProgramResponse?> UpdateProgramAsync(
        Guid id,
        UpdateTrainingProgramRequest request,
        Guid updatedBy,
        CancellationToken cancellationToken = default);
}
