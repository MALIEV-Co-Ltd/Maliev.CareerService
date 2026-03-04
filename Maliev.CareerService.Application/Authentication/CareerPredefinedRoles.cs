namespace Maliev.CareerService.Application.Authentication;

/// <summary>
/// Defines predefined roles and their associated permissions for the Career Service.
/// </summary>
public static class CareerPredefinedRoles
{
    /// <summary>Admin role with full access.</summary>
    public const string Admin = "roles.career.admin";
    /// <summary>HR role with extensive but not full management access.</summary>
    public const string HR = "roles.career.hr";
    /// <summary>Manager role for team management.</summary>
    public const string Manager = "roles.career.manager";
    /// <summary>Employee role for self-service actions.</summary>
    public const string Employee = "roles.career.employee";

    /// <summary>
    /// Collection of all predefined roles for the Career Service.
    /// </summary>
    public static readonly IReadOnlyList<(string RoleId, string Description, string[] Permissions)> All = new List<(string, string, string[])>
    {
        (Admin, "Full administrative access to all career operations", CareerPermissions.All),

        (HR, "Extensive management access for human resources", new[]
        {
            CareerPermissions.Trainings.Create,
            CareerPermissions.Trainings.Read,
            CareerPermissions.Trainings.Update,
            CareerPermissions.Trainings.Enroll,
            CareerPermissions.Trainings.Complete,
            CareerPermissions.Trainings.Certify,
            CareerPermissions.Trainings.ViewOwn,
            CareerPermissions.Trainings.ViewTeam,
            CareerPermissions.Trainings.Manage,
            CareerPermissions.Evaluations.Create,
            CareerPermissions.Evaluations.Read,
            CareerPermissions.Evaluations.Submit,
            CareerPermissions.Evaluations.Approve,
            CareerPermissions.Paths.View,
            CareerPermissions.Paths.Assign,
            CareerPermissions.Development.ViewOwn,
            CareerPermissions.Development.ViewTeam,
            CareerPermissions.Development.Manage,
            CareerPermissions.JobPostings.Read,
            CareerPermissions.JobPostings.Manage,
            CareerPermissions.Reports.Read,
            CareerPermissions.Applications.Read,
            CareerPermissions.Applications.ReadAll,
            CareerPermissions.MandatoryTrainings.View,
            CareerPermissions.MandatoryTrainings.Manage,
            CareerPermissions.ComplianceReports.View
        }),

        (Manager, "Team management access", new[]
        {
            CareerPermissions.Evaluations.Create,
            CareerPermissions.Evaluations.Read,
            CareerPermissions.Evaluations.Submit,
            CareerPermissions.Evaluations.Approve,
            CareerPermissions.Development.ViewTeam,
            CareerPermissions.Development.Manage,
            CareerPermissions.Trainings.Read,
            CareerPermissions.Trainings.Enroll,
            CareerPermissions.Trainings.ViewTeam,
            CareerPermissions.JobPostings.Read,
            CareerPermissions.Applications.Read,
            CareerPermissions.Applications.ReadAll
        }),

        (Employee, "Self-service actions for employees", new[]
        {
            CareerPermissions.Trainings.Read,
            CareerPermissions.Trainings.Enroll,
            CareerPermissions.Trainings.Complete,
            CareerPermissions.Trainings.ViewOwn,
            CareerPermissions.Evaluations.Read,
            CareerPermissions.Development.ViewOwn,
            CareerPermissions.Paths.View,
            CareerPermissions.JobPostings.Read,
            CareerPermissions.Applications.Read
        })
    };

    /// <summary>
    /// Gets the permissions associated with a specific role.
    /// </summary>
    /// <param name="role">The role ID to get permissions for.</param>
    /// <returns>An array of permission strings.</returns>
    public static string[] GetPermissions(string role)
    {
        return All.FirstOrDefault(r => r.RoleId == role).Permissions ?? Array.Empty<string>();
    }
}
