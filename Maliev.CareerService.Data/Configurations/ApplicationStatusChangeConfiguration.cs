using Maliev.CareerService.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.CareerService.Data.Configurations;

/// <summary>
/// Entity Framework configuration for ApplicationStatusChange
/// </summary>
public class ApplicationStatusChangeConfiguration : IEntityTypeConfiguration<ApplicationStatusChange>
{
    public void Configure(EntityTypeBuilder<ApplicationStatusChange> builder)
    {
        builder.ToTable("application_status_changes");

        // Primary key
        builder.HasKey(e => e.Id);

        // Indexes
        builder.HasIndex(e => e.ApplicationId)
            .HasDatabaseName("idx_status_changes_application");

        builder.HasIndex(e => e.ChangedBy)
            .HasDatabaseName("idx_status_changes_changed_by");

        builder.HasIndex(e => e.ChangedAt)
            .HasDatabaseName("idx_status_changes_changed_at");

        // Required fields
        builder.Property(e => e.ToStatus)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.FromStatus)
            .HasMaxLength(50);

        // Relationships
        builder.HasOne(e => e.Application)
            .WithMany(a => a.StatusChanges)
            .HasForeignKey(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query Filter to exclude status changes for soft-deleted applications
        builder.HasQueryFilter(asc => asc.Application.IsDeleted == false);
    }
}
