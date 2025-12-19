namespace Maliev.CareerService.Api.Models;

/// <summary>
/// Defines valid document type constants.
/// </summary>
public static class DocumentType
{
    /// <summary>
    /// Resume document type.
    /// </summary>
    public const string Resume = "Resume";
    /// <summary>
    /// Cover letter document type.
    /// </summary>
    public const string CoverLetter = "CoverLetter";
    /// <summary>
    /// Certificate document type.
    /// </summary>
    public const string Certificate = "Certificate";
    /// <summary>
    /// Portfolio document type.
    /// </summary>
    public const string Portfolio = "Portfolio";
    /// <summary>
    /// Transcript document type.
    /// </summary>
    public const string Transcript = "Transcript";
    /// <summary>
    /// Other document type.
    /// </summary>
    public const string Other = "Other";

    /// <summary>
    /// Gets an array of all valid document types.
    /// </summary>
    public static readonly string[] ValidTypes =
    [
        Resume,
        CoverLetter,
        Certificate,
        Portfolio,
        Transcript,
        Other
    ];
}
