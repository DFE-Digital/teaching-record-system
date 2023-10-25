using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class PersonsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "persons",
                columns: table => new
                {
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trn = table.Column<string>(type: "character(7)", fixedLength: true, maxLength: 7, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    email_address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    national_insurance_number = table.Column<string>(type: "character(9)", fixedLength: true, maxLength: 9, nullable: true),
                    dqt_contact_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dqt_state = table.Column<int>(type: "integer", nullable: true),
                    dqt_first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    dqt_middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    dqt_last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_persons", x => x.person_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_persons_dqt_contact_id",
                table: "persons",
                column: "dqt_contact_id",
                unique: true,
                filter: "dqt_contact_id is not null");

            migrationBuilder.CreateIndex(
                name: "ix_persons_trn",
                table: "persons",
                column: "trn",
                unique: true,
                filter: "trn is not null");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "persons");
        }
    }
}
