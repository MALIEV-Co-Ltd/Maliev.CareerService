namespace Maliev.CareerService.Api.Models.DevelopmentPlans;

/// <summary>
/// Request model for updating an Individual Development Plan
/// </summary>
public class UpdateIDPRequest
{
    public string RowVersion { get; set; } = string.Empty;
}
