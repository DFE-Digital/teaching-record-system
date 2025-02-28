using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddRequiredColumnsRouteToProfessionalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "award_date_required",
                table: "routes_to_professional_status",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "degree_type_required",
                table: "routes_to_professional_status",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "induction_exemption_required",
                table: "routes_to_professional_status",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "training_age_specialism_type_required",
                table: "routes_to_professional_status",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "training_country_required",
                table: "routes_to_professional_status",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "training_end_date_required",
                table: "routes_to_professional_status",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "training_provider_required",
                table: "routes_to_professional_status",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "training_start_date_required",
                table: "routes_to_professional_status",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "training_subjects_required",
                table: "routes_to_professional_status",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("002f7c96-f6ae-4e67-8f8b-d2f1c1317273"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("02a2135c-ac34-4481-a293-8a00aab7ee69"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("10078157-e8c3-42f7-a050-d8b802e83f7b"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 2, 0, 0, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("12a742c3-1cd4-43b7-a2fa-1000bd4cc373"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 2, 0, 0, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("1c626be0-5a64-47ec-8349-75008f52bc2c"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("20f67e38-f117-4b42-bbfc-5812aa717b94"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 2, 0, 0, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("2b106b9d-ba39-4e2d-a42e-0ce827fdc324"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 0, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("2b4862ca-bd30-4a3a-bfce-52b57c2946c7"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("32017d68-9da4-43b2-ae91-4f24c68f6f78"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("321d5f9a-9581-4936-9f63-cfddd2a95fe2"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 2, 0, 0, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("34222549-ed59-4c4a-811d-c0894e78d4c3"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 0, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("3604ef30-8f11-4494-8b52-a2f9c5371e03"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 2, 1, 0, 1, 2, 2, 2, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("4163c2fb-6163-409f-85fd-56e7c70a54dd"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("4477e45d-c531-4c63-9f4b-e157766366fb"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "professional_status_type", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 2, 0, 0, 1, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("4514ec65-20b0-4465-b66f-4718963c5b80"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("45c93b5b-b4dc-4d0f-b0de-d612521e0a13"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 0, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("4b6fc697-be67-43d3-9021-cc662c4a559f"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("4bd7a9f0-28ca-4977-a044-a7b7828d469b"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("50d18f17-ee26-4dad-86ca-1aae3f956bfc"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("51756384-cfea-4f63-80e5-f193686e0f71"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 0, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("52835b1f-1f2e-4665-abc6-7fb1ef0a80bb"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 2, 1, 0, 1, 2, 2, 2, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("53a7fbda-25fd-4482-9881-5cf65053888d"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 2, 0, 0, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("5748d41d-7b53-4ee6-833a-83080a3bd8ef"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("57b86cef-98e2-4962-a74a-d47c7a34b838"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 2, 0, 0, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("5b7d1c4e-fb2b-479c-bdee-5818daaa8a07"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "professional_status_type", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("5b7f5e90-1ca6-4529-baa0-dfba68e698b8"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 2, 0, 0, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("64c28594-4b63-42b3-8b47-e3f140879e66"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("6987240e-966e-485f-b300-23b54937fb3a"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 2, 0, 0, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("6f27bdeb-d00a-4ef9-b0ea-26498ce64713"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 2, 1, 0, 1, 2, 2, 2, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("700ec96f-6bbf-4080-87bd-94ef65a6a879"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 2, 0, 0, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("70368ff2-8d2b-467e-ad23-efe7f79995d7"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("7721655f-165f-4737-97d4-17fc6991c18c"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 1, 0, 0, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("779bd3c6-6b3a-4204-9489-1bbb381b52bf"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("7c04865f-fa39-458a-bc39-07dd46b88154"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("7f09002c-5dad-4839-9693-5e030d037ae9"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "professional_status_type", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 2, 0, 0, 0, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("82aa14d3-ef6a-4b46-a10c-dc850ddcef5f"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("877ba701-fe26-4951-9f15-171f3755d50d"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 2, 1, 0, 0, 2, 2, 2, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("88867b43-897b-49b5-97cc-f4f81a1d5d44"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 0, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("8f5c0431-d006-4eda-9336-16dfc6a26a78"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "professional_status_type", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("97497716-5ac5-49aa-a444-27fa3e2c152a"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 2, 0, 0, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("97e1811b-d46c-483e-aec3-4a2dd51a55fe"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 2, 0, 0, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("9a6f368f-06e7-4a74-b269-6886c48a49da"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("a6431d4b-e4cd-4e59-886b-358221237e75"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("aa1efd16-d59c-4e18-a496-16e39609b389"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("abcb0a14-0c21-4598-a42c-a007d4b048ac"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("be6eaf8c-92dd-4eff-aad3-1c89c4bec18c"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 2, 2, 0, 0, 2, 1, 2, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("bed14b00-5d08-4580-83b5-86d71a4f1a24"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 1, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("bfef20b2-5ac4-486d-9493-e5a4538e1be9"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 1, 0, 1, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("c80cb763-0d61-4cf1-a749-37c1d0ab85f8"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("c97c0fd2-fd84-4949-97c7-b0e2422fb3c8"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "professional_status_type", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 2, 0, 0, 0, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("ce61056e-e681-471e-af48-5ffbf2653500"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 0, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("d0b60864-ab1c-4d49-a5c2-ff4bd9872ee1"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 2, 0, 1, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("d5eb09cc-c64f-45df-a46d-08277a25de7a"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("d9490e58-acdc-4a38-b13e-5a5c21417737"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 2, 0, 0, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("d9eef3f8-fde6-4a3f-a361-f6655a42fa1e"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "professional_status_type", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 2, 0, 0, 0, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("dbc4125b-9235-41e4-abd2-baabbf63f829"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "professional_status_type", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 2, 0, 0, 1, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("e5c198fa-35f0-4a13-9d07-8b0239b4957a"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("eba0b7ae-cbce-44d5-a56f-988d69b03001"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "professional_status_type", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("ec95c276-25d9-491f-99a2-8d92f10e1e94"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "professional_status_type", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("ef46ff51-8dc0-481e-b158-61ccea9943d9"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 2, 0, 2, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("f4da123b-5c37-4060-ab00-52de4bd3599e"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 0, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("f5390be5-8336-4951-b97b-5b45d00b7a76"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("f85962c9-cf0c-415d-9de5-a397f95ae261"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 1, 1, 0, 1, 1, 1, 1, 0 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("fc16290c-ac1e-4830-b7e9-35708f1bded3"),
                columns: new[] { "award_date_required", "degree_type_required", "induction_exemption_required", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[] { 1, 0, 2, 0, 0, 0, 0, 0, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "award_date_required",
                table: "routes_to_professional_status");

            migrationBuilder.DropColumn(
                name: "degree_type_required",
                table: "routes_to_professional_status");

            migrationBuilder.DropColumn(
                name: "induction_exemption_required",
                table: "routes_to_professional_status");

            migrationBuilder.DropColumn(
                name: "training_age_specialism_type_required",
                table: "routes_to_professional_status");

            migrationBuilder.DropColumn(
                name: "training_country_required",
                table: "routes_to_professional_status");

            migrationBuilder.DropColumn(
                name: "training_end_date_required",
                table: "routes_to_professional_status");

            migrationBuilder.DropColumn(
                name: "training_provider_required",
                table: "routes_to_professional_status");

            migrationBuilder.DropColumn(
                name: "training_start_date_required",
                table: "routes_to_professional_status");

            migrationBuilder.DropColumn(
                name: "training_subjects_required",
                table: "routes_to_professional_status");

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
                value: 1);

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
    }
}
