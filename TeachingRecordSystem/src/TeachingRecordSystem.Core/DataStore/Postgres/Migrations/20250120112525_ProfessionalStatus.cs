using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ProfessionalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dqt_created_on",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "dqt_first_sync",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "dqt_last_sync",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "dqt_modified_on",
                table: "qualifications");

            migrationBuilder.RenameColumn(
                name: "dqt_state",
                table: "qualifications",
                newName: "training_age");

            migrationBuilder.AddColumn<int>(
                name: "age_range_from",
                table: "qualifications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "age_range_to",
                table: "qualifications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "award_date",
                table: "qualifications",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "country_id",
                table: "qualifications",
                type: "character varying(4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dqt_early_years_status_name",
                table: "qualifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dqt_early_years_status_value",
                table: "qualifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "dqt_initial_teacher_training_id",
                table: "qualifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "dqt_qts_registration_id",
                table: "qualifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dqt_teacher_status_name",
                table: "qualifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dqt_teacher_status_value",
                table: "qualifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "induction_exemption_reason_id",
                table: "qualifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "route_to_professional_status_id",
                table: "qualifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "qualifications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "training_provider_id",
                table: "qualifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid[]>(
                name: "training_subject_ids",
                table: "qualifications",
                type: "uuid[]",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "job_name",
                table: "job_metadata",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "training_providers",
                columns: table => new
                {
                    training_provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_training_providers", x => x.training_provider_id);
                });

            migrationBuilder.CreateTable(
                name: "training_subjects",
                columns: table => new
                {
                    training_subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_training_subjects", x => x.training_subject_id);
                });

            migrationBuilder.InsertData(
                table: "training_providers",
                columns: new[] { "training_provider_id", "is_active", "name" },
                values: new object[] { new Guid("98bcf32f-9f84-4142-89a5-accb616153a2"), true, "Test provider" });

            migrationBuilder.InsertData(
                table: "training_subjects",
                columns: new[] { "training_subject_id", "is_active", "name" },
                values: new object[] { new Guid("02d718fb-2686-41ee-8819-79266b139ec7"), true, "Test subject" });

            migrationBuilder.AddForeignKey(
                name: "fk_qualifications_countries_country_id",
                table: "qualifications",
                column: "country_id",
                principalTable: "countries",
                principalColumn: "country_id");

            migrationBuilder.AddForeignKey(
                name: "fk_qualifications_induction_exemption_reasons_induction_exempt",
                table: "qualifications",
                column: "induction_exemption_reason_id",
                principalTable: "induction_exemption_reasons",
                principalColumn: "induction_exemption_reason_id");

            migrationBuilder.AddForeignKey(
                name: "fk_qualifications_routes_to_professional_status_route_to_profe",
                table: "qualifications",
                column: "route_to_professional_status_id",
                principalTable: "routes_to_professional_status",
                principalColumn: "route_to_professional_status_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_qualifications_training_providers_training_provider_id",
                table: "qualifications",
                column: "training_provider_id",
                principalTable: "training_providers",
                principalColumn: "training_provider_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_qualifications_countries_country_id",
                table: "qualifications");

            migrationBuilder.DropForeignKey(
                name: "fk_qualifications_induction_exemption_reasons_induction_exempt",
                table: "qualifications");

            migrationBuilder.DropForeignKey(
                name: "fk_qualifications_routes_to_professional_status_route_to_profe",
                table: "qualifications");

            migrationBuilder.DropForeignKey(
                name: "fk_qualifications_training_providers_training_provider_id",
                table: "qualifications");

            migrationBuilder.DropTable(
                name: "training_providers");

            migrationBuilder.DropTable(
                name: "training_subjects");

            migrationBuilder.DropColumn(
                name: "age_range_from",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "age_range_to",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "award_date",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "country_id",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "dqt_early_years_status_name",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "dqt_early_years_status_value",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "dqt_initial_teacher_training_id",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "dqt_qts_registration_id",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "dqt_teacher_status_name",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "dqt_teacher_status_value",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "induction_exemption_reason_id",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "route_to_professional_status_id",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "status",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "training_provider_id",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "training_subject_ids",
                table: "qualifications");

            migrationBuilder.RenameColumn(
                name: "training_age",
                table: "qualifications",
                newName: "dqt_state");

            migrationBuilder.AddColumn<DateTime>(
                name: "dqt_created_on",
                table: "qualifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "dqt_first_sync",
                table: "qualifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "dqt_last_sync",
                table: "qualifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "dqt_modified_on",
                table: "qualifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "job_name",
                table: "job_metadata",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);
        }
    }
}
