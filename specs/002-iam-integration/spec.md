# Feature Specification: Permission-Based Authorization Migration

**Feature Branch**: `002-iam-integration`  
**Created**: 2025-12-21  
**Status**: Draft  
**Input**: User description: "Permission-Based Authorization Migration. Defines permissions for Training, Evaluation, Career Path, and Employee Development operations. Predefines roles: career-admin, career-hr, career-manager, career-employee with specific permission mappings."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Secure Training Management (Priority: P1)

As a Career Admin, I want to manage training programs so that the organization has up-to-date learning resources.

**Why this priority**: Training management is a core functional area of the Career Service. Ensuring only authorized users can delete or create trainings protects data integrity.

**Independent Test**: Can be fully tested by attempting to create and delete a training program with a 'career-admin' role vs a 'career-employee' role.

**Acceptance Scenarios**:

1. **Given** a user with 'career-admin' role, **When** they request to create a training program, **Then** the system allows the operation.
2. **Given** a user with 'career-employee' role, **When** they request to delete a training program, **Then** the system denies the operation with an 'Unauthorized' response.

---

### User Story 2 - Manager-led Development (Priority: P2)

As a Manager, I want to view and manage my team's development plans so that I can support their career growth.

**Why this priority**: Managerial oversight is critical for employee development workflows.

**Independent Test**: Can be tested by logging in as a 'career-manager' and attempting to view a development plan belonging to a team member.

**Acceptance Scenarios**:

1. **Given** a user with 'career-manager' role, **When** they request to view a team development plan, **Then** the system returns the plan details.
2. **Given** a user with 'career-manager' role, **When** they attempt to delete a training program, **Then** the system denies the request.

---

### User Story 3 - Employee Self-Service (Priority: P3)

As an Employee, I want to enroll in trainings and view my own career path so that I can take ownership of my development.

**Why this priority**: Self-service functionality reduces HR overhead and empowers employees.

**Independent Test**: Can be tested by an employee enrolling in a training and viewing their assigned career path.

**Acceptance Scenarios**:

1. **Given** a user with 'career-employee' role, **When** they request to enroll in an available training, **Then** the enrollment is successful.
2. **Given** a user with 'career-employee' role, **When** they request to view their own career path, **Then** the system displays the path.

---

### Edge Cases

- **Mixed Roles**: What happens when a user is assigned multiple roles? (System should grant the union of all permissions).
- **Permission Revocation**: How does the system handle an active session if a permission is revoked from a role? (Session should reflect changes on the next token refresh or request).
- **Resource Ownership**: How does the system ensure an employee with `evaluations.read` only sees their own? (Business logic must supplement permission checks with ownership validation).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST define a granular permission set for Training operations (create, read, update, delete, enroll, complete, certify).
- **FR-002**: System MUST define a granular permission set for Evaluation operations (create, read, submit, approve).
- **FR-003**: System MUST define a granular permission set for Career Path operations (view, create, assign).
- **FR-004**: System MUST define a granular permission set for Employee Development operations (view-own, view-team, manage).
- **FR-005**: System MUST provide a mechanism to register predefined roles: `career-admin`, `career-hr`, `career-manager`, and `career-employee`.
- **FR-006**: System MUST map permissions to roles according to the specification:
    - `career-admin`: All permissions.
    - `career-hr`: All except `trainings.delete` and `paths.create`.
    - `career-manager`: `evaluations.*`, `development.view-team`, `development.manage`, `trainings.read`, `trainings.enroll`.
    - `career-employee`: `trainings.read`, `trainings.enroll`, `trainings.complete`, `evaluations.read` (own), `development.view-own`, `paths.view`.
- **FR-007**: System MUST enforce these permissions across all relevant API controllers:
    - `TrainingProgramsController`
    - `DevelopmentPlansController`
    - `DevelopmentGoalsController`
    - `EnrollmentsController`
    - `JobPostingsController`
    - `ReportsController`
    - `ELearningResourcesController`
    - `ApplicationsController`

### Key Entities *(include if feature involves data)*

- **Permission**: Represents a specific action that can be performed on a resource (e.g., `career.trainings.create`).
- **Role**: A collection of permissions assigned to users (e.g., `career-manager`).
- **User-Role Mapping**: The association between a system user and one or more roles.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 16 specified permissions are registered in the Identity Access Management (IAM) system.
- **SC-002**: All 4 predefined roles are correctly configured with their respective permission sets.
- **SC-003**: Unauthorized access attempts to protected resources (e.g., an employee trying to delete a training) are blocked with 100% accuracy.
- **SC-004**: Permission checks add less than 10ms of overhead to API request processing.