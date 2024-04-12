using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class VerifiedDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "verified_dates_of_birth",
                table: "one_login_users",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "verified_names",
                table: "one_login_users",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "verified_dates_of_birth",
                table: "one_login_users");

            migrationBuilder.DropColumn(
                name: "verified_names",
                table: "one_login_users");
        }
    }
}
