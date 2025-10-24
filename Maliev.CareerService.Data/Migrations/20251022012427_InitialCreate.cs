using System;
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
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_postings", x => x.id);
                    table.CheckConstraint("chk_job_postings_deadline_future", "application_deadline > created_at");
                    table.CheckConstraint("chk_job_postings_salary_range", "salary_min IS NULL OR salary_max IS NULL OR salary_min <= salary_max");
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
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_applications", x => x.id);
                    table.ForeignKey(
                        name: "f_k_job_applications__job_postings_job_posting_id",
                        column: x => x.job_posting_id,
                        principalTable: "job_postings",
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
                    reason = table.Column<string>(type: "text", nullable: true),
                    is_reversal = table.Column<bool>(type: "boolean", nullable: false),
                    reversed_change_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_status_changes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_application_status_changes__job_applications_application_id",
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
                columns: ["job_posting_id", "applicant_email"],
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_job_postings_active",
                table: "job_postings",
                column: "is_active");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "application_status_changes");

            migrationBuilder.DropTable(
                name: "job_applications");

            migrationBuilder.DropTable(
                name: "job_postings");
        }
    }
}
