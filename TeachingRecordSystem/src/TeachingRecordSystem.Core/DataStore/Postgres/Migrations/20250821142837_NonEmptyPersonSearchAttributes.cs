using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class NonEmptyPersonSearchAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Procedure("p_refresh_person_search_attributes_v7.sql");
            migrationBuilder.Procedure("p_refresh_previous_names_person_search_attributes_v3.sql");
            migrationBuilder.Procedure("p_refresh_tps_employments_person_search_attributes_v2.sql");

            migrationBuilder.Sql("delete from person_search_attributes where trim(attribute_value) = '';");

            migrationBuilder.Sql("alter table person_search_attributes add constraint ck_person_search_attributes_value check (trim(attribute_value) != '');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("alter table person_search_attributes drop constraint ck_person_search_attributes_value;");

            migrationBuilder.Procedure("p_refresh_tps_employments_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("p_refresh_previous_names_person_search_attributes_v2.sql");
            migrationBuilder.Procedure("p_refresh_person_search_attributes_v6.sql");
        }
    }
}
