using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZU_DCMS.INFRASTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class ConfigUpdatesBookingSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // __ Create sequence for booking codes __ //
            // __ This sequence will be used to generate unique booking codes in the format "BKN-000001", "BKN-000002", etc. __ //

            migrationBuilder.Sql
            (@"
                CREATE SEQUENCE BookingCodeSeq
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
