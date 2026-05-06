using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZU_DCMS.INFRASTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class ClinicalUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Clinics",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Clinics",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "NameAr", "NameEn" },
                values: new object[] { "عيادات التشخيص", "Diagnosis Clinics" });

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "NameAr", "NameEn" },
                values: new object[] { "عيادات حشو العصب", "Endodontics Clinics" });

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "NameAr", "NameEn" },
                values: new object[] { "عيادات الجراحة", "Surgery Clinics" });

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "NameAr", "NameEn" },
                values: new object[] { "عيادات طب الفم واللثة", "Periodontics Clinics" });

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "NameAr", "NameEn" },
                values: new object[] { "عيادات الحشو العادي", "Restorative Clinics" });

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "NameAr", "NameEn" },
                values: new object[] { "عيادات الأطفال", "Pediatric Dentistry Clinics" });

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "NameAr", "NameEn" },
                values: new object[] { "التركيبات الثابتة", "Fixed Prosthodontics Clinics" });

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "NameAr", "NameEn" },
                values: new object[] { "التركيبات المتحركة", "Removable Prosthodontics Clinics" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Clinics");
        }
    }
}
