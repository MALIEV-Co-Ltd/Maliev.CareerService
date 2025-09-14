using Maliev.CareerService.Api.Models;

namespace Maliev.CareerService.Api.Services;

public interface IJobPositionService
{
    Task<JobPositionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<JobPositionDto>> SearchAsync(JobPositionSearchRequest request, CancellationToken cancellationToken = default);
    Task<JobPositionDto> CreateAsync(CreateJobPositionRequest request, CancellationToken cancellationToken = default);
    Task<JobPositionDto?> UpdateAsync(int id, UpdateJobPositionRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByTitleAndDepartmentAsync(string title, string department, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetDepartmentsAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<JobPositionDto>> GetPublicPositionsAsync(JobPositionSearchRequest request, CancellationToken cancellationToken = default);
}