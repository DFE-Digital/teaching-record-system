using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class TrainingAgeSpecialism : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "training_age_range_to",
                table: "qualifications",
                newName: "training_age_specialism_range_to");

            migrationBuilder.RenameColumn(
                name: "training_age_range_from",
                table: "qualifications",
                newName: "training_age_specialism_range_from");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "training_age_specialism_range_to",
                table: "qualifications",
                newName: "training_age_range_to");

            migrationBuilder.RenameColumn(
                name: "training_age_specialism_range_from",
                table: "qualifications",
                newName: "training_age_range_from");
        }
    }
}
