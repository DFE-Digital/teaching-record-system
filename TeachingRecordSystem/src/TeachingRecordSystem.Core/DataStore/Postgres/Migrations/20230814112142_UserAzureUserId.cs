using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class UserAzureUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "azure_ad_subject",
                table: "users",
                newName: "azure_ad_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_azure_ad_user_id",
                table: "users",
                column: "azure_ad_user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_azure_ad_user_id",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "azure_ad_user_id",
                table: "users",
                newName: "azure_ad_subject");
        }
    }
}
