using CareerDbContext = Maliev.CareerService.Infrastructure.Data.CareerDbContext;
using Maliev.CareerService.Api.Models.JobPostings;
using Maliev.CareerService.Api.Authentication;
using Maliev.CareerService.Domain.Entities;
using Maliev.CareerService.Tests.Factories;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Maliev.CareerService.Tests.Integration;

/// <summary>
/// Integration tests for JobPostingsController
/// </summary>
public class JobPostingControllerTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetJobPostings_WithoutFilters_ReturnsActivePostings()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/career/v1/job-postings");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<JobPostingListResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result!.Items);
        Assert.True(result.TotalCount > 0);
        Assert.All(result.Items, p => Assert.True(p.IsActive));
    }

    [Fact]
    public async Task GetJobPostings_WithDepartmentFilter_ReturnsFilteredPostings()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/career/v1/job-postings?department=Engineering");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<JobPostingListResponse>();
        Assert.NotNull(result);
        Assert.All(result!.Items, p => Assert.Equal("Engineering", p.Department));
    }

    [Fact]
    public async Task GetJobPostings_WithLocationFilter_ReturnsFilteredPostings()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/career/v1/job-postings?location=Bangkok");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<JobPostingListResponse>();
        Assert.NotNull(result);
        Assert.All(result!.Items, p => Assert.Equal("Bangkok", p.Location));
    }

    [Fact]
    public async Task GetJobPostings_WithEmploymentTypeFilter_ReturnsFilteredPostings()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/career/v1/job-postings?employmentType=Full-time");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<JobPostingListResponse>();
        Assert.NotNull(result);
        Assert.All(result!.Items, p => Assert.Equal("Full-time", p.EmploymentType));
    }

    [Fact]
    public async Task GetJobPostings_WithSearchKeyword_ReturnsMatchingPostings()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/career/v1/job-postings?search=Software");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<JobPostingListResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result!.Items);
    }

    [Fact]
    public async Task GetJobPostings_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/career/v1/job-postings?offset=0&limit=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<JobPostingListResponse>();
        Assert.NotNull(result);
        Assert.True(result!.Items.Count <= 2);
        Assert.Equal(2, result.PageSize);
    }

    [Fact]
    public async Task GetJobPostings_WithInvalidLimit_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/career/v1/job-postings?limit=200");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetJobPosting_WithValidId_ReturnsPosting()
    {
        // Arrange
        var postingId = await SeedSinglePostingAsync();

        // Act
        var response = await _client.GetAsync($"/career/v1/job-postings/{postingId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<JobPostingResponse>();
        Assert.NotNull(result);
        Assert.Equal(postingId, result!.Id);
        Assert.False(string.IsNullOrEmpty(result.DescriptionHtml));
        Assert.False(string.IsNullOrEmpty(result.RequirementsHtml));
        Assert.False(string.IsNullOrEmpty(result.ResponsibilitiesHtml));
    }

    [Fact]
    public async Task GetJobPosting_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/career/v1/job-postings/{invalidId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateJobPosting_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        var permissions = CareerPredefinedRoles.GetPermissions(CareerPredefinedRoles.HR);
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _factory.CreateTestJwtToken("hr-staff-id", new[] { CareerPredefinedRoles.HR }, permissions));

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
        var response = await _client.PostAsJsonAsync("/career/v1/job-postings", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<JobPostingResponse>();
        Assert.NotNull(result);
        Assert.Equal(request.PositionTitle, result!.PositionTitle);
        Assert.Equal(request.PositionCode, result.PositionCode);
    }

    [Fact]
    public async Task UpdateJobPosting_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var permissions = CareerPredefinedRoles.GetPermissions(CareerPredefinedRoles.HR);
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _factory.CreateTestJwtToken("hr-staff-id", new[] { CareerPredefinedRoles.HR }, permissions));
        var postingId = await SeedSinglePostingAsync();

        // Get current posting for RowVersion
        var getResponse = await _client.GetAsync($"/career/v1/job-postings/{postingId}");
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
        var response = await _client.PutAsJsonAsync($"/career/v1/job-postings/{postingId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<JobPostingResponse>();
        Assert.NotNull(result);
        Assert.Equal(request.PositionTitle, result!.PositionTitle);
    }

    [Fact]
    public async Task DeleteJobPosting_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var permissions = CareerPredefinedRoles.GetPermissions(CareerPredefinedRoles.HR);
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _factory.CreateTestJwtToken("hr-staff-id", new[] { CareerPredefinedRoles.HR }, permissions));
        var postingId = await SeedSinglePostingAsync();

        // Act
        var response = await _client.DeleteAsync($"/career/v1/job-postings/{postingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task SeedTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CareerDbContext>();

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
        // Clean database before seeding to ensure test isolation
        await _factory.CleanDatabaseAsync();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CareerDbContext>();

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
            UpdatedBy = Guid.NewGuid(),
        };

        dbContext.JobPostings.Add(posting);
        await dbContext.SaveChangesAsync();

        return posting.Id;
    }
}
