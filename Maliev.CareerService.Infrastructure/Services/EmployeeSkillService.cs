
using Maliev.CareerService.Application.Models.Skills;
using Maliev.CareerService.Infrastructure.Data;
using Maliev.CareerService.Domain.Entities;
using Maliev.CareerService.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Infrastructure.Services;

/// <summary>
/// Service implementation for managing employee skills (Feature 003)
/// </summary>
public class EmployeeSkillService(
    CareerDbContext dbContext,
    ILogger<EmployeeSkillService> logger) : IEmployeeSkillService
{
    private readonly CareerDbContext _dbContext = dbContext;
    private readonly ILogger<EmployeeSkillService> _logger = logger;

    /// <inheritdoc />
    public async Task<EmployeeSkillDto> AddSkillAsync(
        Guid employeeId,
        AddSkillRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        // Check for existing skill with same name for this employee
        var existing = await _dbContext.Skills
            .FirstOrDefaultAsync(s => s.EmployeeId == employeeId &&
                                     s.SkillName.ToLower() == request.SkillName.ToLower() &&
                                     !s.IsDeleted, cancellationToken);

        if (existing != null)
        {
            throw new InvalidOperationException($"Employee already has skill: {request.SkillName}");
        }

        var skill = new Skill
        {
            EmployeeId = employeeId,
            SkillName = request.SkillName,
            ProficiencyLevel = request.ProficiencyLevel,
            IsDevelopmentArea = request.IsDevelopmentArea,
            Notes = request.Notes,
            LastAssessedDate = DateTime.UtcNow,
            CreatedBy = currentUserId,
            UpdatedBy = currentUserId
        };

        _dbContext.Skills.Add(skill);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Skill added: Employee {EmployeeId}, Skill {SkillName}, RecordId {RecordId}",
            employeeId,
            request.SkillName,
            skill.Id);

        return MapToDto(skill);
    }

    /// <inheritdoc />
    public async Task<List<EmployeeSkillDto>> GetByEmployeeIdAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var skills = await _dbContext.Skills
            .Where(s => s.EmployeeId == employeeId && !s.IsDeleted)
            .OrderBy(s => s.SkillName)
            .ToListAsync(cancellationToken);

        return skills.Select(MapToDto).ToList();
    }

    /// <inheritdoc />
    public async Task<EmployeeSkillDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var skill = await _dbContext.Skills
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);

        return skill == null ? null : MapToDto(skill);
    }

    /// <inheritdoc />
    public async Task<EmployeeSkillDto?> UpdateAsync(
        Guid id,
        UpdateEmployeeSkillRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        var skill = await _dbContext.Skills
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);

        if (skill == null)
        {
            return null;
        }

        // Update fields
        bool levelChanged = skill.ProficiencyLevel != request.ProficiencyLevel;

        skill.ProficiencyLevel = request.ProficiencyLevel;
        skill.IsDevelopmentArea = request.IsDevelopmentArea;
        skill.Notes = request.Notes;
        skill.UpdatedBy = currentUserId;

        if (levelChanged)
        {
            skill.LastAssessedDate = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Skill updated: RecordId {RecordId}, Employee {EmployeeId}",
            id,
            skill.EmployeeId);

        return MapToDto(skill);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(
        Guid id,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        var skill = await _dbContext.Skills
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);

        if (skill == null)
        {
            return false;
        }

        skill.IsDeleted = true;
        skill.UpdatedBy = currentUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Skill deleted (soft): RecordId {RecordId}, Employee {EmployeeId}",
            id,
            skill.EmployeeId);

        return true;
    }

    private static EmployeeSkillDto MapToDto(Skill skill)
    {
        return new EmployeeSkillDto
        {
            Id = skill.Id,
            EmployeeId = skill.EmployeeId,
            SkillName = skill.SkillName,
            ProficiencyLevel = skill.ProficiencyLevel,
            LastAssessedDate = skill.LastAssessedDate,
            IsDevelopmentArea = skill.IsDevelopmentArea,
            Notes = skill.Notes
        };
    }
}
