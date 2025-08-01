using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonCreatedByTps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "capita_trn_changed_on",
                table: "persons");

            migrationBuilder.AddColumn<bool>(
                name: "created_by_tps",
                table: "persons",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by_tps",
                table: "persons");

            migrationBuilder.AddColumn<DateTime>(
                name: "capita_trn_changed_on",
                table: "persons",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
