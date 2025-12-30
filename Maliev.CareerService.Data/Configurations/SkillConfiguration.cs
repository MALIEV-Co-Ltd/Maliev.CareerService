using Maliev.CareerService.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.CareerService.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Skill
/// </summary>
public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        // Table name
        builder.ToTable("skills");

        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.EmployeeId)
            .IsRequired()
            .HasColumnName("employee_id");

        builder.Property(e => e.SkillName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("skill_name");

        builder.Property(e => e.ProficiencyLevel)
            .IsRequired()
            .HasColumnName("proficiency_level");

        builder.Property(e => e.LastAssessedDate)
            .IsRequired()
            .HasColumnName("last_assessed_date");

        builder.Property(e => e.IsDevelopmentArea)
            .HasColumnName("is_development_area");

        builder.Property(e => e.Notes)
            .HasMaxLength(1000)
            .HasColumnName("notes");

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
        builder.HasIndex(e => new { e.EmployeeId, e.SkillName })
            .IsUnique()
            .HasDatabaseName("uq_skills_employee_skill_name");

        builder.HasIndex(e => e.EmployeeId)
            .HasDatabaseName("ix_skills_employee_id");

        builder.HasIndex(e => e.IsDevelopmentArea)
            .HasDatabaseName("ix_skills_is_development_area")
            .HasFilter("is_development_area = true");

        // Check constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("chk_skills_proficiency_level_range",
                "proficiency_level BETWEEN 1 AND 5");
        });

        // Query filter for soft delete
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
