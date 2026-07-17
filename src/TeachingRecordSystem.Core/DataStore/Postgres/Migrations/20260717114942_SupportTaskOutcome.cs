using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class SupportTaskOutcome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "outcome_label",
                table: "support_tasks");

            migrationBuilder.AddColumn<int>(
                name: "outcome",
                table: "support_tasks",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "outcome",
                table: "support_tasks");

            migrationBuilder.AddColumn<string>(
                name: "outcome_label",
                table: "support_tasks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
