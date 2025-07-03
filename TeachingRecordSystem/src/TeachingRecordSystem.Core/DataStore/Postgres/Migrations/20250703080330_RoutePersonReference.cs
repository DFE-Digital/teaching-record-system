using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RoutePersonReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_qualifications_source_application_user_id_source_applicatio",
                table: "qualifications");

            migrationBuilder.CreateIndex(
                name: "ix_qualifications_person_id_source_application_user_id_source_",
                table: "qualifications",
                columns: new[] { "person_id", "source_application_user_id", "source_application_reference" },
                unique: true,
                filter: "source_application_user_id is not null and source_application_reference is not null");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_qualifications_person_id_source_application_user_id_source_",
                table: "qualifications");

            migrationBuilder.CreateIndex(
                name: "ix_qualifications_source_application_user_id_source_applicatio",
                table: "qualifications",
                columns: new[] { "source_application_user_id", "source_application_reference" },
                unique: true,
                filter: "source_application_user_id is not null and source_application_reference is not null");
        }
    }
}
