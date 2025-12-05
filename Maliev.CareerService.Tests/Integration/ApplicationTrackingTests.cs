using Maliev.CareerService.Api.Models.Applications;
using Maliev.CareerService.Api.Services.External;
using Maliev.CareerService.Data.Models;
using Maliev.CareerService.Tests.Factories;
using Maliev.CareerService.Tests.Mocks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Maliev.CareerService.Tests.Helpers;

namespace Maliev.CareerService.Tests.Integration;

/// <summary>
/// Integration tests for application tracking
/// </summary>
public class ApplicationTrackingTests(ApplicationTrackingTests.CustomWebApplicationFactory factory) : IClassFixture<ApplicationTrackingTests.CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();

    [DockerRequiredFact]
    public async Task GetApplications_AsApplicant_ReturnsOwnApplicationsOnly()
    {
        // Arrange
        var applicantEmail = "applicant1@example.com";
        await SeedTestApplicationsAsync(applicantEmail);

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer Applicant {applicantEmail}");

        // Act
        var response = await _client.GetAsync("/careers/v1/job-applications");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<JobApplicationListResponse>();
        Assert.NotNull(result);
        Assert.All(result!.Items, a => Assert.Equal(applicantEmail, a.ApplicantEmail));
    }

    [DockerRequiredFact]
    public async Task GetApplications_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/careers/v1/job-applications");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [DockerRequiredFact]
    public async Task GetApplications_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var applicantEmail = "applicant2@example.com";
        await SeedMultipleApplicationsAsync(applicantEmail, 5);

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer Applicant {applicantEmail}");

        // Act
        var response = await _client.GetAsync("/careers/v1/job-applications?offset=0&limit=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<JobApplicationListResponse>();
        Assert.NotNull(result);
        Assert.True(result!.Items.Count <= 2);
        Assert.Equal(2, result.PageSize);
    }

    [DockerRequiredFact]
    public async Task GetApplication_ById_AsOwner_ReturnsApplication()
    {
        // Arrange
        var applicantEmail = "owner@example.com";
        var applicationId = await SeedSingleApplicationAsync(applicantEmail);

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer Applicant {applicantEmail}");

        // Act
        var response = await _client.GetAsync($"/careers/v1/job-applications/{applicationId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<JobApplicationResponse>();
        Assert.NotNull(result);
        Assert.Equal(applicationId, result!.Id);
        Assert.Equal(applicantEmail, result.ApplicantEmail);
        Assert.NotNull(result.JobPosting);
    }

    [DockerRequiredFact]
    public async Task GetApplication_ById_AsNonOwner_ReturnsForbidden()
    {
        // Arrange
        var ownerEmail = "owner@example.com";
        var otherEmail = "other@example.com";
        var applicationId = await SeedSingleApplicationAsync(ownerEmail);

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer Applicant {otherEmail}");

        // Act
        var response = await _client.GetAsync($"/careers/v1/job-applications/{applicationId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [DockerRequiredFact]
    public async Task GetApplication_ById_AsHRStaff_ReturnsApplication()
    {
        // Arrange
        var applicantEmail = "applicant@example.com";
        var applicationId = await SeedSingleApplicationAsync(applicantEmail);

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer HRStaff hr@company.com");

        // Act
        var response = await _client.GetAsync($"/careers/v1/job-applications/{applicationId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<JobApplicationResponse>();
        Assert.NotNull(result);
        Assert.Equal(applicationId, result!.Id);
    }

    [DockerRequiredFact]
    public async Task GetApplication_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer Applicant test@example.com");

        // Act
        var response = await _client.GetAsync($"/careers/v1/job-applications/{invalidId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task SeedTestApplicationsAsync(string applicantEmail)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        // Create a job posting first
        var jobPosting = new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Test Position",
            PositionCode = $"TEST-{Guid.NewGuid().ToString()[..8]}",
            Department = "Engineering",
            Location = "Bangkok",
            EmploymentType = "Full-time",
            Description = "Test description",
            Requirements = "Test requirements",
            Responsibilities = "Test responsibilities",
            ApplicationDeadline = DateTime.UtcNow.AddMonths(1),
            PublishedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.JobPostings.Add(jobPosting);

        // Create applications
        var application1 = new JobApplication
        {
            Id = Guid.NewGuid(),
            JobPostingId = jobPosting.Id,
            ApplicantFirstName = "John",
            ApplicantLastName = "Doe",
            ApplicantEmail = applicantEmail,
            ResumeFileId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Status = ApplicationStatus.Submitted,
            AppliedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        // Create another application from different user
        var application2 = new JobApplication
        {
            Id = Guid.NewGuid(),
            JobPostingId = jobPosting.Id,
            ApplicantFirstName = "Jane",
            ApplicantLastName = "Smith",
            ApplicantEmail = "other@example.com",
            ResumeFileId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            Status = ApplicationStatus.Submitted,
            AppliedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.JobApplications.AddRange(application1, application2);
        await dbContext.SaveChangesAsync();
    }

    private async Task SeedMultipleApplicationsAsync(string applicantEmail, int count)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        // Create job postings
        var jobPostings = new List<JobPosting>();
        for (int i = 0; i < count; i++)
        {
            var jobPosting = new JobPosting
            {
                Id = Guid.NewGuid(),
                PositionTitle = $"Position {i}",
                PositionCode = $"POS-{i}-{Guid.NewGuid().ToString()[..8]}",
                Department = "Engineering",
                Location = "Bangkok",
                EmploymentType = "Full-time",
                Description = "Description",
                Requirements = "Requirements",
                Responsibilities = "Responsibilities",
                ApplicationDeadline = DateTime.UtcNow.AddMonths(1),
                PublishedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            };
            jobPostings.Add(jobPosting);
        }

        dbContext.JobPostings.AddRange(jobPostings);

        // Create applications
        var applications = jobPostings.Select(jp => new JobApplication
        {
            Id = Guid.NewGuid(),
            JobPostingId = jp.Id,
            ApplicantFirstName = "Test",
            ApplicantLastName = "User",
            ApplicantEmail = applicantEmail,
            ResumeFileId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Status = ApplicationStatus.Submitted,
            AppliedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        }).ToList();

        dbContext.JobApplications.AddRange(applications);
        await dbContext.SaveChangesAsync();
    }

    private async Task<Guid> SeedSingleApplicationAsync(string applicantEmail)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        // Create a job posting first
        var jobPosting = new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Test Position",
            PositionCode = $"TEST-{Guid.NewGuid().ToString()[..8]}",
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

        dbContext.JobPostings.Add(jobPosting);

        // Create application
        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            JobPostingId = jobPosting.Id,
            ApplicantFirstName = "Test",
            ApplicantLastName = "User",
            ApplicantEmail = applicantEmail,
            ResumeFileId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Status = ApplicationStatus.Submitted,
            AppliedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.JobApplications.Add(application);
        await dbContext.SaveChangesAsync();

        return application.Id;
    }

    /// <summary>
    /// Custom WebApplicationFactory that registers mock external services
    /// </summary>
    public class CustomWebApplicationFactory : TestWebApplicationFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureTestServices(services =>
            {
                // Replace external services with mocks
                services.RemoveAll<IUploadServiceClient>();
                services.AddSingleton<IUploadServiceClient, MockUploadServiceClient>();

                services.RemoveAll<IEmailServiceClient>();
                services.AddSingleton<IEmailServiceClient, MockEmailServiceClient>();

                services.RemoveAll<ICountryServiceClient>();
                services.AddSingleton<ICountryServiceClient, MockCountryServiceClient>();

                services.RemoveAll<IEmployeeServiceClient>();
                services.AddSingleton<IEmployeeServiceClient, MockEmployeeServiceClient>();
            });
        }
    }
}
