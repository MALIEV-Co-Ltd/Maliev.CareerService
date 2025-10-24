using FluentAssertions;
using Maliev.CareerService.Api.Models.JobPostings;
using Maliev.CareerService.Data.Models;
using Maliev.CareerService.Tests.Factories;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Maliev.CareerService.Tests.Helpers;

namespace Maliev.CareerService.Tests.Integration;

/// <summary>
/// Integration tests for JobPostingsController
/// </summary>
public class JobPostingControllerTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();

    [DockerRequiredFact]
    public async Task GetJobPostings_WithoutFilters_ReturnsActivePostings()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/careers/v1/job-postings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JobPostingListResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThan(0);
        result.Items.Should().OnlyContain(p => p.IsActive);
    }

    [DockerRequiredFact]
    public async Task GetJobPostings_WithDepartmentFilter_ReturnsFilteredPostings()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/careers/v1/job-postings?department=Engineering");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JobPostingListResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().OnlyContain(p => p.Department == "Engineering");
    }

    [DockerRequiredFact]
    public async Task GetJobPostings_WithLocationFilter_ReturnsFilteredPostings()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/careers/v1/job-postings?location=Bangkok");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JobPostingListResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().OnlyContain(p => p.Location == "Bangkok");
    }

    [DockerRequiredFact]
    public async Task GetJobPostings_WithEmploymentTypeFilter_ReturnsFilteredPostings()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/careers/v1/job-postings?employmentType=Full-time");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JobPostingListResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().OnlyContain(p => p.EmploymentType == "Full-time");
    }

    [DockerRequiredFact]
    public async Task GetJobPostings_WithSearchKeyword_ReturnsMatchingPostings()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/careers/v1/job-postings?search=Software");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JobPostingListResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [DockerRequiredFact]
    public async Task GetJobPostings_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/careers/v1/job-postings?offset=0&limit=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JobPostingListResponse>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().BeLessThanOrEqualTo(2);
        result.PageSize.Should().Be(2);
    }

    [DockerRequiredFact]
    public async Task GetJobPostings_WithInvalidLimit_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/careers/v1/job-postings?limit=200");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [DockerRequiredFact]
    public async Task GetJobPosting_WithValidId_ReturnsPosting()
    {
        // Arrange
        var postingId = await SeedSinglePostingAsync();

        // Act
        var response = await _client.GetAsync($"/careers/v1/job-postings/{postingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JobPostingResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(postingId);
        result.DescriptionHtml.Should().NotBeNullOrEmpty();
        result.RequirementsHtml.Should().NotBeNullOrEmpty();
        result.ResponsibilitiesHtml.Should().NotBeNullOrEmpty();
    }

    [DockerRequiredFact]
    public async Task GetJobPosting_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/careers/v1/job-postings/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [DockerRequiredFact]
    public async Task CreateJobPosting_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer admin HRStaff");

        var request = new CreateJobPostingRequest
        {
            PositionTitle = "Senior Software Engineer",
            PositionCode = "SSE-2025-001",
            Department = "Engineering",
            Location = "Bangkok",
            EmploymentType = "Full-time",
            SalaryMin = 50000,
            SalaryMax = 80000,
            Currency = "THB",
            Description = "# Job Description\n\nWe are looking for a senior software engineer.",
            Requirements = "# Requirements\n\n- 5+ years experience",
            Responsibilities = "# Responsibilities\n\n- Lead development",
            ApplicationDeadline = DateTime.UtcNow.AddMonths(1),
            PublishImmediately = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/careers/v1/job-postings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<JobPostingResponse>();
        result.Should().NotBeNull();
        result!.PositionTitle.Should().Be(request.PositionTitle);
        result.PositionCode.Should().Be(request.PositionCode);
    }

    [DockerRequiredFact]
    public async Task UpdateJobPosting_WithValidRequest_ReturnsOk()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer admin HRStaff");
        var postingId = await SeedSinglePostingAsync();

        // Get current posting for RowVersion
        var getResponse = await _client.GetAsync($"/careers/v1/job-postings/{postingId}");
        var currentPosting = await getResponse.Content.ReadFromJsonAsync<JobPostingResponse>();

        var request = new UpdateJobPostingRequest
        {
            PositionTitle = "Updated Position Title",
            Department = "Engineering",
            Location = "Bangkok",
            EmploymentType = "Full-time",
            Description = "Updated description",
            Requirements = "Updated requirements",
            Responsibilities = "Updated responsibilities",
            ApplicationDeadline = DateTime.UtcNow.AddMonths(2),
            RowVersion = currentPosting!.RowVersion
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/careers/v1/job-postings/{postingId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JobPostingResponse>();
        result.Should().NotBeNull();
        result!.PositionTitle.Should().Be(request.PositionTitle);
    }

    [DockerRequiredFact]
    public async Task DeleteJobPosting_WithValidId_ReturnsNoContent()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer admin HRStaff");
        var postingId = await SeedSinglePostingAsync();

        // Act
        var response = await _client.DeleteAsync($"/careers/v1/job-postings/{postingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task SeedTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        // Clear existing data
        dbContext.JobPostings.RemoveRange(dbContext.JobPostings);
        await dbContext.SaveChangesAsync();

        // Add test data
        var postings = new List<JobPosting>
        {
            new() {
                Id = Guid.NewGuid(),
                PositionTitle = "Software Engineer",
                PositionCode = "SE-2025-001",
                Department = "Engineering",
                Location = "Bangkok",
                EmploymentType = "Full-time",
                Description = "Software Engineer position",
                Requirements = "- 3+ years experience",
                Responsibilities = "- Develop software",
                ApplicationDeadline = DateTime.UtcNow.AddMonths(1),
                PublishedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            },
            new() {
                Id = Guid.NewGuid(),
                PositionTitle = "Product Manager",
                PositionCode = "PM-2025-001",
                Department = "Product",
                Location = "Remote",
                EmploymentType = "Full-time",
                Description = "Product Manager position",
                Requirements = "- 5+ years experience",
                Responsibilities = "- Manage products",
                ApplicationDeadline = DateTime.UtcNow.AddMonths(1),
                PublishedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            }
        };

        dbContext.JobPostings.AddRange(postings);
        await dbContext.SaveChangesAsync();
    }

    private async Task<Guid> SeedSinglePostingAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        var posting = new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Test Position",
            PositionCode = "TEST-2025-001",
            Department = "Engineering",
            Location = "Bangkok",
            EmploymentType = "Full-time",
            Description = "# Test Description",
            Requirements = "# Test Requirements",
            Responsibilities = "# Test Responsibilities",
            ApplicationDeadline = DateTime.UtcNow.AddMonths(1),
            PublishedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.JobPostings.Add(posting);
        await dbContext.SaveChangesAsync();

        return posting.Id;
    }
}
