using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZU_DCMS.INFRASTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class FixClinicRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiagnosisTypes_Clinics_ClinicId",
                table: "DiagnosisTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Procedures_Clinics_ClinicId",
                table: "Procedures");

            migrationBuilder.DropIndex(
                name: "IX_Procedures_ClinicId",
                table: "Procedures");

            migrationBuilder.DropIndex(
                name: "IX_DiagnosisTypes_ClinicId",
                table: "DiagnosisTypes");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "Procedures");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "DiagnosisTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "Procedures",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "DiagnosisTypes",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 10,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 11,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 12,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 13,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 14,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 15,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 1,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 2,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 3,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 4,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 5,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 6,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 7,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 8,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 9,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 10,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 11,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 12,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 13,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 14,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 15,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 16,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 17,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 18,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 19,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 20,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 21,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 22,
                column: "ClinicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 23,
                column: "ClinicId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Procedures_ClinicId",
                table: "Procedures",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_DiagnosisTypes_ClinicId",
                table: "DiagnosisTypes",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiagnosisTypes_Clinics_ClinicId",
                table: "DiagnosisTypes",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Procedures_Clinics_ClinicId",
                table: "Procedures",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id");
        }
    }
}
