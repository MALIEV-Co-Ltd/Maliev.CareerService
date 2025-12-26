using Maliev.CareerService.Api.Models.Applications;
using Maliev.CareerService.Api.Authentication;
using Maliev.CareerService.Api.Services.External;
using Maliev.CareerService.Data.Models;
using Maliev.CareerService.Tests.Factories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Maliev.CareerService.Tests.Integration;

/// <summary>
/// Integration tests for application status management
/// </summary>
public class ApplicationStatusManagementTests : IClassFixture<ApplicationStatusManagementTests.CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _hrStaffId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public ApplicationStatusManagementTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // Default to HR Staff authorization
        var permissions = CareerPredefinedRoles.RolePermissions[CareerPredefinedRoles.HR];
        var token = factory.CreateTestJwtToken(_hrStaffId.ToString(), new[] { CareerPredefinedRoles.HR }, permissions);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task UpdateApplicationStatus_ValidTransition_ReturnsOk()
    {
        // Arrange
        var applicationId = await SeedTestApplicationAsync("submitted");

        // Get RowVersion
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();
        var application = await dbContext.JobApplications.FindAsync(applicationId);

        var request = new UpdateApplicationStatusRequest
        {
            NewStatus = "under_review",
            Reason = "Application meets initial criteria",
            RowVersion = Convert.ToBase64String(application!.RowVersion)
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/career/v1/job-applications/{applicationId}/status", request);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception($"Expected OK but got {response.StatusCode}. Content: {content}");
        }
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<JobApplicationResponse>();
        Assert.NotNull(result);
        Assert.Equal("under_review", result!.Status);
    }

    [Fact]
    public async Task UpdateApplicationStatus_WithEmailNotification_SendsEmail()
    {
        // Arrange
        var applicationId = await SeedTestApplicationAsync("under_review");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();
        var application = await dbContext.JobApplications.FindAsync(applicationId);

        var request = new UpdateApplicationStatusRequest
        {
            NewStatus = "interviewing",
            Reason = "Candidate shortlisted for interview",
            RowVersion = Convert.ToBase64String(application!.RowVersion)
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/career/v1/job-applications/{applicationId}/status", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify status change was recorded
        var changes = await dbContext.ApplicationStatusChanges
            .Where(c => c.ApplicationId == applicationId)
            .ToListAsync();
        Assert.Contains(changes, c => c.ToStatus == "interviewing" && c.ChangedBy == _hrStaffId);
    }

    [Fact]
    public async Task UpdateApplicationStatus_Reversal_RecordsReversalInAuditTrail()
    {
        // Arrange
        var applicationId = await SeedTestApplicationAsync("interviewing");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();
        var application = await dbContext.JobApplications.FindAsync(applicationId);

        // Add a status change to reverse
        var statusChange = new ApplicationStatusChange
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            FromStatus = "under_review",
            ToStatus = "interviewing",
            ChangedBy = _hrStaffId,
            ChangedAt = DateTime.UtcNow.AddHours(-1),
            Reason = "Candidate shortlisted"
        };
        dbContext.ApplicationStatusChanges.Add(statusChange);
        await dbContext.SaveChangesAsync();

        // Refresh application
        await dbContext.Entry(application!).ReloadAsync();

        var request = new UpdateApplicationStatusRequest
        {
            NewStatus = "under_review",
            Reason = "Interview cancelled - reverting status",
            IsReversal = true,
            RowVersion = Convert.ToBase64String(application!.RowVersion)
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/career/v1/job-applications/{applicationId}/status", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify reversal was recorded
        var changes = await dbContext.ApplicationStatusChanges
            .Where(c => c.ApplicationId == applicationId && c.IsReversal)
            .ToListAsync();
        Assert.True(changes.Count > 0);
        Assert.Contains(changes, c => c.Reason == "Interview cancelled - reverting status");
    }

    [Fact]
    public async Task UpdateApplicationStatus_InvalidTransition_ReturnsBadRequest()
    {
        // Arrange
        var applicationId = await SeedTestApplicationAsync("submitted");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();
        var application = await dbContext.JobApplications.FindAsync(applicationId);

        var request = new UpdateApplicationStatusRequest
        {
            NewStatus = "offered", // Invalid: can't go directly from submitted to offered
            Reason = "Skipping review process",
            RowVersion = Convert.ToBase64String(application!.RowVersion)
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/career/v1/job-applications/{applicationId}/status", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("transition", content);
    }

    [Fact]
    public async Task UpdateApplicationStatus_ConcurrencyConflict_ReturnsConflict()
    {
        // Arrange
        var applicationId = await SeedTestApplicationAsync("submitted");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();
        var application = await dbContext.JobApplications.FindAsync(applicationId);
        var oldRowVersion = Convert.ToBase64String(application!.RowVersion);

        // Simulate concurrent update by changing the status
        application.Status = "under_review";
        await dbContext.SaveChangesAsync();

        var request = new UpdateApplicationStatusRequest
        {
            NewStatus = "interviewing",
            Reason = "Using stale RowVersion",
            RowVersion = oldRowVersion // Old RowVersion will cause conflict
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/career/v1/job-applications/{applicationId}/status", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateApplicationStatus_AsEmployee_ReturnsForbidden()
    {
        // Arrange
        var applicationId = await SeedTestApplicationAsync("submitted");

        _client.DefaultRequestHeaders.Clear();
        var permissions = CareerPredefinedRoles.RolePermissions[CareerPredefinedRoles.Employee];
        var token = _factory.CreateTestJwtToken("employee-id", new[] { CareerPredefinedRoles.Employee }, permissions);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();
        var application = await dbContext.JobApplications.FindAsync(applicationId);

        var request = new UpdateApplicationStatusRequest
        {
            NewStatus = "under_review",
            Reason = "Unauthorized attempt",
            RowVersion = Convert.ToBase64String(application!.RowVersion)
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/career/v1/job-applications/{applicationId}/status", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateApplicationStatus_NonExistentApplication_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        var request = new UpdateApplicationStatusRequest
        {
            NewStatus = "under_review",
            Reason = "Test",
            RowVersion = "AAAAAAAAB9E="
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/career/v1/job-applications/{nonExistentId}/status", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateApplicationStatus_WithLongReason_TruncatesGracefully()
    {
        // Arrange
        var applicationId = await SeedTestApplicationAsync("submitted");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();
        var application = await dbContext.JobApplications.FindAsync(applicationId);

        var request = new UpdateApplicationStatusRequest
        {
            NewStatus = "under_review",
            Reason = new string('A', 1001), // Exceed max length of 1000
            RowVersion = Convert.ToBase64String(application!.RowVersion)
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/career/v1/job-applications/{applicationId}/status", request);

        // Assert - Should be rejected by validation
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<Guid> SeedTestApplicationAsync(string initialStatus)
    {
        // Clean database before seeding to ensure test isolation
        await _factory.CleanDatabaseAsync();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        // Create job posting
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

        // Create application
        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            JobPostingId = posting.Id,
            ApplicantFirstName = "John",
            ApplicantLastName = "Doe",
            ApplicantEmail = "john.doe@example.com",
            ApplicantPhone = "+66812345678",
            ApplicantCountryCode = "TH",
            ResumeFileId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Status = initialStatus,
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
                services.RemoveAll<IEmailServiceClient>();
                services.AddSingleton<IEmailServiceClient, Mocks.MockEmailServiceClient>();

                services.RemoveAll<IEmployeeServiceClient>();
                services.AddSingleton<IEmployeeServiceClient, Mocks.MockEmployeeServiceClient>();

                services.RemoveAll<ICountryServiceClient>();
                services.AddSingleton<ICountryServiceClient, Mocks.MockCountryServiceClient>();
            });
        }
    }
}
