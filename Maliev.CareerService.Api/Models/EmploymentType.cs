namespace Maliev.CareerService.Api.Models;

public static class EmploymentType
{
    public const string FullTime = "Full-time";
    public const string PartTime = "Part-time";
    public const string Contract = "Contract";
    public const string Intern = "Intern";
    public const string Freelance = "Freelance";
    
    public static readonly string[] ValidTypes = 
    {
        FullTime,
        PartTime,
        Contract,
        Intern,
        Freelance
    };
}