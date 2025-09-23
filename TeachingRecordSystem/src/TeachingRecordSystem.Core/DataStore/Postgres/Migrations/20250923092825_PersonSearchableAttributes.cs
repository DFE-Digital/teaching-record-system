using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class PersonSearchableAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "last_names",
                table: "persons",
                type: "varchar[]",
                nullable: true,
                collation: "case_insensitive");

            migrationBuilder.AddColumn<string[]>(
                name: "names",
                table: "persons",
                type: "varchar[]",
                nullable: true,
                collation: "case_insensitive");

            migrationBuilder.AddColumn<string[]>(
                name: "national_insurance_numbers",
                table: "persons",
                type: "varchar[]",
                nullable: true,
                collation: "case_insensitive");

            migrationBuilder.Procedure("fn_split_names_v1.sql");

            migrationBuilder.Procedure("p_refresh_person_names_v1.sql");

            migrationBuilder.Procedure("p_refresh_person_ninos_v1.sql");

            migrationBuilder.Procedure("fn_insert_person_attrs_v1.sql");

            migrationBuilder.Procedure("fn_update_person_attrs_v1.sql");

            migrationBuilder.Procedure("fn_insert_previous_names_person_attrs_v1.sql");

            migrationBuilder.Procedure("fn_update_previous_names_person_attrs_v1.sql");

            migrationBuilder.Procedure("fn_delete_previous_names_person_attrs_v1.sql");

            migrationBuilder.Procedure("fn_insert_tps_employments_person_attrs_v1.sql");

            migrationBuilder.Procedure("fn_update_tps_employments_person_attrs_v1.sql");

            migrationBuilder.Procedure("fn_delete_tps_employments_person_attrs_v1.sql");

            migrationBuilder.Trigger("trg_insert_person_attrs_v1.sql");

            migrationBuilder.Trigger("trg_update_person_attrs_v1.sql");

            migrationBuilder.Trigger("trg_insert_previous_names_person_attrs_v1.sql");

            migrationBuilder.Trigger("trg_update_previous_names_person_attrs_v1.sql");

            migrationBuilder.Trigger("trg_delete_previous_names_person_attrs_v1.sql");

            migrationBuilder.Trigger("trg_insert_tps_employments_person_attrs_v1.sql");

            migrationBuilder.Trigger("trg_update_tps_employments_person_attrs_v1.sql");

            migrationBuilder.Trigger("trg_delete_tps_employments_person_attrs_v1.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("drop trigger trg_delete_tps_employments_person_attrs on tps_employments;");

            migrationBuilder.Sql("drop trigger trg_update_tps_employments_person_attrs on tps_employments;");

            migrationBuilder.Sql("drop trigger trg_insert_tps_employments_person_attrs on tps_employments;");

            migrationBuilder.Sql("drop trigger trg_delete_previous_names_person_attrs on previous_names;");

            migrationBuilder.Sql("drop trigger trg_update_previous_names_person_attrs on previous_names;");

            migrationBuilder.Sql("drop trigger trg_insert_previous_names_person_attrs on previous_names;");

            migrationBuilder.Sql("drop trigger trg_update_person_attrs on persons;");

            migrationBuilder.Sql("drop trigger trg_insert_person_attrs on persons;");

            migrationBuilder.Sql("drop function fn_delete_tps_employments_person_attrs");

            migrationBuilder.Sql("drop function fn_update_tps_employments_person_attrs");

            migrationBuilder.Sql("drop function fn_insert_tps_employments_person_attrs");

            migrationBuilder.Sql("drop function fn_delete_previous_names_person_attrs;");

            migrationBuilder.Sql("drop function fn_update_previous_names_person_attrs;");

            migrationBuilder.Sql("drop function fn_insert_previous_names_person_attrs;");

            migrationBuilder.Sql("drop function fn_insert_person_attrs;");

            migrationBuilder.Sql("drop function fn_update_person_attrs;");

            migrationBuilder.Sql("drop procedure p_refresh_person_ninos;");

            migrationBuilder.Sql("drop procedure p_refresh_person_names;");

            migrationBuilder.Sql("drop function fn_split_names;");

            migrationBuilder.DropColumn(
                name: "last_names",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "names",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "national_insurance_numbers",
                table: "persons");
        }
    }
}
