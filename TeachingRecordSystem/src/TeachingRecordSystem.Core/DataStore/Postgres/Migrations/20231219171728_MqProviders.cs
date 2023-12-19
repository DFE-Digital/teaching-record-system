using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class MqProviders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "mq_provider_id",
                table: "qualifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "mandatory_qualification_providers",
                columns: table => new
                {
                    mandatory_qualification_provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mandatory_qualification_providers", x => x.mandatory_qualification_provider_id);
                });

            migrationBuilder.InsertData(
                table: "mandatory_qualification_providers",
                columns: new[] { "mandatory_qualification_provider_id", "name" },
                values: new object[,]
                {
                    { new Guid("0c30f666-647c-4ea8-8883-0fc6010b56be"), "University of Oxford/Oxford Polytechnic" },
                    { new Guid("26204149-349c-4ad6-9466-bb9b83723eae"), "Liverpool John Moores University" },
                    { new Guid("374dceb8-8224-45b8-b7dc-a6b0282b1065"), "Bristol Polytechnic" },
                    { new Guid("3fc648a7-18e4-49e7-8a4b-1612616b72d5"), "University of London" },
                    { new Guid("707d58ca-1953-413b-9a46-41e9b0be885e"), "University of Hertfordshire" },
                    { new Guid("89f9a1aa-3d68-4985-a4ce-403b6044c18c"), "University of Leeds" },
                    { new Guid("aa5c300e-3b7c-456c-8183-3520b3d55dca"), "University of Manchester" },
                    { new Guid("aec32252-ef25-452e-a358-34a04e03369c"), "University of Newcastle-upon-Tyne" },
                    { new Guid("d0e6d54c-5e90-438a-945d-f97388c2b352"), "University of Cambridge" },
                    { new Guid("d4fc958b-21de-47ec-9f03-36ae237a1b11"), "University College, Swansea" },
                    { new Guid("d9ee7054-7fde-4cfd-9a5e-4b99511d1b3d"), "University of Plymouth" },
                    { new Guid("e28ea41d-408d-4c89-90cc-8b9b04ac68f5"), "University of Birmingham" },
                    { new Guid("f417e73e-e2ad-40eb-85e3-55865be7f6be"), "Mary Hare School / University of Hertfordshire" },
                    { new Guid("fbf22e04-b274-4c80-aba8-79fb6a7a32ce"), "University of Edinburgh" }
                });

            migrationBuilder.AddForeignKey(
                name: "fk_qualifications_mandatory_qualification_provider",
                table: "qualifications",
                column: "mq_provider_id",
                principalTable: "mandatory_qualification_providers",
                principalColumn: "mandatory_qualification_provider_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_qualifications_mandatory_qualification_provider",
                table: "qualifications");

            migrationBuilder.DropTable(
                name: "mandatory_qualification_providers");

            migrationBuilder.DropColumn(
                name: "mq_provider_id",
                table: "qualifications");
        }
    }
}
