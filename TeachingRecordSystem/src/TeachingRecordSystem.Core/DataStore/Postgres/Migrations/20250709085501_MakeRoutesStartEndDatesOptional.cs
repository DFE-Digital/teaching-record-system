using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class MakeRoutesStartEndDatesOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("10078157-e8c3-42f7-a050-d8b802e83f7b"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("12a742c3-1cd4-43b7-a2fa-1000bd4cc373"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("20f67e38-f117-4b42-bbfc-5812aa717b94"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("321d5f9a-9581-4936-9f63-cfddd2a95fe2"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("4477e45d-c531-4c63-9f4b-e157766366fb"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("53a7fbda-25fd-4482-9881-5cf65053888d"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("5b7f5e90-1ca6-4529-baa0-dfba68e698b8"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("700ec96f-6bbf-4080-87bd-94ef65a6a879"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("7721655f-165f-4737-97d4-17fc6991c18c"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("7f09002c-5dad-4839-9693-5e030d037ae9"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("97497716-5ac5-49aa-a444-27fa3e2c152a"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("97e1811b-d46c-483e-aec3-4a2dd51a55fe"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("bfef20b2-5ac4-486d-9493-e5a4538e1be9"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("c97c0fd2-fd84-4949-97c7-b0e2422fb3c8"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("d0b60864-ab1c-4d49-a5c2-ff4bd9872ee1"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("d9490e58-acdc-4a38-b13e-5a5c21417737"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("d9eef3f8-fde6-4a3f-a361-f6655a42fa1e"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("dbc4125b-9235-41e4-abd2-baabbf63f829"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("ef46ff51-8dc0-481e-b158-61ccea9943d9"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("f85962c9-cf0c-415d-9de5-a397f95ae261"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 0, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("10078157-e8c3-42f7-a050-d8b802e83f7b"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("12a742c3-1cd4-43b7-a2fa-1000bd4cc373"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("20f67e38-f117-4b42-bbfc-5812aa717b94"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("321d5f9a-9581-4936-9f63-cfddd2a95fe2"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("4477e45d-c531-4c63-9f4b-e157766366fb"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("53a7fbda-25fd-4482-9881-5cf65053888d"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("5b7f5e90-1ca6-4529-baa0-dfba68e698b8"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("700ec96f-6bbf-4080-87bd-94ef65a6a879"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("7721655f-165f-4737-97d4-17fc6991c18c"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("7f09002c-5dad-4839-9693-5e030d037ae9"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("97497716-5ac5-49aa-a444-27fa3e2c152a"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("97e1811b-d46c-483e-aec3-4a2dd51a55fe"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("bfef20b2-5ac4-486d-9493-e5a4538e1be9"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("c97c0fd2-fd84-4949-97c7-b0e2422fb3c8"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("d0b60864-ab1c-4d49-a5c2-ff4bd9872ee1"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("d9490e58-acdc-4a38-b13e-5a5c21417737"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("d9eef3f8-fde6-4a3f-a361-f6655a42fa1e"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("dbc4125b-9235-41e4-abd2-baabbf63f829"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("ef46ff51-8dc0-481e-b158-61ccea9943d9"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("f85962c9-cf0c-415d-9de5-a397f95ae261"),
                columns: new[] { "training_end_date_required", "training_start_date_required" },
                values: new object[] { 1, 1 });
        }
    }
}
