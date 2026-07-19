# Maliev.CareerService — Agent Coding Guide

> This is an independent git repo within the MALIEV workspace (`B:\maliev`). All commands run from `B:\maliev\Maliev.CareerService`.

---

## Build, Test & Lint Commands

### .NET (C# — .NET 10)

```powershell
# Build (treats warnings as errors — all must be fixed)
dotnet build Maliev.CareerService.slnx

# Run all tests
dotnet test Maliev.CareerService.slnx --verbosity normal

# Run a single test method
dotnet test --filter "FullyQualifiedName~JobPostingControllerTests.GetJobPostings_WithoutFilters_ReturnsActivePostings"

# Run all tests in a class
dotnet test --filter "FullyQualifiedName~JobPostingControllerTests"

# Run with code coverage
dotnet test Maliev.CareerService.slnx --collect:"XPlat Code Coverage"

# Format check
dotnet format Maliev.CareerService.slnx

# EF Core migrations (Infrastructure project only)
dotnet ef migrations add <Name> --project Maliev.CareerService.Infrastructure --startup-project Maliev.CareerService.Infrastructure

# Database update
dotnet ef database update --project Maliev.CareerService.Infrastructure --startup-project Maliev.CareerService.Infrastructure
```

### Docker & Infrastructure

- The project uses **Testcontainers** for integration tests (PostgreSQL, Redis, RabbitMQ).
- **Docker Build**:
  ```bash
  docker build -f Maliev.CareerService.Api/Dockerfile .
  ```
  *Note*: Dockerfile is located in the API project folder, but build context must be root.

---

## Code Style & Conventions

### Project Structure
```
Maliev.CareerService/
├── Maliev.CareerService.Api/              # Controllers, Consumers, Middleware
├── Maliev.CareerService.Application/      # Use cases, DTOs, Interfaces, Handlers
├── Maliev.CareerService.Domain/           # Entities, value objects, domain interfaces
├── Maliev.CareerService.Infrastructure/   # EF Core DbContext, repositories, HTTP clients
├── Maliev.CareerService.Tests/            # Unit + Integration tests (xUnit)
├── Directory.Build.props                  # Central package versioning
└── Maliev.CareerService.slnx             # Solution file (.slnx preferred over .sln)
```

### C# Naming & Formatting
- **Namespaces**: File-scoped (`namespace Maliev.CareerService.Domain.Entities;`)
- **Classes/Methods/Properties**: `PascalCase`
- **Private fields**: `_camelCase` (underscore prefix)
- **Parameters/locals**: `camelCase`
- **Async methods**: Suffix with `Async` (e.g., `GetJobPostingsAsync`)
- **Interfaces**: Prefix with `I` (e.g., `IJobPostingService`)
- **Permissions**: GCP-style `{domain}.{plural-resource}.{action}` as `public const string` in a `Permissions` static class
  - Valid: `career.job-postings.create`, `career.applications.submit`
  - Invalid: `career.job-posting.create` (singular), `career.create` (missing resource)
- **XML docs**: Required on ALL public methods and properties
- **Nullable**: Enabled (`<Nullable>enable</Nullable>`). Use `?` explicitly
- **Imports**: System first, then third-party, then local. Alphabetize within groups. Remove unused `using`
- **Braces**: Allman style (new line) for methods and control structures. Expression-bodied for properties/accessors
- **Indentation**: 4 spaces, LF line endings, UTF-8, trim trailing whitespace

### C# Patterns
- **DI**: Constructor injection with `private readonly` fields
- **Controllers**: `[ApiController]`, `[ApiVersion("1")]`, `[Route("career/v{version:apiVersion}/[resource]")]`
- **Logging**: `ILogger<T>` with structured placeholders (never interpolate): `_logger.LogInformation("Processing {JobPostingId}", jobPostingId)`
- **Error handling**: Global exception middleware. Return `ProblemDetails` / `ErrorResponse` DTOs. Never expose stack traces
- **Manual mapping**: Static extension methods (`ToDto()`, `ToEntity()`). AutoMapper is banned
- **Validation**: `System.ComponentModel.DataAnnotations` on DTOs. FluentValidation is banned

---

## Banned Libraries (Build Will Fail)

| Banned | Use Instead |
|--------|-------------|
| AutoMapper | Manual mapping extensions |
| FluentValidation | DataAnnotations or manual validation |
| FluentAssertions | Standard xUnit `Assert.*` |
| Swashbuckle/Swagger | Scalar (at `/career/scalar`) |
| InMemoryDatabase (EF Core) | Testcontainers with real PostgreSQL |

---

## Testing Rules

- **Framework**: xUnit with standard `Assert` (`Assert.Equal`, `Assert.NotNull`, etc.)
- **Naming**: `MethodName_StateUnderTest_ExpectedBehavior` or `HTTP_METHOD_Path_Scenario_ExpectedStatus`
- **Coverage**: Minimum 80% per service
- **Integration tests**: `BaseIntegrationTestFactory<TProgram, TDbContext>` with Testcontainers (PostgreSQL, Redis, RabbitMQ). Never InMemoryDatabase
- **System tests** (Tier 3): `AspireTestFixture` with `[Collection("AspireDomainTests")]` — shared AppHost, never one per class
- **Eventual consistency**: Use `TestHelpers.WaitForAsync`. Never `Task.Delay`
- **MassTransit consumers**: Must have consumer tests using `AddMassTransitTestHarness()`

### Testing Strategy (4-Tier Pyramid Context)

This service's tests cover **Tier 1 (Unit)** and **Tier 2 (Service Integration)** of the MALIEV testing pyramid:

| Tier | What to Test | Infrastructure |
|------|-------------|---------------|
| **Unit** | Business logic, domain models, service methods with mocked dependencies | None (mocks only) |
| **Service Integration** | API endpoints, database persistence, permission enforcement, input validation | `BaseIntegrationTestFactory` + Testcontainers (Postgres/Redis/RabbitMQ) |

**Tier 3 (System Integration)** — cross-service workflows and event chains — is tested in `Maliev.Aspire.Tests/`.

> Full ecosystem test strategy: `Maliev.Aspire.Tests/TEST_PLAN.md`

---

## Mandatory Rules

- **`TreatWarningsAsErrors = true`**: Zero warnings allowed. No suppression
- **`[RequirePermission("career.resources.action")]`**: On all endpoints, not plain `[Authorize]`
- **API versioning**: All routes versioned (`v1/`)
- **Service prefix**: Routes prefixed with `/career`
- **Scalar docs**: Configured at `/career/scalar`
- **Secrets**: Never hardcoded. Use GCP Secret Manager or environment variables
- **Async/await**: All the way down. Pass `CancellationToken`
- **EF Core Design package**: Only in Infrastructure project, never in Api
- **PostgreSQL xmin**: Shadow property only — `entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion()`. Never add entity property
- **Temporary files**: Generate in `/temp` folder, clean up afterwards

### Constitution (Critical Rules)

Refer to `.specify/memory/constitution.md` for the full list of non-negotiable architectural and development rules.
- **Service Autonomy**: Own database, no direct access to other service DBs.
- **Secrets**: No secrets in code. Use environment variables.
- **Observability**: Metrics and structured logging are mandatory.

---

## Database & EF Core — Mandatory Rules

### EF Core Design Package
- `Microsoft.EntityFrameworkCore.Design` MUST NOT be in Api projects
- It belongs ONLY in the Infrastructure project where migrations live
- Migration commands must target Infrastructure as both project and startup-project:
  ```
  dotnet ef migrations add <Name> --project Maliev.CareerService.Infrastructure --startup-project Maliev.CareerService.Infrastructure
  ```

### PostgreSQL xmin Concurrency — Mandatory Pattern
Use shadow property ONLY. Never add a Xmin/xmin property to domain entities.
```csharp
entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();
```
- Never use `UseXminAsConcurrencyToken()` (removed in Npgsql EF v7)
- Never use entity property `public uint Xmin { get; set; }` or `public uint xmin { get; set; }`
- Never use `.Ignore(e => e.Xmin)` — remove the entity property instead

---

## Git Rules

- Each `Maliev.*` folder is an independent git repo. `cd` into it before git commands
- **Commit early and often** after every meaningful unit of work. Do not accumulate changes
- **Never use `git checkout` to restore files** — commit first, then `git revert` or `git reset --soft`
- Feature branches merged to `develop` via PR. Do not push without being asked
