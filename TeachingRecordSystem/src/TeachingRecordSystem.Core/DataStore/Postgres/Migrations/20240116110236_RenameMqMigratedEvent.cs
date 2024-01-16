using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RenameMqMigratedEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "events",
                keyColumn: "event_name",
                keyValue: "MandatoryQualificationDqtMigratedEvent",
                column: "event_name",
                value: "MandatoryQualificationDqtImportedEvent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "events",
                keyColumn: "event_name",
                keyValue: "MandatoryQualificationDqtImportedEvent",
                column: "event_name",
                value: "MandatoryQualificationDqtMigratedEvent");
        }
    }
}
