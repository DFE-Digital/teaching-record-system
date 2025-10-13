using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ProcessUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "processes",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "dqt_user_id",
                table: "processes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dqt_user_name",
                table: "processes",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "event_name",
                table: "process_events",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "ix_processes_process_type",
                table: "processes",
                column: "process_type");

            migrationBuilder.AddForeignKey(
                name: "fk_processes_users_user_id",
                table: "processes",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_processes_users_user_id",
                table: "processes");

            migrationBuilder.DropIndex(
                name: "ix_processes_process_type",
                table: "processes");

            migrationBuilder.DropColumn(
                name: "dqt_user_id",
                table: "processes");

            migrationBuilder.DropColumn(
                name: "dqt_user_name",
                table: "processes");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "processes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "event_name",
                table: "process_events",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);
        }
    }
}
