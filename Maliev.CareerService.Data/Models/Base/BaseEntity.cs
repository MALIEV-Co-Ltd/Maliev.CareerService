using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Data.Models.Base;

/// <summary>
/// Base entity class with common audit fields and soft delete support
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Record creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// User ID who created this record
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// User ID who last updated this record
    /// </summary>
    public Guid UpdatedBy { get; set; }

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Optimistic concurrency token
    /// </summary>
    [Timestamp]
    public byte[] RowVersion { get; set; } = [];
}
