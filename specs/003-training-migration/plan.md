# Implementation Plan: Training Records and Skills Migration

**Branch**: `003-training-migration` | **Date**: 2025-12-28 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/003-training-migration/spec.md`

## Summary

Migrate training-related functionality (employee training records, skills matrix, mandatory training requirements, and compliance reporting) from Employee Service to Career Service. This extends the existing Career Service (which already handles training programs and enrollments) to include completion tracking, skills management, and compliance automation. No new technology stack required - implementation uses existing .NET 10, PostgreSQL, MassTransit, and Redis infrastructure.

## Technical Context

**Language/Version**: .NET 10.0 (C# 14)
**Primary Dependencies**: ASP.NET Core, Entity Framework Core, MassTransit (RabbitMQ), Npgsql (PostgreSQL), Maliev.Aspire.ServiceDefaults
**Storage**: PostgreSQL with Entity Framework Core (same database as existing Career Service)
**Testing**: xUnit with Testcontainers (PostgreSQL, RabbitMQ, Redis)
**Target Platform**: Linux containers (Docker)
**Project Type**: Microservice extension (adding to existing 3-project structure: Api, Data, Tests)
**Performance Goals**:
- Compliance reports under 5 seconds for 5,000 employees
- 100+ concurrent HR admin operations
- Event publishing within 30 seconds
**Constraints**:
- No FluentValidation (use DataAnnotations)
- No AutoMapper (explicit mapping)
- No FluentAssertions (xUnit Assert only)
- Zero warnings policy
- Real infrastructure testing only (Testcontainers)
**Scale/Scope**:
- Support 5,000+ employees
- 50+ skills per employee
- Estimated 5,000 LOC across 30 new files

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Service Autonomy
✅ **PASS** - Career Service owns training_records, skills, and mandatory_training_requirements tables. No direct database access to Employee Service. Integration via events only (EmployeeCreatedEvent consumed).

### Explicit Contracts
✅ **PASS** - OpenAPI documentation required via Scalar. All new endpoints documented. Integration events defined with clear contracts.

### Test-First Development
✅ **PASS** - Tests must be written immediately after spec approval, before implementation. Red-Green-Refactor cycle mandatory.

### Real Infrastructure Testing
✅ **PASS** - All tests use Testcontainers for PostgreSQL, RabbitMQ, and Redis. No in-memory substitutes allowed.

### Auditability & Observability
✅ **PASS** - All entities include CreatedDate, CreatedBy, ModifiedDate, ModifiedBy fields per clarification session. Structured JSON logging with required LogLevel configuration.

### Security & Compliance
✅ **PASS** - Authorization model defined: employees read-only their own records, HR admins full access, managers limited to direct reports. Integration with existing IAM service for permission checks.

### Secrets Management
✅ **PASS** - Uses existing Google Secret Manager integration. No new secrets required.

### Zero Warnings Policy
✅ **PASS** - Build configuration already treats warnings as errors.

### Clean Project Artifacts
✅ **PASS** - No additional root-level markdown files. CODEOWNERS already exists.

### Docker Best Practices
✅ **PASS** - Uses existing Dockerfile in Maliev.CareerService.Api/ with built-in `app` user, BuildKit secrets for NuGet.

### .NET Aspire Integration
✅ **PASS** - Already integrated via Maliev.Aspire.ServiceDefaults NuGet package. No changes needed.

### Code Quality & Library Standards
✅ **PASS** - No AutoMapper, FluentValidation, or FluentAssertions. Uses standard .NET patterns.

### Project Structure & Naming
✅ **PASS** - Flat structure at root: Maliev.CareerService.Api, Maliev.CareerService.Data, Maliev.CareerService.Tests.

### CI/CD Standards
✅ **PASS** - Uses existing ci-develop.yml, ci-staging.yml, ci-main.yml workflows. Testcontainers for integration tests.

### Business Metrics & Analytics
✅ **PASS** - Compliance metrics defined in success criteria (SC-003 through SC-010). Background services publish business-relevant metrics.

**Constitution Compliance**: ✅ ALL GATES PASSED - No violations, no justification table needed.

## Project Structure

### Documentation (this feature)

```text
specs/003-training-migration/
├── spec.md              # Feature specification (completed)
├── plan.md              # This file (/speckit.plan output)
├── research.md          # Phase 0 output (to be generated)
├── data-model.md        # Phase 1 output (to be generated)
├── quickstart.md        # Phase 1 output (to be generated)
├── contracts/           # Phase 1 output (to be generated)
│   ├── training-records-api.yaml
│   ├── skills-api.yaml
│   ├── mandatory-training-api.yaml
│   └── events.yaml
└── tasks.md             # Phase 2 output (/speckit.tasks - NOT this command)
```

### Source Code (repository root)

**Existing Structure** (no changes to layout):
```text
Maliev.CareerService/
├── Maliev.CareerService.Api/
│   ├── Controllers/
│   │   ├── (existing: JobPostingsController, etc.)
│   │   ├── TrainingRecordsController.cs     # NEW
│   │   ├── SkillsController.cs              # NEW
│   │   └── MandatoryTrainingController.cs   # NEW
│   ├── Program.cs                           # MODIFY
│   ├── Dockerfile                           # NO CHANGE
│   └── appsettings.json                     # NO CHANGE
├── Maliev.CareerService.Data/
│   ├── CareerDbContext.cs                   # MODIFY (add DbSets)
│   ├── Entities/
│   │   ├── (existing: TrainingProgram, Enrollment, etc.)
│   │   ├── TrainingRecord.cs                # NEW
│   │   ├── Skill.cs                         # NEW
│   │   └── MandatoryTrainingRequirement.cs  # NEW
│   ├── Repositories/
│   │   ├── TrainingRecordRepository.cs      # NEW
│   │   ├── SkillRepository.cs               # NEW
│   │   └── MandatoryTrainingRepository.cs   # NEW
│   ├── Commands/
│   │   ├── RecordTrainingCompletionCommand.cs   # NEW
│   │   ├── UpdateSkillCommand.cs                # NEW
│   │   └── CreateMandatoryRequirementCommand.cs # NEW
│   ├── Queries/
│   │   ├── GetTrainingRecordsQuery.cs           # NEW
│   │   ├── GetSkillsQuery.cs                    # NEW
│   │   └── GetTrainingComplianceReportQuery.cs  # NEW
│   ├── DTOs/
│   │   ├── TrainingRecordDto.cs                 # NEW
│   │   ├── EmployeeSkillDto.cs                  # NEW
│   │   └── TrainingComplianceReportDto.cs       # NEW
│   ├── BackgroundServices/
│   │   ├── CertificationExpirationReminderBackgroundService.cs   # NEW
│   │   └── OverdueTrainingEscalationBackgroundService.cs         # NEW
│   ├── Consumers/
│   │   └── EmployeeCreatedEventConsumer.cs      # NEW
│   └── Migrations/                              # NEW MIGRATION
└── Maliev.CareerService.Tests/
    ├── Integration/
    │   ├── TrainingRecordsControllerTests.cs    # NEW
    │   ├── SkillsControllerTests.cs             # NEW
    │   ├── MandatoryTrainingControllerTests.cs  # NEW
    │   ├── CertificationExpirationTests.cs      # NEW
    │   └── OverdueTrainingEscalationTests.cs    # NEW
    └── Unit/
        ├── TrainingRecordValidationTests.cs     # NEW
        ├── SkillValidationTests.cs              # NEW
        └── ComplianceReportTests.cs             # NEW
```

**Structure Decision**: This is a service extension, not a new service. We add new controllers, entities, repositories, commands/queries, and background services to the existing 3-project flat structure. No additional projects needed. All new files follow existing patterns in Career Service.

## Complexity Tracking

*No constitutional violations - table not applicable.*

## Phase 0: Research

**Research questions to resolve**:

1. **Background Service Scheduling**: What scheduling library/pattern does Career Service use for daily jobs? (For CertificationExpirationReminderBackgroundService and OverdueTrainingEscalationBackgroundService)
2. **Notification System**: How does Career Service send notifications to employees and managers? (Email service integration pattern, event-based notifications, etc.)
3. **Employee Data Access**: How does Career Service obtain employee department/position information for mandatory training assignment? (Direct API call to Employee Service, cached data, event-driven sync?)
4. **Manager Hierarchy**: How does Career Service determine manager-employee relationships for authorization checks? (Employee Service API, IAM service, cached organizational structure?)
5. **Event Retry Mechanism**: What is the standard retry/queue pattern for failed event processing in Career Service? (MassTransit built-in retry, custom retry logic, dead letter queue handling?)
6. **Audit Field Population**: How are CreatedBy/ModifiedBy fields populated in existing Career Service entities? (ClaimsPrincipal integration, middleware pattern, repository pattern?)

**Research output**: All answers documented in `research.md` with decision rationale.

## Phase 1: Design

**Data model artifacts** (`data-model.md`):
- TrainingRecord entity with all fields from spec
- Skill entity with unique constraint on (employee_id, skill_name)
- MandatoryTrainingRequirement entity with foreign key to TrainingProgram
- Enum definitions: TrainingType, TrainingStatus, ProficiencyLevel
- Entity relationships and navigation properties
- Audit field patterns (CreatedDate, CreatedBy, ModifiedDate, ModifiedBy)

**API contracts** (`contracts/` directory):
- `training-records-api.yaml`: OpenAPI spec for TrainingRecordsController
- `skills-api.yaml`: OpenAPI spec for SkillsController
- `mandatory-training-api.yaml`: OpenAPI spec for MandatoryTrainingController
- `events.yaml`: Event schemas for TrainingCompletedEvent, MandatoryTrainingOverdueEvent, CertificationExpiringEvent, EmployeeCreatedEvent (consumed)

**Developer quickstart** (`quickstart.md`):
- Database migration steps
- Testing with Testcontainers
- Running background services locally
- Simulating Employee Service events
- Authorization testing patterns
- Sample API requests

## Implementation Phases (Post-Planning)

### Phase 1: Schema & Entities (Days 1-2)
1. Create EF Core migration for 3 new tables
2. Add domain entities with DataAnnotations validation
3. Update CareerDbContext with new DbSets and OnModelCreating configuration
4. Write entity validation unit tests

### Phase 2: Repositories & Services (Days 3-4)
1. Implement repositories with explicit mapping (no AutoMapper)
2. Create command handlers with business logic
3. Create query handlers including compliance report logic
4. Write repository and handler unit tests

### Phase 3: Controllers (Day 5)
1. Add 3 new controllers with authorization attributes
2. Configure routing following existing patterns
3. Add Scalar/OpenAPI documentation
4. Write controller integration tests with Testcontainers

### Phase 4: Background Services & Events (Day 6)
1. Implement CertificationExpirationReminderBackgroundService
2. Implement OverdueTrainingEscalationBackgroundService
3. Add EmployeeCreatedEventConsumer
4. Configure MassTransit consumers in Program.cs
5. Write background service and event consumer tests

### Phase 5: Data Migration (Day 7)
1. Export training_records, skills, mandatory_training_requirements from Employee Service
2. Import to Career Service PostgreSQL
3. Link training_program_id where course names match
4. Verify data integrity and foreign key relationships

### Phase 6: Testing & Validation (Days 8-10)
1. Run all unit tests (target 80%+ coverage)
2. Run all integration tests with Testcontainers
3. Verify API functionality end-to-end
4. Update OpenAPI documentation
5. Verify zero warnings build
6. Test authorization for all roles (employee, manager, HR admin)

## Estimated Impact

- **New Files**: ~30 C# files
- **New Lines of Code**: ~5,000 LOC
- **Modified Files**: 2 (Program.cs, CareerDbContext.cs)
- **New Database Tables**: 3
- **New Controllers**: 3
- **New Background Services**: 2
- **New Event Consumers**: 1
- **New Tests**: ~15 test classes

## Rollback Strategy

If issues occur during deployment:
1. Keep Employee Service training endpoints active during transition
2. Use feature flag to switch between Employee Service and Career Service endpoints
3. Data can remain in both databases during validation period
4. Revert migration if critical issues found
5. Remove Career Service additions and restore Employee Service as primary source

## Success Validation

Post-deployment verification:
- All 10 success criteria from spec.md validated
- Compliance reports generate successfully for existing data
- Background services run on schedule without errors
- Integration events published and consumed correctly
- Authorization checks enforce correct access patterns
- Zero warnings in build output
- All tests passing with real infrastructure (Testcontainers)
