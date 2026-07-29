using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class SupportTaskSourceApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "source_application_user_id",
                table: "support_tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_support_tasks_application_users_source_application_user_id",
                table: "support_tasks",
                column: "source_application_user_id",
                principalTable: "users",
                principalColumn: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_support_tasks_application_users_source_application_user_id",
                table: "support_tasks");

            migrationBuilder.DropColumn(
                name: "source_application_user_id",
                table: "support_tasks");
        }
    }
}
