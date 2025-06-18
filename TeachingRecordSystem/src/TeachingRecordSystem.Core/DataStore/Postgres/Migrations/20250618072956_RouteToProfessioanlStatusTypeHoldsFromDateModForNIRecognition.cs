using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RouteToProfessioanlStatusTypeHoldsFromDateModForNIRecognition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("3604ef30-8f11-4494-8b52-a2f9c5371e03"),
                column: "holds_from_required",
                value: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("3604ef30-8f11-4494-8b52-a2f9c5371e03"),
                column: "holds_from_required",
                value: 2);
        }
    }
}
