using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class PersonEmploymentsAddNinoAndPostcode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "national_insurance_number",
                table: "person_employments",
                type: "character(9)",
                fixedLength: true,
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "person_postcode",
                table: "person_employments",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.Procedure("p_refresh_person_employments_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_delete_person_employments_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_insert_person_employments_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_update_person_employments_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("p_refresh_person_search_attributes_v3.sql");
            migrationBuilder.Trigger("trg_delete_person_employments_person_search_attributes_v1.sql");
            migrationBuilder.Trigger("trg_insert_person_employments_person_search_attributes_v1.sql");
            migrationBuilder.Trigger("trg_update_person_employments_person_search_attributes_v1.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Procedure("p_refresh_person_search_attributes_v2.sql");
            migrationBuilder.Sql("drop trigger trg_delete_person_employments_person_search_attributes on person_employments");
            migrationBuilder.Sql("drop trigger trg_insert_person_employments_person_search_attributes on person_employments");
            migrationBuilder.Sql("drop trigger trg_update_person_employments_person_search_attributes on person_employments");
            migrationBuilder.Sql("drop function fn_delete_person_employments_person_search_attributes");
            migrationBuilder.Sql("drop function fn_insert_person_employments_person_search_attributes");
            migrationBuilder.Sql("drop function fn_update_person_employments_person_search_attributes");
            migrationBuilder.Sql("drop procedure p_refresh_person_employments_person_search_attributes");

            migrationBuilder.DropColumn(
                name: "national_insurance_number",
                table: "person_employments");

            migrationBuilder.DropColumn(
                name: "person_postcode",
                table: "person_employments");


        }
    }
}
