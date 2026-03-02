using Maliev.Aspire.ServiceDefaults.IAM;
using Maliev.CareerService.Api.Authentication;

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
    /// <param name="configuration">The application configuration.</param>
    /// <param name="logger">The logger.</param>
    public CareerIAMRegistrationService(
        IConfiguration configuration,
        ILogger<CareerIAMRegistrationService> logger)
        : base(configuration, logger, "career")
    {
    }

    /// <inheritdoc/>
    protected override IEnumerable<PermissionRegistration> GetPermissions()
    {
        return CareerPermissions.AllWithDescriptions.Select(p => new PermissionRegistration
        {
            PermissionId = p.Key,
            Description = p.Value
        });
    }

    /// <inheritdoc/>
    protected override IEnumerable<RoleRegistration> GetPredefinedRoles()
    {
        return CareerPredefinedRoles.All.Select(r => new RoleRegistration
        {
            RoleId = r.RoleId,
            Description = r.Description,
            PermissionIds = r.Permissions.ToList(),
            IsCustom = false
        });
    }
}
