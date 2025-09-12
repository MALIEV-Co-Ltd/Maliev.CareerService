using Maliev.CareerService.Api.Models;

namespace Maliev.CareerService.Api.Services;

public interface IJobApplicationService
{
    Task<JobApplicationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<JobApplicationDto>> GetByJobPositionIdAsync(int jobPositionId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<PagedResult<JobApplicationDto>> GetAllAsync(int page = 1, int pageSize = 20, string? status = null, CancellationToken cancellationToken = default);
    Task<JobApplicationDto> CreateAsync(CreateJobApplicationRequest request, CancellationToken cancellationToken = default);
    Task<JobApplicationDto?> UpdateStatusAsync(int id, UpdateJobApplicationRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> HasExistingApplicationAsync(string email, int jobPositionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<JobApplicationDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}