namespace Maliev.CareerService.Domain.Entities;

public class JobPositionLocation
{
    public int JobPositionId { get; set; }
    public int WorkLocationId { get; set; }

    public JobPosition JobPosition { get; set; } = null!;
    public WorkLocation WorkLocation { get; set; } = null!;
}
