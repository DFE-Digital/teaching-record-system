using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class FilterDeletedTeamNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_teams_name",
                table: "teams");

            migrationBuilder.CreateIndex(
                name: "ix_teams_name",
                table: "teams",
                column: "name",
                unique: true,
                filter: "deleted_on is null");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_teams_name",
                table: "teams");

            migrationBuilder.CreateIndex(
                name: "ix_teams_name",
                table: "teams",
                column: "name",
                unique: true);
        }
    }
}
