namespace Maliev.CareerService.Data.Entities;

public class JobPositionLocation
{
    public int JobPositionId { get; set; }
    public int WorkLocationId { get; set; }

    // Navigation properties
    public JobPosition JobPosition { get; set; } = null!;
    public WorkLocation WorkLocation { get; set; } = null!;
}
