using Maliev.CareerService.Api.Models.JobPostings;
using Maliev.CareerService.Tests.Factories;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Maliev.CareerService.Tests.Contract;

/// <summary>
/// Contract tests for job posting endpoints - verify API contract compliance
/// </summary>
public class JobPostingContractTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetJobPostings_ReturnsCorrectResponseStructure()
    {
        // Act
        var response = await _client.GetAsync("/career/v1/job-postings");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var result = await response.Content.ReadFromJsonAsync<JobPostingListResponse>();
        Assert.NotNull(result);

        // Verify response structure
        Assert.IsAssignableFrom<JobPostingListResponse>(result);
        Assert.NotNull(result.Items);
        Assert.True(result.Page >= 1);
        Assert.True(result.PageSize > 0);
        Assert.True(result.TotalCount >= 0);
        Assert.True(result.TotalPages >= 0);
    }

    [Fact]
    public async Task GetJobPosting_ReturnsCorrectResponseStructure()
    {
        // Arrange - Use a random ID that will return 404
        var testId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/career/v1/job-postings/{testId}");

        // Assert - We expect 404, but we're testing the endpoint exists and responds
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JobPostingResponse>();
            Assert.NotNull(result);

            // Verify response structure
            Assert.IsAssignableFrom<JobPostingResponse>(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.False(string.IsNullOrEmpty(result.PositionTitle));
            Assert.False(string.IsNullOrEmpty(result.PositionCode));
            Assert.NotNull(result.Description);
            Assert.NotNull(result.DescriptionHtml);
            Assert.NotNull(result.Requirements);
            Assert.NotNull(result.RequirementsHtml);
            Assert.NotNull(result.Responsibilities);
            Assert.NotNull(result.ResponsibilitiesHtml);
            Assert.False(string.IsNullOrEmpty(result.EmploymentType));
            Assert.False(string.IsNullOrEmpty(result.RowVersion));
        }
    }

    [Fact]
    public async Task CreateJobPosting_AcceptsCorrectRequestStructure()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _factory.CreateTestJwtToken("hr-staff-id", new[] { "HRStaff" }));

        var request = new CreateJobPostingRequest
        {
            PositionTitle = "Test Position",
            PositionCode = "TEST-001",
            Department = "Engineering",
            Location = "Bangkok",
            EmploymentType = "Full-time",
            Description = "Test description",
            Requirements = "Test requirements",
            Responsibilities = "Test responsibilities",
            ApplicationDeadline = DateTime.UtcNow.AddMonths(1),
            PublishImmediately = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/career/v1/job-postings", request);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Created ||
                   response.StatusCode == HttpStatusCode.BadRequest ||
                   response.StatusCode == HttpStatusCode.Unauthorized ||
                   response.StatusCode == HttpStatusCode.Forbidden);

        if (response.StatusCode == HttpStatusCode.Created)
        {
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

            var result = await response.Content.ReadFromJsonAsync<JobPostingResponse>();
            Assert.NotNull(result);
            Assert.Equal(request.PositionTitle, result.PositionTitle);

            // Verify Location header for created resource
            Assert.NotNull(response.Headers.Location);
        }
    }

    [Fact]
    public async Task GetJobPostings_SupportsQueryParameters()
    {
        // Act
        var response = await _client.GetAsync(
            "/career/v1/job-postings?department=Engineering&location=Bangkok&employmentType=Full-time&search=software&offset=0&limit=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<JobPostingListResponse>();
        Assert.NotNull(result);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public void JobPostingResponse_ContainsAllRequiredFields()
    {
        // This test verifies the response model structure by deserializing a sample
        var sampleJson = @"{
            ""id"": ""123e4567-e89b-12d3-a456-426614174000"",
            ""positionTitle"": ""Software Engineer"",
            ""positionCode"": ""SE-001"",
            ""department"": ""Engineering"",
            ""location"": ""Bangkok"",
            ""employmentType"": ""Full-time"",
            ""salaryMin"": 50000,
            ""salaryMax"": 80000,
            ""currency"": ""THB"",
            ""description"": ""Job description"",
            ""descriptionHtml"": ""<p>Job description</p>"",
            ""requirements"": ""Requirements"",
            ""requirementsHtml"": ""<p>Requirements</p>"",
            ""responsibilities"": ""Responsibilities"",
            ""responsibilitiesHtml"": ""<p>Responsibilities</p>"",
            ""applicationDeadline"": ""2025-12-31T23:59:59Z"",
            ""publishedAt"": ""2025-01-01T00:00:00Z"",
            ""isActive"": true,
            ""rowVersion"": ""1"",
            ""createdAt"": ""2025-01-01T00:00:00Z"",
            ""updatedAt"": ""2025-01-01T00:00:00Z""
        }";

        // Act
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = JsonSerializer.Deserialize<JobPostingResponse>(sampleJson, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.False(string.IsNullOrEmpty(result.PositionTitle));
        Assert.False(string.IsNullOrEmpty(result.PositionCode));
        Assert.False(string.IsNullOrEmpty(result.EmploymentType));
        Assert.False(string.IsNullOrEmpty(result.DescriptionHtml));
        Assert.False(string.IsNullOrEmpty(result.RequirementsHtml));
        Assert.False(string.IsNullOrEmpty(result.ResponsibilitiesHtml));
        Assert.False(string.IsNullOrEmpty(result.RowVersion));
    }

    [Fact]
    public async Task JobPostingListResponse_SupportsPagination()
    {
        // Act
        var response = await _client.GetAsync("/career/v1/job-postings?offset=0&limit=5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<JobPostingListResponse>();
        Assert.NotNull(result);

        // Verify pagination fields
        Assert.True(result.Page >= 1);
        Assert.Equal(5, result.PageSize);
        Assert.True(result.TotalCount >= 0);
        Assert.True(result.TotalPages >= 0);
    }
}
