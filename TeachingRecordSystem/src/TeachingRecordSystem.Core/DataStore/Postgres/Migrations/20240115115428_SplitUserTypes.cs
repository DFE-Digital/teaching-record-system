using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class SplitUserTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string[]>(
                name: "roles",
                table: "users",
                type: "varchar[]",
                nullable: true,
                oldClrType: typeof(string[]),
                oldType: "varchar[]");

            migrationBuilder.AddColumn<string[]>(
                name: "api_roles",
                table: "users",
                type: "varchar[]",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("a81394d1-a498-46d8-af3e-e077596ab303"),
                column: "user_type",
                value: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "api_roles",
                table: "users");

            migrationBuilder.AlterColumn<string[]>(
                name: "roles",
                table: "users",
                type: "varchar[]",
                nullable: false,
                defaultValue: new string[0],
                oldClrType: typeof(string[]),
                oldType: "varchar[]",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("a81394d1-a498-46d8-af3e-e077596ab303"),
                columns: new[] { "azure_ad_user_id", "dqt_user_id", "email", "roles", "user_type" },
                values: new object[] { null, null, null, new[] { "Administrator" }, 2 });
        }
    }
}
