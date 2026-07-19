# Implementation Plan: Career Service Web API

**Branch**: `001-career-service-api` | **Date**: 2025-10-21 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-career-service-api/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

The Career Service provides a comprehensive API for managing job applications, employee learning and development, and HR career operations at MALIEV Co., Ltd. The service enables external applicants to search and apply for positions, employees to access training programs and manage development plans, and HR staff to manage the entire talent lifecycle including recruitment, training administration, and analytics.

**Primary Requirements**:
- External job application portal with search, filter, and application tracking with status history
- Employee learning portal with training enrollment, e-learning resources, and development planning
- Individual Development Plan (IDP) workflow with goals tracking and HR approval process
- HR management console for recruitment and learning administration
- Integration with external services (Employee, Upload, Auth, Country, External LMS)
- Prometheus-compatible metrics endpoint for business and operational analytics
- Email notifications for application status changes

**Technical Approach**:
- .NET 9 WebAPI microservice following Clean Architecture
- PostgreSQL 18 database with Entity Framework Core 9.0.9
- External LMS integration for training content (URL references only)
- Manual training completion marking by HR staff
- Role-based access control (Applicants, Employees, HR Staff)
- Per-user rate limiting (100-500 req/min based on role)
- Optimistic concurrency with audit trails for all mutations

## Technical Context

**Language/Version**: .NET 10.0 (C# 13)
**Primary Dependencies**:
- Entity Framework Core 9.0.10 with Npgsql 9.0.4 (PostgreSQL provider)
- Serilog 8.0.2 (structured logging to stdout JSON only)
- AutoMapper 12.0.1 (DTO mapping)
- FluentValidation 11.5.1 (request validation)
- Polly (HTTP retry policies with exponential backoff)
- Asp.Versioning.Http 8.1.0 (API versioning)
- AspNetCore.HealthChecks.UI.Client 8.0.1 (health checks)
- Scalar/OpenAPI (Microsoft.OpenApi 9.0.0) for API documentation
- JWT Bearer authentication with RSA public key validation

**Storage**: PostgreSQL 18 database (`career_app_db`)
- Job postings, applications (with status change history), training programs, e-learning resources
- Enrollments, individual development plans (with workflow states), development goals
- Audit logs for all mutations (create, update, status changes, workflow transitions)
- Optimistic concurrency using RowVersion byte array
- Snake_case naming convention for tables and columns

**Testing**: xUnit with FluentAssertions 8.6.0, Moq 4.20.72
- PostgreSQL test database (Docker container - NO in-memory database)
- TestWebApplicationFactory for integration tests
- TestDatabaseFixture with automatic migration and cleanup
- Mock authentication (TestAuthHandler with Admin claims)
- Mocked external service clients (Employee, Upload, Auth, Country, External LMS)
- Contract tests for all API endpoints
- Minimum 80% coverage for critical business logic

**Target Platform**: Linux containers (Docker) on Google Kubernetes Engine (GKE)
- ASPNETCORE_URLS: http://+:8080
- Non-root user (appuser UID 1000)
- PostgreSQL client installed for health checks
- Health check endpoint: /career/liveness

**Project Type**: Microservice API (3-project solution)
- Maliev.CareerService.Api (WebAPI controllers, services, middleware, validators)
- Maliev.CareerService.Data (DbContext, entities, configurations, migrations)
- Maliev.CareerService.Tests (unit, integration, contract tests)

**Performance Goals**:
- 99.5% uptime during business hours
- Search/filter results within 2 seconds for up to 10,000 job postings
- Support 500 concurrent users without performance degradation
- Application submission within 10 minutes end-to-end
- Metrics data available within 5 minutes of events

**Constraints**:
- All secrets via Google Secret Manager (no hardcoded credentials)
- Zero build warnings (TreatWarningsAsErrors enabled)
- Manual EF Core migrations (not auto-applied on startup)
- External LMS hosts training content (service stores URLs only)
- Manual training completion marking by HR (no automated LMS sync)
- Email notifications only (no SMS or push notifications)
- 5 files max per application (10MB per file, 25MB total)
- Per-user rate limiting (100 req/min anonymous, 200 applicants, 300 employees, 500 HR)

**Scale/Scope**:
- 10,000+ job postings in database
- 500 concurrent users (mix of applicants, employees, HR staff)
- 86 functional requirements across 10 categories
- 11 key entities with complex relationships (including IDP workflow and status history)
- 4 external service integrations + External LMS
- 12 success criteria with measurable outcomes
- 3 deployment environments (dev, staging, production)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### I. Service Autonomy ✅ PASS
- Career Service has own PostgreSQL database (`career_app_db`)
- Self-contained domain logic for recruitment and learning
- Interacts with others only via APIs (Employee, Upload, Auth, Country, External LMS)
- No direct database access to other services

### II. Explicit Contracts ✅ PASS
- All APIs documented via Scalar/OpenAPI at `/career/scalar/v1`
- API versioning 1.0 using Asp.Versioning
- Backward-compatible migrations enforced
- Contract tests verify all endpoints

### III. Test-First Development ✅ PASS (Planned)
- Tests will be authored immediately after specification approval
- TDD approach: Red-Green-Refactor
- Unit tests for validators and business services
- Integration tests for end-to-end workflows
- Contract tests for all API endpoints
- Minimum 80% coverage for business-critical logic
- Test code reviewed equally with production code

### IV. PostgreSQL-Only Testing ✅ PASS
- **ALL tests use real PostgreSQL 18 database**
- Docker container for local development (docker-compose.test.yml)
- GitHub Actions PostgreSQL service container for CI
- TestDatabaseFixture manages migrations and cleanup
- NO EF Core InMemoryDatabase provider
- Test database schema mirrors production exactly
- Connection string: `ConnectionStrings__CareerDbContext` environment variable

### V. Auditability & Observability ✅ PASS
- Structured JSON logging via Serilog to stdout only
- Audit logs for all mutations (job postings, applications, enrollments, status changes)
- Traceable user/action IDs in all logs
- Health checks: `/career/liveness` (simple), `/career/readiness` (database check)
- Prometheus-compatible metrics at `/metrics` endpoint

### VI. Security & Compliance ✅ PASS
- JWT Bearer authentication with RSA public key validation
- Role-based authorization (Applicants, Employees, HR Staff)
- Sensitive data encrypted at rest and in transit
- Per-user rate limiting to prevent abuse
- GDPR compliance: applicants can request data deletion
- 2-year data retention policy for applications
- Email address validation for applicants
- File upload validation (type, size, security scan via Upload Service)

### VII. Secrets Management ✅ PASS
- **NO secrets in source code**
- All secrets from Google Secret Manager mounted at `/mnt/secrets`
- Connection strings, JWT keys, external service URLs via environment variables
- Public repository sanitized (mock URLs in CI: http://mock-service-name)
- Pre-commit scans enforced

### VIII. Zero Warnings Policy ✅ PASS (Planned)
- `TreatWarningsAsErrors` enabled in all .csproj files
- CI/CD fails on any warnings
- Nullable reference types properly configured
- All analyzer warnings addressed before merge

### IX. Clean Project Artifacts ✅ PASS (Planned)
- `.gitignore` excludes bin/, obj/, .vs/, TestResults/
- `.dockerignore` in repository root
- Unused boilerplate files removed
- Only project-specific files remain

### X. Simplicity & Maintainability ✅ PASS
- YAGNI applied: no speculative features
- Clean Architecture: Controllers → Services → Data
- Stateless microservice design
- Readable code with clear naming conventions
- Shared code through standard NuGet packages (versioned)

### XI. Business Metrics & Analytics ✅ PASS
- **Prometheus-compatible `/metrics` endpoint** (FR-044 to FR-050)
- Recruitment metrics: applications received, conversion rates, time-to-hire, positions filled
- Learning metrics: enrollment rates, completion rates, popular programs, certifications
- HR operational metrics: active postings, interview ratios, offer acceptance, capacity utilization
- System performance metrics: API response times, error rates, availability, concurrent users
- Metrics tagged with: service_name, version, environment
- NO PII exposure in metrics
- Contract tests validate metrics endpoint presence and format

**Constitutional Compliance**: ✅ ALL GATES PASSED

No violations requiring complexity tracking. Service follows all constitutional principles.

## Project Structure

### Documentation (this feature)

```
specs/001-career-service-api/
├── spec.md              # Feature specification (completed)
├── plan.md              # This file (/speckit.plan output)
├── research.md          # Phase 0 output (technology decisions)
├── data-model.md        # Phase 1 output (entities, relationships)
├── quickstart.md        # Phase 1 output (local development guide)
├── contracts/           # Phase 1 output (OpenAPI schemas)
│   ├── job-postings.yaml
│   ├── applications.yaml
│   ├── training-programs.yaml
│   ├── elearning-resources.yaml
│   ├── enrollments.yaml
│   ├── development-plans.yaml
│   ├── development-goals.yaml
│   └── metrics.yaml
└── tasks.md             # Phase 2 output (/speckit.tasks - NOT created by /speckit.plan)
```

### Source Code (repository root)

```
Maliev.CareerService/
├── Maliev.CareerService.sln
├── .gitignore
├── .dockerignore
├── README.md
├── docker-compose.test.yml       # PostgreSQL for local testing
│
├── Maliev.CareerService.Api/
│   ├── Maliev.CareerService.Api.csproj
│   ├── Program.cs                # Middleware pipeline, DI configuration
│   ├── Dockerfile                # Multi-stage Docker build
│   ├── Properties/
│   │   └── launchSettings.json   # Auto-launch Scalar UI
│   ├── appsettings.json          # Base configuration
│   ├── appsettings.Development.json  # Local development overrides
│   ├── Controllers/
│   │   ├── JobPostingsController.cs
│   │   ├── ApplicationsController.cs
│   │   ├── TrainingProgramsController.cs
│   │   ├── ELearningResourcesController.cs  # Self-paced learning content
│   │   ├── EnrollmentsController.cs
│   │   ├── DevelopmentPlansController.cs
│   │   ├── DevelopmentGoalsController.cs    # Goals within IDPs
│   │   └── MetricsController.cs  # Business/operational metrics
│   ├── Services/
│   │   ├── IJobPostingService.cs
│   │   ├── JobPostingService.cs
│   │   ├── IApplicationService.cs
│   │   ├── ApplicationService.cs
│   │   ├── ITrainingProgramService.cs
│   │   ├── TrainingProgramService.cs
│   │   ├── IELearningResourceService.cs
│   │   ├── ELearningResourceService.cs
│   │   ├── IEnrollmentService.cs
│   │   ├── EnrollmentService.cs
│   │   ├── IDevelopmentPlanService.cs
│   │   ├── DevelopmentPlanService.cs
│   │   ├── IDevelopmentGoalService.cs
│   │   ├── DevelopmentGoalService.cs
│   │   └── External/
│   │       ├── IEmployeeServiceClient.cs
│   │       ├── EmployeeServiceClient.cs
│   │       ├── IUploadServiceClient.cs
│   │       ├── UploadServiceClient.cs
│   │       ├── ICountryServiceClient.cs
│   │       ├── CountryServiceClient.cs
│   │       └── ExternalServiceOptions.cs
│   ├── Models/
│   │   ├── JobPostings/
│   │   │   ├── CreateJobPostingRequest.cs
│   │   │   ├── UpdateJobPostingRequest.cs
│   │   │   └── JobPostingResponse.cs
│   │   ├── Applications/
│   │   │   ├── CreateApplicationRequest.cs
│   │   │   ├── UpdateApplicationRequest.cs
│   │   │   ├── ApplicationResponse.cs
│   │   │   └── ApplicationStatusChangeResponse.cs  # Status history
│   │   ├── TrainingPrograms/
│   │   │   ├── CreateTrainingProgramRequest.cs
│   │   │   ├── UpdateTrainingProgramRequest.cs
│   │   │   └── TrainingProgramResponse.cs
│   │   ├── ELearningResources/
│   │   │   ├── CreateELearningResourceRequest.cs
│   │   │   ├── UpdateELearningResourceRequest.cs
│   │   │   └── ELearningResourceResponse.cs
│   │   ├── Enrollments/
│   │   │   ├── CreateEnrollmentRequest.cs
│   │   │   └── EnrollmentResponse.cs
│   │   ├── DevelopmentPlans/
│   │   │   ├── CreateIDPRequest.cs
│   │   │   ├── UpdateIDPRequest.cs
│   │   │   ├── SubmitIDPRequest.cs
│   │   │   ├── ApproveIDPRequest.cs
│   │   │   └── IDPResponse.cs
│   │   ├── DevelopmentGoals/
│   │   │   ├── CreateDevelopmentGoalRequest.cs
│   │   │   ├── UpdateDevelopmentGoalRequest.cs
│   │   │   └── DevelopmentGoalResponse.cs
│   │   └── Common/
│   │       ├── PaginatedResponse.cs
│   │       └── ErrorResponse.cs
│   ├── Validators/
│   │   ├── CreateJobPostingRequestValidator.cs
│   │   ├── CreateApplicationRequestValidator.cs
│   │   ├── CreateTrainingProgramRequestValidator.cs
│   │   ├── CreateELearningResourceRequestValidator.cs
│   │   ├── CreateIDPRequestValidator.cs
│   │   └── CreateDevelopmentGoalRequestValidator.cs
│   ├── Middleware/
│   │   ├── ExceptionHandlingMiddleware.cs
│   │   └── RequestLoggingMiddleware.cs
│   └── Mapping/
│       └── CareerServiceMappingProfile.cs  # AutoMapper profile
│
├── Maliev.CareerService.Data/
│   ├── Maliev.CareerService.Data.csproj
│   ├── CareerDbContext.cs
│   ├── CareerDbContextFactory.cs  # IDesignTimeDbContextFactory for migrations
│   ├── Models/
│   │   ├── JobPosting.cs
│   │   ├── Application.cs
│   │   ├── Applicant.cs
│   │   ├── ApplicationStatusChange.cs  # Audit trail for status history
│   │   ├── TrainingProgram.cs
│   │   ├── ELearningResource.cs
│   │   ├── Enrollment.cs
│   │   ├── IndividualDevelopmentPlan.cs  # IDP with workflow states
│   │   ├── EmployeeDevelopmentGoal.cs    # Goals within IDPs
│   │   ├── Employee.cs
│   │   ├── ApplicationStatus.cs  # State machine constants
│   │   └── IDPStatus.cs  # IDP workflow state constants
│   ├── Configurations/
│   │   ├── JobPostingConfiguration.cs
│   │   ├── ApplicationConfiguration.cs
│   │   ├── ApplicantConfiguration.cs
│   │   ├── ApplicationStatusChangeConfiguration.cs
│   │   ├── TrainingProgramConfiguration.cs
│   │   ├── ELearningResourceConfiguration.cs
│   │   ├── EnrollmentConfiguration.cs
│   │   ├── IndividualDevelopmentPlanConfiguration.cs
│   │   ├── EmployeeDevelopmentGoalConfiguration.cs
│   │   └── EmployeeConfiguration.cs
│   └── Migrations/
│       └── (generated by dotnet ef migrations add)
│
└── Maliev.CareerService.Tests/
    ├── Maliev.CareerService.Tests.csproj
    ├── Fixtures/
    │   ├── TestDatabaseFixture.cs      # PostgreSQL setup and cleanup
    │   └── TestWebApplicationFactory.cs  # Integration test factory
    ├── Unit/
    │   ├── Services/
    │   │   ├── JobPostingServiceTests.cs
    │   │   ├── ApplicationServiceTests.cs
    │   │   ├── TrainingProgramServiceTests.cs
    │   │   ├── ELearningResourceServiceTests.cs
    │   │   ├── EnrollmentServiceTests.cs
    │   │   ├── DevelopmentPlanServiceTests.cs
    │   │   └── DevelopmentGoalServiceTests.cs
    │   └── Validators/
    │       ├── CreateJobPostingRequestValidatorTests.cs
    │       ├── CreateApplicationRequestValidatorTests.cs
    │       ├── CreateELearningResourceRequestValidatorTests.cs
    │       ├── CreateIDPRequestValidatorTests.cs
    │       └── CreateDevelopmentGoalRequestValidatorTests.cs
    ├── Integration/
    │   ├── JobPostingsEndpointTests.cs
    │   ├── ApplicationsEndpointTests.cs
    │   │   └── ApplicationStatusHistoryTests.cs  # Status history endpoint
    │   ├── TrainingProgramsEndpointTests.cs
    │   ├── ELearningResourcesEndpointTests.cs
    │   ├── EnrollmentsEndpointTests.cs
    │   ├── DevelopmentPlansEndpointTests.cs
    │   │   ├── IDPWorkflowTests.cs  # Submit and approve endpoints
    │   │   └── DevelopmentGoalsEndpointTests.cs
    │   └── MetricsEndpointTests.cs
    └── Contract/
        └── ApiContractTests.cs  # Verify all endpoints match OpenAPI spec

.github/
└── workflows/
    ├── ci-develop.yml       # CI/CD for develop branch
    ├── ci-staging.yml       # CI/CD for staging branch
    └── ci-main.yml          # CI/CD for main branch
```

**Structure Decision**: Three-project microservice solution following MALIEV standard patterns:
- **Api project**: Controllers, services, DTOs, validators, middleware, external clients
- **Data project**: DbContext, entities, EF Core configurations, migrations
- **Tests project**: Unit tests (validators, services), integration tests (endpoints), contract tests (OpenAPI compliance)

This structure enables:
- Clear separation of concerns (API logic vs data access)
- Independent testing of each layer
- Easy migration management with design-time factory
- Standard MALIEV microservice patterns for maintainability

## Complexity Tracking

*No constitutional violations requiring justification.*

All requirements align with MALIEV constitution principles. Service follows standard three-project microservice pattern with no additional complexity.

---

## Phase 0: Outline & Research

**Status**: Pending execution
**Output**: `research.md`

**Research Tasks**:

1. **Markdown Rendering and Sanitization**
   - Research: Best libraries for Markdown-to-HTML conversion in .NET 9
   - Research: XSS prevention strategies for user-generated Markdown
   - Decision needed: Markdig vs CommonMark.NET vs Microsoft.AspNetCore.Mvc.Razor.Markdown
   - Output: Chosen library, sanitization approach, allowed Markdown syntax

2. **Rate Limiting Configuration**
   - Research: ASP.NET Core 9.0 built-in rate limiting patterns
   - Research: Per-user vs per-IP rate limiting strategies
   - Research: Rate limit bypass for health checks and metrics endpoints
   - Decision needed: Token bucket vs fixed window vs sliding window algorithms
   - Output: Rate limiting configuration per user role

3. **File Upload Security**
   - Research: File type validation strategies (MIME type vs extension vs magic bytes)
   - Research: Integration with Upload Service for virus scanning
   - Research: Streaming vs buffering for large file uploads
   - Decision needed: Client-side vs server-side file validation
   - Output: File upload validation pipeline

4. **External LMS Integration**
   - Research: Common LMS API patterns (Moodle, Canvas, Blackboard)
   - Research: URL reference storage strategies
   - Research: Training content deep linking best practices
   - Decision needed: URL validation and reachability checks
   - Output: LMS integration approach, URL schema

5. **Email Notification Service**
   - Research: SendGrid vs AWS SES vs Azure Communication Services
   - Research: Email template management strategies
   - Research: Notification delivery failure handling
   - Decision needed: Email service provider, template storage
   - Output: Email service integration approach

6. **Metrics Collection**
   - Research: Prometheus client library for .NET (prometheus-net)
   - Research: Business metrics design patterns
   - Research: Metrics aggregation strategies
   - Decision needed: Push vs pull metrics, aggregation intervals
   - Output: Metrics endpoint implementation approach

7. **State Machine for Application Status**
   - Research: State machine libraries (Stateless vs custom)
   - Research: Terminal state handling
   - Research: Audit trail for state transitions
   - Decision needed: Library vs hand-rolled state machine
   - Output: Application status transition rules

8. **Optimistic Concurrency with PostgreSQL**
   - Research: RowVersion implementation in Npgsql
   - Research: Concurrency conflict resolution strategies
   - Research: Version field serialization (Base64 for API)
   - Decision needed: Automatic retry vs client-side handling
   - Output: Concurrency control implementation pattern

9. **Pagination and Filtering**
   - Research: Cursor-based vs offset-based pagination
   - Research: Dynamic LINQ for flexible filtering
   - Research: Performance optimization for large datasets
   - Decision needed: Pagination strategy, max page size
   - Output: Pagination and filtering patterns

10. **Testing with PostgreSQL**
    - Research: Testcontainers.PostgreSql vs Docker Compose
    - Research: Database cleanup strategies (DELETE vs TRUNCATE)
    - Research: Test isolation with transactions
    - Decision needed: Container per test class vs shared container
    - Output: Test database fixture implementation

**Deliverable**: `research.md` with all technology decisions, rationales, and alternatives considered

---

## Phase 1: Design & Contracts

**Status**: Pending (blocked by Phase 0)
**Prerequisites**: `research.md` complete

**Design Artifacts**:

1. **data-model.md**
   - Entity definitions with all attributes and types
   - Relationships (one-to-many, many-to-many)
   - Indexes for performance optimization
   - Validation rules from functional requirements
   - State machines (ApplicationStatus, EnrollmentStatus)
   - Audit fields (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
   - Optimistic concurrency (Version byte array)

2. **contracts/** (OpenAPI YAML schemas)
   - `job-postings.yaml`: GET /career/api/v1/job-postings (list, search, filter), GET /{id}, POST, PUT, DELETE
   - `applications.yaml`: POST /career/api/v1/applications, GET /my-applications, PUT /{id}/withdraw, GET /{id}/status-history
   - `training-programs.yaml`: GET /career/api/v1/training-programs (list, filter), GET /{id}, POST, PUT, DELETE (HR only)
   - `elearning-resources.yaml`: GET /career/api/v1/elearning-resources (list, filter by type), GET /{id}, POST, PUT, DELETE (HR only)
   - `enrollments.yaml`: POST /career/api/v1/enrollments, GET /my-enrollments, PUT /{id}/complete (HR only)
   - `development-plans.yaml`: GET /career/api/v1/development-plans (my plans), POST, PUT, DELETE, POST /{id}/submit, POST /{id}/approve
   - `development-goals.yaml`: GET /career/api/v1/development-plans/{idpId}/goals, POST, PUT, DELETE (within IDP)
   - `metrics.yaml`: GET /metrics (Prometheus format)
   - All endpoints include request/response schemas, error responses, authentication requirements

3. **quickstart.md**
   - Prerequisites (Docker, .NET 9 SDK, PostgreSQL client)
   - Clone and build instructions
   - PostgreSQL setup with docker-compose.test.yml
   - Environment variable configuration
   - Database migration steps
   - Running the service locally
   - Accessing Scalar UI at `/career/scalar/v1`
   - Running tests
   - Common troubleshooting scenarios

**Agent Context Update**:
- Run `.specify/scripts/powershell/update-agent-context.ps1 -AgentType claude`
- Add .NET 9, Entity Framework Core 9.0.9, PostgreSQL 18 to technology list
- Add Markdown sanitization, rate limiting, external LMS integration patterns
- Preserve manual additions between markers

**Deliverable**: Complete design documentation ready for `/speckit.tasks` command

---

## Next Steps

1. Execute Phase 0 research to resolve all technology decisions
2. Generate Phase 1 design artifacts (data model, API contracts, quickstart guide)
3. Update agent context with new technologies
4. Re-validate Constitution Check with completed design
5. Proceed to `/speckit.tasks` to generate implementation task list

**Command to continue**: `/speckit.tasks` (after plan completion)
