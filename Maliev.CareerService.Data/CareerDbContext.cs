using Maliev.CareerService.Data.Configurations;
using Maliev.CareerService.Data.Models;
using Maliev.CareerService.Data.Models.Base;
using Microsoft.EntityFrameworkCore;
using Maliev.Aspire.ServiceDefaults.Database;

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

    // Feature 003: Training Records and Skills Migration
    public DbSet<TrainingRecord> TrainingRecords => Set<TrainingRecord>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<MandatoryTrainingRequirement> MandatoryTrainingRequirements => Set<MandatoryTrainingRequirement>();

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

                    // Apply PostgreSQL snake_case naming convention globally
                    SnakeCaseNamingHelper.ApplySnakeCaseNaming(modelBuilder);
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

        // Feature 003: Training Records and Skills Migration configurations
        modelBuilder.ApplyConfiguration(new TrainingRecordConfiguration());
        modelBuilder.ApplyConfiguration(new SkillConfiguration());
        modelBuilder.ApplyConfiguration(new MandatoryTrainingRequirementConfiguration());
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
                // Only set timestamps if not explicitly provided (i.e., still at default)
                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = now;
                }
                if (entry.Entity.UpdatedAt == default)
                {
                    entry.Entity.UpdatedAt = now;
                }
                // CreatedBy and UpdatedBy should be set by the application layer

                // Initialize RowVersion for new entities (start at 1)
                if (entry.Entity.RowVersion == null || entry.Entity.RowVersion.Length == 0)
                {
                    entry.Entity.RowVersion = BitConverter.GetBytes(1L);
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                // UpdatedBy should be set by the application layer

                // Update RowVersion for concurrency control (PostgreSQL doesn't auto-increment like SQL Server)
                UpdateRowVersion(entry.Entity);

                // Mark RowVersion as modified so EF Core knows to persist it to the database
                entry.Property(nameof(BaseEntity.RowVersion)).IsModified = true;
            }
        }
    }

    /// <summary>
    /// Update RowVersion to a new value for concurrency control
    /// PostgreSQL doesn't auto-increment rowversion like SQL Server, so we handle it manually
    /// </summary>
    private static void UpdateRowVersion(BaseEntity entity)
    {
        // Get current RowVersion or initialize to empty array if null
        var currentVersion = entity.RowVersion ?? Array.Empty<byte>();

        // Ensure we have at least 8 bytes for a 64-bit integer
        long versionNumber;
        if (currentVersion.Length >= 8)
        {
            // Treat as a 64-bit integer and increment
            versionNumber = BitConverter.ToInt64(currentVersion, 0);
        }
        else if (currentVersion.Length > 0)
        {
            // Pad with zeros to make 8 bytes
            var paddedVersion = new byte[8];
            Array.Copy(currentVersion, 0, paddedVersion, 0, currentVersion.Length);
            versionNumber = BitConverter.ToInt64(paddedVersion, 0);
        }
        else
        {
            // Start from 0 if null or empty
            versionNumber = 0;
        }

        // Increment and convert back to byte array
        versionNumber++;
        entity.RowVersion = BitConverter.GetBytes(versionNumber);
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
