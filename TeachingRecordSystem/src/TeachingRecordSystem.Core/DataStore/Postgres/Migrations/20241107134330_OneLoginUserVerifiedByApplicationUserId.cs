using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class OneLoginUserVerifiedByApplicationUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "verified_by_application_user_id",
                table: "one_login_users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_one_login_users_application_users_verified_by_application_u",
                table: "one_login_users",
                column: "verified_by_application_user_id",
                principalTable: "users",
                principalColumn: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_one_login_users_application_users_verified_by_application_u",
                table: "one_login_users");

            migrationBuilder.DropColumn(
                name: "verified_by_application_user_id",
                table: "one_login_users");
        }
    }
}
