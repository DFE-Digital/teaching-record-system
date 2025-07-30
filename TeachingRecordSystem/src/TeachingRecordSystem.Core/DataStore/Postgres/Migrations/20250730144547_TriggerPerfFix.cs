using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class TriggerPerfFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Procedure("p_refresh_previous_names_person_search_attributes_v2.sql");
            migrationBuilder.Procedure("p_refresh_person_search_attributes_v6.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Procedure("p_refresh_person_search_attributes_v5.sql");
            migrationBuilder.Procedure("p_refresh_previous_names_person_search_attributes_v1.sql");
        }
    }
}
