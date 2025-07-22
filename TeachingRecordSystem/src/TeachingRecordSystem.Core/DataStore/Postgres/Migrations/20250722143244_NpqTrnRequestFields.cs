using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class NpqTrnRequestFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "npq_application_id",
                table: "trn_request_metadata",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "npq_evidence_file_id",
                table: "trn_request_metadata",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "npq_evidence_file_name",
                table: "trn_request_metadata",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "npq_name",
                table: "trn_request_metadata",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "npq_training_provider",
                table: "trn_request_metadata",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "npq_working_in_educational_setting",
                table: "trn_request_metadata",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "npq_application_id",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "npq_evidence_file_id",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "npq_evidence_file_name",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "npq_name",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "npq_training_provider",
                table: "trn_request_metadata");

            migrationBuilder.DropColumn(
                name: "npq_working_in_educational_setting",
                table: "trn_request_metadata");
        }
    }
}
