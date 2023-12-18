using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RenameEventSourceUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                update events set
                payload = jsonb_set(payload, array['RaisedBy'], to_jsonb(payload->>'SourceUserId'::text), true) - 'SourceUserId'
                where payload->>'RaisedBy' is null;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                update events set
                payload = jsonb_set(payload, array['SourceUserId'], to_jsonb(payload->>'RaisedBy'::text), true) - 'RaisedBy'
                where payload->>'SourceUserId' is null;
                """);
        }
    }
}
