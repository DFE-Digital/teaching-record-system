using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RenameAuthRegistrationTokensColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "email",
                table: "authz_registration_tokens",
                newName: "emailaddress");

            migrationBuilder.RenameColumn(
                name: "authz_registration_token",
                table: "authz_registration_tokens",
                newName: "token");

            migrationBuilder.RenameIndex(
                name: "ix_authz_registration_tokens_email",
                table: "authz_registration_tokens",
                newName: "ix_authz_registration_tokens_emailaddress");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "emailaddress",
                table: "authz_registration_tokens",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "token",
                table: "authz_registration_tokens",
                newName: "authz_registration_token");

            migrationBuilder.RenameIndex(
                name: "ix_authz_registration_tokens_emailaddress",
                table: "authz_registration_tokens",
                newName: "ix_authz_registration_tokens_email");
        }
    }
}
