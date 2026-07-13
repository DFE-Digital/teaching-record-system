using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class SupportTaskColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "assigned_to_user_id",
                table: "support_tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "completed_by_user_id",
                table: "support_tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "completed_on",
                table: "support_tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "outcome_label",
                table: "support_tasks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "subject_email_address",
                table: "support_tasks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                collation: "case_insensitive");

            migrationBuilder.AddColumn<string>(
                name: "subject_name",
                table: "support_tasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "subject_names",
                table: "support_tasks",
                type: "varchar[]",
                nullable: true,
                collation: "case_insensitive");

            migrationBuilder.CreateIndex(
                name: "ix_support_tasks_subject_email_address_subject_names",
                table: "support_tasks",
                columns: new[] { "subject_email_address", "subject_names" })
                .Annotation("Npgsql:CreatedConcurrently", true)
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Relational:Collation", new[] { "case_insensitive" });

            migrationBuilder.AddForeignKey(
                name: "fk_support_tasks_users_assigned_to_user_id",
                table: "support_tasks",
                column: "assigned_to_user_id",
                principalTable: "users",
                principalColumn: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_support_tasks_users_completed_by_user_id",
                table: "support_tasks",
                column: "completed_by_user_id",
                principalTable: "users",
                principalColumn: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_support_tasks_users_assigned_to_user_id",
                table: "support_tasks");

            migrationBuilder.DropForeignKey(
                name: "fk_support_tasks_users_completed_by_user_id",
                table: "support_tasks");

            migrationBuilder.DropIndex(
                name: "ix_support_tasks_subject_email_address_subject_names",
                table: "support_tasks");

            migrationBuilder.DropColumn(
                name: "assigned_to_user_id",
                table: "support_tasks");

            migrationBuilder.DropColumn(
                name: "completed_by_user_id",
                table: "support_tasks");

            migrationBuilder.DropColumn(
                name: "completed_on",
                table: "support_tasks");

            migrationBuilder.DropColumn(
                name: "outcome_label",
                table: "support_tasks");

            migrationBuilder.DropColumn(
                name: "subject_email_address",
                table: "support_tasks");

            migrationBuilder.DropColumn(
                name: "subject_name",
                table: "support_tasks");

            migrationBuilder.DropColumn(
                name: "subject_names",
                table: "support_tasks");
        }
    }
}
