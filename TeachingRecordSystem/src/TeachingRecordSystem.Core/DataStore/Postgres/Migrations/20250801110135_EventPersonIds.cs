using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class EventPersonIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE EXTENSION IF NOT EXISTS pg_trgm;
                CREATE EXTENSION IF NOT EXISTS btree_gin;
                """);

            migrationBuilder.Sql(
                "alter table events add column if not exists person_ids uuid[] not null default '{}'::uuid[]");

            migrationBuilder.Sql(
                "create index concurrently if not exists ix_events_person_ids on events using gin (person_ids, event_name)",
                suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_events_person_ids",
                table: "events");

            migrationBuilder.DropColumn(
                name: "person_ids",
                table: "events");
        }
    }
}
