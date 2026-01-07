using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ChangeInductionExemptionsWording : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("0997ab13-7412-4560-8191-e51ed4d58d2a"),
                column: "name",
                value: "They qualified through a further education route between 1 September 2001 and 1 September 2004");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("15014084-2d8d-4f51-9198-b0e1881f8896"),
                column: "name",
                value: "They qualified between 7 May 1999 and 1 April 2003 and first taught in Wales for at least 2 terms");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("204f86eb-0383-40eb-b793-6fccb76ecee2"),
                column: "name",
                value: "Exempt due to data loss or system error");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("243b21a8-0be4-4af5-8874-85944357e7f8"),
                column: "name",
                value: "They passed induction in Jersey");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("3471ab35-e6e4-4fa9-a72b-b8bd113df591"),
                column: "name",
                value: "They passed induction in Northern Ireland");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("39550fa9-3147-489d-b808-4feea7f7f979"),
                column: "name",
                value: "They passed induction in Wales");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("42bb7bbc-a92c-4886-b319-3c1a5eac319a"),
                column: "name",
                value: "They’re a registered teacher with at least 2 years’ full-time teaching experience");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("5a80cee8-98a8-426b-8422-b0e81cb49b36"),
                column: "name",
                value: "They qualified before 07 May 2000");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("7d17d904-c1c6-451b-9e09-031314bd35f7"),
                column: "name",
                value: "They passed induction in Service Children’s Education schools in Germany or Cyprus");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("a112e691-1694-46a7-8f33-5ec5b845c181"),
                column: "name",
                value: "They have or are eligible for full registration in Scotland");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("a751494a-7e7a-4836-96cb-00b9ed6e1b5f"),
                column: "name",
                value: "They passed their probationary period in Gibraltar");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("e5c3847d-8fb6-4b31-8726-812392da8c5c"),
                column: "name",
                value: "They passed induction in the Isle of Man");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("e7118bab-c2b1-4fe8-ad3f-4095d73f5b85"),
                column: "name",
                value: "They qualified through a European Economic Area (EEA) mutual recognition route");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("fea2db23-93e0-49af-96fd-83c815c17c0b"),
                column: "name",
                value: "They passed induction in Guernsey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("0997ab13-7412-4560-8191-e51ed4d58d2a"),
                column: "name",
                value: "Qualified through Further Education route between 1 Sep 2001 and 1 Sep 2004");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("15014084-2d8d-4f51-9198-b0e1881f8896"),
                column: "name",
                value: "Qualified between 07 May 1999 and 01 Apr 2003. First post was in Wales and lasted a minimum of two terms.");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("204f86eb-0383-40eb-b793-6fccb76ecee2"),
                column: "name",
                value: "Exempt - Data Loss/Error Criteria");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("243b21a8-0be4-4af5-8874-85944357e7f8"),
                column: "name",
                value: "Passed induction in Jersey");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("3471ab35-e6e4-4fa9-a72b-b8bd113df591"),
                column: "name",
                value: "Passed induction in Northern Ireland");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("39550fa9-3147-489d-b808-4feea7f7f979"),
                column: "name",
                value: "Passed induction in Wales");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("42bb7bbc-a92c-4886-b319-3c1a5eac319a"),
                column: "name",
                value: "Registered teacher with at least 2 years full-time teaching experience");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("5a80cee8-98a8-426b-8422-b0e81cb49b36"),
                column: "name",
                value: "Qualified before 07 May 2000");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("7d17d904-c1c6-451b-9e09-031314bd35f7"),
                column: "name",
                value: "Passed induction in Service Children's Education schools in Germany or Cyprus");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("a112e691-1694-46a7-8f33-5ec5b845c181"),
                column: "name",
                value: "Has, or is eligible for, full registration in Scotland");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("a751494a-7e7a-4836-96cb-00b9ed6e1b5f"),
                column: "name",
                value: "Passed probationary period in Gibraltar");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("e5c3847d-8fb6-4b31-8726-812392da8c5c"),
                column: "name",
                value: "Passed induction in Isle of Man");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("e7118bab-c2b1-4fe8-ad3f-4095d73f5b85"),
                column: "name",
                value: "Qualified through EEA mutual recognition route");

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("fea2db23-93e0-49af-96fd-83c815c17c0b"),
                column: "name",
                value: "Passed induction in Guernsey");
        }
    }
}
