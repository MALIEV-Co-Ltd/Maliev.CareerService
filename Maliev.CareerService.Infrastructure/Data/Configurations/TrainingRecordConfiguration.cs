using Maliev.CareerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.CareerService.Infrastructure.Data.Configurations;

public class TrainingRecordConfiguration : IEntityTypeConfiguration<TrainingRecord>
{
    public void Configure(EntityTypeBuilder<TrainingRecord> builder)
    {
        builder.ToTable("training_records");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.EmployeeId).HasColumnName("employee_id").IsRequired();
        builder.Property(x => x.TrainingProgramId).HasColumnName("training_program_id");
        builder.Property(x => x.CourseName).HasColumnName("course_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.CompletionDate).HasColumnName("completion_date");
        builder.Property(x => x.ExpirationDate).HasColumnName("expiration_date");
        builder.Property(x => x.CertificateDocumentId).HasColumnName("certificate_document_id");
        builder.Property(x => x.TrainingType).HasColumnName("training_type").HasConversion<int>();
        builder.Property(x => x.Provider).HasColumnName("provider").HasMaxLength(200);
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<int>();
        builder.Property(x => x.Score).HasColumnName("score");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.RowVersion).HasColumnName("row_version");

        builder.HasOne(x => x.TrainingProgram)
            .WithMany()
            .HasForeignKey(x => x.TrainingProgramId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.TrainingProgramId);
    }
}
