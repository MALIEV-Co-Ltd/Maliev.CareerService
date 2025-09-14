using Maliev.CareerService.Api.Models;

namespace Maliev.CareerService.Api.Services;

public interface IWorkLocationService
{
    Task<WorkLocationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkLocationDto>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<WorkLocationDto> CreateAsync(CreateWorkLocationRequest request, CancellationToken cancellationToken = default);
    Task<WorkLocationDto?> UpdateAsync(int id, UpdateWorkLocationRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAndCityAsync(string name, string city, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetCitiesAsync(CancellationToken cancellationToken = default);
}