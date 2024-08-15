using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AlertTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "alert_categories",
                columns: table => new
                {
                    alert_category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alert_categories", x => x.alert_category_id);
                });

            migrationBuilder.CreateTable(
                name: "alert_types",
                columns: table => new
                {
                    alert_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alert_category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alert_types", x => x.alert_type_id);
                    table.ForeignKey(
                        name: "fk_alert_types_alert_category",
                        column: x => x.alert_category_id,
                        principalTable: "alert_categories",
                        principalColumn: "alert_category_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "alerts",
                columns: table => new
                {
                    alert_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alert_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    details = table.Column<string>(type: "text", nullable: false),
                    external_link = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alerts", x => x.alert_id);
                    table.ForeignKey(
                        name: "fk_alerts_alert_type",
                        column: x => x.alert_type_id,
                        principalTable: "alert_types",
                        principalColumn: "alert_type_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_alerts_person",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "person_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_alert_types_alert_category_id",
                table: "alert_types",
                column: "alert_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_alerts_alert_type_id",
                table: "alerts",
                column: "alert_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_alerts_person_id",
                table: "alerts",
                column: "person_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alerts");

            migrationBuilder.DropTable(
                name: "alert_types");

            migrationBuilder.DropTable(
                name: "alert_categories");
        }
    }
}
