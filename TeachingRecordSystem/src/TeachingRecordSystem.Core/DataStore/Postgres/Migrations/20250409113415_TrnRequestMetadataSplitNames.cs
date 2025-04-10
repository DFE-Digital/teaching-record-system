using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class TrnRequestMetadataSplitNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "first_name",
                table: "trn_request_metadata",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_name",
                table: "trn_request_metadata",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "middle_name",
                table: "trn_request_metadata",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "first_name",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "last_name",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "middle_name",
                table: "trn_request_metadata");
        }
    }
}
