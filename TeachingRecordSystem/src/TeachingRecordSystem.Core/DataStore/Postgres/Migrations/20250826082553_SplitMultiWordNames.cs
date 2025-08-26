using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class SplitMultiWordNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Procedure("p_refresh_person_search_attributes_v8.sql");
            migrationBuilder.Procedure("p_refresh_previous_names_person_search_attributes_v4.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Procedure("p_refresh_person_search_attributes_v7.sql");
            migrationBuilder.Procedure("p_refresh_previous_names_person_search_attributes_v3.sql");
        }
    }
}
