namespace Maliev.CareerService.Api.Models;

public static class SkillLevel
{
    public const string Beginner = "Beginner";
    public const string Intermediate = "Intermediate";
    public const string Advanced = "Advanced";
    public const string Expert = "Expert";
    
    public static readonly string[] ValidLevels = 
    {
        Beginner,
        Intermediate,
        Advanced,
        Expert
    };
}