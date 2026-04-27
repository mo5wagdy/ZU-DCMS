using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ZU_DCMS.INFRASTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class ClinicRelationsUpdates : Migration
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

            migrationBuilder.AlterColumn<int>(
                name: "ClinicId",
                table: "Procedures",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ClinicId",
                table: "DiagnosisTypes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "ClinicDiagnosisTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicId = table.Column<int>(type: "int", nullable: false),
                    DiagnosisTypeId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicDiagnosisTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicDiagnosisTypes_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicDiagnosisTypes_DiagnosisTypes_DiagnosisTypeId",
                        column: x => x.DiagnosisTypeId,
                        principalTable: "DiagnosisTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicProcedures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicId = table.Column<int>(type: "int", nullable: false),
                    ProcedureId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicProcedures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicProcedures_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicProcedures_Procedures_ProcedureId",
                        column: x => x.ProcedureId,
                        principalTable: "Procedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ClinicDiagnosisTypes",
                columns: new[] { "Id", "ClinicId", "CreatedAt", "CreatedByUserId", "DiagnosisTypeId", "IsActive", "IsDeleted", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, true, false, null, null },
                    { 2, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, true, false, null, null },
                    { 3, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, true, false, null, null },
                    { 4, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, true, false, null, null },
                    { 5, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, true, false, null, null },
                    { 6, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, true, false, null, null },
                    { 7, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, true, false, null, null },
                    { 8, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, true, false, null, null },
                    { 16, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, true, false, null, null },
                    { 17, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, true, false, null, null },
                    { 18, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, true, false, null, null },
                    { 19, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, true, false, null, null },
                    { 20, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, true, false, null, null },
                    { 23, 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, true, false, null, null },
                    { 24, 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, true, false, null, null },
                    { 27, 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, true, false, null, null },
                    { 30, 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, true, false, null, null },
                    { 31, 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, true, false, null, null }
                });

            migrationBuilder.InsertData(
                table: "ClinicProcedures",
                columns: new[] { "Id", "ClinicId", "CreatedAt", "CreatedByUserId", "IsActive", "IsDeleted", "ProcedureId", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1, 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 1, null, null },
                    { 2, 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 2, null, null },
                    { 3, 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 3, null, null },
                    { 4, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 4, null, null },
                    { 5, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 5, null, null },
                    { 6, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 6, null, null },
                    { 7, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 7, null, null },
                    { 8, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 8, null, null },
                    { 18, 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 7, null, null }
                });

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ClinicId", "NameEn" },
                values: new object[] { null, "Dental Caries" });

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
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { null, "PULP_NEC", "نخر اللب", "Pulp Necrosis" });

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { null, "PERI_ABS", "خراج ذروي", "Periapical Abscess" });

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { null, "GINGIVITIS", "التهاب لثة", "Gingivitis" });

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { null, "PERIODONT", "أمراض اللثة", "Periodontitis" });

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
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { null, "ROOT_REM", "بقايا جذر", "Root Remnants" });

            migrationBuilder.InsertData(
                table: "DiagnosisTypes",
                columns: new[] { "Id", "ClinicId", "Code", "CreatedAt", "CreatedByUserId", "IsActive", "IsDeleted", "NameAr", "NameEn", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 9, null, "ABSCESS", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "خراج", "Abscess", null, null },
                    { 10, null, "FRACTURE", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "كسر في السن", "Tooth Fracture", null, null },
                    { 11, null, "MISSING", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "سن مفقود", "Missing Tooth", null, null },
                    { 12, null, "MALOCCLUS", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "سوء الإطباق", "Malocclusion", null, null },
                    { 13, null, "EARLY_CAR", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "تسوس مبكر للأطفال", "Early Childhood Caries", null, null },
                    { 14, null, "GINGIVAL_H", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "تضخم اللثة", "Gingival Hyperplasia", null, null },
                    { 15, null, "MOBILITY", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "تحرك الأسنان", "Tooth Mobility", null, null }
                });

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
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { null, "GIC_FILL", "حشو زجاجي", "GIC Filling" });

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { null, "AMAL_FILL", "حشو أملغم", "Amalgam Filling" });

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { null, "RCT", "حشو عصب", "Root Canal Treatment" });

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { null, "PULPECT", "استئصال لب", "Pulpectomy" });

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { null, "RETREATM", "إعادة حشو عصب", "RCT Retreatment" });

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { null, "EXTRACT", "خلع سن", "Extraction" });

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { null, "SURG_EXT", "خلع جراحي", "Surgical Extraction" });

            migrationBuilder.InsertData(
                table: "Procedures",
                columns: new[] { "Id", "ClinicId", "Code", "CreatedAt", "CreatedByUserId", "IsActive", "IsDeleted", "NameAr", "NameEn", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 9, null, "INC_DRAIN", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "فتح وتصريف خراج", "Incision & Drainage", null, null },
                    { 10, null, "BIOPSY", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "خزعة", "Biopsy", null, null },
                    { 11, null, "SCALING", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "تنظيف جير", "Scaling", null, null },
                    { 12, null, "ROOT_PLAN", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "تنعيم جذور", "Root Planing", null, null },
                    { 13, null, "GINGIVECT", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "استئصال لثة", "Gingivectomy", null, null },
                    { 14, null, "PULPOTOMY", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "بتر لب", "Pulpotomy", null, null },
                    { 15, null, "SSC_CROWN", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "تاج فولاذي للأطفال", "Stainless Steel Crown", null, null },
                    { 16, null, "FLUORIDE", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "فلورايد", "Fluoride Application", null, null },
                    { 17, null, "SEALANT", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "حماية أسنان", "Fissure Sealant", null, null },
                    { 18, null, "CROWN_PREP", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "تحضير تاج", "Crown Preparation", null, null },
                    { 19, null, "BRIDGE", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "جسر أسنان", "Bridge", null, null },
                    { 20, null, "CEMENT", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "تثبيت تركيبة", "Cementation", null, null },
                    { 21, null, "COMP_DEN", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "طقم أسنان كامل", "Complete Denture", null, null },
                    { 22, null, "PART_DEN", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "طقم أسنان جزئي", "Partial Denture", null, null },
                    { 23, null, "DEN_ADJ", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "تعديل طقم", "Denture Adjustment", null, null }
                });

            migrationBuilder.InsertData(
                table: "ClinicDiagnosisTypes",
                columns: new[] { "Id", "ClinicId", "CreatedAt", "CreatedByUserId", "DiagnosisTypeId", "IsActive", "IsDeleted", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 9, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 9, true, false, null, null },
                    { 10, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 10, true, false, null, null },
                    { 11, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 11, true, false, null, null },
                    { 12, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 12, true, false, null, null },
                    { 13, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 13, true, false, null, null },
                    { 14, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 14, true, false, null, null },
                    { 15, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 15, true, false, null, null },
                    { 21, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 9, true, false, null, null },
                    { 22, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 10, true, false, null, null },
                    { 25, 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 14, true, false, null, null },
                    { 26, 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 15, true, false, null, null },
                    { 28, 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 10, true, false, null, null },
                    { 29, 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 13, true, false, null, null },
                    { 32, 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 11, true, false, null, null },
                    { 33, 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 10, true, false, null, null },
                    { 34, 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 11, true, false, null, null }
                });

            migrationBuilder.InsertData(
                table: "ClinicProcedures",
                columns: new[] { "Id", "ClinicId", "CreatedAt", "CreatedByUserId", "IsActive", "IsDeleted", "ProcedureId", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 9, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 9, null, null },
                    { 10, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 10, null, null },
                    { 11, 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 11, null, null },
                    { 12, 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 12, null, null },
                    { 13, 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 13, null, null },
                    { 14, 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 14, null, null },
                    { 15, 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 15, null, null },
                    { 16, 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 16, null, null },
                    { 17, 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 17, null, null },
                    { 19, 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 18, null, null },
                    { 20, 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 19, null, null },
                    { 21, 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 20, null, null },
                    { 22, 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 21, null, null },
                    { 23, 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 22, null, null },
                    { 24, 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, 23, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicDiagnosisTypes_ClinicId_DiagnosisTypeId",
                table: "ClinicDiagnosisTypes",
                columns: new[] { "ClinicId", "DiagnosisTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicDiagnosisTypes_DiagnosisTypeId",
                table: "ClinicDiagnosisTypes",
                column: "DiagnosisTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicProcedures_ClinicId_ProcedureId",
                table: "ClinicProcedures",
                columns: new[] { "ClinicId", "ProcedureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicProcedures_ProcedureId",
                table: "ClinicProcedures",
                column: "ProcedureId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiagnosisTypes_Clinics_ClinicId",
                table: "DiagnosisTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Procedures_Clinics_ClinicId",
                table: "Procedures");

            migrationBuilder.DropTable(
                name: "ClinicDiagnosisTypes");

            migrationBuilder.DropTable(
                name: "ClinicProcedures");

            migrationBuilder.DeleteData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.AlterColumn<int>(
                name: "ClinicId",
                table: "Procedures",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ClinicId",
                table: "DiagnosisTypes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ClinicId", "NameEn" },
                values: new object[] { 1, "Caries" });

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "ClinicId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { 1, "PERIO_D", "أمراض اللثة", "Periodontal Disease" });

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { 1, "ABSCESS", "خراج", "Abscess" });

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { 1, "FRACTURE", "كسر في السن", "Tooth Fracture" });

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { 1, "MISSING", "سن مفقود", "Missing Tooth" });

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "ClinicId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DiagnosisTypes",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { 1, "ORTHO_N", "تقويم مطلوب", "Orthodontic Need" });

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 1,
                column: "ClinicId",
                value: 5);

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { 5, "AMAL_FILL", "حشو أملغم", "Amalgam Filling" });

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { 2, "RCT", "حشو عصب", "Root Canal" });

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { 2, "PULPOTOMY", "بتر لب", "Pulpotomy" });

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { 3, "EXTRACT", "خلع سن", "Extraction" });

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { 3, "SURG_EXT", "خلع جراحي", "Surgical Extraction" });

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { 4, "SCALING", "تنظيف جير", "Scaling" });

            migrationBuilder.UpdateData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "ClinicId", "Code", "NameAr", "NameEn" },
                values: new object[] { 4, "ROOT_PLAN", "تنعيم جذور", "Root Planing" });

            migrationBuilder.AddForeignKey(
                name: "FK_DiagnosisTypes_Clinics_ClinicId",
                table: "DiagnosisTypes",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Procedures_Clinics_ClinicId",
                table: "Procedures",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
