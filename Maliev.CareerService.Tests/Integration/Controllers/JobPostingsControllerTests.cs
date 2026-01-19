using System.Net;
using System.Net.Http.Json;
using Maliev.CareerService.Api.Models.JobPostings;
using Maliev.CareerService.Data.Models;
using Xunit;

namespace Maliev.CareerService.Tests.Integration.Controllers;

public class JobPostingsControllerTests : BaseIntegrationTest
{
    public JobPostingsControllerTests(CareerServiceFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetJobPostings_ShouldReturnOk()
    {
        // Arrange
        var posting = new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Software Engineer",
            PositionCode = "SE001",
            Description = "Job Description",
            Requirements = "Requirements",
            Responsibilities = "Responsibilities",
            EmploymentType = "Full-time",
            IsActive = true,
            PublishedAt = DateTime.UtcNow.AddDays(-1),
            ApplicationDeadline = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await SeedDatabaseAsync(posting);

        // Act
        var response = await Client.GetAsync("/career/v1/job-postings");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JobPostingListResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task GetJobPosting_ShouldReturnOk_WhenExists()
    {
        // Arrange
        var posting = new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Software Engineer",
            PositionCode = "SE002",
            Description = "Job Description",
            Requirements = "Requirements",
            Responsibilities = "Responsibilities",
            EmploymentType = "Full-time",
            IsActive = true,
            PublishedAt = DateTime.UtcNow.AddDays(-1),
            ApplicationDeadline = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await SeedDatabaseAsync(posting);

        // Act
        var response = await Client.GetAsync($"/career/v1/job-postings/{posting.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JobPostingResponse>();
        Assert.NotNull(result);
        Assert.Equal(posting.Id, result.Id);
    }

    [Fact]
    public async Task GetJobPosting_ShouldReturnNotFound_WhenNotExists()
    {
        // Act
        var response = await Client.GetAsync($"/career/v1/job-postings/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
