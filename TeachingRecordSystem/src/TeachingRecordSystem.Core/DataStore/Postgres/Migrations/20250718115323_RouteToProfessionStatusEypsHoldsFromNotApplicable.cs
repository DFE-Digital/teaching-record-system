using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RouteToProfessionStatusEypsHoldsFromNotApplicable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("5b7d1c4e-fb2b-479c-bdee-5818daaa8a07"),
                column: "professional_status_type",
                value: 1);

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("8f5c0431-d006-4eda-9336-16dfc6a26a78"),
                column: "holds_from_required",
                value: 2);

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("eba0b7ae-cbce-44d5-a56f-988d69b03001"),
                column: "holds_from_required",
                value: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("5b7d1c4e-fb2b-479c-bdee-5818daaa8a07"),
                column: "professional_status_type",
                value: 2);

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("8f5c0431-d006-4eda-9336-16dfc6a26a78"),
                column: "holds_from_required",
                value: 1);

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("eba0b7ae-cbce-44d5-a56f-988d69b03001"),
                column: "holds_from_required",
                value: 1);
        }
    }
}
