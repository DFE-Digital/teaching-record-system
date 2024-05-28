using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class OptimizePersonSearchAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Procedure("fn_delete_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_insert_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_update_person_search_attributes_v2.sql");
            migrationBuilder.Procedure("p_refresh_person_search_attributes_v2.sql");
            migrationBuilder.Trigger("trg_delete_person_search_attributes_v1.sql");
            migrationBuilder.Trigger("trg_insert_person_search_attributes_v1.sql");
            migrationBuilder.Trigger("trg_update_person_search_attributes_v2.sql");
            migrationBuilder.Sql("drop procedure p_refresh_name_person_search_attributes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Procedure("p_refresh_name_person_search_attributes_v1.sql");
            migrationBuilder.Trigger("trg_update_person_search_attributes_v1.sql");
            migrationBuilder.Sql("drop trigger trg_insert_person_search_attributes on persons");
            migrationBuilder.Sql("drop trigger trg_delete_person_search_attributes on persons");
            migrationBuilder.Procedure("p_refresh_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_update_person_search_attributes_v1.sql");
            migrationBuilder.Sql("drop function fn_insert_person_search_attributes");
            migrationBuilder.Sql("drop function fn_delete_person_search_attributes");
        }
    }
}
