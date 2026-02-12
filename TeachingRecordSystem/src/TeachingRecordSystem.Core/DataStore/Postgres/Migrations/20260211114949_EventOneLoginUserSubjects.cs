using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class EventOneLoginUserSubjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "one_login_user_subjects",
                table: "process_events",
                type: "varchar(255)[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.CreateIndex(
                name: "ix_process_events_one_login_user_subjects_event_name",
                table: "process_events",
                columns: new[] { "one_login_user_subjects", "event_name" })
                .Annotation("Npgsql:CreatedConcurrently", true)
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_process_events_one_login_user_subjects_event_name",
                table: "process_events");

            migrationBuilder.DropColumn(
                name: "one_login_user_subjects",
                table: "process_events");
        }
    }
}
