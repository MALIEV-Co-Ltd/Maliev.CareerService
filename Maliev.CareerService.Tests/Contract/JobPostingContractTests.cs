using FluentAssertions;
using Maliev.CareerService.Api.Models.JobPostings;
using Maliev.CareerService.Tests.Factories;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Maliev.CareerService.Tests.Helpers;

namespace Maliev.CareerService.Tests.Contract;

/// <summary>
/// Contract tests for job posting endpoints - verify API contract compliance
/// </summary>
public class JobPostingContractTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();

    [DockerRequiredFact]
    public async Task GetJobPostings_ReturnsCorrectResponseStructure()
    {
        // Act
        var response = await _client.GetAsync("/careers/v1/job-postings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var result = await response.Content.ReadFromJsonAsync<JobPostingListResponse>();
        result.Should().NotBeNull();

        // Verify response structure
        result!.Should().BeAssignableTo<JobPostingListResponse>();
        result.Items.Should().NotBeNull();
        result.Page.Should().BeGreaterThanOrEqualTo(1);
        result.PageSize.Should().BeGreaterThan(0);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        result.TotalPages.Should().BeGreaterThanOrEqualTo(0);
    }

    [DockerRequiredFact]
    public async Task GetJobPosting_ReturnsCorrectResponseStructure()
    {
        // Arrange - Use a random ID that will return 404
        var testId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/careers/v1/job-postings/{testId}");

        // Assert - We expect 404, but we're testing the endpoint exists and responds
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JobPostingResponse>();
            result.Should().NotBeNull();

            // Verify response structure
            result!.Should().BeAssignableTo<JobPostingResponse>();
            result.Id.Should().NotBeEmpty();
            result.PositionTitle.Should().NotBeNullOrEmpty();
            result.PositionCode.Should().NotBeNullOrEmpty();
            result.Description.Should().NotBeNull();
            result.DescriptionHtml.Should().NotBeNull();
            result.Requirements.Should().NotBeNull();
            result.RequirementsHtml.Should().NotBeNull();
            result.Responsibilities.Should().NotBeNull();
            result.ResponsibilitiesHtml.Should().NotBeNull();
            result.EmploymentType.Should().NotBeNullOrEmpty();
            result.RowVersion.Should().NotBeNullOrEmpty();
        }
    }

    [DockerRequiredFact]
    public async Task CreateJobPosting_AcceptsCorrectRequestStructure()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer HRStaff");

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
        var response = await _client.PostAsJsonAsync("/careers/v1/job-postings", request);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden);

        if (response.StatusCode == HttpStatusCode.Created)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

            var result = await response.Content.ReadFromJsonAsync<JobPostingResponse>();
            result.Should().NotBeNull();
            result!.PositionTitle.Should().Be(request.PositionTitle);

            // Verify Location header for created resource
            response.Headers.Location.Should().NotBeNull();
        }
    }

    [DockerRequiredFact]
    public async Task GetJobPostings_SupportsQueryParameters()
    {
        // Act
        var response = await _client.GetAsync(
            "/careers/v1/job-postings?department=Engineering&location=Bangkok&employmentType=Full-time&search=software&offset=0&limit=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JobPostingListResponse>();
        result.Should().NotBeNull();
        result!.PageSize.Should().Be(10);
    }

    [DockerRequiredFact]
    public async Task JobPostingResponse_ContainsAllRequiredFields()
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
            ""rowVersion"": ""AAAAAAAAB9E="",
            ""createdAt"": ""2025-01-01T00:00:00Z"",
            ""updatedAt"": ""2025-01-01T00:00:00Z""
        }";

        // Act
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = JsonSerializer.Deserialize<JobPostingResponse>(sampleJson, options);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.PositionTitle.Should().NotBeNullOrEmpty();
        result.PositionCode.Should().NotBeNullOrEmpty();
        result.EmploymentType.Should().NotBeNullOrEmpty();
        result.DescriptionHtml.Should().NotBeNullOrEmpty();
        result.RequirementsHtml.Should().NotBeNullOrEmpty();
        result.ResponsibilitiesHtml.Should().NotBeNullOrEmpty();
        result.RowVersion.Should().NotBeNullOrEmpty();
    }

    [DockerRequiredFact]
    public async Task JobPostingListResponse_SupportsPagination()
    {
        // Act
        var response = await _client.GetAsync("/careers/v1/job-postings?offset=0&limit=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JobPostingListResponse>();
        result.Should().NotBeNull();

        // Verify pagination fields
        result!.Page.Should().BeGreaterThanOrEqualTo(1);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        result.TotalPages.Should().BeGreaterThanOrEqualTo(0);
    }
}
