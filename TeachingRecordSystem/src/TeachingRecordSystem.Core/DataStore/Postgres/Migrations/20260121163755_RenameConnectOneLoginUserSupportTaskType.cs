using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RenameConnectOneLoginUserSupportTaskType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "support_task_types",
                keyColumn: "support_task_type",
                keyValue: 1,
                column: "name",
                value: "GOV.UK One Login - record matching");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "support_task_types",
                keyColumn: "support_task_type",
                keyValue: 1,
                column: "name",
                value: "connect GOV.UK One Login user to a teaching record");
        }
    }
}
