using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class OneLoginUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "one_login_users",
                columns: table => new
                {
                    subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    core_identity_vc = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    first_one_login_sign_in = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_one_login_sign_in = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    first_sign_in = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_sign_in = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    person_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_one_login_users", x => x.subject);
                    table.ForeignKey(
                        name: "fk_one_login_users_persons_person_id",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "person_id");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "one_login_users");
        }
    }
}
