using Maliev.CareerService.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.CareerService.Data.Configurations;

/// <summary>
/// Entity Framework configuration for JobPosting
/// </summary>
public class JobPostingConfiguration : IEntityTypeConfiguration<JobPosting>
{
    public void Configure(EntityTypeBuilder<JobPosting> builder)
    {
        builder.ToTable("job_postings");

        // Primary key
        builder.HasKey(e => e.Id);

        // Indexes
        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("idx_job_postings_active");

        builder.HasIndex(e => e.Department)
            .HasDatabaseName("idx_job_postings_department");

        builder.HasIndex(e => e.EmploymentType)
            .HasDatabaseName("idx_job_postings_employment_type");

        builder.HasIndex(e => e.ApplicationDeadline)
            .HasDatabaseName("idx_job_postings_deadline");

        builder.HasIndex(e => e.PublishedAt)
            .HasDatabaseName("idx_job_postings_published");

        // Composite index for active postings query (most common query pattern)
        builder.HasIndex(e => new { e.IsActive, e.PublishedAt, e.ApplicationDeadline })
            .HasDatabaseName("idx_job_postings_active_list");

        // Unique constraint on position_code
        builder.HasIndex(e => e.PositionCode)
            .IsUnique()
            .HasDatabaseName("uq_job_postings_position_code");

        // Required fields
        builder.Property(e => e.PositionTitle)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.PositionCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.EmploymentType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Department)
            .HasMaxLength(100);

        builder.Property(e => e.Location)
            .HasMaxLength(100);

        builder.Property(e => e.Currency)
            .HasMaxLength(3);

        builder.Property(e => e.Description)
            .IsRequired();

        builder.Property(e => e.Requirements)
            .IsRequired();

        builder.Property(e => e.Responsibilities)
            .IsRequired();

        // Decimal precision for salary
        builder.Property(e => e.SalaryMin)
            .HasPrecision(18, 2);

        builder.Property(e => e.SalaryMax)
            .HasPrecision(18, 2);

        // Check constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("chk_job_postings_salary_range",
                "salary_min IS NULL OR salary_max IS NULL OR salary_min <= salary_max");

            t.HasCheckConstraint("chk_job_postings_deadline_future",
                "application_deadline > created_at");
        });

        // Relationships
        builder.HasMany(e => e.Applications)
            .WithOne(a => a.JobPosting)
            .HasForeignKey(a => a.JobPostingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
