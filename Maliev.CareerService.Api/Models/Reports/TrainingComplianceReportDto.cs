namespace Maliev.CareerService.Api.Models.Reports;

/// <summary>
/// Data transfer object for training compliance report (Feature 003)
/// </summary>
public class TrainingComplianceReportDto
{
    /// <summary>
    /// Gets or sets the overall compliance percentage (0-100)
    /// </summary>
    public decimal OverallComplianceRate { get; set; }

    /// <summary>
    /// Gets or sets the total number of employees included in the report
    /// </summary>
    public int TotalEmployees { get; set; }

    /// <summary>
    /// Gets or sets the number of fully compliant employees
    /// </summary>
    public int FullyCompliantEmployees { get; set; }

    /// <summary>
    /// Gets or sets the number of partially compliant employees
    /// </summary>
    public int PartiallyCompliantEmployees { get; set; }

    /// <summary>
    /// Gets or sets the number of non-compliant employees
    /// </summary>
    public int NonCompliantEmployees { get; set; }

    /// <summary>
    /// Gets or sets compliance breakdown by department
    /// </summary>
    public List<DepartmentComplianceDto> DepartmentBreakdown { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of overdue training assignments
    /// </summary>
    public List<OverdueTrainingDto> OverdueTrainings { get; set; } = [];
}

/// <summary>
/// Compliance metrics for a specific department
/// </summary>
public class DepartmentComplianceDto
{
    /// <summary>
    /// Gets or sets the department identifier
    /// </summary>
    public Guid DepartmentId { get; set; }

    /// <summary>
    /// Gets or sets the department name
    /// </summary>
    public string DepartmentName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the compliance rate for this department (0-100)
    /// </summary>
    public decimal ComplianceRate { get; set; }

    /// <summary>
    /// Gets or sets the number of employees in this department
    /// </summary>
    public int TotalEmployees { get; set; }
}

/// <summary>
/// Details of an overdue training assignment
/// </summary>
public class OverdueTrainingDto
{
    /// <summary>
    /// Gets or sets the employee identifier
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Gets or sets the employee's full name
    /// </summary>
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the training program name
    /// </summary>
    public string TrainingName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date the training was due
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Gets or sets the number of days the training is overdue
    /// </summary>
    public int DaysOverdue { get; set; }
}
