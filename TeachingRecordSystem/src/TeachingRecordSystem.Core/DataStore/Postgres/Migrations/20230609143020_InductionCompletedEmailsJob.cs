using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class InductionCompletedEmailsJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "induction_completed_emails_jobs",
                columns: table => new
                {
                    induction_completed_emails_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    awarded_to_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    executed_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_induction_completed_emails_jobs", x => x.induction_completed_emails_job_id);
                });

            migrationBuilder.CreateTable(
                name: "induction_completed_emails_job_items",
                columns: table => new
                {
                    induction_completed_emails_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trn = table.Column<string>(type: "character(7)", fixedLength: true, maxLength: 7, nullable: false),
                    email_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    personalization = table.Column<string>(type: "jsonb", nullable: false),
                    email_sent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_induction_completed_emails_job_items", x => new { x.induction_completed_emails_job_id, x.person_id });
                    table.ForeignKey(
                        name: "fk_induction_completed_emails_job_items_induction_completed_em",
                        column: x => x.induction_completed_emails_job_id,
                        principalTable: "induction_completed_emails_jobs",
                        principalColumn: "induction_completed_emails_job_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_induction_completed_emails_job_items_personalization",
                table: "induction_completed_emails_job_items",
                column: "personalization")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_induction_completed_emails_jobs_executed_utc",
                table: "induction_completed_emails_jobs",
                column: "executed_utc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "induction_completed_emails_job_items");

            migrationBuilder.DropTable(
                name: "induction_completed_emails_jobs");
        }
    }
}
