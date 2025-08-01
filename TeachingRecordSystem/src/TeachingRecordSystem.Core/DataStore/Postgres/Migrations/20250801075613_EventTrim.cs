using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class EventTrim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_events_alert_id_event_name",
                table: "events");

            migrationBuilder.DropIndex(
                name: "ix_events_key",
                table: "events");

            migrationBuilder.DropIndex(
                name: "ix_events_payload",
                table: "events");

            migrationBuilder.DropIndex(
                name: "ix_events_qualification_id_event_name",
                table: "events");

            migrationBuilder.DropColumn(
                name: "key",
                table: "events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "key",
                table: "events",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_events_alert_id_event_name",
                table: "events",
                columns: new[] { "alert_id", "event_name" },
                filter: "alert_id is not null");

            migrationBuilder.CreateIndex(
                name: "ix_events_key",
                table: "events",
                column: "key",
                unique: true,
                filter: "key is not null");

            migrationBuilder.CreateIndex(
                name: "ix_events_payload",
                table: "events",
                column: "payload")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_events_qualification_id_event_name",
                table: "events",
                columns: new[] { "qualification_id", "event_name" },
                filter: "qualification_id is not null");
        }
    }
}
