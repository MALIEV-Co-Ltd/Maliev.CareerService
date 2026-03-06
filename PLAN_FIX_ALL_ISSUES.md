# Plan: Fix All Code Review Issues

## Overview

This plan addresses all issues found in the code review:
1. **High**: Duplicate service files (Clean Architecture migration)
2. **Medium**: xmin configuration missing explicit column type
3. **Low**: Numbered xminValue variables, magic string, test factory migration discovery

---

## Phase 1: Complete Clean Architecture Migration (High Priority)

### Step 1.1: Update Program.cs DI Registrations

**File**: `Maliev.CareerService.Api/Program.cs`

Change the using statements from:
```csharp
using Maliev.CareerService.Api.Services;
using Maliev.CareerService.Api.Services.External;
```

To:
```csharp
using Maliev.CareerService.Application.Services;
using Maliev.CareerService.Infrastructure.Services;
using Maliev.CareerService.Infrastructure.Services.External;
```

Update all DI registrations from concrete Api classes to Infrastructure classes (the interfaces are the same):
- `MarkdownService` → `Infrastructure.Services.MarkdownService`
- `JobPostingService` → `Infrastructure.Services.JobPostingService`
- `ApplicationService` → `Infrastructure.Services.ApplicationService`
- etc.

### Step 1.2: Update All Controllers to Use Application Models

**Files**: All controllers in `Maliev.CareerService.Api/Controllers/`

Change all `using` statements from:
```csharp
using Maliev.CareerService.Api.Models.XXX;
```

To:
```csharp
using Maliev.CareerService.Application.Models.XXX;
```

Controllers to update:
- ApplicationsController.cs
- DevelopmentGoalsController.cs
- DevelopmentPlansController.cs
- ELearningResourcesController.cs
- EnrollmentsController.cs
- JobPostingsController.cs
- MandatoryTrainingController.cs
- ReportsController.cs
- SkillsController.cs
- TrainingProgramsController.cs
- TrainingRecordsController.cs

### Step 1.3: Update Service Interfaces References (if needed)

**Files**: Check each controller for direct `using Maliev.CareerService.Api.Services;` that should become `using Maliev.CareerService.Application.Services;`

### Step 1.4: Delete Duplicate Files

Delete the following duplicate directories:
```bash
rm -rf Maliev.CareerService.Api/Services/        # Keep only Controllers/
rm -rf Maliev.CareerService.Api/Models/          # All DTOs are in Application
rm -rf Maliev.CareerService.Api/Mapping/        # All mappers are in Application
```

---

## Phase 2: Simplify xmin Concurrency with Native EF Core (Medium Priority)

### Step 2.1: Add Version Property to BaseEntity

**File**: `Maliev.CareerService.Domain/Entities/BaseEntity.cs`

Add the `[Timestamp]` attribute to a new `Version` property:
```csharp
using System.ComponentModel.DataAnnotations;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    [Timestamp]
    public uint Version { get; set; }
}
```

This automatically:
- Maps to PostgreSQL `xmin` system column
- Sets `xid` column type automatically
- Acts as concurrency token
- Generates value on add/update

### Step 2.2: Remove ConfigureXminProperty Helper

**File**: `Maliev.CareerService.Infrastructure/Data/CareerDbContext.cs`

Remove the `ConfigureXminProperty<T>` method and all calls to it in `OnModelCreating`. The `[Timestamp]` attribute handles everything.

### Step 2.3: Update All DTOs - Change RowVersion from string to uint

**Files**: All Response and Request DTOs with `RowVersion` property in `Maliev.CareerService.Application/Models/`

Change from:
```csharp
public string RowVersion { get; set; } = string.Empty;
```

To:
```csharp
public uint RowVersion { get; set; }
```

Files to update (22 matches from grep):
- TrainingPrograms/UpdateTrainingProgramRequest.cs
- TrainingPrograms/TrainingProgramResponse.cs
- JobPostings/UpdateJobPostingRequest.cs
- JobPostings/JobPostingResponse.cs
- Enrollments/TrainingEnrollmentResponse.cs
- Enrollments/MarkTrainingCompleteRequest.cs
- ELearningResources/ELearningResourceResponse.cs
- DevelopmentPlans/IDPResponse.cs
- DevelopmentPlans/UpdateIDPRequest.cs
- DevelopmentPlans/ApproveIDPRequest.cs
- DevelopmentGoals/UpdateGoalStatusRequest.cs
- DevelopmentGoals/UpdateDevelopmentGoalRequest.cs
- DevelopmentGoals/DevelopmentGoalResponse.cs
- Applications/UpdateApplicationStatusRequest.cs
- Applications/JobApplicationResponse.cs

### Step 2.4: Update DomainToDtoMapper

**File**: `Maliev.CareerService.Application/Mapping/DomainToDtoMapper.cs`

Change from:
```csharp
RowVersion = xmin.ToString()
```

To:
```csharp
RowVersion = entity.Version
```

Remove all shadow property access code:
```csharp
// DELETE THESE PATTERNS FROM ALL SERVICE FILES:
var xmin = _dbContext.Entry(entity).Property<uint>("xmin").CurrentValue;
// ...
_dbContext.Entry(entity).Property("xmin").OriginalValue = xminValue;
```

### Step 2.5: Update Infrastructure Services - Remove Manual xmin Access

**Files**: All service files in `Maliev.CareerService.Infrastructure/Services/`

Remove all manual shadow property access:
- Delete `uint.TryParse` with manual Property access
- Remove `out var xminValue` patterns
- EF Core now tracks `Version` property automatically

Example change - DELETE this pattern:
```csharp
if (!uint.TryParse(request.RowVersion, out var xminValue8))
{
    throw new ArgumentException("Invalid RowVersion format...");
}
_dbContext.Entry(posting).Property("xmin").OriginalValue = xminValue8;
```

REPLACE with simple assignment (no manual concurrency needed - EF Core handles it):
```csharp
// EF Core automatically handles concurrency check via [Timestamp] Version property
```

---

## Phase 3: Fix Low Severity Issues

### Step 3.1: Fix Numbered xminValue Variables (if not fixed by Phase 2)

If the xmin migration above doesn't apply (e.g., Api/Services files), rename all:
- `xminValue2` → `xminValue`
- `xminValue3` → `xminValue`
- `xminValue4` → `xminValue`
- `xminValue5` → `xminValue`
- `xminValue6` → `xminValue`
- `xminValue7` → `xminValue`
- `xminValue8` → `xminValue`

Files: DevelopmentGoalService.cs, DevelopmentPlanService.cs, EnrollmentService.cs, TrainingProgramService.cs, ApplicationService.cs, JobPostingService.cs

### Step 3.2: Fix Magic String in Program.cs

**File**: `Maliev.CareerService.Api/Program.cs:110`

Change from:
```csharp
if (app.Environment.EnvironmentName != "Testing")
```

To:
```csharp
if (!app.Environment.IsEnvironment("Testing"))
```

### Step 3.3: Fix Test Factory Migration Discovery

**File**: `Maliev.CareerService.Tests/Testing/BaseIntegrationTestFactory.cs`

Update `CreateDbContext()` method to specify migrations assembly:

```csharp
public TDbContext CreateDbContext()
{
    var connectionString = _postgresContainer!.GetConnectionString();
    var optionsBuilder = new DbOptionsBuilder<TDbContext>();
    optionsBuilder.UseNpgsql(
        connectionString,
        x => x.MigrationsAssembly("Maliev.CareerService.Infrastructure")
    );
    return (TDbContext)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options)!;
}
```

Then update `ApplyMigrationsAsync()` to use `MigrateAsync()` instead of `EnsureCreatedAsync()`:

```csharp
private async Task ApplyMigrationsAsync()
{
    await using var context = CreateDbContext();
    await context.Database.MigrateAsync();
}
```

Also remove the now-meaningless `ClearMigrationsHistoryAsync()` call in `InitializeAsync()` (or keep it if switching to MigrateAsync makes it meaningful again - but then don't drop the table, just delete all rows).

---

## Summary of Breaking Changes

| Change | Impact |
|--------|--------|
| Delete Api/Services, Api/Models, Api/Mapping | Large refactor - all controller references must change |
| RowVersion: string → uint | Breaking API change - clients must send uint instead of string |
| Remove manual xmin shadow property access | Simplifies all service update methods |
| EnsureCreatedAsync → MigrateAsync in tests | Tests now run real migrations |

---

## Verification Steps

After implementing:

1. **Build**: `dotnet build` - must succeed with 0 warnings
2. **Test**: `dotnet test --verbosity normal` - all tests pass
3. **Manual**: Verify a concurrent update triggers DbUpdateConcurrencyException correctly

---

## Files Summary

| Action | Count |
|--------|-------|
| Files to modify (Program.cs, controllers) | ~15 |
| Files to delete (Api duplicates) | ~50+ |
| DTOs with RowVersion type change | ~15 |
| Services to simplify (remove xmin manual code) | ~6 |
| xminValue rename fixes | 7 occurrences |
