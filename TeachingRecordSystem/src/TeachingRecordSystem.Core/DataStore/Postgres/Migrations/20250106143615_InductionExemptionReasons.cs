using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class InductionExemptionReasons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "induction_exemption_reasons",
                table: "persons");

            migrationBuilder.AddColumn<Guid[]>(
                name: "induction_exemption_reason_ids",
                table: "persons",
                type: "uuid[]",
                nullable: false,
                defaultValue: new Guid[0]);

            migrationBuilder.CreateTable(
                name: "induction_exemption_reasons",
                columns: table => new
                {
                    induction_exemption_reason_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_induction_exemption_reasons", x => x.induction_exemption_reason_id);
                });

            migrationBuilder.InsertData(
                table: "induction_exemption_reasons",
                columns: new[] { "induction_exemption_reason_id", "is_active", "name" },
                values: new object[,]
                {
                    { new Guid("0997ab13-7412-4560-8191-e51ed4d58d2a"), true, "Qualified through Further Education route between 1 Sep 2001 and 1 Sep 2004" },
                    { new Guid("15014084-2d8d-4f51-9198-b0e1881f8896"), true, "Qualified between 07 May 1999 and 01 Apr 2003. First post was in Wales and lasted a minimum of two terms." },
                    { new Guid("204f86eb-0383-40eb-b793-6fccb76ecee2"), true, "Exempt - Data Loss/Error Criteria" },
                    { new Guid("243b21a8-0be4-4af5-8874-85944357e7f8"), true, "Passed induction in Jersey" },
                    { new Guid("3471ab35-e6e4-4fa9-a72b-b8bd113df591"), true, "Passed induction in Northern Ireland" },
                    { new Guid("35caa6a3-49f2-4a63-bd5a-2ba5fa9dc5db"), true, "Exempt through QTLS status provided they maintain membership of The Society of Education and Training" },
                    { new Guid("39550fa9-3147-489d-b808-4feea7f7f979"), true, "Passed induction in Wales" },
                    { new Guid("42bb7bbc-a92c-4886-b319-3c1a5eac319a"), true, "Registered teacher with at least 2 years full-time teaching experience" },
                    { new Guid("4c97e211-10d2-4c63-8da9-b0fcebe7f2f9"), true, "Overseas Trained Teacher" },
                    { new Guid("5a80cee8-98a8-426b-8422-b0e81cb49b36"), true, "Qualified before 07 May 2000" },
                    { new Guid("7d17d904-c1c6-451b-9e09-031314bd35f7"), true, "Passed induction in Service Children's Education schools in Germany or Cyprus" },
                    { new Guid("a112e691-1694-46a7-8f33-5ec5b845c181"), true, "Has, or is eligible for, full registration in Scotland" },
                    { new Guid("a5faff9f-29ce-4a6b-a7b8-0c1f57f15920"), true, "Exempt" },
                    { new Guid("a751494a-7e7a-4836-96cb-00b9ed6e1b5f"), true, "Passed probationary period in Gibraltar" },
                    { new Guid("e5c3847d-8fb6-4b31-8726-812392da8c5c"), true, "Passed induction in Isle of Man" },
                    { new Guid("e7118bab-c2b1-4fe8-ad3f-4095d73f5b85"), true, "Qualified through EEA mutual recognition route" },
                    { new Guid("fea2db23-93e0-49af-96fd-83c815c17c0b"), true, "Passed induction in Guernsey" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "induction_exemption_reasons");

            migrationBuilder.DropColumn(
                name: "induction_exemption_reason_ids",
                table: "persons");

            migrationBuilder.AddColumn<int>(
                name: "induction_exemption_reasons",
                table: "persons",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
