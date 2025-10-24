# Tasks: Career Service Web API

**Input**: Design documents from `/specs/001-career-service-api/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/career-api-v1.yaml

**Tests**: Tests are included as per Constitution Principle III (Test-First Development). Tests are authored BEFORE implementation (TDD approach: Red-Green-Refactor).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions
- 3-project .NET solution structure from plan.md:
  - `Maliev.CareerService.Api/` - Controllers, services, DTOs, validators, middleware
  - `Maliev.CareerService.Data/` - Entities, configurations, DbContext, migrations
  - `Maliev.CareerService.Tests/` - Unit, integration, contract tests

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create solution structure with 3 projects: Maliev.CareerService.Api, Maliev.CareerService.Data, Maliev.CareerService.Tests in repository root
- [X] T002 Configure .gitignore to exclude bin/, obj/, .vs/, TestResults/, appsettings.Development.json
- [X] T003 [P] Create Maliev.CareerService.Api.csproj with .NET 9.0, TreatWarningsAsErrors=true, Nullable=enable
- [X] T004 [P] Create Maliev.CareerService.Data.csproj with .NET 9.0, EntityFrameworkCore 9.0.9, Npgsql.EntityFrameworkCore.PostgreSQL 9.0.2
- [X] T005 [P] Create Maliev.CareerService.Tests.csproj with xUnit, FluentAssertions 8.6.0, Moq 4.20.72, Testcontainers.PostgreSql
- [X] T006 [P] Add NuGet packages to Api project: Serilog.AspNetCore 8.0.2, AutoMapper 12.0.1, FluentValidation 11.5.1, Asp.Versioning.Http 8.1.0, Microsoft.OpenApi 9.0.0, Scalar.AspNetCore, Markdig 0.37.0, HtmlSanitizer 8.1.870, prometheus-net 8.2.1, Polly
- [X] T007 [P] Create docker-compose.test.yml in repository root for PostgreSQL 18 container with ports, environment variables
- [X] T008 [P] Create Dockerfile in Maliev.CareerService.Api/ with multi-stage build, non-root user (appuser UID 1000), ASPNETCORE_URLS http://+:8080
- [X] T009 Create .dockerignore in repository root to exclude bin/, obj/, .vs/, .git/, TestResults/
- [X] T010 Create README.md in repository root with project overview, quickstart instructions reference

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T011 Create CareerDbContext.cs in Maliev.CareerService.Data/ with DbContextOptions, OnModelCreating for snake_case naming convention
- [X] T012 Create CareerDbContextFactory.cs in Maliev.CareerService.Data/ implementing IDesignTimeDbContextFactory for EF Core migrations
- [X] T013 [P] Create base entity classes: BaseEntity with Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted, RowVersion in Maliev.CareerService.Data/Models/Base/
- [X] T014 [P] Create ApplicationStatus.cs in Maliev.CareerService.Data/Models/ with state machine constants (Submitted, UnderReview, Interviewing, Offered, Accepted, Rejected, Withdrawn)
- [X] T015 [P] Create EnrollmentStatus.cs in Maliev.CareerService.Data/Models/ with status constants (Enrolled, InProgress, Completed, Cancelled)
- [X] T016 [P] Create IDPStatus.cs in Maliev.CareerService.Data/Models/ with status constants (Draft, Submitted, Approved, InProgress, Completed)
- [X] T017 [P] Implement IMarkdownService interface in Maliev.CareerService.Api/Services/ with ConvertToHtml method
- [X] T018 [P] Implement MarkdownService in Maliev.CareerService.Api/Services/ using Markdig with HtmlSanitizer (allow h1-h3, p, ul, ol, li, strong, em, a)
- [X] T019 [P] Create ExceptionHandlingMiddleware.cs in Maliev.CareerService.Api/Middleware/ to catch exceptions and return standardized ErrorResponse
- [X] T020 [P] Create RequestLoggingMiddleware.cs in Maliev.CareerService.Api/Middleware/ for structured logging with Serilog
- [X] T021 [P] Create ErrorResponse.cs in Maliev.CareerService.Api/Models/Common/ with ErrorCode, Message, Details properties
- [X] T022 [P] Create PaginatedResponse.cs in Maliev.CareerService.Api/Models/Common/ with Items, TotalCount, Offset, Limit properties
- [X] T023 Configure Serilog in Program.cs to write JSON logs to stdout only (no file logging)
- [X] T024 Configure JWT Bearer authentication in Program.cs with RSA public key validation, issuer, audience from configuration
- [X] T025 Configure rate limiting in Program.cs with FixedWindowLimiter for anonymous (100/min), applicant (200/min), employee (300/min), hrstaff (500/min) policies
- [X] T026 Configure health checks in Program.cs: /career/liveness (simple), /career/readiness (EF Core database check)
- [X] T027 Configure Swagger and Scalar UI in Program.cs at /career/swagger and /career/scalar/v1 routes
- [X] T028 Configure API versioning in Program.cs with Asp.Versioning v1.0
- [X] T029 Create TestDatabaseFixture.cs in Maliev.CareerService.Tests/Fixtures/ using Testcontainers.PostgreSql with InitializeAsync, DisposeAsync, ConnectionString property
- [X] T030 [P] Create TestAuthHandler.cs in Maliev.CareerService.Tests/Mocks/ for mock JWT authentication with Admin claims
- [X] T031 [P] Create MockEmployeeServiceClient.cs in Maliev.CareerService.Tests/Mocks/ implementing IEmployeeServiceClient
- [X] T032 [P] Create MockUploadServiceClient.cs in Maliev.CareerService.Tests/Mocks/ implementing IUploadServiceClient
- [X] T033 [P] Create MockCountryServiceClient.cs in Maliev.CareerService.Tests/Mocks/ implementing ICountryServiceClient
- [X] T034 [P] Create MockEmailServiceClient.cs in Maliev.CareerService.Tests/Mocks/ implementing IEmailServiceClient
- [X] T035 Create TestWebApplicationFactory.cs in Maliev.CareerService.Tests/ extending WebApplicationFactory with TestDatabaseFixture, mock service clients, TestAuthHandler

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Job Application Lifecycle for External Applicants (Priority: P1) 🎯 MVP

**Goal**: Enable external applicants to discover job openings, search and filter positions, apply with credentials and documents, and track application status through the entire hiring process.

**Independent Test**: Create sample job postings, have a test applicant search for jobs, submit an application with resume upload, and view their application status. Delivers immediate value by enabling external recruitment.

### Tests for User Story 1

**NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T036 [P] [US1] Create JobPostingControllerTests.cs in Maliev.CareerService.Tests/Integration/ with tests for GET /career/api/v1/job-postings (list, search, filter)
- [X] T037 [P] [US1] Create JobApplicationSubmissionTests.cs in Maliev.CareerService.Tests/Integration/ with tests for POST /career/api/v1/job-applications (submit application, validate files, duplicate detection)
- [X] T038 [P] [US1] Create ApplicationTrackingTests.cs in Maliev.CareerService.Tests/Integration/ with tests for GET /career/api/v1/job-applications (view own applications by applicant)
- [X] T039 [P] [US1] Create JobPostingContractTests.cs in Maliev.CareerService.Tests/Contract/ verifying OpenAPI schema compliance for job posting endpoints
- [X] T040 [P] [US1] Create JobApplicationContractTests.cs in Maliev.CareerService.Tests/Contract/ verifying OpenAPI schema compliance for application endpoints

### Implementation for User Story 1

**Entities & Data Layer**

- [X] T041 [P] [US1] Create JobPosting.cs entity in Maliev.CareerService.Data/Models/ with properties: Id, PositionTitle, PositionCode, Department, Location, EmploymentType, SalaryMin, SalaryMax, Currency, Description (Markdown), Requirements (Markdown), Responsibilities (Markdown), ApplicationDeadline, PublishedAt, IsActive, audit fields
- [X] T042 [P] [US1] Create JobApplication.cs entity in Maliev.CareerService.Data/Models/ with properties: Id, JobPostingId, ApplicantFirstName, ApplicantLastName, ApplicantEmail, ApplicantPhone, ApplicantCountryCode, ResumeFileId, CoverLetter, AdditionalFileIds (uuid array), Status, AppliedAt, audit fields
- [X] T043 [P] [US1] Create ApplicationStatusChange.cs entity in Maliev.CareerService.Data/Models/ with properties: Id, ApplicationId, FromStatus, ToStatus, ChangedBy, ChangedAt, Reason, IsReversal, ReversedChangeId
- [X] T044 [P] [US1] Create JobPostingConfiguration.cs in Maliev.CareerService.Data/Configurations/ with Fluent API: table name job_postings, indexes (idx_job_postings_active, idx_job_postings_department, idx_job_postings_employment_type), unique constraint on position_code, check constraints (salary_min <= salary_max, application_deadline > now())
- [X] T045 [P] [US1] Create JobApplicationConfiguration.cs in Maliev.CareerService.Data/Configurations/ with Fluent API: table name job_applications, indexes (idx_job_applications_posting, idx_job_applications_email, idx_job_applications_status), composite unique constraint on (job_posting_id, applicant_email), array column for additional_file_ids
- [X] T046 [P] [US1] Create ApplicationStatusChangeConfiguration.cs in Maliev.CareerService.Data/Configurations/ with Fluent API: table name application_status_changes, indexes (idx_status_changes_application, idx_status_changes_changed_by)
- [X] T047 [US1] Add DbSet properties for JobPosting, JobApplication, ApplicationStatusChange to CareerDbContext.cs
- [X] T048 [US1] Create initial EF Core migration: dotnet ef migrations add InitialCreate --project Maliev.CareerService.Data --startup-project Maliev.CareerService.Api

**DTOs & Mapping**

- [X] T049 [P] [US1] Create JobPostingResponse.cs in Maliev.CareerService.Api/Models/JobPostings/ with all fields including DescriptionHtml, RequirementsHtml, ResponsibilitiesHtml (rendered Markdown), RowVersion (Base64)
- [X] T050 [P] [US1] Create CreateJobPostingRequest.cs in Maliev.CareerService.Api/Models/JobPostings/ with validation attributes
- [X] T051 [P] [US1] Create UpdateJobPostingRequest.cs in Maliev.CareerService.Api/Models/JobPostings/ with RowVersion for optimistic concurrency
- [X] T052 [P] [US1] Create JobPostingListResponse.cs in Maliev.CareerService.Api/Models/JobPostings/ inheriting from PaginatedResponse<JobPostingResponse>
- [X] T053 [P] [US1] Create JobApplicationResponse.cs in Maliev.CareerService.Api/Models/Applications/ with fields including ResumeFileUrl, AdditionalFileUrls (from Upload Service), ApplicantCountryName (from Country Service), nested JobPostingResponse
- [X] T054 [P] [US1] Create SubmitJobApplicationRequest.cs in Maliev.CareerService.Api/Models/Applications/ with validation attributes (required fields, email format, file count validation)
- [X] T055 [P] [US1] Create JobApplicationListResponse.cs in Maliev.CareerService.Api/Models/Applications/ inheriting from PaginatedResponse<JobApplicationResponse>
- [X] T056 [US1] Create CareerServiceMappingProfile.cs in Maliev.CareerService.Api/Mapping/ with AutoMapper mappings for JobPosting <-> JobPostingResponse, JobApplication <-> JobApplicationResponse, including custom resolvers for Markdown conversion and external service lookups

**Validators**

- [X] T057 [P] [US1] Create CreateJobPostingRequestValidator.cs in Maliev.CareerService.Api/Validators/ using FluentValidation: validate required fields, position_code format, application_deadline in future, salary_min <= salary_max, Markdown content length limits
- [X] T058 [P] [US1] Create UpdateJobPostingRequestValidator.cs in Maliev.CareerService.Api/Validators/ using FluentValidation: same rules as Create plus RowVersion required
- [X] T059 [P] [US1] Create SubmitJobApplicationRequestValidator.cs in Maliev.CareerService.Api/Validators/ using FluentValidation: validate required fields, email format, phone format, resume_file_id UUID, additional_file_ids array length <= 4, total files <= 5

**Services & Business Logic**

- [X] T060 [P] [US1] Create IJobPostingService.cs interface in Maliev.CareerService.Api/Services/ with methods: GetActivePostingsAsync, GetPostingByIdAsync, SearchPostingsAsync, CreatePostingAsync, UpdatePostingAsync, DeletePostingAsync (soft delete)
- [X] T061 [US1] Implement JobPostingService.cs in Maliev.CareerService.Api/Services/ with business logic: filter active postings, convert Markdown to HTML via IMarkdownService, apply pagination, handle not found scenarios
- [X] T062 [P] [US1] Create IApplicationService.cs interface in Maliev.CareerService.Api/Services/ with methods: SubmitApplicationAsync, GetApplicationByIdAsync, GetApplicantApplicationsAsync, ValidateDuplicateAsync, ValidateDeadlineAsync
- [X] T063 [US1] Implement ApplicationService.cs in Maliev.CareerService.Api/Services/ with business logic: validate deadline not passed, check duplicate email per posting, validate file IDs via Upload Service, create application with status=Submitted, integrate with Country Service for country names, send confirmation email via Email Service
- [X] T064 [P] [US1] Create IEmployeeServiceClient.cs interface in Maliev.CareerService.Api/Services/External/ with GetEmployeeAsync, ValidateEmployeeAsync methods
- [X] T065 [P] [US1] Implement EmployeeServiceClient.cs in Maliev.CareerService.Api/Services/External/ using HttpClient with Polly retry policies (exponential backoff 3 retries)
- [X] T066 [P] [US1] Create IUploadServiceClient.cs interface in Maliev.CareerService.Api/Services/External/ with ValidateFileAsync, GetFileUrlAsync methods
- [X] T067 [P] [US1] Implement UploadServiceClient.cs in Maliev.CareerService.Api/Services/External/ using HttpClient with Polly retry policies
- [X] T068 [P] [US1] Create ICountryServiceClient.cs interface in Maliev.CareerService.Api/Services/External/ with GetCountryNameAsync method
- [X] T069 [P] [US1] Implement CountryServiceClient.cs in Maliev.CareerService.Api/Services/External/ using HttpClient with Polly retry policies
- [X] T070 [P] [US1] Create IEmailServiceClient.cs interface in Maliev.CareerService.Api/Services/External/ with SendApplicationConfirmationAsync, SendStatusChangeNotificationAsync methods
- [X] T071 [P] [US1] Implement EmailServiceClient.cs in Maliev.CareerService.Api/Services/External/ using HttpClient with Polly retry policies

**Controllers**

- [X] T072 [US1] Create JobPostingsController.cs in Maliev.CareerService.Api/Controllers/ with endpoints: GET /career/api/v1/job-postings (list with filters: department, location, employment_type, search keyword, offset, limit), GET /career/api/v1/job-postings/{id} (detail) - both AllowAnonymous, apply rate limiter policy "anonymous"
- [X] T073 [US1] Add POST /career/api/v1/job-postings to JobPostingsController with [Authorize(Roles="HRStaff")], validate request with CreateJobPostingRequestValidator, call JobPostingService.CreatePostingAsync, return 201 Created with JobPostingResponse
- [X] T074 [US1] Add PUT /career/api/v1/job-postings/{id} to JobPostingsController with [Authorize(Roles="HRStaff")], validate UpdateJobPostingRequest, handle optimistic concurrency (catch DbUpdateConcurrencyException, return 409 Conflict)
- [X] T075 [US1] Add DELETE /career/api/v1/job-postings/{id} to JobPostingsController with [Authorize(Roles="HRStaff")], soft delete via JobPostingService.DeletePostingAsync, return 204 No Content
- [X] T076 [US1] Create ApplicationsController.cs in Maliev.CareerService.Api/Controllers/ with endpoint: POST /career/api/v1/job-applications (submit application) - AllowAnonymous, validate SubmitJobApplicationRequest, call ApplicationService.SubmitApplicationAsync, return 201 Created with JobApplicationResponse, handle 409 Conflict for duplicate, handle 400 Bad Request for deadline passed or file validation failures
- [X] T077 [US1] Add GET /career/api/v1/job-applications to ApplicationsController with [Authorize(Roles="Applicant,HRStaff")], filter by authenticated user (applicants see own only, HR sees all), support query params: job_posting_id, status, offset, limit, return JobApplicationListResponse
- [X] T078 [US1] Add GET /career/api/v1/job-applications/{id} to ApplicationsController with [Authorize(Roles="Applicant,HRStaff")], verify access (applicants can only view own applications), return JobApplicationResponse with nested job posting, file URLs from Upload Service, country name from Country Service

**Configuration & Dependency Injection**

- [X] T079 [US1] Register services in Program.cs: AddScoped<IJobPostingService, JobPostingService>, AddScoped<IApplicationService, ApplicationService>, AddHttpClient for external service clients with BaseAddress from configuration
- [X] T080 [US1] Add appsettings.json configuration for ExternalServices section with EmployeeService, UploadService, CountryService, EmailService BaseUrl and Timeout properties
- [X] T081 [US1] Configure AutoMapper in Program.cs: AddAutoMapper(typeof(CareerServiceMappingProfile))
- [X] T082 [US1] Configure FluentValidation in Program.cs: AddValidatorsFromAssemblyContaining<CreateJobPostingRequestValidator>()

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently. External applicants can search jobs, apply with resume, and track applications.

---

## Phase 4: User Story 2 - Employee Learning and Development Portal (Priority: P2)

**Goal**: Enable MALIEV employees to access available training programs, enroll in e-learning courses, track professional development progress, and manage self-development plans to enhance skills and career growth.

**Independent Test**: Create sample training programs and e-learning courses, have a test employee browse available offerings, enroll in courses, complete learning modules, and view development history. Delivers value by enabling continuous employee development.

### Tests for User Story 2

- [x] T083 [P] [US2] Create TrainingProgramControllerTests.cs in Maliev.CareerService.Tests/Integration/ with tests for GET /career/api/v1/training-programs (list, filter by category), GET /career/api/v1/training-programs/{id}
- [x] T084 [P] [US2] Create TrainingEnrollmentTests.cs in Maliev.CareerService.Tests/Integration/ with tests for POST /career/api/v1/training-enrollments (enroll, capacity validation), GET /career/api/v1/training-enrollments (view own enrollments)
- [x] T085 [P] [US2] Create ELearningResourceTests.cs in Maliev.CareerService.Tests/Integration/ with tests for GET /career/api/v1/elearning-resources (list, filter by type), GET /career/api/v1/elearning-resources/{id} (access external LMS URL)
- [x] T086 [P] [US2] Create TrainingProgramContractTests.cs in Maliev.CareerService.Tests/Contract/ verifying OpenAPI schema compliance for training endpoints

### Implementation for User Story 2

**Entities & Data Layer**

- [X] T087 [P] [US2] Create TrainingProgram.cs entity in Maliev.CareerService.Data/Models/ with properties: Id, ProgramCode, ProgramName, Description, Category, DurationHours, Provider, ExternalLmsUrl, IsMandatory, TargetRoles (string array), IsActive, audit fields
- [X] T088 [P] [US2] Create EmployeeTrainingEnrollment.cs entity in Maliev.CareerService.Data/Models/ with properties: Id, TrainingProgramId, EmployeeId, EnrolledAt, EnrollmentType (Voluntary/Mandatory/Assigned), Status (Enrolled/InProgress/Completed/Cancelled), StartedAt, CompletedAt, CompletionNotes, MarkedCompleteBy, audit fields
- [X] T089 [P] [US2] Create ELearningResource.cs entity in Maliev.CareerService.Data/Models/ with properties: Id, ResourceCode, Title, Description, ResourceType (Video/Document/Interactive/Quiz), Category, ExternalLmsUrl, EstimatedMinutes, IsActive, audit fields
- [X] T090 [P] [US2] Create TrainingProgramConfiguration.cs in Maliev.CareerService.Data/Configurations/ with Fluent API: table name training_programs, indexes, unique constraint on program_code, array column for target_roles
- [X] T091 [P] [US2] Create EmployeeTrainingEnrollmentConfiguration.cs in Maliev.CareerService.Data/Configurations/ with Fluent API: table name employee_training_enrollments, indexes, composite unique constraint on (training_program_id, employee_id)
- [X] T092 [P] [US2] Create ELearningResourceConfiguration.cs in Maliev.CareerService.Data/Configurations/ with Fluent API: table name elearning_resources, indexes, unique constraint on resource_code
- [X] T093 [US2] Add DbSet properties for TrainingProgram, EmployeeTrainingEnrollment, ELearningResource to CareerDbContext.cs
- [X] T094 [US2] Create EF Core migration for User Story 2 entities: dotnet ef migrations add AddTrainingAndLearningEntities --project Maliev.CareerService.Data --startup-project Maliev.CareerService.Api

**DTOs & Mapping**

- [X] T095 [P] [US2] Create TrainingProgramResponse.cs in Maliev.CareerService.Api/Models/TrainingPrograms/ with all fields, RowVersion (Base64)
- [X] T096 [P] [US2] Create CreateTrainingProgramRequest.cs in Maliev.CareerService.Api/Models/TrainingPrograms/ with validation attributes
- [X] T097 [P] [US2] Create UpdateTrainingProgramRequest.cs in Maliev.CareerService.Api/Models/TrainingPrograms/ with RowVersion
- [X] T098 [P] [US2] Create TrainingProgramListResponse.cs in Maliev.CareerService.Api/Models/TrainingPrograms/ inheriting from PaginatedResponse
- [X] T099 [P] [US2] Create TrainingEnrollmentResponse.cs in Maliev.CareerService.Api/Models/Enrollments/ with nested TrainingProgramResponse
- [X] T100 [P] [US2] Create EnrollInTrainingRequest.cs in Maliev.CareerService.Api/Models/Enrollments/ with TrainingProgramId
- [X] T101 [P] [US2] Create MarkTrainingCompleteRequest.cs in Maliev.CareerService.Api/Models/Enrollments/ with CompletionNotes, RowVersion
- [X] T102 [P] [US2] Create TrainingEnrollmentListResponse.cs in Maliev.CareerService.Api/Models/Enrollments/ inheriting from PaginatedResponse
- [X] T103 [P] [US2] Create ELearningResourceResponse.cs in Maliev.CareerService.Api/Models/ELearningResources/ with all fields, RowVersion
- [X] T104 [P] [US2] Create ELearningResourceListResponse.cs in Maliev.CareerService.Api/Models/ELearningResources/ inheriting from PaginatedResponse
- [X] T105 [US2] Add AutoMapper mappings for TrainingProgram, EmployeeTrainingEnrollment, ELearningResource to CareerServiceMappingProfile.cs

**Validators**

- [X] T106 [P] [US2] Create CreateTrainingProgramRequestValidator.cs in Maliev.CareerService.Api/Validators/ using FluentValidation: validate program_code format, duration_hours > 0, external_lms_url valid URI if provided
- [X] T107 [P] [US2] Create UpdateTrainingProgramRequestValidator.cs in Maliev.CareerService.Api/Validators/ using FluentValidation: same as Create plus RowVersion required
- [X] T108 [P] [US2] Create EnrollInTrainingRequestValidator.cs in Maliev.CareerService.Api/Validators/ using FluentValidation: validate TrainingProgramId is valid UUID
- [X] T109 [P] [US2] Create MarkTrainingCompleteRequestValidator.cs in Maliev.CareerService.Api/Validators/ using FluentValidation: validate RowVersion required, CompletionNotes optional

**Services & Business Logic**

- [X] T110 [P] [US2] Create ITrainingProgramService.cs interface in Maliev.CareerService.Api/Services/ with methods: GetActiveProgramsAsync, GetProgramByIdAsync, FilterProgramsAsync, CreateProgramAsync, UpdateProgramAsync
- [X] T111 [US2] Implement TrainingProgramService.cs in Maliev.CareerService.Api/Services/ with business logic: filter by category, is_mandatory, validate program_code uniqueness, apply pagination
- [X] T112 [P] [US2] Create IEnrollmentService.cs interface in Maliev.CareerService.Api/Services/ with methods: EnrollEmployeeAsync, GetEmployeeEnrollmentsAsync, MarkCompletedAsync, ValidateCapacityAsync, CheckDuplicateEnrollmentAsync
- [X] T113 [US2] Implement EnrollmentService.cs in Maliev.CareerService.Api/Services/ with business logic: validate employee via IEmployeeServiceClient, check duplicate enrollment (composite unique constraint), validate capacity limits, create enrollment with status=Enrolled, HR staff can mark as completed with completion timestamp
- [X] T114 [P] [US2] Create IELearningResourceService.cs interface in Maliev.CareerService.Api/Services/ with methods: GetActiveResourcesAsync, GetResourceByIdAsync, FilterResourcesAsync
- [X] T115 [US2] Implement ELearningResourceService.cs in Maliev.CareerService.Api/Services/ with business logic: filter by category, resource_type, apply pagination, return external_lms_url for access

**Controllers**

- [X] T116 [US2] Create TrainingProgramsController.cs in Maliev.CareerService.Api/Controllers/ with endpoints: GET /career/api/v1/training-programs (list with filters: category, is_mandatory, offset, limit) - [Authorize(Roles="Employee,HRStaff")], GET /career/api/v1/training-programs/{id} - [Authorize]
- [X] T117 [US2] Add POST /career/api/v1/training-programs to TrainingProgramsController with [Authorize(Roles="HRStaff")], validate CreateTrainingProgramRequest, call TrainingProgramService.CreateProgramAsync, return 201 Created
- [X] T118 [US2] Add PUT /career/api/v1/training-programs/{id} to TrainingProgramsController with [Authorize(Roles="HRStaff")], validate UpdateTrainingProgramRequest, handle optimistic concurrency
- [X] T119 [US2] Create EnrollmentsController.cs in Maliev.CareerService.Api/Controllers/ with endpoint: GET /career/api/v1/training-enrollments (list own enrollments with optional status filter) - [Authorize(Roles="Employee")]
- [X] T120 [US2] Add POST /career/api/v1/training-enrollments to EnrollmentsController with [Authorize(Roles="Employee")], validate EnrollInTrainingRequest, get authenticated employee ID from JWT claims, call EnrollmentService.EnrollEmployeeAsync, return 201 Created, handle 409 Conflict for duplicate enrollment or capacity exceeded
- [X] T121 [US2] Add PATCH /career/api/v1/training-enrollments/{id}/complete to EnrollmentsController with [Authorize(Roles="HRStaff")], validate MarkTrainingCompleteRequest, call EnrollmentService.MarkCompletedAsync with HR user ID from JWT claims, return 200 OK
- [X] T122 [US2] Create ELearningResourcesController.cs in Maliev.CareerService.Api/Controllers/ with endpoints: GET /career/api/v1/elearning-resources (list with filters: category, resource_type, offset, limit) - [Authorize(Roles="Employee,HRStaff")], GET /career/api/v1/elearning-resources/{id} - [Authorize]

**Configuration & Dependency Injection**

- [X] T123 [US2] Register services in Program.cs: AddScoped<ITrainingProgramService, TrainingProgramService>, AddScoped<IEnrollmentService, EnrollmentService>, AddScoped<IELearningResourceService, ELearningResourceService>

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently. Employees can browse training programs, enroll in courses, and HR can mark completions.

---

## Phase 5: User Story 3 - HR Staff Career Management Operations (Priority: P2)

**Goal**: Enable HR staff to manage all career-related aspects including posting job openings, reviewing applications, managing candidate pipelines, administering training programs, tracking employee development, and generating reports on recruitment and learning activities.

**Independent Test**: Have an HR staff member create job postings with Markdown formatting, review submitted applications, schedule interviews, manage training program offerings, review employee development progress, and generate recruitment analytics reports. Delivers value by enabling HR to manage the entire talent lifecycle.

**NOTE**: This story extends User Story 1 and User Story 2 with HR-specific functionality, so many entities and services are already implemented. This phase focuses on HR-specific endpoints and features.

### Tests for User Story 3

- [x] T124 [P] [US3] Create ApplicationStatusManagementTests.cs in Maliev.CareerService.Tests/Integration/ with tests for PATCH /career/api/v1/job-applications/{id}/status (update status, status reversal with audit trail, email notifications)
- [x] T125 [P] [US3] Create ApplicationStatusHistoryTests.cs in Maliev.CareerService.Tests/Integration/ with tests for GET /career/api/v1/job-applications/{id}/status-history (view full audit trail including reversals)
- [x] T126 [P] [US3] Create RecruitmentMetricsTests.cs in Maliev.CareerService.Tests/Integration/ with tests for recruitment report generation (applications per position, time-to-hire, pipeline status)
- [x] T127 [P] [US3] Create LearningMetricsTests.cs in Maliev.CareerService.Tests/Integration/ with tests for learning report generation (enrollments, completion rates, popular programs)
- [x] T128 [P] [US3] Create MarkdownRenderingTests.cs in Maliev.CareerService.Tests/Unit/ with tests for MarkdownService: proper HTML rendering, XSS prevention, script injection blocking

### Implementation for User Story 3

**DTOs & Additional Models**

- [x] T129 [P] [US3] Create UpdateApplicationStatusRequest.cs in Maliev.CareerService.Api/Models/Applications/ with properties: NewStatus, Reason, IsReversal, RowVersion
- [x] T130 [P] [US3] Create StatusChangeRecord.cs in Maliev.CareerService.Api/Models/Applications/ with properties: Id, FromStatus, ToStatus, ChangedBy, ChangedByName, ChangedAt, Reason, IsReversal
- [x] T131 [P] [US3] Create StatusHistoryResponse.cs in Maliev.CareerService.Api/Models/Applications/ with properties: ApplicationId, Changes (List<StatusChangeRecord>)
- [x] T132 [P] [US3] Create RecruitmentMetricsResponse.cs in Maliev.CareerService.Api/Models/Reports/ with properties: TotalApplications, ApplicationsPerPosting, ConversionRates, AverageTimeToHire, PositionsFilled, PositionsOpen, ApplicationVolumeTrends
- [x] T133 [P] [US3] Create LearningMetricsResponse.cs in Maliev.CareerService.Api/Models/Reports/ with properties: EnrollmentRates, CompletionRates, TimeToComplete, PopularPrograms, CertificationRates, IDPAdoption
- [x] T134 [P] [US3] Create HROperationalMetricsResponse.cs in Maliev.CareerService.Api/Models/Reports/ with properties: ActiveJobPostings, ApplicantToInterviewRatio, OfferAcceptanceRates, TrainingCapacityUtilization, AverageReviewTime

**Validators**

- [x] T135 [P] [US3] Create UpdateApplicationStatusRequestValidator.cs in Maliev.CareerService.Api/Validators/ using FluentValidation: validate NewStatus is valid state machine value, RowVersion required, Reason optional string max length 1000

**Services & Business Logic**

- [x] T136 [US3] Add methods to IApplicationService: UpdateApplicationStatusAsync, GetStatusHistoryAsync, ValidateStatusTransitionAsync
- [x] T137 [US3] Implement UpdateApplicationStatusAsync in ApplicationService with business logic: validate status transition, create ApplicationStatusChange record, update application status, handle reversals (set IsReversal=true, link to original change), send email notification via IEmailServiceClient, handle optimistic concurrency
- [x] T138 [US3] Implement GetStatusHistoryAsync in ApplicationService: retrieve all ApplicationStatusChange records for application ordered by ChangedAt DESC, include user names from Employee Service
- [x] T139 [P] [US3] Create IReportService.cs interface in Maliev.CareerService.Api/Services/ with methods: GenerateRecruitmentMetricsAsync, GenerateLearningMetricsAsync, GenerateHROperationalMetricsAsync
- [x] T140 [US3] Implement ReportService.cs in Maliev.CareerService.Api/Services/ with business logic: aggregate data from job_postings, job_applications, training_programs, employee_training_enrollments tables, calculate metrics (count, average, conversion rates, trends), apply date range filters, cache results for 5 minutes using IMemoryCache

**Controllers**

- [x] T141 [US3] Add PATCH /career/api/v1/job-applications/{id}/status to ApplicationsController with [Authorize(Roles="HRStaff")], validate UpdateApplicationStatusRequest, get authenticated HR user ID from JWT claims, call ApplicationService.UpdateApplicationStatusAsync, return 200 OK with updated JobApplicationResponse, handle 409 Conflict for concurrency issues
- [x] T142 [US3] Add GET /career/api/v1/job-applications/{id}/status-history to ApplicationsController with [Authorize(Roles="HRStaff")], call ApplicationService.GetStatusHistoryAsync, return StatusHistoryResponse with full audit trail
- [x] T143 [US3] Create ReportsController.cs in Maliev.CareerService.Api/Controllers/ with endpoint: GET /career/api/v1/reports/recruitment-metrics (with query params: start_date, end_date) - [Authorize(Roles="HRStaff")]
- [x] T144 [US3] Add GET /career/api/v1/reports/learning-metrics to ReportsController with [Authorize(Roles="HRStaff")], call ReportService.GenerateLearningMetricsAsync, return LearningMetricsResponse
- [x] T145 [US3] Add GET /career/api/v1/reports/hr-operational-metrics to ReportsController with [Authorize(Roles="HRStaff")], call ReportService.GenerateHROperationalMetricsAsync, return HROperationalMetricsResponse

**Configuration & Dependency Injection**

- [x] T146 [US3] Register services in Program.cs: AddScoped<IReportService, ReportService>, AddMemoryCache() for report caching

**Checkpoint**: At this point, User Stories 1, 2, AND 3 should all work independently. HR staff can manage full talent lifecycle including recruitment, training administration, and analytics.

---

## Phase 6: User Story 4 - Employee Self-Development Planning (Priority: P3)

**Goal**: Enable employees to create and manage personal development plans, set career goals, identify skill gaps, and track progress toward career advancement within MALIEV, aligning growth with organizational needs.

**Independent Test**: Have an employee create a development plan with career goals, identify required skills, link relevant training courses to their plan, and track completion progress. Delivers value by enabling proactive career planning.

### Tests for User Story 4

- [x] T147 [P] [US4] Create DevelopmentPlanControllerTests.cs in Maliev.CareerService.Tests/Integration/ with tests for GET /career/api/v1/idps (list own plans), POST /career/api/v1/idps (create IDP), PUT /career/api/v1/idps/{id} (update draft IDP), PATCH /career/api/v1/idps/{id}/submit (submit for approval), PATCH /career/api/v1/idps/{id}/approve (HR approval)
- [x] T148 [P] [US4] Create DevelopmentGoalTests.cs in Maliev.CareerService.Tests/Integration/ with tests for POST /career/api/v1/idps/{idpId}/goals (add goal), PUT /career/api/v1/goals/{id} (update goal), PATCH /career/api/v1/goals/{id}/status (update progress)
- [x] T149 [P] [US4] Create IDPContractTests.cs in Maliev.CareerService.Tests/Contract/ verifying OpenAPI schema compliance for IDP endpoints

### Implementation for User Story 4

**Entities & Data Layer**

- [x] T150 [P] [US4] Create IndividualDevelopmentPlan.cs entity in Maliev.CareerService.Data/Models/ with properties: Id, EmployeeId, PlanYear, Status (Draft/Submitted/Approved/InProgress/Completed), SubmittedAt, ApprovedAt, ApprovedBy, audit fields
- [x] T151 [P] [US4] Create EmployeeDevelopmentGoal.cs entity in Maliev.CareerService.Data/Models/ with properties: Id, IdpId, GoalTitle, GoalDescription, Category (Technical/Leadership/SoftSkills/Certification), TargetDate, Status (NotStarted/InProgress/Completed/Deferred), CompletionDate, ActionItems, ProgressNotes, audit fields
- [x] T152 [P] [US4] Create IndividualDevelopmentPlanConfiguration.cs in Maliev.CareerService.Data/Configurations/ with Fluent API: table name individual_development_plans, indexes, composite unique constraint on (employee_id, plan_year)
- [x] T153 [P] [US4] Create EmployeeDevelopmentGoalConfiguration.cs in Maliev.CareerService.Data/Configurations/ with Fluent API: table name employee_development_goals, indexes on (idp_id, status, target_date)
- [x] T154 [US4] Add DbSet properties for IndividualDevelopmentPlan, EmployeeDevelopmentGoal to CareerDbContext.cs
- [x] T155 [US4] Create EF Core migration for User Story 4 entities: dotnet ef migrations add AddDevelopmentPlanningEntities --project Maliev.CareerService.Data --startup-project Maliev.CareerService.Api

**DTOs & Mapping**

- [x] T156 [P] [US4] Create IDPResponse.cs in Maliev.CareerService.Api/Models/DevelopmentPlans/ with properties: Id, EmployeeId, PlanYear, Status, SubmittedAt, ApprovedAt, ApprovedBy, Goals (List<DevelopmentGoalResponse>), audit fields, RowVersion
- [x] T157 [P] [US4] Create CreateIDPRequest.cs in Maliev.CareerService.Api/Models/DevelopmentPlans/ with property: PlanYear
- [x] T158 [P] [US4] Create UpdateIDPRequest.cs in Maliev.CareerService.Api/Models/DevelopmentPlans/ with property: RowVersion
- [x] T159 [P] [US4] Create ApproveIDPRequest.cs in Maliev.CareerService.Api/Models/DevelopmentPlans/ with properties: ApprovalNotes, RowVersion
- [x] T160 [P] [US4] Create IDPListResponse.cs in Maliev.CareerService.Api/Models/DevelopmentPlans/ with properties: Items (List<IDPResponse>), TotalCount
- [x] T161 [P] [US4] Create DevelopmentGoalResponse.cs in Maliev.CareerService.Api/Models/DevelopmentGoals/ with all fields, RowVersion
- [x] T162 [P] [US4] Create CreateDevelopmentGoalRequest.cs in Maliev.CareerService.Api/Models/DevelopmentGoals/ with properties: GoalTitle, GoalDescription, Category, TargetDate, ActionItems
- [x] T163 [P] [US4] Create UpdateDevelopmentGoalRequest.cs in Maliev.CareerService.Api/Models/DevelopmentGoals/ with properties: GoalTitle, GoalDescription, Category, TargetDate, ActionItems, ProgressNotes, RowVersion
- [x] T164 [P] [US4] Create UpdateGoalStatusRequest.cs in Maliev.CareerService.Api/Models/DevelopmentGoals/ with properties: Status, CompletionDate, ProgressNotes, RowVersion
- [x] T165 [US4] Add AutoMapper mappings for IndividualDevelopmentPlan, EmployeeDevelopmentGoal to CareerServiceMappingProfile.cs

**Validators**

- [x] T166 [P] [US4] Create CreateIDPRequestValidator.cs in Maliev.CareerService.Api/Validators/ using FluentValidation: validate PlanYear >= current year - 1 (allow previous year updates)
- [x] T167 [P] [US4] Create UpdateIDPRequestValidator.cs in Maliev.CareerService.Api/Validators/ using FluentValidation: validate RowVersion required
- [x] T168 [P] [US4] Create ApproveIDPRequestValidator.cs in Maliev.CareerService.Api/Validators/ using FluentValidation: validate RowVersion required, ApprovalNotes optional
- [x] T169 [P] [US4] Create CreateDevelopmentGoalRequestValidator.cs in Maliev.CareerService.Api/Validators/ using FluentValidation: validate GoalTitle max length 200, TargetDate in future, Category valid enum
- [x] T170 [P] [US4] Create UpdateDevelopmentGoalRequestValidator.cs in Maliev.CareerService.Api/Validators/ using FluentValidation: same as Create plus RowVersion required
- [x] T171 [P] [US4] Create UpdateGoalStatusRequestValidator.cs in Maliev.CareerService.Api/Validators/ using FluentValidation: validate Status valid enum, if Status=Completed then CompletionDate required, RowVersion required

**Services & Business Logic**

- [x] T172 [P] [US4] Create IDevelopmentPlanService.cs interface in Maliev.CareerService.Api/Services/ with methods: GetEmployeeIDPsAsync, GetIDPByIdAsync, CreateIDPAsync, UpdateIDPAsync, SubmitIDPAsync, ApproveIDPAsync, CheckDuplicateYearAsync
- [x] T173 [US4] Implement DevelopmentPlanService.cs in Maliev.CareerService.Api/Services/ with business logic: validate employee via IEmployeeServiceClient, check duplicate plan year (composite unique constraint), only allow updates if Status=Draft, SubmitIDPAsync changes status to Submitted and sets SubmittedAt, ApproveIDPAsync (HR only) changes status to Approved and sets ApprovedAt/ApprovedBy, handle optimistic concurrency
- [x] T174 [P] [US4] Create IDevelopmentGoalService.cs interface in Maliev.CareerService.Api/Services/ with methods: CreateGoalAsync, UpdateGoalAsync, UpdateGoalStatusAsync, GetGoalsByIDPAsync
- [x] T175 [US4] Implement DevelopmentGoalService.cs in Maliev.CareerService.Api/Services/ with business logic: validate IDP exists and belongs to employee, validate target_date in future for new goals, only allow goal edits if IDP Status != Approved (or if just updating status), UpdateGoalStatusAsync sets CompletionDate if Status=Completed, handle optimistic concurrency

**Controllers**

- [x] T176 [US4] Create DevelopmentPlansController.cs in Maliev.CareerService.Api/Controllers/ with endpoint: GET /career/api/v1/idps (list own IDPs with optional filters: plan_year, status) - [Authorize(Roles="Employee,HRStaff")], employee sees own only unless HR
- [x] T177 [US4] Add POST /career/api/v1/idps to DevelopmentPlansController with [Authorize(Roles="Employee")], validate CreateIDPRequest, get authenticated employee ID from JWT claims, call DevelopmentPlanService.CreateIDPAsync, return 201 Created, handle 409 Conflict for duplicate plan_year
- [x] T178 [US4] Add GET /career/api/v1/idps/{id} to DevelopmentPlansController with [Authorize(Roles="Employee,HRStaff")], verify access (employees can only view own IDPs unless HR), call DevelopmentPlanService.GetIDPByIdAsync, return IDPResponse with nested Goals list
- [x] T179 [US4] Add PUT /career/api/v1/idps/{id} to DevelopmentPlansController with [Authorize(Roles="Employee")], verify IDP belongs to authenticated employee, verify Status=Draft, validate UpdateIDPRequest, call DevelopmentPlanService.UpdateIDPAsync, handle optimistic concurrency
- [x] T180 [US4] Add PATCH /career/api/v1/idps/{id}/submit to DevelopmentPlansController with [Authorize(Roles="Employee")], verify IDP belongs to authenticated employee, verify Status=Draft, call DevelopmentPlanService.SubmitIDPAsync, return 200 OK
- [x] T181 [US4] Add PATCH /career/api/v1/idps/{id}/approve to DevelopmentPlansController with [Authorize(Roles="HRStaff")], validate ApproveIDPRequest, verify Status=Submitted, get authenticated HR user ID from JWT claims, call DevelopmentPlanService.ApproveIDPAsync, return 200 OK
- [x] T182 [US4] Create DevelopmentGoalsController.cs in Maliev.CareerService.Api/Controllers/ with endpoint: POST /career/api/v1/idps/{idpId}/goals (add goal to IDP) - [Authorize(Roles="Employee")], verify IDP belongs to authenticated employee, validate CreateDevelopmentGoalRequest, call DevelopmentGoalService.CreateGoalAsync, return 201 Created
- [x] T183 [US4] Add PUT /career/api/v1/goals/{id} to DevelopmentGoalsController with [Authorize(Roles="Employee")], verify goal's IDP belongs to authenticated employee, validate UpdateDevelopmentGoalRequest, call DevelopmentGoalService.UpdateGoalAsync, handle optimistic concurrency
- [x] T184 [US4] Add PATCH /career/api/v1/goals/{id}/status to DevelopmentGoalsController with [Authorize(Roles="Employee")], verify goal's IDP belongs to authenticated employee, validate UpdateGoalStatusRequest, call DevelopmentGoalService.UpdateGoalStatusAsync, return 200 OK

**Configuration & Dependency Injection**

- [x] T185 [US4] Register services in Program.cs: AddScoped<IDevelopmentPlanService, DevelopmentPlanService>, AddScoped<IDevelopmentGoalService, DevelopmentGoalService>

**Checkpoint**: All user stories (1-4) should now be independently functional. Employees can manage comprehensive development plans aligned with career goals.

---

## Phase 7: Prometheus Metrics & Monitoring (Cross-Cutting)

**Purpose**: Expose business and operational metrics for monitoring and analytics (FR-044 to FR-050)

### Tests for Metrics

- [x] T186 [P] Create MetricsEndpointTests.cs in Maliev.CareerService.Tests/Integration/ with tests for GET /career/metrics (verify Prometheus format, verify all metric families present, verify no PII exposure)
- [x] T187 [P] Create MetricsContractTests.cs in Maliev.CareerService.Tests/Contract/ verifying /career/metrics endpoint returns text/plain content type

### Implementation for Metrics

- [x] T188 [P] Create MetricsService.cs in Maliev.CareerService.Api/Services/ with Prometheus metric collectors: Counter for career_job_applications_total (labels: status), career_training_enrollments_total (labels: status), Gauge for career_active_job_postings, career_concurrent_users, Histogram for career_api_response_seconds (labels: endpoint, method)
- [x] T189 Add prometheus-net middleware in Program.cs: app.UseHttpMetrics(), map metrics endpoint at /career/metrics with AllowAnonymous
- [x] T190 Create MetricsController.cs in Maliev.CareerService.Api/Controllers/ with GET /career/metrics endpoint returning metrics in Prometheus text format (note: prometheus-net handles this automatically, controller just for documentation)
- [x] T191 Instrument ApplicationService with metrics: increment career_job_applications_total counter on SubmitApplicationAsync, UpdateApplicationStatusAsync
- [x] T192 Instrument EnrollmentService with metrics: increment career_training_enrollments_total counter on EnrollEmployeeAsync, MarkCompletedAsync
- [x] T193 Instrument JobPostingService with metrics: update career_active_job_postings gauge on CreatePostingAsync, DeletePostingAsync
- [x] T194 Add middleware to track concurrent users: increment/decrement career_concurrent_users gauge on request start/end
- [x] T195 Verify metrics collection doesn't impact API response times (should add < 5ms overhead)

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [x] T196 [P] Create comprehensive API documentation in README.md: architecture overview, API endpoints summary, authentication guide, rate limiting policies, error response formats, external service dependencies, API usage examples with curl, troubleshooting guide, deployment instructions
- [x] T202 Code cleanup: remove unused imports, apply consistent formatting (dotnet format), ensure all public APIs have XML documentation comments
- [x] T203 Security hardening: run dotnet list package --vulnerable, update any vulnerable dependencies, run OWASP dependency check
- [x] T204 Performance optimization: add database query indexes based on application query patterns (composite index on job_postings), add response caching for read-heavy endpoints (job postings, training programs, e-learning resources)
- [x] T207 Run full quickstart validation: test solution builds, fix test project issues, verify API and Data projects compile with 0 errors/warnings, validate unit tests pass (integration tests require Docker)

**Removed Tasks** (per user requirements):
- ~~T197~~ - DEVELOPMENT.md (not needed - all documentation in README.md)
- ~~T198~~ - CI workflows (already exist in .github/workflows)
- ~~T199-T201~~ - CD workflows (already exist in .github/workflows with GitOps PR creation)
- ~~T205-T206~~ - K8s manifests (already verified in maliev-gitops repository)
- ~~T208~~ - DEPLOYMENT.md (not needed - deployment documented in README.md)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational completion - MVP delivery
- **User Story 2 (Phase 4)**: Depends on Foundational completion - Can run in parallel with US1 if staffed
- **User Story 3 (Phase 5)**: Depends on US1 and US2 entities/services - Extends both stories with HR features
- **User Story 4 (Phase 6)**: Depends on Foundational completion - Can run in parallel with US1/US2 if staffed
- **Metrics (Phase 7)**: Depends on US1, US2, US3, US4 services for instrumentation
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories - **MVP SCOPE**
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Independent of US1
- **User Story 3 (P2)**: Extends US1 (job application management) and US2 (training administration) with HR features - Depends on US1 and US2 entities
- **User Story 4 (P3)**: Can start after Foundational (Phase 2) - Independent of other stories

### Within Each User Story

- Tests MUST be written and FAIL before implementation (TDD)
- Entities and configurations before services
- Services before controllers
- External service clients in parallel
- Controllers depend on corresponding services
- Story complete before moving to next priority

### Parallel Opportunities

**Setup Phase (Phase 1)**:
- T003, T004, T005, T006, T007, T008, T009 can all run in parallel (different files)

**Foundational Phase (Phase 2)**:
- T013, T014, T015, T016, T017, T018, T019, T020, T021, T022 can run in parallel
- T030, T031, T032, T033, T034 (test mocks) can run in parallel

**User Story 1 Tests**:
- T036, T037, T038, T039, T040 can run in parallel

**User Story 1 Entities**:
- T041, T042, T043, T044, T045, T046 can run in parallel

**User Story 1 DTOs**:
- T049, T050, T051, T052, T053, T054, T055 can run in parallel

**User Story 1 Validators**:
- T057, T058, T059 can run in parallel

**User Story 1 Interfaces**:
- T060, T062, T064, T066, T068, T070 can run in parallel

**User Story 1 External Clients**:
- T065, T067, T069, T071 can run in parallel after interfaces

**User Story 2 Tests**:
- T083, T084, T085, T086 can run in parallel

**User Story 2 Entities**:
- T087, T088, T089, T090, T091, T092 can run in parallel

**User Story 2 DTOs**:
- T095, T096, T097, T098, T099, T100, T101, T102, T103, T104 can run in parallel

**User Story 2 Validators**:
- T106, T107, T108, T109 can run in parallel

**User Story 2 Interfaces**:
- T110, T112, T114 can run in parallel

**User Story 3 Tests**:
- T124, T125, T126, T127, T128 can run in parallel

**User Story 3 DTOs**:
- T129, T130, T131, T132, T133, T134 can run in parallel

**User Story 4 Tests**:
- T147, T148, T149 can run in parallel

**User Story 4 Entities**:
- T150, T151, T152, T153 can run in parallel

**User Story 4 DTOs**:
- T156, T157, T158, T159, T160, T161, T162, T163, T164 can run in parallel

**User Story 4 Validators**:
- T166, T167, T168, T169, T170, T171 can run in parallel

**User Story 4 Interfaces**:
- T172, T174 can run in parallel

**Metrics Tests**:
- T186, T187 can run in parallel

**Polish Phase**:
- T196, T197, T198, T199, T200, T201, T205, T206 can all run in parallel (different files)

### Cross-Story Parallelization

Once Foundational (Phase 2) is complete, the following user stories can proceed in parallel with separate team members:
- **Developer A**: User Story 1 (T036-T082) - Job Application Lifecycle
- **Developer B**: User Story 2 (T083-T123) - Employee Learning Portal
- **Developer C**: User Story 4 (T147-T185) - Self-Development Planning

User Story 3 should start after User Story 1 and User Story 2 entities are available (no need to wait for full completion, just entities and base services).

---

## Parallel Example: User Story 1

### Tests Launch (5 parallel tasks):
```bash
Task: "Create JobPostingControllerTests.cs in Maliev.CareerService.Tests/Integration/"
Task: "Create JobApplicationSubmissionTests.cs in Maliev.CareerService.Tests/Integration/"
Task: "Create ApplicationTrackingTests.cs in Maliev.CareerService.Tests/Integration/"
Task: "Create JobPostingContractTests.cs in Maliev.CareerService.Tests/Contract/"
Task: "Create JobApplicationContractTests.cs in Maliev.CareerService.Tests/Contract/"
```

### Entities Launch (6 parallel tasks):
```bash
Task: "Create JobPosting.cs entity in Maliev.CareerService.Data/Models/"
Task: "Create JobApplication.cs entity in Maliev.CareerService.Data/Models/"
Task: "Create ApplicationStatusChange.cs entity in Maliev.CareerService.Data/Models/"
Task: "Create JobPostingConfiguration.cs in Maliev.CareerService.Data/Configurations/"
Task: "Create JobApplicationConfiguration.cs in Maliev.CareerService.Data/Configurations/"
Task: "Create ApplicationStatusChangeConfiguration.cs in Maliev.CareerService.Data/Configurations/"
```

### DTOs Launch (7 parallel tasks):
```bash
Task: "Create JobPostingResponse.cs in Maliev.CareerService.Api/Models/JobPostings/"
Task: "Create CreateJobPostingRequest.cs in Maliev.CareerService.Api/Models/JobPostings/"
Task: "Create UpdateJobPostingRequest.cs in Maliev.CareerService.Api/Models/JobPostings/"
Task: "Create JobPostingListResponse.cs in Maliev.CareerService.Api/Models/JobPostings/"
Task: "Create JobApplicationResponse.cs in Maliev.CareerService.Api/Models/Applications/"
Task: "Create SubmitJobApplicationRequest.cs in Maliev.CareerService.Api/Models/Applications/"
Task: "Create JobApplicationListResponse.cs in Maliev.CareerService.Api/Models/Applications/"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only) - Recommended for Initial Delivery

1. Complete Phase 1: Setup (T001-T010)
2. Complete Phase 2: Foundational (T011-T035) - **CRITICAL GATE**
3. Complete Phase 3: User Story 1 (T036-T082)
4. **STOP and VALIDATE**: Test User Story 1 independently:
   - Start PostgreSQL container
   - Apply migrations
   - Run API
   - Test job posting creation, search, filtering
   - Test application submission with file uploads
   - Test application tracking
   - Verify all tests pass
5. Deploy to development environment and demo

**MVP Scope**: External recruitment portal is fully functional. Applicants can search for jobs and apply with resumes. This delivers immediate business value and validates core architecture.

### Incremental Delivery (Recommended Production Strategy)

1. **Sprint 1**: Setup + Foundational → Foundation ready (T001-T035)
2. **Sprint 2**: User Story 1 → Test independently → Deploy/Demo (T036-T082) - **MVP!**
3. **Sprint 3**: User Story 2 → Test independently → Deploy/Demo (T083-T123) - Employee learning portal
4. **Sprint 4**: User Story 3 → Test independently → Deploy/Demo (T124-T146) - HR operations
5. **Sprint 5**: User Story 4 → Test independently → Deploy/Demo (T147-T185) - Development planning
6. **Sprint 6**: Metrics + Polish → Production hardening (T186-T208)

Each sprint delivers incremental value without breaking previous functionality.

### Parallel Team Strategy (3 Developers)

With multiple developers after Foundational phase completes:

**Phase 2 Complete → Branch Out**:
- **Developer A**: User Story 1 (T036-T082) - Job applications
- **Developer B**: User Story 2 (T083-T123) - Learning portal
- **Developer C**: User Story 4 (T147-T185) - Development plans

**After US1 & US2 Entities Ready**:
- **Developer A or B**: User Story 3 (T124-T146) - HR extensions
- **Developer C**: Continue User Story 4

**Final Integration**:
- **All Developers**: Metrics (T186-T195), Polish (T196-T208)

Stories integrate independently with minimal merge conflicts due to distinct file paths.

---

## Notes

- **[P] tasks**: Different files, no dependencies, can run in parallel
- **[Story] label**: Maps task to specific user story for traceability
- **Tests First**: TDD approach enforced by Constitution Principle III - all tests written before implementation
- **PostgreSQL-Only**: No in-memory database testing per Constitution Principle IV - all tests use real PostgreSQL via Testcontainers
- **Zero Warnings**: TreatWarningsAsErrors enabled in all .csproj files per Constitution Principle VIII
- **Optimistic Concurrency**: All update operations must handle DbUpdateConcurrencyException and return 409 Conflict
- **External Services**: All external service calls use Polly retry policies with exponential backoff (3 retries)
- **Markdown Security**: All Markdown content sanitized via HtmlSanitizer before rendering to prevent XSS
- **Rate Limiting**: Applied per-user based on role (anonymous/applicant/employee/hrstaff)
- **Metrics Collection**: Instrumented throughout services, no PII exposure
- **Commit Strategy**: Commit after each task or logical group for easy rollback
- **Story Checkpoints**: Validate each story independently before proceeding to next
- **Avoid**: Vague tasks, same file conflicts, cross-story dependencies that break independence

---

## Task Summary

**Total Tasks**: 208

**Phase Breakdown**:
- Phase 1 (Setup): 10 tasks
- Phase 2 (Foundational): 25 tasks
- Phase 3 (User Story 1): 47 tasks (T036-T082)
- Phase 4 (User Story 2): 41 tasks (T083-T123)
- Phase 5 (User Story 3): 23 tasks (T124-T146)
- Phase 6 (User Story 4): 39 tasks (T147-T185)
- Phase 7 (Metrics): 10 tasks (T186-T195)
- Phase 8 (Polish): 13 tasks (T196-T208)

**User Story Task Counts**:
- US1 (Job Application Lifecycle): 47 tasks
- US2 (Employee Learning Portal): 41 tasks
- US3 (HR Operations): 23 tasks
- US4 (Self-Development Planning): 39 tasks

**Parallel Opportunities Identified**: 89 tasks marked with [P] across all phases

**MVP Scope**: Phase 1 + Phase 2 + Phase 3 = 82 tasks (Setup + Foundational + User Story 1)

**Format Validation**: ✅ ALL tasks follow the required checklist format with checkbox, ID, optional [P] and [Story] labels, and file paths
