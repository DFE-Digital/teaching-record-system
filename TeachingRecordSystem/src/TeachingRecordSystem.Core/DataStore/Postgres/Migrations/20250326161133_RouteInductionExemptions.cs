using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RouteInductionExemptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_qualifications_induction_exemption_reasons_induction_exempt",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "induction_exemption_reason_id",
                table: "qualifications");

            migrationBuilder.AddColumn<Guid>(
                name: "induction_exemption_reason_id",
                table: "routes_to_professional_status",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "exempt_from_induction",
                table: "qualifications",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "route_implicit_exemption",
                table: "induction_exemption_reasons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("0997ab13-7412-4560-8191-e51ed4d58d2a"),
                column: "route_implicit_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("15014084-2d8d-4f51-9198-b0e1881f8896"),
                column: "route_implicit_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("204f86eb-0383-40eb-b793-6fccb76ecee2"),
                column: "route_implicit_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("243b21a8-0be4-4af5-8874-85944357e7f8"),
                column: "route_implicit_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("3471ab35-e6e4-4fa9-a72b-b8bd113df591"),
                column: "route_implicit_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("35caa6a3-49f2-4a63-bd5a-2ba5fa9dc5db"),
                column: "route_implicit_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("39550fa9-3147-489d-b808-4feea7f7f979"),
                column: "route_implicit_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("42bb7bbc-a92c-4886-b319-3c1a5eac319a"),
                column: "route_implicit_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("4c97e211-10d2-4c63-8da9-b0fcebe7f2f9"),
                column: "route_implicit_exemption",
                value: true);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("5a80cee8-98a8-426b-8422-b0e81cb49b36"),
                column: "route_implicit_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("7d17d904-c1c6-451b-9e09-031314bd35f7"),
                column: "route_implicit_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("a112e691-1694-46a7-8f33-5ec5b845c181"),
                column: "route_implicit_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("a5faff9f-29ce-4a6b-a7b8-0c1f57f15920"),
                column: "route_implicit_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("a751494a-7e7a-4836-96cb-00b9ed6e1b5f"),
                column: "route_implicit_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("e5c3847d-8fb6-4b31-8726-812392da8c5c"),
                column: "route_implicit_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("e7118bab-c2b1-4fe8-ad3f-4095d73f5b85"),
                column: "route_implicit_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("fea2db23-93e0-49af-96fd-83c815c17c0b"),
                column: "route_implicit_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("002f7c96-f6ae-4e67-8f8b-d2f1c1317273"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("02a2135c-ac34-4481-a293-8a00aab7ee69"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("10078157-e8c3-42f7-a050-d8b802e83f7b"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("12a742c3-1cd4-43b7-a2fa-1000bd4cc373"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("1c626be0-5a64-47ec-8349-75008f52bc2c"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("20f67e38-f117-4b42-bbfc-5812aa717b94"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("2b106b9d-ba39-4e2d-a42e-0ce827fdc324"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("2b4862ca-bd30-4a3a-bfce-52b57c2946c7"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("32017d68-9da4-43b2-ae91-4f24c68f6f78"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("321d5f9a-9581-4936-9f63-cfddd2a95fe2"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("34222549-ed59-4c4a-811d-c0894e78d4c3"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("3604ef30-8f11-4494-8b52-a2f9c5371e03"),
                column: "induction_exemption_reason_id",
                value: new Guid("3471ab35-e6e4-4fa9-a72b-b8bd113df591"));

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("4163c2fb-6163-409f-85fd-56e7c70a54dd"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("4477e45d-c531-4c63-9f4b-e157766366fb"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("4514ec65-20b0-4465-b66f-4718963c5b80"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("45c93b5b-b4dc-4d0f-b0de-d612521e0a13"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("4b6fc697-be67-43d3-9021-cc662c4a559f"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("4bd7a9f0-28ca-4977-a044-a7b7828d469b"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("50d18f17-ee26-4dad-86ca-1aae3f956bfc"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("51756384-cfea-4f63-80e5-f193686e0f71"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("52835b1f-1f2e-4665-abc6-7fb1ef0a80bb"),
                column: "induction_exemption_reason_id",
                value: new Guid("a112e691-1694-46a7-8f33-5ec5b845c181"));

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("53a7fbda-25fd-4482-9881-5cf65053888d"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("5748d41d-7b53-4ee6-833a-83080a3bd8ef"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("57b86cef-98e2-4962-a74a-d47c7a34b838"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("5b7d1c4e-fb2b-479c-bdee-5818daaa8a07"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("5b7f5e90-1ca6-4529-baa0-dfba68e698b8"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("64c28594-4b63-42b3-8b47-e3f140879e66"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("6987240e-966e-485f-b300-23b54937fb3a"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("6f27bdeb-d00a-4ef9-b0ea-26498ce64713"),
                column: "induction_exemption_reason_id",
                value: new Guid("4c97e211-10d2-4c63-8da9-b0fcebe7f2f9"));

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("700ec96f-6bbf-4080-87bd-94ef65a6a879"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("70368ff2-8d2b-467e-ad23-efe7f79995d7"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("7721655f-165f-4737-97d4-17fc6991c18c"),
                columns: new[] { "induction_exemption_reason_id", "induction_exemption_required" },
                values: new object[] { null, 2 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("779bd3c6-6b3a-4204-9489-1bbb381b52bf"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("7c04865f-fa39-458a-bc39-07dd46b88154"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("7f09002c-5dad-4839-9693-5e030d037ae9"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("82aa14d3-ef6a-4b46-a10c-dc850ddcef5f"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("877ba701-fe26-4951-9f15-171f3755d50d"),
                columns: new[] { "induction_exemption_reason_id", "induction_exemption_required" },
                values: new object[] { null, 2 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("88867b43-897b-49b5-97cc-f4f81a1d5d44"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("8f5c0431-d006-4eda-9336-16dfc6a26a78"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("97497716-5ac5-49aa-a444-27fa3e2c152a"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("97e1811b-d46c-483e-aec3-4a2dd51a55fe"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("9a6f368f-06e7-4a74-b269-6886c48a49da"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("a6431d4b-e4cd-4e59-886b-358221237e75"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("aa1efd16-d59c-4e18-a496-16e39609b389"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("abcb0a14-0c21-4598-a42c-a007d4b048ac"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("be6eaf8c-92dd-4eff-aad3-1c89c4bec18c"),
                columns: new[] { "induction_exemption_reason_id", "induction_exemption_required" },
                values: new object[] { new Guid("35caa6a3-49f2-4a63-bd5a-2ba5fa9dc5db"), 1 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("bed14b00-5d08-4580-83b5-86d71a4f1a24"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("bfef20b2-5ac4-486d-9493-e5a4538e1be9"),
                columns: new[] { "induction_exemption_reason_id", "induction_exemption_required" },
                values: new object[] { null, 2 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("c80cb763-0d61-4cf1-a749-37c1d0ab85f8"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("c97c0fd2-fd84-4949-97c7-b0e2422fb3c8"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("ce61056e-e681-471e-af48-5ffbf2653500"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("d0b60864-ab1c-4d49-a5c2-ff4bd9872ee1"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("d5eb09cc-c64f-45df-a46d-08277a25de7a"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("d9490e58-acdc-4a38-b13e-5a5c21417737"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("d9eef3f8-fde6-4a3f-a361-f6655a42fa1e"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("dbc4125b-9235-41e4-abd2-baabbf63f829"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("e5c198fa-35f0-4a13-9d07-8b0239b4957a"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("eba0b7ae-cbce-44d5-a56f-988d69b03001"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("ec95c276-25d9-491f-99a2-8d92f10e1e94"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("ef46ff51-8dc0-481e-b158-61ccea9943d9"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("f4da123b-5c37-4060-ab00-52de4bd3599e"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("f5390be5-8336-4951-b97b-5b45d00b7a76"),
                column: "induction_exemption_reason_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("f85962c9-cf0c-415d-9de5-a397f95ae261"),
                columns: new[] { "induction_exemption_reason_id", "induction_exemption_required" },
                values: new object[] { null, 2 });

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("fc16290c-ac1e-4830-b7e9-35708f1bded3"),
                column: "induction_exemption_reason_id",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "induction_exemption_reason_id",
                table: "routes_to_professional_status");

            migrationBuilder.DropColumn(
                name: "exempt_from_induction",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "route_implicit_exemption",
                table: "induction_exemption_reasons");

            migrationBuilder.AddColumn<Guid>(
                name: "induction_exemption_reason_id",
                table: "qualifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("7721655f-165f-4737-97d4-17fc6991c18c"),
                column: "induction_exemption_required",
                value: 1);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("877ba701-fe26-4951-9f15-171f3755d50d"),
                column: "induction_exemption_required",
                value: 1);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("be6eaf8c-92dd-4eff-aad3-1c89c4bec18c"),
                column: "induction_exemption_required",
                value: 2);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("bfef20b2-5ac4-486d-9493-e5a4538e1be9"),
                column: "induction_exemption_required",
                value: 1);

            migrationBuilder.UpdateData(
                table: "routes_to_professional_status",
                keyColumn: "route_to_professional_status_id",
                keyValue: new Guid("f85962c9-cf0c-415d-9de5-a397f95ae261"),
                column: "induction_exemption_required",
                value: 1);

            migrationBuilder.AddForeignKey(
                name: "fk_qualifications_induction_exemption_reasons_induction_exempt",
                table: "qualifications",
                column: "induction_exemption_reason_id",
                principalTable: "induction_exemption_reasons",
                principalColumn: "induction_exemption_reason_id");
        }
    }
}
