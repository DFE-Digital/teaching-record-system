using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Route : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    country_id = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_countries", x => x.country_id);
                });

            migrationBuilder.CreateTable(
                name: "routes",
                columns: table => new
                {
                    route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_reference = table.Column<string>(type: "text", nullable: true),
                    qualification_type = table.Column<int>(type: "integer", nullable: false),
                    qualification_id = table.Column<Guid>(type: "uuid", nullable: true),
                    route_type = table.Column<int>(type: "integer", nullable: false),
                    route_status = table.Column<int>(type: "integer", nullable: false),
                    country_id = table.Column<string>(type: "character varying(4)", nullable: true),
                    induction_exemption_reason = table.Column<int>(type: "integer", nullable: true),
                    itt_provider_id = table.Column<Guid>(type: "uuid", nullable: true),
                    programme_start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    programme_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    age_range_from = table.Column<int>(type: "integer", nullable: true),
                    age_range_to = table.Column<int>(type: "integer", nullable: true),
                    subjects = table.Column<List<Guid>>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_routes", x => x.route_id);
                    table.ForeignKey(
                        name: "fk_routes_countries_country_id",
                        column: x => x.country_id,
                        principalTable: "countries",
                        principalColumn: "country_id");
                    table.ForeignKey(
                        name: "fk_routes_persons_person_id",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "person_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_routes_qualifications_qualification_id",
                        column: x => x.qualification_id,
                        principalTable: "qualifications",
                        principalColumn: "qualification_id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_routes_person_id",
                table: "routes",
                column: "person_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "routes");

            migrationBuilder.DropTable(
                name: "countries");
        }
    }
}
