using Maliev.CareerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.CareerService.Infrastructure.Data.Configurations;

public class WorkLocationConfiguration : IEntityTypeConfiguration<WorkLocation>
{
    public void Configure(EntityTypeBuilder<WorkLocation> builder)
    {
        builder.ToTable("work_locations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Address).HasColumnName("address").HasMaxLength(500);
        builder.Property(x => x.City).HasColumnName("city").HasMaxLength(100).IsRequired();
        builder.Property(x => x.CountryId).HasColumnName("country_id");
        builder.Property(x => x.IsRemoteAllowed).HasColumnName("is_remote_allowed");
        builder.Property(x => x.IsHybrid).HasColumnName("is_hybrid");
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.CreatedDate).HasColumnName("created_date");
        builder.Property(x => x.ModifiedDate).HasColumnName("modified_date");
    }
}

public class JobPositionConfiguration : IEntityTypeConfiguration<JobPosition>
{
    public void Configure(EntityTypeBuilder<JobPosition> builder)
    {
        builder.ToTable("job_positions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Department).HasColumnName("department").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").IsRequired();
        builder.Property(x => x.Requirements).HasColumnName("requirements");
        builder.Property(x => x.Responsibilities).HasColumnName("responsibilities");
        builder.Property(x => x.EmploymentType).HasColumnName("employment_type").HasMaxLength(50).IsRequired();
        builder.Property(x => x.ExperienceLevel).HasColumnName("experience_level").HasMaxLength(50).IsRequired();
        builder.Property(x => x.SalaryRangeMin).HasColumnName("salary_range_min").HasPrecision(10, 2);
        builder.Property(x => x.SalaryRangeMax).HasColumnName("salary_range_max").HasPrecision(10, 2);
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.IsPublic).HasColumnName("is_public");
        builder.Property(x => x.CreatedDate).HasColumnName("created_date");
        builder.Property(x => x.ModifiedDate).HasColumnName("modified_date");
    }
}

public class JobPositionSkillConfiguration : IEntityTypeConfiguration<JobPositionSkill>
{
    public void Configure(EntityTypeBuilder<JobPositionSkill> builder)
    {
        builder.ToTable("job_position_skills");

        builder.HasKey(x => new { x.JobPositionId, x.SkillId });

        builder.Property(x => x.JobPositionId).HasColumnName("job_position_id").IsRequired();
        builder.Property(x => x.SkillId).HasColumnName("skill_id").IsRequired();
        builder.Property(x => x.RequiredLevel).HasColumnName("required_level").HasMaxLength(50).IsRequired();
        builder.Property(x => x.IsRequired).HasColumnName("is_required");

        builder.HasOne(x => x.JobPosition)
            .WithMany(x => x.JobPositionSkills)
            .HasForeignKey(x => x.JobPositionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Skill)
            .WithMany()
            .HasForeignKey(x => x.SkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class JobPositionLocationConfiguration : IEntityTypeConfiguration<JobPositionLocation>
{
    public void Configure(EntityTypeBuilder<JobPositionLocation> builder)
    {
        builder.ToTable("job_position_locations");

        builder.HasKey(x => new { x.JobPositionId, x.WorkLocationId });

        builder.Property(x => x.JobPositionId).HasColumnName("job_position_id").IsRequired();
        builder.Property(x => x.WorkLocationId).HasColumnName("work_location_id").IsRequired();

        builder.HasOne(x => x.JobPosition)
            .WithMany(x => x.JobPositionLocations)
            .HasForeignKey(x => x.JobPositionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.WorkLocation)
            .WithMany(x => x.JobPositionLocations)
            .HasForeignKey(x => x.WorkLocationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
