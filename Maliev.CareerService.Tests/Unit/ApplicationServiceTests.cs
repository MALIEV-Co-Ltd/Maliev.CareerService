using AutoMapper;
using FluentAssertions;
using Maliev.CareerService.Api.Models.Applications;
using Maliev.CareerService.Api.Models.JobPostings;
using Maliev.CareerService.Api.Services;
using Maliev.CareerService.Api.Services.External;
using Maliev.CareerService.Data;
using Maliev.CareerService.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.CareerService.Tests.Unit;

/// <summary>
/// Unit tests for ApplicationService business logic
/// </summary>
public class ApplicationServiceTests : IDisposable
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMarkdownService> _markdownServiceMock;
    private readonly Mock<IUploadServiceClient> _uploadServiceMock;
    private readonly Mock<ICountryServiceClient> _countryServiceMock;
    private readonly Mock<IEmailServiceClient> _emailServiceMock;
    private readonly Mock<IEmployeeServiceClient> _employeeServiceMock;
    private readonly Mock<IMetricsService> _metricsServiceMock;
    private readonly Mock<ILogger<ApplicationService>> _loggerMock;
    private readonly CareerDbContext _dbContext;
    private readonly ApplicationService _service;

    public ApplicationServiceTests()
    {
        // Setup in-memory database for unit testing
        var options = new DbContextOptionsBuilder<CareerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CareerDbContext(options);

        // Setup mocks
        _mapperMock = new Mock<IMapper>();
        _markdownServiceMock = new Mock<IMarkdownService>();
        _uploadServiceMock = new Mock<IUploadServiceClient>();
        _countryServiceMock = new Mock<ICountryServiceClient>();
        _emailServiceMock = new Mock<IEmailServiceClient>();
        _employeeServiceMock = new Mock<IEmployeeServiceClient>();
        _metricsServiceMock = new Mock<IMetricsService>();
        _loggerMock = new Mock<ILogger<ApplicationService>>();

        // Create service instance
        _service = new ApplicationService(
            _dbContext,
            _mapperMock.Object,
            _markdownServiceMock.Object,
            _uploadServiceMock.Object,
            _countryServiceMock.Object,
            _emailServiceMock.Object,
            _employeeServiceMock.Object,
            _metricsServiceMock.Object,
            _loggerMock.Object
        );
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    #region SubmitApplicationAsync Tests

    [Fact]
    public async Task SubmitApplicationAsync_WithValidRequest_CreatesApplication()
    {
        // Arrange
        var jobPosting = new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Software Engineer",
            PositionCode = "SE-001",
            Department = "Engineering",
            Location = "Bangkok",
            EmploymentType = "Full-time",
            Description = "Description",
            Requirements = "Requirements",
            Responsibilities = "Responsibilities",
            ApplicationDeadline = DateTime.UtcNow.AddMonths(1), // Future deadline
            PublishedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        await _dbContext.JobPostings.AddAsync(jobPosting);
        await _dbContext.SaveChangesAsync();

        var request = new SubmitJobApplicationRequest
        {
            JobPostingId = jobPosting.Id,
            ApplicantFirstName = "John",
            ApplicantLastName = "Doe",
            ApplicantEmail = "john.doe@example.com",
            ApplicantPhone = "+66812345678",
            ApplicantCountryCode = "TH",
            ResumeFileId = Guid.NewGuid(),
            CoverLetter = "I am interested in this position",
            AdditionalFileIds = []
        };

        var expectedApplication = new JobApplication
        {
            Id = Guid.NewGuid(),
            JobPostingId = request.JobPostingId,
            ApplicantFirstName = request.ApplicantFirstName,
            ApplicantLastName = request.ApplicantLastName,
            ApplicantEmail = request.ApplicantEmail,
            ResumeFileId = request.ResumeFileId,
            Status = ApplicationStatus.Submitted,
            AppliedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        var expectedResponse = new JobApplicationResponse
        {
            Id = expectedApplication.Id,
            ApplicantFirstName = expectedApplication.ApplicantFirstName,
            ApplicantLastName = expectedApplication.ApplicantLastName,
            ApplicantEmail = expectedApplication.ApplicantEmail,
            Status = ApplicationStatus.Submitted
        };

        _uploadServiceMock
            .Setup(x => x.ValidateFileAsync(request.ResumeFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mapperMock
            .Setup(x => x.Map<JobApplication>(request))
            .Returns(expectedApplication);

        _mapperMock
            .Setup(x => x.Map<JobApplicationResponse>(It.IsAny<JobApplication>()))
            .Returns(expectedResponse);

        // Act
        var result = await _service.SubmitApplicationAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.ApplicantEmail.Should().Be(request.ApplicantEmail);

        var savedApplication = await _dbContext.JobApplications.FirstOrDefaultAsync();
        savedApplication.Should().NotBeNull();
        savedApplication!.ApplicantEmail.Should().Be(request.ApplicantEmail);

        _metricsServiceMock.Verify(
            x => x.IncrementJobApplications(ApplicationStatus.Submitted),
            Times.Once);
    }

    [Fact]
    public async Task SubmitApplicationAsync_WhenDeadlinePassed_ThrowsInvalidOperationException()
    {
        // Arrange
        var jobPosting = new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Software Engineer",
            PositionCode = "SE-002",
            Department = "Engineering",
            Location = "Bangkok",
            EmploymentType = "Full-time",
            Description = "Description",
            Requirements = "Requirements",
            Responsibilities = "Responsibilities",
            ApplicationDeadline = DateTime.UtcNow.AddDays(-1), // Past deadline
            PublishedAt = DateTime.UtcNow.AddMonths(-1),
            IsActive = true,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        await _dbContext.JobPostings.AddAsync(jobPosting);
        await _dbContext.SaveChangesAsync();

        var request = new SubmitJobApplicationRequest
        {
            JobPostingId = jobPosting.Id,
            ApplicantFirstName = "John",
            ApplicantLastName = "Doe",
            ApplicantEmail = "john.doe@example.com",
            ApplicantPhone = "+66812345678",
            ApplicantCountryCode = "TH",
            ResumeFileId = Guid.NewGuid(),
            CoverLetter = "I am interested",
            AdditionalFileIds = []
        };

        // Act & Assert
        await _service.Invoking(s => s.SubmitApplicationAsync(request))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*deadline*passed*");
    }

    [Fact]
    public async Task SubmitApplicationAsync_WhenDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var jobPosting = new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Software Engineer",
            PositionCode = "SE-003",
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

        await _dbContext.JobPostings.AddAsync(jobPosting);

        // Add existing application
        var existingApplication = new JobApplication
        {
            Id = Guid.NewGuid(),
            JobPostingId = jobPosting.Id,
            ApplicantFirstName = "Jane",
            ApplicantLastName = "Doe",
            ApplicantEmail = "jane.doe@example.com",
            ApplicantPhone = "+66812345678",
            ApplicantCountryCode = "TH",
            ResumeFileId = Guid.NewGuid(),
            Status = ApplicationStatus.Submitted,
            AppliedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        await _dbContext.JobApplications.AddAsync(existingApplication);
        await _dbContext.SaveChangesAsync();

        var request = new SubmitJobApplicationRequest
        {
            JobPostingId = jobPosting.Id,
            ApplicantFirstName = "Jane",
            ApplicantLastName = "Doe",
            ApplicantEmail = "jane.doe@example.com", // Same email
            ApplicantPhone = "+66812345678",
            ApplicantCountryCode = "TH",
            ResumeFileId = Guid.NewGuid(),
            CoverLetter = "Another application",
            AdditionalFileIds = []
        };

        // Act & Assert
        await _service.Invoking(s => s.SubmitApplicationAsync(request))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task SubmitApplicationAsync_WhenResumeFileInvalid_ThrowsArgumentException()
    {
        // Arrange
        var jobPosting = new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Software Engineer",
            PositionCode = "SE-004",
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

        await _dbContext.JobPostings.AddAsync(jobPosting);
        await _dbContext.SaveChangesAsync();

        var invalidResumeId = Guid.NewGuid();
        var request = new SubmitJobApplicationRequest
        {
            JobPostingId = jobPosting.Id,
            ApplicantFirstName = "John",
            ApplicantLastName = "Doe",
            ApplicantEmail = "john.doe@example.com",
            ApplicantPhone = "+66812345678",
            ApplicantCountryCode = "TH",
            ResumeFileId = invalidResumeId,
            CoverLetter = "I am interested",
            AdditionalFileIds = []
        };

        _uploadServiceMock
            .Setup(x => x.ValidateFileAsync(invalidResumeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await _service.Invoking(s => s.SubmitApplicationAsync(request))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Resume file*not found*");
    }

    [Fact]
    public async Task SubmitApplicationAsync_WhenAdditionalFilesInvalid_ThrowsArgumentException()
    {
        // Arrange
        var jobPosting = new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Software Engineer",
            PositionCode = "SE-005",
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

        await _dbContext.JobPostings.AddAsync(jobPosting);
        await _dbContext.SaveChangesAsync();

        var resumeId = Guid.NewGuid();
        var validFileId = Guid.NewGuid();
        var invalidFileId = Guid.NewGuid();

        var request = new SubmitJobApplicationRequest
        {
            JobPostingId = jobPosting.Id,
            ApplicantFirstName = "John",
            ApplicantLastName = "Doe",
            ApplicantEmail = "john.doe@example.com",
            ApplicantPhone = "+66812345678",
            ApplicantCountryCode = "TH",
            ResumeFileId = resumeId,
            CoverLetter = "I am interested",
            AdditionalFileIds = [validFileId, invalidFileId]
        };

        _uploadServiceMock
            .Setup(x => x.ValidateFileAsync(resumeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _uploadServiceMock
            .Setup(x => x.ValidateFileAsync(validFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _uploadServiceMock
            .Setup(x => x.ValidateFileAsync(invalidFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await _service.Invoking(s => s.SubmitApplicationAsync(request))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*file IDs are not found*");
    }

    #endregion

    #region UpdateApplicationStatusAsync Tests

    [Fact]
    public async Task UpdateApplicationStatusAsync_WithValidTransition_UpdatesStatus()
    {
        // Arrange
        var hrUserId = Guid.NewGuid();
        var jobPosting = new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Software Engineer",
            PositionCode = "SE-006",
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

        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            JobPostingId = jobPosting.Id,
            ApplicantFirstName = "John",
            ApplicantLastName = "Doe",
            ApplicantEmail = "john.doe@example.com",
            ApplicantPhone = "+66812345678",
            ApplicantCountryCode = "TH",
            ResumeFileId = Guid.NewGuid(),
            Status = ApplicationStatus.Submitted,
            AppliedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid(),
            JobPosting = jobPosting
        };

        await _dbContext.JobPostings.AddAsync(jobPosting);
        await _dbContext.JobApplications.AddAsync(application);
        await _dbContext.SaveChangesAsync();

        // Detach to get fresh RowVersion
        _dbContext.Entry(application).State = EntityState.Detached;
        var freshApplication = await _dbContext.JobApplications
            .Include(a => a.JobPosting)
            .FirstAsync(a => a.Id == application.Id);

        var request = new UpdateApplicationStatusRequest
        {
            NewStatus = ApplicationStatus.UnderReview,
            Reason = "Application meets initial criteria",
            RowVersion = Convert.ToBase64String(freshApplication.RowVersion)
        };

        var expectedResponse = new JobApplicationResponse
        {
            Id = application.Id,
            Status = ApplicationStatus.UnderReview
        };

        _mapperMock
            .Setup(x => x.Map<JobApplicationResponse>(It.IsAny<JobApplication>()))
            .Returns(expectedResponse);

        _emailServiceMock
            .Setup(x => x.SendStatusChangeNotificationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateApplicationStatusAsync(application.Id, request, hrUserId);

        // Assert
        result.Should().NotBeNull();

        var updatedApplication = await _dbContext.JobApplications.FirstAsync(a => a.Id == application.Id);
        updatedApplication.Status.Should().Be(ApplicationStatus.UnderReview);

        var statusChange = await _dbContext.ApplicationStatusChanges.FirstAsync();
        statusChange.Should().NotBeNull();
        statusChange.FromStatus.Should().Be(ApplicationStatus.Submitted);
        statusChange.ToStatus.Should().Be(ApplicationStatus.UnderReview);
        statusChange.ChangedBy.Should().Be(hrUserId);
        statusChange.Reason.Should().Be(request.Reason);

        _metricsServiceMock.Verify(
            x => x.IncrementJobApplications(ApplicationStatus.UnderReview),
            Times.Once);

        _emailServiceMock.Verify(
            x => x.SendStatusChangeNotificationAsync(
                application.ApplicantEmail,
                It.IsAny<string>(),
                jobPosting.PositionTitle,
                ApplicationStatus.UnderReview,
                request.Reason,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateApplicationStatusAsync_WithInvalidTransition_ThrowsInvalidOperationException()
    {
        // Arrange
        var hrUserId = Guid.NewGuid();
        var jobPosting = new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Software Engineer",
            PositionCode = "SE-007",
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

        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            JobPostingId = jobPosting.Id,
            ApplicantFirstName = "John",
            ApplicantLastName = "Doe",
            ApplicantEmail = "john.doe@example.com",
            ApplicantPhone = "+66812345678",
            ApplicantCountryCode = "TH",
            ResumeFileId = Guid.NewGuid(),
            Status = ApplicationStatus.Submitted,
            AppliedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid(),
            JobPosting = jobPosting
        };

        await _dbContext.JobPostings.AddAsync(jobPosting);
        await _dbContext.JobApplications.AddAsync(application);
        await _dbContext.SaveChangesAsync();

        _dbContext.Entry(application).State = EntityState.Detached;
        var freshApplication = await _dbContext.JobApplications
            .Include(a => a.JobPosting)
            .FirstAsync(a => a.Id == application.Id);

        var request = new UpdateApplicationStatusRequest
        {
            NewStatus = ApplicationStatus.Offered, // Invalid: can't go from Submitted to Offered
            Reason = "Skipping steps",
            RowVersion = Convert.ToBase64String(freshApplication.RowVersion)
        };

        // Act & Assert
        await _service.Invoking(s => s.UpdateApplicationStatusAsync(application.Id, request, hrUserId))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Invalid status transition*");
    }

    [Fact(Skip = "InMemoryDatabase does not properly support RowVersion concurrency tokens. " +
                 "See Integration/ApplicationStatusManagementTests.UpdateApplicationStatus_ConcurrencyConflict_ReturnsConflict " +
                 "for the integration test that validates this functionality with real PostgreSQL.")]
    public async Task UpdateApplicationStatusAsync_WithStaleRowVersion_ThrowsDbUpdateConcurrencyException()
    {
        // Arrange
        var hrUserId = Guid.NewGuid();
        var jobPosting = new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Software Engineer",
            PositionCode = "SE-008",
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

        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            JobPostingId = jobPosting.Id,
            ApplicantFirstName = "John",
            ApplicantLastName = "Doe",
            ApplicantEmail = "john.doe@example.com",
            ApplicantPhone = "+66812345678",
            ApplicantCountryCode = "TH",
            ResumeFileId = Guid.NewGuid(),
            Status = ApplicationStatus.Submitted,
            AppliedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid(),
            JobPosting = jobPosting
        };

        await _dbContext.JobPostings.AddAsync(jobPosting);
        await _dbContext.JobApplications.AddAsync(application);
        await _dbContext.SaveChangesAsync();

        var staleRowVersion = Convert.ToBase64String(application.RowVersion);

        // Simulate concurrent update
        application.Status = ApplicationStatus.UnderReview;
        await _dbContext.SaveChangesAsync();

        var request = new UpdateApplicationStatusRequest
        {
            NewStatus = ApplicationStatus.Rejected,
            Reason = "Using stale version",
            RowVersion = staleRowVersion // This is now stale
        };

        // Act & Assert
        await _service.Invoking(s => s.UpdateApplicationStatusAsync(application.Id, request, hrUserId))
            .Should().ThrowAsync<DbUpdateConcurrencyException>()
            .WithMessage("*modified by another user*");
    }

    #endregion

    #region ValidateStatusTransition Tests

    [Theory]
    [InlineData(ApplicationStatus.Submitted, ApplicationStatus.UnderReview, true)]
    [InlineData(ApplicationStatus.Submitted, ApplicationStatus.Rejected, true)]
    [InlineData(ApplicationStatus.Submitted, ApplicationStatus.Withdrawn, true)]
    [InlineData(ApplicationStatus.Submitted, ApplicationStatus.Offered, false)]
    [InlineData(ApplicationStatus.UnderReview, ApplicationStatus.Interviewing, true)]
    [InlineData(ApplicationStatus.UnderReview, ApplicationStatus.Submitted, true)] // Reversal
    [InlineData(ApplicationStatus.Interviewing, ApplicationStatus.Offered, true)]
    [InlineData(ApplicationStatus.Interviewing, ApplicationStatus.UnderReview, true)] // Reversal
    [InlineData(ApplicationStatus.Offered, ApplicationStatus.Accepted, true)]
    [InlineData(ApplicationStatus.Offered, ApplicationStatus.Interviewing, true)] // Reversal
    [InlineData(ApplicationStatus.Accepted, ApplicationStatus.Rejected, false)] // Terminal state
    [InlineData(ApplicationStatus.Rejected, ApplicationStatus.UnderReview, false)] // Terminal state
    [InlineData(ApplicationStatus.Withdrawn, ApplicationStatus.Submitted, false)] // Terminal state
    public void ValidateStatusTransition_ValidatesCorrectly(string fromStatus, string toStatus, bool expectedResult)
    {
        // Act
        var result = _service.ValidateStatusTransition(fromStatus, toStatus);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region GetStatusHistoryAsync Tests

    [Fact]
    public async Task GetStatusHistoryAsync_ReturnsFullAuditTrail()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var hrUser1Id = Guid.NewGuid();
        var hrUser2Id = Guid.NewGuid();

        var application = new JobApplication
        {
            Id = applicationId,
            JobPostingId = Guid.NewGuid(),
            ApplicantFirstName = "John",
            ApplicantLastName = "Doe",
            ApplicantEmail = "john.doe@example.com",
            ApplicantPhone = "+66812345678",
            ApplicantCountryCode = "TH",
            ResumeFileId = Guid.NewGuid(),
            Status = ApplicationStatus.Interviewing,
            AppliedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        var change1 = new ApplicationStatusChange
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            FromStatus = ApplicationStatus.Submitted,
            ToStatus = ApplicationStatus.UnderReview,
            ChangedBy = hrUser1Id,
            ChangedAt = DateTime.UtcNow.AddDays(-2),
            Reason = "Initial review",
            IsReversal = false
        };

        var change2 = new ApplicationStatusChange
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            FromStatus = ApplicationStatus.UnderReview,
            ToStatus = ApplicationStatus.Interviewing,
            ChangedBy = hrUser2Id,
            ChangedAt = DateTime.UtcNow.AddDays(-1),
            Reason = "Scheduling interview",
            IsReversal = false
        };

        await _dbContext.JobApplications.AddAsync(application);
        await _dbContext.ApplicationStatusChanges.AddRangeAsync(change1, change2);
        await _dbContext.SaveChangesAsync();

        _employeeServiceMock
            .Setup(x => x.GetEmployeeAsync(hrUser1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Api.Services.External.EmployeeResponse(
                Id: hrUser1Id,
                FirstName: "Jane",
                LastName: "Smith",
                Email: "jane.smith@maliev.com",
                Department: "HR",
                Position: "HR Manager"));

        _employeeServiceMock
            .Setup(x => x.GetEmployeeAsync(hrUser2Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Api.Services.External.EmployeeResponse(
                Id: hrUser2Id,
                FirstName: "Bob",
                LastName: "Johnson",
                Email: "bob.johnson@maliev.com",
                Department: "HR",
                Position: "HR Staff"));

        // Act
        var result = await _service.GetStatusHistoryAsync(applicationId);

        // Assert
        result.Should().NotBeNull();
        result.ApplicationId.Should().Be(applicationId);
        result.Changes.Should().HaveCount(2);

        // Ordered by ChangedAt DESC (newest first)
        result.Changes[0].ToStatus.Should().Be(ApplicationStatus.Interviewing);
        result.Changes[0].ChangedByName.Should().Be("Bob Johnson");
        result.Changes[1].ToStatus.Should().Be(ApplicationStatus.UnderReview);
        result.Changes[1].ChangedByName.Should().Be("Jane Smith");
    }

    [Fact]
    public async Task GetStatusHistoryAsync_WhenApplicationNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await _service.Invoking(s => s.GetStatusHistoryAsync(nonExistentId))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    #endregion

    #region ValidateDuplicateAsync Tests

    [Fact]
    public async Task ValidateDuplicateAsync_WhenDuplicateExists_ReturnsTrue()
    {
        // Arrange
        var jobPostingId = Guid.NewGuid();
        var email = "test@example.com";

        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            JobPostingId = jobPostingId,
            ApplicantFirstName = "John",
            ApplicantLastName = "Doe",
            ApplicantEmail = email,
            ApplicantPhone = "+66812345678",
            ApplicantCountryCode = "TH",
            ResumeFileId = Guid.NewGuid(),
            Status = ApplicationStatus.Submitted,
            AppliedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        await _dbContext.JobApplications.AddAsync(application);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.ValidateDuplicateAsync(jobPostingId, email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateDuplicateAsync_WhenNoDuplicate_ReturnsFalse()
    {
        // Arrange
        var jobPostingId = Guid.NewGuid();
        var email = "test@example.com";

        // Act
        var result = await _service.ValidateDuplicateAsync(jobPostingId, email);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ValidateDeadlineAsync Tests

    [Fact]
    public async Task ValidateDeadlineAsync_WhenDeadlineInFuture_ReturnsTrue()
    {
        // Arrange
        var jobPosting = new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Software Engineer",
            PositionCode = "SE-009",
            Department = "Engineering",
            Location = "Bangkok",
            EmploymentType = "Full-time",
            Description = "Description",
            Requirements = "Requirements",
            Responsibilities = "Responsibilities",
            ApplicationDeadline = DateTime.UtcNow.AddMonths(1), // Future
            PublishedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        await _dbContext.JobPostings.AddAsync(jobPosting);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.ValidateDeadlineAsync(jobPosting.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateDeadlineAsync_WhenDeadlinePassed_ReturnsFalse()
    {
        // Arrange
        var jobPosting = new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Software Engineer",
            PositionCode = "SE-010",
            Department = "Engineering",
            Location = "Bangkok",
            EmploymentType = "Full-time",
            Description = "Description",
            Requirements = "Requirements",
            Responsibilities = "Responsibilities",
            ApplicationDeadline = DateTime.UtcNow.AddDays(-1), // Past
            PublishedAt = DateTime.UtcNow.AddMonths(-1),
            IsActive = true,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        await _dbContext.JobPostings.AddAsync(jobPosting);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.ValidateDeadlineAsync(jobPosting.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateDeadlineAsync_WhenPostingNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await _service.Invoking(s => s.ValidateDeadlineAsync(nonExistentId))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    #endregion
}
