using Maliev.CareerService.Domain.Entities;
using Maliev.CareerService.Infrastructure.ValueGenerators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.CareerService.Infrastructure.Data.Configurations;

public class ApplicationStatusChangeConfiguration : IEntityTypeConfiguration<ApplicationStatusChange>
{
    public void Configure(EntityTypeBuilder<ApplicationStatusChange> builder)
    {
        builder.ToTable("application_status_changes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ApplicationId).HasColumnName("application_id").IsRequired();
        builder.Property(x => x.FromStatus).HasColumnName("from_status").HasMaxLength(50);
        builder.Property(x => x.ToStatus).HasColumnName("to_status").HasMaxLength(50).IsRequired();
        builder.Property(x => x.ChangedBy).HasColumnName("changed_by").IsRequired();
        builder.Property(x => x.ChangedAt).HasColumnName("changed_at");
        builder.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(500);
        builder.Property(x => x.IsReversal).HasColumnName("is_reversal");
        builder.Property(x => x.ReversedChangeId).HasColumnName("reversed_change_id");

        builder.HasOne(x => x.Application)
            .WithMany(x => x.StatusChanges)
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ApplicationId);
    }
}

public class MandatoryTrainingRequirementConfiguration : IEntityTypeConfiguration<MandatoryTrainingRequirement>
{
    public void Configure(EntityTypeBuilder<MandatoryTrainingRequirement> builder)
    {
        builder.ToTable("mandatory_training_requirements");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TrainingProgramId).HasColumnName("training_program_id").IsRequired();
        builder.Property(x => x.DepartmentId).HasColumnName("department_id");
        builder.Property(x => x.PositionId).HasColumnName("position_id");
        builder.Property(x => x.CompletionDeadlineDays).HasColumnName("completion_deadline_days");
        builder.Property(x => x.RecertificationMonths).HasColumnName("recertification_months");
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.RowVersion).HasColumnName("row_version").IsRequired();

        builder.HasOne(x => x.TrainingProgram)
            .WithMany()
            .HasForeignKey(x => x.TrainingProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.TrainingProgramId, x.DepartmentId, x.PositionId });
    }
}
