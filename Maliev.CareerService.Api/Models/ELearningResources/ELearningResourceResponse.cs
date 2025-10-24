namespace Maliev.CareerService.Api.Models.ELearningResources;

/// <summary>
/// Response DTO for e-learning resource
/// </summary>
public class ELearningResourceResponse
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

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
    public bool IsActive { get; set; }

    /// <summary>
    /// When the record was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the record was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Optimistic concurrency token (Base64 encoded)
    /// </summary>
    public string RowVersion { get; set; } = string.Empty;
}
