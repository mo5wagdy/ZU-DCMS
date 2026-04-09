using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ZU_DCMS.INFRASTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class Domain_Updates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NationalityCode",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Diagnosis",
                table: "DiagnosisRecords");

            migrationBuilder.DropColumn(
                name: "ProceduresDone",
                table: "CaseSessions");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Patients",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DiagnosisTypeId",
                table: "DiagnosisRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasFollowUp",
                table: "CaseSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DiagnosisTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClinicId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiagnosisTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiagnosisTypes_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Procedures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClinicId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Procedures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Procedures_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaseSessionProcedures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseSessionId = table.Column<int>(type: "int", nullable: false),
                    ProcedureId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseSessionProcedures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseSessionProcedures_CaseSessions_CaseSessionId",
                        column: x => x.CaseSessionId,
                        principalTable: "CaseSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaseSessionProcedures_Procedures_ProcedureId",
                        column: x => x.ProcedureId,
                        principalTable: "Procedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "DiagnosisTypes",
                columns: new[] { "Id", "ClinicId", "Code", "CreatedAt", "CreatedByUserId", "IsActive", "IsDeleted", "NameAr", "NameEn", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1, 1, "CARIES", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "تسوس", "Caries", null, null },
                    { 2, 1, "PULPITIS", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "التهاب لب", "Pulpitis", null, null },
                    { 3, 1, "PERIO_D", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "أمراض اللثة", "Periodontal Disease", null, null },
                    { 4, 1, "ABSCESS", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "خراج", "Abscess", null, null },
                    { 5, 1, "FRACTURE", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "كسر في السن", "Tooth Fracture", null, null },
                    { 6, 1, "MISSING", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "سن مفقود", "Missing Tooth", null, null },
                    { 7, 1, "IMPACTED", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "سن معشم", "Impacted Tooth", null, null },
                    { 8, 1, "ORTHO_N", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "تقويم مطلوب", "Orthodontic Need", null, null }
                });

            migrationBuilder.InsertData(
                table: "Procedures",
                columns: new[] { "Id", "ClinicId", "Code", "CreatedAt", "CreatedByUserId", "IsActive", "IsDeleted", "NameAr", "NameEn", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1, 5, "COMP_FILL", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "حشو كومبوزيت", "Composite Filling", null, null },
                    { 2, 5, "AMAL_FILL", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "حشو أملغم", "Amalgam Filling", null, null },
                    { 3, 2, "RCT", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "حشو عصب", "Root Canal", null, null },
                    { 4, 2, "PULPOTOMY", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "بتر لب", "Pulpotomy", null, null },
                    { 5, 3, "EXTRACT", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "خلع سن", "Extraction", null, null },
                    { 6, 3, "SURG_EXT", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "خلع جراحي", "Surgical Extraction", null, null },
                    { 7, 4, "SCALING", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "تنظيف جير", "Scaling", null, null },
                    { 8, 4, "ROOT_PLAN", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "تنعيم جذور", "Root Planing", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiagnosisRecords_DiagnosisTypeId",
                table: "DiagnosisRecords",
                column: "DiagnosisTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseSessionProcedures_CaseSessionId_ProcedureId",
                table: "CaseSessionProcedures",
                columns: new[] { "CaseSessionId", "ProcedureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseSessionProcedures_ProcedureId",
                table: "CaseSessionProcedures",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_DiagnosisTypes_ClinicId",
                table: "DiagnosisTypes",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_DiagnosisTypes_Code",
                table: "DiagnosisTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Procedures_ClinicId",
                table: "Procedures",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_Procedures_Code",
                table: "Procedures",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DiagnosisRecords_DiagnosisTypes_DiagnosisTypeId",
                table: "DiagnosisRecords",
                column: "DiagnosisTypeId",
                principalTable: "DiagnosisTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiagnosisRecords_DiagnosisTypes_DiagnosisTypeId",
                table: "DiagnosisRecords");

            migrationBuilder.DropTable(
                name: "CaseSessionProcedures");

            migrationBuilder.DropTable(
                name: "DiagnosisTypes");

            migrationBuilder.DropTable(
                name: "Procedures");

            migrationBuilder.DropIndex(
                name: "IX_DiagnosisRecords_DiagnosisTypeId",
                table: "DiagnosisRecords");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "DiagnosisTypeId",
                table: "DiagnosisRecords");

            migrationBuilder.DropColumn(
                name: "HasFollowUp",
                table: "CaseSessions");

            migrationBuilder.AddColumn<string>(
                name: "NationalityCode",
                table: "Patients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Diagnosis",
                table: "DiagnosisRecords",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProceduresDone",
                table: "CaseSessions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }
    }
}
