using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class EstablishmentSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_person_employments_establishment_id",
                table: "person_employments");

            migrationBuilder.AlterColumn<Guid>(
                name: "establishment_id",
                table: "person_employments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "urn",
                table: "establishments",
                type: "integer",
                fixedLength: true,
                maxLength: 6,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldFixedLength: true,
                oldMaxLength: 6);

            migrationBuilder.AlterColumn<string>(
                name: "la_name",
                table: "establishments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldCollation: "case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "establishment_type_name",
                table: "establishments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldCollation: "case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "establishment_type_group_name",
                table: "establishments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "establishment_type_group_code",
                table: "establishments",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "establishment_type_code",
                table: "establishments",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "establishment_status_name",
                table: "establishments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "establishment_status_code",
                table: "establishments",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "establishment_source_id",
                table: "establishments",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "establishment_sources",
                columns: table => new
                {
                    establishment_source_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_establishment_sources", x => x.establishment_source_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_person_employments_establishment_id",
                table: "person_employments",
                column: "establishment_id");

            migrationBuilder.CreateIndex(
                name: "ix_person_employments_person_id",
                table: "person_employments",
                column: "person_id");

            migrationBuilder.InsertData(
                table: "establishment_sources",
                columns: new[] { "establishment_source_id", "name" },
                values: new object[,]
                {
                    { 1, "GIAS" },
                    { 2, "TPS" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_establishment_establishment_source_id",
                table: "establishments",
                column: "establishment_source_id");

            migrationBuilder.AddForeignKey(
                name: "fk_establishments_establishment_source_id",
                table: "establishments",
                column: "establishment_source_id",
                principalTable: "establishment_sources",
                principalColumn: "establishment_source_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_person_employments_establishment_id",
                table: "person_employments",
                column: "establishment_id",
                principalTable: "establishments",
                principalColumn: "establishment_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_establishments_establishment_source_id",
                table: "establishments");

            migrationBuilder.DropForeignKey(
                name: "fk_person_employments_establishment_id",
                table: "person_employments");

            migrationBuilder.DropTable(
                name: "establishment_sources");

            migrationBuilder.DropIndex(
                name: "ix_person_employments_establishment_id",
                table: "person_employments");

            migrationBuilder.DropIndex(
                name: "ix_person_employments_person_id",
                table: "person_employments");

            migrationBuilder.DropIndex(
                name: "ix_establishment_establishment_source_id",
                table: "establishments");

            migrationBuilder.DropColumn(
                name: "establishment_source_id",
                table: "establishments");

            migrationBuilder.AlterColumn<Guid>(
                name: "establishment_id",
                table: "person_employments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "urn",
                table: "establishments",
                type: "integer",
                fixedLength: true,
                maxLength: 6,
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldFixedLength: true,
                oldMaxLength: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "la_name",
                table: "establishments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldCollation: "case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "establishment_type_name",
                table: "establishments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldCollation: "case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "establishment_type_group_name",
                table: "establishments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "establishment_type_group_code",
                table: "establishments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "establishment_type_code",
                table: "establishments",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "establishment_status_name",
                table: "establishments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "establishment_status_code",
                table: "establishments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_person_employments_establishment_id",
                table: "person_employments",
                column: "establishment_id",
                principalTable: "establishments",
                principalColumn: "establishment_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
