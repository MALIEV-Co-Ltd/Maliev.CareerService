namespace Maliev.CareerService.Api.Models;

/// <summary>
/// Defines valid skill proficiency level constants.
/// </summary>
public static class SkillLevel
{
    /// <summary>
    /// Beginner skill level.
    /// </summary>
    public const string Beginner = "Beginner";
    /// <summary>
    /// Intermediate skill level.
    /// </summary>
    public const string Intermediate = "Intermediate";
    /// <summary>
    /// Advanced skill level.
    /// </summary>
    public const string Advanced = "Advanced";
    /// <summary>
    /// Expert skill level.
    /// </summary>
    public const string Expert = "Expert";

    /// <summary>
    /// Gets an array of all valid skill levels.
    /// </summary>
    public static readonly string[] ValidLevels =
    [
        Beginner,
        Intermediate,
        Advanced,
        Expert
    ];
}