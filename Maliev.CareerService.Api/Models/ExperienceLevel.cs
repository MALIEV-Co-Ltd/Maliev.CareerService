namespace Maliev.CareerService.Api.Models;

/// <summary>
/// Defines valid experience level constants.
/// </summary>
public static class ExperienceLevel
{
    /// <summary>
    /// Entry-level experience.
    /// </summary>
    public const string Entry = "Entry";
    /// <summary>
    /// Mid-level experience.
    /// </summary>
    public const string Mid = "Mid";
    /// <summary>
    /// Senior-level experience.
    /// </summary>
    public const string Senior = "Senior";
    /// <summary>
    /// Executive-level experience.
    /// </summary>
    public const string Executive = "Executive";
    /// <summary>
    /// Intern-level experience.
    /// </summary>
    public const string Intern = "Intern";

    /// <summary>
    /// Gets an array of all valid experience levels.
    /// </summary>
    public static readonly string[] ValidLevels =
    [
        Entry,
        Mid,
        Senior,
        Executive,
        Intern
    ];
}
