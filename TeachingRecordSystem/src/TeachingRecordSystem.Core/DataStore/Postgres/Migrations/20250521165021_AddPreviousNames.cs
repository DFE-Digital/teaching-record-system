using System;
using Microsoft.EntityFrameworkCore.Migrations;
using TeachingRecordSystem.Core.Services.DqtReporting;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddPreviousNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "previous_names",
                columns: table => new
                {
                    previous_name_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, collation: "case_insensitive"),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, collation: "case_insensitive"),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_previous_names", x => x.previous_name_id);
                    table.ForeignKey(
                        name: "fk_previous_names_person",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "person_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_previous_names_person_id",
                table: "previous_names",
                column: "person_id");

            migrationBuilder.Sql($"ALTER PUBLICATION {DqtReportingService.TrsDbPublicationName} SET TABLE qualifications, tps_establishments, tps_employments, establishments, persons, alerts, alert_types, alert_categories, events, training_providers, notes, previous_names;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"ALTER PUBLICATION {DqtReportingService.TrsDbPublicationName} SET TABLE qualifications, tps_establishments, tps_employments, establishments, persons, alerts, alert_types, alert_categories, events, training_providers, notes;");

            migrationBuilder.DropTable(
                name: "previous_names");
        }
    }
}
