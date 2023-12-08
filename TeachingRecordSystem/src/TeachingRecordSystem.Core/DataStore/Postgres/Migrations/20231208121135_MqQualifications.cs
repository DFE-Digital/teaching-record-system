using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class MqQualifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "qualifications",
                columns: table => new
                {
                    qualification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    qualification_type = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dqt_qualification_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dqt_first_sync = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dqt_last_sync = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dqt_state = table.Column<int>(type: "integer", nullable: true),
                    dqt_created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dqt_modified_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    mq_specialism = table.Column<int>(type: "integer", nullable: true),
                    mq_status = table.Column<int>(type: "integer", nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    dqt_mq_establishment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dqt_specialism_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_qualifications", x => x.qualification_id);
                    table.ForeignKey(
                        name: "fk_qualifications_created_by",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_qualifications_deleted_by",
                        column: x => x.deleted_by_user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "fk_qualifications_person",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "person_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_qualifications_updated_by",
                        column: x => x.updated_by_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_qualifications_dqt_qualification_id",
                table: "qualifications",
                column: "dqt_qualification_id",
                unique: true,
                filter: "dqt_qualification_id is not null");

            migrationBuilder.CreateIndex(
                name: "ix_qualifications_person_id",
                table: "qualifications",
                column: "person_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "qualifications");
        }
    }
}
