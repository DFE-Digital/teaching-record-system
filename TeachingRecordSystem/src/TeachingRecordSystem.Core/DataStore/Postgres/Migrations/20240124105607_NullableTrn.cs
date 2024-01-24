using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class NullableTrn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "trn",
                table: "persons",
                type: "character(7)",
                fixedLength: true,
                maxLength: 7,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character(7)",
                oldFixedLength: true,
                oldMaxLength: 7);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "trn",
                table: "persons",
                type: "character(7)",
                fixedLength: true,
                maxLength: 7,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character(7)",
                oldFixedLength: true,
                oldMaxLength: 7,
                oldNullable: true);
        }
    }
}
