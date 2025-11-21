using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class OneLoginUserIdVerificationSupportTaskType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "support_task_types",
                columns: new[] { "support_task_type", "name" },
                values: new object[] { 8, "GOV.UK One Login - identity verification" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "support_task_types",
                keyColumn: "support_task_type",
                keyValue: 8);
        }
    }
}
