namespace Maliev.CareerService.Domain.Entities;

public class ApplicationStatusChange
{
    public Guid Id { get; set; }

    public Guid ApplicationId { get; set; }

    public JobApplication Application { get; set; } = null!;

    public string? FromStatus { get; set; }

    public string ToStatus { get; set; } = string.Empty;

    public Guid ChangedBy { get; set; }

    public DateTime ChangedAt { get; set; }

    public string? Reason { get; set; }

    public bool IsReversal { get; set; }

    public Guid? ReversedChangeId { get; set; }
}
