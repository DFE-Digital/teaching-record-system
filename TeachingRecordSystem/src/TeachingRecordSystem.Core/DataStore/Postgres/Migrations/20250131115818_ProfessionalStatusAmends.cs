using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ProfessionalStatusAmends : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_qualifications_countries_country_id",
                table: "qualifications");

            migrationBuilder.RenameColumn(
                name: "training_age",
                table: "qualifications",
                newName: "training_age_specialism_type");

            migrationBuilder.RenameColumn(
                name: "country_id",
                table: "qualifications",
                newName: "training_country_id");

            migrationBuilder.RenameColumn(
                name: "award_date",
                table: "qualifications",
                newName: "training_start_date");

            migrationBuilder.RenameColumn(
                name: "age_range_to",
                table: "qualifications",
                newName: "training_age_range_to");

            migrationBuilder.RenameColumn(
                name: "age_range_from",
                table: "qualifications",
                newName: "training_age_range_from");

            migrationBuilder.AddColumn<DateOnly>(
                name: "awarded_date",
                table: "qualifications",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "training_end_date",
                table: "qualifications",
                type: "date",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_qualifications_countries_training_country_id",
                table: "qualifications",
                column: "training_country_id",
                principalTable: "countries",
                principalColumn: "country_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_qualifications_countries_training_country_id",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "awarded_date",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "training_end_date",
                table: "qualifications");

            migrationBuilder.RenameColumn(
                name: "training_start_date",
                table: "qualifications",
                newName: "award_date");

            migrationBuilder.RenameColumn(
                name: "training_country_id",
                table: "qualifications",
                newName: "country_id");

            migrationBuilder.RenameColumn(
                name: "training_age_specialism_type",
                table: "qualifications",
                newName: "training_age");

            migrationBuilder.RenameColumn(
                name: "training_age_range_to",
                table: "qualifications",
                newName: "age_range_to");

            migrationBuilder.RenameColumn(
                name: "training_age_range_from",
                table: "qualifications",
                newName: "age_range_from");

            migrationBuilder.AddForeignKey(
                name: "fk_qualifications_countries_country_id",
                table: "qualifications",
                column: "country_id",
                principalTable: "countries",
                principalColumn: "country_id");
        }
    }
}
