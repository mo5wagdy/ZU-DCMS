using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZU_DCMS.INFRASTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffCodeSequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql
            (@"
                CREATE SEQUENCE StudentCodeSeq
                    START WITH 1 INCREMENT BY 1 NO CYCLE;
        
                CREATE SEQUENCE DoctorCodeSeq
                    START WITH 1 INCREMENT BY 1 NO CYCLE;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
