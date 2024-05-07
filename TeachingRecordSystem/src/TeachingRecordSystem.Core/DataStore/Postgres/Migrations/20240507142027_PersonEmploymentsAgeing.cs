using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class PersonEmploymentsAgeing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_person_employments_establishment_id",
                table: "person_employments");

            migrationBuilder.AddColumn<string>(
                name: "key",
                table: "tps_csv_extract_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "establishment_id",
                table: "person_employments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "key",
                table: "person_employments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "last_extract_date",
                table: "person_employments",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "last_known_employed_date",
                table: "person_employments",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateIndex(
                name: "ix_tps_csv_extract_items_key",
                table: "tps_csv_extract_items",
                column: "key");

            migrationBuilder.CreateIndex(
                name: "ix_person_employments_key",
                table: "person_employments",
                column: "key");

            migrationBuilder.AddForeignKey(
                name: "fk_person_employments_establishment_id",
                table: "person_employments",
                column: "establishment_id",
                principalTable: "establishments",
                principalColumn: "establishment_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_person_employments_establishment_id",
                table: "person_employments");

            migrationBuilder.DropIndex(
                name: "ix_tps_csv_extract_items_key",
                table: "tps_csv_extract_items");

            migrationBuilder.DropIndex(
                name: "ix_person_employments_key",
                table: "person_employments");

            migrationBuilder.DropColumn(
                name: "key",
                table: "tps_csv_extract_items");

            migrationBuilder.DropColumn(
                name: "key",
                table: "person_employments");

            migrationBuilder.DropColumn(
                name: "last_extract_date",
                table: "person_employments");

            migrationBuilder.DropColumn(
                name: "last_known_employed_date",
                table: "person_employments");

            migrationBuilder.AlterColumn<Guid>(
                name: "establishment_id",
                table: "person_employments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "fk_person_employments_establishment_id",
                table: "person_employments",
                column: "establishment_id",
                principalTable: "establishments",
                principalColumn: "establishment_id");
        }
    }
}
