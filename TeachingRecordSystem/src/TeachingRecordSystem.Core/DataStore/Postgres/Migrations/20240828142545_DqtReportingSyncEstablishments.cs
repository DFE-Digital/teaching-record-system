using Microsoft.EntityFrameworkCore.Migrations;
using TeachingRecordSystem.Core.Services.DqtReporting;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class DqtReportingSyncEstablishments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"ALTER PUBLICATION {DqtReportingService.TrsDbPublicationName} SET TABLE qualifications, tps_establishments, tps_employments, establishments;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"ALTER PUBLICATION {DqtReportingService.TrsDbPublicationName} SET TABLE qualifications, tps_establishments, tps_employments;");
        }
    }
}
