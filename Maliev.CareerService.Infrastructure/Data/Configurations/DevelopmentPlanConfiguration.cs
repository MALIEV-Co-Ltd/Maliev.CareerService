using Maliev.CareerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.CareerService.Infrastructure.Data.Configurations;

public class IndividualDevelopmentPlanConfiguration : IEntityTypeConfiguration<IndividualDevelopmentPlan>
{
    public void Configure(EntityTypeBuilder<IndividualDevelopmentPlan> builder)
    {
        builder.ToTable("individual_development_plans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.EmployeeId).HasColumnName("employee_id").IsRequired();
        builder.Property(x => x.PlanYear).HasColumnName("plan_year").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(x => x.SubmittedAt).HasColumnName("submitted_at");
        builder.Property(x => x.ApprovedAt).HasColumnName("approved_at");
        builder.Property(x => x.ApprovedBy).HasColumnName("approved_by");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.RowVersion).HasColumnName("row_version");

        builder.HasIndex(x => new { x.EmployeeId, x.PlanYear }).IsUnique();
    }
}

public class EmployeeDevelopmentGoalConfiguration : IEntityTypeConfiguration<EmployeeDevelopmentGoal>
{
    public void Configure(EntityTypeBuilder<EmployeeDevelopmentGoal> builder)
    {
        builder.ToTable("employee_development_goals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.IdpId).HasColumnName("individual_development_plan_id").IsRequired();
        builder.Property(x => x.GoalTitle).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(x => x.GoalDescription).HasColumnName("description");
        builder.Property(x => x.Category).HasColumnName("category").HasMaxLength(100).IsRequired();
        builder.Property(x => x.TargetDate).HasColumnName("target_date");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(x => x.CompletionDate).HasColumnName("completed_at");
        builder.Property(x => x.ActionItems).HasColumnName("action_items");
        builder.Property(x => x.ProgressNotes).HasColumnName("progress_notes");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.RowVersion).HasColumnName("row_version");

        builder.HasOne(x => x.Idp)
            .WithMany(x => x.Goals)
            .HasForeignKey(x => x.IdpId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.IdpId);
    }
}
