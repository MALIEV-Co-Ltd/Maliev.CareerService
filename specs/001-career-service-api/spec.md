# Feature Specification: Career Service Web API

**Feature Branch**: `001-career-service-api`
**Created**: 2025-10-21
**Status**: Draft
**Input**: User description: "Create a career service web api project. The career service serves as API endpoint for applicants to view, search, filter, apply, and manage their job applications to MALIEV Co., Ltd. The service act as a career portal for applications, but also contain career portal for MALIEV's employees, containing available trainings, e-learning courses, self development programs. Also as a portal for HR staffs to manage career related aspect of the employees in the business. The service must communicate with external services (employee service - for managing employees related data, upload service - for uploading and managing files, auth service - for acquiring the JWT token for communication, country service - when needing to pull data about list of countries, etc)"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Job Application Lifecycle for External Applicants (Priority: P1)

External applicants can discover job openings at MALIEV Co., Ltd., search and filter positions based on their preferences, apply for positions with their credentials and documents, and track the status of their applications through the entire hiring process.

**Why this priority**: This is the core value proposition of the career service - enabling external talent acquisition. Without this, the service cannot fulfill its primary business objective of attracting and managing job applicants.

**Independent Test**: Can be fully tested by creating sample job postings, having a test applicant search for jobs, submit an application with resume upload, and view their application status. Delivers immediate value by enabling external recruitment.

**Acceptance Scenarios**:

1. **Given** a visitor on the MALIEV career portal, **When** they search for "3D printing engineer" positions in Bangkok, **Then** they see all relevant open positions with job details (title, location, department, posted date)
2. **Given** an applicant viewing a job posting, **When** they click "Apply Now" and submit required information (personal details, resume, cover letter), **Then** their application is recorded and they receive confirmation with application reference number
3. **Given** an applicant with submitted applications, **When** they log in to their applicant portal, **Then** they see all their applications with current status (Submitted, Under Review, Interview Scheduled, Rejected, Offer Extended)
4. **Given** an applicant searching for jobs, **When** they apply filters (location, department, job type, experience level), **Then** the job listings update to show only matching positions
5. **Given** an applicant with an existing application, **When** they want to update their application documents before the deadline, **Then** they can replace or add additional documents to their submission

---

### User Story 2 - Employee Learning and Development Portal (Priority: P2)

MALIEV employees can access available training programs, enroll in structured courses with capacity limits, and access self-paced e-learning resources to enhance their skills and career growth within the organization.

**Why this priority**: Employee retention and development is crucial for long-term business success. This story delivers value by supporting internal talent growth, but is secondary to external recruitment functionality.

**Independent Test**: Can be fully tested by creating sample training programs and e-learning resources, having a test employee browse available offerings, enroll in programs, access self-paced content, and view their development history. Delivers value by enabling continuous employee development.

**Acceptance Scenarios**:

1. **Given** an employee logged into the career portal, **When** they navigate to the Learning & Development section, **Then** they see all available training programs (requiring enrollment) and e-learning resources (self-paced, no enrollment needed) with descriptions, duration, and type
2. **Given** an employee viewing available training programs, **When** they filter by category (technical skills, soft skills, safety training, management) and training type (online, in-person, hybrid), **Then** they see only programs matching their criteria
3. **Given** an employee interested in a training program with available capacity, **When** they enroll in the program, **Then** the enrollment is recorded and they receive confirmation with training schedule details
4. **Given** an employee browsing e-learning resources, **When** they view a resource (video, document, interactive module, quiz), **Then** they can access the external LMS content directly without enrollment
5. **Given** an employee enrolled in training programs, **When** they access their learning dashboard, **Then** they see their enrolled courses, completion status, and completion dates
6. **Given** an employee completing a training module verified by HR, **When** HR marks the training as completed in the system, **Then** the employee's learning record is updated and visible to both the employee and HR

---

### User Story 3 - HR Staff Career Management Operations (Priority: P2)

HR staff can manage all career-related aspects of employees and applicants, including posting job openings, reviewing applications, managing candidate pipelines, administering training programs, tracking employee development, and generating reports on recruitment and learning activities.

**Why this priority**: HR operational efficiency is critical for managing both external recruitment and internal development. This story enables HR to execute their responsibilities effectively, supporting both P1 and P2 stories.

**Independent Test**: Can be fully tested by having an HR staff member create job postings, review submitted applications, schedule interviews, manage training program offerings, review employee development progress, and generate recruitment analytics reports. Delivers value by enabling HR to manage the entire talent lifecycle.

**Acceptance Scenarios**:

1. **Given** an HR staff member logged into the system, **When** they create a new job posting with details using Markdown formatting for description and requirements (title, description, requirements, location, salary range, closing date), **Then** the position appears on the public career portal with properly formatted content for applicants to view
2. **Given** an HR staff member reviewing applications, **When** they access a specific job posting, **Then** they see all applicant submissions with filtering and sorting options (application date, qualifications, status)
3. **Given** an HR staff member evaluating an applicant, **When** they update the application status (e.g., from "Under Review" to "Interview Scheduled"), **Then** the applicant sees the updated status in their portal and receives an email notification about the status change
4. **Given** an HR staff member managing training programs, **When** they create or update a learning course with content and enrollment limits, **Then** employees can see and enroll in the course up to the capacity limit
5. **Given** an HR staff member, **When** they generate a recruitment report for a specific time period, **Then** they see metrics including applications received, positions filled, average time-to-hire, and candidate pipeline status
6. **Given** an HR staff member reviewing employee development, **When** they access an employee's learning history, **Then** they see all completed and in-progress training with completion dates and assessment scores
7. **Given** an HR staff member creating a job posting, **When** they use Markdown syntax for formatting (headings, bullet lists, bold text), **Then** the content is rendered as properly formatted HTML for applicants without requiring technical knowledge

---

### User Story 4 - Employee Self-Development Planning (Priority: P3)

Employees can create and manage Individual Development Plans (IDPs), set specific development goals with action items, track progress toward goals, and submit plans for HR approval to align their growth with organizational needs and career advancement opportunities.

**Why this priority**: While valuable for employee engagement and retention, this is more of an enhancement to the basic learning functionality. It can be delivered after core training access is available.

**Independent Test**: Can be fully tested by having an employee create an IDP with multiple development goals, set target dates and action items, submit the plan for approval, have HR approve it, and track progress as goals are completed. Delivers value by enabling structured career planning with management oversight.

**Acceptance Scenarios**:

1. **Given** an employee accessing their career portal, **When** they create a new Individual Development Plan for the current year, **Then** the plan is saved in Draft status and they can add development goals to it
2. **Given** an employee with a draft IDP, **When** they add development goals specifying title, description, category (Technical/Leadership/SoftSkills/Certification), target date, and action items, **Then** each goal is saved and linked to their IDP
3. **Given** an employee with a draft IDP containing goals, **When** they submit the plan for HR approval, **Then** the plan status changes to "Submitted" and HR is notified for review
4. **Given** an HR staff member reviewing a submitted IDP, **When** they approve the plan, **Then** the plan status changes to "Approved" and the employee is notified
5. **Given** an employee with an approved IDP, **When** they update the status of individual goals (Not Started → In Progress → Completed/Deferred) and add progress notes, **Then** the goal status and notes are saved and visible to both employee and HR
6. **Given** an employee viewing their development plan dashboard, **When** they access their IDP, **Then** they see all goals with their status, target dates, completion percentage, and can add or update progress notes in Markdown format
7. **Given** an employee with only one IDP per year allowed, **When** they attempt to create a second IDP for the same year, **Then** the system prevents duplicate creation and displays an error message

---

### Edge Cases

#### Job Applications
- What happens when an applicant tries to apply for a job after the application deadline has passed?
- How does the system handle duplicate applications from the same applicant for the same position?
- What happens when an applicant tries to upload a 6th file to their application?
- How does the system handle file uploads that exceed size limits (per-file 10MB, total 25MB, or count limit of 5 files) or are in unsupported formats?
- What happens when an applicant's session expires while they are filling out a lengthy application form?
- How does the system handle data privacy requirements when an applicant requests deletion of their personal information?
- What happens when HR reverses an application status from "Rejected" back to "Under Review" - should the applicant be notified of the status change?
- How does the system handle multiple rapid status changes by different HR staff members working on the same application?
- How does the system handle concurrent updates to the same application status by multiple HR staff members?
- What happens when querying application status history for an application with hundreds of status changes?

#### Training & Learning
- What happens when an employee tries to enroll in a training program that has reached capacity?
- What happens when an employee's employment status changes (resignation, termination) - should their learning progress be retained or archived?
- How does the system handle timezone differences for training schedules when employees work in different locations?
- What happens if HR staff marks training as completed in Career Service but the employee hasn't actually completed it in the external LMS?
- How does the system handle situations where an employee completes training in the LMS but HR hasn't yet marked it complete in Career Service?
- What happens when an e-learning resource's external LMS URL becomes invalid or returns a 404 error?
- How does the system handle employees accessing e-learning resources when the external LMS is temporarily unavailable?

#### Development Plans & Goals
- What happens when an employee tries to create a second IDP for the same year?
- What happens when an employee tries to update a draft IDP that has already been submitted for approval?
- What happens when an employee tries to submit an IDP with no goals attached to it?
- How does the system handle concurrent IDP updates by the employee and their manager/HR?
- What happens when a development goal's target date passes but the goal status is still "Not Started" or "In Progress"?
- What happens when an employee tries to update a goal after their IDP has been approved?
- How does the system handle orphaned goals when an IDP is deleted?

#### System & Integration
- What happens when external service dependencies (Employee Service, Upload Service, Auth Service, Country Service) are temporarily unavailable?
- How does the system ensure metrics collection doesn't impact application performance during high-traffic periods?
- What happens when the metrics endpoint is queried with very large date ranges that could return excessive data?
- What happens when HR staff paste content with embedded HTML or scripts into Markdown fields?
- How does the system handle invalid or malformed Markdown in job postings created by HR staff?
- What happens when a user exceeds their rate limit - are they temporarily blocked, and what error message do they receive?
- How does the system handle rate limiting for shared IP addresses (e.g., applicants from same company network or internet cafe)?

## Requirements *(mandatory)*

### Functional Requirements

#### Job Application Management (External Applicants)

- **FR-001**: System MUST allow external visitors to view all active job postings without authentication
- **FR-002**: System MUST allow applicants to create accounts with email verification for application submission
- **FR-003**: System MUST provide search functionality for job postings by keywords (job title, description, skills)
- **FR-004**: System MUST provide filtering options for job postings including location, department, job type (full-time, part-time, contract), experience level, and posted date
- **FR-005**: System MUST allow authenticated applicants to submit applications including personal information, contact details, work history, education, and attached documents (resume, cover letter, portfolio)
- **FR-006**: System MUST validate required fields, file formats, file count (maximum 5 files), and total upload size (maximum 25MB combined) before accepting application submissions
- **FR-007**: System MUST generate unique application reference numbers for each submission
- **FR-008**: System MUST allow applicants to view all their submitted applications with current status
- **FR-009**: System MUST allow applicants to withdraw applications before they are reviewed
- **FR-010**: System MUST prevent duplicate applications for the same position by the same applicant
- **FR-011**: System MUST enforce application deadlines and prevent submissions after closing dates

#### Employee Learning & Development

- **FR-012**: System MUST display all available training programs and e-learning courses to authenticated employees
- **FR-013**: System MUST allow employees to filter learning content by category, duration, format (online, in-person, hybrid), and skill level
- **FR-014**: System MUST allow employees to enroll in available training programs with capacity validation
- **FR-015**: System MUST track employee enrollment status and course progress for each learning activity
- **FR-016**: System MUST record course completions and issue digital certificates or completion records
- **FR-017**: System MUST allow employees to view their learning history including completed courses, in-progress courses, and earned certifications
- **FR-018**: System MUST support employee creation and management of personal development plans with career goals
- **FR-019**: System MUST allow employees to link training courses to their development plan objectives
- **FR-020**: System MUST track and display progress toward development plan completion

#### HR Staff Operations

- **FR-021**: System MUST allow HR staff to create, edit, and publish job postings with full details (title, description, requirements, responsibilities, salary range, location, department, employment type, closing date) using Markdown formatting for description, requirements, and responsibilities fields
- **FR-021a**: System MUST render Markdown-formatted job posting content as properly formatted HTML when displayed to applicants and employees
- **FR-021b**: System MUST validate Markdown content to prevent malicious scripts or unsafe HTML injection
- **FR-022**: System MUST allow HR staff to close or unpublish job postings
- **FR-023**: System MUST allow HR staff to view all applications for specific job postings with filtering and sorting capabilities
- **FR-024**: System MUST allow HR staff to update application status through defined workflow stages (Submitted, Under Review, Interview Scheduled, Rejected, Offer Extended, Hired) with ability to reverse or modify status changes at any time
- **FR-025**: System MUST allow HR staff to add notes and evaluation comments to individual applications
- **FR-026**: System MUST allow HR staff to create and manage training programs and e-learning courses with external content references (URLs to LMS), prerequisites, capacity limits, and schedules
- **FR-027**: System MUST allow HR staff to view employee enrollment lists for each training program
- **FR-028**: System MUST allow HR staff to manually mark employee training as completed (after verification in external LMS) and record assessment scores
- **FR-029**: System MUST allow HR staff to access employee development plans and learning histories
- **FR-030**: System MUST generate reports on recruitment metrics (applications per position, time-to-hire, candidate pipeline status, acceptance rates)
- **FR-031**: System MUST generate reports on learning and development metrics (course enrollments, completion rates, employee development plan progress)

#### Integration & External Services

- **FR-032**: System MUST integrate with Employee Service to retrieve and validate employee data for authenticated users
- **FR-033**: System MUST integrate with Upload Service for secure storage and retrieval of application documents and learning materials
- **FR-034**: System MUST integrate with Auth Service to acquire JWT tokens for authenticated service-to-service communication
- **FR-035**: System MUST integrate with Country Service to populate country selection fields in application forms
- **FR-036**: System MUST handle external service unavailability gracefully with appropriate error messages and retry mechanisms
- **FR-037**: System MUST validate authentication tokens for all protected endpoints
- **FR-037a**: System MUST enforce per-user rate limits to prevent API abuse: anonymous users (100 requests/minute), applicants (200 requests/minute), employees (300 requests/minute), HR staff (500 requests/minute)

#### Data Management & Security

- **FR-038**: System MUST enforce role-based access control with distinct permissions for External Applicants, Employees, and HR Staff
- **FR-039**: System MUST encrypt sensitive personal information at rest and in transit
- **FR-040**: System MUST maintain audit logs for all data modifications including job postings, applications, employee records, and all application status transitions (including reversals and modifications)
- **FR-041**: System MUST support data retention policies for application records with a retention period of 2 years after position closure
- **FR-042**: System MUST allow applicants to request deletion of their personal data in compliance with privacy regulations
- **FR-043**: System MUST validate file uploads for type, size limits, and security threats before storage

#### Monitoring & Analytics

- **FR-044**: System MUST expose a `/metrics` endpoint providing business and operational metrics for monitoring and analytics
- **FR-045**: System MUST collect and expose recruitment metrics including total applications received, applications per job posting, application conversion rates by status, average time-to-hire, positions filled vs open, and application volume trends over time
- **FR-046**: System MUST collect and expose employee learning metrics including course enrollment rates, completion rates, time to complete courses, most popular training programs, certification achievement rates, and employee development plan adoption
- **FR-047**: System MUST collect and expose HR operational metrics including active job postings, applicant-to-interview ratio, offer acceptance rates, training capacity utilization, and average application review time
- **FR-048**: System MUST collect and expose system performance metrics including API response times, error rates, service availability, external service integration health, and concurrent user counts
- **FR-049**: System MUST provide metrics in a standard format (Prometheus-compatible) for integration with monitoring and analytics platforms
- **FR-050**: System MUST allow authorized users (HR leadership, business analysts) to access metrics data for reporting and decision-making

#### E-Learning Resources (Self-Paced Learning)

- **FR-051**: System MUST provide browsable catalog of e-learning resources (videos, documents, interactive modules, quizzes) to authenticated employees
- **FR-052**: System MUST allow employees to filter e-learning resources by type (Video, Document, Interactive, Quiz), category, and active status
- **FR-053**: System MUST provide direct links to external LMS content for each resource via ExternalLmsUrl property
- **FR-054**: System MUST track resource metadata including title, description, resource type, estimated completion time (minutes), and active status
- **FR-055**: System MUST allow HR staff to create, update, and deactivate e-learning resources
- **FR-056**: System MUST enforce unique resource codes for all e-learning resources
- **FR-057**: System MUST prevent duplicate resource codes when creating new e-learning resources with appropriate error message
- **FR-058**: System MUST support pagination for browsing large catalogs of e-learning resources
- **FR-059**: System MUST allow employees to access e-learning resources without enrollment (self-paced, on-demand access)
- **FR-060**: System MUST track resource usage metrics (views, completion rates) through separate analytics endpoints

#### Development Goals Management

- **FR-061**: System MUST allow employees to create development goals within their Individual Development Plans
- **FR-062**: System MUST support goal categories: Technical, Leadership, SoftSkills, Certification
- **FR-063**: System MUST track goal lifecycle states: NotStarted → InProgress → Completed or Deferred
- **FR-064**: System MUST allow employees to set target completion dates for development goals
- **FR-065**: System MUST allow employees to add action items and progress notes in Markdown format to development goals
- **FR-066**: System MUST allow employees to update goal status, progress notes, and actual completion date
- **FR-067**: System MUST validate that employees can only modify goals within their own IDPs
- **FR-068**: System MUST support listing all goals for a specific IDP with pagination
- **FR-069**: System MUST allow employees to delete goals from draft IDPs only
- **FR-070**: System MUST track goal completion percentage across an IDP for progress visualization

#### Individual Development Plan Workflow

- **FR-071**: System MUST enforce IDP workflow states: Draft → Submitted → Approved
- **FR-072**: System MUST allow employees to submit draft IDPs for HR approval via dedicated endpoint
- **FR-073**: System MUST record submission timestamp when IDP transitions from Draft to Submitted status
- **FR-074**: System MUST allow HR staff to approve submitted IDPs via dedicated endpoint
- **FR-075**: System MUST record approval timestamp and approver ID when IDP transitions to Approved status
- **FR-076**: System MUST prevent employees from creating duplicate IDPs for the same year with appropriate error message
- **FR-077**: System MUST prevent IDP updates in Submitted or Approved states except through proper workflow actions
- **FR-078**: System MUST allow employees to view submission and approval history for their IDPs
- **FR-079**: System MUST notify HR when an IDP is submitted for approval (notification mechanism to be defined)
- **FR-080**: System MUST allow only one active IDP per employee per year

#### Application Status History Tracking

- **FR-081**: System MUST maintain complete audit trail of all application status changes including reversals
- **FR-082**: System MUST record who made each status change, when it occurred, and reason/notes for the change
- **FR-083**: System MUST provide endpoint to retrieve full status history for any application ordered chronologically
- **FR-084**: System MUST include previous status, new status, changed by user ID, timestamp, and notes in status history
- **FR-085**: System MUST support HR reversing status changes (e.g., Rejected → Under Review) with full audit trail
- **FR-086**: System MUST persist status history separately from current application status for immutable audit log

### Key Entities *(include if feature involves data)*

- **Job Posting**: Represents an open position at MALIEV, containing title, description (Markdown-formatted), requirements (Markdown-formatted), responsibilities (Markdown-formatted), department, location, employment type, salary range, posting date, closing date, and status (active, closed, filled)

- **Application**: Represents a candidate's submission for a job posting, containing applicant information, submitted documents, application date, current status, HR notes, and complete audit trail of all status transitions (including reversals)

- **Applicant**: External user who submits job applications, containing personal information, contact details, account credentials, and relationships to their applications

- **Training Program**: Represents a structured learning opportunity with enrollment requirements, containing program code, title, description, category (Technical, Leadership, Compliance), format (online, in-person, hybrid), duration in hours, capacity limit (max participants), prerequisites, provider information, mandatory flag, external content URL (reference to LMS), schedule information, and active status

- **E-Learning Resource**: Represents self-paced learning content accessible without enrollment, containing resource code, title, description, resource type (Video, Document, Interactive, Quiz), category, estimated completion time (minutes), external LMS URL for direct content access, and active status

- **Course Enrollment**: Tracks employee participation in training programs, containing employee ID, program ID, enrollment date, enrollment status (Enrolled, Completed, Cancelled, NoShow), progress percentage, completion date, assessment scores, and cancellation reason

- **Individual Development Plan (IDP)**: Represents an employee's annual career development roadmap with workflow support, containing employee ID, plan year, workflow status (Draft, Submitted, Approved), submission timestamp, approval timestamp, approver ID, current position, desired career path, skills assessment, development priorities (Markdown-formatted), and success metrics

- **Employee Development Goal**: Represents specific, measurable objectives within an IDP, containing parent IDP ID, goal title, category (Technical, Leadership, SoftSkills, Certification), status (NotStarted, InProgress, Completed, Deferred), target completion date, actual completion date, action items (Markdown-formatted), progress notes (Markdown-formatted), and completion percentage

- **Application Status Change**: Audit trail entry for job application status transitions, containing application ID, previous status, new status, changed by user ID, change timestamp, reason/notes for change, and change type (normal transition, reversal, correction)

- **Employee**: Internal MALIEV staff member who accesses learning resources and development tools, containing employee ID (from Employee Service), role, department, and relationships to enrollments and development plans

- **HR Staff**: Administrative users who manage job postings, review applications, administer training programs, and access reporting, with extended permissions beyond regular employees

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: External applicants can discover relevant job openings, complete an application, and receive confirmation within 10 minutes from first visit
- **SC-002**: Employees can browse available training programs, enroll in a course, and access learning materials within 3 minutes
- **SC-003**: HR staff can create a new job posting and publish it to the career portal within 5 minutes
- **SC-004**: HR staff can review an application, access all submitted documents, and update status within 2 minutes
- **SC-005**: System maintains 99.5% uptime during business hours for all user-facing operations
- **SC-006**: Search and filter operations return results within 2 seconds for datasets up to 10,000 job postings
- **SC-007**: System successfully handles concurrent operations from 500 users (mix of applicants, employees, and HR staff) without performance degradation
- **SC-008**: 95% of applicants successfully complete application submission on first attempt without errors
- **SC-009**: 90% of employees successfully enroll in training programs on first attempt without assistance
- **SC-010**: Zero security incidents involving unauthorized access to applicant personal information or employee learning records
- **SC-011**: All integration points with external services (Employee, Upload, Auth, Country) maintain 99% successful request rate with proper error handling for failures
- **SC-012**: Business and HR stakeholders can access up-to-date metrics and analytics data with no more than 5-minute delay from actual events

## Assumptions

1. **Authentication**: Applicants use email/password authentication, while employees and HR staff authenticate through the existing Auth Service using corporate credentials
2. **File Storage**: The Upload Service provides secure storage with virus scanning and supports common document formats (PDF, DOC, DOCX) with limits of 10MB per file, maximum 5 files per application, and 25MB total combined size per application
3. **Employee Data**: The Employee Service provides accurate employee status, department, and role information via authenticated API calls
4. **Country Data**: The Country Service provides a standardized list of countries for address and location fields in application forms
5. **Network**: All service-to-service communication occurs over secure internal network with JWT token authentication
6. **Compliance**: Data handling follows standard GDPR and local privacy regulations requiring user consent and data deletion capabilities
7. **Deployment**: The service follows MALIEV's standard microservices architecture with Kubernetes deployment and GitOps workflow
8. **Application Lifecycle**: Standard recruitment workflow includes stages: Submitted → Under Review → Interview Scheduled → (Rejected | Offer Extended) → Hired
9. **Training Capacity**: Training programs have enrollment capacity limits managed by HR staff, with waitlist functionality for popular courses
10. **Development Plans**: Employee development plans are optional and self-managed, with visibility to HR for career pathing support
11. **Email Notifications**: Applicants receive email notifications for application status changes, account verification, and important updates
12. **Data Retention**: Application data is retained for 2 years after position closure for compliance and future reference, unless applicant requests deletion
13. **Session Management**: User sessions expire after 30 minutes of inactivity for security, with auto-save functionality for in-progress applications
14. **Markdown Support**: Job postings support standard Markdown syntax (headings, lists, bold, italic, links) with sanitization to prevent XSS attacks and script injection, making it easy for non-technical HR staff to create formatted content
15. **External LMS**: Training program content is hosted in an external Learning Management System (LMS); Career Service stores references/URLs to content and tracks employee completion status locally through manual HR verification (no automated LMS integration for completion sync)
16. **Rate Limiting**: API requests are rate-limited per user based on role to prevent abuse and ensure fair resource allocation across all users

## Dependencies

1. **Employee Service**: Required for employee authentication, role verification, and department/organizational data
2. **Upload Service**: Required for storing and retrieving application documents and training materials
3. **Auth Service**: Required for JWT token generation and validation for service-to-service communication
4. **Country Service**: Required for populating location and address selection fields
5. **Database**: PostgreSQL database for storing job postings, applications, training programs, enrollments, and development plans
6. **Email Service**: Required for applicant account verification and email notifications for application status updates
7. **Monitoring Infrastructure**: Prometheus or compatible metrics collection system for ingesting and visualizing business and operational metrics
8. **External LMS**: Learning Management System hosting training content (videos, documents, SCORM packages); Career Service references content via URLs
9. **Kubernetes Infrastructure**: GKE cluster with ArgoCD for automated deployment
10. **Secrets Management**: Google Secret Manager for storing database credentials and service API keys

## Out of Scope

1. **Interview Scheduling**: Calendar integration and interview room booking functionality (managed through separate HR systems)
2. **Offer Letter Generation**: Automated generation of employment offer documents (handled by HR document management system)
3. **Payroll Integration**: Connection to payroll systems for employee compensation data
4. **Performance Reviews**: Employee performance evaluation and review processes (separate performance management system)
5. **Background Checks**: Integration with third-party background verification services
6. **Applicant Testing**: Online skill assessments or aptitude tests for candidates
7. **Video Interviews**: Video conferencing integration for remote interviews
8. **Social Media Integration**: Sharing job postings to LinkedIn, Facebook, or other social platforms
9. **Referral Programs**: Employee referral tracking and reward management
10. **Advanced Analytics**: Machine learning-based candidate matching or predictive hiring analytics
11. **Mobile Applications**: Native iOS or Android apps (initial scope is web API only)
12. **Public API**: Third-party developer access to career data (internal services only)

## Clarifications

### Session 2025-10-21

- Q: How should training program content (videos, documents, SCORM packages, interactive modules) be stored and delivered? → A: Store references/URLs to external LMS content; Career Service tracks completion status; employee data from Employee Service
- Q: How should training completion status be synchronized between the external LMS and Career Service? → A: HR staff manually marks training as completed in Career Service after verifying in LMS
- Q: Can HR staff reverse or undo application status changes (e.g., move from "Rejected" back to "Under Review")? → A: Allow HR to reverse/undo status changes with full audit trail of all transitions
- Q: What are the total file count and combined size limits for application uploads? → A: 5 files maximum per application, 25MB total combined size
- Q: What rate limiting strategy should be implemented to prevent API abuse? → A: Per-user rate limits: anonymous (100 req/min), applicants (200 req/min), employees (300 req/min), HR (500 req/min)
