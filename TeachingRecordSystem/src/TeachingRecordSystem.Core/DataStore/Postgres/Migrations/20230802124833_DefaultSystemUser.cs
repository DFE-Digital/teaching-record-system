using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class DefaultSystemUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "user_id", "active", "azure_ad_subject", "email", "name", "roles", "user_type" },
                values: new object[] { new Guid("a81394d1-a498-46d8-af3e-e077596ab303"), true, null, null, "System", new string[0], 2 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("a81394d1-a498-46d8-af3e-e077596ab303"));
        }
    }
}
