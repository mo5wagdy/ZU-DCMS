using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZU_DCMS.INFRASTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurationsUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "Bookings");

            migrationBuilder.AlterColumn<string>(
                name: "PatientCode",
                table: "Patients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValueSql: "'PAT-' + CAST(YEAR(GETDATE()) AS VARCHAR) + '-' + RIGHT('0000' + CAST(NEXT VALUE FOR PatientCodeSeq AS VARCHAR), 4)");

            migrationBuilder.AddColumn<int>(
                name: "MaxAcademicYear",
                table: "Clinics",
                type: "int",
                nullable: false,
                defaultValue: 4);

            migrationBuilder.AddColumn<int>(
                name: "MaxCasesPerStudent",
                table: "Clinics",
                type: "int",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<int>(
                name: "MinAcademicYear",
                table: "Clinics",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TermId",
                table: "CaseAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CaseReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseAssignmentId = table.Column<int>(type: "int", nullable: false),
                    TeachingAssistantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseReviews", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "MaxAcademicYear", "MaxCasesPerStudent", "MinAcademicYear" },
                values: new object[] { 4, 5, 1 });

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "MaxAcademicYear", "MaxCasesPerStudent", "MinAcademicYear" },
                values: new object[] { 4, 3, 2 });

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "MaxAcademicYear", "MaxCasesPerStudent", "MinAcademicYear" },
                values: new object[] { 4, 3, 3 });

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "MaxAcademicYear", "MaxCasesPerStudent", "MinAcademicYear" },
                values: new object[] { 4, 3, 2 });

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "MaxAcademicYear", "MaxCasesPerStudent", "MinAcademicYear" },
                values: new object[] { 4, 3, 1 });

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "MaxAcademicYear", "MaxCasesPerStudent", "MinAcademicYear" },
                values: new object[] { 4, 3, 1 });

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "MaxAcademicYear", "MaxCasesPerStudent", "MinAcademicYear" },
                values: new object[] { 4, 3, 2 });

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "MaxAcademicYear", "MaxCasesPerStudent", "MinAcademicYear" },
                values: new object[] { 4, 3, 2 });

            migrationBuilder.CreateIndex(
                name: "IX_Patients_PhoneNumber",
                table: "Patients",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseAssignments_TermId",
                table: "CaseAssignments",
                column: "TermId");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseAssignments_Terms_TermId",
                table: "CaseAssignments",
                column: "TermId",
                principalTable: "Terms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseAssignments_Terms_TermId",
                table: "CaseAssignments");

            migrationBuilder.DropTable(
                name: "CaseReviews");

            migrationBuilder.DropIndex(
                name: "IX_Patients_PhoneNumber",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_CaseAssignments_TermId",
                table: "CaseAssignments");

            migrationBuilder.DropColumn(
                name: "MaxAcademicYear",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "MaxCasesPerStudent",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "MinAcademicYear",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "TermId",
                table: "CaseAssignments");

            migrationBuilder.AlterColumn<string>(
                name: "PatientCode",
                table: "Patients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "'PAT-' + CAST(YEAR(GETDATE()) AS VARCHAR) + '-' + RIGHT('0000' + CAST(NEXT VALUE FOR PatientCodeSeq AS VARCHAR), 4)",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<int>(
                name: "PaymentId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GatewayName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GatewayReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingId",
                table: "Payments",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PatientId",
                table: "Payments",
                column: "PatientId");
        }
    }
}
