using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class EmailJobItemsTrnIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_qts_awarded_emails_job_items_trn",
                table: "qts_awarded_emails_job_items",
                column: "trn");

            migrationBuilder.CreateIndex(
                name: "ix_international_qts_awarded_emails_job_items_trn",
                table: "international_qts_awarded_emails_job_items",
                column: "trn");

            migrationBuilder.CreateIndex(
                name: "ix_induction_completed_emails_job_items_trn",
                table: "induction_completed_emails_job_items",
                column: "trn");

            migrationBuilder.CreateIndex(
                name: "ix_eyts_awarded_emails_job_items_trn",
                table: "eyts_awarded_emails_job_items",
                column: "trn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_qts_awarded_emails_job_items_trn",
                table: "qts_awarded_emails_job_items");

            migrationBuilder.DropIndex(
                name: "ix_international_qts_awarded_emails_job_items_trn",
                table: "international_qts_awarded_emails_job_items");

            migrationBuilder.DropIndex(
                name: "ix_induction_completed_emails_job_items_trn",
                table: "induction_completed_emails_job_items");

            migrationBuilder.DropIndex(
                name: "ix_eyts_awarded_emails_job_items_trn",
                table: "eyts_awarded_emails_job_items");
        }
    }
}
