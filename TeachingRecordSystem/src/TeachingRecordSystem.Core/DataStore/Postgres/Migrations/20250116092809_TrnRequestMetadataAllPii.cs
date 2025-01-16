using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class TrnRequestMetadataAllPii : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address_line1",
                table: "trn_request_metadata",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_line2",
                table: "trn_request_metadata",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_line3",
                table: "trn_request_metadata",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "trn_request_metadata",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "country",
                table: "trn_request_metadata",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "gender",
                table: "trn_request_metadata",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "national_insurance_number",
                table: "trn_request_metadata",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "postcode",
                table: "trn_request_metadata",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "potential_duplicate",
                table: "trn_request_metadata",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address_line1",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "address_line2",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "address_line3",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "city",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "country",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "gender",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "national_insurance_number",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "postcode",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "potential_duplicate",
                table: "trn_request_metadata");
        }
    }
}
