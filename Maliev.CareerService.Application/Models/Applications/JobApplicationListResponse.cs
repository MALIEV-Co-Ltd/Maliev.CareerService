using Maliev.CareerService.Application.Models.Common;

namespace Maliev.CareerService.Application.Models.Applications;

/// <summary>
/// Paginated list of job applications
/// </summary>
public class JobApplicationListResponse : PaginatedResponse<JobApplicationResponse>
{
}
