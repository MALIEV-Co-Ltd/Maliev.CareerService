using FluentAssertions;
using Maliev.CareerService.Api.Models;
using Maliev.CareerService.Api.Services;
using Maliev.CareerService.Data.DbContexts;
using Maliev.CareerService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.CareerService.Tests.Services;

public class JobApplicationServiceTests : IDisposable
{
    private readonly CareerDbContext _context;
    private readonly Mock<ILogger<JobApplicationService>> _mockLogger;
    private readonly JobApplicationService _jobApplicationService;

    public JobApplicationServiceTests()
    {
        var options = new DbContextOptionsBuilder<CareerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CareerDbContext(options);
        _mockLogger = new Mock<ILogger<JobApplicationService>>();

        _jobApplicationService = new JobApplicationService(
            _context,
            _mockLogger.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var jobPosition = new JobPosition
        {
            Id = 1,
            Title = "Software Engineer",
            Department = "Engineering",
            Description = "Develop software applications",
            EmploymentType = "Full-time",
            ExperienceLevel = "Mid-level",
            IsActive = true,
            IsPublic = true
        };

        var jobApplication = new JobApplication
        {
            Id = 1,
            JobPositionId = 1,
            ApplicantEmail = "test@example.com",
            ApplicantName = "Test Applicant",
            ApplicantPhone = "+66123456789",
            LinkedInProfile = "https://linkedin.com/in/testuser",
            PortfolioUrl = "https://portfolio.example.com",
            Status = "Submitted",
            ApplicationDate = DateTime.UtcNow,
            Notes = "Great candidate"
        };

        _context.JobPositions.Add(jobPosition);
        _context.JobApplications.Add(jobApplication);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingApplication_ReturnsJobApplicationDto()
    {
        // Act
        var result = await _jobApplicationService.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.JobPositionId.Should().Be(1);
        result.ApplicantEmail.Should().Be("test@example.com");
        result.ApplicantName.Should().Be("Test Applicant");
        result.ApplicantPhone.Should().Be("+66123456789");
        result.LinkedInProfile.Should().Be("https://linkedin.com/in/testuser");
        result.PortfolioUrl.Should().Be("https://portfolio.example.com");
        result.Status.Should().Be("Submitted");
        result.Notes.Should().Be("Great candidate");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingApplication_ReturnsNull()
    {
        // Act
        var result = await _jobApplicationService.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByJobPositionIdAsync_ExistingPosition_ReturnsApplications()
    {
        // Act
        var result = await _jobApplicationService.GetByJobPositionIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().JobPositionId.Should().Be(1);
        result.Items.First().ApplicantEmail.Should().Be("test@example.com");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetByJobPositionIdAsync_NonExistingPosition_ReturnsEmpty()
    {
        // Act
        var result = await _jobApplicationService.GetByJobPositionIdAsync(999);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllApplications()
    {
        // Act
        var result = await _jobApplicationService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAsync_WithStatusFilter_ReturnsFilteredResults()
    {
        // Arrange
        var additionalApplication = new JobApplication
        {
            Id = 2,
            JobPositionId = 1,
            ApplicantEmail = "another@example.com",
            ApplicantName = "Another Applicant",
            Status = "Under Review",
            ApplicationDate = DateTime.UtcNow.AddDays(-1)
        };

        _context.JobApplications.Add(additionalApplication);
        await _context.SaveChangesAsync();

        // Act
        var result = await _jobApplicationService.GetAllAsync(1, 10, "Submitted");

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Status.Should().Be("Submitted");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedJobApplication()
    {
        // Arrange
        var request = new CreateJobApplicationRequest
        {
            JobPositionId = 1,
            ApplicantEmail = "newapplicant@example.com",
            ApplicantName = "New Applicant",
            ApplicantPhone = "+66987654321",
            LinkedInProfile = "https://linkedin.com/in/newapplicant",
            PortfolioUrl = "https://newportfolio.example.com"
        };

        // Act
        var result = await _jobApplicationService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.JobPositionId.Should().Be(1);
        result.ApplicantEmail.Should().Be("newapplicant@example.com");
        result.ApplicantName.Should().Be("New Applicant");
        result.ApplicantPhone.Should().Be("+66987654321");
        result.LinkedInProfile.Should().Be("https://linkedin.com/in/newapplicant");
        result.PortfolioUrl.Should().Be("https://newportfolio.example.com");
        result.Status.Should().Be("Submitted");

        // Verify in database
        var applicationInDb = await _context.JobApplications.FirstAsync(ja => ja.Id == result.Id);
        applicationInDb.ApplicantEmail.Should().Be("newapplicant@example.com");
        applicationInDb.Status.Should().Be("Submitted");
    }

    [Fact]
    public async Task CreateAsync_NonExistingJobPosition_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateJobApplicationRequest
        {
            JobPositionId = 999, // Non-existing job position
            ApplicantEmail = "test@example.com",
            ApplicantName = "Test Applicant"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _jobApplicationService.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_DuplicateApplication_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateJobApplicationRequest
        {
            JobPositionId = 1,
            ApplicantEmail = "test@example.com", // Same email as existing application
            ApplicantName = "Duplicate Applicant"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _jobApplicationService.CreateAsync(request));
    }

    [Fact]
    public async Task UpdateStatusAsync_ExistingApplication_ReturnsUpdatedJobApplication()
    {
        // Arrange
        var request = new UpdateJobApplicationRequest
        {
            Status = "Under Review",
            Notes = "Moving to next stage"
        };

        // Act
        var result = await _jobApplicationService.UpdateStatusAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Under Review");
        result.Notes.Should().Be("Moving to next stage");
        result.LastStatusChange.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateStatusAsync_NonExistingApplication_ReturnsNull()
    {
        // Arrange
        var request = new UpdateJobApplicationRequest
        {
            Status = "Under Review"
        };

        // Act
        var result = await _jobApplicationService.UpdateStatusAsync(999, request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateStatusAsync_InvalidStatus_ThrowsArgumentException()
    {
        // Arrange
        var request = new UpdateJobApplicationRequest
        {
            Status = "Invalid Status"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _jobApplicationService.UpdateStatusAsync(1, request));
    }

    [Fact]
    public async Task DeleteAsync_ExistingApplication_ReturnsTrue()
    {
        // Act
        var result = await _jobApplicationService.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();

        // Verify application is deleted from database
        var applicationInDb = await _context.JobApplications.FirstOrDefaultAsync(ja => ja.Id == 1);
        applicationInDb.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingApplication_ReturnsFalse()
    {
        // Act
        var result = await _jobApplicationService.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_ExistingApplication_ReturnsTrue()
    {
        // Act
        var result = await _jobApplicationService.ExistsAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistingApplication_ReturnsFalse()
    {
        // Act
        var result = await _jobApplicationService.ExistsAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasExistingApplicationAsync_ExistingApplication_ReturnsTrue()
    {
        // Act
        var result = await _jobApplicationService.HasExistingApplicationAsync("test@example.com", 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasExistingApplicationAsync_NonExistingApplication_ReturnsFalse()
    {
        // Act
        var result = await _jobApplicationService.HasExistingApplicationAsync("notfound@example.com", 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsMatchingResults()
    {
        // Act
        var result = await _jobApplicationService.GetByEmailAsync("test@example.com");

        // Assert
        var applications = result.ToList();
        applications.Should().HaveCount(1);
        applications.First().ApplicantEmail.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_NonExistingEmail_ReturnsEmpty()
    {
        // Act
        var result = await _jobApplicationService.GetByEmailAsync("notfound@example.com");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_OrdersByApplicationDateDescending()
    {
        // Arrange
        var additionalApplication = new JobApplication
        {
            Id = 2,
            JobPositionId = 1,
            ApplicantEmail = "another@example.com",
            ApplicantName = "Another Applicant",
            Status = "Under Review",
            ApplicationDate = DateTime.UtcNow.AddDays(1) // Future date to test ordering
        };

        _context.JobApplications.Add(additionalApplication);
        await _context.SaveChangesAsync();

        // Act
        var result = await _jobApplicationService.GetAllAsync(1, 10);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.First().ApplicantName.Should().Be("Another Applicant"); // Most recent first
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}