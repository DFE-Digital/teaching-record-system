using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AlertReferenceDataSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "dqt_sanction_code",
                table: "alert_types",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true,
                collation: "case_insensitive");

            migrationBuilder.AddColumn<int>(
                name: "prohibition_level",
                table: "alert_types",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "alert_categories",
                columns: new[] { "alert_category_id", "name" },
                values: new object[,]
                {
                    { new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), "GTC Decision" },
                    { new Guid("0ae0707b-1503-477d-bc0f-1505ed95dbdf"), "Failed induction" },
                    { new Guid("227b75e5-bb98-496c-8860-1baea37aa5c6"), "TRA Decision (SoS)" },
                    { new Guid("38df5a00-94ab-486f-8905-d5b2eac04000"), "Section 128 (SoS)" },
                    { new Guid("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"), "GTC Prohibition from teaching" },
                    { new Guid("768c9eb4-355b-4491-bb20-67eb59a97579"), "Flags" },
                    { new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), "GTC Restriction" },
                    { new Guid("b2b19019-b165-47a3-8745-3297ff152581"), "Prohibition from teaching" },
                    { new Guid("cbf7633f-3904-407d-8371-42a473fa641f"), "Restriction" },
                    { new Guid("e8a9ee91-bf7f-4f70-bc66-a644d522384e"), "DBS" },
                    { new Guid("ee78d44d-abf8-44a9-b22b-87a821f8d3c9"), "EEA Decision" },
                    { new Guid("ff18c0a8-aaea-4c8b-93a2-2206beea1d7a"), "Not true alert" }
                });

            migrationBuilder.InsertData(
                table: "alert_types",
                columns: new[] { "alert_type_id", "alert_category_id", "dqt_sanction_code", "name", "prohibition_level" },
                values: new object[,]
                {
                    { new Guid("0740f9eb-ece3-4394-a230-453da224d337"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), "A16", "No Sanction - conviction for a relevant offence", 0 },
                    { new Guid("0ae8d4b6-ec9b-47ca-9338-6dae9192afe5"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), "A4", "Reprimand - unacceptable professional conduct", 0 },
                    { new Guid("17b4fe26-7468-4702-92e5-785b861cf0fa"), new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), "A24", "Suspension order - with conditions - (arising from breach of previous condition(s))", 2 },
                    { new Guid("18e04dcb-fb86-4b05-8d5d-ff9c5da738dd"), new Guid("cbf7633f-3904-407d-8371-42a473fa641f"), "B2A", "Restricted by the Secretary of State - Permitted to work as teacher", 1 },
                    { new Guid("1a2b06ae-7e9f-4761-b95d-397ca5da4b13"), new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), "A13", "Suspension order - unacceptable professional conduct - with conditions", 2 },
                    { new Guid("1ebd1620-293d-4169-ba78-0b41a6413ad9"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), "A7", "Conditional Registration Order - serious professional incompetence", 2 },
                    { new Guid("241eeb78-fac7-4c77-8059-c12e93dc2fae"), new Guid("38df5a00-94ab-486f-8905-d5b2eac04000"), "T7", "Section 128 barring direction", 4 },
                    { new Guid("2c496e3f-00d3-4f0d-81f3-21458fe707b3"), new Guid("b2b19019-b165-47a3-8745-3297ff152581"), "G2", "Formerly barred by the Independent Safeguarding Authority", 1 },
                    { new Guid("2ca98658-1d5b-49d5-b05f-cc08c8b8502c"), new Guid("ee78d44d-abf8-44a9-b22b-87a821f8d3c9"), "T8", "Teacher sanctioned in other EEA member state", 3 },
                    { new Guid("33e00e46-6513-4136-adfd-1352cf34d8ec"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), "A22", "No Sanction - breach of condition(s)", 0 },
                    { new Guid("3499860a-a0fb-43e3-878e-c226d14150b0"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), "A3", "Conditional Registration Order - unacceptable professional conduct", 2 },
                    { new Guid("38db7946-2dbf-408e-bc48-1625829e7dfe"), new Guid("cbf7633f-3904-407d-8371-42a473fa641f"), "B2B", "Restricted by the Secretary of State - Not Permitted to work as teacher", 1 },
                    { new Guid("3c5fc83b-10e1-4a15-83e6-794fce3e0b45"), new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), "A23", "Suspension order - without conditions - (arising from breach of previous condition(s))", 2 },
                    { new Guid("3f7de5fd-05a8-404f-a97c-428f54e81322"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), "A8", "Reprimand - serious professional incompetence", 0 },
                    { new Guid("40794ea8-eda2-40a8-a26a-5f447aae6c99"), new Guid("e8a9ee91-bf7f-4f70-bc66-a644d522384e"), "G1", "A possible matching record was found. Please contact the DBS before employing this person", 1 },
                    { new Guid("50508749-7a6b-4175-8538-9a1e55692efd"), new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), "A14", "Suspension order - serious professional incompetence - with conditions", 2 },
                    { new Guid("50feafbc-5124-4189-b06c-6463c7ebb8a8"), new Guid("b2b19019-b165-47a3-8745-3297ff152581"), "T3", "Prohibition by the Secretary of State - deregistered by GTC Scotland", 1 },
                    { new Guid("552ee226-a3a9-4dc3-8d04-0b7e4f641b51"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), "A15", "For internal information only - historic GTC finding of unsuitable for registration", 0 },
                    { new Guid("5aa21b8f-2069-43c9-8afd-05b34b02505f"), new Guid("b2b19019-b165-47a3-8745-3297ff152581"), "T5", "Prohibition by the Secretary of State - refer to GTC Northern Ireland", 1 },
                    { new Guid("5ea8bb68-4774-4ad8-b635-213a0cdda4c3"), new Guid("0ae0707b-1503-477d-bc0f-1505ed95dbdf"), "C3", "Restricted by the Secretary of State - failed probation - permitted to carry out specified work for a period equal in length to a statutory induction period only", 1 },
                    { new Guid("62715a16-69f8-44f7-90f4-df83cd0c9f16"), new Guid("ff18c0a8-aaea-4c8b-93a2-2206beea1d7a"), "B4", "Employers to contact the Secretary of State", 0 },
                    { new Guid("651e1f56-3135-4961-bd7e-3f7b2c75cb04"), new Guid("0ae0707b-1503-477d-bc0f-1505ed95dbdf"), "C1", "Prohibited by the Secretary of State - failed probation", 1 },
                    { new Guid("72e48b6a-e781-4bf3-910b-91f2d28f2eaa"), new Guid("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"), "A21B", "Prohibition Order - conviction of a relevant offence - eligible to reapply after specified time", 1 },
                    { new Guid("78f88de2-9ec1-41b8-948a-33bdff223206"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), "A11", "No Sanction - unacceptable professional conduct", 0 },
                    { new Guid("7924fe90-483c-49f8-84fc-674feddba848"), new Guid("227b75e5-bb98-496c-8860-1baea37aa5c6"), "T6", "Secretary of State decision- no prohibition", 0 },
                    { new Guid("872d7700-aa6f-435e-b5f9-821fb087962a"), new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), "A2", "Suspension order - unacceptable professional conduct - without conditions", 2 },
                    { new Guid("8ef92c14-4b1f-4530-9189-779ad9f3cefd"), new Guid("b2b19019-b165-47a3-8745-3297ff152581"), "B3", "Prohibited by an Independent Schools Tribunal or Secretary of State", 1 },
                    { new Guid("950d3eed-bef5-448a-b0f0-bf9c54f2103b"), new Guid("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"), "A21A", "Prohibition Order - conviction of a relevant offence - ineligible to reapply", 1 },
                    { new Guid("993daa42-96cb-4621-bd9e-d4b195076bbe"), new Guid("b2b19019-b165-47a3-8745-3297ff152581"), "B6", "Formerly on List 99", 1 },
                    { new Guid("9fafaa80-f9f8-44a0-b7b3-cffedcbe0298"), new Guid("0ae0707b-1503-477d-bc0f-1505ed95dbdf"), "C2", "Failed induction", 1 },
                    { new Guid("a414283f-7d5b-4587-83bf-f6da8c05b8d5"), new Guid("b2b19019-b165-47a3-8745-3297ff152581"), "T2", "Interim prohibition by the Secretary of State", 1 },
                    { new Guid("a5bd4352-2cec-4417-87a1-4b6b79d033c2"), new Guid("b2b19019-b165-47a3-8745-3297ff152581"), "T4", "Prohibition by the Secretary of State - refer to the Education Workforce Council, Wales", 1 },
                    { new Guid("a6f51ccc-a19c-4dc2-ba80-ffb7a95ff2ee"), new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), "A6", "Suspension order - serious professional incompetence - without conditions", 2 },
                    { new Guid("a6fc9f2e-8923-4163-978e-93bd901d146f"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), "A18", "Conditional Registration Order - conviction of a relevant offence", 2 },
                    { new Guid("ae3e385d-03f8-4f12-9ce2-006afe827d23"), new Guid("768c9eb4-355b-4491-bb20-67eb59a97579"), "T9", "FOR INTERNAL INFORMATION ONLY - see alert details", 0 },
                    { new Guid("af65c236-47a6-427b-8e4b-930de6d256f0"), new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), "A19", "Suspension order - conviction of a relevant offence - without conditions", 2 },
                    { new Guid("b6c8d8f1-723e-49a5-9551-25805e3e29b9"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), "A12", "No Sanction - serious professional incompetence", 0 },
                    { new Guid("c02bdc3a-7a19-4034-aa23-3a23c54e1d34"), new Guid("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"), "A5A", "Prohibition Order - serious professional incompetence - Ineligible to reapply", 1 },
                    { new Guid("cac68337-3f95-4475-97cf-1381e6b74700"), new Guid("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"), "A5B", "Prohibition Order - serious professional incompetence - Eligible to reapply after specified time", 1 },
                    { new Guid("d372fcfa-1c4a-4fed-84c8-4c7885575681"), new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), "A20", "Suspension order - conviction of a relevant offence - with conditions", 2 },
                    { new Guid("e3658a61-bee2-4df1-9a26-e010681ee310"), new Guid("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"), "A1B", "Prohibition Order - unacceptable professional conduct - Eligible to reapply after specified time", 1 },
                    { new Guid("eab8b66d-68d0-4cb9-8e4d-bbd245648fb6"), new Guid("b2b19019-b165-47a3-8745-3297ff152581"), "B1", "Barring by the Secretary of State", 1 },
                    { new Guid("ed0cd700-3fb2-4db0-9403-ba57126090ed"), new Guid("b2b19019-b165-47a3-8745-3297ff152581"), "T1", "Prohibition by the Secretary of State - misconduct", 1 },
                    { new Guid("fa6bd220-61b0-41fc-9066-421b3b9d7885"), new Guid("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"), "A1A", "Prohibition Order - unacceptable professional conduct - Ineligible to reapply", 1 },
                    { new Guid("fcff87d6-88f5-4fc5-ac81-5350b4fdd9e1"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), "A17", "Reprimand - conviction of a relevant offence", 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("0740f9eb-ece3-4394-a230-453da224d337"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("0ae8d4b6-ec9b-47ca-9338-6dae9192afe5"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("17b4fe26-7468-4702-92e5-785b861cf0fa"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("18e04dcb-fb86-4b05-8d5d-ff9c5da738dd"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("1a2b06ae-7e9f-4761-b95d-397ca5da4b13"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("1ebd1620-293d-4169-ba78-0b41a6413ad9"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("241eeb78-fac7-4c77-8059-c12e93dc2fae"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("2c496e3f-00d3-4f0d-81f3-21458fe707b3"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("2ca98658-1d5b-49d5-b05f-cc08c8b8502c"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("33e00e46-6513-4136-adfd-1352cf34d8ec"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("3499860a-a0fb-43e3-878e-c226d14150b0"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("38db7946-2dbf-408e-bc48-1625829e7dfe"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("3c5fc83b-10e1-4a15-83e6-794fce3e0b45"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("3f7de5fd-05a8-404f-a97c-428f54e81322"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("40794ea8-eda2-40a8-a26a-5f447aae6c99"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("50508749-7a6b-4175-8538-9a1e55692efd"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("50feafbc-5124-4189-b06c-6463c7ebb8a8"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("552ee226-a3a9-4dc3-8d04-0b7e4f641b51"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("5aa21b8f-2069-43c9-8afd-05b34b02505f"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("5ea8bb68-4774-4ad8-b635-213a0cdda4c3"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("62715a16-69f8-44f7-90f4-df83cd0c9f16"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("651e1f56-3135-4961-bd7e-3f7b2c75cb04"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("72e48b6a-e781-4bf3-910b-91f2d28f2eaa"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("78f88de2-9ec1-41b8-948a-33bdff223206"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("7924fe90-483c-49f8-84fc-674feddba848"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("872d7700-aa6f-435e-b5f9-821fb087962a"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("8ef92c14-4b1f-4530-9189-779ad9f3cefd"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("950d3eed-bef5-448a-b0f0-bf9c54f2103b"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("993daa42-96cb-4621-bd9e-d4b195076bbe"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("9fafaa80-f9f8-44a0-b7b3-cffedcbe0298"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("a414283f-7d5b-4587-83bf-f6da8c05b8d5"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("a5bd4352-2cec-4417-87a1-4b6b79d033c2"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("a6f51ccc-a19c-4dc2-ba80-ffb7a95ff2ee"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("a6fc9f2e-8923-4163-978e-93bd901d146f"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("ae3e385d-03f8-4f12-9ce2-006afe827d23"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("af65c236-47a6-427b-8e4b-930de6d256f0"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("b6c8d8f1-723e-49a5-9551-25805e3e29b9"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("c02bdc3a-7a19-4034-aa23-3a23c54e1d34"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("cac68337-3f95-4475-97cf-1381e6b74700"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("d372fcfa-1c4a-4fed-84c8-4c7885575681"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("e3658a61-bee2-4df1-9a26-e010681ee310"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("eab8b66d-68d0-4cb9-8e4d-bbd245648fb6"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("ed0cd700-3fb2-4db0-9403-ba57126090ed"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("fa6bd220-61b0-41fc-9066-421b3b9d7885"));

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("fcff87d6-88f5-4fc5-ac81-5350b4fdd9e1"));

            migrationBuilder.DeleteData(
                table: "alert_categories",
                keyColumn: "alert_category_id",
                keyValue: new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"));

            migrationBuilder.DeleteData(
                table: "alert_categories",
                keyColumn: "alert_category_id",
                keyValue: new Guid("0ae0707b-1503-477d-bc0f-1505ed95dbdf"));

            migrationBuilder.DeleteData(
                table: "alert_categories",
                keyColumn: "alert_category_id",
                keyValue: new Guid("227b75e5-bb98-496c-8860-1baea37aa5c6"));

            migrationBuilder.DeleteData(
                table: "alert_categories",
                keyColumn: "alert_category_id",
                keyValue: new Guid("38df5a00-94ab-486f-8905-d5b2eac04000"));

            migrationBuilder.DeleteData(
                table: "alert_categories",
                keyColumn: "alert_category_id",
                keyValue: new Guid("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"));

            migrationBuilder.DeleteData(
                table: "alert_categories",
                keyColumn: "alert_category_id",
                keyValue: new Guid("768c9eb4-355b-4491-bb20-67eb59a97579"));

            migrationBuilder.DeleteData(
                table: "alert_categories",
                keyColumn: "alert_category_id",
                keyValue: new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"));

            migrationBuilder.DeleteData(
                table: "alert_categories",
                keyColumn: "alert_category_id",
                keyValue: new Guid("b2b19019-b165-47a3-8745-3297ff152581"));

            migrationBuilder.DeleteData(
                table: "alert_categories",
                keyColumn: "alert_category_id",
                keyValue: new Guid("cbf7633f-3904-407d-8371-42a473fa641f"));

            migrationBuilder.DeleteData(
                table: "alert_categories",
                keyColumn: "alert_category_id",
                keyValue: new Guid("e8a9ee91-bf7f-4f70-bc66-a644d522384e"));

            migrationBuilder.DeleteData(
                table: "alert_categories",
                keyColumn: "alert_category_id",
                keyValue: new Guid("ee78d44d-abf8-44a9-b22b-87a821f8d3c9"));

            migrationBuilder.DeleteData(
                table: "alert_categories",
                keyColumn: "alert_category_id",
                keyValue: new Guid("ff18c0a8-aaea-4c8b-93a2-2206beea1d7a"));

            migrationBuilder.DropColumn(
                name: "dqt_sanction_code",
                table: "alert_types");

            migrationBuilder.DropColumn(
                name: "prohibition_level",
                table: "alert_types");
        }
    }
}
