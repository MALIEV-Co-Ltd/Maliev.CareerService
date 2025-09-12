using Maliev.CareerService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Data.DbContexts;

public class CareerDbContext : DbContext
{
    public CareerDbContext(DbContextOptions<CareerDbContext> options) : base(options)
    {
    }

    public DbSet<JobPosition> JobPositions => Set<JobPosition>();
    public DbSet<WorkLocation> WorkLocations => Set<WorkLocation>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<ApplicationDocument> ApplicationDocuments => Set<ApplicationDocument>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<JobPositionLocation> JobPositionLocations => Set<JobPositionLocation>();
    public DbSet<JobPositionSkill> JobPositionSkills => Set<JobPositionSkill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure JobPosition
        modelBuilder.Entity<JobPosition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Department).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.EmploymentType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ExperienceLevel).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.SalaryRangeMin).HasColumnType("decimal(10,2)");
            entity.Property(e => e.SalaryRangeMax).HasColumnType("decimal(10,2)");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ModifiedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Index for performance
            entity.HasIndex(e => e.Department);
            entity.HasIndex(e => e.EmploymentType);
            entity.HasIndex(e => e.ExperienceLevel);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsPublic);
        });

        // Configure WorkLocation
        modelBuilder.Entity<WorkLocation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ModifiedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Index for performance
            entity.HasIndex(e => e.City);
            entity.HasIndex(e => e.CountryId);
            entity.HasIndex(e => e.IsActive);
        });

        // Configure JobApplication
        modelBuilder.Entity<JobApplication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ApplicantEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ApplicantName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ApplicantPhone).HasMaxLength(50);
            entity.Property(e => e.LinkedInProfile).HasMaxLength(500);
            entity.Property(e => e.PortfolioUrl).HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Submitted");
            entity.Property(e => e.ApplicationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastStatusChange).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ModifiedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Foreign key relationship
            entity.HasOne(e => e.JobPosition)
                .WithMany(e => e.JobApplications)
                .HasForeignKey(e => e.JobPositionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            entity.HasIndex(e => e.JobPositionId);
            entity.HasIndex(e => e.ApplicantEmail);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ApplicationDate);
        });

        // Configure ApplicationDocument
        modelBuilder.Entity<ApplicationDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocumentType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.GcsBucket).IsRequired().HasMaxLength(100);
            entity.Property(e => e.GcsObjectName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.GcsUri).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.MimeType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UploadDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Description).HasMaxLength(500);

            // Foreign key relationship
            entity.HasOne(e => e.JobApplication)
                .WithMany(e => e.ApplicationDocuments)
                .HasForeignKey(e => e.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            entity.HasIndex(e => e.JobApplicationId);
            entity.HasIndex(e => e.DocumentType);
        });

        // Configure Skill
        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ModifiedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Unique constraint on skill name
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsActive);
        });

        // Configure JobPositionLocation (many-to-many)
        modelBuilder.Entity<JobPositionLocation>(entity =>
        {
            entity.HasKey(e => new { e.JobPositionId, e.WorkLocationId });

            entity.HasOne(e => e.JobPosition)
                .WithMany(e => e.JobPositionLocations)
                .HasForeignKey(e => e.JobPositionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.WorkLocation)
                .WithMany(e => e.JobPositionLocations)
                .HasForeignKey(e => e.WorkLocationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure JobPositionSkill (many-to-many with additional properties)
        modelBuilder.Entity<JobPositionSkill>(entity =>
        {
            entity.HasKey(e => new { e.JobPositionId, e.SkillId });
            entity.Property(e => e.RequiredLevel).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.JobPosition)
                .WithMany(e => e.JobPositionSkills)
                .HasForeignKey(e => e.JobPositionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Skill)
                .WithMany(e => e.JobPositionSkills)
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entities = ChangeTracker.Entries()
            .Where(x => x.Entity is JobPosition or WorkLocation or JobApplication or Skill && 
                       (x.State == EntityState.Added || x.State == EntityState.Modified));

        foreach (var entity in entities)
        {
            var now = DateTime.UtcNow;

            if (entity.State == EntityState.Added)
            {
                ((dynamic)entity.Entity).CreatedDate = now;
            }

            ((dynamic)entity.Entity).ModifiedDate = now;
        }
    }
}