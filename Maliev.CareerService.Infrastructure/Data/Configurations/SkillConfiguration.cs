using Maliev.CareerService.Domain.Entities;
using Maliev.CareerService.Infrastructure.ValueGenerators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.CareerService.Infrastructure.Data.Configurations;

public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("skills");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.EmployeeId).HasColumnName("employee_id").IsRequired();
        builder.Property(x => x.SkillName).HasColumnName("skill_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ProficiencyLevel).HasColumnName("proficiency_level").HasConversion<int>();
        builder.Property(x => x.LastAssessedDate).HasColumnName("last_assessed_date");
        builder.Property(x => x.IsDevelopmentArea).HasColumnName("is_development_area");
        builder.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.RowVersion).HasColumnName("row_version").IsRequired();

        builder.HasIndex(x => new { x.EmployeeId, x.SkillName }).IsUnique();
    }
}

public class ELearningResourceConfiguration : IEntityTypeConfiguration<ELearningResource>
{
    public void Configure(EntityTypeBuilder<ELearningResource> builder)
    {
        builder.ToTable("e_learning_resources");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ResourceCode).HasColumnName("resource_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description");
        builder.Property(x => x.ResourceType).HasColumnName("resource_type").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Category).HasColumnName("category").HasMaxLength(100);
        builder.Property(x => x.ExternalLmsUrl).HasColumnName("external_lms_url").HasMaxLength(500);
        builder.Property(x => x.EstimatedMinutes).HasColumnName("estimated_minutes");
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.RowVersion).HasColumnName("row_version").IsRequired();
    }
}
