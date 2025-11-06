namespace Maliev.CareerService.Api.Models;

public static class ExperienceLevel
{
    public const string Entry = "Entry";
    public const string Mid = "Mid";
    public const string Senior = "Senior";
    public const string Executive = "Executive";
    public const string Intern = "Intern";

    public static readonly string[] ValidLevels =
    [
        Entry,
        Mid,
        Senior,
        Executive,
        Intern
    ];
}