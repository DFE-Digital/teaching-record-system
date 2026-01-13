using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class UseSharedOneLoginSigningKeysNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("0f18f1ec-a102-4023-843f-1cadef3e6e14"),
                column: "use_shared_one_login_signing_keys",
                value: null);

            migrationBuilder.Sql("update users set use_shared_one_login_signing_keys = null where is_oidc_client is distinct from true;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("0f18f1ec-a102-4023-843f-1cadef3e6e14"),
                column: "use_shared_one_login_signing_keys",
                value: false);
        }
    }
}
