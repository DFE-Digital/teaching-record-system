using Microsoft.EntityFrameworkCore.Migrations;
using TeachingRecordSystem.Core.Services.DqtReporting;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ReportingSyncIntegrationTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"ALTER PUBLICATION {DqtReportingService.TrsDbPublicationName} SET TABLE qualifications, tps_establishments, tps_employments, establishments, persons, alerts, alert_types, alert_categories, events, training_providers, notes, integration_transactions, integration_transaction_records");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"ALTER PUBLICATION {DqtReportingService.TrsDbPublicationName} SET TABLE qualifications, tps_establishments, tps_employments, establishments, persons, alerts, alert_types, alert_categories, events, notes, integration_transactions, integration_transaction_records");
        }
    }
}
