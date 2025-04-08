using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class NoteTableRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_dqt_note_persons_person_id",
                table: "dqt_note");

            migrationBuilder.DropPrimaryKey(
                name: "pk_dqt_note",
                table: "dqt_note");

            migrationBuilder.RenameTable(
                name: "dqt_note",
                newName: "dqt_notes");

            migrationBuilder.AddPrimaryKey(
                name: "pk_dqt_notes",
                table: "dqt_notes",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_dqt_notes_persons_person_id",
                table: "dqt_notes",
                column: "person_id",
                principalTable: "persons",
                principalColumn: "person_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_dqt_notes_persons_person_id",
                table: "dqt_notes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_dqt_notes",
                table: "dqt_notes");

            migrationBuilder.RenameTable(
                name: "dqt_notes",
                newName: "dqt_note");

            migrationBuilder.AddPrimaryKey(
                name: "pk_dqt_note",
                table: "dqt_note",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_dqt_note_persons_person_id",
                table: "dqt_note",
                column: "person_id",
                principalTable: "persons",
                principalColumn: "person_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
