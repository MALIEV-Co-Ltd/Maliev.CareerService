namespace Maliev.CareerService.Api.Authentication;

/// <summary>
/// Defines predefined roles and their associated permissions for the Career Service.
/// </summary>
public static class CareerPredefinedRoles
{
    /// <summary>Admin role with full access.</summary>
    public const string Admin = "career-admin";
    /// <summary>HR role with extensive but not full management access.</summary>
    public const string HR = "career-hr";
    /// <summary>Manager role for team management.</summary>
    public const string Manager = "career-manager";
    /// <summary>Employee role for self-service actions.</summary>
    public const string Employee = "career-employee";

    /// <summary>
    /// Maps each predefined role to its associated set of permission strings.
    /// </summary>
    public static readonly Dictionary<string, string[]> RolePermissions = new()
    {
        [Admin] = new[] { "career.*" },
        [HR] = new[]
        {
            CareerPermissions.Trainings.Create,
            CareerPermissions.Trainings.Read,
            CareerPermissions.Trainings.Update,
            CareerPermissions.Trainings.Enroll,
            CareerPermissions.Trainings.Complete,
            CareerPermissions.Trainings.Certify,
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
            CareerPermissions.Training.ViewOwn,
            CareerPermissions.Training.ViewTeam,
            CareerPermissions.Training.Manage,
            CareerPermissions.MandatoryTraining.View,
            CareerPermissions.MandatoryTraining.Manage,
            CareerPermissions.ComplianceReports.View
        },
        [Manager] = new[]
        {
            CareerPermissions.Evaluations.Create,
            CareerPermissions.Evaluations.Read,
            CareerPermissions.Evaluations.Submit,
            CareerPermissions.Evaluations.Approve,
            CareerPermissions.Development.ViewTeam,
            CareerPermissions.Development.Manage,
            CareerPermissions.Trainings.Read,
            CareerPermissions.Trainings.Enroll,
            CareerPermissions.JobPostings.Read,
            CareerPermissions.Applications.Read,
            CareerPermissions.Applications.ReadAll,
            CareerPermissions.Training.ViewTeam
        },
        [Employee] = new[]
        {
            CareerPermissions.Trainings.Read,
            CareerPermissions.Trainings.Enroll,
            CareerPermissions.Trainings.Complete,
            CareerPermissions.Evaluations.Read, // Logic handles "own"
            CareerPermissions.Development.ViewOwn,
            CareerPermissions.Paths.View,
            CareerPermissions.JobPostings.Read,
            CareerPermissions.Applications.Read,
            CareerPermissions.Training.ViewOwn
        }
    };
}