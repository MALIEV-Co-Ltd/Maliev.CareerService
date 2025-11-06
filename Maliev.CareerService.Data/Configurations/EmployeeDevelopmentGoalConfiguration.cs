using Maliev.CareerService.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.CareerService.Data.Configurations;

/// <summary>
/// EF Core configuration for EmployeeDevelopmentGoal entity
/// </summary>
public class EmployeeDevelopmentGoalConfiguration : IEntityTypeConfiguration<EmployeeDevelopmentGoal>
{
    public void Configure(EntityTypeBuilder<EmployeeDevelopmentGoal> builder)
    {
        // Table name (will be converted to snake_case by convention)
        builder.ToTable("employee_development_goals");

        // Primary key
        builder.HasKey(goal => goal.Id);

        // Properties
        builder.Property(goal => goal.IdpId)
            .IsRequired();

        builder.Property(goal => goal.GoalTitle)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(goal => goal.GoalDescription)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(goal => goal.Category)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(goal => goal.TargetDate)
            .IsRequired();

        builder.Property(goal => goal.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(goal => goal.CompletionDate)
            .IsRequired(false);

        builder.Property(goal => goal.ActionItems)
            .IsRequired(false)
            .HasMaxLength(4000);

        builder.Property(goal => goal.ProgressNotes)
            .IsRequired(false)
            .HasMaxLength(4000);

        // Indexes
        builder.HasIndex(goal => goal.IdpId);

        builder.HasIndex(goal => new { goal.IdpId, goal.Status })
            .HasDatabaseName("ix_employee_development_goals_idp_id_status");

        builder.HasIndex(goal => goal.TargetDate);

        // Relationships are configured in IndividualDevelopmentPlanConfiguration

        // Optimistic concurrency
        builder.Property(goal => goal.RowVersion)
            .IsRowVersion()
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValueSql("'\\x00000000000000000001'::bytea");
    }
}
