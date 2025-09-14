using Maliev.CareerService.Api.Models;
using Maliev.CareerService.Data.DbContexts;
using Maliev.CareerService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Api.Services;

public class JobApplicationService : IJobApplicationService
{
    private readonly CareerDbContext _context;
    private readonly ILogger<JobApplicationService> _logger;

    public JobApplicationService(
        CareerDbContext context,
        ILogger<JobApplicationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<JobApplicationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var application = await _context.JobApplications
            .Include(ja => ja.JobPosition)
            .Include(ja => ja.ApplicationDocuments)
            .FirstOrDefaultAsync(ja => ja.Id == id, cancellationToken);

        if (application == null)
            return null;

        return MapToDto(application);
    }

    public async Task<PagedResult<JobApplicationDto>> GetByJobPositionIdAsync(int jobPositionId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _context.JobApplications
            .Include(ja => ja.JobPosition)
            .Include(ja => ja.ApplicationDocuments)
            .Where(ja => ja.JobPositionId == jobPositionId)
            .OrderByDescending(ja => ja.ApplicationDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var applications = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = applications.Select(MapToDto).ToList();

        return new PagedResult<JobApplicationDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<JobApplicationDto>> GetAllAsync(int page = 1, int pageSize = 20, string? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.JobApplications
            .Include(ja => ja.JobPosition)
            .Include(ja => ja.ApplicationDocuments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(ja => ja.Status == status);
        }

        query = query.OrderByDescending(ja => ja.ApplicationDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var applications = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = applications.Select(MapToDto).ToList();

        return new PagedResult<JobApplicationDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<JobApplicationDto> CreateAsync(CreateJobApplicationRequest request, CancellationToken cancellationToken = default)
    {
        // Verify job position exists and is public/active
        var jobPosition = await _context.JobPositions
            .FirstOrDefaultAsync(jp => jp.Id == request.JobPositionId && jp.IsPublic && jp.IsActive, cancellationToken);

        if (jobPosition == null)
        {
            throw new ArgumentException("Job position not found or not available for applications", nameof(request.JobPositionId));
        }

        // Check for duplicate application
        var existingApplication = await _context.JobApplications
            .AnyAsync(ja => ja.JobPositionId == request.JobPositionId && ja.ApplicantEmail == request.ApplicantEmail, cancellationToken);

        if (existingApplication)
        {
            throw new InvalidOperationException("An application for this position already exists with this email address");
        }

        var application = new JobApplication
        {
            JobPositionId = request.JobPositionId,
            ApplicantEmail = request.ApplicantEmail,
            ApplicantName = request.ApplicantName,
            ApplicantPhone = request.ApplicantPhone,
            LinkedInProfile = request.LinkedInProfile,
            PortfolioUrl = request.PortfolioUrl,
            Status = ApplicationStatus.Submitted,
            Notes = request.CoverLetterText
        };

        _context.JobApplications.Add(application);
        await _context.SaveChangesAsync(cancellationToken);

        // Load the complete entity with relationships
        var createdApplication = await _context.JobApplications
            .Include(ja => ja.JobPosition)
            .Include(ja => ja.ApplicationDocuments)
            .FirstAsync(ja => ja.Id == application.Id, cancellationToken);

        _logger.LogInformation("Created job application for {ApplicantName} ({ApplicantEmail}) for position {JobPositionId}", 
            application.ApplicantName, application.ApplicantEmail, application.JobPositionId);

        return MapToDto(createdApplication);
    }

    public async Task<JobApplicationDto?> UpdateStatusAsync(int id, UpdateJobApplicationRequest request, CancellationToken cancellationToken = default)
    {
        var application = await _context.JobApplications
            .Include(ja => ja.JobPosition)
            .Include(ja => ja.ApplicationDocuments)
            .FirstOrDefaultAsync(ja => ja.Id == id, cancellationToken);

        if (application == null)
            return null;

        // Validate status
        if (!ApplicationStatus.ValidStatuses.Contains(request.Status))
        {
            throw new ArgumentException($"Invalid status: {request.Status}", nameof(request.Status));
        }

        var oldStatus = application.Status;
        application.Status = request.Status;
        application.Notes = request.Notes;
        application.LastStatusChange = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated job application {ApplicationId} status from {OldStatus} to {NewStatus}", 
            id, oldStatus, request.Status);

        return MapToDto(application);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var application = await _context.JobApplications
            .Include(ja => ja.ApplicationDocuments)
            .FirstOrDefaultAsync(ja => ja.Id == id, cancellationToken);
        
        if (application == null)
            return false;

        // Note: In a real system, you might want to soft delete applications 
        // or require special permissions to delete them for compliance reasons
        _context.JobApplications.Remove(application);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted job application with ID {ApplicationId}", id);

        return true;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.JobApplications.AnyAsync(ja => ja.Id == id, cancellationToken);
    }

    public async Task<bool> HasExistingApplicationAsync(string email, int jobPositionId, CancellationToken cancellationToken = default)
    {
        return await _context.JobApplications
            .AnyAsync(ja => ja.ApplicantEmail == email && ja.JobPositionId == jobPositionId, cancellationToken);
    }

    public async Task<IEnumerable<JobApplicationDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var applications = await _context.JobApplications
            .Include(ja => ja.JobPosition)
            .Include(ja => ja.ApplicationDocuments)
            .Where(ja => ja.ApplicantEmail == email)
            .OrderByDescending(ja => ja.ApplicationDate)
            .ToListAsync(cancellationToken);

        return applications.Select(MapToDto);
    }

    private static JobApplicationDto MapToDto(JobApplication application)
    {
        return new JobApplicationDto
        {
            Id = application.Id,
            JobPositionId = application.JobPositionId,
            ApplicantEmail = application.ApplicantEmail,
            ApplicantName = application.ApplicantName,
            ApplicantPhone = application.ApplicantPhone,
            LinkedInProfile = application.LinkedInProfile,
            PortfolioUrl = application.PortfolioUrl,
            Status = application.Status,
            ApplicationDate = application.ApplicationDate,
            LastStatusChange = application.LastStatusChange,
            Notes = application.Notes,
            CreatedDate = application.CreatedDate,
            ModifiedDate = application.ModifiedDate,
            JobPosition = application.JobPosition == null ? null : new JobPositionDto
            {
                Id = application.JobPosition.Id,
                Title = application.JobPosition.Title,
                Department = application.JobPosition.Department,
                Description = application.JobPosition.Description,
                Requirements = application.JobPosition.Requirements,
                Responsibilities = application.JobPosition.Responsibilities,
                EmploymentType = application.JobPosition.EmploymentType,
                ExperienceLevel = application.JobPosition.ExperienceLevel,
                SalaryRangeMin = application.JobPosition.SalaryRangeMin,
                SalaryRangeMax = application.JobPosition.SalaryRangeMax,
                Currency = application.JobPosition.Currency,
                IsActive = application.JobPosition.IsActive,
                IsPublic = application.JobPosition.IsPublic,
                CreatedDate = application.JobPosition.CreatedDate,
                ModifiedDate = application.JobPosition.ModifiedDate,
                WorkLocations = new List<WorkLocationDto>(), // Not needed for application context
                Skills = new List<JobPositionSkillDto>(), // Not needed for application context
                ApplicationCount = 0 // Not needed for application context
            },
            Documents = application.ApplicationDocuments.Select(ad => new ApplicationDocumentDto
            {
                Id = ad.Id,
                JobApplicationId = ad.JobApplicationId,
                DocumentType = ad.DocumentType,
                OriginalFileName = ad.OriginalFileName,
                GcsBucket = ad.GcsBucket,
                GcsObjectName = ad.GcsObjectName,
                GcsUri = ad.GcsUri,
                FileSize = ad.FileSize,
                MimeType = ad.MimeType,
                UploadDate = ad.UploadDate,
                IsRequired = ad.IsRequired,
                DisplayOrder = ad.DisplayOrder,
                Description = ad.Description
            }).OrderBy(d => d.DisplayOrder).ToList()
        };
    }
}