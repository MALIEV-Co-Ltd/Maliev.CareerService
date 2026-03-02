using Maliev.CareerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.CareerService.Data.Configurations;

/// <summary>
/// Entity Framework configuration for TrainingRecord
/// </summary>
public class TrainingRecordConfiguration : IEntityTypeConfiguration<TrainingRecord>
{
    public void Configure(EntityTypeBuilder<TrainingRecord> builder)
    {
        // Table name
        builder.ToTable("training_records");

        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.EmployeeId)
            .IsRequired()
            .HasColumnName("employee_id");

        builder.Property(e => e.TrainingProgramId)
            .HasColumnName("training_program_id");

        builder.Property(e => e.CourseName)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("course_name");

        builder.Property(e => e.CompletionDate)
            .IsRequired()
            .HasColumnName("completion_date");

        builder.Property(e => e.ExpirationDate)
            .HasColumnName("expiration_date");

        builder.Property(e => e.CertificateDocumentId)
            .HasColumnName("certificate_document_id");

        builder.Property(e => e.TrainingType)
            .IsRequired()
            .HasColumnName("training_type");

        builder.Property(e => e.Provider)
            .HasMaxLength(200)
            .HasColumnName("provider");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasColumnName("status");

        builder.Property(e => e.Score)
            .HasPrecision(5, 2)
            .HasColumnName("score");

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
        builder.HasIndex(e => e.EmployeeId)
            .HasDatabaseName("ix_training_records_employee_id");

        builder.HasIndex(e => e.ExpirationDate)
            .HasDatabaseName("ix_training_records_expiration_date")
            .HasFilter("expiration_date IS NOT NULL");

        builder.HasIndex(e => e.TrainingProgramId)
            .HasDatabaseName("ix_training_records_training_program_id");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("ix_training_records_status");

        // Check constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("chk_training_records_expiration_after_completion",
                "expiration_date IS NULL OR expiration_date > completion_date");
            t.HasCheckConstraint("chk_training_records_completion_not_future",
                "completion_date <= NOW()");
            t.HasCheckConstraint("chk_training_records_score_range",
                "score IS NULL OR (score >= 0 AND score <= 100)");
        });

        // Query filter for soft delete
        builder.HasQueryFilter(e => !e.IsDeleted);

        // Relationships
        builder.HasOne(e => e.TrainingProgram)
            .WithMany()
            .HasForeignKey(e => e.TrainingProgramId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
