using AutoMapper;
using FluentAssertions;
using Maliev.CareerService.Api.Models.DevelopmentGoals;
using Maliev.CareerService.Api.Models.DevelopmentPlans;
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
/// Unit tests for DevelopmentPlanService
/// </summary>
public class DevelopmentPlanServiceTests : IDisposable
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IEmployeeServiceClient> _employeeServiceMock;
    private readonly Mock<ILogger<DevelopmentPlanService>> _loggerMock;
    private readonly CareerDbContext _dbContext;
    private readonly DevelopmentPlanService _service;

    public DevelopmentPlanServiceTests()
    {
        var options = new DbContextOptionsBuilder<CareerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CareerDbContext(options);
        _mapperMock = new Mock<IMapper>();
        _employeeServiceMock = new Mock<IEmployeeServiceClient>();
        _loggerMock = new Mock<ILogger<DevelopmentPlanService>>();

        _service = new DevelopmentPlanService(
            _dbContext,
            _mapperMock.Object,
            _employeeServiceMock.Object,
            _loggerMock.Object
        );
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetEmployeeIDPsAsync Tests

    [Fact]
    public async Task GetEmployeeIDPsAsync_ReturnsEmployeeIDPs()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var idp1 = CreateIDP(employeeId, 2024, IDPStatus.Draft);
        var idp2 = CreateIDP(employeeId, 2025, IDPStatus.Submitted);
        var otherEmployeeIdp = CreateIDP(Guid.NewGuid(), 2024, IDPStatus.Draft);

        await _dbContext.IndividualDevelopmentPlans.AddRangeAsync(idp1, idp2, otherEmployeeIdp);
        await _dbContext.SaveChangesAsync();

        SetupMapperForIDP();

        // Act
        var result = await _service.GetEmployeeIDPsAsync(employeeId);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEmployeeIDPsAsync_WithPlanYearFilter_ReturnsMatchingIDPs()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var idp2024 = CreateIDP(employeeId, 2024, IDPStatus.Draft);
        var idp2025 = CreateIDP(employeeId, 2025, IDPStatus.Submitted);

        await _dbContext.IndividualDevelopmentPlans.AddRangeAsync(idp2024, idp2025);
        await _dbContext.SaveChangesAsync();

        SetupMapperForIDP();

        // Act
        var result = await _service.GetEmployeeIDPsAsync(employeeId, planYear: 2024);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetEmployeeIDPsAsync_WithStatusFilter_ReturnsMatchingIDPs()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var draftIdp = CreateIDP(employeeId, 2024, IDPStatus.Draft);
        var submittedIdp = CreateIDP(employeeId, 2025, IDPStatus.Submitted);

        await _dbContext.IndividualDevelopmentPlans.AddRangeAsync(draftIdp, submittedIdp);
        await _dbContext.SaveChangesAsync();

        SetupMapperForIDP();

        // Act
        var result = await _service.GetEmployeeIDPsAsync(employeeId, status: IDPStatus.Draft);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetEmployeeIDPsAsync_OrdersByYearDescending()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var idp2024 = CreateIDP(employeeId, 2024, IDPStatus.Draft);
        var idp2025 = CreateIDP(employeeId, 2025, IDPStatus.Submitted);
        var idp2023 = CreateIDP(employeeId, 2023, IDPStatus.Approved);

        await _dbContext.IndividualDevelopmentPlans.AddRangeAsync(idp2024, idp2025, idp2023);
        await _dbContext.SaveChangesAsync();

        _mapperMock
            .Setup(x => x.Map<List<IDPResponse>>(It.IsAny<List<IndividualDevelopmentPlan>>()))
            .Returns((List<IndividualDevelopmentPlan> idps) => idps.Select(i => new IDPResponse
            {
                Id = i.Id,
                PlanYear = i.PlanYear,
                Status = i.Status
            }).ToList());

        // Act
        var result = await _service.GetEmployeeIDPsAsync(employeeId);

        // Assert
        result.Items[0].PlanYear.Should().Be(2025);
        result.Items[1].PlanYear.Should().Be(2024);
        result.Items[2].PlanYear.Should().Be(2023);
    }

    #endregion

    #region GetIDPByIdAsync Tests

    [Fact]
    public async Task GetIDPByIdAsync_WhenExists_ReturnsIDP()
    {
        // Arrange
        var idp = CreateIDP(Guid.NewGuid(), 2024, IDPStatus.Draft);
        await _dbContext.IndividualDevelopmentPlans.AddAsync(idp);
        await _dbContext.SaveChangesAsync();

        SetupMapperForIDP();

        // Act
        var result = await _service.GetIDPByIdAsync(idp.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(idp.Id);
    }

    [Fact]
    public async Task GetIDPByIdAsync_WhenNotExists_ReturnsNull()
    {
        // Act
        var result = await _service.GetIDPByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateIDPAsync Tests

    [Fact]
    public async Task CreateIDPAsync_WithValidRequest_CreatesIDP()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var request = new CreateIDPRequest
        {
            PlanYear = 2024
        };

        _employeeServiceMock
            .Setup(x => x.GetEmployeeAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeResponse(
                employeeId,
                "John",
                "Doe",
                "john.doe@maliev.com",
                "Engineering",
                "Developer"
            ));

        _mapperMock
            .Setup(x => x.Map<IndividualDevelopmentPlan>(request))
            .Returns(new IndividualDevelopmentPlan
            {
                Id = Guid.NewGuid(),
                PlanYear = request.PlanYear,
                Status = IDPStatus.Draft
            });

        SetupMapperForIDP();

        // Act
        var result = await _service.CreateIDPAsync(employeeId, request);

        // Assert
        result.Should().NotBeNull();

        var savedIdp = await _dbContext.IndividualDevelopmentPlans.FirstOrDefaultAsync();
        savedIdp.Should().NotBeNull();
        savedIdp!.EmployeeId.Should().Be(employeeId);
        savedIdp.PlanYear.Should().Be(2024);
        savedIdp.CreatedBy.Should().Be(employeeId);
        savedIdp.UpdatedBy.Should().Be(employeeId);
    }

    [Fact]
    public async Task CreateIDPAsync_WhenEmployeeNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var request = new CreateIDPRequest
        {
            PlanYear = 2024
        };

        _employeeServiceMock
            .Setup(x => x.GetEmployeeAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeResponse?)null);

        // Act & Assert
        await _service.Invoking(s => s.CreateIDPAsync(employeeId, request))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task CreateIDPAsync_WhenDuplicateYear_ThrowsInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var existingIdp = CreateIDP(employeeId, 2024, IDPStatus.Draft);
        await _dbContext.IndividualDevelopmentPlans.AddAsync(existingIdp);
        await _dbContext.SaveChangesAsync();

        var request = new CreateIDPRequest
        {
            PlanYear = 2024
        };

        _employeeServiceMock
            .Setup(x => x.GetEmployeeAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeResponse(
                employeeId,
                "John",
                "Doe",
                "john.doe@maliev.com",
                "Engineering",
                "Developer"
            ));

        // Act & Assert
        await _service.Invoking(s => s.CreateIDPAsync(employeeId, request))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*2024*already exists*");
    }

    #endregion

    #region UpdateIDPAsync Tests

    [Fact]
    public async Task UpdateIDPAsync_WithValidRequest_UpdatesIDP()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var idp = CreateIDP(employeeId, 2024, IDPStatus.Draft);
        await _dbContext.IndividualDevelopmentPlans.AddAsync(idp);
        await _dbContext.SaveChangesAsync();

        _dbContext.Entry(idp).State = EntityState.Detached;
        var freshIdp = await _dbContext.IndividualDevelopmentPlans.FirstAsync(i => i.Id == idp.Id);

        var request = new UpdateIDPRequest
        {
            RowVersion = Convert.ToBase64String(freshIdp.RowVersion)
        };

        SetupMapperForIDP();

        // Act
        var result = await _service.UpdateIDPAsync(idp.Id, request, employeeId);

        // Assert
        result.Should().NotBeNull();

        var updated = await _dbContext.IndividualDevelopmentPlans.FirstAsync(i => i.Id == idp.Id);
        updated.UpdatedBy.Should().Be(employeeId);
    }

    [Fact]
    public async Task UpdateIDPAsync_WhenNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new UpdateIDPRequest
        {
            RowVersion = Convert.ToBase64String(new byte[8])
        };

        // Act & Assert
        await _service.Invoking(s => s.UpdateIDPAsync(Guid.NewGuid(), request, Guid.NewGuid()))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task UpdateIDPAsync_WhenWrongOwner_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var idp = CreateIDP(ownerId, 2024, IDPStatus.Draft);
        await _dbContext.IndividualDevelopmentPlans.AddAsync(idp);
        await _dbContext.SaveChangesAsync();

        var request = new UpdateIDPRequest
        {
            RowVersion = Convert.ToBase64String(idp.RowVersion)
        };

        // Act & Assert
        await _service.Invoking(s => s.UpdateIDPAsync(idp.Id, request, otherUserId))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*only update your own*");
    }

    [Fact]
    public async Task UpdateIDPAsync_WhenNotDraftStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var idp = CreateIDP(employeeId, 2024, IDPStatus.Submitted);
        await _dbContext.IndividualDevelopmentPlans.AddAsync(idp);
        await _dbContext.SaveChangesAsync();

        var request = new UpdateIDPRequest
        {
            RowVersion = Convert.ToBase64String(idp.RowVersion)
        };

        // Act & Assert
        await _service.Invoking(s => s.UpdateIDPAsync(idp.Id, request, employeeId))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Only Draft IDPs can be updated*");
    }

    [Fact]
    public async Task UpdateIDPAsync_WithStaleRowVersion_ThrowsDbUpdateConcurrencyException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var idp = CreateIDP(employeeId, 2024, IDPStatus.Draft);
        await _dbContext.IndividualDevelopmentPlans.AddAsync(idp);
        await _dbContext.SaveChangesAsync();

        var staleRowVersion = Convert.ToBase64String(new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 });
        var request = new UpdateIDPRequest
        {
            RowVersion = staleRowVersion
        };

        // Act & Assert
        await _service.Invoking(s => s.UpdateIDPAsync(idp.Id, request, employeeId))
            .Should().ThrowAsync<DbUpdateConcurrencyException>()
            .WithMessage("*modified by another user*");
    }

    #endregion

    #region SubmitIDPAsync Tests

    [Fact]
    public async Task SubmitIDPAsync_WithValidRequest_SubmitsIDP()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var idp = CreateIDP(employeeId, 2024, IDPStatus.Draft);
        await _dbContext.IndividualDevelopmentPlans.AddAsync(idp);
        await _dbContext.SaveChangesAsync();

        SetupMapperForIDP();

        // Act
        var result = await _service.SubmitIDPAsync(idp.Id, employeeId);

        // Assert
        result.Should().NotBeNull();

        var submitted = await _dbContext.IndividualDevelopmentPlans.FirstAsync(i => i.Id == idp.Id);
        submitted.Status.Should().Be(IDPStatus.Submitted);
        submitted.SubmittedAt.Should().NotBeNull();
        submitted.UpdatedBy.Should().Be(employeeId);
    }

    [Fact]
    public async Task SubmitIDPAsync_WhenNotFound_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await _service.Invoking(s => s.SubmitIDPAsync(Guid.NewGuid(), Guid.NewGuid()))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task SubmitIDPAsync_WhenWrongOwner_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var idp = CreateIDP(ownerId, 2024, IDPStatus.Draft);
        await _dbContext.IndividualDevelopmentPlans.AddAsync(idp);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await _service.Invoking(s => s.SubmitIDPAsync(idp.Id, otherUserId))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*only submit your own*");
    }

    [Fact]
    public async Task SubmitIDPAsync_WhenNotDraftStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var idp = CreateIDP(employeeId, 2024, IDPStatus.Approved);
        await _dbContext.IndividualDevelopmentPlans.AddAsync(idp);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await _service.Invoking(s => s.SubmitIDPAsync(idp.Id, employeeId))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Only Draft IDPs can be submitted*");
    }

    #endregion

    #region ApproveIDPAsync Tests

    [Fact]
    public async Task ApproveIDPAsync_WithValidRequest_ApprovesIDP()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var hrUserId = Guid.NewGuid();
        var idp = CreateIDP(employeeId, 2024, IDPStatus.Submitted);
        await _dbContext.IndividualDevelopmentPlans.AddAsync(idp);
        await _dbContext.SaveChangesAsync();

        _dbContext.Entry(idp).State = EntityState.Detached;
        var freshIdp = await _dbContext.IndividualDevelopmentPlans.FirstAsync(i => i.Id == idp.Id);

        var request = new ApproveIDPRequest
        {
            RowVersion = Convert.ToBase64String(freshIdp.RowVersion)
        };

        SetupMapperForIDP();

        // Act
        var result = await _service.ApproveIDPAsync(idp.Id, request, hrUserId);

        // Assert
        result.Should().NotBeNull();

        var approved = await _dbContext.IndividualDevelopmentPlans.FirstAsync(i => i.Id == idp.Id);
        approved.Status.Should().Be(IDPStatus.Approved);
        approved.ApprovedAt.Should().NotBeNull();
        approved.ApprovedBy.Should().Be(hrUserId);
        approved.UpdatedBy.Should().Be(hrUserId);
    }

    [Fact]
    public async Task ApproveIDPAsync_WhenNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new ApproveIDPRequest
        {
            RowVersion = Convert.ToBase64String(new byte[8])
        };

        // Act & Assert
        await _service.Invoking(s => s.ApproveIDPAsync(Guid.NewGuid(), request, Guid.NewGuid()))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task ApproveIDPAsync_WhenNotSubmittedStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var idp = CreateIDP(Guid.NewGuid(), 2024, IDPStatus.Draft);
        await _dbContext.IndividualDevelopmentPlans.AddAsync(idp);
        await _dbContext.SaveChangesAsync();

        var request = new ApproveIDPRequest
        {
            RowVersion = Convert.ToBase64String(idp.RowVersion)
        };

        // Act & Assert
        await _service.Invoking(s => s.ApproveIDPAsync(idp.Id, request, Guid.NewGuid()))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Only Submitted IDPs can be approved*");
    }

    [Fact]
    public async Task ApproveIDPAsync_WithStaleRowVersion_ThrowsDbUpdateConcurrencyException()
    {
        // Arrange
        var idp = CreateIDP(Guid.NewGuid(), 2024, IDPStatus.Submitted);
        await _dbContext.IndividualDevelopmentPlans.AddAsync(idp);
        await _dbContext.SaveChangesAsync();

        var staleRowVersion = Convert.ToBase64String(new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 });
        var request = new ApproveIDPRequest
        {
            RowVersion = staleRowVersion
        };

        // Act & Assert
        await _service.Invoking(s => s.ApproveIDPAsync(idp.Id, request, Guid.NewGuid()))
            .Should().ThrowAsync<DbUpdateConcurrencyException>()
            .WithMessage("*modified by another user*");
    }

    #endregion

    #region CheckDuplicateYearAsync Tests

    [Fact]
    public async Task CheckDuplicateYearAsync_WhenExists_ReturnsTrue()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var idp = CreateIDP(employeeId, 2024, IDPStatus.Draft);
        await _dbContext.IndividualDevelopmentPlans.AddAsync(idp);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.CheckDuplicateYearAsync(employeeId, 2024);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckDuplicateYearAsync_WhenNotExists_ReturnsFalse()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var idp = CreateIDP(employeeId, 2024, IDPStatus.Draft);
        await _dbContext.IndividualDevelopmentPlans.AddAsync(idp);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.CheckDuplicateYearAsync(employeeId, 2025);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private IndividualDevelopmentPlan CreateIDP(Guid employeeId, int planYear, string status)
    {
        return new IndividualDevelopmentPlan
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            PlanYear = planYear,
            Status = status,
            Goals = new List<EmployeeDevelopmentGoal>(),
            CreatedBy = employeeId,
            UpdatedBy = employeeId
        };
    }

    private void SetupMapperForIDP()
    {
        _mapperMock
            .Setup(x => x.Map<IDPResponse>(It.IsAny<IndividualDevelopmentPlan>()))
            .Returns((IndividualDevelopmentPlan idp) => new IDPResponse
            {
                Id = idp.Id,
                EmployeeId = idp.EmployeeId,
                PlanYear = idp.PlanYear,
                Status = idp.Status,
                Goals = new List<DevelopmentGoalResponse>()
            });

        _mapperMock
            .Setup(x => x.Map<List<IDPResponse>>(It.IsAny<List<IndividualDevelopmentPlan>>()))
            .Returns((List<IndividualDevelopmentPlan> idps) => idps.Select(idp => new IDPResponse
            {
                Id = idp.Id,
                EmployeeId = idp.EmployeeId,
                PlanYear = idp.PlanYear,
                Status = idp.Status,
                Goals = new List<DevelopmentGoalResponse>()
            }).ToList());
    }

    #endregion
}
