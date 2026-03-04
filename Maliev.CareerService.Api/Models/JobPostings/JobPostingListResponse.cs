using Maliev.CareerService.Api.Models.Common;

namespace Maliev.CareerService.Api.Models.JobPostings;

/// <summary>
/// Paginated list of job postings
/// </summary>
public class JobPostingListResponse : PaginatedResponse<JobPostingResponse>
{
}
