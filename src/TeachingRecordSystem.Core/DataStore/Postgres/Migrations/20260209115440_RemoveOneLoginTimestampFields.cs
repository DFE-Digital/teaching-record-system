using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOneLoginTimestampFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "first_one_login_sign_in",
                table: "one_login_users");

            migrationBuilder.DropColumn(
                name: "first_sign_in",
                table: "one_login_users");

            migrationBuilder.DropColumn(
                name: "last_one_login_sign_in",
                table: "one_login_users");

            migrationBuilder.DropColumn(
                name: "last_sign_in",
                table: "one_login_users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "first_one_login_sign_in",
                table: "one_login_users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "first_sign_in",
                table: "one_login_users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_one_login_sign_in",
                table: "one_login_users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_sign_in",
                table: "one_login_users",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
