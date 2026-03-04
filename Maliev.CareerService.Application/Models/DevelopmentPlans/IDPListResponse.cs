namespace Maliev.CareerService.Application.Models.DevelopmentPlans;

/// <summary>
/// Response model for list of Individual Development Plans
/// </summary>
public class IDPListResponse
{
    /// <summary>
    /// Gets or sets the list of Individual Development Plans.
    /// </summary>
    public List<IDPResponse> Items { get; set; } = [];
    /// <summary>
    /// Gets or sets the total count of Individual Development Plans.
    /// </summary>
    public int TotalCount { get; set; }
}
