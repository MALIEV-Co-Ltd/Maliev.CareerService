namespace Maliev.CareerService.Application.Authorization;

/// <summary>
/// Defines granular permission constants for the Career Service.
/// All permissions follow the naming convention: career.[category].[action]
/// </summary>
public static class CareerPermissions
{
    /// <summary>Permissions related to training programs and records (Feature 003).</summary>
    public static class Trainings
    {
        /// <summary>Permission to create training programs.</summary>
        public const string Create = "career.trainings.create";
        /// <summary>Permission to read training program details.</summary>
        public const string Read = "career.trainings.read";
        /// <summary>Permission to update training programs.</summary>
        public const string Update = "career.trainings.update";
        /// <summary>Permission to delete training programs.</summary>
        public const string Delete = "career.trainings.delete";
        /// <summary>Permission to enroll in training programs.</summary>
        public const string Enroll = "career.trainings.enroll";
        /// <summary>Permission to mark training as completed.</summary>
        public const string Complete = "career.trainings.complete";
        /// <summary>Permission to issue training certifications.</summary>
        public const string Certify = "career.trainings.certify";
        /// <summary>Permission to view own training records and skills.</summary>
        public const string ViewOwn = "career.trainings.view-own";
        /// <summary>Permission to view team training records and skills (managers).</summary>
        public const string ViewTeam = "career.trainings.view-team";
        /// <summary>Permission to manage all training records and skills (HR admins).</summary>
        public const string Manage = "career.trainings.manage";
    }

    /// <summary>Permissions related to performance evaluations.</summary>
    public static class Evaluations
    {
        /// <summary>Permission to create performance evaluations.</summary>
        public const string Create = "career.evaluations.create";
        /// <summary>Permission to read performance evaluations.</summary>
        public const string Read = "career.evaluations.read";
        /// <summary>Permission to submit performance evaluations.</summary>
        public const string Submit = "career.evaluations.submit";
        /// <summary>Permission to approve performance evaluations.</summary>
        public const string Approve = "career.evaluations.approve";
    }

    /// <summary>Permissions related to career paths.</summary>
    public static class Paths
    {
        /// <summary>Permission to view career paths.</summary>
        public const string View = "career.paths.view";
        /// <summary>Permission to create career paths.</summary>
        public const string Create = "career.paths.create";
        /// <summary>Permission to assign employees to career paths.</summary>
        public const string Assign = "career.paths.assign";
    }

    /// <summary>Permissions related to employee development planning.</summary>
    public static class Development
    {
        /// <summary>Permission to view own development plan.</summary>
        public const string ViewOwn = "career.development.view-own";
        /// <summary>Permission to view team development plans.</summary>
        public const string ViewTeam = "career.development.view-team";
        /// <summary>Permission to manage development plans.</summary>
        public const string Manage = "career.development.manage";
    }

    /// <summary>Permissions related to job postings.</summary>
    public static class JobPostings
    {
        /// <summary>Permission to read job postings.</summary>
        public const string Read = "career.jobpostings.read";
        /// <summary>Permission to manage (create/update/delete) job postings.</summary>
        public const string Manage = "career.jobpostings.manage";
    }

    /// <summary>Permissions related to reporting.</summary>
    public static class Reports
    {
        /// <summary>Permission to read HR and recruitment reports.</summary>
        public const string Read = "career.reports.read";
    }

    /// <summary>Permissions related to job applications.</summary>
    public static class Applications
    {
        /// <summary>Permission to read job applications.</summary>
        public const string Read = "career.applications.read";
        /// <summary>Permission to read all job applications.</summary>
        public const string ReadAll = "career.applications.read-all";
    }

    /// <summary>Permissions related to mandatory training requirements (Feature 003).</summary>
    public static class MandatoryTrainings
    {
        /// <summary>Permission to view mandatory training requirements.</summary>
        public const string View = "career.mandatory-trainings.view";
        /// <summary>Permission to manage mandatory training requirements.</summary>
        public const string Manage = "career.mandatory-trainings.manage";
    }

    /// <summary>Permissions related to compliance reporting (Feature 003).</summary>
    public static class ComplianceReports
    {
        /// <summary>Permission to view training compliance reports.</summary>
        public const string View = "career.compliance-reports.view";
    }

    /// <summary>
    /// Collection of all defined career permissions with descriptions.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> AllWithDescriptions = new Dictionary<string, string>
    {
        { Trainings.Create, "Create training programs" },
        { Trainings.Read, "Read training program details" },
        { Trainings.Update, "Update training programs" },
        { Trainings.Delete, "Delete training programs" },
        { Trainings.Enroll, "Enroll in training programs" },
        { Trainings.Complete, "Mark training as completed" },
        { Trainings.Certify, "Issue training certifications" },
        { Trainings.ViewOwn, "View own training records and skills" },
        { Trainings.ViewTeam, "View team training records and skills" },
        { Trainings.Manage, "Manage all training records and skills" },

        { Evaluations.Create, "Create performance evaluations" },
        { Evaluations.Read, "Read performance evaluations" },
        { Evaluations.Submit, "Submit performance evaluations" },
        { Evaluations.Approve, "Approve performance evaluations" },

        { Paths.View, "View career paths" },
        { Paths.Create, "Create career paths" },
        { Paths.Assign, "Assign employees to career paths" },

        { Development.ViewOwn, "View own development plan" },
        { Development.ViewTeam, "View team development plans" },
        { Development.Manage, "Manage development plans" },

        { JobPostings.Read, "Read job postings" },
        { JobPostings.Manage, "Manage job postings" },

        { Reports.Read, "Read HR and recruitment reports" },

        { Applications.Read, "Read job applications" },
        { Applications.ReadAll, "Read all job applications" },

        { MandatoryTrainings.View, "View mandatory training requirements" },
        { MandatoryTrainings.Manage, "Manage mandatory training requirements" },

        { ComplianceReports.View, "View training compliance reports" }
    };

    /// <summary>
    /// Collection of all defined career permissions.
    /// </summary>
    public static string[] All => AllWithDescriptions.Keys.ToArray();
}
