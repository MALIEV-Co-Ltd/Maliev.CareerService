using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models.Applications;

/// <summary>
/// Request to update application status
/// </summary>
public class UpdateApplicationStatusRequest
{
    /// <summary>
    /// New status to transition to
    /// </summary>
    [Required(ErrorMessage = "New status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    public string NewStatus { get; set; } = string.Empty;

    /// <summary>
    /// Reason for the status change
    /// </summary>
    [StringLength(1000, ErrorMessage = "Reason cannot exceed 1000 characters")]
    public string? Reason { get; set; }

    /// <summary>
    /// Whether this is a reversal of a previous status change
    /// </summary>
    public bool IsReversal { get; set; }

    /// <summary>
    /// Row version for optimistic concurrency control
    /// </summary>
    [Required(ErrorMessage = "Row version is required")]
    public string RowVersion { get; set; } = string.Empty;
}
