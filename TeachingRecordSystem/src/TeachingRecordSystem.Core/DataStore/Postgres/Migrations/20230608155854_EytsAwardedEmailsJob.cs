using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class EytsAwardedEmailsJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "eyts_awarded_emails_jobs",
                columns: table => new
                {
                    eyts_awarded_emails_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    awarded_to_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    executed_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_eyts_awarded_emails_jobs", x => x.eyts_awarded_emails_job_id);
                });

            migrationBuilder.CreateTable(
                name: "eyts_awarded_emails_job_items",
                columns: table => new
                {
                    eyts_awarded_emails_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trn = table.Column<string>(type: "character(7)", fixedLength: true, maxLength: 7, nullable: false),
                    email_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    personalization = table.Column<string>(type: "jsonb", nullable: false),
                    email_sent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_eyts_awarded_emails_job_items", x => new { x.eyts_awarded_emails_job_id, x.person_id });
                    table.ForeignKey(
                        name: "fk_eyts_awarded_emails_job_items_eyts_awarded_emails_jobs_eyts",
                        column: x => x.eyts_awarded_emails_job_id,
                        principalTable: "eyts_awarded_emails_jobs",
                        principalColumn: "eyts_awarded_emails_job_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_eyts_awarded_emails_job_items_personalization",
                table: "eyts_awarded_emails_job_items",
                column: "personalization")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_eyts_awarded_emails_jobs_executed_utc",
                table: "eyts_awarded_emails_jobs",
                column: "executed_utc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "eyts_awarded_emails_job_items");

            migrationBuilder.DropTable(
                name: "eyts_awarded_emails_jobs");
        }
    }
}
