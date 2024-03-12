using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationUserOidcInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "client_id",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "client_secret",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "post_logout_redirect_uris",
                table: "users",
                type: "varchar[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "redirect_uris",
                table: "users",
                type: "varchar[]",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_client_id",
                table: "users",
                column: "client_id",
                unique: true,
                filter: "client_id is not null");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_client_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "client_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "client_secret",
                table: "users");

            migrationBuilder.DropColumn(
                name: "post_logout_redirect_uris",
                table: "users");

            migrationBuilder.DropColumn(
                name: "redirect_uris",
                table: "users");
        }
    }
}
