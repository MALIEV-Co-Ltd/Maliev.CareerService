using AutoMapper;
using FluentAssertions;
using Maliev.CareerService.Api.Models.JobPostings;
using Maliev.CareerService.Api.Services;
using Maliev.CareerService.Data;
using Maliev.CareerService.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.CareerService.Tests.Unit;

/// <summary>
/// Unit tests for JobPostingService business logic
/// </summary>
public class JobPostingServiceTests : IDisposable
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMarkdownService> _markdownServiceMock;
    private readonly Mock<IMetricsService> _metricsServiceMock;
    private readonly Mock<ILogger<JobPostingService>> _loggerMock;
    private readonly CareerDbContext _dbContext;
    private readonly JobPostingService _service;

    public JobPostingServiceTests()
    {
        var options = new DbContextOptionsBuilder<CareerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CareerDbContext(options);

        _mapperMock = new Mock<IMapper>();
        _markdownServiceMock = new Mock<IMarkdownService>();
        _metricsServiceMock = new Mock<IMetricsService>();
        _loggerMock = new Mock<ILogger<JobPostingService>>();

        _service = new JobPostingService(
            _dbContext,
            _mapperMock.Object,
            _markdownServiceMock.Object,
            _metricsServiceMock.Object,
            _loggerMock.Object
        );

        // Setup markdown service to return simple HTML
        _markdownServiceMock
            .Setup(x => x.ToHtml(It.IsAny<string>()))
            .Returns((string md) => $"<p>{md}</p>");
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetActivePostingsAsync Tests

    [Fact]
    public async Task GetActivePostingsAsync_ReturnsOnlyActivePostings()
    {
        // Arrange
        var activePosting = CreateJobPosting("SE-001", isActive: true, publishedAt: DateTime.UtcNow, deadlineInFuture: true);
        var inactivePosting = CreateJobPosting("SE-002", isActive: false, publishedAt: DateTime.UtcNow, deadlineInFuture: true);
        var expiredPosting = CreateJobPosting("SE-003", isActive: true, publishedAt: DateTime.UtcNow, deadlineInFuture: false);

        await _dbContext.JobPostings.AddRangeAsync(activePosting, inactivePosting, expiredPosting);
        await _dbContext.SaveChangesAsync();

        SetupMapperForPosting();

        // Act
        var result = await _service.GetActivePostingsAsync(1, 10);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetActivePostingsAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        for (int i = 1; i <= 15; i++)
        {
            var posting = CreateJobPosting($"SE-{i:D3}", isActive: true, publishedAt: DateTime.UtcNow.AddDays(-i), deadlineInFuture: true);
            await _dbContext.JobPostings.AddAsync(posting);
        }
        await _dbContext.SaveChangesAsync();

        SetupMapperForPosting();

        // Act
        var result = await _service.GetActivePostingsAsync(pageNumber: 2, pageSize: 5);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(15);
        result.Items.Should().HaveCount(5);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetActivePostingsAsync_OrdersByPublishedDateDescending()
    {
        // Arrange
        var posting1 = CreateJobPosting("SE-001", isActive: true, publishedAt: DateTime.UtcNow.AddDays(-3), deadlineInFuture: true);
        var posting2 = CreateJobPosting("SE-002", isActive: true, publishedAt: DateTime.UtcNow.AddDays(-1), deadlineInFuture: true);
        var posting3 = CreateJobPosting("SE-003", isActive: true, publishedAt: DateTime.UtcNow.AddDays(-2), deadlineInFuture: true);

        await _dbContext.JobPostings.AddRangeAsync(posting1, posting2, posting3);
        await _dbContext.SaveChangesAsync();

        _mapperMock
            .Setup(x => x.Map<JobPostingResponse>(It.IsAny<JobPosting>()))
            .Returns((JobPosting jp) => new JobPostingResponse
            {
                Id = jp.Id,
                PositionCode = jp.PositionCode,
                PublishedAt = jp.PublishedAt
            });

        // Act
        var result = await _service.GetActivePostingsAsync(1, 10);

        // Assert
        result.Items[0].PositionCode.Should().Be("SE-002"); // Most recent
        result.Items[1].PositionCode.Should().Be("SE-003");
        result.Items[2].PositionCode.Should().Be("SE-001"); // Oldest
    }

    #endregion

    #region GetPostingByIdAsync Tests

    [Fact]
    public async Task GetPostingByIdAsync_WhenExists_ReturnsPosting()
    {
        // Arrange
        var posting = CreateJobPosting("SE-001", isActive: true, publishedAt: DateTime.UtcNow, deadlineInFuture: true);
        await _dbContext.JobPostings.AddAsync(posting);
        await _dbContext.SaveChangesAsync();

        SetupMapperForPosting();

        // Act
        var result = await _service.GetPostingByIdAsync(posting.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(posting.Id);
    }

    [Fact]
    public async Task GetPostingByIdAsync_WhenNotExists_ReturnsNull()
    {
        // Act
        var result = await _service.GetPostingByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region SearchPostingsAsync Tests

    [Fact]
    public async Task SearchPostingsAsync_WithSearchTerm_FiltersCorrectly()
    {
        // Arrange
        var posting1 = CreateJobPosting("SE-001", isActive: true, publishedAt: DateTime.UtcNow, deadlineInFuture: true);
        posting1.PositionTitle = "Senior Software Engineer";

        var posting2 = CreateJobPosting("PM-001", isActive: true, publishedAt: DateTime.UtcNow, deadlineInFuture: true);
        posting2.PositionTitle = "Product Manager";

        await _dbContext.JobPostings.AddRangeAsync(posting1, posting2);
        await _dbContext.SaveChangesAsync();

        SetupMapperForPosting();

        // Act
        var result = await _service.SearchPostingsAsync(
            searchTerm: "Engineer",
            department: null,
            location: null,
            employmentType: null,
            pageNumber: 1,
            pageSize: 10);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchPostingsAsync_WithDepartment_FiltersCorrectly()
    {
        // Arrange
        var posting1 = CreateJobPosting("SE-001", isActive: true, publishedAt: DateTime.UtcNow, deadlineInFuture: true);
        posting1.Department = "Engineering";

        var posting2 = CreateJobPosting("HR-001", isActive: true, publishedAt: DateTime.UtcNow, deadlineInFuture: true);
        posting2.Department = "Human Resources";

        await _dbContext.JobPostings.AddRangeAsync(posting1, posting2);
        await _dbContext.SaveChangesAsync();

        SetupMapperForPosting();

        // Act
        var result = await _service.SearchPostingsAsync(
            searchTerm: null,
            department: "Engineering",
            location: null,
            employmentType: null,
            pageNumber: 1,
            pageSize: 10);

        // Assert
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchPostingsAsync_WithLocation_FiltersCorrectly()
    {
        // Arrange
        var posting1 = CreateJobPosting("SE-001", isActive: true, publishedAt: DateTime.UtcNow, deadlineInFuture: true);
        posting1.Location = "Bangkok";

        var posting2 = CreateJobPosting("SE-002", isActive: true, publishedAt: DateTime.UtcNow, deadlineInFuture: true);
        posting2.Location = "Chiang Mai";

        await _dbContext.JobPostings.AddRangeAsync(posting1, posting2);
        await _dbContext.SaveChangesAsync();

        SetupMapperForPosting();

        // Act
        var result = await _service.SearchPostingsAsync(
            searchTerm: null,
            department: null,
            location: "Bangkok",
            employmentType: null,
            pageNumber: 1,
            pageSize: 10);

        // Assert
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchPostingsAsync_WithEmploymentType_FiltersCorrectly()
    {
        // Arrange
        var posting1 = CreateJobPosting("SE-001", isActive: true, publishedAt: DateTime.UtcNow, deadlineInFuture: true);
        posting1.EmploymentType = "Full-time";

        var posting2 = CreateJobPosting("SE-002", isActive: true, publishedAt: DateTime.UtcNow, deadlineInFuture: true);
        posting2.EmploymentType = "Contract";

        await _dbContext.JobPostings.AddRangeAsync(posting1, posting2);
        await _dbContext.SaveChangesAsync();

        SetupMapperForPosting();

        // Act
        var result = await _service.SearchPostingsAsync(
            searchTerm: null,
            department: null,
            location: null,
            employmentType: "Full-time",
            pageNumber: 1,
            pageSize: 10);

        // Assert
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchPostingsAsync_WithMultipleFilters_CombinesCorrectly()
    {
        // Arrange
        var posting1 = CreateJobPosting("SE-001", isActive: true, publishedAt: DateTime.UtcNow, deadlineInFuture: true);
        posting1.PositionTitle = "Software Engineer";
        posting1.Department = "Engineering";
        posting1.Location = "Bangkok";
        posting1.EmploymentType = "Full-time";

        var posting2 = CreateJobPosting("SE-002", isActive: true, publishedAt: DateTime.UtcNow, deadlineInFuture: true);
        posting2.PositionTitle = "Software Engineer";
        posting2.Department = "Engineering";
        posting2.Location = "Chiang Mai";
        posting2.EmploymentType = "Full-time";

        await _dbContext.JobPostings.AddRangeAsync(posting1, posting2);
        await _dbContext.SaveChangesAsync();

        SetupMapperForPosting();

        // Act
        var result = await _service.SearchPostingsAsync(
            searchTerm: "Engineer",
            department: "Engineering",
            location: "Bangkok",
            employmentType: "Full-time",
            pageNumber: 1,
            pageSize: 10);

        // Assert
        result.TotalCount.Should().Be(1);
    }

    #endregion

    #region CreatePostingAsync Tests

    [Fact]
    public async Task CreatePostingAsync_WithValidRequest_CreatesPosting()
    {
        // Arrange
        var createdBy = Guid.NewGuid();
        var request = new CreateJobPostingRequest
        {
            PositionCode = "SE-001",
            PositionTitle = "Software Engineer",
            Department = "Engineering",
            Location = "Bangkok",
            EmploymentType = "Full-time",
            Description = "Description",
            Requirements = "Requirements",
            Responsibilities = "Responsibilities",
            ApplicationDeadline = DateTime.UtcNow.AddMonths(1),
            PublishImmediately = true
        };

        _mapperMock
            .Setup(x => x.Map<JobPosting>(request))
            .Returns(new JobPosting
            {
                Id = Guid.NewGuid(),
                PositionCode = request.PositionCode,
                PositionTitle = request.PositionTitle,
                Department = request.Department,
                Location = request.Location,
                EmploymentType = request.EmploymentType,
                Description = request.Description,
                Requirements = request.Requirements,
                Responsibilities = request.Responsibilities,
                ApplicationDeadline = request.ApplicationDeadline,
                IsActive = true  // JobPosting entity has IsActive, not the request
            });

        SetupMapperForPosting();

        // Act
        var result = await _service.CreatePostingAsync(request, createdBy);

        // Assert
        result.Should().NotBeNull();

        var savedPosting = await _dbContext.JobPostings.FirstOrDefaultAsync();
        savedPosting.Should().NotBeNull();
        savedPosting!.PositionCode.Should().Be("SE-001");
        savedPosting.CreatedBy.Should().Be(createdBy);
        savedPosting.UpdatedBy.Should().Be(createdBy);
    }

    [Fact]
    public async Task CreatePostingAsync_WhenDuplicatePositionCode_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingPosting = CreateJobPosting("SE-001", isActive: true, publishedAt: DateTime.UtcNow, deadlineInFuture: true);
        await _dbContext.JobPostings.AddAsync(existingPosting);
        await _dbContext.SaveChangesAsync();

        var request = new CreateJobPostingRequest
        {
            PositionCode = "SE-001",
            PositionTitle = "Another Position",
            Department = "Engineering",
            Location = "Bangkok",
            EmploymentType = "Full-time",
            Description = "Description",
            Requirements = "Requirements",
            Responsibilities = "Responsibilities",
            ApplicationDeadline = DateTime.UtcNow.AddMonths(1),
            PublishImmediately = true
        };

        // Act & Assert
        await _service.Invoking(s => s.CreatePostingAsync(request, Guid.NewGuid()))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*position code*already exists*");
    }

    [Fact]
    public async Task CreatePostingAsync_WhenActive_UpdatesMetrics()
    {
        // Arrange
        var request = new CreateJobPostingRequest
        {
            PositionCode = "SE-001",
            PositionTitle = "Software Engineer",
            Department = "Engineering",
            Location = "Bangkok",
            EmploymentType = "Full-time",
            Description = "Description",
            Requirements = "Requirements",
            Responsibilities = "Responsibilities",
            ApplicationDeadline = DateTime.UtcNow.AddMonths(1),
            PublishImmediately = true
        };

        _mapperMock
            .Setup(x => x.Map<JobPosting>(request))
            .Returns(new JobPosting
            {
                Id = Guid.NewGuid(),
                PositionCode = request.PositionCode,
                PositionTitle = request.PositionTitle,
                IsActive = true
            });

        SetupMapperForPosting();

        // Act
        await _service.CreatePostingAsync(request, Guid.NewGuid());

        // Assert
        _metricsServiceMock.Verify(
            x => x.SetActiveJobPostings(It.IsAny<int>()),
            Times.Once);
    }

    #endregion

    #region UpdatePostingAsync Tests

    [Fact]
    public async Task UpdatePostingAsync_WhenExists_UpdatesPosting()
    {
        // Arrange
        var posting = CreateJobPosting("SE-001", isActive: true, publishedAt: DateTime.UtcNow, deadlineInFuture: true);
        await _dbContext.JobPostings.AddAsync(posting);
        await _dbContext.SaveChangesAsync();

        // Detach and reload to get fresh RowVersion
        _dbContext.Entry(posting).State = EntityState.Detached;
        var freshPosting = await _dbContext.JobPostings.FirstAsync(p => p.Id == posting.Id);

        var updatedBy = Guid.NewGuid();
        var request = new UpdateJobPostingRequest
        {
            PositionTitle = "Senior Software Engineer",
            RowVersion = Convert.ToBase64String(freshPosting.RowVersion)
        };

        _mapperMock
            .Setup(x => x.Map(request, It.IsAny<JobPosting>()))
            .Callback<UpdateJobPostingRequest, JobPosting>((req, post) =>
            {
                post.PositionTitle = req.PositionTitle;
            });

        SetupMapperForPosting();

        // Act
        var result = await _service.UpdatePostingAsync(posting.Id, request, updatedBy);

        // Assert
        result.Should().NotBeNull();

        var updatedPosting = await _dbContext.JobPostings.FirstAsync(p => p.Id == posting.Id);
        updatedPosting.PositionTitle.Should().Be("Senior Software Engineer");
        updatedPosting.UpdatedBy.Should().Be(updatedBy);
    }

    [Fact]
    public async Task UpdatePostingAsync_WhenNotExists_ReturnsNull()
    {
        // Arrange
        var request = new UpdateJobPostingRequest
        {
            PositionTitle = "New Title",
            RowVersion = Convert.ToBase64String(new byte[8])
        };

        // Act
        var result = await _service.UpdatePostingAsync(Guid.NewGuid(), request, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region DeletePostingAsync Tests

    [Fact]
    public async Task DeletePostingAsync_WhenExists_SoftDeletesPosting()
    {
        // Arrange
        var posting = CreateJobPosting("SE-001", isActive: true, publishedAt: DateTime.UtcNow, deadlineInFuture: true);
        await _dbContext.JobPostings.AddAsync(posting);
        await _dbContext.SaveChangesAsync();

        var deletedBy = Guid.NewGuid();

        // Act
        var result = await _service.DeletePostingAsync(posting.Id, deletedBy);

        // Assert
        result.Should().BeTrue();

        // Need to ignore query filters to retrieve soft-deleted entity
        var deletedPosting = await _dbContext.JobPostings
            .IgnoreQueryFilters()
            .FirstAsync(p => p.Id == posting.Id);
        deletedPosting.IsDeleted.Should().BeTrue();
        deletedPosting.UpdatedBy.Should().Be(deletedBy);
    }

    [Fact]
    public async Task DeletePostingAsync_WhenNotExists_ReturnsFalse()
    {
        // Act
        var result = await _service.DeletePostingAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeletePostingAsync_WhenActivePosting_UpdatesMetrics()
    {
        // Arrange
        var posting = CreateJobPosting("SE-001", isActive: true, publishedAt: DateTime.UtcNow, deadlineInFuture: true);
        await _dbContext.JobPostings.AddAsync(posting);
        await _dbContext.SaveChangesAsync();

        // Act
        await _service.DeletePostingAsync(posting.Id, Guid.NewGuid());

        // Assert
        _metricsServiceMock.Verify(
            x => x.SetActiveJobPostings(It.IsAny<int>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private JobPosting CreateJobPosting(string positionCode, bool isActive, DateTime publishedAt, bool deadlineInFuture)
    {
        return new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionCode = positionCode,
            PositionTitle = $"Position {positionCode}",
            Department = "Engineering",
            Location = "Bangkok",
            EmploymentType = "Full-time",
            Description = "Description",
            Requirements = "Requirements",
            Responsibilities = "Responsibilities",
            ApplicationDeadline = deadlineInFuture ? DateTime.UtcNow.AddMonths(1) : DateTime.UtcNow.AddDays(-1),
            PublishedAt = publishedAt,
            IsActive = isActive,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };
    }

    private void SetupMapperForPosting()
    {
        _mapperMock
            .Setup(x => x.Map<JobPostingResponse>(It.IsAny<JobPosting>()))
            .Returns((JobPosting jp) => new JobPostingResponse
            {
                Id = jp.Id,
                PositionCode = jp.PositionCode,
                PositionTitle = jp.PositionTitle,
                Department = jp.Department,
                Location = jp.Location,
                EmploymentType = jp.EmploymentType,
                PublishedAt = jp.PublishedAt,
                ApplicationDeadline = jp.ApplicationDeadline
            });
    }

    #endregion
}
