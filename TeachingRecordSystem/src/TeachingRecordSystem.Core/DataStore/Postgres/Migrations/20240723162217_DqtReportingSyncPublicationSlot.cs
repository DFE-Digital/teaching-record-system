using Microsoft.EntityFrameworkCore.Migrations;
using TeachingRecordSystem.Core.Services.DqtReporting;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class DqtReportingSyncPublicationSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"SELECT * FROM pg_create_logical_replication_slot('{DqtReportingService.TrsDbReplicationSlotName}', 'pgoutput');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"SELECT pg_drop_replication_slot('{DqtReportingService.TrsDbReplicationSlotName}');");
        }
    }
}
