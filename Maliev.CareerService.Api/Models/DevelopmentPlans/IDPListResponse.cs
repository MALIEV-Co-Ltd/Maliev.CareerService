namespace Maliev.CareerService.Api.Models.DevelopmentPlans;

/// <summary>
/// Response model for list of Individual Development Plans
/// </summary>
public class IDPListResponse
{
    public List<IDPResponse> Items { get; set; } = [];
    public int TotalCount { get; set; }
}
