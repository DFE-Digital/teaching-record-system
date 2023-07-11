using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    public partial class TrnRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trn_requests",
                columns: table => new
                {
                    trn_request_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    client_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    request_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    teacher_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trn_requests", x => x.trn_request_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_trn_requests_client_id_request_id",
                table: "trn_requests",
                columns: new[] { "client_id", "request_id" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trn_requests");
        }
    }
}
