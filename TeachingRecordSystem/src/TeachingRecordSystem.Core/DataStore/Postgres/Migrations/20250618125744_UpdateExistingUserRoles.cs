using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExistingUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update users set role = 'RecordManager' where role = 'SupportOfficer'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update users set role = 'SupportOfficer' where role = 'RecordManager'");
        }
    }
}
