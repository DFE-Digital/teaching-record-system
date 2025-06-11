using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Holds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "award_date_required",
                table: "route_to_professional_status_types",
                newName: "holds_from_required");

            migrationBuilder.RenameColumn(
                name: "awarded_date",
                table: "qualifications",
                newName: "holds_from");

            migrationBuilder.Sql("update qualifications set status = 1 where status = 7 and qualification_type = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "holds_from_required",
                table: "route_to_professional_status_types",
                newName: "award_date_required");

            migrationBuilder.RenameColumn(
                name: "holds_from",
                table: "qualifications",
                newName: "awarded_date");
        }
    }
}
