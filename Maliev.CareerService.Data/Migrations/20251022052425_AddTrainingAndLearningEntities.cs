using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maliev.CareerService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingAndLearningEntities : Migration
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
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_elearning_resources", x => x.id);
                    table.CheckConstraint("chk_elearning_resources_estimated_minutes_positive", "estimated_minutes IS NULL OR estimated_minutes > 0");
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
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_programs", x => x.id);
                    table.CheckConstraint("chk_training_programs_duration_positive", "duration_hours > 0");
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
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_training_enrollments", x => x.id);
                    table.CheckConstraint("chk_employee_training_enrollments_dates", "completed_at IS NULL OR started_at IS NOT NULL");
                    table.ForeignKey(
                        name: "f_k_employee_training_enrollments__training_programs_training_pro~",
                        column: x => x.training_program_id,
                        principalTable: "training_programs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                columns: ["training_program_id", "employee_id"],
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "elearning_resources");

            migrationBuilder.DropTable(
                name: "employee_training_enrollments");

            migrationBuilder.DropTable(
                name: "training_programs");
        }
    }
}
