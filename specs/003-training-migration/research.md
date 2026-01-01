# Research: Training Migration Implementation Patterns

**Feature**: Training Records and Skills Migration
**Date**: 2025-12-28
**Branch**: `003-training-migration`

## Overview

This document records research findings on existing Career Service patterns to ensure the training migration implementation follows established conventions and best practices.

## Research Questions & Answers

### 1. Background Service Scheduling

**Question**: What scheduling library/pattern does Career Service use for daily jobs?

**Answer**: The service uses .NET's built-in `BackgroundService` pattern. There are currently NO recurring/scheduled background services with timers or cron jobs. The only existing background service is IAM registration which runs once on startup with retry logic.

**Decision**: Implement certification expiration and overdue training checks using `BackgroundService` with `PeriodicTimer` for daily execution.

**Rationale**:
- Consistent with .NET best practices
- No additional dependencies needed
- Simple scheduling with `PeriodicTimer` (available since .NET 6)
- Easy to test with Testcontainers

**Implementation Pattern**:
```csharp
public class CertificationExpirationReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<CertificationExpirationReminderBackgroundService> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for service to fully start
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));

        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            await CheckExpiringCertificationsAsync(stoppingToken);
        }
    }

    private async Task CheckExpiringCertificationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CareerDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationServiceClient>();

        try
        {
            var now = DateTime.UtcNow;
            var expiringIn30Days = now.AddDays(30);
            var expiringIn60Days = now.AddDays(60);
            var expiringIn90Days = now.AddDays(90);

            // Find certifications expiring in 30/60/90 days
            var expiring = await dbContext.TrainingRecords
                .Where(tr => tr.ExpirationDate != null &&
                            tr.ExpirationDate >= now &&
                            tr.ExpirationDate <= expiringIn90Days)
                .ToListAsync(cancellationToken);

            foreach (var record in expiring)
            {
                var daysUntilExpiration = (record.ExpirationDate.Value - now).Days;

                await notificationService.SendCertificationReminderAsync(
                    record.EmployeeId,
                    record.CourseName,
                    record.ExpirationDate.Value,
                    daysUntilExpiration,
                    cancellationToken);
            }

            _logger.LogInformation(
                "Processed {Count} expiring certifications",
                expiring.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check expiring certifications");
            // Don't throw - allow timer to continue
        }
    }
}
```

**Registration**:
```csharp
// In Program.cs
builder.Services.AddHostedService<CertificationExpirationReminderBackgroundService>();
builder.Services.AddHostedService<OverdueTrainingEscalationBackgroundService>();
```

**Alternatives Considered**:
- Hangfire: Rejected - adds complexity and external dependency
- Quartz.NET: Rejected - overkill for simple daily jobs
- Azure Functions/AWS Lambda: Rejected - requires separate deployment

---

### 2. Notification System

**Question**: How does Career Service send notifications to employees and managers?

**Answer**: Notifications are sent via HTTP calls to a dedicated NotificationService using typed HttpClient. The pattern uses:
- Template-based emails with structured data
- Fire-and-forget pattern for non-blocking notifications
- Retry logic via Polly (AddStandardResilienceHandler)

**Decision**: Create INotificationServiceClient with training-specific notification methods using the same HTTP client pattern.

**Rationale**:
- Consistent with existing email notification pattern
- Reuses service discovery and resilience infrastructure
- Template-based approach is maintainable
- Fire-and-forget prevents blocking operations

**Implementation Pattern**:
```csharp
public interface INotificationServiceClient
{
    Task SendCertificationReminderAsync(
        Guid employeeId,
        string courseName,
        DateTime expirationDate,
        int daysUntilExpiration,
        CancellationToken cancellationToken = default);

    Task SendTrainingCompletionNotificationAsync(
        Guid employeeId,
        Guid? managerId,
        string courseName,
        DateTime completionDate,
        CancellationToken cancellationToken = default);

    Task SendMandatoryTrainingReminderAsync(
        Guid employeeId,
        string trainingName,
        DateTime dueDate,
        int daysOverdue,
        CancellationToken cancellationToken = default);
}

public class NotificationServiceClient : INotificationServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationServiceClient> _logger;

    public async Task SendCertificationReminderAsync(
        Guid employeeId,
        string courseName,
        DateTime expirationDate,
        int daysUntilExpiration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new SendEmailRequest(
                To: employeeId,  // Notification service resolves email from ID
                TemplateId: GetReminderTemplateId(daysUntilExpiration),
                TemplateData: new Dictionary<string, object>
                {
                    ["courseName"] = courseName,
                    ["expirationDate"] = expirationDate.ToString("yyyy-MM-dd"),
                    ["daysRemaining"] = daysUntilExpiration
                });

            var response = await _httpClient.PostAsJsonAsync(
                "/emails/v1/send",
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            _logger.LogInformation(
                "Sent certification reminder to employee {EmployeeId} for course {Course}",
                employeeId,
                courseName);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to send certification reminder");
            throw; // Will be caught by background service
        }
    }

    private static string GetReminderTemplateId(int daysRemaining) => daysRemaining switch
    {
        <= 30 => "certification-expiring-30days",
        <= 60 => "certification-expiring-60days",
        _ => "certification-expiring-90days"
    };

    private record SendEmailRequest(
        Guid To,
        string TemplateId,
        Dictionary<string, object> TemplateData);
}
```

**Registration**:
```csharp
// In Program.cs
builder.AddServiceClient<INotificationServiceClient, NotificationServiceClient>("NotificationService");
```

**Template IDs to Create**:
- `certification-expiring-30days`
- `certification-expiring-60days`
- `certification-expiring-90days`
- `training-completed`
- `mandatory-training-overdue`

**Alternatives Considered**:
- Direct SMTP: Rejected - NotificationService provides central template management
- Event-based notifications: Rejected - adds unnecessary complexity
- In-app notifications only: Rejected - email required for compliance

---

### 3. Employee Data Access

**Question**: How does Career Service obtain employee department/position information?

**Answer**: Employee information is accessed via HTTP calls to EmployeeService using typed HttpClient (IEmployeeServiceClient). Data is NOT cached - always fetched on-demand.

**Decision**: Reuse existing IEmployeeServiceClient. Extend EmployeeResponse to include ManagerId field for authorization checks.

**Rationale**:
- Client already exists and is working
- On-demand fetching ensures data accuracy
- Standard resilience handler provides retry logic
- No caching means no stale data issues

**Implementation Pattern**:
```csharp
// Extend existing EmployeeResponse (coordinate with Employee Service team)
public record EmployeeResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Department,
    string? Position,
    Guid? ManagerId,         // NEW - needed for manager authorization
    Guid? DepartmentId,      // NEW - needed for mandatory training assignment
    Guid? PositionId);       // NEW - needed for mandatory training assignment

// Usage in authorization check
public async Task<bool> CanManagerAccessAsync(
    Guid managerId,
    Guid employeeId,
    CancellationToken cancellationToken)
{
    var employee = await _employeeService.GetEmployeeAsync(employeeId, cancellationToken);

    if (employee == null)
    {
        _logger.LogWarning("Employee {EmployeeId} not found", employeeId);
        return false;
    }

    return employee.ManagerId == managerId;
}

// Usage in mandatory training assignment
public async Task AssignMandatoryTrainingAsync(
    Guid employeeId,
    CancellationToken cancellationToken)
{
    var employee = await _employeeService.GetEmployeeAsync(employeeId, cancellationToken);

    if (employee == null)
    {
        _logger.LogWarning("Cannot assign mandatory training - employee {EmployeeId} not found", employeeId);
        return;
    }

    // Find applicable mandatory training requirements
    var requirements = await _dbContext.MandatoryTrainingRequirements
        .Where(mtr => mtr.IsActive &&
                     (mtr.DepartmentId == null || mtr.DepartmentId == employee.DepartmentId) &&
                     (mtr.PositionId == null || mtr.PositionId == employee.PositionId))
        .ToListAsync(cancellationToken);

    // Create enrollments for each requirement
    foreach (var req in requirements)
    {
        // Create enrollment with deadline
        var deadline = DateTime.UtcNow.AddDays(req.CompletionDeadlineDays);
        // ... enrollment logic
    }
}
```

**Error Handling**:
```csharp
try
{
    var employee = await _employeeService.GetEmployeeAsync(employeeId, cancellationToken);
    if (employee == null)
    {
        _logger.LogWarning("Employee {EmployeeId} not found", employeeId);
        // Handle gracefully - don't crash
        return NotFound(new { error = "Employee not found" });
    }
}
catch (HttpRequestException ex)
{
    _logger.LogError(ex, "Failed to fetch employee {EmployeeId} from Employee Service", employeeId);
    return StatusCode(503, new { error = "Employee Service unavailable" });
}
```

**Alternatives Considered**:
- Local employee table cache: Rejected - creates data sync issues
- Event-driven employee sync: Rejected - adds complexity for marginal benefit
- Batch employee fetch endpoint: Consider for future optimization if needed

---

### 4. Manager Hierarchy

**Question**: How does Career Service determine manager-employee relationships for authorization checks?

**Answer**: Manager-employee relationships are NOT tracked locally. Authorization uses permission-based access control with wildcard matching. The `ViewTeam` permission indicates manager privileges.

**Decision**: Implement combined approach:
- Permission-based authorization (`career.training.view-team` for managers)
- Runtime manager verification via EmployeeService for fine-grained access control

**Rationale**:
- Consistent with existing authorization patterns
- No local hierarchy tracking reduces complexity
- EmployeeService is source of truth
- Wildcard permissions allow flexible access control

**Implementation Pattern**:
```csharp
// Define permissions
public static class CareerPermissions
{
    public static class Training
    {
        public const string ViewOwn = "career.training.view-own";
        public const string ViewTeam = "career.training.view-team";  // For managers
        public const string Manage = "career.training.manage";       // For HR admins
    }
}

// Controller with permission check
[HttpGet("employees/{employeeId:guid}/training-records")]
[RequirePermission(CareerPermissions.Training.ViewOwn)]
public async Task<ActionResult<List<TrainingRecordResponse>>> GetTrainingRecords(
    Guid employeeId,
    CancellationToken cancellationToken)
{
    // Check access: Employee sees own, manager sees team, HR sees all
    var permissions = User.FindAll("permissions").Select(c => c.Value).ToList();
    var userId = GetAuthenticatedUserId();

    bool hasAccess = false;

    // HR with wildcard can see all
    if (PermissionMatcher.Match("Permission:career.*", permissions))
    {
        hasAccess = true;
    }
    // Manager with view-team must be the employee's manager
    else if (PermissionMatcher.Match($"Permission:{CareerPermissions.Training.ViewTeam}", permissions))
    {
        hasAccess = await IsManagerOfEmployeeAsync(userId, employeeId, cancellationToken);
    }
    // Employee can see own
    else if (employeeId == userId)
    {
        hasAccess = true;
    }

    if (!hasAccess)
    {
        return Forbid();
    }

    var records = await _trainingRecordService.GetByEmployeeIdAsync(employeeId, cancellationToken);
    return Ok(records);
}

private async Task<bool> IsManagerOfEmployeeAsync(
    Guid managerId,
    Guid employeeId,
    CancellationToken cancellationToken)
{
    try
    {
        var employee = await _employeeService.GetEmployeeAsync(employeeId, cancellationToken);
        return employee?.ManagerId == managerId;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to verify manager relationship");
        return false; // Deny access on error
    }
}
```

**Permission Registration**:
```csharp
// In IAM registration service
new PermissionRegistration(
    CareerPermissions.Training.ViewOwn,
    "View own training records"),
new PermissionRegistration(
    CareerPermissions.Training.ViewTeam,
    "View team training records (managers)"),
new PermissionRegistration(
    CareerPermissions.Training.Manage,
    "Manage all training records (HR)"),
```

**Alternatives Considered**:
- Local manager table: Rejected - creates data duplication
- Claims-based team membership: Rejected - JWT would be too large
- Manager-only endpoints: Rejected - less flexible than permission-based

---

### 5. Event Retry Mechanism

**Question**: What is the standard retry/queue pattern for failed event processing in Career Service?

**Answer**: MassTransit is registered and configured but NO consumers or retry policies are currently implemented. The infrastructure is ready but unused in the codebase.

**Decision**: Do NOT implement event-based communication for training migration. Use HTTP-only approach for consistency with existing patterns.

**Rationale**:
- Career Service has ZERO event consumers currently
- HTTP client pattern is well-established and proven
- Adding events would introduce new testing complexity
- Synchronous communication is simpler and sufficient
- Can add events later if async communication becomes necessary

**Recommendation**: If events are needed in the future, use this pattern:
```csharp
// Registration in Program.cs
builder.AddMassTransitWithRabbitMq(cfg =>
{
    cfg.AddConsumer<EmployeeCreatedConsumer>();

    cfg.UsingRabbitMq((context, rabbitCfg) =>
    {
        rabbitCfg.UseMessageRetry(r => r.Intervals(
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(5)
        ));

        rabbitCfg.ConfigureEndpoints(context);
    });
});

// Consumer with retry
public class EmployeeCreatedConsumer : IConsumer<EmployeeCreated>
{
    public async Task Consume(ConsumeContext<EmployeeCreated> context)
    {
        var message = context.Message;

        // If this throws, MassTransit will retry based on policy
        await AssignMandatoryTrainingAsync(message.EmployeeId, context.CancellationToken);
    }
}
```

**Alternatives Considered**:
- Event-driven architecture: Rejected - not needed for current requirements
- Webhook-based integration: Rejected - adds complexity
- Polling Employee Service: Rejected - inefficient

**Final Decision**: Stick with HTTP-only. No events needed for this migration.

---

### 6. Audit Field Population

**Question**: How are CreatedBy/ModifiedBy fields populated in existing Career Service entities?

**Answer**: Audit fields are populated by:
1. **DbContext.SaveChanges override** - automatically sets CreatedAt/UpdatedAt timestamps
2. **Application layer (services)** - manually sets CreatedBy/UpdatedBy user IDs
3. **Controllers** - extract user ID from JWT claims ("sub" claim)

**Decision**: Follow the exact same pattern. All training entities inherit from BaseEntity and use DbContext override for timestamps + manual user ID population in services.

**Rationale**:
- Well-established pattern across all existing entities
- Clear separation of concerns
- DbContext handles technical concerns (timestamps, row versioning)
- Services handle business concerns (user IDs)
- No magic middleware - explicit is better

**Implementation Pattern**:
```csharp
// Entity inherits from BaseEntity
public class TrainingRecord : BaseEntity
{
    // Domain properties
    public Guid EmployeeId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    // ... other fields

    // BaseEntity provides:
    // - Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted, RowVersion
}

// Service manually sets user IDs
public async Task<TrainingRecordResponse> RecordCompletionAsync(
    RecordTrainingCompletionRequest request,
    Guid hrUserId,  // Passed from controller
    CancellationToken cancellationToken)
{
    var record = request.ToTrainingRecord();

    // MANUAL user ID population
    record.CreatedBy = hrUserId;
    record.UpdatedBy = hrUserId;

    _dbContext.TrainingRecords.Add(record);

    // DbContext.SaveChanges will automatically set:
    // - CreatedAt = DateTime.UtcNow
    // - UpdatedAt = DateTime.UtcNow
    // - RowVersion = 1
    await _dbContext.SaveChangesAsync(cancellationToken);

    return record.ToResponse();
}

// Controller extracts user ID from JWT
[HttpPost("employees/{employeeId:guid}/training-records")]
[RequirePermission(CareerPermissions.Training.Manage)]
public async Task<ActionResult<TrainingRecordResponse>> RecordTrainingCompletion(
    Guid employeeId,
    [FromBody] RecordTrainingCompletionRequest request,
    CancellationToken cancellationToken)
{
    var userId = GetAuthenticatedUserId();
    if (userId == Guid.Empty)
    {
        return Unauthorized(new { error = "User ID not found in claims" });
    }

    var result = await _trainingRecordService.RecordCompletionAsync(
        request,
        userId,  // Pass to service
        cancellationToken);

    return CreatedAtAction(nameof(GetTrainingRecord), new { id = result.Id }, result);
}

private Guid GetAuthenticatedUserId()
{
    // Try "sub" claim first (standard), fallback to legacy claim
    var userIdClaim = User.FindFirst("sub")?.Value ??
                     User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

    return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
}
```

**DbContext already handles timestamps** (no changes needed):
```csharp
// From CareerDbContext.cs - already implemented
public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    UpdateAuditFields();  // Sets CreatedAt/UpdatedAt
    return base.SaveChangesAsync(cancellationToken);
}
```

**Alternatives Considered**:
- Middleware to set audit fields: Rejected - implicit behavior is harder to test
- DbContext interceptor: Rejected - adds complexity
- Repository pattern with audit: Rejected - services already exist

---

## Summary

All patterns are well-documented in existing code. The training migration should:

1. ✅ Use `BackgroundService` with `PeriodicTimer` for daily jobs
2. ✅ Use typed HttpClient for NotificationService integration
3. ✅ Reuse existing IEmployeeServiceClient with extended response
4. ✅ Use permission-based authorization + runtime manager verification
5. ✅ Avoid events - stick with HTTP-only approach
6. ✅ Follow BaseEntity pattern for audit fields

All decisions prioritize consistency with existing architecture over introducing new patterns.

## Next Steps

Proceed to Phase 1: Design
- Generate data-model.md with entity definitions
- Generate API contracts in contracts/ directory
- Generate quickstart.md for developers
- Update agent context with research findings
