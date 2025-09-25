using System;
using Microsoft.EntityFrameworkCore.Migrations;
using TeachingRecordSystem.Core.Services.DqtReporting;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class SupportTaskTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "support_task_type_id",
                table: "support_tasks");

            migrationBuilder.CreateTable(
                name: "support_task_types",
                columns: table => new
                {
                    support_task_type = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_support_task_types", x => x.support_task_type);
                });

            migrationBuilder.Sql($"ALTER PUBLICATION {DqtReportingService.TrsDbPublicationName} ADD TABLE support_task_types;");

            migrationBuilder.InsertData(
                table: "support_task_types",
                columns: new[] { "support_task_type", "name" },
                values: new object[,]
                {
                    { 1, "connect GOV.UK One Login user to a teaching record" },
                    { 2, "change name request" },
                    { 3, "change date of birth request" },
                    { 4, "TRN request from API" },
                    { 5, "TRN request from NPQ" },
                    { 6, "manual checks needed" },
                    { 7, "teacher pensions potential duplicate" }
                });

            migrationBuilder.AddForeignKey(
                name: "fk_support_tasks_support_task_types_support_task_type",
                table: "support_tasks",
                column: "support_task_type",
                principalTable: "support_task_types",
                principalColumn: "support_task_type",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_support_tasks_support_task_types_support_task_type",
                table: "support_tasks");

            migrationBuilder.Sql($"ALTER PUBLICATION {DqtReportingService.TrsDbPublicationName} DROP TABLE support_task_types;");

            migrationBuilder.DropTable(
                name: "support_task_types");

            migrationBuilder.AddColumn<Guid>(
                name: "support_task_type_id",
                table: "support_tasks",
                type: "uuid",
                nullable: true);
        }
    }
}
