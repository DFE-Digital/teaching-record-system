using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RemovePayloadFromEventIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_events_person_id_event_name",
                table: "events");

            migrationBuilder.CreateIndex(
                name: "ix_events_person_id_event_name",
                table: "events",
                columns: new[] { "person_id", "event_name" },
                filter: "person_id is not null");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_events_person_id_event_name",
                table: "events");

            migrationBuilder.CreateIndex(
                name: "ix_events_person_id_event_name",
                table: "events",
                columns: new[] { "person_id", "event_name" },
                filter: "person_id is not null")
                .Annotation("Npgsql:IndexInclude", new[] { "payload" });
        }
    }
}
