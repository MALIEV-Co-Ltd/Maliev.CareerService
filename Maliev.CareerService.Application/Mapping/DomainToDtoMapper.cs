using Maliev.CareerService.Application.Models.Applications;
using Maliev.CareerService.Application.Models.DevelopmentGoals;
using Maliev.CareerService.Application.Models.DevelopmentPlans;
using Maliev.CareerService.Application.Models.ELearningResources;
using Maliev.CareerService.Application.Models.Enrollments;
using Maliev.CareerService.Application.Models.JobPostings;
using Maliev.CareerService.Application.Models.TrainingPrograms;
using Maliev.CareerService.Application.Models.TrainingRecords;
using Maliev.CareerService.Domain.Entities;
using ApplicationStatus = Maliev.CareerService.Domain.Entities.ApplicationStatusConstants;
using TrainingEnrollmentStatus = Maliev.CareerService.Domain.Entities.TrainingEnrollmentStatusConstants;
using IDPStatus = Maliev.CareerService.Domain.Entities.IDPStatusConstants;
using DevelopmentGoalStatus = Maliev.CareerService.Domain.Entities.DevelopmentGoalStatusConstants;

namespace Maliev.CareerService.Application.Mapping;

/// <summary>
/// Extension methods for mapping domain entities to DTOs manually
/// </summary>
public static class DomainToDtoMapper
{
    // JobPosting Mappings
    /// <summary>
    /// Maps JobPosting entity to JobPostingResponse
    /// </summary>
    public static JobPostingResponse ToJobPostingResponse(this JobPosting jobPosting)
    {
        return new JobPostingResponse
        {
            Id = jobPosting.Id,
            PositionTitle = jobPosting.PositionTitle,
            PositionCode = jobPosting.PositionCode,
            Department = jobPosting.Department,
            Location = jobPosting.Location,
            EmploymentType = jobPosting.EmploymentType,
            Description = jobPosting.Description,
            Requirements = jobPosting.Requirements,
            Responsibilities = jobPosting.Responsibilities,
            SalaryMin = jobPosting.SalaryMin,
            SalaryMax = jobPosting.SalaryMax,
            Currency = jobPosting.Currency,
            IsActive = jobPosting.IsActive,
            PublishedAt = jobPosting.PublishedAt,
            ApplicationDeadline = jobPosting.ApplicationDeadline,
            CreatedAt = jobPosting.CreatedAt,
            UpdatedAt = jobPosting.UpdatedAt,
            RowVersion = jobPosting.Version.ToString()
        };
    }

    /// <summary>
    /// Maps CreateJobPostingRequest to JobPosting entity
    /// </summary>
    public static JobPosting ToJobPosting(this CreateJobPostingRequest request)
    {
        return new JobPosting
        {
            PositionTitle = request.PositionTitle,
            PositionCode = request.PositionCode,
            Department = request.Department,
            Location = request.Location,
            EmploymentType = request.EmploymentType,
            Description = request.Description,
            Requirements = request.Requirements,
            Responsibilities = request.Responsibilities,
            SalaryMin = request.SalaryMin,
            SalaryMax = request.SalaryMax,
            Currency = request.Currency,
            ApplicationDeadline = request.ApplicationDeadline,
            IsActive = request.PublishImmediately,
            PublishedAt = request.PublishImmediately ? DateTime.UtcNow : null
        };
    }

    /// <summary>
    /// Updates JobPosting entity from UpdateJobPostingRequest
    /// </summary>
    public static void UpdateJobPosting(this JobPosting jobPosting, UpdateJobPostingRequest request)
    {
        jobPosting.PositionTitle = request.PositionTitle;
        jobPosting.Department = request.Department;
        jobPosting.Location = request.Location;
        jobPosting.EmploymentType = request.EmploymentType;
        jobPosting.Description = request.Description;
        jobPosting.Requirements = request.Requirements;
        jobPosting.Responsibilities = request.Responsibilities;
        jobPosting.SalaryMin = request.SalaryMin;
        jobPosting.SalaryMax = request.SalaryMax;
        jobPosting.Currency = request.Currency;
        jobPosting.ApplicationDeadline = request.ApplicationDeadline;
        jobPosting.IsActive = request.IsActive;

        if (request.IsActive && jobPosting.PublishedAt == null)
        {
            jobPosting.PublishedAt = DateTime.UtcNow;
        }
    }

    // JobApplication Mappings
    /// <summary>
    /// Maps JobApplication entity to JobApplicationResponse
    /// </summary>
    public static JobApplicationResponse ToJobApplicationResponse(this JobApplication application)
    {
        return new JobApplicationResponse
        {
            Id = application.Id,
            JobPostingId = application.JobPostingId,
            ApplicantEmail = application.ApplicantEmail,
            ApplicantFirstName = application.ApplicantFirstName,
            ApplicantLastName = application.ApplicantLastName,
            ApplicantPhone = application.ApplicantPhone,
            ApplicantCountryCode = application.ApplicantCountryCode,
            ResumeFileId = application.ResumeFileId,
            AdditionalFileIds = application.AdditionalFileIds,
            CoverLetter = application.CoverLetter,
            Status = application.Status,
            AppliedAt = application.AppliedAt,
            UpdatedAt = application.UpdatedAt,
            RowVersion = application.Version.ToString()
        };
    }

    /// <summary>
    /// Maps SubmitJobApplicationRequest to JobApplication entity
    /// </summary>
    public static JobApplication ToJobApplication(this SubmitJobApplicationRequest request)
    {
        return new JobApplication
        {
            JobPostingId = request.JobPostingId,
            ApplicantEmail = request.ApplicantEmail,
            ApplicantFirstName = request.ApplicantFirstName,
            ApplicantLastName = request.ApplicantLastName,
            ApplicantPhone = request.ApplicantPhone,
            ApplicantCountryCode = request.ApplicantCountryCode,
            ResumeFileId = request.ResumeFileId,
            AdditionalFileIds = request.AdditionalFileIds,
            CoverLetter = request.CoverLetter,
            Status = ApplicationStatus.Submitted,
            AppliedAt = DateTime.UtcNow
        };
    }

    // TrainingProgram Mappings
    /// <summary>
    /// Maps TrainingProgram entity to TrainingProgramResponse
    /// </summary>
    public static TrainingProgramResponse ToTrainingProgramResponse(this TrainingProgram program)
    {
        return new TrainingProgramResponse
        {
            Id = program.Id,
            ProgramName = program.ProgramName,
            ProgramCode = program.ProgramCode,
            Description = program.Description,
            Category = program.Category,
            Provider = program.Provider,
            DurationHours = program.DurationHours,
            ExternalLmsUrl = program.ExternalLmsUrl,
            TargetRoles = program.TargetRoles,
            MaxParticipants = program.MaxParticipants,
            IsMandatory = program.IsMandatory,
            IsActive = program.IsActive,
            ValidityMonths = program.ValidityMonths,
            CreatedAt = program.CreatedAt,
            UpdatedAt = program.UpdatedAt,
            RowVersion = program.Version.ToString()
        };
    }

    /// <summary>
    /// Maps CreateTrainingProgramRequest to TrainingProgram entity
    /// </summary>
    public static TrainingProgram ToTrainingProgram(this CreateTrainingProgramRequest request)
    {
        return new TrainingProgram
        {
            ProgramName = request.ProgramName,
            ProgramCode = request.ProgramCode,
            Description = request.Description,
            Category = request.Category,
            Provider = request.Provider,
            DurationHours = request.DurationHours,
            ExternalLmsUrl = request.ExternalLmsUrl,
            TargetRoles = request.TargetRoles,
            MaxParticipants = request.MaxParticipants,
            IsMandatory = request.IsMandatory,
            IsActive = request.IsActive,
            ValidityMonths = request.ValidityMonths
        };
    }

    /// <summary>
    /// Updates TrainingProgram entity from UpdateTrainingProgramRequest
    /// </summary>
    public static void UpdateTrainingProgram(this TrainingProgram program, UpdateTrainingProgramRequest request)
    {
        program.ProgramName = request.ProgramName;
        program.Description = request.Description;
        program.Category = request.Category;
        program.Provider = request.Provider;
        program.DurationHours = request.DurationHours;
        program.ExternalLmsUrl = request.ExternalLmsUrl;
        program.TargetRoles = request.TargetRoles;
        program.MaxParticipants = request.MaxParticipants;
        program.IsMandatory = request.IsMandatory;
        program.IsActive = request.IsActive;
        program.ValidityMonths = request.ValidityMonths;
    }

    // Enrollment Mappings
    /// <summary>
    /// Maps EmployeeTrainingEnrollment entity to TrainingEnrollmentResponse
    /// </summary>
    public static TrainingEnrollmentResponse ToTrainingEnrollmentResponse(this EmployeeTrainingEnrollment enrollment)
    {
        return new TrainingEnrollmentResponse
        {
            Id = enrollment.Id,
            TrainingProgramId = enrollment.TrainingProgramId,
            EmployeeId = enrollment.EmployeeId,
            EnrolledAt = enrollment.EnrolledAt,
            EnrollmentType = enrollment.EnrollmentType,
            Status = enrollment.Status,
            StartedAt = enrollment.StartedAt,
            CompletedAt = enrollment.CompletedAt,
            CompletionNotes = enrollment.CompletionNotes,
            MarkedCompleteBy = enrollment.MarkedCompleteBy,
            CreatedAt = enrollment.CreatedAt,
            UpdatedAt = enrollment.UpdatedAt,
            RowVersion = enrollment.Version.ToString()
        };
    }

    /// <summary>
    /// Maps EnrollInTrainingRequest to EmployeeTrainingEnrollment entity
    /// </summary>
    public static EmployeeTrainingEnrollment ToEmployeeTrainingEnrollment(this EnrollInTrainingRequest request)
    {
        return new EmployeeTrainingEnrollment
        {
            TrainingProgramId = request.TrainingProgramId,
            EnrolledAt = DateTime.UtcNow,
            Status = TrainingEnrollmentStatus.Enrolled
        };
    }

    // ELearningResource Mappings
    /// <summary>
    /// Maps ELearningResource entity to ELearningResourceResponse
    /// </summary>
    public static ELearningResourceResponse ToELearningResourceResponse(this ELearningResource resource)
    {
        return new ELearningResourceResponse
        {
            Id = resource.Id,
            Title = resource.Title,
            Description = resource.Description,
            ResourceCode = resource.ResourceCode,
            Category = resource.Category,
            ResourceType = resource.ResourceType,
            ExternalLmsUrl = resource.ExternalLmsUrl,
            EstimatedMinutes = resource.EstimatedMinutes,
            IsActive = resource.IsActive,
            CreatedAt = resource.CreatedAt,
            UpdatedAt = resource.UpdatedAt,
            RowVersion = resource.Version.ToString()
        };
    }

    // IDP Mappings
    /// <summary>
    /// Maps IndividualDevelopmentPlan entity to IDPResponse
    /// </summary>
    public static IDPResponse ToIDPResponse(this IndividualDevelopmentPlan idp)
    {
        return new IDPResponse
        {
            Id = idp.Id,
            EmployeeId = idp.EmployeeId,
            PlanYear = idp.PlanYear,
            Status = idp.Status,
            SubmittedAt = idp.SubmittedAt,
            ApprovedAt = idp.ApprovedAt,
            ApprovedBy = idp.ApprovedBy,
            CreatedAt = idp.CreatedAt,
            UpdatedAt = idp.UpdatedAt,
            RowVersion = idp.Version.ToString(),
            Goals = idp.Goals?.Select(g => g.ToDevelopmentGoalResponse()).ToList() ?? new List<DevelopmentGoalResponse>()
        };
    }

    /// <summary>
    /// Maps CreateIDPRequest to IndividualDevelopmentPlan entity
    /// </summary>
    public static IndividualDevelopmentPlan ToIndividualDevelopmentPlan(this CreateIDPRequest request)
    {
        return new IndividualDevelopmentPlan
        {
            PlanYear = request.PlanYear,
            Status = IDPStatus.Draft
        };
    }

    // DevelopmentGoal Mappings
    /// <summary>
    /// Maps EmployeeDevelopmentGoal entity to DevelopmentGoalResponse
    /// </summary>
    public static DevelopmentGoalResponse ToDevelopmentGoalResponse(this EmployeeDevelopmentGoal goal)
    {
        return new DevelopmentGoalResponse
        {
            Id = goal.Id,
            IdpId = goal.IdpId,
            GoalTitle = goal.GoalTitle,
            GoalDescription = goal.GoalDescription,
            Category = goal.Category,
            TargetDate = goal.TargetDate,
            Status = goal.Status,
            CompletionDate = goal.CompletionDate,
            ActionItems = goal.ActionItems,
            ProgressNotes = goal.ProgressNotes,
            CreatedAt = goal.CreatedAt,
            UpdatedAt = goal.UpdatedAt,
            RowVersion = goal.Version.ToString()
        };
    }

    /// <summary>
    /// Maps CreateDevelopmentGoalRequest to EmployeeDevelopmentGoal entity
    /// </summary>
    public static EmployeeDevelopmentGoal ToEmployeeDevelopmentGoal(this CreateDevelopmentGoalRequest request)
    {
        return new EmployeeDevelopmentGoal
        {
            GoalTitle = request.GoalTitle,
            GoalDescription = request.GoalDescription,
            Category = request.Category,
            TargetDate = request.TargetDate,
            ActionItems = request.ActionItems,
            Status = DevelopmentGoalStatus.NotStarted
        };
    }

    /// <summary>
    /// Updates EmployeeDevelopmentGoal entity from UpdateDevelopmentGoalRequest
    /// </summary>
    public static void UpdateDevelopmentGoal(this EmployeeDevelopmentGoal goal, UpdateDevelopmentGoalRequest request)
    {
        goal.GoalTitle = request.GoalTitle;
        goal.GoalDescription = request.GoalDescription;
        goal.Category = request.Category;
        goal.TargetDate = request.TargetDate;
        goal.ActionItems = request.ActionItems;
        goal.ProgressNotes = request.ProgressNotes;
    }

    // Feature 003: Training Records and Skills Migration Mappings

    /// <summary>
    /// Maps TrainingRecord entity to TrainingRecordResponse
    /// </summary>
    public static TrainingRecordResponse ToTrainingRecordResponse(this TrainingRecord record)
    {
        return new TrainingRecordResponse
        {
            Id = record.Id,
            EmployeeId = record.EmployeeId,
            TrainingProgramId = record.TrainingProgramId,
            CourseName = record.CourseName,
            CompletionDate = record.CompletionDate,
            ExpirationDate = record.ExpirationDate,
            CertificateDocumentId = record.CertificateDocumentId,
            TrainingType = record.TrainingType,
            Provider = record.Provider,
            Status = record.Status,
            Score = record.Score,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt
        };
    }

    /// <summary>
    /// Maps RecordTrainingCompletionRequest to TrainingRecord entity
    /// </summary>
    public static TrainingRecord ToTrainingRecord(this RecordTrainingCompletionRequest request, Guid employeeId)
    {
        return new TrainingRecord
        {
            EmployeeId = employeeId,
            TrainingProgramId = request.TrainingProgramId,
            CourseName = request.CourseName,
            CompletionDate = request.CompletionDate,
            ExpirationDate = request.ExpirationDate,
            CertificateDocumentId = request.CertificateDocumentId,
            TrainingType = request.TrainingType,
            Provider = request.Provider,
            Status = TrainingStatus.Completed,
            Score = request.Score
        };
    }

    /// <summary>
    /// Updates TrainingRecord entity from UpdateTrainingRecordRequest
    /// </summary>
    public static void UpdateTrainingRecord(this TrainingRecord record, UpdateTrainingRecordRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.CourseName))
            record.CourseName = request.CourseName;

        if (request.ExpirationDate.HasValue)
            record.ExpirationDate = request.ExpirationDate;

        if (request.CertificateDocumentId.HasValue)
            record.CertificateDocumentId = request.CertificateDocumentId;

        if (!string.IsNullOrWhiteSpace(request.Provider))
            record.Provider = request.Provider;

        if (request.Status.HasValue)
            record.Status = request.Status.Value;

        if (request.Score.HasValue)
            record.Score = request.Score;
    }
}
