using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class PersonMergedWith : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "merged_with_person_id",
                table: "persons",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_persons_persons_merged_with_person_id",
                table: "persons",
                column: "merged_with_person_id",
                principalTable: "persons",
                principalColumn: "person_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_persons_persons_merged_with_person_id",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "merged_with_person_id",
                table: "persons");
        }
    }
}
