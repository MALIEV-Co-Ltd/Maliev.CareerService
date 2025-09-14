using Maliev.CareerService.Api.Models;

namespace Maliev.CareerService.Api.Services;

public interface ISkillService
{
    Task<SkillDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SkillDto>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<IEnumerable<SkillDto>> GetByCategoryAsync(string category, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<SkillDto> CreateAsync(CreateSkillRequest request, CancellationToken cancellationToken = default);
    Task<SkillDto?> UpdateAsync(int id, UpdateSkillRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<SkillDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
}