using Maliev.CareerService.Api.Models.TrainingRecords;

namespace Maliev.CareerService.Api.Services;

/// <summary>
/// Service interface for managing training records (Feature 003)
/// </summary>
public interface ITrainingRecordService
{
    /// <summary>
    /// Records a training completion for an employee
    /// </summary>
    Task<TrainingRecordResponse> RecordCompletionAsync(
        Guid employeeId,
        RecordTrainingCompletionRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all training records for an employee
    /// </summary>
    Task<TrainingRecordListResponse> GetByEmployeeIdAsync(
        Guid employeeId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific training record by ID
    /// </summary>
    Task<TrainingRecordResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing training record
    /// </summary>
    Task<TrainingRecordResponse?> UpdateAsync(
        Guid id,
        UpdateTrainingRecordRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all training records expiring within the specified number of days
    /// </summary>
    Task<TrainingRecordListResponse> GetExpiringAsync(
        int days,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a training record
    /// </summary>
    Task<bool> DeleteAsync(
        Guid id,
        Guid currentUserId,
        CancellationToken cancellationToken = default);
}
