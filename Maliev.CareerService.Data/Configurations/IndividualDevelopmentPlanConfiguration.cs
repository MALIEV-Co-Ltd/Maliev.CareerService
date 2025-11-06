using Maliev.CareerService.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.CareerService.Data.Configurations;

/// <summary>
/// EF Core configuration for IndividualDevelopmentPlan entity
/// </summary>
public class IndividualDevelopmentPlanConfiguration : IEntityTypeConfiguration<IndividualDevelopmentPlan>
{
    public void Configure(EntityTypeBuilder<IndividualDevelopmentPlan> builder)
    {
        // Table name (will be converted to snake_case by convention)
        builder.ToTable("individual_development_plans");

        // Primary key
        builder.HasKey(idp => idp.Id);

        // Properties
        builder.Property(idp => idp.EmployeeId)
            .IsRequired();

        builder.Property(idp => idp.PlanYear)
            .IsRequired();

        builder.Property(idp => idp.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(idp => idp.SubmittedAt)
            .IsRequired(false);

        builder.Property(idp => idp.ApprovedAt)
            .IsRequired(false);

        builder.Property(idp => idp.ApprovedBy)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(idp => idp.EmployeeId);

        builder.HasIndex(idp => new { idp.EmployeeId, idp.PlanYear })
            .IsUnique()
            .HasDatabaseName("ix_individual_development_plans_employee_id_plan_year_unique");

        builder.HasIndex(idp => idp.Status);

        // Relationships
        builder.HasMany(idp => idp.Goals)
            .WithOne(goal => goal.Idp)
            .HasForeignKey(goal => goal.IdpId)
            .OnDelete(DeleteBehavior.Cascade);

        // Optimistic concurrency
        builder.Property(idp => idp.RowVersion)
            .IsRowVersion()
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValueSql("'\\x00000000000000000001'::bytea");
    }
}
