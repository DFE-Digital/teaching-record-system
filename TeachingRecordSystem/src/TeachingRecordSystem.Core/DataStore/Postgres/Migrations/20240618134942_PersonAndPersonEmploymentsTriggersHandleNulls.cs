using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class PersonAndPersonEmploymentsTriggersHandleNulls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Procedure("fn_update_person_employments_person_search_attributes_v2.sql");
            migrationBuilder.Procedure("fn_update_person_search_attributes_v3.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Procedure("fn_update_person_employments_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_update_person_search_attributes_v2.sql");
        }
    }
}
