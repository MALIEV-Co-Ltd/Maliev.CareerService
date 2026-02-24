using Maliev.CareerService.Api.Models.Enrollments;

namespace Maliev.CareerService.Api.Services;

/// <summary>
/// Service interface for managing training enrollments
/// </summary>
public interface IEnrollmentService
{
    /// <summary>
    /// Enrolls an employee in a training program
    /// </summary>
    /// <param name="request">Enrollment request</param>
    /// <param name="employeeId">Employee ID from JWT claims</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created enrollment details</returns>
    Task<TrainingEnrollmentResponse> EnrollEmployeeAsync(
        EnrollInTrainingRequest request,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets enrollments for a specific employee with optional status filter
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of enrollments</returns>
    Task<TrainingEnrollmentListResponse> GetEmployeeEnrollmentsAsync(
        Guid employeeId,
        string? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a training enrollment as completed (HR staff only)
    /// </summary>
    /// <param name="id">Enrollment ID</param>
    /// <param name="request">Completion request with notes and row version</param>
    /// <param name="markedCompleteBy">HR staff user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated enrollment details or null if not found</returns>
    Task<TrainingEnrollmentResponse?> MarkCompletedAsync(
        Guid id,
        MarkTrainingCompleteRequest request,
        Guid markedCompleteBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if training program has available capacity
    /// </summary>
    /// <param name="trainingProgramId">Training program ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if capacity is available, false otherwise</returns>
    Task<bool> ValidateCapacityAsync(
        Guid trainingProgramId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if employee is already enrolled in the training program
    /// </summary>
    /// <param name="trainingProgramId">Training program ID</param>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if duplicate enrollment exists, false otherwise</returns>
    Task<bool> CheckDuplicateEnrollmentAsync(
        Guid trainingProgramId,
        Guid employeeId,
        CancellationToken cancellationToken = default);
}
