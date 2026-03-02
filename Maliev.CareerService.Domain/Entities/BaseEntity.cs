using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    [ConcurrencyCheck]
    public byte[] RowVersion { get; set; } = [];
}
