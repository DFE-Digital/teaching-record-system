using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Establishments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "establishments",
                columns: table => new
                {
                    establishment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    urn = table.Column<int>(type: "integer", fixedLength: true, maxLength: 6, nullable: false),
                    la_code = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    la_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, collation: "case_insensitive"),
                    establishment_number = table.Column<string>(type: "character(4)", fixedLength: true, maxLength: 4, nullable: true),
                    establishment_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false, collation: "case_insensitive"),
                    establishment_type_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    establishment_type_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, collation: "case_insensitive"),
                    establishment_type_group_code = table.Column<int>(type: "integer", nullable: false),
                    establishment_type_group_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    establishment_status_code = table.Column<int>(type: "integer", nullable: false),
                    establishment_status_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    street = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, collation: "case_insensitive"),
                    locality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, collation: "case_insensitive"),
                    address3 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, collation: "case_insensitive"),
                    town = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, collation: "case_insensitive"),
                    county = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, collation: "case_insensitive"),
                    postcode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_establishments", x => x.establishment_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_establishment_la_code_establishment_number",
                table: "establishments",
                columns: new[] { "la_code", "establishment_number" });

            migrationBuilder.CreateIndex(
                name: "ix_establishment_urn",
                table: "establishments",
                column: "urn",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "establishments");
        }
    }
}
