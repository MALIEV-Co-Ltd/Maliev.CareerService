using Maliev.CareerService.Api.Mapping;
using Maliev.CareerService.Api.Models.Applications;
using Maliev.CareerService.Api.Models.Common;
using Maliev.CareerService.Api.Services.External;
using Maliev.CareerService.Data;
using Maliev.CareerService.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Api.Services;

/// <summary>
/// Service implementation for managing job applications
/// </summary>
public class ApplicationService(
    CareerDbContext dbContext,
    IMarkdownService markdownService,
    IUploadServiceClient uploadService,
    ICountryServiceClient countryService,
    IEmailServiceClient emailService,
    IEmployeeServiceClient employeeService,
    IMetricsService metricsService,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ApplicationService> logger) : IApplicationService
{
    private readonly CareerDbContext _dbContext = dbContext;
    private readonly IMarkdownService _markdownService = markdownService;
    private readonly IUploadServiceClient _uploadService = uploadService;
    private readonly ICountryServiceClient _countryService = countryService;
    private readonly IEmailServiceClient _emailService = emailService;
    private readonly IEmployeeServiceClient _employeeService = employeeService;
    private readonly IMetricsService _metricsService = metricsService;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<ApplicationService> _logger = logger;

    /// <inheritdoc />
    public async Task<JobApplicationResponse> SubmitApplicationAsync(
        SubmitJobApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate deadline
        if (!await ValidateDeadlineAsync(request.JobPostingId, cancellationToken))
        {
            throw new InvalidOperationException("The application deadline for this job posting has passed.");
        }

        // Check for duplicate application
        if (await ValidateDuplicateAsync(request.JobPostingId, request.ApplicantEmail, cancellationToken))
        {
            throw new InvalidOperationException($"An application from {request.ApplicantEmail} for this job posting already exists.");
        }

        // Validate resume file exists
        if (!await _uploadService.ValidateFileAsync(request.ResumeFileId, cancellationToken))
        {
            throw new ArgumentException($"Resume file {request.ResumeFileId} not found or inaccessible.", nameof(request.ResumeFileId));
        }

        // Validate additional files if provided
        if (request.AdditionalFileIds.Length > 0)
        {
            var fileValidationTasks = request.AdditionalFileIds.Select(fileId =>
                _uploadService.ValidateFileAsync(fileId, cancellationToken));

            var validationResults = await Task.WhenAll(fileValidationTasks);

            var invalidFiles = request.AdditionalFileIds
                .Where((fileId, index) => !validationResults[index])
                .ToList();

            if (invalidFiles.Count != 0)
            {
                throw new ArgumentException($"The following file IDs are not found or inaccessible: {string.Join(", ", invalidFiles)}", nameof(request.AdditionalFileIds));
            }
        }

        // Create application entity
        var application = request.ToJobApplication();

        _dbContext.JobApplications.Add(application);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Track metrics
        _metricsService.IncrementJobApplications(application.Status);

        _logger.LogInformation(
            "Job application {ApplicationId} submitted for job posting {JobPostingId} by {Email}",
            application.Id,
            application.JobPostingId,
            application.ApplicantEmail);

        // Send confirmation email asynchronously (fire-and-forget, don't block submission)
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var scopedDbContext = scope.ServiceProvider.GetRequiredService<CareerDbContext>();
                var scopedEmailService = scope.ServiceProvider.GetRequiredService<IEmailServiceClient>();

                var jobPosting = await scopedDbContext.JobPostings
                    .FirstOrDefaultAsync(jp => jp.Id == request.JobPostingId, cancellationToken);

                if (jobPosting != null)
                {
                    await scopedEmailService.SendApplicationConfirmationAsync(
                        application.ApplicantEmail,
                        $"{application.ApplicantFirstName} {application.ApplicantLastName}",
                        jobPosting.PositionTitle,
                        application.Id.ToString(),
                        cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email for application {ApplicationId}", application.Id);
            }
        }, cancellationToken);

        // Return response with enriched data
        return await MapToResponseAsync(application, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<JobApplicationResponse?> GetApplicationByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var application = await _dbContext.JobApplications
            .Include(ja => ja.JobPosting)
            .FirstOrDefaultAsync(ja => ja.Id == id, cancellationToken);

        if (application == null)
        {
            return null;
        }

        return await MapToResponseAsync(application, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<JobApplicationListResponse> GetApplicantApplicationsAsync(
        string applicantEmail,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var lowerEmail = applicantEmail.ToLower();
        var query = _dbContext.JobApplications
            .Include(ja => ja.JobPosting)
            .Where(ja => ja.ApplicantEmail.ToLower() == lowerEmail)
            .OrderByDescending(ja => ja.AppliedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var applications = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Map all applications to responses with enriched data
        var responseTasks = applications.Select(app => MapToResponseAsync(app, cancellationToken));
        var responses = await Task.WhenAll(responseTasks);

        return new JobApplicationListResponse
        {
            Items = [.. responses],
            Page = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    /// <inheritdoc />
    public async Task<bool> ValidateDuplicateAsync(
        Guid jobPostingId,
        string applicantEmail,
        CancellationToken cancellationToken = default)
    {
        var lowerEmail = applicantEmail.ToLower();
        return await _dbContext.JobApplications
            .AnyAsync(ja =>
                ja.JobPostingId == jobPostingId &&
                ja.ApplicantEmail.ToLower() == lowerEmail,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ValidateDeadlineAsync(
        Guid jobPostingId,
        CancellationToken cancellationToken = default)
    {
        var posting = await _dbContext.JobPostings
            .FirstOrDefaultAsync(jp => jp.Id == jobPostingId, cancellationToken) ?? throw new InvalidOperationException($"Job posting {jobPostingId} not found.");
        return posting.ApplicationDeadline > DateTime.UtcNow;
    }

    /// <summary>
    /// Maps JobApplication entity to JobApplicationResponse with enriched data from external services
    /// </summary>
    private async Task<JobApplicationResponse> MapToResponseAsync(
        JobApplication application,
        CancellationToken cancellationToken)
    {
        var response = application.ToJobApplicationResponse();

        // Get file URLs from Upload Service
        var fileIds = new List<Guid> { application.ResumeFileId };
        fileIds.AddRange(application.AdditionalFileIds);

        try
        {
            var fileUrls = await _uploadService.GetFileUrlsAsync(fileIds, cancellationToken);

            response.ResumeFileUrl = fileUrls.GetValueOrDefault(application.ResumeFileId);
            response.AdditionalFileUrls = application.AdditionalFileIds
                .Select(id => fileUrls.GetValueOrDefault(id))
                .Where(url => url != null)
                .ToArray()!;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get file URLs for application {ApplicationId}", application.Id);
        }

        // Get country name from Country Service
        if (!string.IsNullOrEmpty(application.ApplicantCountryCode))
        {
            try
            {
                response.ApplicantCountryName = await _countryService.GetCountryNameAsync(
                    application.ApplicantCountryCode,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to get country name for code {CountryCode}",
                    application.ApplicantCountryCode);
            }
        }

        // Map JobPosting if included
        if (application.JobPosting != null)
        {
            var jobPostingResponse = application.JobPosting.ToJobPostingResponse();
            if (jobPostingResponse != null)
            {
                jobPostingResponse.DescriptionHtml = _markdownService.ToHtml(application.JobPosting.Description);
                jobPostingResponse.RequirementsHtml = _markdownService.ToHtml(application.JobPosting.Requirements);
                jobPostingResponse.ResponsibilitiesHtml = _markdownService.ToHtml(application.JobPosting.Responsibilities);
                response.JobPosting = jobPostingResponse;
            }
        }

        return response;
    }

    /// <inheritdoc />
    public async Task<JobApplicationResponse> UpdateApplicationStatusAsync(
        Guid applicationId,
        UpdateApplicationStatusRequest request,
        Guid hrUserId,
        CancellationToken cancellationToken = default)
    {
        // Load application to get current state
        var application = await _dbContext.JobApplications
            .Include(a => a.JobPosting)
            .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken) ?? throw new InvalidOperationException($"Application {applicationId} not found.");

        // Manually check concurrency - compare client's RowVersion with DB's RowVersion
        var clientRowVersion = Convert.FromBase64String(request.RowVersion);
        if (!clientRowVersion.SequenceEqual(application.RowVersion))
        {
            throw new DbUpdateConcurrencyException(
                "The application has been modified by another user. Please refresh and try again.");
        }

        // Store current status before modifying
        var originalStatus = application.Status;

        // Validate status transition
        if (!ValidateStatusTransition(originalStatus, request.NewStatus))
        {
            throw new InvalidOperationException(
                $"Invalid status transition from {originalStatus} to {request.NewStatus}.");
        }

        // Create status change record
        var statusChange = new ApplicationStatusChange
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            FromStatus = originalStatus,
            ToStatus = request.NewStatus,
            ChangedBy = hrUserId,
            ChangedAt = DateTime.UtcNow,
            Reason = request.Reason,
            IsReversal = request.IsReversal
        };

        // If this is a reversal, try to link to the original change
        if (request.IsReversal)
        {
            var originalChange = await _dbContext.ApplicationStatusChanges
                .Where(c => c.ApplicationId == applicationId &&
                           c.ToStatus == originalStatus &&
                           c.FromStatus == request.NewStatus)
                .OrderByDescending(c => c.ChangedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (originalChange != null)
            {
                statusChange.ReversedChangeId = originalChange.Id;
            }
        }

        _dbContext.ApplicationStatusChanges.Add(statusChange);

        // Update application status
        application.Status = request.NewStatus;
        application.UpdatedBy = hrUserId;
        application.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new DbUpdateConcurrencyException(
                "The application has been modified by another user. Please refresh and try again.");
        }

        // Track metrics
        _metricsService.IncrementJobApplications(application.Status);

        // Send email notification to applicant
        try
        {
            await _emailService.SendStatusChangeNotificationAsync(
                recipientEmail: application.ApplicantEmail,
                applicantName: $"{application.ApplicantFirstName} {application.ApplicantLastName}",
                positionTitle: application.JobPosting.PositionTitle,
                newStatus: request.NewStatus,
                additionalMessage: request.Reason,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Status change notification sent to {Email} for application {ApplicationId}",
                application.ApplicantEmail,
                applicationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send status change notification to {Email} for application {ApplicationId}",
                application.ApplicantEmail,
                applicationId);
            // Don't fail the status update if email fails
        }

        // Return updated application
        return await GetApplicationByIdAsync(applicationId, cancellationToken)
            ?? throw new InvalidOperationException($"Failed to retrieve updated application {applicationId}.");
    }

    /// <inheritdoc />
    public async Task<StatusHistoryResponse> GetStatusHistoryAsync(
        Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        // Verify application exists
        var applicationExists = await _dbContext.JobApplications
            .AnyAsync(a => a.Id == applicationId, cancellationToken);

        if (!applicationExists)
        {
            throw new InvalidOperationException($"Application {applicationId} not found.");
        }

        // Get all status changes ordered by ChangedAt DESC (newest first)
        var changes = await _dbContext.ApplicationStatusChanges
            .Where(c => c.ApplicationId == applicationId)
            .OrderByDescending(c => c.ChangedAt)
            .ToListAsync(cancellationToken);

        // Get unique user IDs
        var userIds = changes.Select(c => c.ChangedBy).Distinct().ToList();

        // Get user names from Employee Service
        var userNames = new Dictionary<Guid, string>();
        foreach (var userId in userIds)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeAsync(userId, cancellationToken);
                if (employee != null)
                {
                    userNames[userId] = $"{employee.FirstName} {employee.LastName}";
                }
                else
                {
                    userNames[userId] = $"User {userId}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to get employee name for user {UserId}",
                    userId);
                userNames[userId] = $"User {userId}";
            }
        }

        // Map to response
        var response = new StatusHistoryResponse
        {
            ApplicationId = applicationId,
            Changes = [.. changes.Select(c => new StatusChangeRecord
            {
                Id = c.Id,
                FromStatus = c.FromStatus,
                ToStatus = c.ToStatus,
                ChangedBy = c.ChangedBy,
                ChangedByName = userNames.GetValueOrDefault(c.ChangedBy, $"User {c.ChangedBy}"),
                ChangedAt = c.ChangedAt,
                Reason = c.Reason,
                IsReversal = c.IsReversal
            })]
        };

        return response;
    }

    /// <inheritdoc />
    public bool ValidateStatusTransition(string fromStatus, string toStatus)
    {
        // Define valid state transitions
        var validTransitions = new Dictionary<string, string[]>
        {
            [ApplicationStatus.Submitted] =
            [
                ApplicationStatus.UnderReview,
                ApplicationStatus.Rejected,
                ApplicationStatus.Withdrawn
            ],
            [ApplicationStatus.UnderReview] =
            [
                ApplicationStatus.Interviewing,
                ApplicationStatus.Rejected,
                ApplicationStatus.Withdrawn,
                ApplicationStatus.Submitted // Allow reversal
            ],
            [ApplicationStatus.Interviewing] =
            [
                ApplicationStatus.Offered,
                ApplicationStatus.Rejected,
                ApplicationStatus.Withdrawn,
                ApplicationStatus.UnderReview // Allow reversal
            ],
            [ApplicationStatus.Offered] =
            [
                ApplicationStatus.Accepted,
                ApplicationStatus.Rejected,
                ApplicationStatus.Withdrawn,
                ApplicationStatus.Interviewing // Allow reversal
            ],
            [ApplicationStatus.Accepted] = [],
            [ApplicationStatus.Rejected] = [],
            [ApplicationStatus.Withdrawn] = []
        };

        if (!validTransitions.TryGetValue(fromStatus, out var allowedStatuses))
        {
            return false;
        }

        return allowedStatuses.Contains(toStatus, StringComparer.OrdinalIgnoreCase);
    }
}
