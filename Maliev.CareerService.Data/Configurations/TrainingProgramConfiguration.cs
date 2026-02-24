using Maliev.CareerService.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.CareerService.Data.Configurations;

/// <summary>
/// Entity Framework configuration for TrainingProgram
/// </summary>
public class TrainingProgramConfiguration : IEntityTypeConfiguration<TrainingProgram>
{
    public void Configure(EntityTypeBuilder<TrainingProgram> builder)
    {
        // Table name
        builder.ToTable("training_programs");

        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.ProgramCode)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("program_code");

        builder.Property(e => e.ProgramName)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("program_name");

        builder.Property(e => e.Description)
            .IsRequired()
            .HasColumnName("description");

        builder.Property(e => e.Category)
            .HasMaxLength(100)
            .HasColumnName("category");

        builder.Property(e => e.DurationHours)
            .HasPrecision(6, 2)
            .HasColumnName("duration_hours");

        builder.Property(e => e.Provider)
            .HasMaxLength(200)
            .HasColumnName("provider");

        builder.Property(e => e.ExternalLmsUrl)
            .HasMaxLength(500)
            .HasColumnName("external_lms_url");

        builder.Property(e => e.IsMandatory)
            .HasColumnName("is_mandatory");

        builder.Property(e => e.TargetRoles)
            .HasColumnName("target_roles")
            .HasColumnType("text[]");

        builder.Property(e => e.MaxParticipants)
            .HasColumnName("max_participants");

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
        builder.HasIndex(e => e.ProgramCode)
            .IsUnique()
            .HasDatabaseName("uq_training_programs_program_code");

        builder.HasIndex(e => e.Category)
            .HasDatabaseName("ix_training_programs_category");

        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("ix_training_programs_is_active");

        builder.HasIndex(e => e.IsMandatory)
            .HasDatabaseName("ix_training_programs_is_mandatory");

        // Check constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("chk_training_programs_duration_positive", "duration_hours > 0");
        });

        // Query filter for soft delete
        builder.HasQueryFilter(e => !e.IsDeleted);

        // Relationships
        builder.HasMany(e => e.Enrollments)
            .WithOne(e => e.TrainingProgram)
            .HasForeignKey(e => e.TrainingProgramId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
