using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class TrnRequestPreviousNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "previous_first_name",
                table: "trn_request_metadata",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "previous_last_name",
                table: "trn_request_metadata",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "previous_first_name",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "previous_last_name",
                table: "trn_request_metadata");
        }
    }
}
