using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    public partial class AddIdentityUserIdAndLinkedToIdentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "identity_user_id",
                table: "trn_requests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "linked_to_identity",
                table: "trn_requests",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "identity_user_id",
                table: "trn_requests");

            migrationBuilder.DropColumn(
                name: "linked_to_identity",
                table: "trn_requests");
        }
    }
}
