using Maliev.CareerService.Api.Models;

namespace Maliev.CareerService.Api.Services;

public interface IDocumentService
{
    Task<ApplicationDocumentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApplicationDocumentDto>> GetByApplicationIdAsync(int applicationId, CancellationToken cancellationToken = default);
    Task<ApplicationDocumentDto> UploadDocumentAsync(int applicationId, DocumentUploadRequest request, CancellationToken cancellationToken = default);
    Task<DocumentDownloadResponse?> GetDownloadUrlAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<long> GetTotalFileSizeByApplicationAsync(int applicationId, CancellationToken cancellationToken = default);
}