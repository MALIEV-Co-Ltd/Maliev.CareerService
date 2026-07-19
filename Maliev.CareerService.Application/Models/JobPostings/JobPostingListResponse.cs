using Maliev.CareerService.Application.Models.Common;

namespace Maliev.CareerService.Application.Models.JobPostings;

/// <summary>
/// Paginated list of job postings
/// </summary>
public class JobPostingListResponse : PaginatedResponse<JobPostingResponse>
{
}
