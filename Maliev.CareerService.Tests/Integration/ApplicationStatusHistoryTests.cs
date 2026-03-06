using CareerDbContext = Maliev.CareerService.Infrastructure.Data.CareerDbContext;
using Maliev.CareerService.Application.Models.Applications;
using Maliev.CareerService.Api.Authentication;
using Maliev.CareerService.Application.Services.External;
using Maliev.CareerService.Domain.Entities;
using Maliev.CareerService.Tests.Factories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Maliev.CareerService.Tests.Integration;

/// <summary>
/// Integration tests for application status history and audit trail
/// </summary>
public class ApplicationStatusHistoryTests : IClassFixture<ApplicationStatusHistoryTests.CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _hrStaffId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public ApplicationStatusHistoryTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // Default to HR Staff authorization
        var permissions = CareerPredefinedRoles.GetPermissions(CareerPredefinedRoles.HR);
        var token = factory.CreateTestJwtToken(_hrStaffId.ToString(), new[] { CareerPredefinedRoles.HR }, permissions);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task GetStatusHistory_ExistingApplication_ReturnsFullAuditTrail()
    {
        // Arrange
        var applicationId = await SeedApplicationWithStatusHistoryAsync();

        // Act
        var response = await _client.GetAsync($"/career/v1/job-applications/{applicationId}/status-history");

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception($"Expected OK but got {response.StatusCode}. Content: {content}");
        }
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<StatusHistoryResponse>();
        Assert.NotNull(result);
        Assert.Equal(applicationId, result!.ApplicationId);
        Assert.NotEmpty(result.Changes);
        Assert.True(result.Changes.Count >= 3); // At least Submitted, UnderReview, Interview
    }

    [Fact]
    public async Task GetStatusHistory_OrderedByChangedAtDesc_ReturnsNewestFirst()
    {
        // Arrange
        var applicationId = await SeedApplicationWithStatusHistoryAsync();

        // Act
        var response = await _client.GetAsync($"/career/v1/job-applications/{applicationId}/status-history");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<StatusHistoryResponse>();
        Assert.NotNull(result);

        // Verify chronological order (newest first)
        for (int i = 0; i < result!.Changes.Count - 1; i++)
        {
            Assert.True(result.Changes[i].ChangedAt >= result.Changes[i + 1].ChangedAt);
        }
    }

    [Fact]
    public async Task GetStatusHistory_IncludesReversals_MarkedAsReversalInHistory()
    {
        // Arrange
        var applicationId = await SeedApplicationWithReversalHistoryAsync();

        // Act
        var response = await _client.GetAsync($"/career/v1/job-applications/{applicationId}/status-history");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<StatusHistoryResponse>();
        Assert.NotNull(result);
        Assert.Contains(result!.Changes, c => c.IsReversal == true);
    }

    [Fact]
    public async Task GetStatusHistory_IncludesUserNames_PopulatesChangedByName()
    {
        // Arrange
        var applicationId = await SeedApplicationWithStatusHistoryAsync();

        // Act
        var response = await _client.GetAsync($"/career/v1/job-applications/{applicationId}/status-history");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<StatusHistoryResponse>();
        Assert.NotNull(result);

        // Verify all changes have user names populated
        foreach (var change in result!.Changes)
        {
            Assert.False(string.IsNullOrEmpty(change.ChangedByName));
        }
    }

    [Fact]
    public async Task GetStatusHistory_IncludesReasons_DisplaysStatusChangeReasons()
    {
        // Arrange
        var applicationId = await SeedApplicationWithStatusHistoryAsync();

        // Act
        var response = await _client.GetAsync($"/career/v1/job-applications/{applicationId}/status-history");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<StatusHistoryResponse>();
        Assert.NotNull(result);

        // Verify changes have reasons
        Assert.Contains(result!.Changes, c => !string.IsNullOrEmpty(c.Reason));
    }

    [Fact]
    public async Task GetStatusHistory_NonExistentApplication_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/career/v1/job-applications/{nonExistentId}/status-history");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetStatusHistory_AsEmployee_ReturnsForbidden()
    {
        // Arrange
        var applicationId = await SeedApplicationWithStatusHistoryAsync();

        var permissions = CareerPredefinedRoles.GetPermissions(CareerPredefinedRoles.Employee);
        var token = _factory.CreateTestJwtToken("employee-id", new[] { CareerPredefinedRoles.Employee }, permissions);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/career/v1/job-applications/{applicationId}/status-history");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetStatusHistory_EmptyHistory_ReturnsEmptyList()
    {
        // Arrange
        var applicationId = await SeedApplicationWithoutStatusHistoryAsync();

        // Act
        var response = await _client.GetAsync($"/career/v1/job-applications/{applicationId}/status-history");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<StatusHistoryResponse>();
        Assert.NotNull(result);
        Assert.Equal(applicationId, result!.ApplicationId);
        Assert.Empty(result.Changes);
    }

    private async Task<Guid> SeedApplicationWithStatusHistoryAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CareerDbContext>();

        // Create job posting
        var posting = new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Senior Developer",
            PositionCode = $"SD-{Guid.NewGuid().ToString()[..8]}",
            Department = "Engineering",
            Location = "Bangkok",
            EmploymentType = "Full-time",
            Description = "Senior Developer position",
            Requirements = "5+ years experience",
            Responsibilities = "Lead development",
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
            ApplicantFirstName = "Jane",
            ApplicantLastName = "Smith",
            ApplicantEmail = "jane.smith@example.com",
            ApplicantPhone = "+66812345678",
            ApplicantCountryCode = "TH",
            ResumeFileId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Status = "Interview",
            AppliedAt = DateTime.UtcNow.AddDays(-10),
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.JobApplications.Add(application);
        await dbContext.SaveChangesAsync();

        // Create status history
        var changes = new List<ApplicationStatusChange>
        {
            new() {
                Id = Guid.NewGuid(),
                ApplicationId = application.Id,
                FromStatus = null,
                ToStatus = "Submitted",
                ChangedBy = Guid.NewGuid(),
                ChangedAt = DateTime.UtcNow.AddDays(-10),
                Reason = "Initial submission"
            },
            new() {
                Id = Guid.NewGuid(),
                ApplicationId = application.Id,
                FromStatus = "Submitted",
                ToStatus = "UnderReview",
                ChangedBy = _hrStaffId,
                ChangedAt = DateTime.UtcNow.AddDays(-8),
                Reason = "Application meets initial criteria"
            },
            new() {
                Id = Guid.NewGuid(),
                ApplicationId = application.Id,
                FromStatus = "UnderReview",
                ToStatus = "Interview",
                ChangedBy = _hrStaffId,
                ChangedAt = DateTime.UtcNow.AddDays(-5),
                Reason = "Candidate shortlisted for interview"
            }
        };

        dbContext.ApplicationStatusChanges.AddRange(changes);
        await dbContext.SaveChangesAsync();

        return application.Id;
    }

    private async Task<Guid> SeedApplicationWithReversalHistoryAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CareerDbContext>();

        // Create job posting
        var posting = new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Data Analyst",
            PositionCode = $"DA-{Guid.NewGuid().ToString()[..8]}",
            Department = "Analytics",
            Location = "Bangkok",
            EmploymentType = "Full-time",
            Description = "Data Analyst position",
            Requirements = "3+ years experience",
            Responsibilities = "Analyze data",
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
            ApplicantFirstName = "Bob",
            ApplicantLastName = "Johnson",
            ApplicantEmail = "bob.johnson@example.com",
            ApplicantPhone = "+66812345678",
            ApplicantCountryCode = "TH",
            ResumeFileId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Status = "UnderReview",
            AppliedAt = DateTime.UtcNow.AddDays(-7),
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.JobApplications.Add(application);
        await dbContext.SaveChangesAsync();

        // Create status history with reversal
        var originalChange = new ApplicationStatusChange
        {
            Id = Guid.NewGuid(),
            ApplicationId = application.Id,
            FromStatus = "UnderReview",
            ToStatus = "Interview",
            ChangedBy = _hrStaffId,
            ChangedAt = DateTime.UtcNow.AddDays(-3),
            Reason = "Candidate shortlisted"
        };

        var reversalChange = new ApplicationStatusChange
        {
            Id = Guid.NewGuid(),
            ApplicationId = application.Id,
            FromStatus = "Interview",
            ToStatus = "UnderReview",
            ChangedBy = _hrStaffId,
            ChangedAt = DateTime.UtcNow.AddDays(-2),
            Reason = "Interview cancelled - reverting status",
            IsReversal = true,
            ReversedChangeId = originalChange.Id
        };

        dbContext.ApplicationStatusChanges.AddRange([originalChange, reversalChange]);
        await dbContext.SaveChangesAsync();

        return application.Id;
    }

    private async Task<Guid> SeedApplicationWithoutStatusHistoryAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CareerDbContext>();

        // Create job posting
        var posting = new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Junior Developer",
            PositionCode = $"JD-{Guid.NewGuid().ToString()[..8]}",
            Department = "Engineering",
            Location = "Bangkok",
            EmploymentType = "Full-time",
            Description = "Junior Developer position",
            Requirements = "1+ years experience",
            Responsibilities = "Develop features",
            ApplicationDeadline = DateTime.UtcNow.AddMonths(1),
            PublishedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.JobPostings.Add(posting);
        await dbContext.SaveChangesAsync();

        // Create application without history
        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            JobPostingId = posting.Id,
            ApplicantFirstName = "Alice",
            ApplicantLastName = "Brown",
            ApplicantEmail = "alice.brown@example.com",
            ApplicantPhone = "+66812345678",
            ApplicantCountryCode = "TH",
            ResumeFileId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Status = "Submitted",
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
                services.RemoveAll<IEmployeeServiceClient>();
                services.AddSingleton<IEmployeeServiceClient, Mocks.MockEmployeeServiceClient>();
            });
        }
    }
}
