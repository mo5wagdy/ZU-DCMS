using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZU_DCMS.INFRASTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurationsImprovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "Sessions",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_StartTime",
                table: "Sessions",
                column: "StartTime",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sessions_StartTime",
                table: "Sessions");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "Sessions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");
        }
    }
}
