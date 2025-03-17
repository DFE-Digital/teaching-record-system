using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class EstablishmentPhasePupilsFsm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "free_school_meals_percentage",
                table: "establishments",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "number_of_pupils",
                table: "establishments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "phase_of_education_code",
                table: "establishments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phase_of_education_name",
                table: "establishments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "free_school_meals_percentage",
                table: "establishments");

            migrationBuilder.DropColumn(
                name: "number_of_pupils",
                table: "establishments");

            migrationBuilder.DropColumn(
                name: "phase_of_education_code",
                table: "establishments");

            migrationBuilder.DropColumn(
                name: "phase_of_education_name",
                table: "establishments");
        }
    }
}
