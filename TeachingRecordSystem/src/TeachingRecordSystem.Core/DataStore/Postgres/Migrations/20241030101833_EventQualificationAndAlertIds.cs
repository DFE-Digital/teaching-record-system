using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class EventQualificationAndAlertIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "alert_id",
                table: "events",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "qualification_id",
                table: "events",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_events_alert_id_event_name",
                table: "events",
                columns: new[] { "alert_id", "event_name" },
                filter: "alert_id is not null");

            migrationBuilder.CreateIndex(
                name: "ix_events_qualification_id_event_name",
                table: "events",
                columns: new[] { "qualification_id", "event_name" },
                filter: "qualification_id is not null");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_events_alert_id_event_name",
                table: "events");

            migrationBuilder.DropIndex(
                name: "ix_events_qualification_id_event_name",
                table: "events");

            migrationBuilder.DropColumn(
                name: "alert_id",
                table: "events");

            migrationBuilder.DropColumn(
                name: "qualification_id",
                table: "events");
        }
    }
}
