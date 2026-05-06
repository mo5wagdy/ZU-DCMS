using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZU_DCMS.INFRASTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class StuidUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TermRequirements_StudentId_TermId_ClinicId",
                table: "TermRequirements");

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "TermRequirements",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_TermRequirements_StudentId_TermId_ClinicId_AcademicYear",
                table: "TermRequirements",
                columns: new[] { "StudentId", "TermId", "ClinicId", "AcademicYear" },
                unique: true,
                filter: "[StudentId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TermRequirements_StudentId_TermId_ClinicId_AcademicYear",
                table: "TermRequirements");

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "TermRequirements",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TermRequirements_StudentId_TermId_ClinicId",
                table: "TermRequirements",
                columns: new[] { "StudentId", "TermId", "ClinicId" },
                unique: true);
        }
    }
}
