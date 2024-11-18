using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class TrnRequestMetadataOneLoginUserSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "verified_one_login_user_subject",
                table: "trn_request_metadata",
                newName: "one_login_user_subject");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_on",
                table: "trn_request_metadata",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateOnly>(
                name: "date_of_birth",
                table: "trn_request_metadata",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "email_address",
                table: "trn_request_metadata",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "identity_verified",
                table: "trn_request_metadata",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "name",
                table: "trn_request_metadata",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.CreateIndex(
                name: "ix_trn_request_metadata_email_address",
                table: "trn_request_metadata",
                column: "email_address");

            migrationBuilder.CreateIndex(
                name: "ix_trn_request_metadata_one_login_user_subject",
                table: "trn_request_metadata",
                column: "one_login_user_subject");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_trn_request_metadata_email_address",
                table: "trn_request_metadata");

            migrationBuilder.DropIndex(
                name: "ix_trn_request_metadata_one_login_user_subject",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "created_on",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "date_of_birth",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "email_address",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "identity_verified",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "name",
                table: "trn_request_metadata");

            migrationBuilder.RenameColumn(
                name: "one_login_user_subject",
                table: "trn_request_metadata",
                newName: "verified_one_login_user_subject");
        }
    }
}
