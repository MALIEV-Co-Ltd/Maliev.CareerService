using Maliev.CareerService.Domain.Entities;
using Maliev.CareerService.Infrastructure.ValueGenerators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.CareerService.Infrastructure.Data.Configurations;

public class JobPostingConfiguration : IEntityTypeConfiguration<JobPosting>
{
    public void Configure(EntityTypeBuilder<JobPosting> builder)
    {
        builder.ToTable("job_postings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PositionTitle).HasColumnName("position_title").HasMaxLength(200).IsRequired();
        builder.Property(x => x.PositionCode).HasColumnName("position_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Department).HasColumnName("department").HasMaxLength(100);
        builder.Property(x => x.Location).HasColumnName("location").HasMaxLength(200);
        builder.Property(x => x.EmploymentType).HasColumnName("employment_type").HasMaxLength(50).IsRequired();
        builder.Property(x => x.SalaryMin).HasColumnName("salary_min");
        builder.Property(x => x.SalaryMax).HasColumnName("salary_max");
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        builder.Property(x => x.Description).HasColumnName("description").IsRequired();
        builder.Property(x => x.Requirements).HasColumnName("requirements").IsRequired();
        builder.Property(x => x.Responsibilities).HasColumnName("responsibilities").IsRequired();
        builder.Property(x => x.ApplicationDeadline).HasColumnName("application_deadline");
        builder.Property(x => x.PublishedAt).HasColumnName("published_at");
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.RowVersion).HasColumnName("row_version").IsRequired();

        builder.HasIndex(x => x.PositionCode);
        builder.HasIndex(x => x.IsActive);
    }
}

public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.ToTable("job_applications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.JobPostingId).HasColumnName("job_posting_id").IsRequired();
        builder.Property(x => x.ApplicantFirstName).HasColumnName("applicant_first_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ApplicantLastName).HasColumnName("applicant_last_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ApplicantEmail).HasColumnName("applicant_email").HasMaxLength(255).IsRequired();
        builder.Property(x => x.ApplicantPhone).HasColumnName("applicant_phone").HasMaxLength(50);
        builder.Property(x => x.ApplicantCountryCode).HasColumnName("applicant_country_code").HasMaxLength(3);
        builder.Property(x => x.ResumeFileId).HasColumnName("resume_file_id").IsRequired();
        builder.Property(x => x.CoverLetter).HasColumnName("cover_letter");
        builder.Property(x => x.AdditionalFileIds).HasColumnName("additional_file_ids");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(x => x.AppliedAt).HasColumnName("applied_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.RowVersion)
            .HasColumnName("row_version")
            .IsRequired();

        builder.HasOne(x => x.JobPosting)
            .WithMany(x => x.Applications)
            .HasForeignKey(x => x.JobPostingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.JobPostingId);
        builder.HasIndex(x => x.Status);
    }
}
