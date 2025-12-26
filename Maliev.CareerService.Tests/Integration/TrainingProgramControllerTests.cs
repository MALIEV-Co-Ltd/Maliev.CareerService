using Maliev.CareerService.Api.Models.TrainingPrograms;
using Maliev.CareerService.Data.Models;
using Maliev.CareerService.Tests.Factories;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Maliev.CareerService.Tests.Integration;

/// <summary>
/// Integration tests for TrainingProgramsController
/// </summary>
public class TrainingProgramControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TrainingProgramControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // Add Employee authorization header with Read permission
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _factory.CreateTestJwtToken("employee-id", null, new[] { "career.trainings.read" }));
    }

    [Fact]
    public async Task GetTrainingPrograms_WithoutFilters_ReturnsActivePrograms()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/career/v1/training-programs");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TrainingProgramListResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result!.Items);
        Assert.True(result.TotalCount > 0);
        Assert.All(result.Items, p => Assert.True(p.IsActive));
    }

    [Fact]
    public async Task GetTrainingPrograms_WithCategoryFilter_ReturnsFilteredPrograms()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/career/v1/training-programs?category=Leadership");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TrainingProgramListResponse>();
        Assert.NotNull(result);
        Assert.All(result!.Items, p => Assert.Equal("Leadership", p.Category));
    }

    [Fact]
    public async Task GetTrainingPrograms_WithMandatoryFilter_ReturnsFilteredPrograms()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/career/v1/training-programs?isMandatory=true");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TrainingProgramListResponse>();
        Assert.NotNull(result);
        Assert.All(result!.Items, p => Assert.True(p.IsMandatory));
    }

    [Fact]
    public async Task GetTrainingPrograms_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/career/v1/training-programs?offset=0&limit=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TrainingProgramListResponse>();
        Assert.NotNull(result);
        Assert.True(result!.Items.Count <= 2);
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
    }

    [Fact]
    public async Task GetTrainingProgramById_ExistingId_ReturnsProgram()
    {
        // Arrange
        var programId = await SeedSingleProgramAsync();

        // Act
        var response = await _client.GetAsync($"/career/v1/training-programs/{programId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TrainingProgramResponse>();
        Assert.NotNull(result);
        Assert.Equal(programId, result!.Id);
        Assert.False(string.IsNullOrEmpty(result.ProgramName));
        Assert.True(result.DurationHours > 0);
    }

    [Fact]
    public async Task GetTrainingProgramById_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/career/v1/training-programs/{nonExistingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateTrainingProgram_WithCreatePermission_ReturnsCreated()
    {
        // Arrange
        var permissions = new[] { "career.trainings.create" };
        var token = _factory.CreateTestJwtToken("hr-staff-id", new[] { "career-hr" }, permissions);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new CreateTrainingProgramRequest
        {
            ProgramCode = "LEAD-2025-999",
            ProgramName = "Advanced Leadership Training",
            Description = "Comprehensive leadership development program",
            Category = "Leadership",
            DurationHours = 40m,
            Provider = "Internal",
            IsMandatory = true,
            TargetRoles = ["Manager", "Director"],
            MaxParticipants = 20,
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/career/v1/training-programs", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TrainingProgramResponse>();
        Assert.NotNull(result);
        Assert.Equal(request.ProgramCode, result!.ProgramCode);
        Assert.Equal(request.ProgramName, result.ProgramName);
        Assert.Equal(request.DurationHours, result.DurationHours);
    }

    [Fact]
    public async Task CreateTrainingProgram_WithoutCreatePermission_ReturnsForbidden()
    {
        // Arrange - Read-only token
        var permissions = new[] { "career.trainings.read" };
        var token = _factory.CreateTestJwtToken("employee-id", new[] { "career-employee" }, permissions);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new CreateTrainingProgramRequest
        {
            ProgramCode = "TEST-2025-001",
            ProgramName = "Test Program",
            Description = "Test Description",
            Category = "Technical",
            DurationHours = 10m,
            Provider = "Internal",
            IsMandatory = false,
            TargetRoles = [],
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/career/v1/training-programs", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTrainingProgram_WithUpdatePermission_ReturnsOk()
    {
        // Arrange
        var programId = await SeedSingleProgramAsync();

        var permissions = new[] { "career.trainings.read", "career.trainings.update" };
        var token = _factory.CreateTestJwtToken("hr-staff-id", new[] { "career-hr" }, permissions);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Get current program to get RowVersion
        var getResponse = await _client.GetAsync($"/career/v1/training-programs/{programId}");

        if (getResponse.StatusCode != HttpStatusCode.OK)
        {
            var content = await getResponse.Content.ReadAsStringAsync();
            throw new Exception($"Failed to get program. Status: {getResponse.StatusCode}, Content: {content}");
        }

        var program = await getResponse.Content.ReadFromJsonAsync<TrainingProgramResponse>();

        var request = new UpdateTrainingProgramRequest
        {
            ProgramName = "Updated Program Name",
            Description = program!.Description,
            Category = program.Category,
            DurationHours = 50m,
            Provider = program.Provider,
            IsMandatory = program.IsMandatory,
            TargetRoles = program.TargetRoles,
            MaxParticipants = program.MaxParticipants,
            IsActive = program.IsActive,
            RowVersion = program.RowVersion
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/career/v1/training-programs/{programId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TrainingProgramResponse>();
        Assert.NotNull(result);
        Assert.Equal("Updated Program Name", result!.ProgramName);
        Assert.Equal(50m, result.DurationHours);
    }

    private async Task SeedTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        // Clear existing data
        dbContext.TrainingPrograms.RemoveRange(dbContext.TrainingPrograms);
        await dbContext.SaveChangesAsync();

        // Add test data
        var programs = new List<TrainingProgram>
        {
            new() {
                Id = Guid.NewGuid(),
                ProgramCode = "LEAD-2025-001",
                ProgramName = "Leadership Fundamentals",
                Description = "Introduction to leadership principles",
                Category = "Leadership",
                DurationHours = 20m,
                Provider = "Internal",
                IsMandatory = true,
                TargetRoles = ["Manager"],
                MaxParticipants = 30,
                IsActive = true,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            },
            new() {
                Id = Guid.NewGuid(),
                ProgramCode = "TECH-2025-001",
                ProgramName = "Cloud Architecture",
                Description = "Advanced cloud architecture patterns",
                Category = "Technical",
                DurationHours = 40m,
                Provider = "LinkedIn Learning",
                ExternalLmsUrl = "https://linkedin.com/learning/cloud-architecture",
                IsMandatory = false,
                TargetRoles = ["Developer", "Architect"],
                MaxParticipants = null,
                IsActive = true,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            },
            new() {
                Id = Guid.NewGuid(),
                ProgramCode = "COMP-2025-001",
                ProgramName = "Data Privacy and GDPR",
                Description = "Compliance training for data protection",
                Category = "Compliance",
                DurationHours = 5m,
                Provider = "Internal",
                IsMandatory = true,
                TargetRoles = [],
                IsActive = true,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            }
        };

        dbContext.TrainingPrograms.AddRange(programs);
        await dbContext.SaveChangesAsync();
    }

    private async Task<Guid> SeedSingleProgramAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        var program = new TrainingProgram
        {
            Id = Guid.NewGuid(),
            ProgramCode = "TEST-2025-999",
            ProgramName = "Test Training Program",
            Description = "Test program for integration tests",
            Category = "Testing",
            DurationHours = 10m,
            Provider = "Internal",
            IsMandatory = false,
            TargetRoles = [],
            IsActive = true,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.TrainingPrograms.Add(program);
        await dbContext.SaveChangesAsync();

        return program.Id;
    }
}
