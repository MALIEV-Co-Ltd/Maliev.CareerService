using Maliev.CareerService.Application.Models.Common;

namespace Maliev.CareerService.Application.Models.Enrollments;

/// <summary>
/// Paginated list of training enrollments
/// </summary>
public class TrainingEnrollmentListResponse : PaginatedResponse<TrainingEnrollmentResponse>
{
}
