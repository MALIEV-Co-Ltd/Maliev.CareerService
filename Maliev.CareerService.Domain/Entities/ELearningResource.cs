namespace Maliev.CareerService.Domain.Entities;

public class ELearningResource : BaseEntity
{
    public string ResourceCode { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ResourceType { get; set; } = string.Empty;

    public string? Category { get; set; }

    public string? ExternalLmsUrl { get; set; }

    public int? EstimatedMinutes { get; set; }

    public bool IsActive { get; set; } = true;
}

public static class ELearningResourceTypeConstants
{
    public const string Video = "Video";
    public const string Document = "Document";
    public const string Interactive = "Interactive";
    public const string Quiz = "Quiz";
}
