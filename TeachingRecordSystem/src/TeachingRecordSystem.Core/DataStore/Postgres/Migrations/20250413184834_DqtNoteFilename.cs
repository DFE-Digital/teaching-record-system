using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class DqtNoteFilename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "attachment_id",
                table: "dqt_notes",
                newName: "filename");

            migrationBuilder.AddColumn<string>(
                name: "original_filename",
                table: "dqt_notes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "original_filename",
                table: "dqt_notes");

            migrationBuilder.RenameColumn(
                name: "filename",
                table: "dqt_notes",
                newName: "attachment_id");
        }
    }
}
