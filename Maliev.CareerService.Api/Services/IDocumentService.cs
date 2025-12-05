using Maliev.CareerService.Api.Models;

namespace Maliev.CareerService.Api.Services;
/// <summary>
/// Service interface for Document operations
/// </summary>

public interface IDocumentService
{
    /// <summary>
    /// Retrieves a document by its identifier.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document if found; otherwise, null.</returns>
    Task<ApplicationDocumentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves all documents for a specific application.
    /// </summary>
    /// <param name="applicationId">The application identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of documents.</returns>
    Task<IEnumerable<ApplicationDocumentDto>> GetByApplicationIdAsync(int applicationId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Uploads a document for an application.
    /// </summary>
    /// <param name="applicationId">The application identifier.</param>
    /// <param name="request">The upload request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The uploaded document information.</returns>
    Task<ApplicationDocumentDto> UploadDocumentAsync(int applicationId, DocumentUploadRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves a download URL for a document.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The download response if found; otherwise, null.</returns>
    Task<DocumentDownloadResponse?> GetDownloadUrlAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Deletes a document.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted; otherwise, false.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Checks if a document exists.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the total file size for all documents in an application.
    /// </summary>
    /// <param name="applicationId">The application identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The total file size in bytes.</returns>
    Task<long> GetTotalFileSizeByApplicationAsync(int applicationId, CancellationToken cancellationToken = default);
}
