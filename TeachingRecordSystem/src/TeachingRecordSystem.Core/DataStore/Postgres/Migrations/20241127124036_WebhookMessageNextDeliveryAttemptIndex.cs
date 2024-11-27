using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class WebhookMessageNextDeliveryAttemptIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_webhook_messages_next_delivery_attempt",
                table: "webhook_messages",
                column: "next_delivery_attempt",
                filter: "next_delivery_attempt is not null");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_webhook_messages_next_delivery_attempt",
                table: "webhook_messages");
        }
    }
}
