using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZU_DCMS.INFRASTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class NewBugFixs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CaseAssignmentId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CaseAssignmentId",
                table: "Bookings",
                column: "CaseAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ClinicId",
                table: "Bookings",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_CaseAssignments_CaseAssignmentId",
                table: "Bookings",
                column: "CaseAssignmentId",
                principalTable: "CaseAssignments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Clinics_ClinicId",
                table: "Bookings",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_CaseAssignments_CaseAssignmentId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Clinics_ClinicId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CaseAssignmentId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ClinicId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CaseAssignmentId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "Bookings");
        }
    }
}
