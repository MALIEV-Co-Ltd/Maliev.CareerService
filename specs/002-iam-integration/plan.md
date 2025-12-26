# Implementation Plan: Permission-Based Authorization Migration

**Branch**: `002-iam-integration` | **Date**: 2025-12-21 | **Spec**: [/specs/002-iam-integration/spec.md](spec.md)
**Input**: Feature specification from `/specs/002-iam-integration/spec.md`

## Summary

Migrate the Career Service from a potentially basic role-based authorization to a granular permission-based system. This involves defining 16 functional permissions and 4 predefined roles, registering them with a central IAM service, and enforcing them across all API controllers.

## Technical Context

**Language/Version**: C# / .NET 10.0  
**Primary Dependencies**: `Microsoft.AspNetCore.Authentication.JwtBearer`, `Maliev.Aspire.ServiceDefaults` (NuGet), `Npgsql.EntityFrameworkCore.PostgreSQL`  
**Storage**: PostgreSQL  
**Testing**: xUnit, Testcontainers  
**Target Platform**: Docker / Linux (ASP.NET Core 10.0)
**Project Type**: Microservice (Web API)  
**Performance Goals**: Permission checks added < 10ms overhead  
**Constraints**: Zero warnings policy, no AutoMapper, no FluentValidation, no FluentAssertions  
**Scale/Scope**: 16 permissions, 4 roles, ~8 controllers to be updated

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] Service Autonomy: CareerService owns its logic and data.
- [x] Explicit Contracts: OpenAPI via Scalar is already used.
- [x] Test-First: Integration tests will be updated before/during implementation.
- [x] Real Infrastructure: Testcontainers for PostgreSQL/Redis are standard in this project.
- [x] Auditability: Structured logging is configured.
- [x] Security: JWT authentication is already in place; this feature enhances authorization.
- [x] Zero Warnings: Build configuration enforces this.
- [x] Aspire Integration: `Maliev.Aspire.ServiceDefaults` will be migrated from a project reference to a **NuGet package** in Phase 2.
- [x] Code Quality: `FluentValidation` will not be used for new logic; existing usage remains but is outside the scope of this migration.

## Project Structure

### Documentation (this feature)

```text
specs/002-iam-integration/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output
```

### Source Code (repository root)

```text
Maliev.CareerService.Api/
├── Authentication/      # New: Permission-based auth logic
│   ├── CareerPermissions.cs
│   └── CareerPredefinedRoles.cs
├── Services/
│   └── External/
│       ├── IIamServiceClient.cs
│       ├── IamServiceClient.cs
│       └── CareerIAMRegistrationService.cs
└── Controllers/         # Updated with [Authorize] attributes
```

**Structure Decision**: Standard flat structure as per Constitution XV.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Project Ref for ServiceDefaults | Pre-existing | Will be addressed as part of this plan |
| FluentValidation | Pre-existing | Will be avoided for new logic; existing use to be reviewed |