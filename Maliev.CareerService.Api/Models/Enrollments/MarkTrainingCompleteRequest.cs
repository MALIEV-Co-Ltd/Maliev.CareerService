using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models.Enrollments;

/// <summary>
/// Request to mark a training enrollment as complete
/// </summary>
public class MarkTrainingCompleteRequest
{
    /// <summary>
    /// Completion notes from HR staff or system
    /// </summary>
    [StringLength(2000, ErrorMessage = "Completion notes cannot exceed 2000 characters")]
    public string? CompletionNotes { get; set; }

    /// <summary>
    /// Optimistic concurrency token (Base64 encoded)
    /// </summary>
    [Required(ErrorMessage = "RowVersion is required for optimistic concurrency")]
    public string RowVersion { get; set; } = string.Empty;
}
