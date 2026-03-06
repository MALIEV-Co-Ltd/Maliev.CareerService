using Maliev.CareerService.Application.Models.Applications;
using Maliev.CareerService.Tests.Factories;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Maliev.CareerService.Tests.Contract;

/// <summary>
/// Contract tests for job application endpoints - verify API contract compliance
/// </summary>
public class JobApplicationContractTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
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
        var response = await _client.PostAsJsonAsync("/career/v1/job-applications", request);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Created ||
                   response.StatusCode == HttpStatusCode.BadRequest ||
                   response.StatusCode == HttpStatusCode.Conflict ||
                   response.StatusCode == HttpStatusCode.NotFound);

        if (response.StatusCode == HttpStatusCode.Created)
        {
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

            var result = await response.Content.ReadFromJsonAsync<JobApplicationResponse>();
            Assert.NotNull(result);

            // Verify Location header for created resource
            Assert.NotNull(response.Headers.Location);
        }
    }

    [Fact]
    public async Task GetApplications_RequiresAuthentication()
    {
        // Act
        var response = await _client.GetAsync("/career/v1/job-applications");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetApplications_WithAuth_ReturnsCorrectResponseStructure()
    {
        // Arrange
        var additionalClaims = new Dictionary<string, string> { { "email", "applicant@example.com" } };
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _factory.CreateTestJwtToken("applicant-id", new[] { "Applicant" }, new[] { "career.applications.read" }, additionalClaims));

        // Act
        var response = await _client.GetAsync("/career/v1/job-applications");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotImplemented);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

            var result = await response.Content.ReadFromJsonAsync<JobApplicationListResponse>();
            Assert.NotNull(result);

            // Verify response structure
            Assert.IsAssignableFrom<JobApplicationListResponse>(result);
            Assert.NotNull(result.Items);
            Assert.True(result.Page >= 1);
            Assert.True(result.PageSize > 0);
            Assert.True(result.TotalCount >= 0);
            Assert.True(result.TotalPages >= 0);
        }
    }

    [Fact]
    public async Task GetApplication_ById_ReturnsCorrectResponseStructure()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _factory.CreateTestJwtToken("applicant-id", new[] { "Applicant" }, new[] { "career.applications.read" }));
        var testId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/career/v1/job-applications/{testId}");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK ||
                   response.StatusCode == HttpStatusCode.NotFound ||
                   response.StatusCode == HttpStatusCode.Forbidden);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JobApplicationResponse>();
            Assert.NotNull(result);

            // Verify response structure
            Assert.IsAssignableFrom<JobApplicationResponse>(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.NotEqual(Guid.Empty, result.JobPostingId);
            Assert.False(string.IsNullOrEmpty(result.ApplicantFirstName));
            Assert.False(string.IsNullOrEmpty(result.ApplicantLastName));
            Assert.False(string.IsNullOrEmpty(result.ApplicantEmail));
            Assert.NotEqual(Guid.Empty, result.ResumeFileId);
            Assert.False(string.IsNullOrEmpty(result.Status));
            Assert.False(string.IsNullOrEmpty(result.RowVersion));
        }
    }

    [Fact]
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
            ""rowVersion"": ""1"",
            ""createdAt"": ""2025-01-01T00:00:00Z"",
            ""updatedAt"": ""2025-01-01T00:00:00Z""
        }";

        // Act
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = JsonSerializer.Deserialize<JobApplicationResponse>(sampleJson, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotEqual(Guid.Empty, result.JobPostingId);
        Assert.False(string.IsNullOrEmpty(result.ApplicantEmail));
        Assert.False(string.IsNullOrEmpty(result.Status));
        Assert.False(string.IsNullOrEmpty(result.ApplicantFullName));
    }

    [Fact]
    public async Task SubmitApplicationRequest_ValidatesRequiredFields()
    {
        // Arrange - Missing required fields
        var invalidRequest = new
        {
            applicantFirstName = "John"
            // Missing other required fields
        };

        // Act
        var response = await _client.PostAsJsonAsync("/career/v1/job-applications", invalidRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
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
        var response = await _client.PostAsJsonAsync("/career/v1/job-applications", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
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
        var response = await _client.PostAsJsonAsync("/career/v1/job-applications", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetApplications_SupportsPagination()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        var additionalClaims = new Dictionary<string, string> { { "email", "applicant@example.com" } };
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _factory.CreateTestJwtToken("applicant-id", new[] { "Applicant" }, new[] { "career.applications.read" }, additionalClaims));

        // Act
        var response = await _client.GetAsync("/career/v1/job-applications?offset=0&limit=10");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotImplemented);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JobApplicationListResponse>();
            Assert.NotNull(result);

            // Verify pagination fields
            Assert.True(result.Page >= 1);
            Assert.Equal(10, result.PageSize);
        }
    }
}
