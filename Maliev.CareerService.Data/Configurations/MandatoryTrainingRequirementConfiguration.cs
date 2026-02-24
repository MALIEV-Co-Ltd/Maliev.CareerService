using Maliev.CareerService.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.CareerService.Data.Configurations;

/// <summary>
/// Entity Framework configuration for MandatoryTrainingRequirement
/// </summary>
public class MandatoryTrainingRequirementConfiguration : IEntityTypeConfiguration<MandatoryTrainingRequirement>
{
    public void Configure(EntityTypeBuilder<MandatoryTrainingRequirement> builder)
    {
        // Table name
        builder.ToTable("mandatory_training_requirements");

        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.TrainingProgramId)
            .IsRequired()
            .HasColumnName("training_program_id");

        builder.Property(e => e.DepartmentId)
            .HasColumnName("department_id");

        builder.Property(e => e.PositionId)
            .HasColumnName("position_id");

        builder.Property(e => e.CompletionDeadlineDays)
            .IsRequired()
            .HasColumnName("completion_deadline_days");

        builder.Property(e => e.RecertificationMonths)
            .HasColumnName("recertification_months");

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
        builder.HasIndex(e => e.TrainingProgramId)
            .HasDatabaseName("ix_mandatory_training_requirements_training_program_id");

        builder.HasIndex(e => e.DepartmentId)
            .HasDatabaseName("ix_mandatory_training_requirements_department_id");

        builder.HasIndex(e => e.PositionId)
            .HasDatabaseName("ix_mandatory_training_requirements_position_id");

        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("ix_mandatory_training_requirements_is_active")
            .HasFilter("is_active = true");

        // Check constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("chk_mandatory_training_requirements_deadline_range",
                "completion_deadline_days BETWEEN 1 AND 365");
            t.HasCheckConstraint("chk_mandatory_training_requirements_recertification_range",
                "recertification_months IS NULL OR (recertification_months BETWEEN 1 AND 120)");
        });

        // Query filter for soft delete
        builder.HasQueryFilter(e => !e.IsDeleted);

        // Relationships
        builder.HasOne(e => e.TrainingProgram)
            .WithMany()
            .HasForeignKey(e => e.TrainingProgramId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
