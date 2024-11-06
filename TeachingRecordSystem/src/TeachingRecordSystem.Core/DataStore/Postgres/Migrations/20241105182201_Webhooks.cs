using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Webhooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "webhook_endpoints",
                columns: table => new
                {
                    webhook_endpoint_id = table.Column<Guid>(type: "uuid", nullable: false),
                    application_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    api_version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    cloud_event_types = table.Column<List<string>>(type: "text[]", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_webhook_endpoints", x => x.webhook_endpoint_id);
                    table.ForeignKey(
                        name: "fk_webhook_endpoints_application_users_application_user_id",
                        column: x => x.application_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "webhook_messages",
                columns: table => new
                {
                    webhook_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    webhook_endpoint_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cloud_event_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    cloud_event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    api_version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data = table.Column<JsonElement>(type: "jsonb", nullable: false),
                    next_delivery_attempt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivery_attempts = table.Column<List<DateTime>>(type: "timestamp with time zone[]", nullable: false),
                    delivery_errors = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_webhook_messages", x => x.webhook_message_id);
                    table.ForeignKey(
                        name: "fk_webhook_messages_webhook_endpoints_webhook_endpoint_id",
                        column: x => x.webhook_endpoint_id,
                        principalTable: "webhook_endpoints",
                        principalColumn: "webhook_endpoint_id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "webhook_messages");

            migrationBuilder.DropTable(
                name: "webhook_endpoints");
        }
    }
}
