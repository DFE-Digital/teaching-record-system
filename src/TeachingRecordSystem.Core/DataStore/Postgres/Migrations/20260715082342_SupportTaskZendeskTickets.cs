using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class SupportTaskZendeskTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "zendesk_tickets",
                table: "support_tasks",
                type: "varchar[]",
                nullable: false,
                defaultValueSql: "'{}'::varchar[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "zendesk_tickets",
                table: "support_tasks");
        }
    }
}
