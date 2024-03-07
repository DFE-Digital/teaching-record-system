using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class EventPersonId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "person_id",
                table: "events",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                update events set person_id = (payload->>'PersonId')::uuid where person_id is null;
                """);

            migrationBuilder.CreateIndex(
                name: "ix_events_person_id_event_name",
                table: "events",
                columns: new[] { "person_id", "event_name" },
                filter: "person_id is not null")
                .Annotation("Npgsql:IndexInclude", new[] { "payload" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_events_person_id_event_name",
                table: "events");

            migrationBuilder.DropColumn(
                name: "person_id",
                table: "events");
        }
    }
}
