using System;
using Microsoft.EntityFrameworkCore.Migrations;
using TeachingRecordSystem.Core.Services.DqtReporting;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddRouteMigrationReportItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "route_migration_report_items",
                columns: table => new
                {
                    route_migration_report_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    migrated = table.Column<bool>(type: "boolean", nullable: false),
                    not_migrated_reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    dqt_initial_teacher_training_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dqt_itt_slug_id = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    dqt_itt_programme_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    dqt_itt_programme_start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    dqt_itt_programme_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    dqt_itt_result = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    dqt_itt_qualification_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    dqt_itt_qualification_value = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    dqt_itt_provider_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dqt_itt_provider_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    dqt_itt_provider_ukprn = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    dqt_itt_country_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    dqt_itt_country_value = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    dqt_itt_subject1_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    dqt_itt_subject1_value = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    dqt_itt_subject2_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    dqt_itt_subject2_value = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    dqt_itt_subject3_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    dqt_itt_subject3_value = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    dqt_age_range_from = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    dqt_age_range_to = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    dqt_qts_registration_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dqt_teacher_status_name = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    dqt_teacher_status_value = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    dqt_early_years_status_name = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    dqt_early_years_status_value = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    dqt_qts_date = table.Column<DateOnly>(type: "date", nullable: true),
                    dqt_eyts_date = table.Column<DateOnly>(type: "date", nullable: true),
                    dqt_partial_recognition_date = table.Column<DateOnly>(type: "date", nullable: true),
                    dqt_qtls_date = table.Column<DateOnly>(type: "date", nullable: true),
                    dqt_qtls_date_has_been_set = table.Column<bool>(type: "boolean", nullable: true),
                    status_derived_route_to_professional_status_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status_derived_route_to_professional_status_type_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    programme_type_derived_route_to_professional_status_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    programme_type_derived_route_to_professional_status_type_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    itt_qualification_derived_route_to_professional_status_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    itt_qualification_derived_route_to_professional_status_type_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    multiple_potential_compatible_itt_records = table.Column<Guid[]>(type: "uuid[]", nullable: true),
                    induction_exemption_reason_ids_moved_from_person = table.Column<Guid[]>(type: "uuid[]", nullable: true),
                    contact_itt_row_count = table.Column<int>(type: "integer", nullable: false),
                    contact_qts_row_count = table.Column<int>(type: "integer", nullable: false),
                    route_to_professional_status_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    route_to_professional_status_type_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    source_application_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    source_application_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source_application_user_short_name = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    holds_from = table.Column<DateOnly>(type: "date", nullable: true),
                    training_start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    training_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    training_subject1_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    training_subject1_reference = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    training_subject2_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    training_subject2_reference = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    training_subject3_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    training_subject3_reference = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    training_age_specialism_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    training_age_specialism_range_from = table.Column<int>(type: "integer", nullable: true),
                    training_age_specialism_range_to = table.Column<int>(type: "integer", nullable: true),
                    training_country_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    training_country_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    training_provider_id = table.Column<Guid>(type: "uuid", nullable: true),
                    training_provider_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    training_provider_ukprn = table.Column<string>(type: "character(8)", fixedLength: true, maxLength: 8, nullable: true),
                    exempt_from_induction = table.Column<bool>(type: "boolean", nullable: true),
                    exempt_from_induction_due_to_qts_date = table.Column<bool>(type: "boolean", nullable: true),
                    degree_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    degree_type_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_route_migration_report_items", x => x.route_migration_report_item_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_route_migration_report_items_person_id",
                table: "route_migration_report_items",
                column: "person_id");

            migrationBuilder.Sql($"ALTER PUBLICATION {DqtReportingService.TrsDbPublicationName} SET TABLE qualifications, tps_establishments, tps_employments, establishments, persons, alerts, alert_types, alert_categories, events, training_providers, notes, previous_names, route_migration_report_items;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"ALTER PUBLICATION {DqtReportingService.TrsDbPublicationName} SET TABLE qualifications, tps_establishments, tps_employments, establishments, persons, alerts, alert_types, alert_categories, events, training_providers, notes, previous_names;");

            migrationBuilder.DropTable(
                name: "route_migration_report_items");
        }
    }
}
