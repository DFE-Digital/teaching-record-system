using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class TrnRequestMetadataAdditionsForNpq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "previous_middle_name",
                table: "trn_request_metadata",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "work_email_address",
                table: "trn_request_metadata",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "previous_middle_name",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "work_email_address",
                table: "trn_request_metadata");
        }
    }
}
