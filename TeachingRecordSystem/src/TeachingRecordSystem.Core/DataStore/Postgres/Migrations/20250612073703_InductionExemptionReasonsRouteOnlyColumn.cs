using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class InductionExemptionReasonsRouteOnlyColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "route_only_exemption",
                table: "induction_exemption_reasons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("0997ab13-7412-4560-8191-e51ed4d58d2a"),
                column: "route_only_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("15014084-2d8d-4f51-9198-b0e1881f8896"),
                column: "route_only_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("204f86eb-0383-40eb-b793-6fccb76ecee2"),
                column: "route_only_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("243b21a8-0be4-4af5-8874-85944357e7f8"),
                column: "route_only_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("3471ab35-e6e4-4fa9-a72b-b8bd113df591"),
                column: "route_only_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("35caa6a3-49f2-4a63-bd5a-2ba5fa9dc5db"),
                column: "route_only_exemption",
                value: true);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("39550fa9-3147-489d-b808-4feea7f7f979"),
                column: "route_only_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("42bb7bbc-a92c-4886-b319-3c1a5eac319a"),
                column: "route_only_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("4c97e211-10d2-4c63-8da9-b0fcebe7f2f9"),
                column: "route_only_exemption",
                value: true);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("5a80cee8-98a8-426b-8422-b0e81cb49b36"),
                column: "route_only_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("7d17d904-c1c6-451b-9e09-031314bd35f7"),
                column: "route_only_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("a112e691-1694-46a7-8f33-5ec5b845c181"),
                column: "route_only_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("a5faff9f-29ce-4a6b-a7b8-0c1f57f15920"),
                column: "route_only_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("a751494a-7e7a-4836-96cb-00b9ed6e1b5f"),
                column: "route_only_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("e5c3847d-8fb6-4b31-8726-812392da8c5c"),
                column: "route_only_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("e7118bab-c2b1-4fe8-ad3f-4095d73f5b85"),
                column: "route_only_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("fea2db23-93e0-49af-96fd-83c815c17c0b"),
                column: "route_only_exemption",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "route_only_exemption",
                table: "induction_exemption_reasons");
        }
    }
}
