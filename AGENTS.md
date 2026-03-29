# Maliev.CareerService Agent Guidelines

This document provides instructions for agentic coding agents working on the Maliev.CareerService repository.

## 1. Build & Test Commands

### Core Commands
- **Build**: `dotnet build`
  - *Note*: The project enforces "TreatWarningsAsErrors". Ensure 0 warnings.
- **Run All Tests**: `dotnet test --verbosity normal`
- **Run Single Test**:
  ```bash
  dotnet test --filter "FullyQualifiedName~Maliev.CareerService.Tests.Integration.JobPostingControllerTests.GetJobPostings_WithoutFilters_ReturnsActivePostings"
  ```
- **Database Update**:
  ```bash
  dotnet ef database update --project Maliev.CareerService.Infrastructure --startup-project Maliev.CareerService.Infrastructure
  ```

### Docker & Infrastructure
- The project uses **Testcontainers** for integration tests (PostgreSQL, Redis, RabbitMQ).
- **Docker Build**:
  ```bash
  docker build -f Maliev.CareerService.Api/Dockerfile .
  ```
  *Note*: Dockerfile is located in the API project folder, but build context must be root.

## 2. Code Style & Standards

### General Conventions
- **Language**: C# 14 (.NET 10.0)
- **Formatting**: Follow standard .NET coding conventions.
  - Use PascalCase for classes, methods, and public properties.
  - Use camelCase for local variables and parameters.
  - Use `_camelCase` for private fields.
- **Namespaces**: `Maliev.CareerService.Api`, `Maliev.CareerService.Data`, `Maliev.CareerService.Tests`.
- **Constructors**: Use primary constructors where applicable (e.g., `public class MyController(IService service) : ControllerBase`).

### Strict Prohibitions (NON-NEGOTIABLE)
- ❌ **NO AutoMapper**: Use explicit manual mapping methods (e.g., `DomainToDtoMapper.cs`).
- ❌ **NO FluentValidation**: Use standard .NET `System.ComponentModel.DataAnnotations` or manual validation logic.
- ❌ **NO FluentAssertions**: Use standard xUnit `Assert` methods (e.g., `Assert.Equal`, `Assert.NotNull`).
- ❌ **NO In-Memory EF Core**: Use real PostgreSQL via Testcontainers for tests.
- ❌ **NO Warnings**: All warnings are treated as errors.

### Project Structure
- **Flat Structure**:
  - `Maliev.CareerService.Api/`: Core API logic, Controllers, Services.
  - `Maliev.CareerService.Data/`: EF Core DbContext, Entities, Migrations.
  - `Maliev.CareerService.Tests/`: Unit, Integration, and Contract tests.

### API Controllers
- Use `[ApiController]` and `[Route("career/v{version:apiVersion}/[resource]")]`.
- Use `[ApiVersion("1.0")]`.
- Document all public endpoints with XML comments (`///`).
- Use `[ProducesResponseType]` for all possible HTTP status codes.
- Use `CancellationToken` in all async methods.

### Error Handling
- Use standard HTTP status codes (`200`, `201`, `400`, `404`, `500`).
- Return `ProblemDetails` or consistent error objects.
- Log warnings/errors using `ILogger`.

## 3. Testing Guidelines

- **Framework**: xUnit.
- **Pattern**: Arrange-Act-Assert.
- **Integration Tests**:
  - Inherit from `IClassFixture<TestWebApplicationFactory>`.
  - Use `TestWebApplicationFactory` to spin up real containers.
  - Clean database between tests or ensure isolation.
- **Naming**: `MethodName_Condition_ExpectedResult` (e.g., `GetJobPostings_WithInvalidLimit_ReturnsBadRequest`).

### Testing Strategy (4-Tier Pyramid Context)

This service's tests cover **Tier 1 (Unit)** and **Tier 2 (Service Integration)** of the Maliev testing pyramid:

| Tier | What to Test | Infrastructure |
|------|-------------|---------------|
| **Unit** | Business logic, domain models, service methods with mocked dependencies | None (mocks only) |
| **Service Integration** | API endpoints, database persistence, permission enforcement, input validation | `BaseIntegrationTestFactory` + Testcontainers (Postgres/Redis/RabbitMQ) |

**Tier 3 (System Integration)** — cross-service workflows and event chains — is tested in `Maliev.Aspire.Tests/`.

#### Key Rules
- Use `BaseIntegrationTestFactory<TProgram, TDbContext>` for integration tests (real Testcontainers, never InMemoryDatabase)
- Every MassTransit consumer MUST have a consumer test using `services.AddMassTransitTestHarness()`
- Test naming: `MethodName_StateUnderTest_ExpectedBehavior`
- Minimum 80% code coverage
- Use `[Fact]` for single cases, `[Theory]` for parameterized tests

> Full ecosystem test strategy: `Maliev.Aspire.Tests/TEST_PLAN.md`

## 4. Constitution (Critical Rules)

Refer to `.specify/memory/constitution.md` for the full list of non-negotiable architectural and development rules.
- **Service Autonomy**: Own database, no direct access to other service DBs.
- **Secrets**: No secrets in code. Use environment variables.
- **Observability**: Metrics and structured logging are mandatory.


## Git & Version Control — Mandatory Rules

### 🚨 CRITICAL: Always Commit Code Changes (Non-Negotiable)
- **You MUST commit your changes to the local repository after completing any meaningful unit of work.**
- **Never accumulate uncommitted changes.** Do not wait until end of session or until something breaks.
- **Commit early and often** — if a change is meaningful (even a small fix or refactor), commit it.
- **You do NOT need to push to remote** — local commits are sufficient to protect against accidental loss.
- **If you are unsure whether to commit, commit anyway.** Extra commits are harmless; lost work is irreversible.
- This rule applies even if you are just "testing" or "exploring" — use git branches to isolate experimental work and commit those changes too.

### 🚨 CRITICAL: Never Use `git checkout` to Restore Broken Files
- **NEVER use `git checkout` to restore or recover files.** This operation discards uncommitted changes permanently and will result in data loss.
- **To undo/recover from broken files: first commit your current changes, then use `git revert` or `git reset --soft` to safely undo.**

## Database & EF Core — Mandatory Rules

### EF Core Design Package
- ❌ `Microsoft.EntityFrameworkCore.Design` MUST NOT be in Api projects
- ✅ It belongs ONLY in the Infrastructure (or Data) project where migrations live
- Migration commands must target Infrastructure as both project and startup-project (since EF Core Design package is in Infrastructure):
  ```
  dotnet ef migrations add <Name> --project Maliev.<Domain>Service.Infrastructure --startup-project Maliev.<Domain>Service.Infrastructure
  ```

### PostgreSQL xmin Concurrency — Mandatory Pattern
Use shadow property ONLY. Never add a Xmin/xmin property to domain entities.
```csharp
entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();
```
- ❌ Never use `UseXminAsConcurrencyToken()` (removed in Npgsql EF v7)
- ❌ Never use entity property `public uint Xmin { get; set; }` or `public uint xmin { get; set; }`
- ❌ Never use `.Ignore(e => e.Xmin)` — remove the entity property instead
