using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class InductionStatusWithoutExemption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cpd_induction_completed_date",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "cpd_induction_start_date",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "cpd_induction_status",
                table: "persons");

            migrationBuilder.AddColumn<int>(
                name: "induction_status_without_exemption",
                table: "persons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "update persons set induction_status_without_exemption = case when induction_status = 2 then 5 else induction_status end");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "induction_status_without_exemption",
                table: "persons");

            migrationBuilder.AddColumn<DateOnly>(
                name: "cpd_induction_completed_date",
                table: "persons",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "cpd_induction_start_date",
                table: "persons",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cpd_induction_status",
                table: "persons",
                type: "integer",
                nullable: true);
        }
    }
}
