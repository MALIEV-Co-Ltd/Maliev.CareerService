namespace Maliev.CareerService.Api.Models;

/// <summary>
/// Defines valid employment type constants.
/// </summary>
public static class EmploymentType
{
    /// <summary>
    /// Full-time employment.
    /// </summary>
    public const string FullTime = "Full-time";
    /// <summary>
    /// Part-time employment.
    /// </summary>
    public const string PartTime = "Part-time";
    /// <summary>
    /// Contract employment.
    /// </summary>
    public const string Contract = "Contract";
    /// <summary>
    /// Intern employment.
    /// </summary>
    public const string Intern = "Intern";
    /// <summary>
    /// Freelance employment.
    /// </summary>
    public const string Freelance = "Freelance";

    /// <summary>
    /// Gets an array of all valid employment types.
    /// </summary>
    public static readonly string[] ValidTypes =
    [
        FullTime,
        PartTime,
        Contract,
        Intern,
        Freelance
    ];
}