using Maliev.CareerService.Data.Configurations;
using Maliev.CareerService.Data.Models;
using Maliev.CareerService.Data.Models.Base;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Data;

/// <summary>
/// Database context for Career Service
/// </summary>
public class CareerDbContext(DbContextOptions<CareerDbContext> options) : DbContext(options)
{

    // User Story 1: Job Application Lifecycle
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<ApplicationStatusChange> ApplicationStatusChanges => Set<ApplicationStatusChange>();

    // User Story 2: Employee Learning and Development Portal
    public DbSet<TrainingProgram> TrainingPrograms => Set<TrainingProgram>();
    public DbSet<EmployeeTrainingEnrollment> EmployeeTrainingEnrollments => Set<EmployeeTrainingEnrollment>();
    public DbSet<ELearningResource> ELearningResources => Set<ELearningResource>();

    // User Story 4: Employee Self-Development Planning
    public DbSet<IndividualDevelopmentPlan> IndividualDevelopmentPlans => Set<IndividualDevelopmentPlan>();
    public DbSet<EmployeeDevelopmentGoal> EmployeeDevelopmentGoals => Set<EmployeeDevelopmentGoal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply snake_case naming convention to all tables and columns
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Convert table names to snake_case
            var tableName = entity.GetTableName();
            if (!string.IsNullOrEmpty(tableName))
            {
                entity.SetTableName(ToSnakeCase(tableName));
            }

            // Convert column names to snake_case
            foreach (var property in entity.GetProperties())
            {
                var columnName = property.GetColumnName();
                if (!string.IsNullOrEmpty(columnName))
                {
                    property.SetColumnName(ToSnakeCase(columnName));
                }
            }

            // Convert foreign key names to snake_case
            foreach (var key in entity.GetForeignKeys())
            {
                var constraintName = key.GetConstraintName();
                if (!string.IsNullOrEmpty(constraintName))
                {
                    key.SetConstraintName(ToSnakeCase(constraintName));
                }
            }

            // Convert index names to snake_case
            foreach (var index in entity.GetIndexes())
            {
                var indexName = index.GetDatabaseName();
                if (!string.IsNullOrEmpty(indexName))
                {
                    index.SetDatabaseName(ToSnakeCase(indexName));
                }
            }
        }

        // Apply global query filter for soft deletes
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filter = System.Linq.Expressions.Expression.Lambda(
                    System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false)),
                    parameter);

                entityType.SetQueryFilter(filter);
            }
        }

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new JobPostingConfiguration());
        modelBuilder.ApplyConfiguration(new JobApplicationConfiguration());
        modelBuilder.ApplyConfiguration(new ApplicationStatusChangeConfiguration());
        modelBuilder.ApplyConfiguration(new TrainingProgramConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeTrainingEnrollmentConfiguration());
        modelBuilder.ApplyConfiguration(new ELearningResourceConfiguration());
        modelBuilder.ApplyConfiguration(new IndividualDevelopmentPlanConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeDevelopmentGoalConfiguration());
    }

    /// <summary>
    /// Override SaveChanges to automatically set audit fields
    /// </summary>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically set audit fields
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Update audit fields for tracked entities
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
                // CreatedBy and UpdatedBy should be set by the application layer
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                // UpdatedBy should be set by the application layer
            }
        }
    }

    /// <summary>
    /// Convert PascalCase to snake_case
    /// </summary>
    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new System.Text.StringBuilder();
        result.Append(char.ToLowerInvariant(input[0]));

        for (int i = 1; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]))
            {
                result.Append('_');
                result.Append(char.ToLowerInvariant(input[i]));
            }
            else
            {
                result.Append(input[i]);
            }
        }

        return result.ToString();
    }
}
