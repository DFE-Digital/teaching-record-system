using Microsoft.EntityFrameworkCore.Migrations;
using TeachingRecordSystem.Core.Services.DqtReporting;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class InductionStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "induction_statuses",
                columns: table => new
                {
                    induction_status = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_induction_statuses", x => x.induction_status);
                });

            migrationBuilder.Sql($"ALTER PUBLICATION {DqtReportingService.TrsDbPublicationName} ADD TABLE induction_statuses;");

            migrationBuilder.InsertData(
                table: "induction_statuses",
                columns: new[] { "induction_status", "name" },
                values: new object[,]
                {
                    { 0, "none" },
                    { 1, "required to complete" },
                    { 2, "exempt" },
                    { 3, "in progress" },
                    { 4, "passed" },
                    { 5, "failed" },
                    { 6, "failed in Wales" }
                });

            migrationBuilder.AddForeignKey(
                name: "fk_persons_induction_statuses_induction_status",
                table: "persons",
                column: "induction_status",
                principalTable: "induction_statuses",
                principalColumn: "induction_status",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"ALTER PUBLICATION {DqtReportingService.TrsDbPublicationName} DROP TABLE induction_statuses;");

            migrationBuilder.DropForeignKey(
                name: "fk_persons_induction_statuses_induction_status",
                table: "persons");

            migrationBuilder.DropTable(
                name: "induction_statuses");
        }
    }
}
