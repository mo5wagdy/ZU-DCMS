using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZU_DCMS.INFRASTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCaseAssignmentWorkflowTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AssignmentReviewedAt",
                table: "CaseAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAutoAssigned",
                table: "CaseAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNotes",
                table: "CaseAssignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByTAId",
                table: "CaseAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseAssignments_ReviewedByTAId",
                table: "CaseAssignments",
                column: "ReviewedByTAId");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseAssignments_TeachingAssistants_ReviewedByTAId",
                table: "CaseAssignments",
                column: "ReviewedByTAId",
                principalTable: "TeachingAssistants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseAssignments_TeachingAssistants_ReviewedByTAId",
                table: "CaseAssignments");

            migrationBuilder.DropIndex(
                name: "IX_CaseAssignments_ReviewedByTAId",
                table: "CaseAssignments");

            migrationBuilder.DropColumn(
                name: "AssignmentReviewedAt",
                table: "CaseAssignments");

            migrationBuilder.DropColumn(
                name: "IsAutoAssigned",
                table: "CaseAssignments");

            migrationBuilder.DropColumn(
                name: "ReviewNotes",
                table: "CaseAssignments");

            migrationBuilder.DropColumn(
                name: "ReviewedByTAId",
                table: "CaseAssignments");
        }
    }
}
