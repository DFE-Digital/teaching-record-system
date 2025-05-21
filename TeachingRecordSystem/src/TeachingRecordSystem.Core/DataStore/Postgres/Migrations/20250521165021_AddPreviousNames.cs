using System;
using Microsoft.EntityFrameworkCore.Migrations;

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
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, collation: "case_insensitive"),
                    dqt_first_name_previous_name_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dqt_first_name_first_sync = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dqt_first_name_last_sync = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dqt_first_name_state = table.Column<int>(type: "integer", nullable: true),
                    dqt_first_name_created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dqt_first_name_modified_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dqt_middle_name_previous_name_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dqt_middle_name_first_sync = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dqt_middle_name_last_sync = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dqt_middle_name_state = table.Column<int>(type: "integer", nullable: true),
                    dqt_middle_name_created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dqt_middle_name_modified_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dqt_last_name_previous_name_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dqt_last_name_first_sync = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dqt_last_name_last_sync = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dqt_last_name_state = table.Column<int>(type: "integer", nullable: true),
                    dqt_last_name_created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dqt_last_name_modified_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                name: "ix_previous_names_dqt_first_name_previous_name_id",
                table: "previous_names",
                column: "dqt_first_name_previous_name_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_previous_names_dqt_last_name_previous_name_id",
                table: "previous_names",
                column: "dqt_last_name_previous_name_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_previous_names_dqt_middle_name_previous_name_id",
                table: "previous_names",
                column: "dqt_middle_name_previous_name_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_previous_names_person_id",
                table: "previous_names",
                column: "person_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "previous_names");
        }
    }
}
