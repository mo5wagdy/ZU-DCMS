using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZU_DCMS.INFRASTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientCodeSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // __ Create sequence for patient codes __ //
            // __ This sequence will be used to generate unique patient codes in the format "PAT-000001", "PAT-000002", etc. __ //

            migrationBuilder.Sql
            (@"
                CREATE SEQUENCE PatientCodeSeq
                START WITH 1
                INCREMENT BY 1
                NO CYCLE;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS PatientCodeSeq;");
        }
    }
}
