
using Maliev.CareerService.Application.Services.External;

namespace Maliev.CareerService.Infrastructure.Services.External;

/// <summary>
/// HTTP client implementation for Employee Service integration
/// </summary>
public class EmployeeServiceClient : IEmployeeServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmployeeServiceClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeServiceClient"/> class.
    /// </summary>

    public EmployeeServiceClient(
        HttpClient httpClient,
        ILogger<EmployeeServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<EmployeeResponse?> GetEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/employees/v1/{employeeId}", cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<EmployeeResponse>(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get employee {EmployeeId} from Employee Service", employeeId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ValidateEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, $"/employees/v1/{employeeId}");
            var response = await _httpClient.SendAsync(request, cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to validate employee {EmployeeId} from Employee Service", employeeId);
            return false;
        }
    }
}
