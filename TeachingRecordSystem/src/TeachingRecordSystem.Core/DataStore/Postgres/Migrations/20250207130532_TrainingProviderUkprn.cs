using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class TrainingProviderUkprn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "training_providers",
                keyColumn: "training_provider_id",
                keyValue: new Guid("98bcf32f-9f84-4142-89a5-accb616153a2"));

            migrationBuilder.AddColumn<string>(
                name: "ukprn",
                table: "training_providers",
                type: "character(8)",
                fixedLength: true,
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_training_provider_ukprn",
                table: "training_providers",
                column: "ukprn",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_training_provider_ukprn",
                table: "training_providers");

            migrationBuilder.DropColumn(
                name: "ukprn",
                table: "training_providers");

            migrationBuilder.InsertData(
                table: "training_providers",
                columns: new[] { "training_provider_id", "is_active", "name" },
                values: new object[] { new Guid("98bcf32f-9f84-4142-89a5-accb616153a2"), true, "Test provider" });
        }
    }
}
