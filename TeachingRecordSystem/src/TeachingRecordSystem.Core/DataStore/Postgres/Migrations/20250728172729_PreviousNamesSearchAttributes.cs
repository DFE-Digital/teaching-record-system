using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class PreviousNamesSearchAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Procedure("fn_delete_previous_names_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_insert_previous_names_person_search_attributes.sql");
            migrationBuilder.Procedure("p_refresh_previous_names_person_search_attributes_v1.sql");
            migrationBuilder.Trigger("trg_delete_previous_names_person_search_attributes_v1.sql");
            migrationBuilder.Trigger("trg_insert_previous_names_person_search_attributes_v1.sql");
            migrationBuilder.Trigger("trg_update_previous_names_person_search_attributes_v1.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("drop trigger trg_update_previous_names_person_search_attributes on previous_names");
            migrationBuilder.Sql("drop trigger trg_insert_previous_names_person_search_attributes on previous_names");
            migrationBuilder.Sql("drop trigger trg_delete_previous_names_person_search_attributes on previous_names");
            migrationBuilder.Sql("drop procedure p_refresh_previous_names_person_search_attributes_v1");
            migrationBuilder.Sql("drop function fn_insert_previous_names_person_search_attributes");
            migrationBuilder.Sql("drop function fn_delete_previous_names_person_search_attributes_v1");
        }
    }
}
