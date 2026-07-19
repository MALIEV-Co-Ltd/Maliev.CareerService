# Data Model: Career Service Web API

**Feature**: [spec.md](spec.md)
**Created**: 2025-10-21
**Status**: Draft - Phase 1 Design

## Overview

This document defines the complete data model for the Career Service API. All entities follow:
- **Snake_case naming** for database tables and columns
- **PostgreSQL 18** as the target database
- **Entity Framework Core 9.0.9** for ORM
- **Optimistic concurrency** using `row_version` byte arrays
- **Soft deletes** for all entities
- **Audit fields** (created_at, updated_at, created_by, updated_by)

---

## 1. Job Posting Entity

Represents external job opportunities posted by HR staff.

### Table: `job_postings`

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `id` | uuid | No | gen_random_uuid() | Primary key |
| `position_title` | varchar(200) | No | - | Job position name |
| `position_code` | varchar(50) | No | - | Unique position identifier |
| `department` | varchar(100) | No | - | Department name |
| `location` | varchar(200) | No | - | Work location |
| `employment_type` | varchar(50) | No | - | Full-time, Part-time, Contract, Intern |
| `salary_min` | decimal(18,2) | Yes | null | Minimum salary (optional) |
| `salary_max` | decimal(18,2) | Yes | null | Maximum salary (optional) |
| `currency` | varchar(3) | No | 'THB' | ISO 4217 currency code |
| `description` | text | No | - | **Markdown** formatted job description |
| `requirements` | text | No | - | **Markdown** formatted requirements |
| `responsibilities` | text | No | - | **Markdown** formatted responsibilities |
| `application_deadline` | timestamptz | No | - | Application cutoff date |
| `published_at` | timestamptz | Yes | null | Public visibility timestamp |
| `is_active` | boolean | No | true | Active/inactive status |
| `is_deleted` | boolean | No | false | Soft delete flag |
| `created_at` | timestamptz | No | now() | Record creation timestamp |
| `updated_at` | timestamptz | No | now() | Last update timestamp |
| `created_by` | uuid | No | - | Creating user ID (from Auth service) |
| `updated_by` | uuid | No | - | Last updating user ID |
| `row_version` | bytea | No | - | Optimistic concurrency token |

### Indexes
```sql
CREATE INDEX idx_job_postings_active ON job_postings(is_active, application_deadline) WHERE is_deleted = false;
CREATE INDEX idx_job_postings_department ON job_postings(department) WHERE is_deleted = false;
CREATE INDEX idx_job_postings_employment_type ON job_postings(employment_type) WHERE is_deleted = false;
CREATE UNIQUE INDEX idx_job_postings_position_code ON job_postings(position_code) WHERE is_deleted = false;
```

### Constraints
- `position_code` must be unique among non-deleted records
- `application_deadline` must be in the future at creation time
- `salary_min` <= `salary_max` if both provided
- `published_at` must be <= `application_deadline` if set

---

## 2. Job Application Entity

Represents external applicant submissions for job postings.

### Table: `job_applications`

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `id` | uuid | No | gen_random_uuid() | Primary key |
| `job_posting_id` | uuid | No | - | Foreign key to job_postings |
| `applicant_first_name` | varchar(100) | No | - | Applicant's first name |
| `applicant_last_name` | varchar(100) | No | - | Applicant's last name |
| `applicant_email` | varchar(254) | No | - | Contact email (unique per posting) |
| `applicant_phone` | varchar(20) | No | - | Contact phone number |
| `applicant_country_code` | varchar(2) | No | - | ISO 3166-1 alpha-2 country code |
| `resume_file_id` | uuid | No | - | File ID from Upload Service |
| `cover_letter` | text | Yes | null | Optional cover letter |
| `additional_file_ids` | uuid[] | No | '{}' | Array of file IDs (max 4 additional files) |
| `status` | varchar(50) | No | 'submitted' | Application status (state machine) |
| `applied_at` | timestamptz | No | now() | Submission timestamp |
| `is_deleted` | boolean | No | false | Soft delete flag |
| `created_at` | timestamptz | No | now() | Record creation timestamp |
| `updated_at` | timestamptz | No | now() | Last update timestamp |
| `created_by` | uuid | No | - | Creating user ID (applicant) |
| `updated_by` | uuid | No | - | Last updating user ID |
| `row_version` | bytea | No | - | Optimistic concurrency token |

### Status Values (State Machine)
```
submitted → under_review → interviewing → offered → accepted / rejected / withdrawn
```

### Indexes
```sql
CREATE INDEX idx_job_applications_posting ON job_applications(job_posting_id, status) WHERE is_deleted = false;
CREATE INDEX idx_job_applications_email ON job_applications(applicant_email) WHERE is_deleted = false;
CREATE INDEX idx_job_applications_status ON job_applications(status) WHERE is_deleted = false;
CREATE INDEX idx_job_applications_applied_at ON job_applications(applied_at DESC);
```

### Constraints
- `applicant_email` must be unique per `job_posting_id` (composite unique constraint)
- `additional_file_ids` array must have <= 4 elements
- Total file count (`resume_file_id` + `additional_file_ids`) <= 5 files
- `status` must be valid state machine value

---

## 3. Application Status Change Entity

Audit trail for all application status transitions.

### Table: `application_status_changes`

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `id` | uuid | No | gen_random_uuid() | Primary key |
| `application_id` | uuid | No | - | Foreign key to job_applications |
| `from_status` | varchar(50) | Yes | null | Previous status (null for initial submission) |
| `to_status` | varchar(50) | No | - | New status |
| `changed_by` | uuid | No | - | User who made the change |
| `changed_at` | timestamptz | No | now() | Timestamp of change |
| `reason` | text | Yes | null | Optional reason/notes |
| `is_reversal` | boolean | No | false | True if this is a status reversal |
| `reversed_change_id` | uuid | Yes | null | Reference to original change being reversed |

### Indexes
```sql
CREATE INDEX idx_status_changes_application ON application_status_changes(application_id, changed_at DESC);
CREATE INDEX idx_status_changes_changed_by ON application_status_changes(changed_by);
```

### Constraints
- `to_status` must be valid state machine value
- If `is_reversal = true`, `reversed_change_id` must not be null

---

## 4. Training Program Entity

Represents structured training programs for employee development.

### Table: `training_programs`

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `id` | uuid | No | gen_random_uuid() | Primary key |
| `program_code` | varchar(50) | No | - | Unique program identifier |
| `program_name` | varchar(200) | No | - | Program display name |
| `description` | text | No | - | Program description |
| `category` | varchar(100) | No | - | Training category |
| `duration_hours` | int | No | - | Estimated duration in hours |
| `provider` | varchar(200) | Yes | null | Training provider name |
| `external_lms_url` | varchar(500) | Yes | null | URL to external LMS content |
| `is_mandatory` | boolean | No | false | Mandatory for certain roles |
| `target_roles` | varchar(50)[] | No | '{}' | Array of target role names |
| `is_active` | boolean | No | true | Active/inactive status |
| `is_deleted` | boolean | No | false | Soft delete flag |
| `created_at` | timestamptz | No | now() | Record creation timestamp |
| `updated_at` | timestamptz | No | now() | Last update timestamp |
| `created_by` | uuid | No | - | Creating user ID |
| `updated_by` | uuid | No | - | Last updating user ID |
| `row_version` | bytea | No | - | Optimistic concurrency token |

### Indexes
```sql
CREATE UNIQUE INDEX idx_training_programs_code ON training_programs(program_code) WHERE is_deleted = false;
CREATE INDEX idx_training_programs_category ON training_programs(category) WHERE is_deleted = false;
CREATE INDEX idx_training_programs_active ON training_programs(is_active) WHERE is_deleted = false;
```

### Constraints
- `program_code` must be unique among non-deleted records
- `duration_hours` must be > 0

---

## 5. E-Learning Resource Entity

Represents individual e-learning materials and resources.

### Table: `elearning_resources`

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `id` | uuid | No | gen_random_uuid() | Primary key |
| `resource_code` | varchar(50) | No | - | Unique resource identifier |
| `title` | varchar(200) | No | - | Resource title |
| `description` | text | No | - | Resource description |
| `resource_type` | varchar(50) | No | - | video, document, interactive, quiz |
| `category` | varchar(100) | No | - | Learning category |
| `external_lms_url` | varchar(500) | No | - | URL to external LMS content |
| `estimated_minutes` | int | No | - | Estimated completion time |
| `is_active` | boolean | No | true | Active/inactive status |
| `is_deleted` | boolean | No | false | Soft delete flag |
| `created_at` | timestamptz | No | now() | Record creation timestamp |
| `updated_at` | timestamptz | No | now() | Last update timestamp |
| `created_by` | uuid | No | - | Creating user ID |
| `updated_by` | uuid | No | - | Last updating user ID |
| `row_version` | bytea | No | - | Optimistic concurrency token |

### Indexes
```sql
CREATE UNIQUE INDEX idx_elearning_resources_code ON elearning_resources(resource_code) WHERE is_deleted = false;
CREATE INDEX idx_elearning_resources_category ON elearning_resources(category) WHERE is_deleted = false;
CREATE INDEX idx_elearning_resources_type ON elearning_resources(resource_type) WHERE is_deleted = false;
```

### Constraints
- `resource_code` must be unique among non-deleted records
- `estimated_minutes` must be > 0
- `resource_type` must be one of: video, document, interactive, quiz

---

## 6. Employee Training Enrollment Entity

Tracks employee enrollment and completion status for training programs.

### Table: `employee_training_enrollments`

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `id` | uuid | No | gen_random_uuid() | Primary key |
| `training_program_id` | uuid | No | - | Foreign key to training_programs |
| `employee_id` | uuid | No | - | Employee ID from Employee Service |
| `enrolled_at` | timestamptz | No | now() | Enrollment timestamp |
| `enrollment_type` | varchar(50) | No | 'voluntary' | voluntary, mandatory, assigned |
| `status` | varchar(50) | No | 'enrolled' | enrolled, in_progress, completed, cancelled |
| `started_at` | timestamptz | Yes | null | Training start timestamp |
| `completed_at` | timestamptz | Yes | null | Training completion timestamp |
| `completion_notes` | text | Yes | null | HR notes on completion |
| `marked_complete_by` | uuid | Yes | null | HR user who marked as complete |
| `is_deleted` | boolean | No | false | Soft delete flag |
| `created_at` | timestamptz | No | now() | Record creation timestamp |
| `updated_at` | timestamptz | No | now() | Last update timestamp |
| `created_by` | uuid | No | - | Creating user ID |
| `updated_by` | uuid | No | - | Last updating user ID |
| `row_version` | bytea | No | - | Optimistic concurrency token |

### Indexes
```sql
CREATE INDEX idx_training_enrollments_employee ON employee_training_enrollments(employee_id, status) WHERE is_deleted = false;
CREATE INDEX idx_training_enrollments_program ON employee_training_enrollments(training_program_id, status) WHERE is_deleted = false;
CREATE UNIQUE INDEX idx_training_enrollments_unique ON employee_training_enrollments(training_program_id, employee_id) WHERE is_deleted = false;
CREATE INDEX idx_training_enrollments_completed ON employee_training_enrollments(completed_at DESC) WHERE status = 'completed';
```

### Constraints
- Composite unique constraint on (`training_program_id`, `employee_id`) for non-deleted records
- `started_at` must be >= `enrolled_at` if set
- `completed_at` must be >= `started_at` if set
- If `status = 'completed'`, `completed_at` must not be null

---

## 7. Individual Development Plan (IDP) Entity

Represents employee career development goals and action items.

### Table: `individual_development_plans`

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `id` | uuid | No | gen_random_uuid() | Primary key |
| `employee_id` | uuid | No | - | Employee ID from Employee Service |
| `plan_year` | int | No | - | IDP year (e.g., 2025) |
| `status` | varchar(50) | No | 'draft' | draft, submitted, approved, in_progress, completed |
| `submitted_at` | timestamptz | Yes | null | Employee submission timestamp |
| `approved_at` | timestamptz | Yes | null | HR approval timestamp |
| `approved_by` | uuid | Yes | null | HR user who approved |
| `is_deleted` | boolean | No | false | Soft delete flag |
| `created_at` | timestamptz | No | now() | Record creation timestamp |
| `updated_at` | timestamptz | No | now() | Last update timestamp |
| `created_by` | uuid | No | - | Creating user ID |
| `updated_by` | uuid | No | - | Last updating user ID |
| `row_version` | bytea | No | - | Optimistic concurrency token |

### Indexes
```sql
CREATE INDEX idx_idps_employee ON individual_development_plans(employee_id, plan_year) WHERE is_deleted = false;
CREATE UNIQUE INDEX idx_idps_employee_year ON individual_development_plans(employee_id, plan_year) WHERE is_deleted = false;
CREATE INDEX idx_idps_status ON individual_development_plans(status) WHERE is_deleted = false;
```

### Constraints
- Composite unique constraint on (`employee_id`, `plan_year`) for non-deleted records
- `plan_year` must be >= current year - 1 (allow previous year updates)
- If `status = 'approved'`, `approved_at` and `approved_by` must not be null

---

## 8. Employee Development Goal Entity

Individual goals within an IDP.

### Table: `employee_development_goals`

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `id` | uuid | No | gen_random_uuid() | Primary key |
| `idp_id` | uuid | No | - | Foreign key to individual_development_plans |
| `goal_title` | varchar(200) | No | - | Goal summary |
| `goal_description` | text | No | - | Detailed goal description |
| `category` | varchar(100) | No | - | Technical, Leadership, SoftSkills, Certification |
| `target_date` | date | No | - | Goal completion target |
| `status` | varchar(50) | No | 'NotStarted' | NotStarted, InProgress, Completed, Deferred |
| `completion_date` | date | Yes | null | Actual completion date |
| `action_items` | text | Yes | null | Steps to achieve goal |
| `progress_notes` | text | Yes | null | Progress updates |
| `is_deleted` | boolean | No | false | Soft delete flag |
| `created_at` | timestamptz | No | now() | Record creation timestamp |
| `updated_at` | timestamptz | No | now() | Last update timestamp |
| `created_by` | uuid | No | - | Creating user ID |
| `updated_by` | uuid | No | - | Last updating user ID |
| `row_version` | bytea | No | - | Optimistic concurrency token |

### Indexes
```sql
CREATE INDEX idx_dev_goals_idp ON employee_development_goals(idp_id) WHERE is_deleted = false;
CREATE INDEX idx_dev_goals_status ON employee_development_goals(status) WHERE is_deleted = false;
CREATE INDEX idx_dev_goals_target_date ON employee_development_goals(target_date);
```

### Constraints
- `target_date` must be in the future at creation time
- `completion_date` must be <= current date if set
- If `status = 'Completed'`, `completion_date` must not be null
- `category` must be one of: Technical, Leadership, SoftSkills, Certification

---

## Entity Relationships

```
job_postings (1) ----< (N) job_applications
job_applications (1) ----< (N) application_status_changes

training_programs (1) ----< (N) employee_training_enrollments
employee_training_enrollments (N) >---- (1) [Employee Service]

individual_development_plans (1) ----< (N) employee_development_goals
individual_development_plans (N) >---- (1) [Employee Service]

[Upload Service] (1) ----< (N) job_applications (resume_file_id, additional_file_ids)
[Auth Service] (1) ----< (N) all entities (created_by, updated_by)
[Country Service] (1) ----< (N) job_applications (applicant_country_code)
[External LMS] ----< training_programs (external_lms_url)
[External LMS] ----< elearning_resources (external_lms_url)
```

---

## Common Patterns

### 1. Audit Fields
All entities include:
```csharp
public DateTime CreatedAt { get; set; }
public DateTime UpdatedAt { get; set; }
public Guid CreatedBy { get; set; }
public Guid UpdatedBy { get; set; }
```

### 2. Soft Delete
All entities include:
```csharp
public bool IsDeleted { get; set; }
```
Queries must filter `WHERE is_deleted = false` by default.

### 3. Optimistic Concurrency
All entities include:
```csharp
[Timestamp]
public byte[] RowVersion { get; set; }
```
EF Core automatically manages `RowVersion` on updates.

### 4. UUID Primary Keys
All entities use `uuid` (Guid in C#) as primary keys:
```sql
id uuid DEFAULT gen_random_uuid() PRIMARY KEY
```

### 5. Snake_case Configuration
EF Core model builder configuration:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Apply snake_case naming convention
    foreach (var entity in modelBuilder.Model.GetEntityTypes())
    {
        entity.SetTableName(entity.GetTableName().ToSnakeCase());

        foreach (var property in entity.GetProperties())
        {
            property.SetColumnName(property.GetColumnName().ToSnakeCase());
        }
    }
}
```

---

## Migration Strategy

1. **Initial Migration**: Create all tables with indexes and constraints
2. **Data Seeding**: No seed data required (HR staff creates initial data)
3. **Version Control**: Migrations tracked in `Maliev.CareerService.Data/Migrations/`
4. **Deployment**: Migrations applied via `dotnet ef database update` during deployment

---

## Performance Considerations

1. **Indexes**:
   - All foreign keys indexed
   - Common query patterns indexed (status, dates, filters)
   - Partial indexes for `is_deleted = false` to exclude soft-deleted records

2. **Pagination**:
   - Offset-based pagination for simple queries
   - Indexed sorting columns (applied_at, created_at, etc.)

3. **Array Columns**:
   - `additional_file_ids` (uuid[]) and `target_roles` (varchar[]) use PostgreSQL arrays
   - Efficient for small arrays (< 10 elements)

4. **Full-Text Search** (Future Enhancement):
   - Consider `ts_vector` columns for job posting descriptions
   - GIN indexes for full-text search on Markdown content

---

## Security Considerations

1. **No Sensitive Data**:
   - No passwords or payment information stored
   - External user IDs reference Auth Service

2. **Markdown Content**:
   - All Markdown fields sanitized before rendering
   - XSS prevention via HtmlSanitizer

3. **File References**:
   - File IDs validated against Upload Service
   - No direct file storage in database

4. **Audit Trail**:
   - All changes tracked via audit fields
   - Status changes fully audited in `application_status_changes`

---

## Next Steps

- [ ] Generate EF Core entity classes
- [ ] Create Fluent API configurations
- [ ] Write initial migration
- [ ] Define DTOs for API contracts
- [ ] Implement repository interfaces
