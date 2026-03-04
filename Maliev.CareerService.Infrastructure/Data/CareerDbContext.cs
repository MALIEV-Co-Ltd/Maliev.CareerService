using Maliev.CareerService.Infrastructure.Data.Configurations;
using Maliev.CareerService.Domain.Entities;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Infrastructure.Data;

public class CareerDbContext : DbContext
{
    public CareerDbContext(DbContextOptions<CareerDbContext> options) : base(options)
    {
    }

    public DbSet<TrainingRecord> TrainingRecords => Set<TrainingRecord>();
    public DbSet<TrainingProgram> TrainingPrograms => Set<TrainingProgram>();
    public DbSet<EmployeeTrainingEnrollment> EmployeeTrainingEnrollments => Set<EmployeeTrainingEnrollment>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<ApplicationStatusChange> ApplicationStatusChanges => Set<ApplicationStatusChange>();
    public DbSet<ApplicationDocument> ApplicationDocuments => Set<ApplicationDocument>();
    public DbSet<ELearningResource> ELearningResources => Set<ELearningResource>();
    public DbSet<IndividualDevelopmentPlan> IndividualDevelopmentPlans => Set<IndividualDevelopmentPlan>();
    public DbSet<EmployeeDevelopmentGoal> EmployeeDevelopmentGoals => Set<EmployeeDevelopmentGoal>();
    public DbSet<MandatoryTrainingRequirement> MandatoryTrainingRequirements => Set<MandatoryTrainingRequirement>();
    public DbSet<WorkLocation> WorkLocations => Set<WorkLocation>();
    public DbSet<JobPosition> JobPositions => Set<JobPosition>();
    public DbSet<JobPositionSkill> JobPositionSkills => Set<JobPositionSkill>();
    public DbSet<JobPositionLocation> JobPositionLocations => Set<JobPositionLocation>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<OutboxState> OutboxStates => Set<OutboxState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CareerDbContext).Assembly);

        // Configure MassTransit outbox entities as keyless
        modelBuilder.Entity<OutboxMessage>().HasNoKey();
        modelBuilder.Entity<OutboxState>().HasNoKey();
    }
}
