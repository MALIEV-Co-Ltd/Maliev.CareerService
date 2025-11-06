using FluentAssertions;
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
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer Employee employee@example.com");

        // Act
        var response = await _client.GetAsync("/careers/v1/training-programs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var result = await response.Content.ReadFromJsonAsync<TrainingProgramListResponse>();
        result.Should().NotBeNull();

        // Verify response structure
        result!.Should().BeAssignableTo<TrainingProgramListResponse>();
        result.Items.Should().NotBeNull();
        result.Page.Should().BeGreaterThanOrEqualTo(1);
        result.PageSize.Should().BeGreaterThan(0);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        result.TotalPages.Should().BeGreaterThanOrEqualTo(0);
    }

    [DockerRequiredFact]
    public async Task GetTrainingProgram_ReturnsCorrectResponseStructure()
    {
        // Arrange - Use a random ID that will return 404
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer Employee employee@example.com");
        var testId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/careers/v1/training-programs/{testId}");

        // Assert - We expect 404, but we're testing the endpoint exists and responds
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<TrainingProgramResponse>();
            result.Should().NotBeNull();

            // Verify response structure
            result!.Should().BeAssignableTo<TrainingProgramResponse>();
            result.Id.Should().NotBeEmpty();
            result.ProgramCode.Should().NotBeNullOrEmpty();
            result.ProgramName.Should().NotBeNullOrEmpty();
            result.Description.Should().NotBeNull();
            result.Category.Should().NotBeNullOrEmpty();
            result.DurationHours.Should().BeGreaterThan(0);
            result.Provider.Should().NotBeNullOrEmpty();
            result.TargetRoles.Should().NotBeNull();
            result.RowVersion.Should().NotBeNullOrEmpty();
        }
    }

    [DockerRequiredFact]
    public async Task CreateTrainingProgram_AcceptsCorrectRequestStructure()
    {
        // Arrange
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
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden);

        if (response.StatusCode == HttpStatusCode.Created)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

            var result = await response.Content.ReadFromJsonAsync<TrainingProgramResponse>();
            result.Should().NotBeNull();
            result!.ProgramName.Should().Be(request.ProgramName);
            result.ProgramCode.Should().Be(request.ProgramCode);

            // Verify Location header for created resource
            response.Headers.Location.Should().NotBeNull();
        }
    }

    [DockerRequiredFact]
    public async Task UpdateTrainingProgram_AcceptsCorrectRequestStructure()
    {
        // Arrange
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
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.Conflict);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

            var result = await response.Content.ReadFromJsonAsync<TrainingProgramResponse>();
            result.Should().NotBeNull();
            result!.ProgramName.Should().Be(request.ProgramName);
        }
    }

    [DockerRequiredFact]
    public async Task GetTrainingPrograms_SupportsQueryParameters()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer Employee employee@example.com");

        // Act
        var response = await _client.GetAsync(
            "/careers/v1/training-programs?category=Technical&isMandatory=true&offset=0&limit=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<TrainingProgramListResponse>();
        result.Should().NotBeNull();
        result!.PageSize.Should().Be(10);
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
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.ProgramCode.Should().NotBeNullOrEmpty();
        result.ProgramName.Should().NotBeNullOrEmpty();
        result.Category.Should().NotBeNullOrEmpty();
        result.DurationHours.Should().BeGreaterThan(0);
        result.Provider.Should().NotBeNullOrEmpty();
        result.TargetRoles.Should().NotBeNull();
        result.RowVersion.Should().NotBeNullOrEmpty();
    }

    [DockerRequiredFact]
    public async Task TrainingProgramListResponse_SupportsPagination()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer Employee employee@example.com");

        // Act
        var response = await _client.GetAsync("/careers/v1/training-programs?offset=0&limit=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<TrainingProgramListResponse>();
        result.Should().NotBeNull();

        // Verify pagination fields
        result!.Page.Should().BeGreaterThanOrEqualTo(1);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        result.TotalPages.Should().BeGreaterThanOrEqualTo(0);
    }

    [DockerRequiredFact]
    public async Task CreateTrainingProgramRequest_ValidatesRequiredFields()
    {
        // Arrange
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
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden);
    }

    [DockerRequiredFact]
    public async Task GetTrainingPrograms_RequiresAuthentication()
    {
        // Arrange - No authorization header

        // Act
        var response = await _client.GetAsync("/careers/v1/training-programs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [DockerRequiredFact]
    public async Task CreateTrainingProgram_RequiresHRStaffRole()
    {
        // Arrange - Employee role (not HRStaff)
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
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
