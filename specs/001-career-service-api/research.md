# Phase 0: Research & Technology Decisions

**Feature**: Career Service Web API
**Date**: 2025-10-21
**Status**: Completed

## Overview

This document captures all technology decisions and their rationales for the Career Service implementation. Each decision considers best practices, MALIEV constitution compliance, and production requirements.

---

## 1. Markdown Rendering and Sanitization

### Decision: Markdig with HtmlSanitizer

**Chosen Solution**:
- **Markdig 0.37.0** for Markdown-to-HTML conversion
- **HtmlSanitizer 8.1.870** for XSS prevention

**Rationale**:
- Markdig is the most popular and actively maintained .NET Markdown library
- Fully compliant with CommonMark specification
- Excellent performance and extensibility
- HtmlSanitizer provides robust XSS protection with configurable whitelists

**Implementation Approach**:
```csharp
// Service for Markdown conversion
public class MarkdownService : IMarkdownService
{
    private readonly MarkdownPipeline _pipeline;
    private readonly HtmlSanitizer _sanitizer;

    public MarkdownService()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .DisableHtml() // Block raw HTML in Markdown
            .Build();

        _sanitizer = new HtmlSanitizer();
        _sanitizer.AllowedTags.Add("h1");
        _sanitizer.AllowedTags.Add("h2");
        _sanitizer.AllowedTags.Add("h3");
        _sanitizer.AllowedTags.Add("p");
        _sanitizer.AllowedTags.Add("ul");
        _sanitizer.AllowedTags.Add("ol");
        _sanitizer.AllowedTags.Add("li");
        _sanitizer.AllowedTags.Add("strong");
        _sanitizer.AllowedTags.Add("em");
        _sanitizer.AllowedTags.Add("a");
        _sanitizer.AllowedAttributes.Add("href");
    }

    public string ConvertToHtml(string markdown)
    {
        var html = Markdown.ToHtml(markdown, _pipeline);
        return _sanitizer.Sanitize(html);
    }
}
```

**Allowed Markdown Syntax**:
- Headings (H1-H3)
- Paragraphs
- Bold and italic text
- Ordered and unordered lists
- Links (sanitized)
- NO raw HTML, NO JavaScript, NO iframes

**Alternatives Considered**:
- **CommonMark.NET**: Less actively maintained, fewer features
- **Microsoft.AspNetCore.Mvc.Razor.Markdown**: Not suitable for API-only projects

---

## 2. Rate Limiting Configuration

### Decision: ASP.NET Core 9.0 Built-in Rate Limiting with Fixed Window

**Chosen Solution**:
- Fixed window limiter for general endpoints
- Sliding window limiter for batch operations
- Per-user identification via JWT claims

**Rationale**:
- Built-in rate limiting introduced in ASP.NET Core 7.0, mature in 9.0
- No external dependencies (no NuGet packages needed)
- Supports multiple algorithms and partition keys
- Integrates seamlessly with authentication middleware

**Implementation Approach**:
```csharp
builder.Services.AddRateLimiter(options =>
{
    // Anonymous users: 100 req/min
    options.AddFixedWindowLimiter("anonymous", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 10;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // Applicants: 200 req/min
    options.AddFixedWindowLimiter("applicant", opt =>
    {
        opt.PermitLimit = 200;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 10;
    });

    // Employees: 300 req/min
    options.AddFixedWindowLimiter("employee", opt =>
    {
        opt.PermitLimit = 300;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 10;
    });

    // HR Staff: 500 req/min
    options.AddFixedWindowLimiter("hr", opt =>
    {
        opt.PermitLimit = 500;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 10;
    });

    // Batch operations: 10 req/min sliding window
    options.AddSlidingWindowLimiter("batch", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 6; // 10-second segments
        opt.QueueLimit = 5;
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            await context.HttpContext.Response.WriteAsync(
                $"Too many requests. Retry after {retryAfter.TotalSeconds} seconds.", token);
        }
    };
});
```

**Partition Key Strategy**:
- Use `userType` claim from JWT to determine rate limit policy
- Health check and metrics endpoints bypass rate limiting (`[DisableRateLimiting]`)
- Unauthenticated requests use anonymous policy

**Alternatives Considered**:
- **AspNetCoreRateLimit**: Third-party library, adds dependency
- **Token bucket**: More complex, fixed window sufficient for use case
- **Distributed rate limiting**: Not needed for single-instance deployment

---

## 3. File Upload Security

### Decision: Multi-layer Validation + Upload Service Integration

**Chosen Solution**:
- Client-side: File extension and size validation
- Server-side: MIME type validation, magic bytes verification
- Upload Service: Virus scanning via ClamAV

**Rationale**:
- Defense in depth with multiple validation layers
- Leverage existing Upload Service for virus scanning
- Prevent malicious file uploads at multiple checkpoints

**Implementation Approach**:
```csharp
public class FileUploadValidator
{
    private static readonly Dictionary<string, List<byte[]>> _fileSignatures = new()
    {
        { ".pdf", new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 } } }, // %PDF
        { ".docx", new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } }, // PK..
        { ".doc", new List<byte[]> { new byte[] { 0xD0, 0xCF, 0x11, 0xE0 } } } // DOC
    };

    public async Task<ValidationResult> ValidateAsync(IFormFile file)
    {
        // Size validation
        if (file.Length > 10 * 1024 * 1024) // 10MB
            return ValidationResult.Error("File exceeds 10MB limit");

        // Extension validation
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!new[] { ".pdf", ".doc", ".docx" }.Contains(extension))
            return ValidationResult.Error("Invalid file type");

        // Magic bytes validation
        using var stream = file.OpenReadStream();
        var headerBytes = new byte[8];
        await stream.ReadAsync(headerBytes, 0, 8);

        if (!_fileSignatures.TryGetValue(extension, out var validSignatures))
            return ValidationResult.Error("Unknown file type");

        var isValid = validSignatures.Any(sig =>
            headerBytes.Take(sig.Length).SequenceEqual(sig));

        if (!isValid)
            return ValidationResult.Error("File content does not match extension");

        return ValidationResult.Success();
    }
}
```

**Upload Flow**:
1. Client validates extension and size before upload
2. API validates MIME type and magic bytes
3. API forwards to Upload Service via IUploadServiceClient
4. Upload Service scans with ClamAV and stores securely
5. API stores file reference URL returned from Upload Service

**Limits**:
- Max 5 files per application
- Max 10MB per file
- Max 25MB total combined size

**Alternatives Considered**:
- **Built-in virus scanning**: Complex, Upload Service already provides this
- **Content-Type validation only**: Easily spoofed, insufficient

---

## 4. External LMS Integration

### Decision: URL Reference Storage with Manual Completion Tracking

**Chosen Solution**:
- Store LMS content URLs in TrainingProgram entity
- HR staff manually marks completion after verifying in LMS
- No automated LMS API integration

**Rationale**:
- Clarification from user: Manual tracking preferred over automated sync
- Reduces coupling with external LMS system
- Simpler implementation and fewer integration points
- HR maintains control over completion verification

**Implementation Approach**:
```csharp
public class TrainingProgram
{
    [Column("external_content_url")]
    [MaxLength(2000)]
    public string? ExternalContentUrl { get; set; } // URL to LMS content

    [Column("lms_course_id")]
    [MaxLength(255)]
    public string? LmsCourseId { get; set; } // Reference ID in external LMS
}

public class Enrollment
{
    [Column("status")]
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = EnrollmentStatus.Enrolled;

    [Column("completion_date")]
    public DateTime? CompletionDate { get; set; }

    [Column("completed_by_hr_id")]
    public Guid? CompletedByHrId { get; set; } // HR staff who marked complete
}
```

**URL Validation**:
- Validate URL format (must be valid URI)
- Optional: HEAD request to verify URL reachability
- Store LMS course ID for cross-reference

**Completion Workflow**:
1. Employee enrolls in training program via Career Service
2. Employee accesses training via external LMS link
3. Employee completes training in external LMS
4. HR staff verifies completion in LMS
5. HR staff marks enrollment complete in Career Service
6. Completion recorded with HR staff ID and timestamp

**Alternatives Considered**:
- **Polling LMS API**: Complex, requires LMS credentials
- **LMS webhook integration**: Each LMS has different webhook formats
- **SCORM player integration**: Out of scope, LMS already handles content delivery

---

## 5. Email Notification Service

### Decision: Integration with Existing Email Service (Future)

**Chosen Solution**:
- Assume existing Email Service at MALIEV (similar to Upload Service pattern)
- HTTP client integration with retry policies
- Email templates stored in Email Service

**Rationale**:
- Consistent with MALIEV microservices architecture
- Centralized email infrastructure for all services
- Email Service handles templates, delivery tracking, and bounce handling

**Implementation Approach**:
```csharp
public interface IEmailServiceClient
{
    Task<bool> SendApplicationStatusChangeAsync(
        string toEmail,
        string applicantName,
        string jobTitle,
        string newStatus,
        CancellationToken cancellationToken = default);

    Task<bool> SendAccountVerificationAsync(
        string toEmail,
        string verificationLink,
        CancellationToken cancellationToken = default);
}

public class EmailServiceClient : IEmailServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmailServiceClient> _logger;

    // POST /emails/api/v1/send
    public async Task<bool> SendApplicationStatusChangeAsync(...)
    {
        var request = new
        {
            template = "application-status-change",
            to = toEmail,
            variables = new
            {
                applicant_name = applicantName,
                job_title = jobTitle,
                status = newStatus
            }
        };

        var response = await _httpClient.PostAsJsonAsync("/emails/api/v1/send", request);
        return response.IsSuccessStatusCode;
    }
}
```

**Configuration**:
- `ExternalServices__EmailService__BaseUrl` from Google Secret Manager
- Standard 180-second timeout
- Polly retry policy (3 attempts, exponential backoff)

**Email Triggers**:
- Application status change
- Account verification
- Application submission confirmation
- Enrollment confirmation (optional)

**Alternatives Considered**:
- **SendGrid**: Third-party dependency, monthly costs
- **AWS SES**: Vendor lock-in, additional AWS account needed
- **Built-in SMTP**: Not recommended for production, no delivery tracking

---

## 6. Metrics Collection

### Decision: prometheus-net with Business Metrics

**Chosen Solution**:
- **prometheus-net 8.2.1** for Prometheus client library
- Custom business metrics via Counter, Gauge, Histogram
- Automatic ASP.NET Core metrics via UseHttpMetrics()

**Rationale**:
- Prometheus is the standard metrics format for Kubernetes
- prometheus-net is the official .NET client library
- Integrates seamlessly with Grafana for visualization
- Constitution Principle XI compliance (Business Metrics mandatory)

**Implementation Approach**:
```csharp
public class CareerMetrics
{
    // Recruitment metrics
    public static readonly Counter ApplicationsReceived = Metrics.CreateCounter(
        "career_applications_received_total",
        "Total number of job applications received",
        new CounterConfiguration { LabelNames = new[] { "job_posting_id", "status" } });

    public static readonly Gauge ActiveJobPostings = Metrics.CreateGauge(
        "career_active_job_postings",
        "Number of currently active job postings");

    public static readonly Histogram ApplicationProcessingTime = Metrics.CreateHistogram(
        "career_application_processing_seconds",
        "Time to process application from submission to first status change");

    // Learning metrics
    public static readonly Counter TrainingEnrollments = Metrics.CreateCounter(
        "career_training_enrollments_total",
        "Total number of training program enrollments",
        new CounterConfiguration { LabelNames = new[] { "program_id", "category" } });

    public static readonly Gauge TrainingCompletionRate = Metrics.CreateGauge(
        "career_training_completion_rate",
        "Percentage of enrollments that reach completion",
        new GaugeConfiguration { LabelNames = new[] { "program_id" } });

    // System performance metrics (automatic via UseHttpMetrics)
    // - http_request_duration_seconds
    // - http_requests_in_progress
    // - http_requests_received_total
}

// Program.cs
app.UseHttpMetrics(); // Automatic HTTP metrics
app.MapMetrics(); // Expose /metrics endpoint
```

**Metrics Endpoint**: `GET /metrics`
- Prometheus text format
- Accessible by monitoring infrastructure only (no authentication required)
- Rate limiting bypassed for metrics endpoint

**Tags**:
- service_name: "maliev-career-service"
- version: From assembly version
- environment: From ASPNETCORE_ENVIRONMENT

**Alternatives Considered**:
- **App.Metrics**: More features but heavier, Prometheus client sufficient
- **OpenTelemetry**: Future consideration, Prometheus baseline for now

---

## 7. State Machine for Application Status

### Decision: Hand-rolled State Machine with Explicit Transitions

**Chosen Solution**:
- Constants class for status values
- Dictionary-based transition rules
- Validation in service layer before updates

**Rationale**:
- Simple requirements don't justify external library
- Explicit control over business rules
- Easy to test and understand
- No additional dependencies

**Implementation Approach**:
```csharp
public static class ApplicationStatus
{
    public const string Submitted = "Submitted";
    public const string UnderReview = "UnderReview";
    public const string InterviewScheduled = "InterviewScheduled";
    public const string Rejected = "Rejected";
    public const string OfferExtended = "OfferExtended";
    public const string Hired = "Hired";

    public static readonly string[] All =
    {
        Submitted, UnderReview, InterviewScheduled,
        Rejected, OfferExtended, Hired
    };

    // Terminal states cannot be changed
    public static readonly string[] TerminalStates = { Hired };

    // Allowed transitions
    public static readonly Dictionary<string, string[]> AllowedTransitions = new()
    {
        [Submitted] = new[] { UnderReview, Rejected },
        [UnderReview] = new[] { InterviewScheduled, Rejected, Submitted }, // Can revert to Submitted
        [InterviewScheduled] = new[] { OfferExtended, Rejected, UnderReview }, // Can revert
        [Rejected] = new[] { UnderReview }, // Can reconsider (clarification: reversible)
        [OfferExtended] = new[] { Hired, Rejected } // Offer can be rejected
    };
}

public class ApplicationService
{
    private void ValidateStatusTransition(Application application, string newStatus)
    {
        // Terminal state check
        if (ApplicationStatus.TerminalStates.Contains(application.Status))
        {
            throw new InvalidOperationException(
                $"Cannot change application status from terminal state '{application.Status}'");
        }

        // Transition validation
        if (!ApplicationStatus.AllowedTransitions.TryGetValue(application.Status, out var allowed) ||
            !allowed.Contains(newStatus))
        {
            throw new InvalidOperationException(
                $"Cannot transition application from '{application.Status}' to '{newStatus}'");
        }
    }
}
```

**Audit Trail**:
- All status changes logged with timestamp, HR staff ID, old status, new status
- Separate ApplicationStatusHistory table for full audit trail

**Alternatives Considered**:
- **Stateless library**: Overkill for simple state machine
- **Workflow Foundation**: Deprecated, not for .NET Core

---

## 8. Optimistic Concurrency with PostgreSQL

### Decision: EF Core RowVersion with Base64 Serialization

**Chosen Solution**:
- `byte[]` RowVersion property with `[Timestamp]` attribute
- PostgreSQL `bytea` column with default value
- Base64 encoding for API requests/responses

**Rationale**:
- EF Core built-in support for optimistic concurrency
- PostgreSQL xmin can be used but bytea is more explicit
- Base64 encoding standard for binary data in JSON

**Implementation Approach**:
```csharp
// Entity
public class Application
{
    [Column("version")]
    [Timestamp]
    public byte[] Version { get; set; } = null!;
}

// Configuration
public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.Property(a => a.Version)
            .HasColumnName("version")
            .IsRowVersion()
            .HasDefaultValueSql("'\\x0000000000000000'::bytea")
            .ValueGeneratedOnAddOrUpdate()
            .IsRequired();
    }
}

// DTO
public class UpdateApplicationRequest
{
    public string Version { get; set; } = string.Empty; // Base64-encoded
}

// Service
public async Task UpdateAsync(Guid id, UpdateApplicationRequest request)
{
    var application = await _context.Applications.FindAsync(id);
    if (application == null)
        throw new KeyNotFoundException($"Application {id} not found");

    // Validate Base64 format
    byte[] requestVersion;
    try
    {
        requestVersion = Convert.FromBase64String(request.Version);
    }
    catch (FormatException)
    {
        throw new InvalidOperationException(
            $"Invalid version format. Version must be a valid Base64 string.");
    }

    // Check concurrency
    if (!application.Version.SequenceEqual(requestVersion))
    {
        throw new DbUpdateConcurrencyException(
            "Application has been modified by another user");
    }

    // Update and save
    application.Status = request.Status;
    await _context.SaveChangesAsync(); // EF Core handles version update
}
```

**Error Handling**:
- HTTP 409 Conflict for concurrency violations
- HTTP 400 Bad Request for invalid Base64
- Client must retry with latest version

**Alternatives Considered**:
- **PostgreSQL xmin**: Less portable, implicit
- **Timestamp column**: Not reliable with clock skew

---

## 9. Pagination and Filtering

### Decision: Offset-based Pagination with Dynamic Filtering

**Chosen Solution**:
- Offset-based pagination (`page`, `pageSize` query parameters)
- Dynamic LINQ for flexible filtering
- Max page size: 100 items

**Rationale**:
- Simpler for clients (jump to any page)
- No cursor state management
- Sufficient for expected dataset sizes (<10,000 records)
- Dynamic LINQ allows flexible filtering without hardcoded queries

**Implementation Approach**:
```csharp
public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public async Task<PaginatedResponse<JobPostingResponse>> GetAllAsync(
    int page = 1,
    int pageSize = 20,
    string? location = null,
    string? department = null,
    string? jobType = null)
{
    // Validate and clamp page size
    pageSize = Math.Clamp(pageSize, 1, 100);
    page = Math.Max(page, 1);

    var query = _context.JobPostings
        .Where(j => j.Status == JobPostingStatus.Active)
        .AsNoTracking(); // Performance optimization

    // Apply filters
    if (!string.IsNullOrEmpty(location))
        query = query.Where(j => j.Location.Contains(location));

    if (!string.IsNullOrEmpty(department))
        query = query.Where(j => j.Department == department);

    if (!string.IsNullOrEmpty(jobType))
        query = query.Where(j => j.JobType == jobType);

    var totalCount = await query.CountAsync();
    var skip = (page - 1) * pageSize;

    var items = await query
        .OrderByDescending(j => j.PostedDate)
        .Skip(skip)
        .Take(pageSize)
        .ToListAsync();

    return new PaginatedResponse<JobPostingResponse>
    {
        Items = items.Select(MapToResponse).ToList(),
        Page = page,
        PageSize = pageSize,
        TotalCount = totalCount
    };
}
```

**Performance Optimization**:
- Always use `AsNoTracking()` for read-only queries
- Indexes on filtered columns (location, department, job_type)
- Consider query result caching for frequently accessed data

**Alternatives Considered**:
- **Cursor-based pagination**: More complex, better for infinite scroll
- **GraphQL**: Out of scope for REST API project

---

## 10. Testing with PostgreSQL

### Decision: Testcontainers.PostgreSql with Per-Test-Class Isolation

**Chosen Solution**:
- Testcontainers.PostgreSql for automatic container management
- IAsyncLifetime for container lifecycle
- Per-test-class database container
- DELETE cleanup between tests

**Rationale**:
- Perfect test isolation with dedicated containers
- No Docker Compose manual setup required
- Automatic cleanup and disposal
- Matches production PostgreSQL version (18)

**Implementation Approach**:
```csharp
using Testcontainers.PostgreSql;

public class TestDatabaseFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:18-alpine")
            .WithDatabase("career_test_db")
            .WithUsername("postgres")
            .WithPassword("test_password")
            .Build();

        await _postgresContainer.StartAsync();
        ConnectionString = _postgresContainer.GetConnectionString();

        // Apply migrations
        await using var context = CreateDbContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_postgresContainer != null)
        {
            await _postgresContainer.DisposeAsync();
        }
    }

    public CareerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CareerDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new CareerDbContext(options);
    }

    public async Task CleanupAsync()
    {
        await using var context = CreateDbContext();

        // Delete in correct order (child tables first)
        await context.Database.ExecuteSqlRawAsync("DELETE FROM enrollments");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM development_plans");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM applications");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM applicants");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM training_programs");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM job_postings");

        context.ChangeTracker.Clear();
    }
}

// Test class
public class JobPostingServiceTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public JobPostingServiceTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateJobPosting_ValidRequest_ReturnsCreatedPosting()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var service = new JobPostingService(context, _logger);
        var request = new CreateJobPostingRequest { /* ... */ };

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();

        // Cleanup for next test
        await _fixture.CleanupAsync();
    }
}
```

**NuGet Packages**:
- Testcontainers 3.7.0
- Testcontainers.PostgreSql 3.7.0

**CI Configuration**:
- Testcontainers requires Docker daemon access in GitHub Actions
- Use `services: docker:dind` in workflow or install Docker
- Testcontainers handles everything else automatically

**Alternatives Considered**:
- **Docker Compose**: Manual setup, developers must remember to start/stop
- **Shared PostgreSQL container**: Test pollution, harder to debug
- **In-memory database**: Prohibited by Constitution Principle IV

---

## Summary of Technology Decisions

| Area | Decision | Library/Approach |
|------|----------|------------------|
| Markdown Rendering | Markdig + HtmlSanitizer | Markdig 0.37.0, HtmlSanitizer 8.1.870 |
| Rate Limiting | ASP.NET Core built-in | Fixed + sliding window algorithms |
| File Upload Security | Multi-layer validation | Magic bytes + Upload Service |
| External LMS | URL references, manual tracking | No automated integration |
| Email Notifications | Email Service integration | HTTP client pattern |
| Metrics Collection | Prometheus | prometheus-net 8.2.1 |
| State Machine | Hand-rolled | Constants + Dictionary |
| Optimistic Concurrency | EF Core RowVersion | byte[] with Base64 API |
| Pagination | Offset-based | page/pageSize parameters |
| Testing | Testcontainers | PostgreSQL 18 containers |

**All decisions align with MALIEV Constitution and production requirements.**

---

**Phase 0 Complete** ✅
**Next Phase**: Design & Contracts (data-model.md, contracts/, quickstart.md)
