using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RouteToProfessionalStatusTypeFieldRequirementChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("10078157-e8c3-42f7-a050-d8b802e83f7b"),
                column: "name",
                value: "HEI");

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("3604ef30-8f11-4494-8b52-a2f9c5371e03"),
                column: "name",
                value: "Northern Irish Recognition");

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("4163c2fb-6163-409f-85fd-56e7c70a54dd"),
                column: "name",
                value: "Core");

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("52835b1f-1f2e-4665-abc6-7fb1ef0a80bb"),
                column: "name",
                value: "Scottish Recognition");

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("57b86cef-98e2-4962-a74a-d47c7a34b838"),
                columns: new[] { "name", "training_end_date_required", "training_start_date_required" },
                values: new object[] { "Assessment Only", 0, 0 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("6987240e-966e-485f-b300-23b54937fb3a"),
                columns: new[] { "name", "training_end_date_required", "training_start_date_required" },
                values: new object[] { "Postgraduate Teaching Apprenticeship", 0, 0 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("6f27bdeb-d00a-4ef9-b0ea-26498ce64713"),
                column: "name",
                value: "Apply for Qualified Teacher Status in England");

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("877ba701-fe26-4951-9f15-171f3755d50d"),
                column: "name",
                value: "Welsh Recognition");

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("be6eaf8c-92dd-4eff-aad3-1c89c4bec18c"),
                column: "training_provider_required",
                value: 0);

            migrationBuilder.InsertData(
                table: "route_to_professional_status_types",
                columns: new[] { "route_to_professional_status_type_id", "degree_type_required", "holds_from_required", "induction_exemption_reason_id", "induction_exemption_required", "is_active", "name", "professional_status_type", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[,]
                {
                    { new Guid("11b66de5-4670-4c82-86aa-20e42df723b7"), 1, 1, null, 2, true, "Early Years Teacher Degree Apprenticeship", 1, 1, 1, 0, 0, 0, 1 },
                    { new Guid("5d4c01c1-0841-4306-b49c-48ad6499fdc0"), 1, 1, null, 2, true, "Teacher Degree Apprenticeship", 0, 1, 1, 0, 0, 0, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("11b66de5-4670-4c82-86aa-20e42df723b7"));

            migrationBuilder.DeleteData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("5d4c01c1-0841-4306-b49c-48ad6499fdc0"));

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("10078157-e8c3-42f7-a050-d8b802e83f7b"),
                column: "name",
                value: "HEI - HEI Programme Type");

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("3604ef30-8f11-4494-8b52-a2f9c5371e03"),
                column: "name",
                value: "NI R");

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("4163c2fb-6163-409f-85fd-56e7c70a54dd"),
                column: "name",
                value: "Core - Core Programme Type");

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("52835b1f-1f2e-4665-abc6-7fb1ef0a80bb"),
                column: "name",
                value: "Scotland R");

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("57b86cef-98e2-4962-a74a-d47c7a34b838"),
                columns: new[] { "name", "training_end_date_required", "training_start_date_required" },
                values: new object[] { "Assessment Only Route", 1, 1 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("6987240e-966e-485f-b300-23b54937fb3a"),
                columns: new[] { "name", "training_end_date_required", "training_start_date_required" },
                values: new object[] { "Apprenticeship", 1, 1 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("6f27bdeb-d00a-4ef9-b0ea-26498ce64713"),
                column: "name",
                value: "Apply for QTS");

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("877ba701-fe26-4951-9f15-171f3755d50d"),
                column: "name",
                value: "Welsh R");

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("be6eaf8c-92dd-4eff-aad3-1c89c4bec18c"),
                column: "training_provider_required",
                value: 1);
        }
    }
}
