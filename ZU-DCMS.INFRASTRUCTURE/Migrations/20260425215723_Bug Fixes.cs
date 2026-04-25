using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZU_DCMS.INFRASTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class BugFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TermRequirements_Terms_TermId",
                table: "TermRequirements");

            migrationBuilder.CreateIndex(
                name: "IX_Terms_Name_StartDate",
                table: "Terms",
                columns: new[] { "Name", "StartDate" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TermRequirements_Terms_TermId",
                table: "TermRequirements",
                column: "TermId",
                principalTable: "Terms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TermRequirements_Terms_TermId",
                table: "TermRequirements");

            migrationBuilder.DropIndex(
                name: "IX_Terms_Name_StartDate",
                table: "Terms");

            migrationBuilder.AddForeignKey(
                name: "FK_TermRequirements_Terms_TermId",
                table: "TermRequirements",
                column: "TermId",
                principalTable: "Terms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
