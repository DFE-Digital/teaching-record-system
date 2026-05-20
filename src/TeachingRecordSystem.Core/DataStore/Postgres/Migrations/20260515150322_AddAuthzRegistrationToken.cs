using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthzRegistrationToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "authz_registration_tokens",
                columns: table => new
                {
                    authz_registration_token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    trn = table.Column<string>(type: "character(7)", nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, collation: "case_insensitive"),
                    created_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_authz_registration_tokens", x => x.authz_registration_token);
                });

            migrationBuilder.CreateIndex(
                name: "ix_authz_registration_tokens_email",
                table: "authz_registration_tokens",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_authz_registration_tokens_is_active",
                table: "authz_registration_tokens",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_authz_registration_tokens_trn",
                table: "authz_registration_tokens",
                column: "trn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "authz_registration_tokens");
        }
    }
}
