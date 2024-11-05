using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AlertTypeTypoFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("7924fe90-483c-49f8-84fc-674feddba848"),
                column: "name",
                value: "Secretary of State decision - no prohibition");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "alert_types",
                keyColumn: "alert_type_id",
                keyValue: new Guid("7924fe90-483c-49f8-84fc-674feddba848"),
                column: "name",
                value: "Secretary of State decision- no prohibition");
        }
    }
}
