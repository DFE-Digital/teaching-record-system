using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDqtSyncAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dqt_created_on",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "dqt_first_sync",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "dqt_last_sync",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "dqt_modified_on",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "dqt_sanction_id",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "dqt_state",
                table: "alerts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "dqt_created_on",
                table: "alerts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "dqt_first_sync",
                table: "alerts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "dqt_last_sync",
                table: "alerts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "dqt_modified_on",
                table: "alerts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "dqt_sanction_id",
                table: "alerts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dqt_state",
                table: "alerts",
                type: "integer",
                nullable: true);
        }
    }
}
