using Maliev.CareerService.Api.Authentication;
using Maliev.Aspire.ServiceDefaults.IAM;

namespace Maliev.CareerService.Api.Services.External;

/// <summary>
/// Background service that registers the Career Service's permissions and roles with the central IAM on startup.
/// Uses the standard IAMRegistrationService base class.
/// </summary>
public class CareerIAMRegistrationService : IAMRegistrationService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CareerIAMRegistrationService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger.</param>
    public CareerIAMRegistrationService(
        IHttpClientFactory httpClientFactory,
        ILogger<CareerIAMRegistrationService> logger)
        : base(httpClientFactory, logger, "career")
    {
    }

    /// <inheritdoc/>
    protected override IEnumerable<PermissionRegistration> GetPermissions()
    {
        return new List<PermissionRegistration>
        {
            new() { PermissionId = CareerPermissions.Trainings.Create, Description = "Create training programs" },
            new() { PermissionId = CareerPermissions.Trainings.Read, Description = "Read training details" },
            new() { PermissionId = CareerPermissions.Trainings.Update, Description = "Update training programs" },
            new() { PermissionId = CareerPermissions.Trainings.Delete, Description = "Delete trainings" },
            new() { PermissionId = CareerPermissions.Trainings.Enroll, Description = "Enroll in training" },
            new() { PermissionId = CareerPermissions.Trainings.Complete, Description = "Mark training as completed" },
            new() { PermissionId = CareerPermissions.Trainings.Certify, Description = "Issue training certifications" },

            new() { PermissionId = CareerPermissions.Evaluations.Create, Description = "Create performance evaluations" },
            new() { PermissionId = CareerPermissions.Evaluations.Read, Description = "Read evaluations" },
            new() { PermissionId = CareerPermissions.Evaluations.Submit, Description = "Submit evaluations" },
            new() { PermissionId = CareerPermissions.Evaluations.Approve, Description = "Approve evaluations" },

            new() { PermissionId = CareerPermissions.Paths.View, Description = "View career paths" },
            new() { PermissionId = CareerPermissions.Paths.Create, Description = "Create career paths" },
            new() { PermissionId = CareerPermissions.Paths.Assign, Description = "Assign employees to paths" },

            new() { PermissionId = CareerPermissions.Development.ViewOwn, Description = "View own development plan" },
            new() { PermissionId = CareerPermissions.Development.ViewTeam, Description = "View team development plans" },
            new() { PermissionId = CareerPermissions.Development.Manage, Description = "Manage development plans" },

            new() { PermissionId = CareerPermissions.JobPostings.Read, Description = "Read job postings" },
            new() { PermissionId = CareerPermissions.JobPostings.Manage, Description = "Manage job postings" },

            new() { PermissionId = CareerPermissions.Reports.Read, Description = "Read HR reports" },
            new() { PermissionId = CareerPermissions.Applications.Read, Description = "Read job applications" },
            new() { PermissionId = CareerPermissions.Applications.ReadAll, Description = "Read all job applications" }
        };
    }

    /// <inheritdoc/>
    protected override IEnumerable<RoleRegistration> GetPredefinedRoles()
    {
        return CareerPredefinedRoles.RolePermissions.Select(rp => new RoleRegistration
        {
            RoleId = rp.Key,
            Description = $"Predefined {rp.Key} role for Career Service",
            PermissionIds = rp.Value.ToList(),
            IsCustom = false
        });
    }
}