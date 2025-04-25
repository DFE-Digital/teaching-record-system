using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class SupportTaskTrnRequestMetadataLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true,
                oldCollation: "case_insensitive");

            migrationBuilder.AddColumn<Guid>(
                name: "trn_request_application_user_id",
                table: "support_tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trn_request_id",
                table: "support_tasks",
                type: "character varying(100)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_support_tasks_trn_request_metadata_trn_request_application_",
                table: "support_tasks",
                columns: new[] { "trn_request_application_user_id", "trn_request_id" },
                principalTable: "trn_request_metadata",
                principalColumns: new[] { "application_user_id", "request_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_support_tasks_trn_request_metadata_trn_request_application_",
                table: "support_tasks");

            migrationBuilder.DropColumn(
                name: "trn_request_application_user_id",
                table: "support_tasks");

            migrationBuilder.DropColumn(
                name: "trn_request_id",
                table: "support_tasks");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
