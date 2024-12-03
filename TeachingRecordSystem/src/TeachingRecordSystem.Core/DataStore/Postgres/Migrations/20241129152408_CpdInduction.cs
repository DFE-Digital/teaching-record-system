using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class CpdInduction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "induction_exemption_reason",
                table: "persons",
                newName: "cpd_induction_status");

            migrationBuilder.AddColumn<DateOnly>(
                name: "cpd_induction_completed_date",
                table: "persons",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "cpd_induction_cpd_modified_on",
                table: "persons",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "cpd_induction_first_modified_on",
                table: "persons",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "cpd_induction_modified_on",
                table: "persons",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "cpd_induction_start_date",
                table: "persons",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "induction_exemption_reasons",
                table: "persons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "induction_modified_on",
                table: "persons",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cpd_induction_completed_date",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "cpd_induction_cpd_modified_on",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "cpd_induction_first_modified_on",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "cpd_induction_modified_on",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "cpd_induction_start_date",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "induction_exemption_reasons",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "induction_modified_on",
                table: "persons");

            migrationBuilder.RenameColumn(
                name: "cpd_induction_status",
                table: "persons",
                newName: "induction_exemption_reason");
        }
    }
}
