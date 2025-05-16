using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class FixProfessionalStatusTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("4477e45d-c531-4c63-9f4b-e157766366fb"),
                column: "professional_status_type",
                value: 1);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("5b7d1c4e-fb2b-479c-bdee-5818daaa8a07"),
                column: "professional_status_type",
                value: 2);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("7f09002c-5dad-4839-9693-5e030d037ae9"),
                column: "professional_status_type",
                value: 1);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("8f5c0431-d006-4eda-9336-16dfc6a26a78"),
                column: "professional_status_type",
                value: 2);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("c97c0fd2-fd84-4949-97c7-b0e2422fb3c8"),
                column: "professional_status_type",
                value: 1);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("d9eef3f8-fde6-4a3f-a361-f6655a42fa1e"),
                column: "professional_status_type",
                value: 1);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("dbc4125b-9235-41e4-abd2-baabbf63f829"),
                column: "professional_status_type",
                value: 1);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("eba0b7ae-cbce-44d5-a56f-988d69b03001"),
                column: "professional_status_type",
                value: 2);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("ec95c276-25d9-491f-99a2-8d92f10e1e94"),
                column: "professional_status_type",
                value: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("4477e45d-c531-4c63-9f4b-e157766366fb"),
                column: "professional_status_type",
                value: 0);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("5b7d1c4e-fb2b-479c-bdee-5818daaa8a07"),
                column: "professional_status_type",
                value: 0);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("7f09002c-5dad-4839-9693-5e030d037ae9"),
                column: "professional_status_type",
                value: 0);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("8f5c0431-d006-4eda-9336-16dfc6a26a78"),
                column: "professional_status_type",
                value: 0);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("c97c0fd2-fd84-4949-97c7-b0e2422fb3c8"),
                column: "professional_status_type",
                value: 0);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("d9eef3f8-fde6-4a3f-a361-f6655a42fa1e"),
                column: "professional_status_type",
                value: 0);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("dbc4125b-9235-41e4-abd2-baabbf63f829"),
                column: "professional_status_type",
                value: 0);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("eba0b7ae-cbce-44d5-a56f-988d69b03001"),
                column: "professional_status_type",
                value: 0);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("ec95c276-25d9-491f-99a2-8d92f10e1e94"),
                column: "professional_status_type",
                value: 0);
        }
    }
}
