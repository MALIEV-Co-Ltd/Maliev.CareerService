using Maliev.CareerService.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.CareerService.Data.Configurations;

/// <summary>
/// Entity Framework configuration for JobApplication
/// </summary>
public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.ToTable("job_applications");

        // Primary key
        builder.HasKey(e => e.Id);

        // Indexes
        builder.HasIndex(e => e.JobPostingId)
            .HasDatabaseName("idx_job_applications_posting");

        builder.HasIndex(e => e.ApplicantEmail)
            .HasDatabaseName("idx_job_applications_email");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("idx_job_applications_status");

        builder.HasIndex(e => e.AppliedAt)
            .HasDatabaseName("idx_job_applications_applied");

        // Composite unique constraint (one application per email per posting)
        builder.HasIndex(e => new { e.JobPostingId, e.ApplicantEmail })
            .IsUnique()
            .HasDatabaseName("uq_job_applications_posting_email");

        // Required fields
        builder.Property(e => e.ApplicantFirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.ApplicantLastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.ApplicantEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.ApplicantPhone)
            .HasMaxLength(20);

        builder.Property(e => e.ApplicantCountryCode)
            .HasMaxLength(2);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50);

        // Array column for additional file IDs
        builder.Property(e => e.AdditionalFileIds)
            .IsRequired();

        // Row version for optimistic concurrency
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .HasColumnName("row_version")
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValueSql("'\\x00000000000000000001'::bytea");

        // Relationships
        builder.HasOne(e => e.JobPosting)
            .WithMany(p => p.Applications)
            .HasForeignKey(e => e.JobPostingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.StatusChanges)
            .WithOne(sc => sc.Application)
            .HasForeignKey(sc => sc.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
