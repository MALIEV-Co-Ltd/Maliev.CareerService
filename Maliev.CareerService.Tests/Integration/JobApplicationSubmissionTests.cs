using FluentAssertions;
using Maliev.CareerService.Api.Models.Applications;
using Maliev.CareerService.Api.Services.External;
using Maliev.CareerService.Data.Models;
using Maliev.CareerService.Tests.Factories;
using Maliev.CareerService.Tests.Mocks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Maliev.CareerService.Tests.Helpers;

namespace Maliev.CareerService.Tests.Integration;

/// <summary>
/// Integration tests for job application submission
/// </summary>
public class JobApplicationSubmissionTests(JobApplicationSubmissionTests.CustomWebApplicationFactory factory) : IClassFixture<JobApplicationSubmissionTests.CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();

    [DockerRequiredFact]
    public async Task SubmitApplication_WithValidData_ReturnsCreated()
    {
        // Arrange
        var jobPostingId = await SeedTestJobPostingAsync();
        var resumeFileId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var request = new SubmitJobApplicationRequest
        {
            JobPostingId = jobPostingId,
            ApplicantFirstName = "John",
            ApplicantLastName = "Doe",
            ApplicantEmail = "john.doe@example.com",
            ApplicantPhone = "+66812345678",
            ApplicantCountryCode = "TH",
            ResumeFileId = resumeFileId,
            AdditionalFileIds = [],
            CoverLetter = "I am interested in this position."
        };

        // Act
        var response = await _client.PostAsJsonAsync("/careers/v1/job-applications", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<JobApplicationResponse>();
        result.Should().NotBeNull();
        result!.ApplicantEmail.Should().Be(request.ApplicantEmail);
        result.Status.Should().Be("Submitted");
        result.ResumeFileUrl.Should().NotBeNullOrEmpty();
    }

    [DockerRequiredFact]
    public async Task SubmitApplication_WithAdditionalFiles_ReturnsCreated()
    {
        // Arrange
        var jobPostingId = await SeedTestJobPostingAsync();
        var resumeFileId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var additionalFileId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        var request = new SubmitJobApplicationRequest
        {
            JobPostingId = jobPostingId,
            ApplicantFirstName = "Jane",
            ApplicantLastName = "Smith",
            ApplicantEmail = "jane.smith@example.com",
            ResumeFileId = resumeFileId,
            AdditionalFileIds = [additionalFileId]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/careers/v1/job-applications", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<JobApplicationResponse>();
        result.Should().NotBeNull();
        result!.AdditionalFileUrls.Should().HaveCount(1);
    }

    [DockerRequiredFact]
    public async Task SubmitApplication_WithInvalidFileId_ReturnsBadRequest()
    {
        // Arrange
        var jobPostingId = await SeedTestJobPostingAsync();
        var invalidFileId = Guid.NewGuid();

        var request = new SubmitJobApplicationRequest
        {
            JobPostingId = jobPostingId,
            ApplicantFirstName = "John",
            ApplicantLastName = "Doe",
            ApplicantEmail = "john.doe@example.com",
            ResumeFileId = invalidFileId,
            AdditionalFileIds = []
        };

        // Act
        var response = await _client.PostAsJsonAsync("/careers/v1/job-applications", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [DockerRequiredFact]
    public async Task SubmitApplication_AfterDeadline_ReturnsConflict()
    {
        // Arrange
        var jobPostingId = await SeedExpiredJobPostingAsync();
        var resumeFileId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var request = new SubmitJobApplicationRequest
        {
            JobPostingId = jobPostingId,
            ApplicantFirstName = "John",
            ApplicantLastName = "Doe",
            ApplicantEmail = "john.doe@example.com",
            ResumeFileId = resumeFileId,
            AdditionalFileIds = []
        };

        // Act
        var response = await _client.PostAsJsonAsync("/careers/v1/job-applications", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [DockerRequiredFact]
    public async Task SubmitApplication_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var jobPostingId = await SeedTestJobPostingAsync();
        var resumeFileId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var email = "duplicate@example.com";

        // Submit first application
        var firstRequest = new SubmitJobApplicationRequest
        {
            JobPostingId = jobPostingId,
            ApplicantFirstName = "John",
            ApplicantLastName = "Doe",
            ApplicantEmail = email,
            ResumeFileId = resumeFileId,
            AdditionalFileIds = []
        };

        await _client.PostAsJsonAsync("/careers/v1/job-applications", firstRequest);

        // Submit duplicate application
        var duplicateRequest = new SubmitJobApplicationRequest
        {
            JobPostingId = jobPostingId,
            ApplicantFirstName = "Jane",
            ApplicantLastName = "Smith",
            ApplicantEmail = email,
            ResumeFileId = resumeFileId,
            AdditionalFileIds = []
        };

        // Act
        var response = await _client.PostAsJsonAsync("/careers/v1/job-applications", duplicateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [DockerRequiredFact]
    public async Task SubmitApplication_WithTooManyFiles_FailsValidation()
    {
        // Arrange
        var jobPostingId = await SeedTestJobPostingAsync();
        var resumeFileId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var request = new SubmitJobApplicationRequest
        {
            JobPostingId = jobPostingId,
            ApplicantFirstName = "John",
            ApplicantLastName = "Doe",
            ApplicantEmail = "john.doe@example.com",
            ResumeFileId = resumeFileId,
            // Try to add 5 additional files (should fail - max is 4)
            AdditionalFileIds =
            [
                Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()
            ]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/careers/v1/job-applications", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<Guid> SeedTestJobPostingAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        var posting = new JobPosting
        {
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
            PublishedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.JobPostings.Add(posting);
        await dbContext.SaveChangesAsync();

        return posting.Id;
    }

    private async Task<Guid> SeedExpiredJobPostingAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        var posting = new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Expired Position",
            PositionCode = $"EXP-{Guid.NewGuid().ToString()[..8]}",
            Department = "Engineering",
            Location = "Bangkok",
            EmploymentType = "Full-time",
            Description = "Expired position",
            Requirements = "Experience required",
            Responsibilities = "Develop software",
            ApplicationDeadline = DateTime.UtcNow.AddDays(-1), // Expired
            PublishedAt = DateTime.UtcNow.AddMonths(-1),
            IsActive = true,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.JobPostings.Add(posting);
        await dbContext.SaveChangesAsync();

        return posting.Id;
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
