using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maliev.CareerService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddValidityMonthsToTrainingProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "validity_months",
                table: "training_programs",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "validity_months",
                table: "training_programs");
        }
    }
}
