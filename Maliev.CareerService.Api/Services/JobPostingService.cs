using AutoMapper;
using Maliev.CareerService.Api.Models.Common;
using Maliev.CareerService.Api.Models.JobPostings;
using Maliev.CareerService.Data;
using Maliev.CareerService.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Api.Services;

/// <summary>
/// Service implementation for managing job postings
/// </summary>
public class JobPostingService(
    CareerDbContext dbContext,
    IMapper mapper,
    IMarkdownService markdownService,
    IMetricsService metricsService,
    ILogger<JobPostingService> logger) : IJobPostingService
{
    private readonly CareerDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;
    private readonly IMarkdownService _markdownService = markdownService;
    private readonly IMetricsService _metricsService = metricsService;
    private readonly ILogger<JobPostingService> _logger = logger;

    /// <inheritdoc />
    public async Task<JobPostingListResponse> GetActivePostingsAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.JobPostings
            .Where(jp => jp.IsActive && jp.PublishedAt != null && jp.ApplicationDeadline > DateTime.UtcNow)
            .OrderByDescending(jp => jp.PublishedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var postings = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var responses = postings.Select(MapToResponse).ToList();

        return new JobPostingListResponse
        {
            Items = responses,
            Page = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    /// <inheritdoc />
    public async Task<JobPostingResponse?> GetPostingByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var posting = await _dbContext.JobPostings
            .FirstOrDefaultAsync(jp => jp.Id == id, cancellationToken);

        if (posting == null)
        {
            return null;
        }

        return MapToResponse(posting);
    }

    /// <inheritdoc />
    public async Task<JobPostingListResponse> SearchPostingsAsync(
        string? searchTerm,
        string? department,
        string? location,
        string? employmentType,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.JobPostings
            .Where(jp => jp.IsActive && jp.PublishedAt != null && jp.ApplicationDeadline > DateTime.UtcNow);

        // Apply search term filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(jp =>
                jp.PositionTitle.ToLower().Contains(lowerSearchTerm) ||
                jp.Description.ToLower().Contains(lowerSearchTerm) ||
                jp.Requirements.ToLower().Contains(lowerSearchTerm) ||
                jp.Responsibilities.ToLower().Contains(lowerSearchTerm));
        }

        // Apply department filter
        if (!string.IsNullOrWhiteSpace(department))
        {
            var lowerDepartment = department.ToLower();
            query = query.Where(jp => jp.Department != null && jp.Department.ToLower() == lowerDepartment);
        }

        // Apply location filter
        if (!string.IsNullOrWhiteSpace(location))
        {
            var lowerLocation = location.ToLower();
            query = query.Where(jp => jp.Location != null && jp.Location.ToLower() == lowerLocation);
        }

        // Apply employment type filter
        if (!string.IsNullOrWhiteSpace(employmentType))
        {
            var lowerEmploymentType = employmentType.ToLower();
            query = query.Where(jp => jp.EmploymentType.ToLower() == lowerEmploymentType);
        }

        query = query.OrderByDescending(jp => jp.PublishedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var postings = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var responses = postings.Select(MapToResponse).ToList();

        return new JobPostingListResponse
        {
            Items = responses,
            Page = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    /// <inheritdoc />
    public async Task<JobPostingResponse> CreatePostingAsync(
        CreateJobPostingRequest request,
        Guid createdBy,
        CancellationToken cancellationToken = default)
    {
        // Check for duplicate position code
        var existingPosting = await _dbContext.JobPostings
            .FirstOrDefaultAsync(jp => jp.PositionCode == request.PositionCode, cancellationToken);

        if (existingPosting != null)
        {
            throw new InvalidOperationException($"A job posting with position code '{request.PositionCode}' already exists.");
        }

        var posting = _mapper.Map<JobPosting>(request);
        posting.CreatedBy = createdBy;
        posting.UpdatedBy = createdBy;

        _dbContext.JobPostings.Add(posting);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Update active job postings gauge
        if (posting.IsActive)
        {
            var activeCount = await _dbContext.JobPostings
                .CountAsync(jp => jp.IsActive, cancellationToken);
            _metricsService.SetActiveJobPostings(activeCount);
        }

        _logger.LogInformation("Job posting {PostingId} created with position code {PositionCode}", posting.Id, posting.PositionCode);

        return MapToResponse(posting);
    }

    /// <inheritdoc />
    public async Task<JobPostingResponse?> UpdatePostingAsync(
        Guid id,
        UpdateJobPostingRequest request,
        Guid updatedBy,
        CancellationToken cancellationToken = default)
    {
        var posting = await _dbContext.JobPostings
            .FirstOrDefaultAsync(jp => jp.Id == id, cancellationToken);

        if (posting == null)
        {
            return null;
        }

        // Verify RowVersion for optimistic concurrency
        var requestRowVersion = Convert.FromBase64String(request.RowVersion);
        if (!posting.RowVersion.SequenceEqual(requestRowVersion))
        {
            throw new DbUpdateConcurrencyException("The job posting has been modified by another user. Please refresh and try again.");
        }

        // Map updated fields
        _mapper.Map(request, posting);
        posting.UpdatedBy = updatedBy;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Job posting {PostingId} updated", id);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict when updating job posting {PostingId}", id);
            throw;
        }

        return MapToResponse(posting);
    }

    /// <inheritdoc />
    public async Task<bool> DeletePostingAsync(
        Guid id,
        Guid deletedBy,
        CancellationToken cancellationToken = default)
    {
        var posting = await _dbContext.JobPostings
            .FirstOrDefaultAsync(jp => jp.Id == id, cancellationToken);

        if (posting == null)
        {
            return false;
        }

        // Soft delete
        posting.IsDeleted = true;
        posting.UpdatedBy = deletedBy;

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Update active job postings gauge
        if (posting.IsActive)
        {
            var activeCount = await _dbContext.JobPostings
                .CountAsync(jp => jp.IsActive && !jp.IsDeleted, cancellationToken);
            _metricsService.SetActiveJobPostings(activeCount);
        }

        _logger.LogInformation("Job posting {PostingId} soft deleted", id);

        return true;
    }

    /// <summary>
    /// Maps JobPosting entity to JobPostingResponse with Markdown-to-HTML conversion
    /// </summary>
    private JobPostingResponse MapToResponse(JobPosting posting)
    {
        var response = _mapper.Map<JobPostingResponse>(posting);

        // Convert Markdown fields to HTML
        response.DescriptionHtml = _markdownService.ToHtml(posting.Description);
        response.RequirementsHtml = _markdownService.ToHtml(posting.Requirements);
        response.ResponsibilitiesHtml = _markdownService.ToHtml(posting.Responsibilities);

        return response;
    }
}
