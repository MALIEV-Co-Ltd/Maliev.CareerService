using Maliev.CareerService.Api.Services.External;

namespace Maliev.CareerService.Tests.Mocks;

/// <summary>
/// Mock implementation of IUploadServiceClient for testing
/// </summary>
public class MockUploadServiceClient : IUploadServiceClient
{
    private readonly HashSet<Guid> _validFileIds = [];

    public MockUploadServiceClient()
    {
        // Add some test file IDs
        _validFileIds.Add(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        _validFileIds.Add(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));
        _validFileIds.Add(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"));
    }

    public Task<bool> ValidateFileAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_validFileIds.Contains(fileId));
    }

    public Task<string?> GetFileUrlAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        if (!_validFileIds.Contains(fileId))
        {
            return Task.FromResult<string?>(null);
        }

        return Task.FromResult<string?>($"https://storage.example.com/files/{fileId}");
    }

    public Task<Dictionary<Guid, string>> GetFileUrlsAsync(IEnumerable<Guid> fileIds, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<Guid, string>();
        foreach (var fileId in fileIds)
        {
            if (_validFileIds.Contains(fileId))
            {
                result[fileId] = $"https://storage.example.com/files/{fileId}";
            }
        }
        return Task.FromResult(result);
    }

    public void AddValidFileId(Guid fileId)
    {
        _validFileIds.Add(fileId);
    }
}
