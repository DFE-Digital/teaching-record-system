using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class InductionAmends : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "cpd_induction_start_date",
                table: "persons",
                newName: "induction_failed_in_wales_start_date");

            migrationBuilder.RenameColumn(
                name: "cpd_induction_completed_date",
                table: "persons",
                newName: "induction_failed_in_wales_completed_date");

            migrationBuilder.AlterColumn<int>(
                name: "induction_status",
                table: "persons",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "induction_failed",
                table: "persons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "induction_passed",
                table: "persons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "induction_required_to_complete",
                table: "persons",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "induction_failed",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "induction_passed",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "induction_required_to_complete",
                table: "persons");

            migrationBuilder.RenameColumn(
                name: "induction_failed_in_wales_start_date",
                table: "persons",
                newName: "cpd_induction_start_date");

            migrationBuilder.RenameColumn(
                name: "induction_failed_in_wales_completed_date",
                table: "persons",
                newName: "cpd_induction_completed_date");

            migrationBuilder.AlterColumn<int>(
                name: "induction_status",
                table: "persons",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
