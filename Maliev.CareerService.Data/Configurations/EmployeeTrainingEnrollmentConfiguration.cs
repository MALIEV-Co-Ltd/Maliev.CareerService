using Maliev.CareerService.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.CareerService.Data.Configurations;

/// <summary>
/// Entity Framework configuration for EmployeeTrainingEnrollment
/// </summary>
public class EmployeeTrainingEnrollmentConfiguration : IEntityTypeConfiguration<EmployeeTrainingEnrollment>
{
    public void Configure(EntityTypeBuilder<EmployeeTrainingEnrollment> builder)
    {
        // Table name
        builder.ToTable("employee_training_enrollments");

        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.TrainingProgramId)
            .IsRequired()
            .HasColumnName("training_program_id");

        builder.Property(e => e.EmployeeId)
            .IsRequired()
            .HasColumnName("employee_id");

        builder.Property(e => e.EnrolledAt)
            .IsRequired()
            .HasColumnName("enrolled_at");

        builder.Property(e => e.EnrollmentType)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("enrollment_type");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("status");

        builder.Property(e => e.StartedAt)
            .HasColumnName("started_at");

        builder.Property(e => e.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(e => e.CompletionNotes)
            .HasMaxLength(2000)
            .HasColumnName("completion_notes");

        builder.Property(e => e.MarkedCompleteBy)
            .HasColumnName("marked_complete_by");

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
        builder.HasIndex(e => new { e.TrainingProgramId, e.EmployeeId })
            .IsUnique()
            .HasDatabaseName("uq_employee_training_enrollments_program_employee");

        builder.HasIndex(e => e.EmployeeId)
            .HasDatabaseName("ix_employee_training_enrollments_employee_id");

        builder.HasIndex(e => e.TrainingProgramId)
            .HasDatabaseName("ix_employee_training_enrollments_training_program_id");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("ix_employee_training_enrollments_status");

        builder.HasIndex(e => e.EnrollmentType)
            .HasDatabaseName("ix_employee_training_enrollments_enrollment_type");

        builder.HasIndex(e => e.EnrolledAt)
            .HasDatabaseName("ix_employee_training_enrollments_enrolled_at");

        // Check constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("chk_employee_training_enrollments_dates",
                "completed_at IS NULL OR started_at IS NOT NULL");
        });

        // Query filter for soft delete
        builder.HasQueryFilter(e => !e.IsDeleted);

        // Relationships configured in TrainingProgramConfiguration
    }
}
