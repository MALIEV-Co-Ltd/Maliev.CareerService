using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Application.Models.Enrollments;

/// <summary>
/// Request to enroll in a training program
/// </summary>
public class EnrollInTrainingRequest
{
    /// <summary>
    /// Training program ID to enroll in
    /// </summary>
    [Required(ErrorMessage = "Training program ID is required")]
    public Guid TrainingProgramId { get; set; }
}
