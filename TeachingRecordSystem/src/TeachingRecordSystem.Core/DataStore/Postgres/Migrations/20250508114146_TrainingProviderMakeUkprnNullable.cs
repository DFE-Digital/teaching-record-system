using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class TrainingProviderMakeUkprnNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ukprn",
                table: "training_providers",
                type: "character(8)",
                fixedLength: true,
                maxLength: 8,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character(8)",
                oldFixedLength: true,
                oldMaxLength: 8);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ukprn",
                table: "training_providers",
                type: "character(8)",
                fixedLength: true,
                maxLength: 8,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character(8)",
                oldFixedLength: true,
                oldMaxLength: 8,
                oldNullable: true);
        }
    }
}
