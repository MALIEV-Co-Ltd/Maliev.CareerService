namespace Maliev.CareerService.Api.Models;

public static class DocumentType
{
    public const string Resume = "Resume";
    public const string CoverLetter = "CoverLetter";
    public const string Certificate = "Certificate";
    public const string Portfolio = "Portfolio";
    public const string Transcript = "Transcript";
    public const string Other = "Other";
    
    public static readonly string[] ValidTypes = 
    {
        Resume,
        CoverLetter,
        Certificate,
        Portfolio,
        Transcript,
        Other
    };
}