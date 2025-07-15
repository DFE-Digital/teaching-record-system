using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Emails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "awarded_to_utc",
                table: "induction_completed_emails_jobs",
                newName: "passed_end_utc");

            migrationBuilder.CreateTable(
                name: "emails",
                columns: table => new
                {
                    email_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    email_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    personalization = table.Column<string>(type: "jsonb", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    sent_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_emails", x => x.email_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "emails");

            migrationBuilder.RenameColumn(
                name: "passed_end_utc",
                table: "induction_completed_emails_jobs",
                newName: "awarded_to_utc");
        }
    }
}
