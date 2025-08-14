using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class PersonSourceTrnRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "source_application_user_id",
                table: "persons",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source_trn_request_id",
                table: "persons",
                type: "character varying(100)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_persons_trn_request_metadata_source_application_user_id_sou",
                table: "persons",
                columns: new[] { "source_application_user_id", "source_trn_request_id" },
                principalTable: "trn_request_metadata",
                principalColumns: new[] { "application_user_id", "request_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_persons_trn_request_metadata_source_application_user_id_sou",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "source_application_user_id",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "source_trn_request_id",
                table: "persons");
        }
    }
}
