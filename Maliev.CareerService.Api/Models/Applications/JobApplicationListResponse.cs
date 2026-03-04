using Maliev.CareerService.Api.Models.Common;

namespace Maliev.CareerService.Api.Models.Applications;

/// <summary>
/// Paginated list of job applications
/// </summary>
public class JobApplicationListResponse : PaginatedResponse<JobApplicationResponse>
{
}
