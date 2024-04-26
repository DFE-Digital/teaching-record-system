using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class SupportTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "support_tasks",
                columns: table => new
                {
                    support_task_reference = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    support_task_type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    data = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    one_login_user_subject = table.Column<string>(type: "character varying(255)", nullable: true),
                    person_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_support_tasks", x => x.support_task_reference);
                    table.ForeignKey(
                        name: "fk_support_tasks_one_login_user",
                        column: x => x.one_login_user_subject,
                        principalTable: "one_login_users",
                        principalColumn: "subject");
                    table.ForeignKey(
                        name: "fk_support_tasks_person",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "person_id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_support_tasks_one_login_user_subject",
                table: "support_tasks",
                column: "one_login_user_subject");

            migrationBuilder.CreateIndex(
                name: "ix_support_tasks_person_id",
                table: "support_tasks",
                column: "person_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "support_tasks");
        }
    }
}
