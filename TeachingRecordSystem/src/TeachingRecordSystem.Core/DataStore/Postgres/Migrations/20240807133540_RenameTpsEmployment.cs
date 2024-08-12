using System;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RenameTpsEmployment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "person_employments");

            migrationBuilder.RenameColumn(
                name: "withdrawl_indicator",
                table: "tps_csv_extract_load_items",
                newName: "withdrawal_indicator");

            migrationBuilder.RenameColumn(
                name: "withdrawl_indicator",
                table: "tps_csv_extract_items",
                newName: "withdrawal_indicator");

            migrationBuilder.CreateTable(
                name: "tps_employments",
                columns: table => new
                {
                    tps_employment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    establishment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    last_known_tps_employed_date = table.Column<DateOnly>(type: "date", nullable: false),
                    last_extract_date = table.Column<DateOnly>(type: "date", nullable: false),
                    employment_type = table.Column<int>(type: "integer", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    national_insurance_number = table.Column<string>(type: "character(9)", fixedLength: true, maxLength: 9, nullable: true),
                    person_postcode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    withdrawal_confirmed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tps_employments", x => x.tps_employment_id);
                    table.ForeignKey(
                        name: "fk_tps_employments_establishment_id",
                        column: x => x.establishment_id,
                        principalTable: "establishments",
                        principalColumn: "establishment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tps_employments_person_id",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "person_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tps_employments_establishment_id",
                table: "tps_employments",
                column: "establishment_id");

            migrationBuilder.CreateIndex(
                name: "ix_tps_employments_key",
                table: "tps_employments",
                column: "key");

            migrationBuilder.CreateIndex(
                name: "ix_tps_employments_person_id",
                table: "tps_employments",
                column: "person_id");

            migrationBuilder.InsertData(
                table: "establishments",
                columns: new[] { "establishment_id", "urn", "la_code", "la_name", "establishment_number", "establishment_name", "establishment_type_code", "establishment_type_name", "establishment_type_group_code", "establishment_type_group_name", "establishment_status_code", "establishment_status_name", "street", "locality", "address3", "town", "county", "postcode", "establishment_source_id" },
                values: new object[,]
                {
                    { new Guid("2bd072d8-6214-4a77-b9b4-e8d77d96a030"), null, "751", null, "0000", "Multi-Academy Trusts", null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("2b9b1c3e-8057-4a68-8250-2699368e2e98"), null, "751", null, "1570", "The Collective Community Trust", null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("cb7ef0ee-41ce-4c2d-87dc-91aa968ca76c"), null, "751", null, "1572", "Synergy Education Trust Limited", null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("7310b62f-454a-4d2c-8183-124acd71fd7a"), null, "751", null, "1573", "Mosaic Partnership Trust Ltd", null, null, null, null, null, null, null, null, null, null, null, null, 2 }
                });

            migrationBuilder.Procedure("p_refresh_tps_employments_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_delete_tps_employments_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_insert_tps_employments_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_update_tps_employments_person_search_attributes_v1.sql");            
            migrationBuilder.Trigger("trg_delete_tps_employments_person_search_attributes_v1.sql");
            migrationBuilder.Trigger("trg_insert_tps_employments_person_search_attributes_v1.sql");
            migrationBuilder.Trigger("trg_update_tps_employments_person_search_attributes_v1.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tps_employments");

            migrationBuilder.RenameColumn(
                name: "withdrawal_indicator",
                table: "tps_csv_extract_load_items",
                newName: "withdrawl_indicator");

            migrationBuilder.RenameColumn(
                name: "withdrawal_indicator",
                table: "tps_csv_extract_items",
                newName: "withdrawl_indicator");

            migrationBuilder.CreateTable(
                name: "person_employments",
                columns: table => new
                {
                    person_employment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    employment_type = table.Column<int>(type: "integer", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    establishment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_extract_date = table.Column<DateOnly>(type: "date", nullable: false),
                    last_known_employed_date = table.Column<DateOnly>(type: "date", nullable: false),
                    national_insurance_number = table.Column<string>(type: "character(9)", fixedLength: true, maxLength: 9, nullable: true),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_postcode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "ix_person_employments_establishment_id",
                table: "person_employments",
                column: "establishment_id");

            migrationBuilder.CreateIndex(
                name: "ix_person_employments_key",
                table: "person_employments",
                column: "key");

            migrationBuilder.CreateIndex(
                name: "ix_person_employments_person_id",
                table: "person_employments",
                column: "person_id");

            migrationBuilder.Procedure("p_refresh_person_employments_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_delete_person_employments_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_insert_person_employments_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_update_person_employments_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("p_refresh_person_search_attributes_v3.sql");
            migrationBuilder.Trigger("trg_delete_person_employments_person_search_attributes_v1.sql");
            migrationBuilder.Trigger("trg_insert_person_employments_person_search_attributes_v1.sql");
            migrationBuilder.Trigger("trg_update_person_employments_person_search_attributes_v1.sql");
        }
    }
}
