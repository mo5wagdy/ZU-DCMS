using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZU_DCMS.INFRASTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class ClinicBugFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "CaseSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CaseSessions_ClinicId",
                table: "CaseSessions",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseSessions_Clinics_ClinicId",
                table: "CaseSessions",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseSessions_Clinics_ClinicId",
                table: "CaseSessions");

            migrationBuilder.DropIndex(
                name: "IX_CaseSessions_ClinicId",
                table: "CaseSessions");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "CaseSessions");
        }
    }
}
