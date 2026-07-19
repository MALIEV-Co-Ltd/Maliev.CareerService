
using Maliev.CareerService.Application.Mapping;
using Maliev.CareerService.Application.Models.TrainingPrograms;
using Maliev.CareerService.Infrastructure.Data;
using Maliev.CareerService.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Infrastructure.Services;

/// <summary>
/// Service implementation for managing training programs
/// </summary>
public class TrainingProgramService(
    CareerDbContext dbContext,
    ILogger<TrainingProgramService> logger) : ITrainingProgramService
{
    private readonly CareerDbContext _dbContext = dbContext;
    private readonly ILogger<TrainingProgramService> _logger = logger;

    /// <inheritdoc />
    public async Task<TrainingProgramListResponse> GetActiveProgramsAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TrainingPrograms
            .Where(tp => tp.IsActive)
            .OrderBy(tp => tp.ProgramName);

        var totalCount = await query.CountAsync(cancellationToken);

        var programs = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var responses = programs.Select(p => p.ToTrainingProgramResponse()).ToList();

        return new TrainingProgramListResponse
        {
            Items = responses,
            Page = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    /// <inheritdoc />
    public async Task<TrainingProgramResponse?> GetProgramByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var program = await _dbContext.TrainingPrograms
            .FirstOrDefaultAsync(tp => tp.Id == id, cancellationToken);

        if (program == null)
        {
            return null;
        }

        return program.ToTrainingProgramResponse();
    }

    /// <inheritdoc />
    public async Task<TrainingProgramListResponse> FilterProgramsAsync(
        string? category,
        bool? isMandatory,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TrainingPrograms
            .Where(tp => tp.IsActive);

        // Apply category filter
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(tp => tp.Category == category);
        }

        // Apply mandatory filter
        if (isMandatory.HasValue)
        {
            query = query.Where(tp => tp.IsMandatory == isMandatory.Value);
        }

        query = query.OrderBy(tp => tp.ProgramName);

        var totalCount = await query.CountAsync(cancellationToken);

        var programs = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var responses = programs.Select(p => p.ToTrainingProgramResponse()).ToList();

        return new TrainingProgramListResponse
        {
            Items = responses,
            Page = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    /// <inheritdoc />
    public async Task<TrainingProgramResponse> CreateProgramAsync(
        CreateTrainingProgramRequest request,
        Guid createdBy,
        CancellationToken cancellationToken = default)
    {
        // Check for duplicate program code
        var existingProgram = await _dbContext.TrainingPrograms
            .FirstOrDefaultAsync(tp => tp.ProgramCode == request.ProgramCode, cancellationToken);

        if (existingProgram != null)
        {
            throw new InvalidOperationException($"A training program with code '{request.ProgramCode}' already exists.");
        }

        var program = request.ToTrainingProgram();
        program.CreatedBy = createdBy;
        program.UpdatedBy = createdBy;

        _dbContext.TrainingPrograms.Add(program);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Training program {ProgramId} created with code {ProgramCode}", program.Id, program.ProgramCode);

        return program.ToTrainingProgramResponse();
    }

    /// <inheritdoc />
    public async Task<TrainingProgramResponse?> UpdateProgramAsync(
        Guid id,
        UpdateTrainingProgramRequest request,
        Guid updatedBy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(request.RowVersion))
        {
            throw new ArgumentException("RowVersion is required for concurrency control.", nameof(request));
        }

        var program = await _dbContext.TrainingPrograms
            .FirstOrDefaultAsync(tp => tp.Id == id, cancellationToken);

        if (program == null)
        {
            return null;
        }

        // Check version for optimistic concurrency
        if (!uint.TryParse(request.RowVersion, out var expectedVersion))
        {
            throw new ArgumentException("Invalid RowVersion format. Must be a valid unsigned 32-bit integer.", nameof(request.RowVersion));
        }

        if (program.Version != expectedVersion)
        {
            throw new DbUpdateConcurrencyException("The entity has been modified by another user. Please refresh and try again.");
        }

        // Map updated fields
        program.UpdateTrainingProgram(request);
        program.UpdatedBy = updatedBy;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Training program {ProgramId} updated", id);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict when updating training program {ProgramId}", id);
            throw;
        }

        return program.ToTrainingProgramResponse();
    }
}
