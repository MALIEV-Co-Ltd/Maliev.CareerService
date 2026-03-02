using Maliev.CareerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.CareerService.Infrastructure.Data.Configurations;

public class EmployeeTrainingEnrollmentConfiguration : IEntityTypeConfiguration<EmployeeTrainingEnrollment>
{
    public void Configure(EntityTypeBuilder<EmployeeTrainingEnrollment> builder)
    {
        builder.ToTable("employee_training_enrollments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TrainingProgramId).HasColumnName("training_program_id").IsRequired();
        builder.Property(x => x.EmployeeId).HasColumnName("employee_id").IsRequired();
        builder.Property(x => x.EnrolledAt).HasColumnName("enrolled_at");
        builder.Property(x => x.EnrollmentType).HasColumnName("enrollment_type").HasMaxLength(50).IsRequired();
        builder.Property(x => x.DueDate).HasColumnName("due_date");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(x => x.StartedAt).HasColumnName("started_at");
        builder.Property(x => x.CompletedAt).HasColumnName("completed_at");
        builder.Property(x => x.CompletionNotes).HasColumnName("completion_notes");
        builder.Property(x => x.MarkedCompleteBy).HasColumnName("marked_complete_by");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.RowVersion).HasColumnName("row_version");

        builder.HasOne(x => x.TrainingProgram)
            .WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.TrainingProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.EmployeeId, x.TrainingProgramId }).IsUnique();
    }
}
