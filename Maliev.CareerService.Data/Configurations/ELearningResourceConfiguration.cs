using Maliev.CareerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.CareerService.Data.Configurations;

/// <summary>
/// Entity Framework configuration for ELearningResource
/// </summary>
public class ELearningResourceConfiguration : IEntityTypeConfiguration<ELearningResource>
{
    public void Configure(EntityTypeBuilder<ELearningResource> builder)
    {
        // Table name
        builder.ToTable("elearning_resources");

        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.ResourceCode)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("resource_code");

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("title");

        builder.Property(e => e.Description)
            .IsRequired()
            .HasColumnName("description");

        builder.Property(e => e.ResourceType)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("resource_type");

        builder.Property(e => e.Category)
            .HasMaxLength(100)
            .HasColumnName("category");

        builder.Property(e => e.ExternalLmsUrl)
            .HasMaxLength(500)
            .HasColumnName("external_lms_url");

        builder.Property(e => e.EstimatedMinutes)
            .HasColumnName("estimated_minutes");

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(e => e.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(e => e.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(e => e.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .HasColumnName("row_version")
            .ValueGeneratedNever()  // Manually controlled for PostgreSQL
            .HasDefaultValueSql("'\\x00000000000000000001'::bytea");

        // Indexes
        builder.HasIndex(e => e.ResourceCode)
            .IsUnique()
            .HasDatabaseName("uq_elearning_resources_resource_code");

        builder.HasIndex(e => e.Category)
            .HasDatabaseName("ix_elearning_resources_category");

        builder.HasIndex(e => e.ResourceType)
            .HasDatabaseName("ix_elearning_resources_resource_type");

        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("ix_elearning_resources_is_active");

        // Check constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("chk_elearning_resources_estimated_minutes_positive",
                "estimated_minutes IS NULL OR estimated_minutes > 0");
        });

        // Query filter for soft delete
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
