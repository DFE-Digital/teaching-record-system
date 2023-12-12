using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserFksOnQualification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_qualifications_created_by",
                table: "qualifications");

            migrationBuilder.DropForeignKey(
                name: "fk_qualifications_deleted_by",
                table: "qualifications");

            migrationBuilder.DropForeignKey(
                name: "fk_qualifications_updated_by",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "deleted_by_user_id",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                table: "qualifications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "created_by_user_id",
                table: "qualifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "deleted_by_user_id",
                table: "qualifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by_user_id",
                table: "qualifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "fk_qualifications_created_by",
                table: "qualifications",
                column: "created_by_user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_qualifications_deleted_by",
                table: "qualifications",
                column: "deleted_by_user_id",
                principalTable: "users",
                principalColumn: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_qualifications_updated_by",
                table: "qualifications",
                column: "updated_by_user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
