using AutoMapper;
using Maliev.CareerService.Api.Models.Applications;
using Maliev.CareerService.Api.Models.DevelopmentGoals;
using Maliev.CareerService.Api.Models.DevelopmentPlans;
using Maliev.CareerService.Api.Models.ELearningResources;
using Maliev.CareerService.Api.Models.Enrollments;
using Maliev.CareerService.Api.Models.JobPostings;
using Maliev.CareerService.Api.Models.TrainingPrograms;
using Maliev.CareerService.Api.Services;
using Maliev.CareerService.Api.Services.External;
using Maliev.CareerService.Data.Models;

namespace Maliev.CareerService.Api.Mapping;

/// <summary>
/// AutoMapper profile for Career Service
/// </summary>
public class CareerServiceMappingProfile : Profile
{
    public CareerServiceMappingProfile()
    {
        ConfigureJobPostingMappings();
        ConfigureJobApplicationMappings();
        ConfigureTrainingProgramMappings();
        ConfigureEnrollmentMappings();
        ConfigureELearningResourceMappings();
        ConfigureDevelopmentPlanMappings();
        ConfigureDevelopmentGoalMappings();
    }

    private void ConfigureJobPostingMappings()
    {
        // JobPosting -> JobPostingResponse
        CreateMap<JobPosting, JobPostingResponse>()
            .ForMember(dest => dest.DescriptionHtml, opt => opt.Ignore()) // Will be set by custom resolver
            .ForMember(dest => dest.RequirementsHtml, opt => opt.Ignore()) // Will be set by custom resolver
            .ForMember(dest => dest.ResponsibilitiesHtml, opt => opt.Ignore()) // Will be set by custom resolver
            .ForMember(dest => dest.RowVersion, opt => opt.MapFrom(src => Convert.ToBase64String(src.RowVersion)));

        // CreateJobPostingRequest -> JobPosting
        CreateMap<CreateJobPostingRequest, JobPosting>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PublishedAt, opt => opt.MapFrom((src, dest) =>
                src.PublishImmediately ? DateTime.UtcNow : (DateTime?)null))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.PublishImmediately))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Applications, opt => opt.Ignore());

        // UpdateJobPostingRequest -> JobPosting (for updating existing entity)
        CreateMap<UpdateJobPostingRequest, JobPosting>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PositionCode, opt => opt.Ignore()) // Cannot update position code
            .ForMember(dest => dest.PublishedAt, opt => opt.Ignore()) // Managed separately
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore()) // Handled by EF Core
            .ForMember(dest => dest.Applications, opt => opt.Ignore());
    }

    private void ConfigureJobApplicationMappings()
    {
        // JobApplication -> JobApplicationResponse
        CreateMap<JobApplication, JobApplicationResponse>()
            .ForMember(dest => dest.ApplicantCountryName, opt => opt.Ignore()) // Will be set by custom resolver
            .ForMember(dest => dest.ResumeFileUrl, opt => opt.Ignore()) // Will be set by custom resolver
            .ForMember(dest => dest.AdditionalFileUrls, opt => opt.Ignore()) // Will be set by custom resolver
            .ForMember(dest => dest.JobPosting, opt => opt.Ignore()) // Will be mapped separately if needed
            .ForMember(dest => dest.RowVersion, opt => opt.MapFrom(src => Convert.ToBase64String(src.RowVersion)));

        // SubmitJobApplicationRequest -> JobApplication
        CreateMap<SubmitJobApplicationRequest, JobApplication>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ApplicationStatus.Submitted))
            .ForMember(dest => dest.AppliedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.JobPosting, opt => opt.Ignore())
            .ForMember(dest => dest.StatusChanges, opt => opt.Ignore());
    }

    private void ConfigureTrainingProgramMappings()
    {
        // TrainingProgram -> TrainingProgramResponse
        CreateMap<TrainingProgram, TrainingProgramResponse>()
            .ForMember(dest => dest.RowVersion, opt => opt.MapFrom(src => Convert.ToBase64String(src.RowVersion)));

        // CreateTrainingProgramRequest -> TrainingProgram
        CreateMap<CreateTrainingProgramRequest, TrainingProgram>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Enrollments, opt => opt.Ignore());

        // UpdateTrainingProgramRequest -> TrainingProgram (for updating existing entity)
        CreateMap<UpdateTrainingProgramRequest, TrainingProgram>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ProgramCode, opt => opt.Ignore()) // Cannot update program code
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore()) // Handled by EF Core
            .ForMember(dest => dest.Enrollments, opt => opt.Ignore());
    }

    private void ConfigureEnrollmentMappings()
    {
        // EmployeeTrainingEnrollment -> TrainingEnrollmentResponse
        CreateMap<EmployeeTrainingEnrollment, TrainingEnrollmentResponse>()
            .ForMember(dest => dest.TrainingProgram, opt => opt.Ignore()) // Will be mapped separately if needed
            .ForMember(dest => dest.RowVersion, opt => opt.MapFrom(src => Convert.ToBase64String(src.RowVersion)));

        // EnrollInTrainingRequest -> EmployeeTrainingEnrollment
        CreateMap<EnrollInTrainingRequest, EmployeeTrainingEnrollment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore()) // Will be set from JWT claims
            .ForMember(dest => dest.EnrolledAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.EnrollmentType, opt => opt.Ignore()) // Will be determined by service logic
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => TrainingEnrollmentStatus.Enrolled))
            .ForMember(dest => dest.StartedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CompletionNotes, opt => opt.Ignore())
            .ForMember(dest => dest.MarkedCompleteBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.TrainingProgram, opt => opt.Ignore());
    }

    private void ConfigureELearningResourceMappings()
    {
        // ELearningResource -> ELearningResourceResponse
        CreateMap<ELearningResource, ELearningResourceResponse>()
            .ForMember(dest => dest.RowVersion, opt => opt.MapFrom(src => Convert.ToBase64String(src.RowVersion)));
    }

    private void ConfigureDevelopmentPlanMappings()
    {
        // IndividualDevelopmentPlan -> IDPResponse
        CreateMap<IndividualDevelopmentPlan, IDPResponse>()
            .ForMember(dest => dest.RowVersion, opt => opt.MapFrom(src => Convert.ToBase64String(src.RowVersion)));

        // CreateIDPRequest -> IndividualDevelopmentPlan
        CreateMap<CreateIDPRequest, IndividualDevelopmentPlan>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore()) // Will be set from JWT claims
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IDPStatus.Draft))
            .ForMember(dest => dest.SubmittedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Goals, opt => opt.Ignore());
    }

    private void ConfigureDevelopmentGoalMappings()
    {
        // EmployeeDevelopmentGoal -> DevelopmentGoalResponse
        CreateMap<EmployeeDevelopmentGoal, DevelopmentGoalResponse>()
            .ForMember(dest => dest.RowVersion, opt => opt.MapFrom(src => Convert.ToBase64String(src.RowVersion)));

        // CreateDevelopmentGoalRequest -> EmployeeDevelopmentGoal
        CreateMap<CreateDevelopmentGoalRequest, EmployeeDevelopmentGoal>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IdpId, opt => opt.Ignore()) // Will be set from route parameter
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DevelopmentGoalStatus.NotStarted))
            .ForMember(dest => dest.CompletionDate, opt => opt.Ignore())
            .ForMember(dest => dest.ProgressNotes, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Idp, opt => opt.Ignore());

        // UpdateDevelopmentGoalRequest -> EmployeeDevelopmentGoal (for updating existing entity)
        CreateMap<UpdateDevelopmentGoalRequest, EmployeeDevelopmentGoal>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IdpId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CompletionDate, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Idp, opt => opt.Ignore());
    }
}
