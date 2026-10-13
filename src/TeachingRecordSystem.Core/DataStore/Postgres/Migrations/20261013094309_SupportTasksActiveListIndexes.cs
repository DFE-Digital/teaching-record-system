using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class SupportTasksActiveListIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_support_tasks_assigned_to_user_id",
                table: "support_tasks",
                column: "assigned_to_user_id")
                .Annotation("Npgsql:CreatedConcurrently", true);

            migrationBuilder.CreateIndex(
                name: "ix_support_tasks_outstanding_created_on",
                table: "support_tasks",
                column: "created_on",
                filter: "status <> 1 and deleted_on is null")
                .Annotation("Npgsql:CreatedConcurrently", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_support_tasks_assigned_to_user_id",
                table: "support_tasks");

            migrationBuilder.DropIndex(
                name: "ix_support_tasks_outstanding_created_on",
                table: "support_tasks");
        }
    }
}
