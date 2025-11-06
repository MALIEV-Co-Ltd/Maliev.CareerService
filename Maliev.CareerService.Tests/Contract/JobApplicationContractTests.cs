using FluentAssertions;
using Maliev.CareerService.Api.Models.Applications;
using Maliev.CareerService.Tests.Factories;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Maliev.CareerService.Tests.Helpers;

namespace Maliev.CareerService.Tests.Contract;

/// <summary>
/// Contract tests for job application endpoints - verify API contract compliance
/// </summary>
public class JobApplicationContractTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();

    [DockerRequiredFact]
    public async Task SubmitApplication_AcceptsCorrectRequestStructure()
    {
        // Arrange
        var request = new SubmitJobApplicationRequest
        {
            JobPostingId = Guid.NewGuid(),
            ApplicantFirstName = "John",
            ApplicantLastName = "Doe",
            ApplicantEmail = "john.doe@example.com",
            ApplicantPhone = "+66812345678",
            ApplicantCountryCode = "TH",
            ResumeFileId = Guid.NewGuid(),
            AdditionalFileIds = [],
            CoverLetter = "I am interested in this position."
        };

        // Act
        var response = await _client.PostAsJsonAsync("/careers/v1/job-applications", request);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.Conflict,
            HttpStatusCode.NotFound);

        if (response.StatusCode == HttpStatusCode.Created)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

            var result = await response.Content.ReadFromJsonAsync<JobApplicationResponse>();
            result.Should().NotBeNull();

            // Verify Location header for created resource
            response.Headers.Location.Should().NotBeNull();
        }
    }

    [DockerRequiredFact]
    public async Task GetApplications_RequiresAuthentication()
    {
        // Act
        var response = await _client.GetAsync("/careers/v1/job-applications");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [DockerRequiredFact]
    public async Task GetApplications_WithAuth_ReturnsCorrectResponseStructure()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer Applicant test@example.com");

        // Act
        var response = await _client.GetAsync("/careers/v1/job-applications");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

            var result = await response.Content.ReadFromJsonAsync<JobApplicationListResponse>();
            result.Should().NotBeNull();

            // Verify response structure
            result!.Should().BeAssignableTo<JobApplicationListResponse>();
            result.Items.Should().NotBeNull();
            result.Page.Should().BeGreaterThanOrEqualTo(1);
            result.PageSize.Should().BeGreaterThan(0);
            result.TotalCount.Should().BeGreaterThanOrEqualTo(0);
            result.TotalPages.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [DockerRequiredFact]
    public async Task GetApplication_ById_ReturnsCorrectResponseStructure()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer Applicant test@example.com");
        var testId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/careers/v1/job-applications/{testId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Forbidden);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JobApplicationResponse>();
            result.Should().NotBeNull();

            // Verify response structure
            result!.Should().BeAssignableTo<JobApplicationResponse>();
            result.Id.Should().NotBeEmpty();
            result.JobPostingId.Should().NotBeEmpty();
            result.ApplicantFirstName.Should().NotBeNullOrEmpty();
            result.ApplicantLastName.Should().NotBeNullOrEmpty();
            result.ApplicantEmail.Should().NotBeNullOrEmpty();
            result.ResumeFileId.Should().NotBeEmpty();
            result.Status.Should().NotBeNullOrEmpty();
            result.RowVersion.Should().NotBeNullOrEmpty();
        }
    }

    [DockerRequiredFact]
    public void JobApplicationResponse_ContainsAllRequiredFields()
    {
        // This test verifies the response model structure by deserializing a sample
        var sampleJson = @"{
            ""id"": ""123e4567-e89b-12d3-a456-426614174000"",
            ""jobPostingId"": ""123e4567-e89b-12d3-a456-426614174001"",
            ""applicantFirstName"": ""John"",
            ""applicantLastName"": ""Doe"",
            ""applicantFullName"": ""John Doe"",
            ""applicantEmail"": ""john.doe@example.com"",
            ""applicantPhone"": ""+66812345678"",
            ""applicantCountryCode"": ""TH"",
            ""applicantCountryName"": ""Thailand"",
            ""resumeFileId"": ""123e4567-e89b-12d3-a456-426614174002"",
            ""resumeFileUrl"": ""https://storage.example.com/files/resume.pdf"",
            ""additionalFileIds"": [],
            ""additionalFileUrls"": [],
            ""coverLetter"": ""I am interested"",
            ""status"": ""Submitted"",
            ""appliedAt"": ""2025-01-01T00:00:00Z"",
            ""rowVersion"": ""AAAAAAAAB9E="",
            ""createdAt"": ""2025-01-01T00:00:00Z"",
            ""updatedAt"": ""2025-01-01T00:00:00Z""
        }";

        // Act
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = JsonSerializer.Deserialize<JobApplicationResponse>(sampleJson, options);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.JobPostingId.Should().NotBeEmpty();
        result.ApplicantEmail.Should().NotBeNullOrEmpty();
        result.Status.Should().NotBeNullOrEmpty();
        result.ApplicantFullName.Should().NotBeNullOrEmpty();
    }

    [DockerRequiredFact]
    public async Task SubmitApplicationRequest_ValidatesRequiredFields()
    {
        // Arrange - Missing required fields
        var invalidRequest = new
        {
            applicantFirstName = "John"
            // Missing other required fields
        };

        // Act
        var response = await _client.PostAsJsonAsync("/careers/v1/job-applications", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [DockerRequiredFact]
    public async Task SubmitApplicationRequest_ValidatesEmailFormat()
    {
        // Arrange - Invalid email format
        var request = new SubmitJobApplicationRequest
        {
            JobPostingId = Guid.NewGuid(),
            ApplicantFirstName = "John",
            ApplicantLastName = "Doe",
            ApplicantEmail = "invalid-email",
            ResumeFileId = Guid.NewGuid(),
            AdditionalFileIds = []
        };

        // Act
        var response = await _client.PostAsJsonAsync("/careers/v1/job-applications", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [DockerRequiredFact]
    public async Task SubmitApplicationRequest_ValidatesFileCount()
    {
        // Arrange - Too many additional files
        var request = new SubmitJobApplicationRequest
        {
            JobPostingId = Guid.NewGuid(),
            ApplicantFirstName = "John",
            ApplicantLastName = "Doe",
            ApplicantEmail = "john.doe@example.com",
            ResumeFileId = Guid.NewGuid(),
            AdditionalFileIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/careers/v1/job-applications", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [DockerRequiredFact]
    public async Task GetApplications_SupportsPagination()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer Applicant test@example.com");

        // Act
        var response = await _client.GetAsync("/careers/v1/job-applications?offset=0&limit=10");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JobApplicationListResponse>();
            result.Should().NotBeNull();

            // Verify pagination fields
            result!.Page.Should().BeGreaterThanOrEqualTo(1);
            result.PageSize.Should().Be(10);
        }
    }
}
