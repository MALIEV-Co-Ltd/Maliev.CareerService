# Data Model: Training Records and Skills Migration

**Feature**: Training Records and Skills Migration
**Date**: 2025-12-28
**Branch**: `003-training-migration`

## Overview

This document defines the data model for training records, skills matrix, and mandatory training requirements being migrated from Employee Service to Career Service.

All entities inherit from `BaseEntity` which provides standard audit fields (Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted, RowVersion).

## Entities

### TrainingRecord

Represents completion of a training course by an employee.

```csharp
public class TrainingRecord : BaseEntity
{
    [Required]
    public Guid EmployeeId { get; set; }

    public Guid? TrainingProgramId { get; set; }  // FK to existing TrainingProgram

    [Required]
    [MaxLength(200)]
    public string CourseName { get; set; } = string.Empty;

    [Required]
    public DateTime CompletionDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public Guid? CertificateDocumentId { get; set; }  // Reference to Upload Service document

    [Required]
    public TrainingType TrainingType { get; set; }

    [MaxLength(200)]
    public string? Provider { get; set; }

    [Required]
    public TrainingStatus Status { get; set; } = TrainingStatus.Completed;

    [Range(0, 100)]
    public decimal? Score { get; set; }

    // Navigation properties
    public TrainingProgram? TrainingProgram { get; set; }
}
```

**Validation Rules** (from FR-003, FR-004, FR-020):
- CompletionDate cannot be in the future
- ExpirationDate must be after CompletionDate if provided
- Score must be between 0-100 if provided

**State Transitions** (FR-005):
- Completed → Expired (when ExpirationDate reached)
- InProgress → Completed (manual update)
- NotStarted → InProgress → Completed

**Indexes**:
```csharp
modelBuilder.Entity<TrainingRecord>(entity =>
{
    entity.HasIndex(e => e.EmployeeId).HasDatabaseName("idx_training_records_employee");
    entity.HasIndex(e => e.ExpirationDate).HasDatabaseName("idx_training_records_expiration");
    entity.HasIndex(e => e.TrainingProgramId).HasDatabaseName("idx_training_records_program");
});
```

---

### Skill

Represents an employee's skill and proficiency level.

```csharp
public class Skill : BaseEntity
{
    [Required]
    public Guid EmployeeId { get; set; }

    [Required]
    [MaxLength(100)]
    public string SkillName { get; set; } = string.Empty;

    [Required]
    [Range(1, 5)]
    public ProficiencyLevel ProficiencyLevel { get; set; }

    [Required]
    public DateTime LastAssessedDate { get; set; }

    public bool IsDevelopmentArea { get; set; } = false;

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
```

**Validation Rules** (from FR-007, FR-008):
- Unique constraint on (EmployeeId, SkillName) - no duplicate skills per employee
- LastAssessedDate automatically updated when ProficiencyLevel changes
- ProficiencyLevel must be 1-5 (validated by enum range)

**Constraints**:
```csharp
modelBuilder.Entity<Skill>(entity =>
{
    entity.HasIndex(e => new { e.EmployeeId, e.SkillName })
        .IsUnique()
        .HasDatabaseName("idx_skills_employee_name_unique");

    entity.HasIndex(e => e.EmployeeId).HasDatabaseName("idx_skills_employee");

    entity.HasIndex(e => e.IsDevelopmentArea)
        .HasFilter("is_development_area = true")
        .HasDatabaseName("idx_skills_development");
});
```

---

### MandatoryTrainingRequirement

Defines training that must be completed by specific employee groups.

```csharp
public class MandatoryTrainingRequirement : BaseEntity
{
    [Required]
    public Guid TrainingProgramId { get; set; }

    public Guid? DepartmentId { get; set; }  // null = all departments

    public Guid? PositionId { get; set; }     // null = all positions

    [Required]
    [Range(1, 365)]
    public int CompletionDeadlineDays { get; set; } = 30;

    [Range(1, 120)]
    public int? RecertificationMonths { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public TrainingProgram TrainingProgram { get; set; } = null!;
}
```

**Business Rules** (from FR-009 through FR-013):
- Null DepartmentId means requirement applies to all departments
- Null PositionId means requirement applies to all positions
- CompletionDeadlineDays is days from employee hire date
- RecertificationMonths triggers automatic re-enrollment if provided
- IsActive = false stops new assignments but preserves historical data

**Indexes**:
```csharp
modelBuilder.Entity<MandatoryTrainingRequirement>(entity =>
{
    entity.HasIndex(e => e.TrainingProgramId).HasDatabaseName("idx_mandatory_training_program");
    entity.HasIndex(e => e.DepartmentId).HasDatabaseName("idx_mandatory_training_dept");
    entity.HasIndex(e => e.PositionId).HasDatabaseName("idx_mandatory_training_position");
    entity.HasIndex(e => e.IsActive).HasDatabaseName("idx_mandatory_training_active");

    entity.HasOne(e => e.TrainingProgram)
        .WithMany()
        .HasForeignKey(e => e.TrainingProgramId)
        .OnDelete(DeleteBehavior.Restrict);  // Prevent deletion if requirements exist
});
```

---

## Enums

### TrainingType

```csharp
public enum TrainingType
{
    InPerson = 0,
    Online = 1,
    SelfPaced = 2,
    Workshop = 3,
    Certification = 4,
    External = 5
}
```

### TrainingStatus

```csharp
public enum TrainingStatus
{
    Completed = 0,
    InProgress = 1,
    NotStarted = 2,
    Expired = 3,
    Failed = 4
}
```

### ProficiencyLevel

```csharp
public enum ProficiencyLevel
{
    Beginner = 1,
    Elementary = 2,
    Intermediate = 3,
    Advanced = 4,
    Expert = 5
}
```

---

## Entity Relationships

### TrainingRecord Relationships

- **TrainingRecord** ←→ **TrainingProgram**: Many-to-One (optional)
  - A TrainingRecord MAY link to a TrainingProgram (if internal training)
  - A TrainingProgram can have many TrainingRecords
  - FK: TrainingProgramId (nullable)
  - Delete Behavior: Restrict (cannot delete TrainingProgram with records)

### MandatoryTrainingRequirement Relationships

- **MandatoryTrainingRequirement** → **TrainingProgram**: Many-to-One (required)
  - A requirement MUST reference a TrainingProgram
  - A TrainingProgram can have many requirements
  - FK: TrainingProgramId (required)
  - Delete Behavior: Restrict (cannot delete TrainingProgram with requirements)

### No Direct Skill Relationships

- Skills are standalone entities linked only by EmployeeId (external reference)

---

## Database Schema (PostgreSQL)

### training_records Table

```sql
CREATE TABLE training_records (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    employee_id UUID NOT NULL,
    training_program_id UUID NULL REFERENCES training_programs(id) ON DELETE RESTRICT,
    course_name VARCHAR(200) NOT NULL,
    completion_date TIMESTAMP WITH TIME ZONE NOT NULL,
    expiration_date TIMESTAMP WITH TIME ZONE NULL,
    certificate_document_id UUID NULL,
    training_type INTEGER NOT NULL,
    provider VARCHAR(200) NULL,
    status INTEGER NOT NULL DEFAULT 0,
    score DECIMAL(5,2) NULL CHECK (score >= 0 AND score <= 100),

    -- Audit fields (from BaseEntity)
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    row_version BYTEA NOT NULL,

    CONSTRAINT chk_training_record_expiration CHECK (
        expiration_date IS NULL OR expiration_date > completion_date
    ),
    CONSTRAINT chk_training_record_completion_date CHECK (
        completion_date <= NOW()
    )
);

CREATE INDEX idx_training_records_employee ON training_records(employee_id);
CREATE INDEX idx_training_records_expiration ON training_records(expiration_date) WHERE expiration_date IS NOT NULL;
CREATE INDEX idx_training_records_program ON training_records(training_program_id);
CREATE INDEX idx_training_records_status ON training_records(status);
```

### skills Table

```sql
CREATE TABLE skills (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    employee_id UUID NOT NULL,
    skill_name VARCHAR(100) NOT NULL,
    proficiency_level INTEGER NOT NULL CHECK (proficiency_level BETWEEN 1 AND 5),
    last_assessed_date TIMESTAMP WITH TIME ZONE NOT NULL,
    is_development_area BOOLEAN NOT NULL DEFAULT FALSE,
    notes TEXT NULL,

    -- Audit fields (from BaseEntity)
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    row_version BYTEA NOT NULL,

    CONSTRAINT idx_skills_employee_name_unique UNIQUE (employee_id, skill_name)
);

CREATE INDEX idx_skills_employee ON skills(employee_id);
CREATE INDEX idx_skills_development ON skills(is_development_area) WHERE is_development_area = TRUE;
```

### mandatory_training_requirements Table

```sql
CREATE TABLE mandatory_training_requirements (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    training_program_id UUID NOT NULL REFERENCES training_programs(id) ON DELETE RESTRICT,
    department_id UUID NULL,
    position_id UUID NULL,
    completion_deadline_days INTEGER NOT NULL DEFAULT 30 CHECK (completion_deadline_days BETWEEN 1 AND 365),
    recertification_months INTEGER NULL CHECK (recertification_months BETWEEN 1 AND 120),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    -- Audit fields (from BaseEntity)
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    row_version BYTEA NOT NULL
);

CREATE INDEX idx_mandatory_training_program ON mandatory_training_requirements(training_program_id);
CREATE INDEX idx_mandatory_training_dept ON mandatory_training_requirements(department_id);
CREATE INDEX idx_mandatory_training_position ON mandatory_training_requirements(position_id);
CREATE INDEX idx_mandatory_training_active ON mandatory_training_requirements(is_active) WHERE is_active = TRUE;
```

---

## EF Core Configuration

### CareerDbContext Updates

```csharp
public class CareerDbContext : DbContext
{
    // Existing DbSets
    public DbSet<TrainingProgram> TrainingPrograms => Set<TrainingProgram>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    // NEW DbSets
    public DbSet<TrainingRecord> TrainingRecords => Set<TrainingRecord>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<MandatoryTrainingRequirement> MandatoryTrainingRequirements => Set<MandatoryTrainingRequirement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureTrainingRecords(modelBuilder);
        ConfigureSkills(modelBuilder);
        ConfigureMandatoryTrainingRequirements(modelBuilder);
    }

    private static void ConfigureTrainingRecords(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TrainingRecord>(entity =>
        {
            entity.ToTable("training_records");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.CourseName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Provider).HasMaxLength(200);
            entity.Property(e => e.Score).HasPrecision(5, 2);

            entity.HasIndex(e => e.EmployeeId).HasDatabaseName("idx_training_records_employee");
            entity.HasIndex(e => e.ExpirationDate).HasDatabaseName("idx_training_records_expiration");
            entity.HasIndex(e => e.TrainingProgramId).HasDatabaseName("idx_training_records_program");
            entity.HasIndex(e => e.Status).HasDatabaseName("idx_training_records_status");

            entity.HasOne(e => e.TrainingProgram)
                .WithMany()
                .HasForeignKey(e => e.TrainingProgramId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });
    }

    private static void ConfigureSkills(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Skill>(entity =>
        {
            entity.ToTable("skills");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.SkillName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasIndex(e => new { e.EmployeeId, e.SkillName })
                .IsUnique()
                .HasDatabaseName("idx_skills_employee_name_unique");

            entity.HasIndex(e => e.EmployeeId).HasDatabaseName("idx_skills_employee");

            entity.HasIndex(e => e.IsDevelopmentArea)
                .HasFilter("is_development_area = true")
                .HasDatabaseName("idx_skills_development");
        });
    }

    private static void ConfigureMandatoryTrainingRequirements(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MandatoryTrainingRequirement>(entity =>
        {
            entity.ToTable("mandatory_training_requirements");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.TrainingProgramId).HasDatabaseName("idx_mandatory_training_program");
            entity.HasIndex(e => e.DepartmentId).HasDatabaseName("idx_mandatory_training_dept");
            entity.HasIndex(e => e.PositionId).HasDatabaseName("idx_mandatory_training_position");
            entity.HasIndex(e => e.IsActive)
                .HasFilter("is_active = true")
                .HasDatabaseName("idx_mandatory_training_active");

            entity.HasOne(e => e.TrainingProgram)
                .WithMany()
                .HasForeignKey(e => e.TrainingProgramId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
```

---

## Migration Commands

```bash
# Create migration
dotnet ef migrations add AddTrainingMigrationEntities \
    --project Maliev.CareerService.Data \
    --startup-project Maliev.CareerService.Api

# Apply migration (local)
dotnet ef database update \
    --project Maliev.CareerService.Data \
    --startup-project Maliev.CareerService.Api

# Generate SQL script (for production)
dotnet ef migrations script \
    --project Maliev.CareerService.Data \
    --startup-project Maliev.CareerService.Api \
    --output migration.sql
```

---

## Data Migration from Employee Service

### Export Script (Employee Service database)

```sql
-- Export training records
COPY (
    SELECT
        id,
        employee_id,
        course_name,
        completion_date,
        expiration_date,
        certificate_document_id,
        training_type,
        provider,
        status,
        score,
        created_at,
        created_by,
        updated_at,
        updated_by
    FROM training_records
    WHERE is_deleted = FALSE
) TO '/tmp/training_records.csv' WITH CSV HEADER;

-- Export skills
COPY (
    SELECT
        id,
        employee_id,
        skill_name,
        proficiency_level,
        last_assessed_date,
        is_development_area,
        notes,
        created_at,
        created_by,
        updated_at,
        updated_by
    FROM skills
    WHERE is_deleted = FALSE
) TO '/tmp/skills.csv' WITH CSV HEADER;

-- Export mandatory requirements
COPY (
    SELECT
        id,
        training_program_id,
        department_id,
        position_id,
        completion_deadline_days,
        recertification_months,
        is_active,
        created_at,
        created_by,
        updated_at,
        updated_by
    FROM mandatory_training_requirements
) TO '/tmp/mandatory_training.csv' WITH CSV HEADER;
```

### Import Script (Career Service database)

```sql
-- Import to Career Service
COPY training_records (
    id, employee_id, course_name, completion_date, expiration_date,
    certificate_document_id, training_type, provider, status, score,
    created_at, created_by, updated_at, updated_by
) FROM '/tmp/training_records.csv' WITH CSV HEADER;

COPY skills (
    id, employee_id, skill_name, proficiency_level, last_assessed_date,
    is_development_area, notes, created_at, created_by, updated_at, updated_by
) FROM '/tmp/skills.csv' WITH CSV HEADER;

COPY mandatory_training_requirements (
    id, training_program_id, department_id, position_id,
    completion_deadline_days, recertification_months, is_active,
    created_at, created_by, updated_at, updated_by
) FROM '/tmp/mandatory_training.csv' WITH CSV HEADER;

-- Link training records to training programs where possible
UPDATE training_records tr
SET training_program_id = tp.id
FROM training_programs tp
WHERE tr.course_name = tp.name
AND tr.training_program_id IS NULL;
```

---

## Data Integrity Checks

Post-migration validation:

```sql
-- Verify record counts
SELECT 'training_records' AS table_name, COUNT(*) AS record_count FROM training_records
UNION ALL
SELECT 'skills', COUNT(*) FROM skills
UNION ALL
SELECT 'mandatory_training_requirements', COUNT(*) FROM mandatory_training_requirements;

-- Verify foreign key relationships
SELECT COUNT(*) AS orphaned_training_records
FROM training_records tr
WHERE tr.training_program_id IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM training_programs tp WHERE tp.id = tr.training_program_id);

SELECT COUNT(*) AS orphaned_mandatory_requirements
FROM mandatory_training_requirements mtr
WHERE NOT EXISTS (SELECT 1 FROM training_programs tp WHERE tp.id = mtr.training_program_id);

-- Verify uniqueness constraints
SELECT employee_id, skill_name, COUNT(*)
FROM skills
GROUP BY employee_id, skill_name
HAVING COUNT(*) > 1;  -- Should return 0 rows

-- Verify audit fields populated
SELECT COUNT(*) AS records_missing_audit_fields
FROM training_records
WHERE created_by IS NULL OR updated_by IS NULL OR created_at IS NULL OR updated_at IS NULL;
```

---

## Summary

- **3 new entities**: TrainingRecord, Skill, MandatoryTrainingRequirement
- **3 new enums**: TrainingType, TrainingStatus, ProficiencyLevel
- All entities inherit from BaseEntity (audit fields included)
- All constraints and indexes defined
- Migration and data import scripts provided
- Data integrity validation queries included
