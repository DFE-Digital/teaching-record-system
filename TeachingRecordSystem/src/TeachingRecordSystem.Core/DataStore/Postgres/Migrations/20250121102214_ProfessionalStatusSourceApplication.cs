using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ProfessionalStatusSourceApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "source_application_reference",
                table: "qualifications",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "source_application_user_id",
                table: "qualifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_qualifications_source_application_user_id_source_applicatio",
                table: "qualifications",
                columns: new[] { "source_application_user_id", "source_application_reference" },
                unique: true,
                filter: "source_application_user_id is not null and source_application_reference is not null");

            migrationBuilder.AddForeignKey(
                name: "fk_qualifications_application_users_source_application_user_id",
                table: "qualifications",
                column: "source_application_user_id",
                principalTable: "users",
                principalColumn: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_qualifications_application_users_source_application_user_id",
                table: "qualifications");

            migrationBuilder.DropIndex(
                name: "ix_qualifications_source_application_user_id_source_applicatio",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "source_application_reference",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "source_application_user_id",
                table: "qualifications");
        }
    }
}
