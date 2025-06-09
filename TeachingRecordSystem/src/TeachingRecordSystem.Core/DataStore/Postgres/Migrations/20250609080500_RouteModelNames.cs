using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RouteModelNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_qualifications_routes_to_professional_status_route_to_profe",
                table: "qualifications");

            migrationBuilder.DropTable(
                name: "routes_to_professional_status");

            migrationBuilder.RenameColumn(
                name: "route_to_professional_status_id",
                table: "qualifications",
                newName: "route_to_professional_status_type_id");

            migrationBuilder.CreateTable(
                name: "route_to_professional_status_types",
                columns: table => new
                {
                    route_to_professional_status_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    professional_status_type = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    training_start_date_required = table.Column<int>(type: "integer", nullable: false),
                    training_end_date_required = table.Column<int>(type: "integer", nullable: false),
                    award_date_required = table.Column<int>(type: "integer", nullable: false),
                    induction_exemption_required = table.Column<int>(type: "integer", nullable: false),
                    training_provider_required = table.Column<int>(type: "integer", nullable: false),
                    degree_type_required = table.Column<int>(type: "integer", nullable: false),
                    training_country_required = table.Column<int>(type: "integer", nullable: false),
                    training_age_specialism_type_required = table.Column<int>(type: "integer", nullable: false),
                    training_subjects_required = table.Column<int>(type: "integer", nullable: false),
                    induction_exemption_reason_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_route_to_professional_status_types", x => x.route_to_professional_status_type_id);
                    table.ForeignKey(
                        name: "fk_route_to_professional_status_induction_exemption_reason",
                        column: x => x.induction_exemption_reason_id,
                        principalTable: "induction_exemption_reasons",
                        principalColumn: "induction_exemption_reason_id");
                });

            migrationBuilder.InsertData(
                table: "route_to_professional_status_types",
                columns: new[] { "route_to_professional_status_type_id", "award_date_required", "degree_type_required", "induction_exemption_reason_id", "induction_exemption_required", "is_active", "name", "professional_status_type", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[,]
                {
                    { new Guid("002f7c96-f6ae-4e67-8f8b-d2f1c1317273"), 1, 0, null, 2, true, "ProfGCE ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("02a2135c-ac34-4481-a293-8a00aab7ee69"), 1, 0, null, 2, true, "PGCE ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("10078157-e8c3-42f7-a050-d8b802e83f7b"), 1, 1, null, 2, true, "HEI - HEI Programme Type", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("12a742c3-1cd4-43b7-a2fa-1000bd4cc373"), 1, 1, null, 2, true, "School Direct Training Programme Salaried", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("1c626be0-5a64-47ec-8349-75008f52bc2c"), 1, 0, null, 2, true, "PGATD ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("20f67e38-f117-4b42-bbfc-5812aa717b94"), 1, 1, null, 2, true, "Undergraduate Opt In", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("2b106b9d-ba39-4e2d-a42e-0ce827fdc324"), 1, 0, null, 0, true, "European Recognition", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("2b4862ca-bd30-4a3a-bfce-52b57c2946c7"), 1, 0, null, 2, true, "Licensed Teacher Programme", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("32017d68-9da4-43b2-ae91-4f24c68f6f78"), 1, 0, null, 2, true, "HEI - Historic", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("321d5f9a-9581-4936-9f63-cfddd2a95fe2"), 1, 1, null, 2, true, "Primary and secondary undergraduate fee funded", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("34222549-ed59-4c4a-811d-c0894e78d4c3"), 1, 0, null, 0, true, "Graduate Teacher Programme", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("3604ef30-8f11-4494-8b52-a2f9c5371e03"), 1, 2, new Guid("3471ab35-e6e4-4fa9-a72b-b8bd113df591"), 1, true, "NI R", 0, 0, 1, 2, 2, 2, 0 },
                    { new Guid("4163c2fb-6163-409f-85fd-56e7c70a54dd"), 1, 0, null, 2, true, "Core - Core Programme Type", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("4477e45d-c531-4c63-9f4b-e157766366fb"), 1, 1, null, 2, true, "Early Years ITT Graduate Employment Based", 1, 0, 1, 1, 1, 1, 0 },
                    { new Guid("4514ec65-20b0-4465-b66f-4718963c5b80"), 1, 0, null, 2, true, "Legacy ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("45c93b5b-b4dc-4d0f-b0de-d612521e0a13"), 1, 0, null, 0, true, "FE Recognition 2000-2004", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("4b6fc697-be67-43d3-9021-cc662c4a559f"), 1, 0, null, 2, true, "Authorised Teacher Programme", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("4bd7a9f0-28ca-4977-a044-a7b7828d469b"), 1, 0, null, 2, true, "Core Flexible", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("50d18f17-ee26-4dad-86ca-1aae3f956bfc"), 1, 1, null, 2, true, "Troops to Teach", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("51756384-cfea-4f63-80e5-f193686e0f71"), 1, 0, null, 0, true, "Overseas Trained Teacher Programme", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("52835b1f-1f2e-4665-abc6-7fb1ef0a80bb"), 1, 2, new Guid("a112e691-1694-46a7-8f33-5ec5b845c181"), 1, true, "Scotland R", 0, 0, 1, 2, 2, 2, 0 },
                    { new Guid("53a7fbda-25fd-4482-9881-5cf65053888d"), 1, 1, null, 2, true, "Provider led Undergrad", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("5748d41d-7b53-4ee6-833a-83080a3bd8ef"), 1, 0, null, 2, true, "CTC or CCTA", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("57b86cef-98e2-4962-a74a-d47c7a34b838"), 1, 1, null, 2, true, "Assessment Only Route", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("5b7d1c4e-fb2b-479c-bdee-5818daaa8a07"), 1, 0, null, 2, true, "EYTS ITT Migrated", 2, 0, 0, 0, 0, 0, 0 },
                    { new Guid("5b7f5e90-1ca6-4529-baa0-dfba68e698b8"), 1, 1, null, 2, true, "Teach First Programme", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("64c28594-4b63-42b3-8b47-e3f140879e66"), 1, 0, null, 2, true, "Licensed Teacher Programme - Independent School", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("6987240e-966e-485f-b300-23b54937fb3a"), 1, 1, null, 2, true, "Apprenticeship", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("6f27bdeb-d00a-4ef9-b0ea-26498ce64713"), 1, 2, new Guid("4c97e211-10d2-4c63-8da9-b0fcebe7f2f9"), 1, true, "Apply for QTS", 0, 0, 1, 2, 2, 2, 0 },
                    { new Guid("700ec96f-6bbf-4080-87bd-94ef65a6a879"), 1, 1, null, 2, true, "Flexible ITT", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("70368ff2-8d2b-467e-ad23-efe7f79995d7"), 1, 0, null, 2, true, "Registered Teacher Programme", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("7721655f-165f-4737-97d4-17fc6991c18c"), 1, 1, null, 2, true, "PGDE ITT", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("779bd3c6-6b3a-4204-9489-1bbb381b52bf"), 1, 0, null, 2, true, "Licensed Teacher Programme - OTT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("7c04865f-fa39-458a-bc39-07dd46b88154"), 1, 0, null, 2, true, "UGMT ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("7f09002c-5dad-4839-9693-5e030d037ae9"), 1, 1, null, 2, true, "Early Years ITT School Direct", 1, 0, 0, 1, 1, 1, 0 },
                    { new Guid("82aa14d3-ef6a-4b46-a10c-dc850ddcef5f"), 1, 0, null, 2, true, "TCMH", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("877ba701-fe26-4951-9f15-171f3755d50d"), 1, 2, null, 2, true, "Welsh R", 0, 0, 0, 2, 2, 2, 0 },
                    { new Guid("88867b43-897b-49b5-97cc-f4f81a1d5d44"), 1, 0, null, 0, true, "Other Qualifications non ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("8f5c0431-d006-4eda-9336-16dfc6a26a78"), 1, 0, null, 0, true, "EYPS", 2, 0, 0, 0, 0, 0, 0 },
                    { new Guid("97497716-5ac5-49aa-a444-27fa3e2c152a"), 1, 1, null, 2, true, "Provider led Postgrad", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("97e1811b-d46c-483e-aec3-4a2dd51a55fe"), 1, 1, null, 2, true, "School Direct Training Programme Self Funded", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("9a6f368f-06e7-4a74-b269-6886c48a49da"), 1, 0, null, 2, true, "ProfGDE ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("a6431d4b-e4cd-4e59-886b-358221237e75"), 1, 0, null, 2, true, "Graduate non-trained", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("aa1efd16-d59c-4e18-a496-16e39609b389"), 1, 0, null, 2, true, "Long Service", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("abcb0a14-0c21-4598-a42c-a007d4b048ac"), 1, 0, null, 2, true, "School Centered ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("be6eaf8c-92dd-4eff-aad3-1c89c4bec18c"), 1, 2, new Guid("35caa6a3-49f2-4a63-bd5a-2ba5fa9dc5db"), 1, true, "QTLS and SET Membership", 0, 0, 0, 2, 1, 2, 0 },
                    { new Guid("bed14b00-5d08-4580-83b5-86d71a4f1a24"), 1, 0, null, 2, true, "TC ITT", 0, 0, 1, 0, 0, 0, 0 },
                    { new Guid("bfef20b2-5ac4-486d-9493-e5a4538e1be9"), 1, 1, null, 2, true, "High Potential ITT", 0, 0, 1, 1, 1, 1, 0 },
                    { new Guid("c80cb763-0d61-4cf1-a749-37c1d0ab85f8"), 1, 0, null, 2, true, "Legacy Migration", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("c97c0fd2-fd84-4949-97c7-b0e2422fb3c8"), 1, 1, null, 2, true, "Early Years ITT Undergraduate", 1, 0, 0, 1, 1, 1, 0 },
                    { new Guid("ce61056e-e681-471e-af48-5ffbf2653500"), 1, 0, null, 0, true, "Overseas Trained Teacher Recognition", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("d0b60864-ab1c-4d49-a5c2-ff4bd9872ee1"), 1, 1, null, 2, true, "International Qualified Teacher Status", 0, 0, 1, 1, 1, 1, 0 },
                    { new Guid("d5eb09cc-c64f-45df-a46d-08277a25de7a"), 1, 0, null, 2, true, "Licensed Teacher Programme - FE", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("d9490e58-acdc-4a38-b13e-5a5c21417737"), 1, 1, null, 2, true, "School Direct Training Programme", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("d9eef3f8-fde6-4a3f-a361-f6655a42fa1e"), 1, 1, null, 2, true, "Early Years ITT Assessment Only", 1, 0, 0, 1, 1, 1, 0 },
                    { new Guid("dbc4125b-9235-41e4-abd2-baabbf63f829"), 1, 1, null, 2, true, "Early Years ITT Graduate Entry", 1, 0, 1, 1, 1, 1, 0 },
                    { new Guid("e5c198fa-35f0-4a13-9d07-8b0239b4957a"), 1, 0, null, 2, true, "Licensed Teacher Programme - Maintained School", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("eba0b7ae-cbce-44d5-a56f-988d69b03001"), 1, 0, null, 2, true, "EYPS ITT Migrated", 2, 0, 0, 0, 0, 0, 0 },
                    { new Guid("ec95c276-25d9-491f-99a2-8d92f10e1e94"), 1, 0, null, 2, true, "European Recognition - PQTS", 3, 0, 0, 0, 0, 0, 0 },
                    { new Guid("ef46ff51-8dc0-481e-b158-61ccea9943d9"), 1, 1, null, 2, true, "Primary and secondary postgraduate fee funded", 0, 0, 2, 1, 1, 1, 0 },
                    { new Guid("f4da123b-5c37-4060-ab00-52de4bd3599e"), 1, 0, null, 0, true, "EC directive", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("f5390be5-8336-4951-b97b-5b45d00b7a76"), 1, 0, null, 2, true, "PGATC ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("f85962c9-cf0c-415d-9de5-a397f95ae261"), 1, 1, null, 2, true, "Future Teaching Scholars", 0, 0, 1, 1, 1, 1, 0 },
                    { new Guid("fc16290c-ac1e-4830-b7e9-35708f1bded3"), 1, 0, null, 2, true, "Licensed Teacher Programme - Armed Forces", 0, 0, 0, 0, 0, 0, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_route_to_professional_status_induction_exemption_reason_id",
                table: "route_to_professional_status_types",
                column: "induction_exemption_reason_id");

            migrationBuilder.AddForeignKey(
                name: "fk_qualifications_route_to_professional_status_types_route_to_",
                table: "qualifications",
                column: "route_to_professional_status_type_id",
                principalTable: "route_to_professional_status_types",
                principalColumn: "route_to_professional_status_type_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_qualifications_route_to_professional_status_types_route_to_",
                table: "qualifications");

            migrationBuilder.DropTable(
                name: "route_to_professional_status_types");

            migrationBuilder.RenameColumn(
                name: "route_to_professional_status_type_id",
                table: "qualifications",
                newName: "route_to_professional_status_id");

            migrationBuilder.CreateTable(
                name: "routes_to_professional_status",
                columns: table => new
                {
                    route_to_professional_status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    induction_exemption_reason_id = table.Column<Guid>(type: "uuid", nullable: true),
                    award_date_required = table.Column<int>(type: "integer", nullable: false),
                    degree_type_required = table.Column<int>(type: "integer", nullable: false),
                    induction_exemption_required = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    professional_status_type = table.Column<int>(type: "integer", nullable: false),
                    training_age_specialism_type_required = table.Column<int>(type: "integer", nullable: false),
                    training_country_required = table.Column<int>(type: "integer", nullable: false),
                    training_end_date_required = table.Column<int>(type: "integer", nullable: false),
                    training_provider_required = table.Column<int>(type: "integer", nullable: false),
                    training_start_date_required = table.Column<int>(type: "integer", nullable: false),
                    training_subjects_required = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_routes_to_professional_status", x => x.route_to_professional_status_id);
                    table.ForeignKey(
                        name: "fk_route_to_professional_status_induction_exemption_reason",
                        column: x => x.induction_exemption_reason_id,
                        principalTable: "induction_exemption_reasons",
                        principalColumn: "induction_exemption_reason_id");
                });

            migrationBuilder.InsertData(
                table: "routes_to_professional_status",
                columns: new[] { "route_to_professional_status_id", "award_date_required", "degree_type_required", "induction_exemption_reason_id", "induction_exemption_required", "is_active", "name", "professional_status_type", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[,]
                {
                    { new Guid("002f7c96-f6ae-4e67-8f8b-d2f1c1317273"), 1, 0, null, 2, true, "ProfGCE ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("02a2135c-ac34-4481-a293-8a00aab7ee69"), 1, 0, null, 2, true, "PGCE ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("10078157-e8c3-42f7-a050-d8b802e83f7b"), 1, 1, null, 2, true, "HEI - HEI Programme Type", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("12a742c3-1cd4-43b7-a2fa-1000bd4cc373"), 1, 1, null, 2, true, "School Direct Training Programme Salaried", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("1c626be0-5a64-47ec-8349-75008f52bc2c"), 1, 0, null, 2, true, "PGATD ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("20f67e38-f117-4b42-bbfc-5812aa717b94"), 1, 1, null, 2, true, "Undergraduate Opt In", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("2b106b9d-ba39-4e2d-a42e-0ce827fdc324"), 1, 0, null, 0, true, "European Recognition", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("2b4862ca-bd30-4a3a-bfce-52b57c2946c7"), 1, 0, null, 2, true, "Licensed Teacher Programme", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("32017d68-9da4-43b2-ae91-4f24c68f6f78"), 1, 0, null, 2, true, "HEI - Historic", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("321d5f9a-9581-4936-9f63-cfddd2a95fe2"), 1, 1, null, 2, true, "Primary and secondary undergraduate fee funded", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("34222549-ed59-4c4a-811d-c0894e78d4c3"), 1, 0, null, 0, true, "Graduate Teacher Programme", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("3604ef30-8f11-4494-8b52-a2f9c5371e03"), 1, 2, new Guid("3471ab35-e6e4-4fa9-a72b-b8bd113df591"), 1, true, "NI R", 0, 0, 1, 2, 2, 2, 0 },
                    { new Guid("4163c2fb-6163-409f-85fd-56e7c70a54dd"), 1, 0, null, 2, true, "Core - Core Programme Type", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("4477e45d-c531-4c63-9f4b-e157766366fb"), 1, 1, null, 2, true, "Early Years ITT Graduate Employment Based", 1, 0, 1, 1, 1, 1, 0 },
                    { new Guid("4514ec65-20b0-4465-b66f-4718963c5b80"), 1, 0, null, 2, true, "Legacy ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("45c93b5b-b4dc-4d0f-b0de-d612521e0a13"), 1, 0, null, 0, true, "FE Recognition 2000-2004", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("4b6fc697-be67-43d3-9021-cc662c4a559f"), 1, 0, null, 2, true, "Authorised Teacher Programme", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("4bd7a9f0-28ca-4977-a044-a7b7828d469b"), 1, 0, null, 2, true, "Core Flexible", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("50d18f17-ee26-4dad-86ca-1aae3f956bfc"), 1, 1, null, 2, true, "Troops to Teach", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("51756384-cfea-4f63-80e5-f193686e0f71"), 1, 0, null, 0, true, "Overseas Trained Teacher Programme", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("52835b1f-1f2e-4665-abc6-7fb1ef0a80bb"), 1, 2, new Guid("a112e691-1694-46a7-8f33-5ec5b845c181"), 1, true, "Scotland R", 0, 0, 1, 2, 2, 2, 0 },
                    { new Guid("53a7fbda-25fd-4482-9881-5cf65053888d"), 1, 1, null, 2, true, "Provider led Undergrad", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("5748d41d-7b53-4ee6-833a-83080a3bd8ef"), 1, 0, null, 2, true, "CTC or CCTA", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("57b86cef-98e2-4962-a74a-d47c7a34b838"), 1, 1, null, 2, true, "Assessment Only Route", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("5b7d1c4e-fb2b-479c-bdee-5818daaa8a07"), 1, 0, null, 2, true, "EYTS ITT Migrated", 2, 0, 0, 0, 0, 0, 0 },
                    { new Guid("5b7f5e90-1ca6-4529-baa0-dfba68e698b8"), 1, 1, null, 2, true, "Teach First Programme", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("64c28594-4b63-42b3-8b47-e3f140879e66"), 1, 0, null, 2, true, "Licensed Teacher Programme - Independent School", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("6987240e-966e-485f-b300-23b54937fb3a"), 1, 1, null, 2, true, "Apprenticeship", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("6f27bdeb-d00a-4ef9-b0ea-26498ce64713"), 1, 2, new Guid("4c97e211-10d2-4c63-8da9-b0fcebe7f2f9"), 1, true, "Apply for QTS", 0, 0, 1, 2, 2, 2, 0 },
                    { new Guid("700ec96f-6bbf-4080-87bd-94ef65a6a879"), 1, 1, null, 2, true, "Flexible ITT", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("70368ff2-8d2b-467e-ad23-efe7f79995d7"), 1, 0, null, 2, true, "Registered Teacher Programme", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("7721655f-165f-4737-97d4-17fc6991c18c"), 1, 1, null, 2, true, "PGDE ITT", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("779bd3c6-6b3a-4204-9489-1bbb381b52bf"), 1, 0, null, 2, true, "Licensed Teacher Programme - OTT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("7c04865f-fa39-458a-bc39-07dd46b88154"), 1, 0, null, 2, true, "UGMT ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("7f09002c-5dad-4839-9693-5e030d037ae9"), 1, 1, null, 2, true, "Early Years ITT School Direct", 1, 0, 0, 1, 1, 1, 0 },
                    { new Guid("82aa14d3-ef6a-4b46-a10c-dc850ddcef5f"), 1, 0, null, 2, true, "TCMH", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("877ba701-fe26-4951-9f15-171f3755d50d"), 1, 2, null, 2, true, "Welsh R", 0, 0, 0, 2, 2, 2, 0 },
                    { new Guid("88867b43-897b-49b5-97cc-f4f81a1d5d44"), 1, 0, null, 0, true, "Other Qualifications non ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("8f5c0431-d006-4eda-9336-16dfc6a26a78"), 1, 0, null, 0, true, "EYPS", 2, 0, 0, 0, 0, 0, 0 },
                    { new Guid("97497716-5ac5-49aa-a444-27fa3e2c152a"), 1, 1, null, 2, true, "Provider led Postgrad", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("97e1811b-d46c-483e-aec3-4a2dd51a55fe"), 1, 1, null, 2, true, "School Direct Training Programme Self Funded", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("9a6f368f-06e7-4a74-b269-6886c48a49da"), 1, 0, null, 2, true, "ProfGDE ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("a6431d4b-e4cd-4e59-886b-358221237e75"), 1, 0, null, 2, true, "Graduate non-trained", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("aa1efd16-d59c-4e18-a496-16e39609b389"), 1, 0, null, 2, true, "Long Service", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("abcb0a14-0c21-4598-a42c-a007d4b048ac"), 1, 0, null, 2, true, "School Centered ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("be6eaf8c-92dd-4eff-aad3-1c89c4bec18c"), 1, 2, new Guid("35caa6a3-49f2-4a63-bd5a-2ba5fa9dc5db"), 1, true, "QTLS and SET Membership", 0, 0, 0, 2, 1, 2, 0 },
                    { new Guid("bed14b00-5d08-4580-83b5-86d71a4f1a24"), 1, 0, null, 2, true, "TC ITT", 0, 0, 1, 0, 0, 0, 0 },
                    { new Guid("bfef20b2-5ac4-486d-9493-e5a4538e1be9"), 1, 1, null, 2, true, "High Potential ITT", 0, 0, 1, 1, 1, 1, 0 },
                    { new Guid("c80cb763-0d61-4cf1-a749-37c1d0ab85f8"), 1, 0, null, 2, true, "Legacy Migration", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("c97c0fd2-fd84-4949-97c7-b0e2422fb3c8"), 1, 1, null, 2, true, "Early Years ITT Undergraduate", 1, 0, 0, 1, 1, 1, 0 },
                    { new Guid("ce61056e-e681-471e-af48-5ffbf2653500"), 1, 0, null, 0, true, "Overseas Trained Teacher Recognition", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("d0b60864-ab1c-4d49-a5c2-ff4bd9872ee1"), 1, 1, null, 2, true, "International Qualified Teacher Status", 0, 0, 1, 1, 1, 1, 0 },
                    { new Guid("d5eb09cc-c64f-45df-a46d-08277a25de7a"), 1, 0, null, 2, true, "Licensed Teacher Programme - FE", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("d9490e58-acdc-4a38-b13e-5a5c21417737"), 1, 1, null, 2, true, "School Direct Training Programme", 0, 0, 0, 1, 1, 1, 0 },
                    { new Guid("d9eef3f8-fde6-4a3f-a361-f6655a42fa1e"), 1, 1, null, 2, true, "Early Years ITT Assessment Only", 1, 0, 0, 1, 1, 1, 0 },
                    { new Guid("dbc4125b-9235-41e4-abd2-baabbf63f829"), 1, 1, null, 2, true, "Early Years ITT Graduate Entry", 1, 0, 1, 1, 1, 1, 0 },
                    { new Guid("e5c198fa-35f0-4a13-9d07-8b0239b4957a"), 1, 0, null, 2, true, "Licensed Teacher Programme - Maintained School", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("eba0b7ae-cbce-44d5-a56f-988d69b03001"), 1, 0, null, 2, true, "EYPS ITT Migrated", 2, 0, 0, 0, 0, 0, 0 },
                    { new Guid("ec95c276-25d9-491f-99a2-8d92f10e1e94"), 1, 0, null, 2, true, "European Recognition - PQTS", 3, 0, 0, 0, 0, 0, 0 },
                    { new Guid("ef46ff51-8dc0-481e-b158-61ccea9943d9"), 1, 1, null, 2, true, "Primary and secondary postgraduate fee funded", 0, 0, 2, 1, 1, 1, 0 },
                    { new Guid("f4da123b-5c37-4060-ab00-52de4bd3599e"), 1, 0, null, 0, true, "EC directive", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("f5390be5-8336-4951-b97b-5b45d00b7a76"), 1, 0, null, 2, true, "PGATC ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("f85962c9-cf0c-415d-9de5-a397f95ae261"), 1, 1, null, 2, true, "Future Teaching Scholars", 0, 0, 1, 1, 1, 1, 0 },
                    { new Guid("fc16290c-ac1e-4830-b7e9-35708f1bded3"), 1, 0, null, 2, true, "Licensed Teacher Programme - Armed Forces", 0, 0, 0, 0, 0, 0, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_route_to_professional_status_induction_exemption_reason_id",
                table: "routes_to_professional_status",
                column: "induction_exemption_reason_id");

            migrationBuilder.AddForeignKey(
                name: "fk_qualifications_routes_to_professional_status_route_to_profe",
                table: "qualifications",
                column: "route_to_professional_status_id",
                principalTable: "routes_to_professional_status",
                principalColumn: "route_to_professional_status_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
