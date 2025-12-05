namespace Maliev.CareerService.Api.Models.DevelopmentPlans;

/// <summary>
/// Request model for updating an Individual Development Plan
/// </summary>
public class UpdateIDPRequest
{
    /// <summary>
    /// Gets or sets the row version for optimistic concurrency control.
    /// </summary>
    public string RowVersion { get; set; } = string.Empty;
}
