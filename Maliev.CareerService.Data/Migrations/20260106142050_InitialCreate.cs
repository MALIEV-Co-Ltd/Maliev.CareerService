using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maliev.CareerService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "elearning_resources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    resource_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    external_lms_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    estimated_minutes = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false, defaultValueSql: "'\\x00000000000000000001'::bytea")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_e_learning_resources", x => x.id);
                    table.CheckConstraint("chk_elearning_resources_estimated_minutes_positive", "estimated_minutes IS NULL OR estimated_minutes > 0");
                });

            migrationBuilder.CreateTable(
                name: "individual_development_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_year = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false, defaultValueSql: "'\\x00000000000000000001'::bytea")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_individual_development_plans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "job_postings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    position_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    position_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    employment_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    salary_min = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    salary_max = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    description = table.Column<string>(type: "text", nullable: false),
                    requirements = table.Column<string>(type: "text", nullable: false),
                    responsibilities = table.Column<string>(type: "text", nullable: false),
                    application_deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false, defaultValueSql: "'\\x00000000000000000001'::bytea")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_postings", x => x.id);
                    table.CheckConstraint("chk_job_postings_deadline_future", "application_deadline > created_at");
                    table.CheckConstraint("chk_job_postings_salary_range", "salary_min IS NULL OR salary_max IS NULL OR salary_min <= salary_max");
                });

            migrationBuilder.CreateTable(
                name: "skills",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    skill_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    proficiency_level = table.Column<int>(type: "integer", nullable: false),
                    last_assessed_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_development_area = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false, defaultValueSql: "'\\x00000000000000000001'::bytea")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_skills", x => x.id);
                    table.CheckConstraint("chk_skills_proficiency_level_range", "proficiency_level BETWEEN 1 AND 5");
                });

            migrationBuilder.CreateTable(
                name: "training_programs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    program_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    program_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    duration_hours = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    provider = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    external_lms_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    target_roles = table.Column<string[]>(type: "text[]", nullable: false),
                    max_participants = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false, defaultValueSql: "'\\x00000000000000000001'::bytea")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_training_programs", x => x.id);
                    table.CheckConstraint("chk_training_programs_duration_positive", "duration_hours > 0");
                });

            migrationBuilder.CreateTable(
                name: "employee_development_goals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    idp_id = table.Column<Guid>(type: "uuid", nullable: false),
                    goal_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    goal_description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    target_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    action_items = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    progress_notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false, defaultValueSql: "'\\x00000000000000000001'::bytea")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employee_development_goals", x => x.id);
                    table.ForeignKey(
                        name: "fk_employee_development_goals__individual_development_plans_idp_id",
                        column: x => x.idp_id,
                        principalTable: "individual_development_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "job_applications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_posting_id = table.Column<Guid>(type: "uuid", nullable: false),
                    applicant_first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    applicant_last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    applicant_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    applicant_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    applicant_country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    resume_file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cover_letter = table.Column<string>(type: "text", nullable: true),
                    additional_file_ids = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    applied_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false, defaultValueSql: "'\\x00000000000000000001'::bytea")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_applications", x => x.id);
                    table.ForeignKey(
                        name: "fk_job_applications__job_postings_job_posting_id",
                        column: x => x.job_posting_id,
                        principalTable: "job_postings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employee_training_enrollments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    training_program_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    enrolled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    enrollment_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completion_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    marked_complete_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false, defaultValueSql: "'\\x00000000000000000001'::bytea")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employee_training_enrollments", x => x.id);
                    table.CheckConstraint("chk_employee_training_enrollments_dates", "completed_at IS NULL OR started_at IS NOT NULL");
                    table.ForeignKey(
                        name: "fk_employee_training_enrollments__training_programs_training_pro~",
                        column: x => x.training_program_id,
                        principalTable: "training_programs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mandatory_training_requirements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    training_program_id = table.Column<Guid>(type: "uuid", nullable: false),
                    department_id = table.Column<Guid>(type: "uuid", nullable: true),
                    position_id = table.Column<Guid>(type: "uuid", nullable: true),
                    completion_deadline_days = table.Column<int>(type: "integer", nullable: false),
                    recertification_months = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false, defaultValueSql: "'\\x00000000000000000001'::bytea")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mandatory_training_requirements", x => x.id);
                    table.CheckConstraint("chk_mandatory_training_requirements_deadline_range", "completion_deadline_days BETWEEN 1 AND 365");
                    table.CheckConstraint("chk_mandatory_training_requirements_recertification_range", "recertification_months IS NULL OR (recertification_months BETWEEN 1 AND 120)");
                    table.ForeignKey(
                        name: "fk_mandatory_training_requirements__training_programs_training_p~",
                        column: x => x.training_program_id,
                        principalTable: "training_programs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "training_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    training_program_id = table.Column<Guid>(type: "uuid", nullable: true),
                    course_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expiration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    certificate_document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    training_type = table.Column<int>(type: "integer", nullable: false),
                    provider = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false, defaultValueSql: "'\\x00000000000000000001'::bytea")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_training_records", x => x.id);
                    table.CheckConstraint("chk_training_records_completion_not_future", "completion_date <= NOW()");
                    table.CheckConstraint("chk_training_records_expiration_after_completion", "expiration_date IS NULL OR expiration_date > completion_date");
                    table.CheckConstraint("chk_training_records_score_range", "score IS NULL OR (score >= 0 AND score <= 100)");
                    table.ForeignKey(
                        name: "fk_training_records_training_programs_training_program_id",
                        column: x => x.training_program_id,
                        principalTable: "training_programs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "application_status_changes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    application_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    to_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: false),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    is_reversal = table.Column<bool>(type: "boolean", nullable: false),
                    reversed_change_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_application_status_changes", x => x.id);
                    table.ForeignKey(
                        name: "fk_application_status_changes__job_applications_application_id",
                        column: x => x.application_id,
                        principalTable: "job_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_status_changes_application",
                table: "application_status_changes",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "idx_status_changes_changed_at",
                table: "application_status_changes",
                column: "changed_at");

            migrationBuilder.CreateIndex(
                name: "idx_status_changes_changed_by",
                table: "application_status_changes",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "ix_elearning_resources_category",
                table: "elearning_resources",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_elearning_resources_is_active",
                table: "elearning_resources",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_elearning_resources_resource_type",
                table: "elearning_resources",
                column: "resource_type");

            migrationBuilder.CreateIndex(
                name: "uq_elearning_resources_resource_code",
                table: "elearning_resources",
                column: "resource_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_employee_development_goals_idp_id",
                table: "employee_development_goals",
                column: "idp_id");

            migrationBuilder.CreateIndex(
                name: "ix_employee_development_goals_idp_id_status",
                table: "employee_development_goals",
                columns: new[] { "idp_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_employee_development_goals_target_date",
                table: "employee_development_goals",
                column: "target_date");

            migrationBuilder.CreateIndex(
                name: "ix_employee_training_enrollments_employee_id",
                table: "employee_training_enrollments",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_employee_training_enrollments_enrolled_at",
                table: "employee_training_enrollments",
                column: "enrolled_at");

            migrationBuilder.CreateIndex(
                name: "ix_employee_training_enrollments_enrollment_type",
                table: "employee_training_enrollments",
                column: "enrollment_type");

            migrationBuilder.CreateIndex(
                name: "ix_employee_training_enrollments_status",
                table: "employee_training_enrollments",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_employee_training_enrollments_training_program_id",
                table: "employee_training_enrollments",
                column: "training_program_id");

            migrationBuilder.CreateIndex(
                name: "uq_employee_training_enrollments_program_employee",
                table: "employee_training_enrollments",
                columns: new[] { "training_program_id", "employee_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_individual_development_plans_employee_id",
                table: "individual_development_plans",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_individual_development_plans_employee_id_plan_year_unique",
                table: "individual_development_plans",
                columns: new[] { "employee_id", "plan_year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_individual_development_plans_status",
                table: "individual_development_plans",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_job_applications_applied",
                table: "job_applications",
                column: "applied_at");

            migrationBuilder.CreateIndex(
                name: "idx_job_applications_email",
                table: "job_applications",
                column: "applicant_email");

            migrationBuilder.CreateIndex(
                name: "idx_job_applications_posting",
                table: "job_applications",
                column: "job_posting_id");

            migrationBuilder.CreateIndex(
                name: "idx_job_applications_status",
                table: "job_applications",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "uq_job_applications_posting_email",
                table: "job_applications",
                columns: new[] { "job_posting_id", "applicant_email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_job_postings_active",
                table: "job_postings",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_job_postings_active_list",
                table: "job_postings",
                columns: new[] { "is_active", "published_at", "application_deadline" });

            migrationBuilder.CreateIndex(
                name: "idx_job_postings_deadline",
                table: "job_postings",
                column: "application_deadline");

            migrationBuilder.CreateIndex(
                name: "idx_job_postings_department",
                table: "job_postings",
                column: "department");

            migrationBuilder.CreateIndex(
                name: "idx_job_postings_employment_type",
                table: "job_postings",
                column: "employment_type");

            migrationBuilder.CreateIndex(
                name: "idx_job_postings_published",
                table: "job_postings",
                column: "published_at");

            migrationBuilder.CreateIndex(
                name: "uq_job_postings_position_code",
                table: "job_postings",
                column: "position_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mandatory_training_requirements_department_id",
                table: "mandatory_training_requirements",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_mandatory_training_requirements_is_active",
                table: "mandatory_training_requirements",
                column: "is_active",
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "ix_mandatory_training_requirements_position_id",
                table: "mandatory_training_requirements",
                column: "position_id");

            migrationBuilder.CreateIndex(
                name: "ix_mandatory_training_requirements_training_program_id",
                table: "mandatory_training_requirements",
                column: "training_program_id");

            migrationBuilder.CreateIndex(
                name: "ix_skills_employee_id",
                table: "skills",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_skills_is_development_area",
                table: "skills",
                column: "is_development_area",
                filter: "is_development_area = true");

            migrationBuilder.CreateIndex(
                name: "uq_skills_employee_skill_name",
                table: "skills",
                columns: new[] { "employee_id", "skill_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_training_programs_category",
                table: "training_programs",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_training_programs_is_active",
                table: "training_programs",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_training_programs_is_mandatory",
                table: "training_programs",
                column: "is_mandatory");

            migrationBuilder.CreateIndex(
                name: "uq_training_programs_program_code",
                table: "training_programs",
                column: "program_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_training_records_employee_id",
                table: "training_records",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_training_records_expiration_date",
                table: "training_records",
                column: "expiration_date",
                filter: "expiration_date IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_training_records_status",
                table: "training_records",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_training_records_training_program_id",
                table: "training_records",
                column: "training_program_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "application_status_changes");

            migrationBuilder.DropTable(
                name: "elearning_resources");

            migrationBuilder.DropTable(
                name: "employee_development_goals");

            migrationBuilder.DropTable(
                name: "employee_training_enrollments");

            migrationBuilder.DropTable(
                name: "mandatory_training_requirements");

            migrationBuilder.DropTable(
                name: "skills");

            migrationBuilder.DropTable(
                name: "training_records");

            migrationBuilder.DropTable(
                name: "job_applications");

            migrationBuilder.DropTable(
                name: "individual_development_plans");

            migrationBuilder.DropTable(
                name: "training_programs");

            migrationBuilder.DropTable(
                name: "job_postings");
        }
    }
}
