using Maliev.CareerService.Api.Models.Reports;
using Maliev.CareerService.Api.Authentication;
using Maliev.CareerService.Data.Models;
using Maliev.CareerService.Tests.Factories;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Maliev.CareerService.Tests.Integration;

/// <summary>
/// Integration tests for learning and training metrics reporting
/// </summary>
public class LearningMetricsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _hrStaffId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public LearningMetricsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // Default to HR Staff authorization
        var permissions = CareerPredefinedRoles.GetPermissions(CareerPredefinedRoles.HR);
        var token = _factory.CreateTestJwtToken(_hrStaffId.ToString(), new[] { CareerPredefinedRoles.HR }, permissions);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task GetLearningMetrics_ValidDateRange_ReturnsMetrics()
    {
        // Arrange
        await SeedTestLearningDataAsync();

        var startDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/career/v1/reports/learning-metrics?start_date={startDate}&end_date={endDate}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LearningMetricsResponse>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetLearningMetrics_CalculatesEnrollmentRates_ReturnsPercentages()
    {
        // Arrange
        await SeedTestLearningDataAsync();

        var startDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/career/v1/reports/learning-metrics?start_date={startDate}&end_date={endDate}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LearningMetricsResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result!.EnrollmentRates);
    }

    [Fact]
    public async Task GetLearningMetrics_CalculatesCompletionRates_ReturnsAccuratePercentages()
    {
        // Arrange
        await SeedTestLearningDataWithCompletionsAsync();

        var startDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/career/v1/reports/learning-metrics?start_date={startDate}&end_date={endDate}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LearningMetricsResponse>();
        Assert.NotNull(result);
        Assert.True(result!.CompletionRates >= 0);
        Assert.True(result.CompletionRates <= 100);
    }

    [Fact]
    public async Task GetLearningMetrics_CalculatesAverageTimeToComplete_ReturnsValidDuration()
    {
        // Arrange
        await SeedTestLearningDataWithCompletionsAsync();

        var startDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/career/v1/reports/learning-metrics?start_date={startDate}&end_date={endDate}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LearningMetricsResponse>();
        Assert.NotNull(result);
        Assert.True(result!.TimeToComplete >= 0);
    }

    [Fact]
    public async Task GetLearningMetrics_IdentifiesPopularPrograms_ReturnsTopPrograms()
    {
        // Arrange
        await SeedTestLearningDataAsync();

        var startDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/career/v1/reports/learning-metrics?start_date={startDate}&end_date={endDate}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LearningMetricsResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result!.PopularPrograms);
        Assert.NotEmpty(result.PopularPrograms);
    }

    [Fact]
    public async Task GetLearningMetrics_CalculatesCertificationRates_ReturnsPercentages()
    {
        // Arrange
        await SeedTestLearningDataWithCertificationsAsync();

        var startDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/career/v1/reports/learning-metrics?start_date={startDate}&end_date={endDate}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LearningMetricsResponse>();
        Assert.NotNull(result);
        Assert.True(result!.CertificationRates >= 0);
    }

    [Fact]
    public async Task GetLearningMetrics_TracksIDPAdoption_ReturnsAdoptionMetrics()
    {
        // Arrange
        await SeedTestLearningDataAsync();

        var startDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/career/v1/reports/learning-metrics?start_date={startDate}&end_date={endDate}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LearningMetricsResponse>();
        Assert.NotNull(result);
        Assert.True(result!.IDPAdoption >= 0);
    }

    [Fact]
    public async Task GetLearningMetrics_WithoutDateRange_ReturnsLast30Days()
    {
        // Arrange
        await SeedTestLearningDataAsync();

        // Act - No date parameters, should default to last 30 days
        var response = await _client.GetAsync("/career/v1/reports/learning-metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LearningMetricsResponse>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetLearningMetrics_AsEmployee_ReturnsForbidden()
    {
        // Arrange
        _client.DefaultRequestHeaders.Remove("Authorization");
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _factory.CreateTestJwtToken("employee-id", new[] { "Employee" }));

        // Act
        var response = await _client.GetAsync("/career/v1/reports/learning-metrics");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetLearningMetrics_CachesResults_ReturnsSameDataWithin5Minutes()
    {
        // Arrange
        await SeedTestLearningDataAsync();

        var startDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act - First request
        var response1 = await _client.GetAsync($"/career/v1/reports/learning-metrics?start_date={startDate}&end_date={endDate}");
        var result1 = await response1.Content.ReadFromJsonAsync<LearningMetricsResponse>();

        // Act - Second request (should be cached)
        var response2 = await _client.GetAsync($"/career/v1/reports/learning-metrics?start_date={startDate}&end_date={endDate}");
        var result2 = await response2.Content.ReadFromJsonAsync<LearningMetricsResponse>();

        // Assert - Results should be identical (cached)
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1.EnrollmentRates, result2.EnrollmentRates);
        Assert.Equal(result1.CompletionRates, result2.CompletionRates);
        Assert.Equal(result1.TimeToComplete, result2.TimeToComplete);
        Assert.Equal(result1.CertificationRates, result2.CertificationRates);
        Assert.Equal(result1.IDPAdoption, result2.IDPAdoption);
    }

    private async Task SeedTestLearningDataAsync()
    {
        // Clean database and cache before seeding
        await _factory.CleanDatabaseAsync();
        _factory.ClearCache();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        // Create training programs
        var programs = new List<TrainingProgram>
        {
            new() {
                Id = Guid.NewGuid(),
                ProgramCode = $"LEAD-{Guid.NewGuid().ToString()[..8]}",
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
                ProgramCode = $"TECH-{Guid.NewGuid().ToString()[..8]}",
                ProgramName = "Cloud Architecture",
                Description = "Advanced cloud architecture patterns",
                Category = "Technical",
                DurationHours = 40m,
                Provider = "LinkedIn Learning",
                IsMandatory = false,
                TargetRoles = ["Developer", "Architect"],
                IsActive = true,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            },
            new() {
                Id = Guid.NewGuid(),
                ProgramCode = $"COMP-{Guid.NewGuid().ToString()[..8]}",
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

        // Create enrollments
        var enrollments = new List<EmployeeTrainingEnrollment>();
        foreach (var program in programs)
        {
            for (int i = 0; i < 10; i++)
            {
                enrollments.Add(new EmployeeTrainingEnrollment
                {
                    Id = Guid.NewGuid(),
                    TrainingProgramId = program.Id,
                    EmployeeId = Guid.NewGuid(),
                    EnrolledAt = DateTime.UtcNow.AddDays(-15 + i),
                    EnrollmentType = EnrollmentType.Voluntary,
                    Status = TrainingEnrollmentStatus.Enrolled,
                    CreatedBy = Guid.NewGuid(),
                    UpdatedBy = Guid.NewGuid()
                });
            }
        }

        dbContext.EmployeeTrainingEnrollments.AddRange(enrollments);
        await dbContext.SaveChangesAsync();
    }

    private async Task SeedTestLearningDataWithCompletionsAsync()
    {
        // Clean database and cache before seeding
        await _factory.CleanDatabaseAsync();
        _factory.ClearCache();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        // Create training program
        var program = new TrainingProgram
        {
            Id = Guid.NewGuid(),
            ProgramCode = $"TEST-{Guid.NewGuid().ToString()[..8]}",
            ProgramName = "Test Program with Completions",
            Description = "Test program for completion metrics",
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

        // Create enrollments with some completed
        var enrollments = new List<EmployeeTrainingEnrollment>
        {
            new() {
                Id = Guid.NewGuid(),
                TrainingProgramId = program.Id,
                EmployeeId = Guid.NewGuid(),
                EnrolledAt = DateTime.UtcNow.AddDays(-30),
                StartedAt = DateTime.UtcNow.AddDays(-28),
                CompletedAt = DateTime.UtcNow.AddDays(-20),
                EnrollmentType = EnrollmentType.Voluntary,
                Status = TrainingEnrollmentStatus.Completed,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            },
            new() {
                Id = Guid.NewGuid(),
                TrainingProgramId = program.Id,
                EmployeeId = Guid.NewGuid(),
                EnrolledAt = DateTime.UtcNow.AddDays(-25),
                StartedAt = DateTime.UtcNow.AddDays(-23),
                CompletedAt = DateTime.UtcNow.AddDays(-15),
                EnrollmentType = EnrollmentType.Voluntary,
                Status = TrainingEnrollmentStatus.Completed,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            },
            new() {
                Id = Guid.NewGuid(),
                TrainingProgramId = program.Id,
                EmployeeId = Guid.NewGuid(),
                EnrolledAt = DateTime.UtcNow.AddDays(-10),
                StartedAt = DateTime.UtcNow.AddDays(-8),
                EnrollmentType = EnrollmentType.Voluntary,
                Status = TrainingEnrollmentStatus.InProgress,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            }
        };

        dbContext.EmployeeTrainingEnrollments.AddRange(enrollments);
        await dbContext.SaveChangesAsync();
    }

    private async Task SeedTestLearningDataWithCertificationsAsync()
    {
        // Clean database and cache before seeding
        await _factory.CleanDatabaseAsync();
        _factory.ClearCache();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        // Create certification program
        var program = new TrainingProgram
        {
            Id = Guid.NewGuid(),
            ProgramCode = $"CERT-{Guid.NewGuid().ToString()[..8]}",
            ProgramName = "AWS Certified Solutions Architect",
            Description = "AWS certification preparation",
            Category = "Technical",
            DurationHours = 60m,
            Provider = "AWS",
            IsMandatory = false,
            TargetRoles = ["Architect", "Senior Developer"],
            IsActive = true,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.TrainingPrograms.Add(program);
        await dbContext.SaveChangesAsync();

        // Create enrollments with certifications
        var enrollments = new List<EmployeeTrainingEnrollment>
        {
            new() {
                Id = Guid.NewGuid(),
                TrainingProgramId = program.Id,
                EmployeeId = Guid.NewGuid(),
                EnrolledAt = DateTime.UtcNow.AddDays(-60),
                StartedAt = DateTime.UtcNow.AddDays(-58),
                CompletedAt = DateTime.UtcNow.AddDays(-10),
                EnrollmentType = EnrollmentType.Mandatory,
                Status = TrainingEnrollmentStatus.Completed,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            },
            new() {
                Id = Guid.NewGuid(),
                TrainingProgramId = program.Id,
                EmployeeId = Guid.NewGuid(),
                EnrolledAt = DateTime.UtcNow.AddDays(-50),
                StartedAt = DateTime.UtcNow.AddDays(-48),
                CompletedAt = DateTime.UtcNow.AddDays(-8),
                EnrollmentType = EnrollmentType.Mandatory,
                Status = TrainingEnrollmentStatus.Completed,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            }
        };

        dbContext.EmployeeTrainingEnrollments.AddRange(enrollments);
        await dbContext.SaveChangesAsync();
    }
}
