using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class DuplicateAlertType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_alert_types_display_order",
                table: "alert_types");

            migrationBuilder.AlterColumn<int>(
                name: "display_order",
                table: "alert_types",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("0740f9eb-ece3-4394-a230-453da224d337"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("0ae8d4b6-ec9b-47ca-9338-6dae9192afe5"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("17b4fe26-7468-4702-92e5-785b861cf0fa"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("18e04dcb-fb86-4b05-8d5d-ff9c5da738dd"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("1a2b06ae-7e9f-4761-b95d-397ca5da4b13"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("1ebd1620-293d-4169-ba78-0b41a6413ad9"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("241eeb78-fac7-4c77-8059-c12e93dc2fae"),
                column: "display_order",
                value: 1);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("2c496e3f-00d3-4f0d-81f3-21458fe707b3"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("33e00e46-6513-4136-adfd-1352cf34d8ec"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("3499860a-a0fb-43e3-878e-c226d14150b0"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("38db7946-2dbf-408e-bc48-1625829e7dfe"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("3c5fc83b-10e1-4a15-83e6-794fce3e0b45"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("3f7de5fd-05a8-404f-a97c-428f54e81322"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("40794ea8-eda2-40a8-a26a-5f447aae6c99"),
                column: "display_order",
                value: 1);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("50508749-7a6b-4175-8538-9a1e55692efd"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("50feafbc-5124-4189-b06c-6463c7ebb8a8"),
                column: "display_order",
                value: 3);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("552ee226-a3a9-4dc3-8d04-0b7e4f641b51"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("5aa21b8f-2069-43c9-8afd-05b34b02505f"),
                column: "display_order",
                value: 4);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("5ea8bb68-4774-4ad8-b635-213a0cdda4c3"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("651e1f56-3135-4961-bd7e-3f7b2c75cb04"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("72e48b6a-e781-4bf3-910b-91f2d28f2eaa"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("78f88de2-9ec1-41b8-948a-33bdff223206"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("7924fe90-483c-49f8-84fc-674feddba848"),
                column: "display_order",
                value: 1);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("872d7700-aa6f-435e-b5f9-821fb087962a"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("8ef92c14-4b1f-4530-9189-779ad9f3cefd"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("950d3eed-bef5-448a-b0f0-bf9c54f2103b"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("993daa42-96cb-4621-bd9e-d4b195076bbe"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("9fafaa80-f9f8-44a0-b7b3-cffedcbe0298"),
                column: "display_order",
                value: 1);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("a414283f-7d5b-4587-83bf-f6da8c05b8d5"),
                column: "display_order",
                value: 1);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("a5bd4352-2cec-4417-87a1-4b6b79d033c2"),
                column: "display_order",
                value: 5);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("a6f51ccc-a19c-4dc2-ba80-ffb7a95ff2ee"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("a6fc9f2e-8923-4163-978e-93bd901d146f"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("ae3e385d-03f8-4f12-9ce2-006afe827d23"),
                column: "display_order",
                value: 1);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("af65c236-47a6-427b-8e4b-930de6d256f0"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("b6c8d8f1-723e-49a5-9551-25805e3e29b9"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("c02bdc3a-7a19-4034-aa23-3a23c54e1d34"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("cac68337-3f95-4475-97cf-1381e6b74700"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("d372fcfa-1c4a-4fed-84c8-4c7885575681"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("e3658a61-bee2-4df1-9a26-e010681ee310"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("eab8b66d-68d0-4cb9-8e4d-bbd245648fb6"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("ed0cd700-3fb2-4db0-9403-ba57126090ed"),
                column: "display_order",
                value: 2);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("fa6bd220-61b0-41fc-9066-421b3b9d7885"),
                column: "display_order",
                value: null);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("fcff87d6-88f5-4fc5-ac81-5350b4fdd9e1"),
                column: "display_order",
                value: null);

            migrationBuilder.InsertData(
                table: "alert_types",
                columns: new[] { "alert_type_id", "alert_category_id", "display_order", "dqt_sanction_code", "internal_only", "is_active", "name", "prohibition_level" },
                values: new object[] { new Guid("12435c00-88cb-406b-b2b8-7400c1ced7b8"), new Guid("768c9eb4-355b-4491-bb20-67eb59a97579"), 2, "T10", true, true, "FOR INTERNAL USER ONLY – known duplicate record", 0 });

            migrationBuilder.CreateIndex(
                name: "ix_alert_types_display_order",
                table: "alert_types",
                columns: new[] { "alert_category_id", "display_order" },
                unique: true,
                filter: "display_order is not null and is_active = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_alert_types_display_order",
                table: "alert_types");

            migrationBuilder.DeleteData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("12435c00-88cb-406b-b2b8-7400c1ced7b8"));

            migrationBuilder.AlterColumn<int>(
                name: "display_order",
                table: "alert_types",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("0740f9eb-ece3-4394-a230-453da224d337"),
                column: "display_order",
                value: 11);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("0ae8d4b6-ec9b-47ca-9338-6dae9192afe5"),
                column: "display_order",
                value: 16);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("17b4fe26-7468-4702-92e5-785b861cf0fa"),
                column: "display_order",
                value: 29);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("18e04dcb-fb86-4b05-8d5d-ff9c5da738dd"),
                column: "display_order",
                value: 42);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("1a2b06ae-7e9f-4761-b95d-397ca5da4b13"),
                column: "display_order",
                value: 27);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("1ebd1620-293d-4169-ba78-0b41a6413ad9"),
                column: "display_order",
                value: 7);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("241eeb78-fac7-4c77-8059-c12e93dc2fae"),
                column: "display_order",
                value: 43);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("2c496e3f-00d3-4f0d-81f3-21458fe707b3"),
                column: "display_order",
                value: 32);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("33e00e46-6513-4136-adfd-1352cf34d8ec"),
                column: "display_order",
                value: 10);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("3499860a-a0fb-43e3-878e-c226d14150b0"),
                column: "display_order",
                value: 8);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("38db7946-2dbf-408e-bc48-1625829e7dfe"),
                column: "display_order",
                value: 41);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("3c5fc83b-10e1-4a15-83e6-794fce3e0b45"),
                column: "display_order",
                value: 30);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("3f7de5fd-05a8-404f-a97c-428f54e81322"),
                column: "display_order",
                value: 15);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("40794ea8-eda2-40a8-a26a-5f447aae6c99"),
                column: "display_order",
                value: 40);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("50508749-7a6b-4175-8538-9a1e55692efd"),
                column: "display_order",
                value: 25);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("50feafbc-5124-4189-b06c-6463c7ebb8a8"),
                column: "display_order",
                value: 37);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("552ee226-a3a9-4dc3-8d04-0b7e4f641b51"),
                column: "display_order",
                value: 9);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("5aa21b8f-2069-43c9-8afd-05b34b02505f"),
                column: "display_order",
                value: 38);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("5ea8bb68-4774-4ad8-b635-213a0cdda4c3"),
                column: "display_order",
                value: 4);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("651e1f56-3135-4961-bd7e-3f7b2c75cb04"),
                column: "display_order",
                value: 3);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("72e48b6a-e781-4bf3-910b-91f2d28f2eaa"),
                column: "display_order",
                value: 17);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("78f88de2-9ec1-41b8-948a-33bdff223206"),
                column: "display_order",
                value: 13);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("7924fe90-483c-49f8-84fc-674feddba848"),
                column: "display_order",
                value: 44);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("872d7700-aa6f-435e-b5f9-821fb087962a"),
                column: "display_order",
                value: 28);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("8ef92c14-4b1f-4530-9189-779ad9f3cefd"),
                column: "display_order",
                value: 36);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("950d3eed-bef5-448a-b0f0-bf9c54f2103b"),
                column: "display_order",
                value: 18);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("993daa42-96cb-4621-bd9e-d4b195076bbe"),
                column: "display_order",
                value: 33);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("9fafaa80-f9f8-44a0-b7b3-cffedcbe0298"),
                column: "display_order",
                value: 2);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("a414283f-7d5b-4587-83bf-f6da8c05b8d5"),
                column: "display_order",
                value: 34);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("a5bd4352-2cec-4417-87a1-4b6b79d033c2"),
                column: "display_order",
                value: 39);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("a6f51ccc-a19c-4dc2-ba80-ffb7a95ff2ee"),
                column: "display_order",
                value: 26);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("a6fc9f2e-8923-4163-978e-93bd901d146f"),
                column: "display_order",
                value: 6);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("ae3e385d-03f8-4f12-9ce2-006afe827d23"),
                column: "display_order",
                value: 5);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("af65c236-47a6-427b-8e4b-930de6d256f0"),
                column: "display_order",
                value: 24);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("b6c8d8f1-723e-49a5-9551-25805e3e29b9"),
                column: "display_order",
                value: 12);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("c02bdc3a-7a19-4034-aa23-3a23c54e1d34"),
                column: "display_order",
                value: 20);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("cac68337-3f95-4475-97cf-1381e6b74700"),
                column: "display_order",
                value: 19);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("d372fcfa-1c4a-4fed-84c8-4c7885575681"),
                column: "display_order",
                value: 23);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("e3658a61-bee2-4df1-9a26-e010681ee310"),
                column: "display_order",
                value: 21);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("eab8b66d-68d0-4cb9-8e4d-bbd245648fb6"),
                column: "display_order",
                value: 31);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("ed0cd700-3fb2-4db0-9403-ba57126090ed"),
                column: "display_order",
                value: 35);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("fa6bd220-61b0-41fc-9066-421b3b9d7885"),
                column: "display_order",
                value: 22);

            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("fcff87d6-88f5-4fc5-ac81-5350b4fdd9e1"),
                column: "display_order",
                value: 14);

            migrationBuilder.CreateIndex(
                name: "ix_alert_types_display_order",
                table: "alert_types",
                column: "display_order",
                unique: true);
        }
    }
}
