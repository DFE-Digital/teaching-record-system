using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QualifiedTeachersApi.Migrations
{
    /// <inheritdoc />
    public partial class PeopleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "people",
                columns: table => new
                {
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dqt_contact_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dqt_state = table.Column<int>(type: "integer", nullable: true),
                    trn = table.Column<string>(type: "character(7)", fixedLength: true, maxLength: 7, nullable: true),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, collation: "case_insensitive"),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, collation: "case_insensitive"),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_people", x => x.person_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_people_trn",
                table: "people",
                column: "trn",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "people");
        }
    }
}
