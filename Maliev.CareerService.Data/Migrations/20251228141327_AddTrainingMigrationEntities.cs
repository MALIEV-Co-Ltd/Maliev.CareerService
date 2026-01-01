using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maliev.CareerService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingMigrationEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "mandatory_training_requirements");

            migrationBuilder.DropTable(
                name: "skills");

            migrationBuilder.DropTable(
                name: "training_records");
        }
    }
}
