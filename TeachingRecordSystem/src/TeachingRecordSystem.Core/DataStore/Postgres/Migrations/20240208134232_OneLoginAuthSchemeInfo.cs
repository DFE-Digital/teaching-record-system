using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class OneLoginAuthSchemeInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_oidc_client",
                table: "users",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "one_login_authentication_scheme_name",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "one_login_post_logout_redirect_uri_path",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "one_login_redirect_uri_path",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_one_login_authentication_scheme_name",
                table: "users",
                column: "one_login_authentication_scheme_name",
                unique: true,
                filter: "one_login_authentication_scheme_name is not null");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_one_login_authentication_scheme_name",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_oidc_client",
                table: "users");

            migrationBuilder.DropColumn(
                name: "one_login_authentication_scheme_name",
                table: "users");

            migrationBuilder.DropColumn(
                name: "one_login_post_logout_redirect_uri_path",
                table: "users");

            migrationBuilder.DropColumn(
                name: "one_login_redirect_uri_path",
                table: "users");
        }
    }
}
