using Maliev.CareerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.CareerService.Infrastructure.Data.Configurations;

public class TrainingProgramConfiguration : IEntityTypeConfiguration<TrainingProgram>
{
    public void Configure(EntityTypeBuilder<TrainingProgram> builder)
    {
        builder.ToTable("training_programs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ProgramCode).HasColumnName("program_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.ProgramName).HasColumnName("program_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").IsRequired();
        builder.Property(x => x.Category).HasColumnName("category").HasMaxLength(100);
        builder.Property(x => x.DurationHours).HasColumnName("duration_hours");
        builder.Property(x => x.Provider).HasColumnName("provider").HasMaxLength(200);
        builder.Property(x => x.ExternalLmsUrl).HasColumnName("external_lms_url").HasMaxLength(500);
        builder.Property(x => x.IsMandatory).HasColumnName("is_mandatory");
        builder.Property(x => x.TargetRoles).HasColumnName("target_roles");
        builder.Property(x => x.MaxParticipants).HasColumnName("max_participants");
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.ValidityMonths).HasColumnName("validity_months");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.RowVersion).HasColumnName("row_version").IsRequired();
        builder.Property<uint>("Version").HasColumnName("xmin").HasColumnType("xmin").IsRowVersion().ValueGeneratedOnAddOrUpdate();

        builder.HasIndex(x => x.ProgramCode).IsUnique();
    }
}
