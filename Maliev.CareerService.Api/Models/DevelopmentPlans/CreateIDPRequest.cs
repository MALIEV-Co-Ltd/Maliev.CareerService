namespace Maliev.CareerService.Api.Models.DevelopmentPlans;

/// <summary>
/// Request model for creating a new Individual Development Plan
/// </summary>
public class CreateIDPRequest
{
    /// <summary>
    /// Gets or sets the year for which the IDP is created.
    /// </summary>
    public int PlanYear { get; set; }
}
