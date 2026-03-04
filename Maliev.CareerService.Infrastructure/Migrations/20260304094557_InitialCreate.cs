using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Maliev.CareerService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "e_learning_resources",
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
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_e_learning_resources", x => x.id);
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
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_individual_development_plans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "job_positions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    requirements = table.Column<string>(type: "text", nullable: true),
                    responsibilities = table.Column<string>(type: "text", nullable: true),
                    employment_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    experience_level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    salary_range_min = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    salary_range_max = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_positions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "job_postings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    position_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    position_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    employment_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    salary_min = table.Column<decimal>(type: "numeric", nullable: true),
                    salary_max = table.Column<decimal>(type: "numeric", nullable: true),
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
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_postings", x => x.id);
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
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skills", x => x.id);
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
                    duration_hours = table.Column<decimal>(type: "numeric", nullable: false),
                    provider = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    external_lms_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    target_roles = table.Column<string[]>(type: "text[]", nullable: false),
                    max_participants = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    validity_months = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_programs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "work_locations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    country_id = table.Column<int>(type: "integer", nullable: true),
                    is_remote_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    is_hybrid = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_locations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "employee_development_goals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    individual_development_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    target_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    action_items = table.Column<string>(type: "text", nullable: true),
                    progress_notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_development_goals", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_development_goals_individual_development_plans_ind~",
                        column: x => x.individual_development_plan_id,
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
                    applicant_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    applicant_country_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    resume_file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cover_letter = table.Column<string>(type: "text", nullable: true),
                    additional_file_ids = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    applied_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    JobPositionId = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_applications", x => x.id);
                    table.ForeignKey(
                        name: "FK_job_applications_job_positions_JobPositionId",
                        column: x => x.JobPositionId,
                        principalTable: "job_positions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_job_applications_job_postings_job_posting_id",
                        column: x => x.job_posting_id,
                        principalTable: "job_postings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "job_position_skills",
                columns: table => new
                {
                    job_position_id = table.Column<int>(type: "integer", nullable: false),
                    skill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    required_level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_position_skills", x => new { x.job_position_id, x.skill_id });
                    table.ForeignKey(
                        name: "FK_job_position_skills_job_positions_job_position_id",
                        column: x => x.job_position_id,
                        principalTable: "job_positions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_job_position_skills_skills_skill_id",
                        column: x => x.skill_id,
                        principalTable: "skills",
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
                    completion_notes = table.Column<string>(type: "text", nullable: true),
                    marked_complete_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_training_enrollments", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_training_enrollments_training_programs_training_pr~",
                        column: x => x.training_program_id,
                        principalTable: "training_programs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mandatory_training_requirements", x => x.id);
                    table.ForeignKey(
                        name: "FK_mandatory_training_requirements_training_programs_training_~",
                        column: x => x.training_program_id,
                        principalTable: "training_programs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                    score = table.Column<decimal>(type: "numeric", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_records_training_programs_training_program_id",
                        column: x => x.training_program_id,
                        principalTable: "training_programs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "job_position_locations",
                columns: table => new
                {
                    job_position_id = table.Column<int>(type: "integer", nullable: false),
                    work_location_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_position_locations", x => new { x.job_position_id, x.work_location_id });
                    table.ForeignKey(
                        name: "FK_job_position_locations_job_positions_job_position_id",
                        column: x => x.job_position_id,
                        principalTable: "job_positions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_job_position_locations_work_locations_work_location_id",
                        column: x => x.work_location_id,
                        principalTable: "work_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobApplicationId = table.Column<int>(type: "integer", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    GcsBucket = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GcsObjectName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    GcsUri = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UploadDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    JobApplicationId1 = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationDocuments_job_applications_JobApplicationId1",
                        column: x => x.JobApplicationId1,
                        principalTable: "job_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_reversal = table.Column<bool>(type: "boolean", nullable: false),
                    reversed_change_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_status_changes", x => x.id);
                    table.ForeignKey(
                        name: "FK_application_status_changes_job_applications_application_id",
                        column: x => x.application_id,
                        principalTable: "job_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationDocuments_JobApplicationId1",
                table: "ApplicationDocuments",
                column: "JobApplicationId1");

            migrationBuilder.CreateIndex(
                name: "IX_application_status_changes_application_id",
                table: "application_status_changes",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_development_goals_individual_development_plan_id",
                table: "employee_development_goals",
                column: "individual_development_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_training_enrollments_employee_id_training_program_~",
                table: "employee_training_enrollments",
                columns: new[] { "employee_id", "training_program_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employee_training_enrollments_training_program_id",
                table: "employee_training_enrollments",
                column: "training_program_id");

            migrationBuilder.CreateIndex(
                name: "IX_individual_development_plans_employee_id_plan_year",
                table: "individual_development_plans",
                columns: new[] { "employee_id", "plan_year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_job_applications_JobPositionId",
                table: "job_applications",
                column: "JobPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_job_applications_job_posting_id",
                table: "job_applications",
                column: "job_posting_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_applications_status",
                table: "job_applications",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_job_position_locations_work_location_id",
                table: "job_position_locations",
                column: "work_location_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_position_skills_skill_id",
                table: "job_position_skills",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_postings_is_active",
                table: "job_postings",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_job_postings_position_code",
                table: "job_postings",
                column: "position_code");

            migrationBuilder.CreateIndex(
                name: "IX_mandatory_training_requirements_training_program_id_departm~",
                table: "mandatory_training_requirements",
                columns: new[] { "training_program_id", "department_id", "position_id" });

            migrationBuilder.CreateIndex(
                name: "IX_skills_employee_id_skill_name",
                table: "skills",
                columns: new[] { "employee_id", "skill_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_training_programs_program_code",
                table: "training_programs",
                column: "program_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_training_records_employee_id",
                table: "training_records",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_training_records_training_program_id",
                table: "training_records",
                column: "training_program_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationDocuments");

            migrationBuilder.DropTable(
                name: "application_status_changes");

            migrationBuilder.DropTable(
                name: "e_learning_resources");

            migrationBuilder.DropTable(
                name: "employee_development_goals");

            migrationBuilder.DropTable(
                name: "employee_training_enrollments");

            migrationBuilder.DropTable(
                name: "job_position_locations");

            migrationBuilder.DropTable(
                name: "job_position_skills");

            migrationBuilder.DropTable(
                name: "mandatory_training_requirements");

            migrationBuilder.DropTable(
                name: "training_records");

            migrationBuilder.DropTable(
                name: "job_applications");

            migrationBuilder.DropTable(
                name: "individual_development_plans");

            migrationBuilder.DropTable(
                name: "work_locations");

            migrationBuilder.DropTable(
                name: "skills");

            migrationBuilder.DropTable(
                name: "training_programs");

            migrationBuilder.DropTable(
                name: "job_positions");

            migrationBuilder.DropTable(
                name: "job_postings");
        }
    }
}
