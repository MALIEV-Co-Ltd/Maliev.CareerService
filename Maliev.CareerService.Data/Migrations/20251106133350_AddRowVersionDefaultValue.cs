using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maliev.CareerService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionDefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "row_version",
                table: "training_programs",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValueSql: "'\\x00000000000000000001'::bytea",
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "row_version",
                table: "job_postings",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValueSql: "'\\x00000000000000000001'::bytea",
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "row_version",
                table: "job_applications",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValueSql: "'\\x00000000000000000001'::bytea",
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "row_version",
                table: "employee_training_enrollments",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValueSql: "'\\x00000000000000000001'::bytea",
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "row_version",
                table: "elearning_resources",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValueSql: "'\\x00000000000000000001'::bytea",
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true);

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
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false, defaultValueSql: "'\\x00000000000000000001'::bytea")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_individual_development_plans", x => x.id);
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
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false, defaultValueSql: "'\\x00000000000000000001'::bytea")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_development_goals", x => x.id);
                    table.ForeignKey(
                        name: "f_k_employee_development_goals__individual_development_plans_idp_id",
                        column: x => x.idp_id,
                        principalTable: "individual_development_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_job_postings_active_list",
                table: "job_postings",
                columns: new[] { "is_active", "published_at", "application_deadline" });

            migrationBuilder.CreateIndex(
                name: "i_x_employee_development_goals_idp_id",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employee_development_goals");

            migrationBuilder.DropTable(
                name: "individual_development_plans");

            migrationBuilder.DropIndex(
                name: "idx_job_postings_active_list",
                table: "job_postings");

            migrationBuilder.AlterColumn<byte[]>(
                name: "row_version",
                table: "training_programs",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true,
                oldDefaultValueSql: "'\\x00000000000000000001'::bytea");

            migrationBuilder.AlterColumn<byte[]>(
                name: "row_version",
                table: "job_postings",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true,
                oldDefaultValueSql: "'\\x00000000000000000001'::bytea");

            migrationBuilder.AlterColumn<byte[]>(
                name: "row_version",
                table: "job_applications",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true,
                oldDefaultValueSql: "'\\x00000000000000000001'::bytea");

            migrationBuilder.AlterColumn<byte[]>(
                name: "row_version",
                table: "employee_training_enrollments",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true,
                oldDefaultValueSql: "'\\x00000000000000000001'::bytea");

            migrationBuilder.AlterColumn<byte[]>(
                name: "row_version",
                table: "elearning_resources",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true,
                oldDefaultValueSql: "'\\x00000000000000000001'::bytea");
        }
    }
}
