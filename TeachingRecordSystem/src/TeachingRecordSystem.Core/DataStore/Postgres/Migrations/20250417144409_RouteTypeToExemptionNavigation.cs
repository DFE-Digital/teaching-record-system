using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RouteTypeToExemptionNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_route_to_professional_status_induction_exemption_reason_id",
                table: "routes_to_professional_status",
                column: "induction_exemption_reason_id");

            migrationBuilder.AddForeignKey(
                name: "fk_route_to_professional_status_induction_exemption_reason",
                table: "routes_to_professional_status",
                column: "induction_exemption_reason_id",
                principalTable: "induction_exemption_reasons",
                principalColumn: "induction_exemption_reason_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_route_to_professional_status_induction_exemption_reason",
                table: "routes_to_professional_status");

            migrationBuilder.DropIndex(
                name: "ix_route_to_professional_status_induction_exemption_reason_id",
                table: "routes_to_professional_status");
        }
    }
}
