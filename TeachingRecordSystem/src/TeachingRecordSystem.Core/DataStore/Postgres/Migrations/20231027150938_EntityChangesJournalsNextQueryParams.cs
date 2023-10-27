using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class EntityChangesJournalsNextQueryParams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "data_token",
                table: "entity_changes_journals",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "next_query_page_number",
                table: "entity_changes_journals",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "next_query_page_size",
                table: "entity_changes_journals",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "next_query_paging_cookie",
                table: "entity_changes_journals",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "next_query_page_number",
                table: "entity_changes_journals");

            migrationBuilder.DropColumn(
                name: "next_query_page_size",
                table: "entity_changes_journals");

            migrationBuilder.DropColumn(
                name: "next_query_paging_cookie",
                table: "entity_changes_journals");

            migrationBuilder.AlterColumn<string>(
                name: "data_token",
                table: "entity_changes_journals",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
