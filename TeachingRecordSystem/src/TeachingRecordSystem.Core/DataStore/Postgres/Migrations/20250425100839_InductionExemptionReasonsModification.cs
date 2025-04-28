using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class InductionExemptionReasonsModification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("35caa6a3-49f2-4a63-bd5a-2ba5fa9dc5db"),
                column: "route_implicit_exemption",
                value: true);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("4c97e211-10d2-4c63-8da9-b0fcebe7f2f9"),
                column: "route_implicit_exemption",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("35caa6a3-49f2-4a63-bd5a-2ba5fa9dc5db"),
                column: "route_implicit_exemption",
                value: false);

            migrationBuilder.UpdateData(
                table: "induction_exemption_reasons",
                keyColumn: "induction_exemption_reason_id",
                keyValue: new Guid("4c97e211-10d2-4c63-8da9-b0fcebe7f2f9"),
                column: "route_implicit_exemption",
                value: true);
        }
    }
}
