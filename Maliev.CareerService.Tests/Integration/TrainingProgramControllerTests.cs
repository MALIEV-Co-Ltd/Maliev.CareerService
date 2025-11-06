using FluentAssertions;
using Maliev.CareerService.Api.Models.TrainingPrograms;
using Maliev.CareerService.Data.Models;
using Maliev.CareerService.Tests.Factories;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Maliev.CareerService.Tests.Helpers;

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

        // Add Employee authorization header
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer Employee employee@example.com");
    }

    [DockerRequiredFact]
    public async Task GetTrainingPrograms_WithoutFilters_ReturnsActivePrograms()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/careers/v1/training-programs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<TrainingProgramListResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThan(0);
        result.Items.Should().OnlyContain(p => p.IsActive);
    }

    [DockerRequiredFact]
    public async Task GetTrainingPrograms_WithCategoryFilter_ReturnsFilteredPrograms()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/careers/v1/training-programs?category=Leadership");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<TrainingProgramListResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().OnlyContain(p => p.Category == "Leadership");
    }

    [DockerRequiredFact]
    public async Task GetTrainingPrograms_WithMandatoryFilter_ReturnsFilteredPrograms()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/careers/v1/training-programs?isMandatory=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<TrainingProgramListResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().OnlyContain(p => p.IsMandatory);
    }

    [DockerRequiredFact]
    public async Task GetTrainingPrograms_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/careers/v1/training-programs?offset=0&limit=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<TrainingProgramListResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCountLessThanOrEqualTo(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
    }

    [DockerRequiredFact]
    public async Task GetTrainingProgramById_ExistingId_ReturnsProgram()
    {
        // Arrange
        var programId = await SeedSingleProgramAsync();

        // Act
        var response = await _client.GetAsync($"/careers/v1/training-programs/{programId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<TrainingProgramResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(programId);
        result.ProgramName.Should().NotBeNullOrEmpty();
        result.DurationHours.Should().BeGreaterThan(0);
    }

    [DockerRequiredFact]
    public async Task GetTrainingProgramById_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/careers/v1/training-programs/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [DockerRequiredFact]
    public async Task CreateTrainingProgram_AsHRStaff_ReturnsCreated()
    {
        // Arrange
        _client.DefaultRequestHeaders.Remove("Authorization");
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer HRStaff hr@example.com");

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
        var response = await _client.PostAsJsonAsync("/careers/v1/training-programs", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<TrainingProgramResponse>();
        result.Should().NotBeNull();
        result!.ProgramCode.Should().Be(request.ProgramCode);
        result.ProgramName.Should().Be(request.ProgramName);
        result.DurationHours.Should().Be(request.DurationHours);
    }

    [DockerRequiredFact]
    public async Task CreateTrainingProgram_AsEmployee_ReturnsForbidden()
    {
        // Arrange - Already set to Employee in constructor
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
        var response = await _client.PostAsJsonAsync("/careers/v1/training-programs", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [DockerRequiredFact]
    public async Task UpdateTrainingProgram_AsHRStaff_ReturnsOk()
    {
        // Arrange
        var programId = await SeedSingleProgramAsync();

        _client.DefaultRequestHeaders.Remove("Authorization");
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer HRStaff hr@example.com");

        // Get current program to get RowVersion
        var getResponse = await _client.GetAsync($"/careers/v1/training-programs/{programId}");
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
        var response = await _client.PutAsJsonAsync($"/careers/v1/training-programs/{programId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<TrainingProgramResponse>();
        result.Should().NotBeNull();
        result!.ProgramName.Should().Be("Updated Program Name");
        result.DurationHours.Should().Be(50m);
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
