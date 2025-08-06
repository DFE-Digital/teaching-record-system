using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddNpqApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "user_id", "active", "api_roles", "client_id", "client_secret", "is_oidc_client", "name", "one_login_authentication_scheme_name", "one_login_client_id", "one_login_post_logout_redirect_uri_path", "one_login_private_key_pem", "one_login_redirect_uri_path", "post_logout_redirect_uris", "redirect_uris", "short_name", "user_type" },
                values: new object[] { new Guid("0f18f1ec-a102-4023-843f-1cadef3e6e14"), true, null, null, null, false, "NPQ", null, null, null, null, null, null, null, null, 2 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("0f18f1ec-a102-4023-843f-1cadef3e6e14"));
        }
    }
}
