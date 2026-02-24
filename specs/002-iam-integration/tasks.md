# Tasks: Permission-Based Authorization Migration

**Feature**: Permission-Based Authorization Migration
**Branch**: `002-iam-integration`
**Status**: Ready for Implementation

## Implementation Strategy

This feature will be implemented incrementally, starting with core infrastructure and then migrating each functional area (Training, Development, Self-Service) one by one. Each phase results in an independently testable and deployable unit.

## Dependencies

- Phase 2 (Foundational) MUST be completed before any User Story phases.
- User Stories can be implemented in any order once Foundation is ready, but priority follows the spec (US1 -> US2 -> US3).

## Phase 1: Setup

Goal: Align project with Constitution and prepare for implementation.

- [x] T001 Update `Maliev.CareerService.Api/Maliev.CareerService.Api.csproj` to replace the `Maliev.Aspire.ServiceDefaults` project reference with the NuGet package from GitHub Packages.
- [x] T002 Verify `nuget.config` correctly handles GitHub Packages credentials using environment variables.

## Phase 2: Foundational

Goal: Implement the core permission-based authorization infrastructure.

- [x] T003 [P] Create `Maliev.CareerService.Api/Authentication/CareerPermissions.cs` defining nested constants for all 16 permissions.
- [x] T004 [P] Create `Maliev.CareerService.Api/Authentication/CareerPredefinedRoles.cs` defining role-to-permission mappings.
- [x] T005 Create `Maliev.CareerService.Api/Authentication/PermissionRequirement.cs` representing a permission requirement for policies.
- [x] T006 Create `Maliev.CareerService.Api/Authentication/PermissionAuthorizationHandler.cs` to validate the `permissions` claim in the JWT.
- [x] T007 Create `Maliev.CareerService.Api/Authentication/PermissionPolicyProvider.cs` to dynamically generate policies based on permission strings.
- [x] T008 [P] Create `Maliev.CareerService.Api/Services/External/IIamServiceClient.cs` and `IamServiceClient.cs` for communicating with the IAM service.
- [x] T009 Create `Maliev.CareerService.Api/Services/External/CareerIAMRegistrationService.cs` as an `IHostedService` to register permissions and roles on startup.
- [x] T010 Integrate authorization services, policy provider, and IAM registration into `Maliev.CareerService.Api/Program.cs`.

## Phase 3: User Story 1 - Secure Training Management [US1]

Goal: Secure Training Program operations with granular permissions.

- [x] T011 [US1] Update `Maliev.CareerService.Tests/Integration/TrainingProgramControllerTests.cs` to verify that only users with correct permissions can manage training programs (Failing/Red Phase).
- [x] T012 [P] [US1] Apply `[Authorize(Policy = CareerPermissions.Trainings.X)]` attributes to all actions in `Maliev.CareerService.Api/Controllers/TrainingProgramsController.cs`.

## Phase 4: User Story 2 - Manager-led Development [US2]

Goal: Secure Individual Development Plan (IDP) and Goal operations for managers and team members.

- [x] T013 [US2] Update `Maliev.CareerService.Tests/Integration/DevelopmentPlanControllerTests.cs` and `DevelopmentGoalTests.cs` to verify manager and employee access levels (Failing/Red Phase).
- [x] T014 [P] [US2] Apply `[Authorize(Policy = CareerPermissions.Development.X)]` attributes to `Maliev.CareerService.Api/Controllers/DevelopmentPlansController.cs`.
- [x] T015 [P] [US2] Apply `[Authorize(Policy = CareerPermissions.Development.X)]` attributes to `Maliev.CareerService.Api/Controllers/DevelopmentGoalsController.cs`.

## Phase 5: User Story 3 - Employee Self-Service [US3]

Goal: Secure remaining endpoints for enrollment, job postings, reports, and learning resources.

- [x] T016 [US3] Update integration tests `TrainingEnrollmentTests.cs`, `JobPostingControllerTests.cs`, and `ELearningResourceTests.cs` to validate permission enforcement (Failing/Red Phase).
- [x] T017 [P] [US3] Apply `[Authorize(Policy = CareerPermissions.Trainings.Enroll)]` to `Maliev.CareerService.Api/Controllers/EnrollmentsController.cs`.
- [x] T018 [P] [US3] Apply `[Authorize(Policy = CareerPermissions.JobPostings.Read)]` to `Maliev.CareerService.Api/Controllers/JobPostingsController.cs`.
- [x] T019 [P] [US3] Apply `[Authorize(Policy = CareerPermissions.Reports.Read)]` to `Maliev.CareerService.Api/Controllers/ReportsController.cs`.
- [x] T020 [P] [US3] Apply `[Authorize(Policy = CareerPermissions.Trainings.Read)]` to `Maliev.CareerService.Api/Controllers/ELearningResourcesController.cs`.
- [x] T021 [P] [US3] Apply `[Authorize(Policy = CareerPermissions.Applications.Read)]` (or appropriate perm) to `Maliev.CareerService.Api/Controllers/ApplicationsController.cs`.

## Phase 6: Polish & Verification

Goal: Ensure system stability and zero-warning compliance.

- [x] T022 Perform a full build of the solution to verify the Zero Warnings Policy is met.
- [x] T023 Profile API latency with and without permission checks to verify SC-004 (<10ms overhead).
- [x] T024 Audit `Maliev.CareerService.Api` for `FluentValidation` usage and create an issue/plan for migration to DataAnnotations to align with Principle XIV.
- [x] T025 Update `specs/002-iam-integration/quickstart.md` with final implementation details.
- [x] T026 Verify all 16 permissions are successfully registered in logs during service startup.

## Parallel Execution Examples

- **Foundation**: T003, T004, and T008 can be developed in parallel as they are mostly constant definitions and client interfaces.
- **Controllers**: Once Phase 2 and test updates are complete, T012, T014, T015, T017, T018, T019, T020, and T021 can all be implemented in parallel.