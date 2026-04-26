using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZU_DCMS.INFRASTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class AddingTATableWithBugFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TeachingAssistantId",
                table: "CaseReviews",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "TeachingAssistants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TACode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingAssistants", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseReviews_CaseAssignmentId",
                table: "CaseReviews",
                column: "CaseAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseReviews_TeachingAssistantId",
                table: "CaseReviews",
                column: "TeachingAssistantId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingAssistants_ApplicationUserId",
                table: "TeachingAssistants",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeachingAssistants_TACode",
                table: "TeachingAssistants",
                column: "TACode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseReviews_CaseAssignments_CaseAssignmentId",
                table: "CaseReviews",
                column: "CaseAssignmentId",
                principalTable: "CaseAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseReviews_TeachingAssistants_TeachingAssistantId",
                table: "CaseReviews",
                column: "TeachingAssistantId",
                principalTable: "TeachingAssistants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql
            (@"
                CREATE SEQUENCE TACodeSeq
                    START WITH 1 INCREMENT BY 1 NO CYCLE;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseReviews_CaseAssignments_CaseAssignmentId",
                table: "CaseReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseReviews_TeachingAssistants_TeachingAssistantId",
                table: "CaseReviews");

            migrationBuilder.DropTable(
                name: "TeachingAssistants");

            migrationBuilder.DropIndex(
                name: "IX_CaseReviews_CaseAssignmentId",
                table: "CaseReviews");

            migrationBuilder.DropIndex(
                name: "IX_CaseReviews_TeachingAssistantId",
                table: "CaseReviews");

            migrationBuilder.AlterColumn<string>(
                name: "TeachingAssistantId",
                table: "CaseReviews",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
