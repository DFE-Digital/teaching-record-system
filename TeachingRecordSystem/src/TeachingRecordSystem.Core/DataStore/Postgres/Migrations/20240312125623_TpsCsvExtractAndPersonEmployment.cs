using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class TpsCsvExtractAndPersonEmployment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "person_employments",
                columns: table => new
                {
                    person_employment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    establishment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    employment_type = table.Column<int>(type: "integer", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_person_employments", x => x.person_employment_id);
                    table.ForeignKey(
                        name: "fk_person_employments_establishment_id",
                        column: x => x.establishment_id,
                        principalTable: "establishments",
                        principalColumn: "establishment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_person_employments_person_id",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "person_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tps_csv_extracts",
                columns: table => new
                {
                    tps_csv_extract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    filename = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tps_csv_extracts", x => x.tps_csv_extract_id);
                });

            migrationBuilder.CreateTable(
                name: "tps_csv_extract_load_items",
                columns: table => new
                {
                    tps_csv_extract_load_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tps_csv_extract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    national_insurance_number = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    date_of_birth = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    date_of_death = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    member_postcode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    member_email_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    local_authority_code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    establishment_number = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    establishment_postcode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    establishment_email_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    member_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    employment_start_date = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    employment_end_date = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    full_or_part_time_indicator = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    withdrawl_indicator = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    extract_date = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    gender = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    errors = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tps_csv_extract_load_items", x => x.tps_csv_extract_load_item_id);
                    table.ForeignKey(
                        name: "fk_tps_csv_extract_load_items_tps_csv_extract_id",
                        column: x => x.tps_csv_extract_id,
                        principalTable: "tps_csv_extracts",
                        principalColumn: "tps_csv_extract_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tps_csv_extract_items",
                columns: table => new
                {
                    tps_csv_extract_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tps_csv_extract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tps_csv_extract_load_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trn = table.Column<string>(type: "character(7)", fixedLength: true, maxLength: 7, nullable: false),
                    national_insurance_number = table.Column<string>(type: "character(9)", fixedLength: true, maxLength: 9, nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: false),
                    date_of_death = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    member_postcode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    member_email_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    local_authority_code = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    establishment_number = table.Column<string>(type: "character(4)", fixedLength: true, maxLength: 4, nullable: true),
                    establishment_postcode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    establishment_email_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    member_id = table.Column<int>(type: "integer", nullable: true),
                    employment_start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    employment_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    employment_type = table.Column<int>(type: "integer", nullable: false),
                    withdrawl_indicator = table.Column<string>(type: "character(1)", fixedLength: true, maxLength: 1, nullable: true),
                    extract_date = table.Column<DateOnly>(type: "date", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    result = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tps_csv_extract_items", x => x.tps_csv_extract_item_id);
                    table.ForeignKey(
                        name: "fk_tps_csv_extract_items_tps_csv_extract_id",
                        column: x => x.tps_csv_extract_id,
                        principalTable: "tps_csv_extracts",
                        principalColumn: "tps_csv_extract_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tps_csv_extract_items_tps_csv_extract_load_item_id",
                        column: x => x.tps_csv_extract_load_item_id,
                        principalTable: "tps_csv_extract_load_items",
                        principalColumn: "tps_csv_extract_load_item_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tps_csv_extract_items_la_code_establishment_number",
                table: "tps_csv_extract_items",
                columns: new[] { "local_authority_code", "establishment_number" });

            migrationBuilder.CreateIndex(
                name: "ix_tps_csv_extract_items_tps_csv_extract_id",
                table: "tps_csv_extract_items",
                column: "tps_csv_extract_id");

            migrationBuilder.CreateIndex(
                name: "ix_tps_csv_extract_items_tps_csv_extract_load_item_id",
                table: "tps_csv_extract_items",
                column: "tps_csv_extract_load_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_tps_csv_extract_items_trn",
                table: "tps_csv_extract_items",
                column: "trn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "person_employments");

            migrationBuilder.DropTable(
                name: "tps_csv_extract_items");

            migrationBuilder.DropTable(
                name: "tps_csv_extract_load_items");

            migrationBuilder.DropTable(
                name: "tps_csv_extracts");
        }
    }
}
