using Maliev.CareerService.Data.Models.Base;

namespace Maliev.CareerService.Data.Models;

/// <summary>
/// E-Learning resource entity for self-paced learning materials
/// </summary>
public class ELearningResource : BaseEntity
{
    /// <summary>
    /// Unique resource code (e.g., "VID-REACT-001")
    /// </summary>
    public string ResourceCode { get; set; } = string.Empty;

    /// <summary>
    /// Resource title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Resource description in Markdown format
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Resource type (Video, Document, Interactive, Quiz)
    /// </summary>
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>
    /// Resource category (e.g., "Programming", "Soft Skills", "Business")
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// External LMS URL for accessing the resource
    /// </summary>
    public string? ExternalLmsUrl { get; set; }

    /// <summary>
    /// Estimated time to complete in minutes
    /// </summary>
    public int? EstimatedMinutes { get; set; }

    /// <summary>
    /// Whether this resource is currently active and accessible
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// E-Learning resource type constants
/// </summary>
public static class ELearningResourceType
{
    public const string Video = "Video";
    public const string Document = "Document";
    public const string Interactive = "Interactive";
    public const string Quiz = "Quiz";
}
