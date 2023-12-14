using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class GuidEventId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
               """
                update events set
                payload = jsonb_set(payload, array['EventId'], to_jsonb(gen_random_uuid()::text), true)
                where payload->>'EventId' is null;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
               """
                update events set
                payload = payload - 'EventId'
                where payload->>'EventId' is not null;
                """);
        }
    }
}
