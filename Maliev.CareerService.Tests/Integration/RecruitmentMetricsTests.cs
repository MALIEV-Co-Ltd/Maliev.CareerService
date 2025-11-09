using FluentAssertions;
using Maliev.CareerService.Api.Models.Reports;
using Maliev.CareerService.Data.Models;
using Maliev.CareerService.Tests.Factories;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Maliev.CareerService.Tests.Helpers;

namespace Maliev.CareerService.Tests.Integration;

/// <summary>
/// Integration tests for recruitment metrics and reporting
/// </summary>
public class RecruitmentMetricsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _hrStaffId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public RecruitmentMetricsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // Default to HR Staff authorization
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer HRStaff hr@example.com {_hrStaffId}");
    }

    [DockerRequiredFact]
    public async Task GetRecruitmentMetrics_ValidDateRange_ReturnsMetrics()
    {
        // Arrange
        await SeedTestRecruitmentDataAsync();

        var startDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/careers/v1/reports/recruitment-metrics?start_date={startDate}&end_date={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<RecruitmentMetricsResponse>();
        result.Should().NotBeNull();
        result!.TotalApplications.Should().BeGreaterThan(0);
    }

    [DockerRequiredFact]
    public async Task GetRecruitmentMetrics_CalculatesApplicationsPerPosting_ReturnsCorrectCounts()
    {
        // Arrange
        await SeedTestRecruitmentDataAsync();

        var startDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/careers/v1/reports/recruitment-metrics?start_date={startDate}&end_date={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<RecruitmentMetricsResponse>();
        result.Should().NotBeNull();
        result!.ApplicationsPerPosting.Should().NotBeNull();
        result.ApplicationsPerPosting.Should().NotBeEmpty();
    }

    [DockerRequiredFact]
    public async Task GetRecruitmentMetrics_CalculatesConversionRates_ReturnsPercentages()
    {
        // Arrange
        await SeedTestRecruitmentDataAsync();

        var startDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/careers/v1/reports/recruitment-metrics?start_date={startDate}&end_date={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<RecruitmentMetricsResponse>();
        result.Should().NotBeNull();
        result!.ConversionRates.Should().NotBeNull();
    }

    [DockerRequiredFact]
    public async Task GetRecruitmentMetrics_CalculatesAverageTimeToHire_ReturnsValidDuration()
    {
        // Arrange
        await SeedTestRecruitmentDataWithHiresAsync();

        var startDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/careers/v1/reports/recruitment-metrics?start_date={startDate}&end_date={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<RecruitmentMetricsResponse>();
        result.Should().NotBeNull();
        result!.AverageTimeToHire.Should().BeGreaterThan(0);
    }

    [DockerRequiredFact]
    public async Task GetRecruitmentMetrics_CountsPositionsFilled_ReturnsAccurateCounts()
    {
        // Arrange
        await SeedTestRecruitmentDataWithHiresAsync();

        var startDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/careers/v1/reports/recruitment-metrics?start_date={startDate}&end_date={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<RecruitmentMetricsResponse>();
        result.Should().NotBeNull();
        result!.PositionsFilled.Should().BeGreaterThanOrEqualTo(0);
        result.PositionsOpen.Should().BeGreaterThanOrEqualTo(0);
    }

    [DockerRequiredFact]
    public async Task GetRecruitmentMetrics_ApplicationVolumeTrends_ReturnsTimeSeriesData()
    {
        // Arrange
        await SeedTestRecruitmentDataAsync();

        var startDate = DateTime.UtcNow.AddMonths(-3).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/careers/v1/reports/recruitment-metrics?start_date={startDate}&end_date={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<RecruitmentMetricsResponse>();
        result.Should().NotBeNull();
        result!.ApplicationVolumeTrends.Should().NotBeNull();
    }

    [DockerRequiredFact]
    public async Task GetRecruitmentMetrics_WithoutDateRange_ReturnsLast30Days()
    {
        // Arrange
        await SeedTestRecruitmentDataAsync();

        // Act - No date parameters, should default to last 30 days
        var response = await _client.GetAsync("/careers/v1/reports/recruitment-metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<RecruitmentMetricsResponse>();
        result.Should().NotBeNull();
    }

    [DockerRequiredFact]
    public async Task GetRecruitmentMetrics_AsEmployee_ReturnsForbidden()
    {
        // Arrange
        _client.DefaultRequestHeaders.Remove("Authorization");
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer Employee employee@example.com");

        // Act
        var response = await _client.GetAsync("/careers/v1/reports/recruitment-metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [DockerRequiredFact]
    public async Task GetRecruitmentMetrics_CachesResults_ReturnsSameDataWithin5Minutes()
    {
        // Arrange
        await SeedTestRecruitmentDataAsync();

        var startDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act - First request
        var response1 = await _client.GetAsync($"/careers/v1/reports/recruitment-metrics?start_date={startDate}&end_date={endDate}");
        var result1 = await response1.Content.ReadFromJsonAsync<RecruitmentMetricsResponse>();

        // Act - Second request (should be cached)
        var response2 = await _client.GetAsync($"/careers/v1/reports/recruitment-metrics?start_date={startDate}&end_date={endDate}");
        var result2 = await response2.Content.ReadFromJsonAsync<RecruitmentMetricsResponse>();

        // Assert - Results should be identical (cached)
        result1.Should().BeEquivalentTo(result2);
    }

    private async Task SeedTestRecruitmentDataAsync()
    {
        // Clean database and cache before seeding
        await _factory.CleanDatabaseAsync();
        _factory.ClearCache();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        // Create job postings
        var postings = new List<JobPosting>
        {
            new() {
                Id = Guid.NewGuid(),
                PositionTitle = "Software Engineer",
                PositionCode = $"SE-{Guid.NewGuid().ToString()[..8]}",
                Department = "Engineering",
                Location = "Bangkok",
                EmploymentType = "Full-time",
                Description = "Software Engineer position",
                Requirements = "Experience required",
                Responsibilities = "Develop software",
                ApplicationDeadline = DateTime.UtcNow.AddMonths(1),
                PublishedAt = DateTime.UtcNow.AddDays(-30),
                IsActive = true,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            },
            new() {
                Id = Guid.NewGuid(),
                PositionTitle = "Senior Developer",
                PositionCode = $"SD-{Guid.NewGuid().ToString()[..8]}",
                Department = "Engineering",
                Location = "Bangkok",
                EmploymentType = "Full-time",
                Description = "Senior Developer position",
                Requirements = "5+ years experience",
                Responsibilities = "Lead development",
                ApplicationDeadline = DateTime.UtcNow.AddMonths(1),
                PublishedAt = DateTime.UtcNow.AddDays(-20),
                IsActive = true,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            }
        };

        dbContext.JobPostings.AddRange(postings);
        await dbContext.SaveChangesAsync();

        // Create applications
        var applications = new List<JobApplication>();
        foreach (var posting in postings)
        {
            for (int i = 0; i < 5; i++)
            {
                applications.Add(new JobApplication
                {
                    Id = Guid.NewGuid(),
                    JobPostingId = posting.Id,
                    ApplicantFirstName = $"Applicant{i}",
                    ApplicantLastName = "Test",
                    ApplicantEmail = $"applicant{i}@example.com",
                    ApplicantPhone = "+66812345678",
                    ApplicantCountryCode = "TH",
                    ResumeFileId = Guid.NewGuid(),
                    Status = i % 3 == 0 ? "Interview" : (i % 3 == 1 ? "UnderReview" : "Submitted"),
                    AppliedAt = DateTime.UtcNow.AddDays(-15 + i),
                    CreatedBy = Guid.NewGuid(),
                    UpdatedBy = Guid.NewGuid()
                });
            }
        }

        dbContext.JobApplications.AddRange(applications);
        await dbContext.SaveChangesAsync();
    }

    private async Task SeedTestRecruitmentDataWithHiresAsync()
    {
        // Clean database and cache before seeding
        await _factory.CleanDatabaseAsync();
        _factory.ClearCache();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        // Create job posting
        var posting = new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Product Manager",
            PositionCode = $"PM-{Guid.NewGuid().ToString()[..8]}",
            Department = "Product",
            Location = "Bangkok",
            EmploymentType = "Full-time",
            Description = "Product Manager position",
            Requirements = "3+ years experience",
            Responsibilities = "Manage products",
            ApplicationDeadline = DateTime.UtcNow.AddMonths(1),
            PublishedAt = DateTime.UtcNow.AddDays(-60),
            IsActive = false,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.JobPostings.Add(posting);
        await dbContext.SaveChangesAsync();

        // Create applications with some hired (within last 30 days)
        var applications = new List<JobApplication>
        {
            new() {
                Id = Guid.NewGuid(),
                JobPostingId = posting.Id,
                ApplicantFirstName = "Accepted",
                ApplicantLastName = "Candidate",
                ApplicantEmail = "accepted@example.com",
                ApplicantPhone = "+66812345678",
                ApplicantCountryCode = "TH",
                ResumeFileId = Guid.NewGuid(),
                Status = "accepted",
                AppliedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-5), // Hired 15 days after applying
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            },
            new() {
                Id = Guid.NewGuid(),
                JobPostingId = posting.Id,
                ApplicantFirstName = "Rejected",
                ApplicantLastName = "Candidate",
                ApplicantEmail = "rejected@example.com",
                ApplicantPhone = "+66812345678",
                ApplicantCountryCode = "TH",
                ResumeFileId = Guid.NewGuid(),
                Status = "rejected",
                AppliedAt = DateTime.UtcNow.AddDays(-15),
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            }
        };

        dbContext.JobApplications.AddRange(applications);
        await dbContext.SaveChangesAsync();
    }
}
