using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class NoteCreatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "created_by_user_id",
                table: "notes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_notes_users_created_by_user_id",
                table: "notes",
                column: "created_by_user_id",
                principalTable: "users",
                principalColumn: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_notes_users_created_by_user_id",
                table: "notes");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "notes");
        }
    }
}
