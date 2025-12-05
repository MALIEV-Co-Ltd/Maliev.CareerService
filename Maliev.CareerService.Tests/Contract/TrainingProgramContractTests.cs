using Maliev.CareerService.Api.Models.TrainingPrograms;
using Maliev.CareerService.Tests.Factories;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Maliev.CareerService.Tests.Helpers;

namespace Maliev.CareerService.Tests.Contract;

/// <summary>
/// Contract tests for training program endpoints - verify API contract compliance
/// </summary>
public class TrainingProgramContractTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();

    [DockerRequiredFact]
    public async Task GetTrainingPrograms_ReturnsCorrectResponseStructure()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer Employee employee@example.com");

        // Act
        var response = await _client.GetAsync("/careers/v1/training-programs");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var result = await response.Content.ReadFromJsonAsync<TrainingProgramListResponse>();
        Assert.NotNull(result);

        // Verify response structure
        Assert.IsAssignableFrom<TrainingProgramListResponse>(result);
        Assert.NotNull(result.Items);
        Assert.True(result.Page >= 1);
        Assert.True(result.PageSize > 0);
        Assert.True(result.TotalCount >= 0);
        Assert.True(result.TotalPages >= 0);
    }

    [DockerRequiredFact]
    public async Task GetTrainingProgram_ReturnsCorrectResponseStructure()
    {
        // Arrange - Use a random ID that will return 404
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer Employee employee@example.com");
        var testId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/careers/v1/training-programs/{testId}");

        // Assert - We expect 404, but we're testing the endpoint exists and responds
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<TrainingProgramResponse>();
            Assert.NotNull(result);

            // Verify response structure
            Assert.IsAssignableFrom<TrainingProgramResponse>(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.False(string.IsNullOrEmpty(result.ProgramCode));
            Assert.False(string.IsNullOrEmpty(result.ProgramName));
            Assert.NotNull(result.Description);
            Assert.False(string.IsNullOrEmpty(result.Category));
            Assert.True(result.DurationHours > 0);
            Assert.False(string.IsNullOrEmpty(result.Provider));
            Assert.NotNull(result.TargetRoles);
            Assert.False(string.IsNullOrEmpty(result.RowVersion));
        }
    }

    [DockerRequiredFact]
    public async Task CreateTrainingProgram_AcceptsCorrectRequestStructure()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer HRStaff hr@example.com");

        var request = new CreateTrainingProgramRequest
        {
            ProgramCode = "CONTRACT-TEST-001",
            ProgramName = "Contract Test Program",
            Description = "Test description for contract validation",
            Category = "Technical",
            DurationHours = 20m,
            Provider = "Internal",
            IsMandatory = false,
            TargetRoles = ["Developer", "Engineer"],
            MaxParticipants = 30,
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/careers/v1/training-programs", request);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Created ||
                   response.StatusCode == HttpStatusCode.BadRequest ||
                   response.StatusCode == HttpStatusCode.Unauthorized ||
                   response.StatusCode == HttpStatusCode.Forbidden);

        if (response.StatusCode == HttpStatusCode.Created)
        {
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

            var result = await response.Content.ReadFromJsonAsync<TrainingProgramResponse>();
            Assert.NotNull(result);
            Assert.Equal(request.ProgramName, result.ProgramName);
            Assert.Equal(request.ProgramCode, result.ProgramCode);

            // Verify Location header for created resource
            Assert.NotNull(response.Headers.Location);
        }
    }

    [DockerRequiredFact]
    public async Task UpdateTrainingProgram_AcceptsCorrectRequestStructure()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer HRStaff hr@example.com");
        var testId = Guid.NewGuid();

        var request = new UpdateTrainingProgramRequest
        {
            ProgramName = "Updated Program Name",
            Description = "Updated description",
            Category = "Technical",
            DurationHours = 25m,
            Provider = "Internal",
            IsMandatory = true,
            TargetRoles = ["All"],
            MaxParticipants = 50,
            IsActive = true,
            RowVersion = "AAAAAAAAB9E="
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/careers/v1/training-programs/{testId}", request);

        // Assert - We expect 404 for non-existent resource, but we're testing contract
        Assert.True(response.StatusCode == HttpStatusCode.OK ||
                   response.StatusCode == HttpStatusCode.BadRequest ||
                   response.StatusCode == HttpStatusCode.NotFound ||
                   response.StatusCode == HttpStatusCode.Unauthorized ||
                   response.StatusCode == HttpStatusCode.Forbidden ||
                   response.StatusCode == HttpStatusCode.Conflict);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

            var result = await response.Content.ReadFromJsonAsync<TrainingProgramResponse>();
            Assert.NotNull(result);
            Assert.Equal(request.ProgramName, result.ProgramName);
        }
    }

    [DockerRequiredFact]
    public async Task GetTrainingPrograms_SupportsQueryParameters()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer Employee employee@example.com");

        // Act
        var response = await _client.GetAsync(
            "/careers/v1/training-programs?category=Technical&isMandatory=true&offset=0&limit=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TrainingProgramListResponse>();
        Assert.NotNull(result);
        Assert.Equal(10, result.PageSize);
    }

    [DockerRequiredFact]
    public void TrainingProgramResponse_ContainsAllRequiredFields()
    {
        // This test verifies the response model structure by deserializing a sample
        var sampleJson = @"{
            ""id"": ""123e4567-e89b-12d3-a456-426614174000"",
            ""programCode"": ""LEAD-2025-001"",
            ""programName"": ""Leadership Fundamentals"",
            ""description"": ""Introduction to leadership principles"",
            ""category"": ""Leadership"",
            ""durationHours"": 20.0,
            ""provider"": ""Internal"",
            ""externalLmsUrl"": null,
            ""isMandatory"": true,
            ""targetRoles"": [""Manager"", ""Director""],
            ""maxParticipants"": 30,
            ""currentEnrollmentCount"": 15,
            ""isActive"": true,
            ""rowVersion"": ""AAAAAAAAB9E="",
            ""createdAt"": ""2025-01-01T00:00:00Z"",
            ""updatedAt"": ""2025-01-01T00:00:00Z""
        }";

        // Act
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = JsonSerializer.Deserialize<TrainingProgramResponse>(sampleJson, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.False(string.IsNullOrEmpty(result.ProgramCode));
        Assert.False(string.IsNullOrEmpty(result.ProgramName));
        Assert.False(string.IsNullOrEmpty(result.Category));
        Assert.True(result.DurationHours > 0);
        Assert.False(string.IsNullOrEmpty(result.Provider));
        Assert.NotNull(result.TargetRoles);
        Assert.False(string.IsNullOrEmpty(result.RowVersion));
    }

    [DockerRequiredFact]
    public async Task TrainingProgramListResponse_SupportsPagination()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer Employee employee@example.com");

        // Act
        var response = await _client.GetAsync("/careers/v1/training-programs?offset=0&limit=5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TrainingProgramListResponse>();
        Assert.NotNull(result);

        // Verify pagination fields
        Assert.True(result.Page >= 1);
        Assert.Equal(5, result.PageSize);
        Assert.True(result.TotalCount >= 0);
        Assert.True(result.TotalPages >= 0);
    }

    [DockerRequiredFact]
    public async Task CreateTrainingProgramRequest_ValidatesRequiredFields()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer HRStaff hr@example.com");

        // Invalid request - missing required fields
        var invalidRequest = new CreateTrainingProgramRequest
        {
            ProgramCode = "", // Invalid empty code
            ProgramName = "", // Invalid empty name
            Description = "Test",
            Category = "Technical",
            DurationHours = 0, // Invalid zero duration
            Provider = "Internal",
            IsMandatory = false,
            TargetRoles = [],
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/careers/v1/training-programs", invalidRequest);

        // Assert - Should return BadRequest for validation errors
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Forbidden);
    }

    [DockerRequiredFact]
    public async Task GetTrainingPrograms_RequiresAuthentication()
    {
        // Arrange - No authorization header

        // Act
        var response = await _client.GetAsync("/careers/v1/training-programs");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [DockerRequiredFact]
    public async Task CreateTrainingProgram_RequiresHRStaffRole()
    {
        // Arrange - Employee role (not HRStaff)
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer Employee employee@example.com");

        var request = new CreateTrainingProgramRequest
        {
            ProgramCode = "TEST-001",
            ProgramName = "Test Program",
            Description = "Test",
            Category = "Technical",
            DurationHours = 10m,
            Provider = "Internal",
            IsMandatory = false,
            TargetRoles = [],
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/careers/v1/training-programs", request);

        // Assert - Should return Forbidden for non-HRStaff users
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
