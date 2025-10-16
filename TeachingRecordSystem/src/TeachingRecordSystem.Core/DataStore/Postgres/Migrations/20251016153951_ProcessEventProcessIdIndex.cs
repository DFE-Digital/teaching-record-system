using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ProcessEventProcessIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_process_events_person_ids",
                table: "process_events");

            migrationBuilder.CreateIndex(
                name: "ix_process_events_person_ids_event_name",
                table: "process_events",
                columns: new[] { "person_ids", "event_name" })
                .Annotation("Npgsql:CreatedConcurrently", true)
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "ix_process_events_process_id",
                table: "process_events",
                column: "process_id")
                .Annotation("Npgsql:CreatedConcurrently", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_process_events_person_ids_event_name",
                table: "process_events");

            migrationBuilder.DropIndex(
                name: "ix_process_events_process_id",
                table: "process_events");

            migrationBuilder.CreateIndex(
                name: "ix_process_events_person_ids",
                table: "process_events",
                column: "person_ids")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }
    }
}
