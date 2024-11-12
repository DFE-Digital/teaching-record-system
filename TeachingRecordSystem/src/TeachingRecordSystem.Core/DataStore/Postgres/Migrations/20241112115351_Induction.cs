using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Induction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "induction_completed_date",
                table: "persons",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "induction_exemption_reason",
                table: "persons",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "induction_start_date",
                table: "persons",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "induction_status",
                table: "persons",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "induction_completed_date",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "induction_exemption_reason",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "induction_start_date",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "induction_status",
                table: "persons");
        }
    }
}
