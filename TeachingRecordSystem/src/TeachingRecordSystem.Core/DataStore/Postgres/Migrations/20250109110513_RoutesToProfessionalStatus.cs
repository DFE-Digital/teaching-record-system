using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RoutesToProfessionalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "routes_to_professional_status",
                columns: table => new
                {
                    route_to_professional_status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    qualification_type = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_routes_to_professional_status", x => x.route_to_professional_status_id);
                });

            migrationBuilder.InsertData(
                table: "routes_to_professional_status",
                columns: new[] { "route_to_professional_status_id", "is_active", "name", "qualification_type" },
                values: new object[,]
                {
                    { new Guid("002f7c96-f6ae-4e67-8f8b-d2f1c1317273"), true, "ProfGCE ITT", 1 },
                    { new Guid("02a2135c-ac34-4481-a293-8a00aab7ee69"), true, "PGCE ITT", 1 },
                    { new Guid("10078157-e8c3-42f7-a050-d8b802e83f7b"), true, "HEI - HEI Programme Type", 1 },
                    { new Guid("12a742c3-1cd4-43b7-a2fa-1000bd4cc373"), true, "School Direct Training Programme Salaried", 1 },
                    { new Guid("1c626be0-5a64-47ec-8349-75008f52bc2c"), true, "PGATD ITT", 1 },
                    { new Guid("20f67e38-f117-4b42-bbfc-5812aa717b94"), true, "Undergraduate Opt In", 1 },
                    { new Guid("2b106b9d-ba39-4e2d-a42e-0ce827fdc324"), true, "European Recognition", 1 },
                    { new Guid("2b4862ca-bd30-4a3a-bfce-52b57c2946c7"), true, "Licensed Teacher Programme", 1 },
                    { new Guid("32017d68-9da4-43b2-ae91-4f24c68f6f78"), true, "HEI - Historic", 1 },
                    { new Guid("321d5f9a-9581-4936-9f63-cfddd2a95fe2"), true, "Primary and secondary undergraduate fee funded", 1 },
                    { new Guid("34222549-ed59-4c4a-811d-c0894e78d4c3"), true, "Graduate Teacher Programme", 1 },
                    { new Guid("3604ef30-8f11-4494-8b52-a2f9c5371e03"), true, "NI R", 1 },
                    { new Guid("4163c2fb-6163-409f-85fd-56e7c70a54dd"), true, "Core - Core Programme Type", 1 },
                    { new Guid("4477e45d-c531-4c63-9f4b-e157766366fb"), true, "Early Years ITT Graduate Employment Based", 2 },
                    { new Guid("4514ec65-20b0-4465-b66f-4718963c5b80"), true, "Legacy ITT", 1 },
                    { new Guid("45c93b5b-b4dc-4d0f-b0de-d612521e0a13"), true, "FE Recognition 2000-2004", 1 },
                    { new Guid("4b6fc697-be67-43d3-9021-cc662c4a559f"), true, "Authorised Teacher Programme", 1 },
                    { new Guid("4bd7a9f0-28ca-4977-a044-a7b7828d469b"), true, "Core Flexible", 1 },
                    { new Guid("50d18f17-ee26-4dad-86ca-1aae3f956bfc"), true, "Troops to Teach", 1 },
                    { new Guid("51756384-cfea-4f63-80e5-f193686e0f71"), true, "Overseas Trained Teacher Programme", 1 },
                    { new Guid("52835b1f-1f2e-4665-abc6-7fb1ef0a80bb"), true, "Scotland R", 1 },
                    { new Guid("53a7fbda-25fd-4482-9881-5cf65053888d"), true, "Provider led Undergrad", 1 },
                    { new Guid("5748d41d-7b53-4ee6-833a-83080a3bd8ef"), true, "CTC or CCTA", 1 },
                    { new Guid("57b86cef-98e2-4962-a74a-d47c7a34b838"), true, "Assessment Only Route", 1 },
                    { new Guid("5b7d1c4e-fb2b-479c-bdee-5818daaa8a07"), true, "EYTS ITT Migrated", 2 },
                    { new Guid("5b7f5e90-1ca6-4529-baa0-dfba68e698b8"), true, "Teach First Programme", 1 },
                    { new Guid("64c28594-4b63-42b3-8b47-e3f140879e66"), true, "Licensed Teacher Programme - Independent School", 1 },
                    { new Guid("6987240e-966e-485f-b300-23b54937fb3a"), true, "Apprenticeship", 1 },
                    { new Guid("6f27bdeb-d00a-4ef9-b0ea-26498ce64713"), true, "Apply for QTS", 1 },
                    { new Guid("700ec96f-6bbf-4080-87bd-94ef65a6a879"), true, "Flexible ITT", 1 },
                    { new Guid("70368ff2-8d2b-467e-ad23-efe7f79995d7"), true, "Registered Teacher Programme", 1 },
                    { new Guid("7721655f-165f-4737-97d4-17fc6991c18c"), true, "PGDE ITT", 1 },
                    { new Guid("779bd3c6-6b3a-4204-9489-1bbb381b52bf"), true, "Licensed Teacher Programme - OTT", 1 },
                    { new Guid("7c04865f-fa39-458a-bc39-07dd46b88154"), true, "UGMT ITT", 1 },
                    { new Guid("7f09002c-5dad-4839-9693-5e030d037ae9"), true, "Early Years ITT School Direct", 2 },
                    { new Guid("82aa14d3-ef6a-4b46-a10c-dc850ddcef5f"), true, "TCMH", 1 },
                    { new Guid("877ba701-fe26-4951-9f15-171f3755d50d"), true, "Welsh R", 1 },
                    { new Guid("88867b43-897b-49b5-97cc-f4f81a1d5d44"), true, "Other Qualifications non ITT", 1 },
                    { new Guid("8f5c0431-d006-4eda-9336-16dfc6a26a78"), true, "EYPS", 3 },
                    { new Guid("97497716-5ac5-49aa-a444-27fa3e2c152a"), true, "Provider led Postgrad", 1 },
                    { new Guid("97e1811b-d46c-483e-aec3-4a2dd51a55fe"), true, "School Direct Training Programme Self Funded", 1 },
                    { new Guid("9a6f368f-06e7-4a74-b269-6886c48a49da"), true, "ProfGDE ITT", 1 },
                    { new Guid("a6431d4b-e4cd-4e59-886b-358221237e75"), true, "Graduate non-trained", 1 },
                    { new Guid("aa1efd16-d59c-4e18-a496-16e39609b389"), true, "Long Service", 1 },
                    { new Guid("abcb0a14-0c21-4598-a42c-a007d4b048ac"), true, "School Centered ITT", 1 },
                    { new Guid("be6eaf8c-92dd-4eff-aad3-1c89c4bec18c"), true, "QTLS and SET Membership", 1 },
                    { new Guid("bed14b00-5d08-4580-83b5-86d71a4f1a24"), true, "TC ITT", 1 },
                    { new Guid("bfef20b2-5ac4-486d-9493-e5a4538e1be9"), true, "High Potential ITT", 1 },
                    { new Guid("c80cb763-0d61-4cf1-a749-37c1d0ab85f8"), true, "Legacy Migration", 1 },
                    { new Guid("c97c0fd2-fd84-4949-97c7-b0e2422fb3c8"), true, "Early Years ITT Undergraduate", 2 },
                    { new Guid("ce61056e-e681-471e-af48-5ffbf2653500"), true, "Overseas Trained Teacher Recognition", 1 },
                    { new Guid("d0b60864-ab1c-4d49-a5c2-ff4bd9872ee1"), true, "International Qualified Teacher Status", 1 },
                    { new Guid("d5eb09cc-c64f-45df-a46d-08277a25de7a"), true, "Licensed Teacher Programme - FE", 1 },
                    { new Guid("d9490e58-acdc-4a38-b13e-5a5c21417737"), true, "School Direct Training Programme", 1 },
                    { new Guid("d9eef3f8-fde6-4a3f-a361-f6655a42fa1e"), true, "Early Years ITT Assessment Only", 2 },
                    { new Guid("dbc4125b-9235-41e4-abd2-baabbf63f829"), true, "Early Years ITT Graduate Entry", 2 },
                    { new Guid("e5c198fa-35f0-4a13-9d07-8b0239b4957a"), true, "Licensed Teacher Programme - Maintained School", 1 },
                    { new Guid("eba0b7ae-cbce-44d5-a56f-988d69b03001"), true, "EYPS ITT Migrated", 3 },
                    { new Guid("ec95c276-25d9-491f-99a2-8d92f10e1e94"), true, "European Recognition - PQTS", 4 },
                    { new Guid("ef46ff51-8dc0-481e-b158-61ccea9943d9"), true, "Primary and secondary postgraduate fee funded", 1 },
                    { new Guid("f4da123b-5c37-4060-ab00-52de4bd3599e"), true, "EC directive", 1 },
                    { new Guid("f5390be5-8336-4951-b97b-5b45d00b7a76"), true, "PGATC ITT", 1 },
                    { new Guid("f85962c9-cf0c-415d-9de5-a397f95ae261"), true, "Future Teaching Scholars", 1 },
                    { new Guid("fc16290c-ac1e-4830-b7e9-35708f1bded3"), true, "Licensed Teacher Programme - Armed Forces", 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "routes_to_professional_status");
        }
    }
}
