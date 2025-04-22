using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddMissinDqtUserNameDqtNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "original_filename",
                table: "dqt_notes",
                newName: "original_file_name");

            migrationBuilder.RenameColumn(
                name: "filename",
                table: "dqt_notes",
                newName: "file_name");

            migrationBuilder.RenameColumn(
                name: "modified_on",
                table: "dqt_notes",
                newName: "created_on");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "dqt_notes",
                newName: "created_by_dqt_user_id");

            migrationBuilder.AddColumn<string>(
                name: "created_by_dqt_user_name",
                table: "dqt_notes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by_dqt_user_id",
                table: "dqt_notes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_dqt_user_name",
                table: "dqt_notes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_on",
                table: "dqt_notes",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by_dqt_user_name",
                table: "dqt_notes");

            migrationBuilder.DropColumn(
                name: "updated_by_dqt_user_id",
                table: "dqt_notes");

            migrationBuilder.DropColumn(
                name: "updated_by_dqt_user_name",
                table: "dqt_notes");

            migrationBuilder.DropColumn(
                name: "updated_on",
                table: "dqt_notes");

            migrationBuilder.RenameColumn(
                name: "original_file_name",
                table: "dqt_notes",
                newName: "original_filename");

            migrationBuilder.RenameColumn(
                name: "file_name",
                table: "dqt_notes",
                newName: "filename");

            migrationBuilder.RenameColumn(
                name: "created_on",
                table: "dqt_notes",
                newName: "modified_on");

            migrationBuilder.RenameColumn(
                name: "created_by_dqt_user_id",
                table: "dqt_notes",
                newName: "created_by");
        }
    }
}
