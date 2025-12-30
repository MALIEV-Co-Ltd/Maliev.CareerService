# Feature Specification: Training Records and Skills Migration

**Feature Branch**: `003-training-migration`
**Created**: 2025-12-28
**Status**: Draft
**Input**: User description: "Migrate training-related functionality from Employee Service to Career Service"

## Clarifications

### Session 2025-12-28

- Q: Who can view and modify employee training records and skills? → A: Employees can view their own records; HR/managers can view and modify any employee's records
- Q: How should the system handle Employee Service unavailability when processing employee creation events? → A: Queue employee creation events locally with retry; log failures after max retries for manual intervention
- Q: Are audit trails required for training record and skills changes? → A: Track creation and modification timestamps with user ID; no detailed change history
- Q: How should the system handle notification delivery failures beyond logging? → A: Retry failed notifications up to 3 times with exponential backoff; log persistent failures for ops review
- Q: What is the scope of manager access to training records and skills? → A: Managers can only view/modify training records and skills for their direct reports

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Track Employee Training Completion (Priority: P1)

As an HR administrator, I need to record when employees complete training courses so that I can maintain accurate compliance records and track professional development progress.

**Why this priority**: Training record tracking is the foundation for all compliance and skills management. Without accurate completion records, organizations cannot demonstrate regulatory compliance or measure employee development.

**Independent Test**: Can be fully tested by creating a training completion record for an employee, verifying it's stored correctly, and retrieving it. Delivers immediate value by replacing manual spreadsheet tracking.

**Acceptance Scenarios**:

1. **Given** an employee has completed a training course, **When** an HR admin records the completion with course name, date, and certification details, **Then** the system creates a training record with status "Completed"
2. **Given** a training record exists, **When** an HR admin views the employee's training history, **Then** all completed training appears with completion dates and expiration dates (if applicable)
3. **Given** a training course has an expiration date, **When** the expiration date is reached, **Then** the training record status changes to "Expired"

---

### User Story 2 - Monitor Certification Expiration (Priority: P2)

As an HR administrator, I need to be notified when employee certifications are approaching expiration so that I can ensure employees renew before compliance deadlines.

**Why this priority**: Proactive notification prevents compliance gaps and reduces administrative burden. This builds on P1 training records but adds automation that saves significant manual effort.

**Independent Test**: Can be fully tested by creating training records with expiration dates 30/60/90 days in the future, waiting for the scheduled notification service to run, and verifying notifications are sent. Delivers value by eliminating manual expiration tracking.

**Acceptance Scenarios**:

1. **Given** a certification expires in 90 days, **When** the daily expiration check runs, **Then** a notification is sent to the employee and their manager
2. **Given** a certification expires in 30 days, **When** the daily expiration check runs, **Then** an escalated notification is sent to the employee, manager, and HR
3. **Given** a certification has expired, **When** the daily expiration check runs, **Then** the training record status is updated to "Expired" and compliance reporting reflects the gap

---

### User Story 3 - Manage Employee Skills Matrix (Priority: P3)

As a manager or HR administrator, I need to track employee skills and proficiency levels so that I can identify development areas and align skills with career goals. Employees can view their own skills for career planning.

**Why this priority**: Skills tracking enhances the existing Individual Development Plan (IDP) functionality but isn't required for compliance. It provides strategic value for talent management and career development.

**Independent Test**: Can be fully tested by adding skills to an employee's profile as a manager, updating proficiency levels, and verifying employees can view their complete skills matrix. Delivers value by providing structured skill tracking that integrates with career planning.

**Acceptance Scenarios**:

1. **Given** a manager or HR admin is viewing an employee profile, **When** they add a new skill with proficiency level, **Then** the skill appears in the employee's skills matrix with the assessment date
2. **Given** a manager updates an employee's proficiency in a skill, **When** the proficiency level is updated, **Then** the assessment date is automatically updated to today
3. **Given** an employee views their own skills matrix, **When** they identify a skill flagged as a development area, **Then** they can see these highlighted for IDP goal setting (read-only)

---

### User Story 4 - Enforce Mandatory Training Requirements (Priority: P2)

As an HR administrator, I need to define mandatory training for specific departments or positions so that all employees in those groups complete required compliance training on time.

**Why this priority**: Mandatory training enforcement is critical for compliance but depends on training records (P1) being established. It automates compliance management and reduces risk.

**Independent Test**: Can be fully tested by creating a mandatory training requirement for a department, adding a new employee to that department, and verifying the requirement is automatically assigned. Delivers value by automating compliance tracking.

**Acceptance Scenarios**:

1. **Given** a mandatory training requirement exists for a department, **When** a new employee joins that department, **Then** the training requirement is automatically assigned with a deadline based on the configured days from hire date
2. **Given** an employee has not completed mandatory training by the deadline, **When** the daily compliance check runs, **Then** the employee and manager are notified that training is overdue
3. **Given** mandatory training requires recertification every 12 months, **When** an employee's certification expires, **Then** a new training requirement is automatically created with the recertification deadline

---

### User Story 5 - Generate Training Compliance Reports (Priority: P2)

As an HR director or compliance officer, I need comprehensive compliance reports showing training status across the organization so that I can demonstrate regulatory compliance and identify at-risk employees or departments.

**Why this priority**: Compliance reporting is essential for audits and risk management but requires the foundation of training records and mandatory requirements (P1, P2). It provides executive-level visibility.

**Independent Test**: Can be fully tested by creating training records and mandatory requirements for multiple employees across departments, then generating a compliance report and verifying the metrics are accurate. Delivers value by replacing manual compliance tracking and report creation.

**Acceptance Scenarios**:

1. **Given** employees have various training completion statuses, **When** a compliance report is generated, **Then** the report shows overall compliance rate, fully compliant count, and non-compliant count
2. **Given** multiple departments exist with different compliance rates, **When** a compliance report is generated, **Then** the report breaks down compliance by department with individual rates
3. **Given** some employees have overdue mandatory training, **When** a compliance report is generated, **Then** the report lists all overdue training with employee names, training names, due dates, and days overdue

---

### Edge Cases

- What happens when an employee completes the same training multiple times (e.g., annual recertification)? System should maintain all historical records but mark only the most recent as "active" for compliance purposes.
- How does the system handle training that was completed before the system went live? System should allow backdating completion dates and marking records as "migrated" to distinguish from new completions.
- What happens when mandatory training requirements change (e.g., a training is no longer required)? System should allow deactivating requirements without deleting historical data, and should stop assigning new requirements but preserve existing ones.
- How does the system handle employees who transfer between departments with different mandatory training requirements? System should automatically assign new department requirements while preserving previous training records.
- What happens when a training record is entered with a completion date in the future? System must reject the entry with a validation error (per business rules).
- How does the system handle duplicate skill entries for an employee? System must prevent duplicate skill names per employee and return a validation error.
- What happens when a certification expiring soon notification fails to send? System should retry up to 3 times with exponential backoff. If all retries fail, log the failure for operational review. The next scheduled run will attempt to send the notification again if still applicable.
- How does the system handle mandatory training with no recertification period? System should treat it as one-time completion with no expiration.
- What happens when an employee creation event from Employee Service cannot be processed? System should queue the event locally, retry with exponential backoff, and log failures after max retries for manual HR intervention.
- What happens when an employee's manager changes? System should immediately update access permissions so the new manager can view/modify the employee's training records and skills, while the former manager loses access.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST record employee training completions with course name, completion date, expiration date (optional), certificate document reference, training type, provider, status, and score
- **FR-002**: System MUST link training records to existing training programs when the training was taken through an internal program
- **FR-003**: System MUST prevent recording completion dates in the future
- **FR-004**: System MUST validate that expiration dates are after completion dates
- **FR-005**: System MUST update training record status to "Expired" when the expiration date is reached
- **FR-006**: System MUST track employee skills with skill name, proficiency level (1-5 scale), last assessment date, development area flag, and notes
- **FR-007**: System MUST prevent duplicate skill names for the same employee
- **FR-008**: System MUST automatically update skill assessment date when proficiency level is changed
- **FR-009**: System MUST allow defining mandatory training requirements targeting all employees, specific departments, or specific positions
- **FR-010**: System MUST automatically assign mandatory training to employees based on their department and position when requirements are created
- **FR-011**: System MUST automatically assign mandatory training to new employees when they are created based on their department and position
- **FR-012**: System MUST calculate training deadlines based on employee hire date plus configured deadline days
- **FR-013**: System MUST automatically create recertification requirements when training with recertification periods expires
- **FR-014**: System MUST send certification expiring notifications at 90, 60, and 30 days before expiration
- **FR-014a**: System MUST retry failed notification deliveries up to 3 times with exponential backoff
- **FR-014b**: System MUST log notification delivery failures that persist after 3 retry attempts for operational review
- **FR-015**: System MUST escalate overdue mandatory training notifications to managers (7 days overdue) and HR (14 days overdue)
- **FR-016**: System MUST generate compliance reports showing overall compliance rate, compliant/non-compliant employee counts, and compliance breakdown by department
- **FR-017**: System MUST list overdue training in compliance reports with employee name, training name, due date, and days overdue
- **FR-018**: System MUST publish integration events when training is completed, mandatory training becomes overdue, and certifications are expiring
- **FR-019**: System MUST consume employee creation events to automatically assign mandatory training to new hires
- **FR-019a**: System MUST queue employee creation events locally if processing fails due to temporary errors
- **FR-019b**: System MUST retry failed employee creation events with exponential backoff
- **FR-019c**: System MUST log employee creation event processing failures after maximum retry attempts for manual intervention
- **FR-020**: System MUST validate training scores are between 0-100 if provided
- **FR-021**: System MUST allow retrieving training records filtered by employee
- **FR-022**: System MUST allow retrieving certifications that are expiring within a specified timeframe
- **FR-023**: System MUST allow CRUD operations on skills for an employee
- **FR-024**: System MUST allow CRUD operations on mandatory training requirements
- **FR-025**: System MUST enforce permission-based authorization for all training and skills operations
- **FR-026**: System MUST allow employees to view their own training records and skills (read-only access)
- **FR-027**: System MUST allow HR administrators to view and modify training records and skills for any employee
- **FR-027a**: System MUST allow managers to view and modify training records and skills only for their direct reports
- **FR-027b**: System MUST prevent managers from accessing training records and skills for employees who are not their direct reports
- **FR-028**: System MUST prevent employees from modifying their own training records or skills
- **FR-029**: System MUST record creation timestamp and creating user ID for all training records, skills, and mandatory training requirements
- **FR-030**: System MUST record modification timestamp and modifying user ID whenever training records, skills, or mandatory training requirements are updated

### Key Entities

- **Training Record**: Represents completion of a training course by an employee. Includes completion date, optional expiration date, certificate reference, training type (in-person, online, self-paced, workshop, certification, external), provider, status (completed, in-progress, not-started, expired, failed), and optional score. Links to existing training programs when applicable. Audit fields: CreatedDate, CreatedBy (user ID), ModifiedDate, ModifiedBy (user ID).

- **Skill**: Represents an employee's skill and proficiency level. Includes skill name, proficiency level (1=Beginner to 5=Expert), last assessment date, development area flag for IDP integration, and optional notes. Each employee can have multiple skills but no duplicate skill names. Audit fields: CreatedDate, CreatedBy (user ID), ModifiedDate, ModifiedBy (user ID).

- **Mandatory Training Requirement**: Defines training that must be completed by specific employee groups. Includes link to training program, optional department and position targeting (null means all), completion deadline in days from hire date, optional recertification period in months, and active status. Used to automatically assign training to employees. Audit fields: CreatedDate, CreatedBy (user ID), ModifiedDate, ModifiedBy (user ID).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: HR administrators can record a training completion in under 1 minute
- **SC-002**: System automatically identifies and flags expiring certifications without manual review, reducing missed renewals by 95%
- **SC-003**: Compliance reports generate in under 5 seconds for organizations with up to 5,000 employees
- **SC-004**: New employees automatically receive all applicable mandatory training assignments within 1 hour of being created in the system
- **SC-005**: 100% of overdue mandatory training triggers escalation notifications according to the 7-day and 14-day rules
- **SC-006**: Skills matrix supports tracking of 50+ skills per employee without performance degradation
- **SC-007**: Training compliance rate improves by at least 30% within 6 months of deployment due to automated reminders and escalations
- **SC-008**: Manual effort for compliance reporting reduces by 80% compared to spreadsheet-based tracking
- **SC-009**: System supports concurrent access by 100+ HR administrators recording training completions without performance issues
- **SC-010**: Integration events are published within 30 seconds of training completion or compliance status changes
