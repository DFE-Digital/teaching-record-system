using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Squash20251013 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE EXTENSION IF NOT EXISTS pg_trgm;
                CREATE EXTENSION IF NOT EXISTS btree_gin;
                """);

            migrationBuilder.Sql("create collation case_insensitive (provider = icu, locale = 'und-u-ks-level2', deterministic = false);");


            // ***Generated migration starts***

            migrationBuilder.CreateTable(
                name: "alert_categories",
                columns: table => new
                {
                    alert_category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, collation: "case_insensitive"),
                    display_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alert_categories", x => x.alert_category_id);
                });

            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    country_id = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    official_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    citizen_names = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_countries", x => x.country_id);
                });

            migrationBuilder.CreateTable(
                name: "degree_types",
                columns: table => new
                {
                    degree_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_degree_types", x => x.degree_type_id);
                });

            migrationBuilder.CreateTable(
                name: "emails",
                columns: table => new
                {
                    email_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    email_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    personalization = table.Column<string>(type: "jsonb", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    sent_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    email_reply_to_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_emails", x => x.email_id);
                });

            migrationBuilder.CreateTable(
                name: "entity_changes_journals",
                columns: table => new
                {
                    key = table.Column<string>(type: "text", nullable: false),
                    entity_logical_name = table.Column<string>(type: "text", nullable: false),
                    data_token = table.Column<string>(type: "text", nullable: true),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    next_query_page_number = table.Column<int>(type: "integer", nullable: true),
                    next_query_page_size = table.Column<int>(type: "integer", nullable: true),
                    next_query_paging_cookie = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entity_changes_journals", x => new { x.key, x.entity_logical_name });
                });

            migrationBuilder.CreateTable(
                name: "establishment_sources",
                columns: table => new
                {
                    establishment_source_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_establishment_sources", x => x.establishment_source_id);
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    published = table.Column<bool>(type: "boolean", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    person_ids = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    qualification_id = table.Column<Guid>(type: "uuid", nullable: true),
                    alert_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_events", x => x.event_id);
                });

            migrationBuilder.CreateTable(
                name: "eyts_awarded_emails_jobs",
                columns: table => new
                {
                    eyts_awarded_emails_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    awarded_to_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    executed_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_eyts_awarded_emails_jobs", x => x.eyts_awarded_emails_job_id);
                });

            migrationBuilder.CreateTable(
                name: "induction_completed_emails_jobs",
                columns: table => new
                {
                    induction_completed_emails_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    passed_end_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    executed_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_induction_completed_emails_jobs", x => x.induction_completed_emails_job_id);
                });

            migrationBuilder.CreateTable(
                name: "induction_exemption_reasons",
                columns: table => new
                {
                    induction_exemption_reason_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    route_implicit_exemption = table.Column<bool>(type: "boolean", nullable: false),
                    route_only_exemption = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_induction_exemption_reasons", x => x.induction_exemption_reason_id);
                });

            migrationBuilder.CreateTable(
                name: "induction_statuses",
                columns: table => new
                {
                    induction_status = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_induction_statuses", x => x.induction_status);
                });

            migrationBuilder.CreateTable(
                name: "integration_transactions",
                columns: table => new
                {
                    integration_transaction_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    interface_type = table.Column<int>(type: "integer", nullable: false),
                    import_status = table.Column<int>(type: "integer", nullable: false),
                    total_count = table.Column<int>(type: "integer", nullable: false),
                    success_count = table.Column<int>(type: "integer", nullable: false),
                    failure_count = table.Column<int>(type: "integer", nullable: false),
                    duplicate_count = table.Column<int>(type: "integer", nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_integration_transactions", x => x.integration_transaction_id);
                });

            migrationBuilder.CreateTable(
                name: "international_qts_awarded_emails_jobs",
                columns: table => new
                {
                    international_qts_awarded_emails_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    awarded_to_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    executed_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_international_qts_awarded_emails_jobs", x => x.international_qts_awarded_emails_job_id);
                });

            migrationBuilder.CreateTable(
                name: "job_metadata",
                columns: table => new
                {
                    job_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_metadata", x => x.job_name);
                });

            migrationBuilder.CreateTable(
                name: "journey_states",
                columns: table => new
                {
                    instance_id = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    user_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    state = table.Column<string>(type: "text", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_journey_states", x => x.instance_id);
                });

            migrationBuilder.CreateTable(
                name: "mandatory_qualification_providers",
                columns: table => new
                {
                    mandatory_qualification_provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mandatory_qualification_providers", x => x.mandatory_qualification_provider_id);
                });

            migrationBuilder.CreateTable(
                name: "name_synonyms",
                columns: table => new
                {
                    name_synonyms_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, collation: "case_insensitive"),
                    synonyms = table.Column<string[]>(type: "text[]", nullable: false, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_name_synonyms", x => x.name_synonyms_id);
                });

            migrationBuilder.CreateTable(
                name: "oidc_applications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    application_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    client_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    client_secret = table.Column<string>(type: "text", nullable: true),
                    client_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    concurrency_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    consent_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    display_names = table.Column<string>(type: "text", nullable: true),
                    json_web_key_set = table.Column<string>(type: "text", nullable: true),
                    permissions = table.Column<string>(type: "text", nullable: true),
                    post_logout_redirect_uris = table.Column<string>(type: "text", nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    redirect_uris = table.Column<string>(type: "text", nullable: true),
                    requirements = table.Column<string>(type: "text", nullable: true),
                    settings = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_oidc_applications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "oidc_scopes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    concurrency_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    descriptions = table.Column<string>(type: "text", nullable: true),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    display_names = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    resources = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_oidc_scopes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_message_processor_metadata",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_message_processor_metadata", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "person_search_attributes",
                columns: table => new
                {
                    person_search_attribute_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attribute_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, collation: "case_insensitive"),
                    attribute_value = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, collation: "case_insensitive"),
                    tags = table.Column<string[]>(type: "text[]", nullable: false),
                    attribute_key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_person_search_attributes", x => x.person_search_attribute_id);
                });

            migrationBuilder.CreateTable(
                name: "qts_awarded_emails_jobs",
                columns: table => new
                {
                    qts_awarded_emails_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    awarded_to_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    executed_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_qts_awarded_emails_jobs", x => x.qts_awarded_emails_job_id);
                });

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
                    status = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
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

            migrationBuilder.CreateTable(
                name: "support_task_types",
                columns: table => new
                {
                    support_task_type = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_support_task_types", x => x.support_task_type);
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
                name: "tps_establishment_types",
                columns: table => new
                {
                    tps_establishment_type_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    establishment_range_from = table.Column<string>(type: "character(4)", fixedLength: true, maxLength: 4, nullable: false),
                    establishment_range_to = table.Column<string>(type: "character(4)", fixedLength: true, maxLength: 4, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    short_description = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tps_establishment_types", x => x.tps_establishment_type_id);
                });

            migrationBuilder.CreateTable(
                name: "tps_establishments",
                columns: table => new
                {
                    tps_establishment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    la_code = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    establishment_code = table.Column<string>(type: "character(4)", fixedLength: true, maxLength: 4, nullable: false),
                    employers_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    school_gias_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    school_closed_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tps_establishments", x => x.tps_establishment_id);
                });

            migrationBuilder.CreateTable(
                name: "training_providers",
                columns: table => new
                {
                    training_provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ukprn = table.Column<string>(type: "character(8)", fixedLength: true, maxLength: 8, nullable: true),
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
                    reference = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_training_subjects", x => x.training_subject_id);
                });

            migrationBuilder.CreateTable(
                name: "trn_ranges",
                columns: table => new
                {
                    from_trn = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    to_trn = table.Column<int>(type: "integer", nullable: false),
                    next_trn = table.Column<int>(type: "integer", nullable: false),
                    is_exhausted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trn_ranges", x => x.from_trn);
                });

            migrationBuilder.CreateTable(
                name: "trn_requests",
                columns: table => new
                {
                    trn_request_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    client_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    request_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    identity_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    linked_to_identity = table.Column<bool>(type: "boolean", nullable: false),
                    trn_token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trn_requests", x => x.trn_request_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    user_type = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    api_roles = table.Column<string[]>(type: "varchar[]", nullable: true),
                    is_oidc_client = table.Column<bool>(type: "boolean", nullable: true),
                    client_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    client_secret = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    redirect_uris = table.Column<List<string>>(type: "varchar[]", nullable: true),
                    post_logout_redirect_uris = table.Column<List<string>>(type: "varchar[]", nullable: true),
                    one_login_client_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    one_login_private_key_pem = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    one_login_authentication_scheme_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    one_login_redirect_uri_path = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    one_login_post_logout_redirect_uri_path = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    short_name = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    azure_ad_user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    dqt_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "alert_types",
                columns: table => new
                {
                    alert_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alert_category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, collation: "case_insensitive"),
                    dqt_sanction_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true, collation: "case_insensitive"),
                    internal_only = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: true)
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
                name: "establishments",
                columns: table => new
                {
                    establishment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    establishment_source_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    urn = table.Column<int>(type: "integer", fixedLength: true, maxLength: 6, nullable: true),
                    la_code = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    la_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, collation: "case_insensitive"),
                    establishment_number = table.Column<string>(type: "character(4)", fixedLength: true, maxLength: 4, nullable: true),
                    establishment_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false, collation: "case_insensitive"),
                    establishment_type_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    establishment_type_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, collation: "case_insensitive"),
                    establishment_type_group_code = table.Column<int>(type: "integer", nullable: true),
                    establishment_type_group_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    establishment_status_code = table.Column<int>(type: "integer", nullable: true),
                    establishment_status_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    phase_of_education_code = table.Column<int>(type: "integer", nullable: true),
                    phase_of_education_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    number_of_pupils = table.Column<int>(type: "integer", nullable: true),
                    free_school_meals_percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    street = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, collation: "case_insensitive"),
                    locality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, collation: "case_insensitive"),
                    address3 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, collation: "case_insensitive"),
                    town = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, collation: "case_insensitive"),
                    county = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, collation: "case_insensitive"),
                    postcode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_establishments", x => x.establishment_id);
                    table.ForeignKey(
                        name: "fk_establishments_establishment_source_id",
                        column: x => x.establishment_source_id,
                        principalTable: "establishment_sources",
                        principalColumn: "establishment_source_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "eyts_awarded_emails_job_items",
                columns: table => new
                {
                    eyts_awarded_emails_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trn = table.Column<string>(type: "character(7)", fixedLength: true, maxLength: 7, nullable: false),
                    email_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    personalization = table.Column<string>(type: "jsonb", nullable: false),
                    email_sent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_eyts_awarded_emails_job_items", x => new { x.eyts_awarded_emails_job_id, x.person_id });
                    table.ForeignKey(
                        name: "fk_eyts_awarded_emails_job_items_eyts_awarded_emails_jobs_eyts",
                        column: x => x.eyts_awarded_emails_job_id,
                        principalTable: "eyts_awarded_emails_jobs",
                        principalColumn: "eyts_awarded_emails_job_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "induction_completed_emails_job_items",
                columns: table => new
                {
                    induction_completed_emails_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trn = table.Column<string>(type: "character(7)", fixedLength: true, maxLength: 7, nullable: false),
                    email_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    personalization = table.Column<string>(type: "jsonb", nullable: false),
                    email_sent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_induction_completed_emails_job_items", x => new { x.induction_completed_emails_job_id, x.person_id });
                    table.ForeignKey(
                        name: "fk_induction_completed_emails_job_items_induction_completed_em",
                        column: x => x.induction_completed_emails_job_id,
                        principalTable: "induction_completed_emails_jobs",
                        principalColumn: "induction_completed_emails_job_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "route_to_professional_status_types",
                columns: table => new
                {
                    route_to_professional_status_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    professional_status_type = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    training_start_date_required = table.Column<int>(type: "integer", nullable: false),
                    training_end_date_required = table.Column<int>(type: "integer", nullable: false),
                    holds_from_required = table.Column<int>(type: "integer", nullable: false),
                    induction_exemption_required = table.Column<int>(type: "integer", nullable: false),
                    training_provider_required = table.Column<int>(type: "integer", nullable: false),
                    degree_type_required = table.Column<int>(type: "integer", nullable: false),
                    training_country_required = table.Column<int>(type: "integer", nullable: false),
                    training_age_specialism_type_required = table.Column<int>(type: "integer", nullable: false),
                    training_subjects_required = table.Column<int>(type: "integer", nullable: false),
                    induction_exemption_reason_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_route_to_professional_status_types", x => x.route_to_professional_status_type_id);
                    table.ForeignKey(
                        name: "fk_route_to_professional_status_induction_exemption_reason",
                        column: x => x.induction_exemption_reason_id,
                        principalTable: "induction_exemption_reasons",
                        principalColumn: "induction_exemption_reason_id");
                });

            migrationBuilder.CreateTable(
                name: "international_qts_awarded_emails_job_items",
                columns: table => new
                {
                    international_qts_awarded_emails_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trn = table.Column<string>(type: "character(7)", fixedLength: true, maxLength: 7, nullable: false),
                    email_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    personalization = table.Column<string>(type: "jsonb", nullable: false),
                    email_sent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_international_qts_awarded_emails_job_items", x => new { x.international_qts_awarded_emails_job_id, x.person_id });
                    table.ForeignKey(
                        name: "fk_international_qts_awarded_emails_job_items_international_qt",
                        column: x => x.international_qts_awarded_emails_job_id,
                        principalTable: "international_qts_awarded_emails_jobs",
                        principalColumn: "international_qts_awarded_emails_job_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "oidc_authorizations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    application_id = table.Column<Guid>(type: "uuid", nullable: true),
                    concurrency_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    creation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    scopes = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_oidc_authorizations", x => x.id);
                    table.ForeignKey(
                        name: "fk_oidc_authorizations_oidc_applications_application_id",
                        column: x => x.application_id,
                        principalTable: "oidc_applications",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "qts_awarded_emails_job_items",
                columns: table => new
                {
                    qts_awarded_emails_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trn = table.Column<string>(type: "character(7)", fixedLength: true, maxLength: 7, nullable: false),
                    email_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    personalization = table.Column<string>(type: "jsonb", nullable: false),
                    email_sent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_qts_awarded_emails_job_items", x => new { x.qts_awarded_emails_job_id, x.person_id });
                    table.ForeignKey(
                        name: "fk_qts_awarded_emails_job_items_qts_awarded_emails_jobs_qts_aw",
                        column: x => x.qts_awarded_emails_job_id,
                        principalTable: "qts_awarded_emails_jobs",
                        principalColumn: "qts_awarded_emails_job_id",
                        onDelete: ReferentialAction.Cascade);
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
                    withdrawal_indicator = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                name: "api_keys",
                columns: table => new
                {
                    api_key_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    application_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    expires = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_api_keys", x => x.api_key_id);
                    table.ForeignKey(
                        name: "fk_api_key_application_user",
                        column: x => x.application_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "processes",
                columns: table => new
                {
                    process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_type = table.Column<int>(type: "integer", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dqt_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dqt_user_name = table.Column<string>(type: "text", nullable: true),
                    person_ids = table.Column<HashSet<Guid>>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_processes", x => x.process_id);
                    table.ForeignKey(
                        name: "fk_processes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "trn_request_metadata",
                columns: table => new
                {
                    application_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    identity_verified = table.Column<bool>(type: "boolean", nullable: true),
                    email_address = table.Column<string>(type: "text", nullable: true),
                    work_email_address = table.Column<string>(type: "text", nullable: true),
                    one_login_user_subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    first_name = table.Column<string>(type: "text", nullable: true),
                    middle_name = table.Column<string>(type: "text", nullable: true),
                    last_name = table.Column<string>(type: "text", nullable: true),
                    previous_first_name = table.Column<string>(type: "text", nullable: true),
                    previous_middle_name = table.Column<string>(type: "text", nullable: true),
                    previous_last_name = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string[]>(type: "text[]", nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: false),
                    potential_duplicate = table.Column<bool>(type: "boolean", nullable: true),
                    national_insurance_number = table.Column<string>(type: "text", nullable: true),
                    gender = table.Column<int>(type: "integer", nullable: true),
                    address_line1 = table.Column<string>(type: "text", nullable: true),
                    address_line2 = table.Column<string>(type: "text", nullable: true),
                    address_line3 = table.Column<string>(type: "text", nullable: true),
                    city = table.Column<string>(type: "text", nullable: true),
                    postcode = table.Column<string>(type: "text", nullable: true),
                    country = table.Column<string>(type: "text", nullable: true),
                    trn_token = table.Column<string>(type: "text", nullable: true),
                    resolved_person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: true),
                    npq_working_in_educational_setting = table.Column<bool>(type: "boolean", nullable: true),
                    npq_application_id = table.Column<string>(type: "text", nullable: true),
                    npq_name = table.Column<string>(type: "text", nullable: true),
                    npq_training_provider = table.Column<string>(type: "text", nullable: true),
                    npq_evidence_file_id = table.Column<Guid>(type: "uuid", nullable: true),
                    npq_evidence_file_name = table.Column<string>(type: "text", nullable: true),
                    matches = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trn_request_metadata", x => new { x.application_user_id, x.request_id });
                    table.ForeignKey(
                        name: "fk_trn_request_metadata_application_users_application_user_id",
                        column: x => x.application_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "webhook_endpoints",
                columns: table => new
                {
                    webhook_endpoint_id = table.Column<Guid>(type: "uuid", nullable: false),
                    application_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    api_version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    cloud_event_types = table.Column<List<string>>(type: "text[]", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_webhook_endpoints", x => x.webhook_endpoint_id);
                    table.ForeignKey(
                        name: "fk_webhook_endpoints_application_users_application_user_id",
                        column: x => x.application_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "oidc_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    application_id = table.Column<Guid>(type: "uuid", nullable: true),
                    authorization_id = table.Column<Guid>(type: "uuid", nullable: true),
                    concurrency_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    creation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expiration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    payload = table.Column<string>(type: "text", nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    redemption_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reference_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_oidc_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_oidc_tokens_oidc_applications_application_id",
                        column: x => x.application_id,
                        principalTable: "oidc_applications",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_oidc_tokens_oidc_authorizations_authorization_id",
                        column: x => x.authorization_id,
                        principalTable: "oidc_authorizations",
                        principalColumn: "id");
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
                    withdrawal_indicator = table.Column<string>(type: "character(1)", fixedLength: true, maxLength: 1, nullable: true),
                    extract_date = table.Column<DateOnly>(type: "date", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    result = table.Column<int>(type: "integer", nullable: true),
                    key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
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

            migrationBuilder.CreateTable(
                name: "process_events",
                columns: table => new
                {
                    process_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    person_ids = table.Column<Guid[]>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_process_events", x => x.process_event_id);
                    table.ForeignKey(
                        name: "fk_process_events_process_process_id",
                        column: x => x.process_id,
                        principalTable: "processes",
                        principalColumn: "process_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "persons",
                columns: table => new
                {
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    merged_with_person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    trn = table.Column<string>(type: "character(7)", fixedLength: true, maxLength: 7, nullable: true),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, collation: "case_insensitive"),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, collation: "case_insensitive"),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, collation: "case_insensitive"),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    email_address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, collation: "case_insensitive"),
                    national_insurance_number = table.Column<string>(type: "character(9)", fixedLength: true, maxLength: 9, nullable: true),
                    gender = table.Column<int>(type: "integer", nullable: true),
                    induction_status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    induction_status_without_exemption = table.Column<int>(type: "integer", nullable: false),
                    induction_exemption_reason_ids = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    induction_exempt_without_reason = table.Column<bool>(type: "boolean", nullable: false),
                    induction_start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    induction_completed_date = table.Column<DateOnly>(type: "date", nullable: true),
                    induction_modified_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cpd_induction_modified_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cpd_induction_first_modified_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cpd_induction_cpd_modified_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    qts_date = table.Column<DateOnly>(type: "date", nullable: true),
                    qtls_status = table.Column<int>(type: "integer", nullable: false),
                    eyts_date = table.Column<DateOnly>(type: "date", nullable: true),
                    has_eyps = table.Column<bool>(type: "boolean", nullable: false),
                    pqts_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_by_tps = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    source_application_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source_trn_request_id = table.Column<string>(type: "character varying(100)", nullable: true),
                    allow_details_updates_from_source_application = table.Column<bool>(type: "boolean", nullable: false),
                    dqt_contact_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dqt_first_sync = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dqt_last_sync = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dqt_state = table.Column<int>(type: "integer", nullable: true),
                    dqt_created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dqt_modified_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dqt_first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, collation: "case_insensitive"),
                    dqt_middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, collation: "case_insensitive"),
                    dqt_last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, collation: "case_insensitive"),
                    dqt_induction_last_sync = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dqt_induction_modified_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dqt_allow_teacher_identity_sign_in_with_prohibitions = table.Column<bool>(type: "boolean", nullable: false),
                    date_of_death = table.Column<DateOnly>(type: "date", nullable: true),
                    last_names = table.Column<string[]>(type: "varchar[]", nullable: true, collation: "case_insensitive"),
                    names = table.Column<string[]>(type: "varchar[]", nullable: true, collation: "case_insensitive"),
                    national_insurance_numbers = table.Column<string[]>(type: "varchar[]", nullable: true, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_persons", x => x.person_id);
                    table.ForeignKey(
                        name: "fk_persons_induction_statuses_induction_status",
                        column: x => x.induction_status,
                        principalTable: "induction_statuses",
                        principalColumn: "induction_status",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_persons_persons_merged_with_person_id",
                        column: x => x.merged_with_person_id,
                        principalTable: "persons",
                        principalColumn: "person_id");
                    table.ForeignKey(
                        name: "fk_persons_trn_request_metadata_source_application_user_id_sou",
                        columns: x => new { x.source_application_user_id, x.source_trn_request_id },
                        principalTable: "trn_request_metadata",
                        principalColumns: new[] { "application_user_id", "request_id" });
                });

            migrationBuilder.CreateTable(
                name: "webhook_messages",
                columns: table => new
                {
                    webhook_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    webhook_endpoint_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cloud_event_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    cloud_event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    api_version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data = table.Column<JsonElement>(type: "jsonb", nullable: false),
                    next_delivery_attempt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivery_attempts = table.Column<List<DateTime>>(type: "timestamp with time zone[]", nullable: false),
                    delivery_errors = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_webhook_messages", x => x.webhook_message_id);
                    table.ForeignKey(
                        name: "fk_webhook_messages_webhook_endpoints_webhook_endpoint_id",
                        column: x => x.webhook_endpoint_id,
                        principalTable: "webhook_endpoints",
                        principalColumn: "webhook_endpoint_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "alerts",
                columns: table => new
                {
                    alert_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alert_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    details = table.Column<string>(type: "text", nullable: true),
                    external_link = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "integration_transaction_records",
                columns: table => new
                {
                    integration_transaction_record_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    row_data = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    failure_message = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    duplicate = table.Column<bool>(type: "boolean", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    integration_transaction_id = table.Column<long>(type: "bigint", nullable: true),
                    has_active_alert = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_integration_transaction_records", x => x.integration_transaction_record_id);
                    table.ForeignKey(
                        name: "fk_integration_transaction_records_persons_person_id",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "person_id");
                    table.ForeignKey(
                        name: "fk_integrationtransactionrecord_integrationtransaction",
                        column: x => x.integration_transaction_id,
                        principalTable: "integration_transactions",
                        principalColumn: "integration_transaction_id");
                });

            migrationBuilder.CreateTable(
                name: "notes",
                columns: table => new
                {
                    note_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    content_html = table.Column<string>(type: "text", nullable: true),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by_dqt_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by_dqt_user_name = table.Column<string>(type: "text", nullable: true),
                    updated_by_dqt_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_dqt_user_name = table.Column<string>(type: "text", nullable: true),
                    file_id = table.Column<Guid>(type: "uuid", nullable: true),
                    original_file_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notes", x => x.note_id);
                    table.ForeignKey(
                        name: "fk_notes_persons_person_id",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "person_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_notes_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "one_login_users",
                columns: table => new
                {
                    subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    first_one_login_sign_in = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_one_login_sign_in = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    first_sign_in = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_sign_in = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    verified_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    verification_route = table.Column<int>(type: "integer", nullable: true),
                    verified_names = table.Column<string>(type: "jsonb", nullable: true),
                    verified_dates_of_birth = table.Column<string>(type: "jsonb", nullable: true),
                    last_core_identity_vc = table.Column<string>(type: "jsonb", nullable: true),
                    match_route = table.Column<int>(type: "integer", nullable: true),
                    matched_attributes = table.Column<string>(type: "jsonb", nullable: true),
                    verified_by_application_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_one_login_users", x => x.subject);
                    table.ForeignKey(
                        name: "fk_one_login_users_application_users_verified_by_application_u",
                        column: x => x.verified_by_application_user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "fk_one_login_users_persons_person_id",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "person_id");
                });

            migrationBuilder.CreateTable(
                name: "previous_names",
                columns: table => new
                {
                    previous_name_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, collation: "case_insensitive"),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, collation: "case_insensitive"),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, collation: "case_insensitive"),
                    dqt_previous_name_ids = table.Column<Guid[]>(type: "uuid[]", nullable: true),
                    dqt_audit_id = table.Column<Guid>(type: "uuid", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "qualifications",
                columns: table => new
                {
                    qualification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    qualification_type = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mq_provider_id = table.Column<Guid>(type: "uuid", nullable: true),
                    mq_specialism = table.Column<int>(type: "integer", nullable: true),
                    mq_status = table.Column<int>(type: "integer", nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    dqt_qualification_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dqt_mq_establishment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dqt_mq_establishment_value = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    dqt_specialism_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dqt_specialism_value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    route_to_professional_status_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source_application_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source_application_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: true),
                    holds_from = table.Column<DateOnly>(type: "date", nullable: true),
                    training_start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    training_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    training_subject_ids = table.Column<Guid[]>(type: "uuid[]", nullable: true),
                    training_age_specialism_type = table.Column<int>(type: "integer", nullable: true),
                    training_age_specialism_range_from = table.Column<int>(type: "integer", nullable: true),
                    training_age_specialism_range_to = table.Column<int>(type: "integer", nullable: true),
                    training_country_id = table.Column<string>(type: "character varying(10)", nullable: true),
                    training_provider_id = table.Column<Guid>(type: "uuid", nullable: true),
                    exempt_from_induction = table.Column<bool>(type: "boolean", nullable: true),
                    exempt_from_induction_due_to_qts_date = table.Column<bool>(type: "boolean", nullable: true),
                    degree_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dqt_teacher_status_name = table.Column<string>(type: "text", nullable: true),
                    dqt_teacher_status_value = table.Column<string>(type: "text", nullable: true),
                    dqt_early_years_status_name = table.Column<string>(type: "text", nullable: true),
                    dqt_early_years_status_value = table.Column<string>(type: "text", nullable: true),
                    dqt_initial_teacher_training_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dqt_qts_registration_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dqt_age_range_from = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    dqt_age_range_to = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_qualifications", x => x.qualification_id);
                    table.ForeignKey(
                        name: "fk_qualifications_countries_training_country_id",
                        column: x => x.training_country_id,
                        principalTable: "countries",
                        principalColumn: "country_id");
                    table.ForeignKey(
                        name: "fk_qualifications_degree_types_degree_type_id",
                        column: x => x.degree_type_id,
                        principalTable: "degree_types",
                        principalColumn: "degree_type_id");
                    table.ForeignKey(
                        name: "fk_qualifications_mandatory_qualification_provider",
                        column: x => x.mq_provider_id,
                        principalTable: "mandatory_qualification_providers",
                        principalColumn: "mandatory_qualification_provider_id");
                    table.ForeignKey(
                        name: "fk_qualifications_person",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "person_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_qualifications_route_to_professional_status_types_route_to_",
                        column: x => x.route_to_professional_status_type_id,
                        principalTable: "route_to_professional_status_types",
                        principalColumn: "route_to_professional_status_type_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_qualifications_training_providers_training_provider_id",
                        column: x => x.training_provider_id,
                        principalTable: "training_providers",
                        principalColumn: "training_provider_id");
                    table.ForeignKey(
                        name: "fk_qualifications_users_source_application_user_id",
                        column: x => x.source_application_user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "support_tasks",
                columns: table => new
                {
                    support_task_reference = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    support_task_type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    one_login_user_subject = table.Column<string>(type: "text", nullable: true),
                    person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    trn_request_application_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    trn_request_id = table.Column<string>(type: "character varying(100)", nullable: true),
                    data = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_support_tasks", x => x.support_task_reference);
                    table.ForeignKey(
                        name: "fk_support_tasks_person",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "person_id");
                    table.ForeignKey(
                        name: "fk_support_tasks_support_task_types_support_task_type",
                        column: x => x.support_task_type,
                        principalTable: "support_task_types",
                        principalColumn: "support_task_type",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_support_tasks_trn_request_metadata_trn_request_application_",
                        columns: x => new { x.trn_request_application_user_id, x.trn_request_id },
                        principalTable: "trn_request_metadata",
                        principalColumns: new[] { "application_user_id", "request_id" });
                });

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
                    withdrawal_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    person_email_address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    employer_postcode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    employer_email_address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
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

            migrationBuilder.InsertData(
                table: "alert_categories",
                columns: new[] { "alert_category_id", "display_order", "name" },
                values: new object[,]
                {
                    { new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), 4, "GTC Decision" },
                    { new Guid("0ae0707b-1503-477d-bc0f-1505ed95dbdf"), 2, "Failed induction" },
                    { new Guid("227b75e5-bb98-496c-8860-1baea37aa5c6"), 12, "TRA Decision (SoS)" },
                    { new Guid("38df5a00-94ab-486f-8905-d5b2eac04000"), 10, "Section 128 (SoS)" },
                    { new Guid("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"), 5, "GTC Prohibition from teaching" },
                    { new Guid("768c9eb4-355b-4491-bb20-67eb59a97579"), 3, "Flag" },
                    { new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), 6, "GTC Restriction" },
                    { new Guid("b2b19019-b165-47a3-8745-3297ff152581"), 7, "Prohibition from teaching" },
                    { new Guid("cbf7633f-3904-407d-8371-42a473fa641f"), 9, "Restriction" },
                    { new Guid("e4057fc2-a010-42a9-8cb2-7dcc5c9b5fa7"), 11, "SoS Restriction" },
                    { new Guid("e8a9ee91-bf7f-4f70-bc66-a644d522384e"), 8, "Restricted/DBS" },
                    { new Guid("ee78d44d-abf8-44a9-b22b-87a821f8d3c9"), 1, "EEA Decision" }
                });

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "country_id", "citizen_names", "name", "official_name" },
                values: new object[,]
                {
                    { "AD", "Andorran", "Andorra", "The Principality of Andorra" },
                    { "AE", "Citizen of the United Arab Emirates", "United Arab Emirates", "The United Arab Emirates" },
                    { "AF", "Afghan", "Afghanistan", "The Islamic Republic of Afghanistan" },
                    { "AG", "Citizen of Antigua and Barbuda", "Antigua and Barbuda", "Antigua and Barbuda" },
                    { "AI", "Anguillan", "Anguilla", "Anguilla" },
                    { "AL", "Albanian", "Albania", "The Republic of Albania" },
                    { "AM", "Armenian", "Armenia", "The Republic of Armenia" },
                    { "AO", "Angolan", "Angola", "The Republic of Angola" },
                    { "AR", "Argentine", "Argentina", "The Argentine Republic" },
                    { "AT", "Austrian", "Austria", "The Republic of Austria" },
                    { "AU", "Australian", "Australia", "The Commonwealth of Australia" },
                    { "AZ", "Azerbaijani", "Azerbaijan", "The Republic of Azerbaijan" },
                    { "BA", "Citizen of Bosnia and Herzegovina", "Bosnia and Herzegovina", "Bosnia and Herzegovina" },
                    { "BAT", "Not applicable", "British Antarctic Territory", "British Antarctic Territory" },
                    { "BB", "Barbadian", "Barbados", "Barbados" },
                    { "BD", "Bangladeshi", "Bangladesh", "The People's Republic of Bangladesh" },
                    { "BE", "Belgian", "Belgium", "The Kingdom of Belgium" },
                    { "BF", "Burkinan", "Burkina Faso", "Burkina Faso" },
                    { "BG", "Bulgarian", "Bulgaria", "The Republic of Bulgaria" },
                    { "BH", "Bahraini", "Bahrain", "The Kingdom of Bahrain" },
                    { "BI", "Burundian", "Burundi", "The Republic of Burundi" },
                    { "BJ", "Beninese", "Benin", "The Republic of Benin" },
                    { "BM", "Bermudan", "Bermuda", "Bermuda" },
                    { "BN", "Bruneian", "Brunei", "Brunei Darussalam" },
                    { "BO", "Bolivian", "Bolivia", "The Plurinational State of Bolivia" },
                    { "BR", "Brazilian", "Brazil", "The Federative Republic of Brazil" },
                    { "BS", "Bahamian", "The Bahamas", "The Commonwealth of The Bahamas" },
                    { "BT", "Bhutanese", "Bhutan", "The Kingdom of Bhutan" },
                    { "BW", "Botswanan", "Botswana", "The Republic of Botswana" },
                    { "BY", "Belarusian", "Belarus", "The Republic of Belarus" },
                    { "BZ", "Belizean", "Belize", "Belize" },
                    { "CA", "Canadian", "Canada", "Canada" },
                    { "CD", "Congolese (DRC)", "Congo (Democratic Republic)", "The Democratic Republic of the Congo" },
                    { "CF", "Central African", "Central African Republic", "The Central African Republic" },
                    { "CG", "Congolese (Republic of the Congo)", "Congo", "The Republic of the Congo" },
                    { "CH", "Swiss", "Switzerland", "The Swiss Confederation" },
                    { "CI", "Ivorian", "Ivory Coast", "The Republic of Côte D’Ivoire" },
                    { "CL", "Chilean", "Chile", "The Republic of Chile" },
                    { "CM", "Cameroonian", "Cameroon", "The Republic of Cameroon" },
                    { "CN", "Chinese", "China", "The People's Republic of China" },
                    { "CO", "Colombian", "Colombia", "The Republic of Colombia" },
                    { "CR", "Costa Rican", "Costa Rica", "The Republic of Costa Rica" },
                    { "CU", "Cuban", "Cuba", "The Republic of Cuba" },
                    { "CV", "Cape Verdean", "Cape Verde", "The Republic of Cabo Verde" },
                    { "CY", "Cypriot", "Cyprus", "The Republic of Cyprus" },
                    { "CZ", "Czech", "Czechia", "The Czech Republic" },
                    { "DE", "German", "Germany", "The Federal Republic of Germany" },
                    { "DJ", "Djiboutian", "Djibouti", "The Republic of Djibouti" },
                    { "DK", "Danish", "Denmark", "The Kingdom of Denmark" },
                    { "DM", "Dominican", "Dominica", "The Commonwealth of Dominica" },
                    { "DO", "Citizen of the Dominican Republic", "Dominican Republic", "The Dominican Republic" },
                    { "DZ", "Algerian", "Algeria", "The People's Democratic Republic of Algeria" },
                    { "EC", "Ecuadorean", "Ecuador", "The Republic of Ecuador" },
                    { "EE", "Estonian", "Estonia", "The Republic of Estonia" },
                    { "EG", "Egyptian", "Egypt", "The Arab Republic of Egypt" },
                    { "ER", "Eritrean", "Eritrea", "The State of Eritrea" },
                    { "ES", "Spanish", "Spain", "The Kingdom of Spain" },
                    { "ET", "Ethiopian", "Ethiopia", "The Federal Democratic Republic of Ethiopia" },
                    { "FI", "Finnish", "Finland", "The Republic of Finland" },
                    { "FJ", "Fijian", "Fiji", "The Republic of Fiji" },
                    { "FK", "Falkland Islander", "Falkland Islands", "Falkland Islands" },
                    { "FM", "Micronesian", "Federated States of Micronesia", "Federated States of Micronesia" },
                    { "FR", "French", "France", "The French Republic" },
                    { "GA", "Gabonese", "Gabon", "The Gabonese Republic" },
                    { "GB", "Briton, British", "United Kingdom", "The United Kingdom of Great Britain and Northern Ireland" },
                    { "GB-CYM", "Briton, British", "Wales", "Wales" },
                    { "GB-ENG", "Briton, British", "England", "England" },
                    { "GB-NIR", "Briton, British", "Northen Ireland", "Northen Ireland" },
                    { "GB-SCT", "Briton, British", "Scotland", "Scotland" },
                    { "GB-WLS", "Briton, British", "Wales", "Wales" },
                    { "GD", "Grenadian", "Grenada", "Grenada" },
                    { "GE", "Georgian", "Georgia", "Georgia" },
                    { "GG", "Guernseyman/Guernseywoman or Giernési, Ridunian, Sarkee as appropriate", "Guernsey, Alderney, Sark", "Bailiwick of Guernsey" },
                    { "GH", "Ghanaian", "Ghana", "The Republic of Ghana" },
                    { "GI", "Gibraltarian", "Gibraltar", "Gibraltar" },
                    { "GM", "Gambian", "The Gambia", "The Republic of The Gambia" },
                    { "GN", "Guinean", "Guinea", "The Republic of Guinea" },
                    { "GQ", "Equatorial Guinean", "Equatorial Guinea", "The Republic of Equatorial Guinea" },
                    { "GR", "Greek", "Greece", "The Hellenic Republic" },
                    { "GS", "Not applicable", "South Georgia and South Sandwich Islands", "South Georgia and the South Sandwich Islands" },
                    { "GT", "Guatemalan", "Guatemala", "The Republic of Guatemala" },
                    { "GW", "Citizen of Guinea-Bissau", "Guinea-Bissau", "The Republic of Guinea-Bissau" },
                    { "GY", "Guyanese", "Guyana", "The Co-operative Republic of Guyana" },
                    { "HK", "Hongkonger or Cantonese", "Hong Kong", "Hong Kong Special Administrative Region of the People's Republic of China" },
                    { "HN", "Honduran", "Honduras", "The Republic of Honduras" },
                    { "HR", "Croatian", "Croatia", "The Republic of Croatia" },
                    { "HT", "Haitian", "Haiti", "The Republic of Haiti" },
                    { "HU", "Hungarian", "Hungary", "Hungary" },
                    { "ID", "Indonesian", "Indonesia", "The Republic of Indonesia" },
                    { "IE", "Irish", "Ireland", "Ireland" },
                    { "IL", "Israeli", "Israel", "The State of Israel" },
                    { "IM", "Manxman/Manxwoman or Manx", "Isle of Man", "Isle of Man" },
                    { "IN", "Indian", "India", "The Republic of India" },
                    { "IO", "Not applicable", "British Indian Ocean Territory", "The British Indian Ocean Territory" },
                    { "IQ", "Iraqi", "Iraq", "The Republic of Iraq" },
                    { "IR", "Iranian", "Iran", "The Islamic Republic of Iran" },
                    { "IS", "Icelandic", "Iceland", "Iceland" },
                    { "IT", "Italian", "Italy", "The Italian Republic" },
                    { "JE", "Jerseyman/Jerseywoman", "Jersey", "Bailiwick of Jersey" },
                    { "JM", "Jamaican", "Jamaica", "Jamaica" },
                    { "JO", "Jordanian", "Jordan", "The Hashemite Kingdom of Jordan" },
                    { "JP", "Japanese", "Japan", "Japan" },
                    { "KE", "Kenyan", "Kenya", "The Republic of Kenya" },
                    { "KG", "Kyrgyz", "Kyrgyzstan", "The Kyrgyz Republic" },
                    { "KH", "Cambodian", "Cambodia", "The Kingdom of Cambodia" },
                    { "KI", "Citizen of Kiribati", "Kiribati", "The Republic of Kiribati" },
                    { "KM", "Comoran", "Comoros", "The Union of the Comoros" },
                    { "KN", "Citizen of St Christopher (St Kitts) and Nevis", "St Kitts and Nevis", "The Federation of Saint Christopher and Nevis" },
                    { "KP", "North Korean", "North Korea", "The Democratic People's Republic of Korea" },
                    { "KR", "South Korean", "South Korea", "The Republic of Korea" },
                    { "KW", "Kuwaiti", "Kuwait", "The State of Kuwait" },
                    { "KY", "Cayman Islander, Caymanian", "Cayman Islands", "Cayman Islands" },
                    { "KZ", "Kazakh", "Kazakhstan", "The Republic of Kazakhstan" },
                    { "LA", "Lao", "Laos", "The Lao People's Democratic Republic" },
                    { "LB", "Lebanese", "Lebanon", "The Lebanese Republic" },
                    { "LC", "St Lucian", "St Lucia", "Saint Lucia" },
                    { "LI", "Liechtenstein citizen", "Liechtenstein", "The Principality of Liechtenstein" },
                    { "LK", "Sri Lankan", "Sri Lanka", "The Democratic Socialist Republic of Sri Lanka" },
                    { "LR", "Liberian", "Liberia", "The Republic of Liberia" },
                    { "LS", "Citizen of Lesotho", "Lesotho", "The Kingdom of Lesotho" },
                    { "LT", "Lithuanian", "Lithuania", "The Republic of Lithuania" },
                    { "LU", "Luxembourger", "Luxembourg", "The Grand Duchy of Luxembourg" },
                    { "LV", "Latvian", "Latvia", "The Republic of Latvia" },
                    { "LY", "Libyan", "Libya", "State of Libya" },
                    { "MA", "Moroccan", "Morocco", "The Kingdom of Morocco" },
                    { "MC", "Monegasque", "Monaco", "The Principality of Monaco" },
                    { "MD", "Moldovan", "Moldova", "The Republic of Moldova" },
                    { "ME", "Montenegrin", "Montenegro", "Montenegro" },
                    { "MG", "Citizen of Madagascar", "Madagascar", "The Republic of Madagascar" },
                    { "MH", "Marshallese", "Marshall Islands", "The Republic of the Marshall Islands" },
                    { "MK", "Macedonian", "North Macedonia", "Republic of North Macedonia" },
                    { "ML", "Malian", "Mali", "The Republic of Mali" },
                    { "MM", "Citizen of Myanmar", "Myanmar (Burma)", "The Republic of the Union of Myanmar" },
                    { "MN", "Mongolian", "Mongolia", "Mongolia" },
                    { "MR", "Mauritanian", "Mauritania", "The Islamic Republic of Mauritania" },
                    { "MS", "Montserratian", "Montserrat", "Montserrat" },
                    { "MT", "Maltese", "Malta", "The Republic of Malta" },
                    { "MU", "Mauritian", "Mauritius", "The Republic of Mauritius" },
                    { "MV", "Maldivian", "Maldives", "The Republic of Maldives" },
                    { "MW", "Malawian", "Malawi", "The Republic of Malawi" },
                    { "MX", "Mexican", "Mexico", "The United Mexican States" },
                    { "MY", "Malaysian", "Malaysia", "Malaysia" },
                    { "MZ", "Mozambican", "Mozambique", "The Republic of Mozambique" },
                    { "NA", "Namibian", "Namibia", "The Republic of Namibia" },
                    { "NE", "Nigerien", "Niger", "The Republic of Niger" },
                    { "NG", "Nigerian", "Nigeria", "The Federal Republic of Nigeria" },
                    { "NI", "Nicaraguan", "Nicaragua", "The Republic of Nicaragua" },
                    { "NL", "Dutch", "Netherlands", "The Kingdom of the Netherlands" },
                    { "NO", "Norwegian", "Norway", "The Kingdom of Norway" },
                    { "NP", "Nepalese", "Nepal", "Nepal" },
                    { "NR", "Nauruan", "Nauru", "The Republic of Nauru" },
                    { "NZ", "New Zealander", "New Zealand", "New Zealand" },
                    { "OM", "Omani", "Oman", "The Sultanate of Oman" },
                    { "PA", "Panamanian", "Panama", "The Republic of Panama" },
                    { "PE", "Peruvian", "Peru", "The Republic of Peru" },
                    { "PG", "Papua New Guinean", "Papua New Guinea", "The Independent State of Papua New Guinea" },
                    { "PH", "Filipino", "Philippines", "The Republic of the Philippines" },
                    { "PK", "Pakistani", "Pakistan", "The Islamic Republic of Pakistan" },
                    { "PL", "Polish", "Poland", "The Republic of Poland" },
                    { "PN", "Pitcairn Islander or Pitcairner", "Pitcairn, Henderson, Ducie and Oeno Islands", "Pitcairn, Henderson, Ducie and Oeno Islands" },
                    { "PT", "Portuguese", "Portugal", "The Portuguese Republic" },
                    { "PW", "Palauan", "Palau", "The Republic of Palau" },
                    { "PY", "Paraguayan", "Paraguay", "The Republic of Paraguay" },
                    { "QA", "Qatari", "Qatar", "The State of Qatar" },
                    { "RO", "Romanian", "Romania", "Romania" },
                    { "RS", "Serbian", "Serbia", "The Republic of Serbia" },
                    { "RU", "Russian", "Russia", "The Russian Federation" },
                    { "RW", "Rwandan", "Rwanda", "The Republic of Rwanda" },
                    { "SA", "Saudi Arabian", "Saudi Arabia", "The Kingdom of Saudi Arabia" },
                    { "SB", "Solomon Islander", "Solomon Islands", "Solomon Islands" },
                    { "SC", "Citizen of Seychelles", "Seychelles", "The Republic of Seychelles" },
                    { "SD", "Sudanese", "Sudan", "The Republic of the Sudan" },
                    { "SE", "Swedish", "Sweden", "The Kingdom of Sweden" },
                    { "SG", "Singaporean", "Singapore", "The Republic of Singapore" },
                    { "SH", "St Helenian or Tristanian as appropriate. Ascension has no indigenous population", "St Helena, Ascension and Tristan da Cunha", "St Helena, Ascension and Tristan da Cunha" },
                    { "SI", "Slovenian", "Slovenia", "The Republic of Slovenia" },
                    { "SK", "Slovak", "Slovakia", "The Slovak Republic" },
                    { "SL", "Sierra Leonean", "Sierra Leone", "The Republic of Sierra Leone" },
                    { "SM", "San Marinese", "San Marino", "The Republic of San Marino" },
                    { "SN", "Senegalese", "Senegal", "The Republic of Senegal" },
                    { "SO", "Somali", "Somalia", "Federal Republic of Somalia" },
                    { "SR", "Surinamese", "Suriname", "The Republic of Suriname" },
                    { "SS", "South Sudanese", "South Sudan", "The Republic of South Sudan" },
                    { "ST", "Sao Tomean", "Sao Tome and Principe", "The Democratic Republic of Sao Tome and Principe" },
                    { "SV", "Salvadorean", "El Salvador", "The Republic of El Salvador" },
                    { "SY", "Syrian", "Syria", "The Syrian Arab Republic" },
                    { "SZ", "Swazi", "Eswatini", "Kingdom of Eswatini" },
                    { "TC", "Turks and Caicos Islander", "Turks and Caicos Islands", "Turks and Caicos Islands" },
                    { "TD", "Chadian", "Chad", "The Republic of Chad" },
                    { "TG", "Togolese", "Togo", "The Togolese Republic" },
                    { "TH", "Thai", "Thailand", "The Kingdom of Thailand" },
                    { "TJ", "Tajik", "Tajikistan", "The Republic of Tajikistan" },
                    { "TL", "East Timorese", "East Timor", "The Democratic Republic of Timor-Leste" },
                    { "TM", "Turkmen", "Turkmenistan", "Turkmenistan" },
                    { "TN", "Tunisian", "Tunisia", "Republic of Tunisia" },
                    { "TO", "Tongan", "Tonga", "The Kingdom of Tonga" },
                    { "TR", "Turk, Turkish", "Turkey", "Republic of Türkiye" },
                    { "TT", "Trinidad and Tobago citizen", "Trinidad and Tobago", "The Republic of Trinidad and Tobago" },
                    { "TV", "Tuvaluan", "Tuvalu", "Tuvalu" },
                    { "TZ", "Tanzanian", "Tanzania", "The United Republic of Tanzania" },
                    { "UA", "Ukrainian", "Ukraine", "Ukraine" },
                    { "UG", "Ugandan", "Uganda", "The Republic of Uganda" },
                    { "US", "American", "United States", "The United States of America" },
                    { "UY", "Uruguayan", "Uruguay", "The Oriental Republic of Uruguay" },
                    { "UZ", "Uzbek", "Uzbekistan", "The Republic of Uzbekistan" },
                    { "VA", "Vatican citizen", "Vatican City", "Vatican City State" },
                    { "VC", "Vincentian", "St Vincent", "Saint Vincent and the Grenadines" },
                    { "VE", "Venezuelan", "Venezuela", "The Bolivarian Republic of Venezuela" },
                    { "VG", "British Virgin Islander", "British Virgin Islands", "The Virgin Islands" },
                    { "VN", "Vietnamese", "Vietnam", "The Socialist Republic of Viet Nam" },
                    { "VU", "Citizen of Vanuatu", "Vanuatu", "The Republic of Vanuatu" },
                    { "WS", "Samoan", "Samoa", "The Independent State of Samoa" },
                    { "XK", "Kosovan", "Kosovo", "The Republic of Kosovo" },
                    { "XQZ", "Not applicable", "Akrotiri", "Akrotiri" },
                    { "XXD", "Not applicable", "Dhekelia", "Dhekelia" },
                    { "YE", "Yemeni", "Yemen", "The Republic of Yemen" },
                    { "ZA", "South African", "South Africa", "The Republic of South Africa" },
                    { "ZM", "Zambian", "Zambia", "The Republic of Zambia" },
                    { "ZW", "Zimbabwean", "Zimbabwe", "The Republic of Zimbabwe" }
                });

            migrationBuilder.InsertData(
                table: "degree_types",
                columns: new[] { "degree_type_id", "is_active", "name" },
                values: new object[,]
                {
                    { new Guid("02e4f052-bd3b-490c-bea0-bd390bc5b227"), true, "BEng (Hons)/Education" },
                    { new Guid("1fcd0543-14d1-4866-b961-2812239eec06"), true, "BA (Hons) Combined Studies/Education of the Deaf" },
                    { new Guid("2f7a914f-f95f-421a-a55e-60ed88074cf2"), true, "Postgraduate Art Teachers Certificate" },
                    { new Guid("311ef3a9-6aba-4314-acf8-4bba46aebe9e"), true, "Graduate Certificate in Education" },
                    { new Guid("35d04fbb-c19b-4cd9-8fa6-39d90883a52a"), true, "BSc" },
                    { new Guid("40a85dd0-8512-438e-8040-649d7d677d07"), true, "Postgraduate Certificate in Education" },
                    { new Guid("4c0578b6-e9af-4c98-a3bc-038343b1436a"), true, "Certificate in Education (FE)" },
                    { new Guid("4ec0a016-07eb-47b4-8cdd-e276945d401e"), true, "Qualification gained in Europe" },
                    { new Guid("54f72259-23b2-4d79-a6ca-c185084c903f"), true, "PGCE (Articled Teachers Scheme)" },
                    { new Guid("63d80489-ee3d-43af-8c4a-1d6ae0d65f68"), true, "Postgraduate Diploma in Education" },
                    { new Guid("6d07695e-5b5b-4dd4-997c-420e4424255c"), true, "Graduate Diploma" },
                    { new Guid("72dbd225-6a7e-42af-b918-cf284bccaeef"), true, "BSc/Education" },
                    { new Guid("7330e2f5-dd02-4498-9b7c-5cf99d7d0cab"), true, "BSc/Certificate in Education" },
                    { new Guid("7471551d-132e-4c5d-82cc-a41190f01245"), true, "Teachers Certificate FE" },
                    { new Guid("7c703efb-a5d3-41d3-b243-ee8974695dd8"), true, "Professional Graduate Diploma in Education" },
                    { new Guid("826f6cc9-e5f8-4ce7-a5ee-6194d19f7e22"), true, "BA with Intercalated PGCE" },
                    { new Guid("84e541d5-d55a-4d44-bc52-983322c1453f"), true, "BA Education" },
                    { new Guid("85ab05c8-be3a-4a72-9d04-9efc30d87289"), true, "BTech/Education" },
                    { new Guid("8d0440f2-f731-4ac2-b49c-927af903bf59"), true, "Postgraduate Art Teachers Diploma" },
                    { new Guid("969c89e7-35b8-43d8-be07-17ef76c3b4bf"), true, "BA" },
                    { new Guid("984af9ff-bb42-48ac-a634-f2c4954c8158"), true, "BTech (Hons)/Education" },
                    { new Guid("9959e914-f4f4-44cd-909f-e170a0f1ac42"), true, "BSc (Hons)" },
                    { new Guid("9b35bdfa-cbd5-44fd-a45a-6167e7559de7"), true, "BEd (Hons)" },
                    { new Guid("9cf31754-5ac5-46a1-99e5-5c98cba1b881"), true, "Unknown" },
                    { new Guid("9f4af7a8-14a5-4b34-af72-dc04c5245fc7"), true, "BSc (Hons) with Intercalated PGCE" },
                    { new Guid("b02914fe-3a30-4f7c-94ec-0cd87a75834d"), true, "Teachers Certificate" },
                    { new Guid("b44e02b1-7257-4609-a9e5-46ed72c91b98"), true, "Certificate in Education" },
                    { new Guid("b7b0635a-22c3-41e3-a420-77b9b58c51cd"), true, "BEd" },
                    { new Guid("b96d4ad9-6da0-4dad-a9e4-e35b2a0838eb"), true, "BA Combined Studies/Education of the Deaf" },
                    { new Guid("b9ef569f-fb23-4f31-842e-a0d940d911be"), true, "Graduate Certificate in Science and Education" },
                    { new Guid("bc6c1f17-26a5-4987-9d50-2615e138e281"), true, "Degree Equivalent (this will include foreign qualifications)" },
                    { new Guid("c06660d3-8964-40d0-985f-80b25eced995"), true, "BA (Hons) with Intercalated PGCE" },
                    { new Guid("c584eb2f-1419-4870-a230-5d81ae9b5f77"), true, "Postgraduate Certificate in Education (Further Education)" },
                    { new Guid("d82637a0-33ed-4181-b00b-9d53e7853552"), true, "Graduate Certificate in Mathematics and Education" },
                    { new Guid("d8e267d2-ed85-4eee-8119-45d0c6cc5f6b"), true, "Professional Graduate Certificate in Education" },
                    { new Guid("dba69141-4101-4e05-80e0-524e3967d589"), true, "Undergraduate Master of Teaching" },
                    { new Guid("dbb7c27b-8a27-4a94-908d-4b4404acebd5"), true, "BA (Hons)" },
                    { new Guid("eb04bde4-9a7b-4c68-b7e1-a9254e0e7467"), true, "BA Education Certificate" },
                    { new Guid("fc85c7e2-7fd7-4585-8c37-c29852e6027f"), true, "Degree" }
                });

            migrationBuilder.InsertData(
                table: "induction_exemption_reasons",
                columns: new[] { "induction_exemption_reason_id", "is_active", "name", "route_implicit_exemption", "route_only_exemption" },
                values: new object[,]
                {
                    { new Guid("0997ab13-7412-4560-8191-e51ed4d58d2a"), true, "Qualified through Further Education route between 1 Sep 2001 and 1 Sep 2004", false, false },
                    { new Guid("15014084-2d8d-4f51-9198-b0e1881f8896"), true, "Qualified between 07 May 1999 and 01 Apr 2003. First post was in Wales and lasted a minimum of two terms.", false, false },
                    { new Guid("204f86eb-0383-40eb-b793-6fccb76ecee2"), true, "Exempt - Data Loss/Error Criteria", false, false },
                    { new Guid("243b21a8-0be4-4af5-8874-85944357e7f8"), true, "Passed induction in Jersey", false, false },
                    { new Guid("3471ab35-e6e4-4fa9-a72b-b8bd113df591"), true, "Passed induction in Northern Ireland", false, false },
                    { new Guid("35caa6a3-49f2-4a63-bd5a-2ba5fa9dc5db"), true, "Exempt through QTLS status provided they maintain membership of The Society of Education and Training", true, true },
                    { new Guid("39550fa9-3147-489d-b808-4feea7f7f979"), true, "Passed induction in Wales", false, false },
                    { new Guid("42bb7bbc-a92c-4886-b319-3c1a5eac319a"), true, "Registered teacher with at least 2 years full-time teaching experience", false, false },
                    { new Guid("4c97e211-10d2-4c63-8da9-b0fcebe7f2f9"), true, "Overseas Trained Teacher", false, true },
                    { new Guid("5a80cee8-98a8-426b-8422-b0e81cb49b36"), true, "Qualified before 07 May 2000", false, false },
                    { new Guid("7d17d904-c1c6-451b-9e09-031314bd35f7"), true, "Passed induction in Service Children's Education schools in Germany or Cyprus", false, false },
                    { new Guid("a112e691-1694-46a7-8f33-5ec5b845c181"), true, "Has, or is eligible for, full registration in Scotland", false, false },
                    { new Guid("a5faff9f-29ce-4a6b-a7b8-0c1f57f15920"), true, "Exempt", false, false },
                    { new Guid("a751494a-7e7a-4836-96cb-00b9ed6e1b5f"), true, "Passed probationary period in Gibraltar", false, false },
                    { new Guid("e5c3847d-8fb6-4b31-8726-812392da8c5c"), true, "Passed induction in Isle of Man", false, false },
                    { new Guid("e7118bab-c2b1-4fe8-ad3f-4095d73f5b85"), true, "Qualified through EEA mutual recognition route", false, false },
                    { new Guid("fea2db23-93e0-49af-96fd-83c815c17c0b"), true, "Passed induction in Guernsey", false, false }
                });

            migrationBuilder.InsertData(
                table: "induction_statuses",
                columns: new[] { "induction_status", "name" },
                values: new object[,]
                {
                    { 0, "none" },
                    { 1, "required to complete" },
                    { 2, "exempt" },
                    { 3, "in progress" },
                    { 4, "passed" },
                    { 5, "failed" },
                    { 6, "failed in Wales" }
                });

            migrationBuilder.InsertData(
                table: "mandatory_qualification_providers",
                columns: new[] { "mandatory_qualification_provider_id", "name" },
                values: new object[,]
                {
                    { new Guid("0c30f666-647c-4ea8-8883-0fc6010b56be"), "University of Oxford/Oxford Polytechnic" },
                    { new Guid("26204149-349c-4ad6-9466-bb9b83723eae"), "Liverpool John Moores University" },
                    { new Guid("374dceb8-8224-45b8-b7dc-a6b0282b1065"), "Bristol Polytechnic" },
                    { new Guid("3fc648a7-18e4-49e7-8a4b-1612616b72d5"), "University of London" },
                    { new Guid("707d58ca-1953-413b-9a46-41e9b0be885e"), "University of Hertfordshire" },
                    { new Guid("89f9a1aa-3d68-4985-a4ce-403b6044c18c"), "University of Leeds" },
                    { new Guid("aa5c300e-3b7c-456c-8183-3520b3d55dca"), "University of Manchester" },
                    { new Guid("aec32252-ef25-452e-a358-34a04e03369c"), "University of Newcastle-upon-Tyne" },
                    { new Guid("d0e6d54c-5e90-438a-945d-f97388c2b352"), "University of Cambridge" },
                    { new Guid("d4fc958b-21de-47ec-9f03-36ae237a1b11"), "University College, Swansea" },
                    { new Guid("d9ee7054-7fde-4cfd-9a5e-4b99511d1b3d"), "University of Plymouth" },
                    { new Guid("e28ea41d-408d-4c89-90cc-8b9b04ac68f5"), "University of Birmingham" },
                    { new Guid("f417e73e-e2ad-40eb-85e3-55865be7f6be"), "Mary Hare School / University of Hertfordshire" },
                    { new Guid("fbf22e04-b274-4c80-aba8-79fb6a7a32ce"), "University of Edinburgh" }
                });

            migrationBuilder.InsertData(
                table: "route_to_professional_status_types",
                columns: new[] { "route_to_professional_status_type_id", "degree_type_required", "holds_from_required", "induction_exemption_reason_id", "induction_exemption_required", "is_active", "name", "professional_status_type", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[,]
                {
                    { new Guid("002f7c96-f6ae-4e67-8f8b-d2f1c1317273"), 0, 1, null, 2, false, "ProfGCE ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("02a2135c-ac34-4481-a293-8a00aab7ee69"), 0, 1, null, 2, false, "PGCE ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("10078157-e8c3-42f7-a050-d8b802e83f7b"), 1, 1, null, 2, true, "HEI", 0, 0, 0, 0, 1, 0, 0 },
                    { new Guid("11b66de5-4670-4c82-86aa-20e42df723b7"), 1, 1, null, 2, true, "Early Years Teacher Degree Apprenticeship", 1, 1, 1, 0, 0, 0, 1 },
                    { new Guid("12a742c3-1cd4-43b7-a2fa-1000bd4cc373"), 1, 1, null, 2, true, "School Direct Training Programme Salaried", 0, 0, 0, 0, 1, 0, 0 },
                    { new Guid("1c626be0-5a64-47ec-8349-75008f52bc2c"), 0, 1, null, 2, false, "PGATD ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("20f67e38-f117-4b42-bbfc-5812aa717b94"), 1, 1, null, 2, true, "Undergraduate Opt In", 0, 0, 0, 0, 1, 0, 0 },
                    { new Guid("2b106b9d-ba39-4e2d-a42e-0ce827fdc324"), 0, 1, null, 0, false, "European Recognition", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("2b4862ca-bd30-4a3a-bfce-52b57c2946c7"), 0, 1, null, 2, false, "Licensed Teacher Programme", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("32017d68-9da4-43b2-ae91-4f24c68f6f78"), 0, 1, null, 2, false, "HEI - Historic", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("321d5f9a-9581-4936-9f63-cfddd2a95fe2"), 1, 1, null, 2, true, "Primary and secondary undergraduate fee funded", 0, 0, 0, 0, 1, 0, 0 },
                    { new Guid("34222549-ed59-4c4a-811d-c0894e78d4c3"), 0, 1, null, 0, false, "Graduate Teacher Programme", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("4163c2fb-6163-409f-85fd-56e7c70a54dd"), 0, 1, null, 2, false, "Core", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("4477e45d-c531-4c63-9f4b-e157766366fb"), 1, 1, null, 2, true, "Early Years ITT Graduate Employment Based", 1, 0, 1, 0, 1, 0, 0 },
                    { new Guid("4514ec65-20b0-4465-b66f-4718963c5b80"), 0, 1, null, 2, false, "Legacy ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("45c93b5b-b4dc-4d0f-b0de-d612521e0a13"), 0, 1, null, 0, false, "FE Recognition 2000-2004", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("4b6fc697-be67-43d3-9021-cc662c4a559f"), 0, 1, null, 2, false, "Authorised Teacher Programme", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("4bd7a9f0-28ca-4977-a044-a7b7828d469b"), 0, 1, null, 2, false, "Core Flexible", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("50d18f17-ee26-4dad-86ca-1aae3f956bfc"), 1, 1, null, 2, false, "Troops to Teach", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("51756384-cfea-4f63-80e5-f193686e0f71"), 0, 1, null, 0, false, "Overseas Trained Teacher Programme", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("53a7fbda-25fd-4482-9881-5cf65053888d"), 1, 1, null, 2, true, "Provider led Undergrad", 0, 0, 0, 0, 1, 0, 0 },
                    { new Guid("5748d41d-7b53-4ee6-833a-83080a3bd8ef"), 0, 1, null, 2, false, "CTC or CCTA", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("57b86cef-98e2-4962-a74a-d47c7a34b838"), 1, 1, null, 2, true, "Assessment Only", 0, 0, 0, 0, 1, 0, 0 },
                    { new Guid("5b7d1c4e-fb2b-479c-bdee-5818daaa8a07"), 0, 1, null, 2, false, "EYTS ITT Migrated", 1, 0, 0, 0, 0, 0, 0 },
                    { new Guid("5b7f5e90-1ca6-4529-baa0-dfba68e698b8"), 1, 1, null, 2, false, "Teach First Programme", 0, 0, 0, 0, 1, 0, 0 },
                    { new Guid("5d4c01c1-0841-4306-b49c-48ad6499fdc0"), 1, 1, null, 2, true, "Teacher Degree Apprenticeship", 0, 1, 1, 0, 0, 0, 1 },
                    { new Guid("64c28594-4b63-42b3-8b47-e3f140879e66"), 0, 1, null, 2, false, "Licensed Teacher Programme - Independent School", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("6987240e-966e-485f-b300-23b54937fb3a"), 1, 1, null, 2, true, "Postgraduate Teaching Apprenticeship", 0, 0, 0, 0, 1, 0, 0 },
                    { new Guid("700ec96f-6bbf-4080-87bd-94ef65a6a879"), 1, 1, null, 2, true, "Flexible ITT", 0, 0, 0, 0, 1, 0, 0 },
                    { new Guid("70368ff2-8d2b-467e-ad23-efe7f79995d7"), 0, 1, null, 2, false, "Registered Teacher Programme", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("7721655f-165f-4737-97d4-17fc6991c18c"), 1, 1, null, 2, false, "PGDE ITT", 0, 0, 0, 0, 1, 0, 0 },
                    { new Guid("779bd3c6-6b3a-4204-9489-1bbb381b52bf"), 0, 1, null, 2, false, "Licensed Teacher Programme - OTT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("7c04865f-fa39-458a-bc39-07dd46b88154"), 0, 1, null, 2, false, "UGMT ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("7f09002c-5dad-4839-9693-5e030d037ae9"), 1, 1, null, 2, true, "Early Years ITT School Direct", 1, 0, 0, 0, 1, 0, 0 },
                    { new Guid("82aa14d3-ef6a-4b46-a10c-dc850ddcef5f"), 0, 1, null, 2, false, "TCMH", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("877ba701-fe26-4951-9f15-171f3755d50d"), 2, 1, null, 2, true, "Welsh Recognition", 0, 0, 0, 2, 2, 2, 0 },
                    { new Guid("88867b43-897b-49b5-97cc-f4f81a1d5d44"), 0, 1, null, 0, false, "Other Qualifications non ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("8f5c0431-d006-4eda-9336-16dfc6a26a78"), 0, 2, null, 0, false, "EYPS", 2, 0, 0, 0, 0, 0, 0 },
                    { new Guid("97497716-5ac5-49aa-a444-27fa3e2c152a"), 1, 1, null, 2, true, "Provider led Postgrad", 0, 0, 0, 0, 1, 0, 0 },
                    { new Guid("97e1811b-d46c-483e-aec3-4a2dd51a55fe"), 1, 1, null, 2, true, "School Direct Training Programme Self Funded", 0, 0, 0, 0, 1, 0, 0 },
                    { new Guid("9a6f368f-06e7-4a74-b269-6886c48a49da"), 0, 1, null, 2, false, "ProfGDE ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("a6431d4b-e4cd-4e59-886b-358221237e75"), 0, 1, null, 2, false, "Graduate non-trained", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("aa1efd16-d59c-4e18-a496-16e39609b389"), 0, 1, null, 2, false, "Long Service", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("abcb0a14-0c21-4598-a42c-a007d4b048ac"), 0, 1, null, 2, false, "School Centered ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("bed14b00-5d08-4580-83b5-86d71a4f1a24"), 0, 1, null, 2, false, "TC ITT", 0, 0, 1, 0, 0, 0, 0 },
                    { new Guid("bfef20b2-5ac4-486d-9493-e5a4538e1be9"), 1, 1, null, 2, true, "High Potential ITT", 0, 0, 1, 0, 1, 0, 0 },
                    { new Guid("c80cb763-0d61-4cf1-a749-37c1d0ab85f8"), 0, 1, null, 2, false, "Legacy Migration", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("c97c0fd2-fd84-4949-97c7-b0e2422fb3c8"), 1, 1, null, 2, true, "Early Years ITT Undergraduate", 1, 0, 0, 0, 1, 0, 0 },
                    { new Guid("ce61056e-e681-471e-af48-5ffbf2653500"), 0, 1, null, 0, false, "Overseas Trained Teacher Recognition", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("d0b60864-ab1c-4d49-a5c2-ff4bd9872ee1"), 1, 1, null, 2, true, "International Qualified Teacher Status", 0, 0, 1, 0, 1, 0, 0 },
                    { new Guid("d5eb09cc-c64f-45df-a46d-08277a25de7a"), 0, 1, null, 2, false, "Licensed Teacher Programme - FE", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("d9490e58-acdc-4a38-b13e-5a5c21417737"), 1, 1, null, 2, true, "School Direct Training Programme", 0, 0, 0, 0, 1, 0, 0 },
                    { new Guid("d9eef3f8-fde6-4a3f-a361-f6655a42fa1e"), 1, 1, null, 2, true, "Early Years ITT Assessment Only", 1, 0, 0, 0, 1, 0, 0 },
                    { new Guid("dbc4125b-9235-41e4-abd2-baabbf63f829"), 1, 1, null, 2, true, "Early Years ITT Graduate Entry", 1, 0, 1, 0, 1, 0, 0 },
                    { new Guid("e5c198fa-35f0-4a13-9d07-8b0239b4957a"), 0, 1, null, 2, false, "Licensed Teacher Programme - Maintained School", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("eba0b7ae-cbce-44d5-a56f-988d69b03001"), 0, 2, null, 2, false, "EYPS ITT Migrated", 2, 0, 0, 0, 0, 0, 0 },
                    { new Guid("ec95c276-25d9-491f-99a2-8d92f10e1e94"), 0, 1, null, 2, false, "European Recognition - PQTS", 3, 0, 0, 0, 0, 0, 0 },
                    { new Guid("ef46ff51-8dc0-481e-b158-61ccea9943d9"), 1, 1, null, 2, true, "Primary and secondary postgraduate fee funded", 0, 0, 1, 0, 1, 0, 0 },
                    { new Guid("f4da123b-5c37-4060-ab00-52de4bd3599e"), 0, 1, null, 0, false, "EC directive", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("f5390be5-8336-4951-b97b-5b45d00b7a76"), 0, 1, null, 2, false, "PGATC ITT", 0, 0, 0, 0, 0, 0, 0 },
                    { new Guid("f85962c9-cf0c-415d-9de5-a397f95ae261"), 1, 1, null, 2, true, "Future Teaching Scholars", 0, 0, 1, 0, 1, 0, 0 },
                    { new Guid("fc16290c-ac1e-4830-b7e9-35708f1bded3"), 0, 1, null, 2, false, "Licensed Teacher Programme - Armed Forces", 0, 0, 0, 0, 0, 0, 0 }
                });

            migrationBuilder.InsertData(
                table: "support_task_types",
                columns: new[] { "support_task_type", "name" },
                values: new object[,]
                {
                    { 1, "connect GOV.UK One Login user to a teaching record" },
                    { 2, "change name request" },
                    { 3, "change date of birth request" },
                    { 4, "TRN request from API" },
                    { 5, "TRN request from NPQ" },
                    { 6, "manual checks needed" },
                    { 7, "teacher pensions potential duplicate" }
                });

            migrationBuilder.InsertData(
                table: "training_subjects",
                columns: new[] { "training_subject_id", "is_active", "name", "reference" },
                values: new object[,]
                {
                    { new Guid("002bf98f-fd1e-422d-a951-1cd4dd29d4ce"), false, "German Lang, Lit & Cult", "R8820" },
                    { new Guid("00447627-36a3-42c1-9336-5cc4d24e46d3"), true, "geomorphology", "101064" },
                    { new Guid("005ceb13-0881-4ed0-bb19-08d49c3763a0"), true, "musicology", "100667" },
                    { new Guid("006fa254-d985-4f43-82bf-54d49c4fa91c"), true, "rehabilitation studies", "101289" },
                    { new Guid("008ff140-767e-46c0-ac32-85292e33da8f"), true, "medieval history", "100309" },
                    { new Guid("00996bd5-f2f5-4423-bb44-162efb24acb8"), true, "ship design", "100568" },
                    { new Guid("00ac2bfd-091b-49c1-b785-04ca973cf974"), false, "Business Economics", "L1100" },
                    { new Guid("00b2d1d0-628a-4a1d-943a-3e317e9cf45c"), true, "learning disabilities nursing", "100286" },
                    { new Guid("00bd6592-db8b-4eea-89b7-be0922576aa0"), false, "Italian Language & Studies", "R3101" },
                    { new Guid("010d33db-a216-4c3e-bfb3-8c64f1126491"), false, "Manufacturing", "H700" },
                    { new Guid("01561d4b-5dd5-45ae-9989-5a0b713251da"), false, "Business Management Studies", "N1001" },
                    { new Guid("015d862e-2aed-49df-9e5f-d17b0d426972"), true, "food and beverage production", "100526" },
                    { new Guid("0177cdf3-d1e8-4db5-8c44-6421a0f013ce"), true, "James Joyce studies", "101479" },
                    { new Guid("01a3070e-8d75-43ed-a6c7-14bbe0a8b42b"), true, "pest management", "100884" },
                    { new Guid("01fdac1e-d370-4a0e-a390-4df05436c839"), true, "stochastic processes", "101033" },
                    { new Guid("0238f3f2-df59-477a-970a-23d06ce921e6"), false, "Games", "X2003" },
                    { new Guid("02b41511-fa57-4c46-9597-6b2d3a8b74d3"), true, "health and safety management", "100866" },
                    { new Guid("02e05cd6-1962-46fb-8f5e-8ef2ac23d162"), true, "electrical engineering", "100164" },
                    { new Guid("0328bf23-770a-4103-9645-e5efeb7ae8d8"), false, "Performing Arts", "W310" },
                    { new Guid("03358e6a-b8af-4fb0-b83c-dcffb5578e31"), true, "colonial and post-colonial literature", "101108" },
                    { new Guid("033d952b-4f47-47f6-a4c8-f11b30d8b763"), false, "Commerce", "N1206" },
                    { new Guid("03497a81-2f36-4eb2-aaab-5dbe501b6d98"), true, "popular music composition", "101451" },
                    { new Guid("035d5c01-9ed4-43ac-be23-37f12d0c094d"), false, "Sport & exercise science", "C600" },
                    { new Guid("0367b82f-8cfd-420a-bf8a-71dbf878b72e"), true, "Hungarian studies", "101311" },
                    { new Guid("036aedb3-4173-44f3-97ab-0eaba86e03b7"), true, "work-based learning", "101277" },
                    { new Guid("03abaddd-e1d2-4d98-ae33-e2872e0cb488"), false, "Business, Administration and Finance", "N990" },
                    { new Guid("03bb41de-a5d9-43ce-8632-489a34c98762"), false, "Social Work", "L8850" },
                    { new Guid("0416a4f2-4a3a-40ee-847a-bdaa5a0727f2"), false, "Arts Administration", "W9900" },
                    { new Guid("041e55de-d9de-45aa-8710-7bec63db7e13"), true, "biometry", "100865" },
                    { new Guid("042590b0-ce6e-4024-981c-e0bc85af3ea7"), true, "health informatics", "100994" },
                    { new Guid("04284cc5-681f-4545-9918-5e3e67196b4a"), true, "microelectronic engineering", "100168" },
                    { new Guid("0477d8a5-ccbb-47bd-a86f-e1405128fc08"), true, "psychometrics", "101383" },
                    { new Guid("049a7a5d-d3ac-415b-85df-58e82b2dcb5f"), false, "Science and Technology", "F9605" },
                    { new Guid("04e8008c-0e2e-4888-80ef-2ffb5f1e5ab4"), true, "Chinese medical techniques", "100236" },
                    { new Guid("05168ad9-eedf-43c0-b141-e7ab26cab056"), false, "German", "R200" },
                    { new Guid("0523b697-4f5e-4f81-b04d-c2618bc0b218"), false, "Urban Studies", "K4600" },
                    { new Guid("053003f2-8ce0-4943-b0b1-99baf4fd0239"), false, "Mathematics & Computer Studies", "G5004" },
                    { new Guid("05352933-c094-4582-8015-91019dae260e"), true, "chemical physics", "100416" },
                    { new Guid("05389dd4-79f3-484c-ad6e-2b09a3c80947"), false, "Horticultural Science", "D2501" },
                    { new Guid("0543a249-172a-4f0f-84c2-97123ca3f529"), false, "Pure Mathematics", "G1200" },
                    { new Guid("058782be-f887-4851-bb16-18d3620eacdd"), true, "public relations", "100076" },
                    { new Guid("0636913d-783f-4d6d-89cf-f8c3f6f2e0f7"), true, "sculpture", "100592" },
                    { new Guid("0670dc56-acf9-4531-b3e9-fa9472833586"), true, "climate change", "101070" },
                    { new Guid("06fa8277-7809-421d-8a24-f3ab8130149b"), true, "marine sciences", "100418" },
                    { new Guid("07526651-460a-49e4-be71-23d65e5c6ba1"), false, "Archaeology", "V6000" },
                    { new Guid("07b4718e-18be-4d1d-beed-731630fa5c27"), true, "applied psychology", "100493" },
                    { new Guid("07b68d5f-8182-4a3c-8392-d6387875be1c"), false, "French Lang and Literature", "R1103" },
                    { new Guid("07d698d1-4ee3-47ce-8b25-70a1684e0abd"), false, "Drama With English", "W9918" },
                    { new Guid("07d95576-5741-46c8-b3dd-8dd3877e07fe"), true, "computer forensics", "100385" },
                    { new Guid("07e8e170-5b8d-4efc-9416-f32300bce270"), false, "Economics and Business Ed", "N9702" },
                    { new Guid("0807d6ed-2237-44fe-ac11-44cd70854c5c"), false, "Celtic Languages", "Q500" },
                    { new Guid("080ae67b-901a-4fa4-a36c-d911ee2a581e"), false, "Design & Tech : Home Economics", "W9927" },
                    { new Guid("08711a13-26f5-4c60-8de8-8aec7b9691b5"), false, "Rural Studies", "F9010" },
                    { new Guid("08736a66-ecbb-4679-9f0e-93cdfae37fb0"), true, "Breton language", "101419" },
                    { new Guid("08966417-8e15-4ac3-8ae5-182a516b38c7"), false, "Intergrated Studies", "F9629" },
                    { new Guid("08a39573-be7d-465c-9fb7-de4436dbb393"), true, "modern Middle Eastern literature", "101196" },
                    { new Guid("08b5901c-2b50-45d3-8503-b10259f77f13"), false, "History and Geography", "Z0101" },
                    { new Guid("09483d19-dfe8-4e08-abe7-e9de379efb42"), true, "natural language processing", "100961" },
                    { new Guid("097175c6-113d-4ffa-b25c-2f68e46f993b"), true, "Italian studies", "100327" },
                    { new Guid("097620e0-eb23-4fe8-be42-97c4aad5bc08"), false, "Special Education Studies", "X9014" },
                    { new Guid("09819c9d-9588-4f69-b71e-020dfebbd0fa"), true, "international development", "100488" },
                    { new Guid("09ab681b-966a-4c0d-9bc4-e97a0811312d"), true, "financial mathematics", "100401" },
                    { new Guid("09b16d6c-03aa-4c5c-9303-79c270420bf5"), true, "biochemistry", "100344" },
                    { new Guid("09d47234-0d31-4356-881f-b3f4db01af7f"), true, "high performance computing", "100741" },
                    { new Guid("09db8708-b306-4495-b183-7161d5efd5ec"), true, "health sciences", "100246" },
                    { new Guid("0a08ee0b-2bf7-44e1-8fff-72ca8619a3c1"), false, "Studies In Art", "W9922" },
                    { new Guid("0a4a7bcb-7f61-4ba8-af8c-b4df02898d29"), false, "Drama and Contextual Studies", "W4009" },
                    { new Guid("0a5e8fa8-507e-4434-8004-b221a5adcf0e"), true, "Welsh literature", "101163" },
                    { new Guid("0b174977-18ba-4d6e-ae85-4601cf297b8a"), true, "requirements engineering", "100821" },
                    { new Guid("0b34aa64-241b-45dd-bd91-664f66dbd0f3"), false, "Computing and Technology", "G9007" },
                    { new Guid("0b353b77-d02d-4e5e-b1d0-f963f27d4ed4"), true, "popular music", "100841" },
                    { new Guid("0b609275-68df-4077-8a03-04031603d6a1"), true, "African literature", "101188" },
                    { new Guid("0b765f82-8223-49e5-9f69-ed89b7adba68"), true, "plant biochemistry", "100932" },
                    { new Guid("0ba8308c-fc2e-4f05-97f1-dc99d34f6c31"), true, "occupational health", "100248" },
                    { new Guid("0bae8523-c607-4345-9cbb-d7284d0e0d14"), false, "Physics/Engineering Science", "F6006" },
                    { new Guid("0bc8b844-e96c-4fca-b73b-3c0d3615fb3f"), false, "Education Of The Part. Hearing", "X6006" },
                    { new Guid("0bcaf443-de1d-460a-ba5c-f11baff3bc79"), true, "electrical power generation", "101353" },
                    { new Guid("0bce3dad-3452-4e8b-8232-158a71544698"), true, "polymer science and technology", "100145" },
                    { new Guid("0c1d6a94-ca55-4b23-91e3-a4f64f8e2115"), false, "Norwegian", "R7500" },
                    { new Guid("0c267114-e372-437d-9933-446b3cd4fd02"), true, "Italian language", "100326" },
                    { new Guid("0c9fa4b9-5a50-4842-bde4-3d13f0a103a1"), true, "ecology", "100347" },
                    { new Guid("0d29d5fc-242b-4f0b-b16c-6c69bbd94bff"), true, "human biology", "100350" },
                    { new Guid("0d813255-b8ae-4686-b694-35ff085a4d7c"), false, "General Topics In Education", "X8890" },
                    { new Guid("0dada6be-3bf3-49b7-8c97-d00f486371c8"), true, "Portuguese language", "101142" },
                    { new Guid("0de7e1fd-d679-4037-b0ec-244bfeb86159"), false, "Humanities", "Z0060" },
                    { new Guid("0e1cfcf8-d7ae-40a9-a09b-61f369648b9e"), false, "Engineering (Diploma)", "H990" },
                    { new Guid("0e21bc65-ceac-4af3-88d7-88798cac7c5b"), true, "railway engineering", "100157" },
                    { new Guid("0e3352a8-44b7-4963-8ffc-03daeb9223a2"), true, "computer games", "101267" },
                    { new Guid("0f11491d-d1d3-4aa4-9ff9-e905ae6825ae"), false, "Geography (As A Science)", "F8000" },
                    { new Guid("0f3c8673-8bf6-4774-8eea-7063899f6dbb"), false, "General Studies In Humanities", "Y3200" },
                    { new Guid("0f68664e-ba97-454d-ab56-0349579cf647"), true, "political sociology", "100629" },
                    { new Guid("0fa6cf72-549d-424e-affa-52cb6114ffb3"), false, "Creative Studies (Art)", "W1008" },
                    { new Guid("0fbdda8a-cb18-4a37-af1b-ca651de0d1dc"), true, "Margaret Atwood studies", "101480" },
                    { new Guid("0fbec27a-048e-41fd-a14f-da97ef67626f"), false, "Hispanic Studies", "R4003" },
                    { new Guid("100d2adc-c1da-4ea3-a04b-700551d9e587"), true, "Vietnamese language", "101369" },
                    { new Guid("100fb49b-1b22-4973-8547-7eafc0014715"), true, "economic systems", "100606" },
                    { new Guid("10165d35-3e0d-4576-a2f1-b48bc79ac478"), false, "Welsh As A Second Language", "Q520" },
                    { new Guid("101d29db-2e4b-4a8d-9dbe-3ba97886713a"), true, "music technology", "100221" },
                    { new Guid("101e02c6-4634-46ad-89b2-b9a0d3d2d425"), false, "Genetics", "C4000" },
                    { new Guid("102e59ef-4574-4185-916a-a42b60cd6fef"), true, "history of architecture", "100782" },
                    { new Guid("10624120-7343-4f49-9a04-57a25bca422f"), true, "community theatre", "100710" },
                    { new Guid("10d19613-df52-4667-8c8a-f97e90a98af0"), false, "Dummy Subject", "ZZ9999" },
                    { new Guid("1129fa2e-16bd-4ce6-a85e-30c5544571ca"), false, "Food Technology", "D4200" },
                    { new Guid("1137e97c-4f30-401c-a32b-946ff6e77813"), true, "crop production", "100947" },
                    { new Guid("115600e6-88cf-420d-b084-6d11531e1a2e"), true, "engineering physics", "101061" },
                    { new Guid("11910b21-f4d9-4211-a895-a158e2055d1f"), false, "Anthropology", "L6000" },
                    { new Guid("119caf30-8657-4658-b6d3-f9bfa3a7bb40"), false, "Italian", "R300" },
                    { new Guid("11b1bafe-d26c-466c-b058-420aaf45490f"), false, "Victorian Studies", "V1201" },
                    { new Guid("11f926e8-e3bc-43c1-bc8e-e029d3711f81"), true, "emergency nursing", "100284" },
                    { new Guid("11fefa8c-c6bf-469b-afca-6cb9f355010d"), false, "Cinematics", "W5000" },
                    { new Guid("122a7153-b2af-4ba1-9b7c-688c040f6304"), false, "Social Anthropology", "L6001" },
                    { new Guid("122a823b-810d-4f62-9143-6a1b8a0f2275"), true, "maritime archaeology", "101261" },
                    { new Guid("12828dec-1ebc-4310-a2b3-3e1d622e79ea"), true, "epilepsy care", "101333" },
                    { new Guid("128d8774-b7ad-486a-9ff5-4a91c295d2b5"), true, "astrophysics", "100415" },
                    { new Guid("130113b9-4100-44da-a17e-3c67158685e1"), true, "pure mathematics", "100405" },
                    { new Guid("132a4b3f-69c7-41cd-9527-b8e7c6d39ec9"), false, "English As A Foreign Language", "Q3700" },
                    { new Guid("1349b1af-8631-4477-ac30-900675ca7688"), false, "Further Ed. Teacher Training", "X5001" },
                    { new Guid("134f93b0-463f-447e-8336-36fab9ef2834"), true, "popular music performance", "100657" },
                    { new Guid("1377b304-0103-4f4e-ad71-6ff55c7cae46"), false, "Fellow Royal Photographic Society", "10519" },
                    { new Guid("1389bdcc-6bba-45e2-bac9-a1e954239124"), false, "Broad Balanced Science", "F9608" },
                    { new Guid("139ca859-dc3e-4d74-a404-eaa27fcc7bdf"), false, "Bed (UK)", "Z0080" },
                    { new Guid("13a673ca-3bc4-4e7e-a8cc-0ca0da648a81"), false, "Retail Business", "N900" },
                    { new Guid("13ac0aca-e015-4f48-a6e3-ddba998caa0f"), false, "Applied ICT", "I900" },
                    { new Guid("13bd3303-d7d6-49e4-aad7-e38469e1d598"), true, "market research", "100846" },
                    { new Guid("13de25f8-fc36-4154-b751-d0ab535c662a"), true, "forensic psychology", "100387" },
                    { new Guid("140251c7-4f16-4578-b09c-8a55e99dd8fa"), false, "Graphics", "W2102" },
                    { new Guid("1418a886-7be4-4de9-b58c-213f7a8017b1"), false, "Modern Studies", "Y3201" },
                    { new Guid("146aeb34-7a39-449f-abc3-61a4b4cdd4f2"), false, "Icelandic", "ZZ9005" },
                    { new Guid("149c7347-0ea9-4424-aacc-21b54e68479e"), false, "History and The Environment", "V9003" },
                    { new Guid("14ac5701-c761-40e3-98f5-645b8e670bd3"), false, "Computer Science", "I100" },
                    { new Guid("14d25849-7c42-42ff-8447-16e940cda458"), true, "film and sound recording", "100890" },
                    { new Guid("14fb4100-daad-43a3-bf9d-7cc177f5727c"), true, "cellular pathology", "100540" },
                    { new Guid("15048d02-d96c-44a5-be08-d2d1254ccf35"), false, "Language Studies", "Q1400" },
                    { new Guid("15a03936-e4ec-4942-8e1e-40675ba4c10a"), true, "pathology", "100274" },
                    { new Guid("15b11065-d6cf-4fd5-a4ee-8f30adefbf0f"), true, "mapping science", "101058" },
                    { new Guid("15ba6775-9cd7-4213-986b-42bda9de0f37"), true, "Thomas Pynchon studies", "101474" },
                    { new Guid("15c8a0ef-b13d-4bf4-8e81-1e25b232d99d"), true, "broadcast journalism", "100439" },
                    { new Guid("15d6e452-fe60-4306-b477-bbc523e5ffd7"), true, "marine physics", "101390" },
                    { new Guid("15e98c51-fb05-4acf-a89f-c7a32ea8d77d"), false, "Audiology", "B6000" },
                    { new Guid("163a9ae9-28d0-44ad-a1c3-2fad5b884069"), true, "operating department practice", "100273" },
                    { new Guid("1692ab8b-1e2c-4526-a784-c7565d0452da"), true, "secondary education", "100465" },
                    { new Guid("16a152b8-214b-4f46-91f2-fbfe04a9dc97"), true, "veterinary medicine", "100531" },
                    { new Guid("1708895b-2a60-4a86-8ead-be84dce2ab42"), false, "Tech (Home Economics/Textiles)", "W9906" },
                    { new Guid("1717c366-c869-444d-a1db-e387b564432a"), false, "Expressive Arts (Music)", "W3003" },
                    { new Guid("174f8921-6ce5-4c97-b154-05478409b7c0"), true, "Urdu language", "101175" },
                    { new Guid("176547b7-970b-4320-b3a1-50f500ecbeea"), true, "psychology of ageing", "100958" },
                    { new Guid("176f4491-75d6-40db-a57d-d6ad8179506c"), true, "South East Asian history", "100773" },
                    { new Guid("178c0d7f-5583-4f40-a8f6-e5f3d1533b06"), true, "Coptic language", "101414" },
                    { new Guid("179c9c1b-b059-4ba8-992b-f434f19fe368"), true, "musical theatre", "100035" },
                    { new Guid("17a83eeb-f022-4cdf-9142-3e818b02cb7d"), true, "engineering and industrial mathematics", "101028" },
                    { new Guid("17f32ee4-bf01-4d3b-979e-137d0da56884"), false, "Management Studies", "N1100" },
                    { new Guid("180fd8ff-a74f-4c37-b200-3775a0098601"), true, "social history", "100312" },
                    { new Guid("18897954-105e-4ff7-8c10-831ee70b7072"), true, "broadcast engineering", "100539" },
                    { new Guid("18b3d2e8-755a-40aa-ac72-279660dbb60f"), false, "Art & Crafts", "W9000" },
                    { new Guid("18be8c05-cdbc-4cb9-aebe-62b4ac4d6a77"), false, "Education of special education needs children", "X6005" },
                    { new Guid("18c1d74e-4b01-4ced-afb7-726b10a2c7fe"), false, "Creative and Media (Diploma)", "P900" },
                    { new Guid("18c20e91-b116-47db-8003-2d446a368119"), true, "herbal medicine", "100237" },
                    { new Guid("18c42e4a-32ad-4c46-8f47-0d533512cfbe"), false, "Graphic Design", "W2100" },
                    { new Guid("18c68d1b-a6f8-4196-9896-f496e0e9acd1"), false, "Biochemistry", "C7000" },
                    { new Guid("18cdb3e7-3e4f-40bb-8bd5-91708e465eac"), true, "music education and teaching", "100642" },
                    { new Guid("18d039ca-7602-40d0-9e73-4a9d3a082b4e"), true, "physiology", "100262" },
                    { new Guid("18e14dbe-0d5f-480f-8d14-a852e7cac8a5"), true, "environmental sciences", "100381" },
                    { new Guid("18e55e1e-b040-4f85-8d7b-e9bb246852bb"), false, "English For Non-Native Speakrs", "Q3004" },
                    { new Guid("18f5ab32-b0a3-4e73-ad69-99cef9a0d2af"), false, "Special Education Needs", "X161" },
                    { new Guid("192027d5-8de0-479d-aa6f-4660fa668dba"), true, "Japanese studies", "101168" },
                    { new Guid("192c1cc6-9dd9-44a4-b3fa-13857c2bdd39"), false, "Literature (Anglo-Irish)", "P4602" },
                    { new Guid("19428eee-01f0-43ac-9eea-2e5363befe81"), true, "biophysical science", "100949" },
                    { new Guid("198d6afd-b83b-4ef2-b7f2-6cabcd53fce0"), true, "community music", "100854" },
                    { new Guid("199589d3-6196-4c4b-a70e-eba0d37ce656"), true, "international studies", "101288" },
                    { new Guid("19b439e5-b05f-4d02-8b91-2af1f5cdd112"), false, "General Language Studies", "T8880" },
                    { new Guid("19b578fb-58e4-4251-b08e-acb5d28cd00b"), false, "Punjabi", "ZZ9009" },
                    { new Guid("19b95194-9ced-40c1-a2cb-77f8e84a044f"), true, "international relations", "100490" },
                    { new Guid("19c96dff-0f10-4790-875e-9955c3a4aa81"), false, "Media Studies", "P300" },
                    { new Guid("19e2cb94-2215-4a66-8dde-d7c2e812ccbd"), false, "Science & The Environment", "F9012" },
                    { new Guid("1a0244d7-7c6e-475f-bea8-dd029c0f6de6"), false, "History Of Art", "V4000" },
                    { new Guid("1a1cec9c-b20e-4a00-ab88-0db2e9955b48"), false, "Painting", "W1100" },
                    { new Guid("1a2bffd0-afac-43b6-99e2-826b4405c02d"), true, "population biology", "100850" },
                    { new Guid("1a386301-427f-4644-b38c-178c442ea10f"), false, "Modern Hebrew", "ZZ9006" },
                    { new Guid("1a4b33dc-7741-4d94-a8f0-125802434990"), true, "Persian languages", "101193" },
                    { new Guid("1a7283b8-f30f-4a3c-af42-37fad6698e4a"), false, "Religious & Moral Education", "V8008" },
                    { new Guid("1a9917a9-ca2b-4a9f-9036-58fe19d3a82e"), true, "computer games design", "101268" },
                    { new Guid("1ad52ba0-46a2-4435-8fa8-9b750c5e2f9e"), false, "Drama, Music, Movement, Lit", "W4012" },
                    { new Guid("1b14b772-1562-48c1-a65e-62d4bf8f9033"), true, "business information systems", "100361" },
                    { new Guid("1b28203f-061b-4bc1-8e77-13ce64f663b8"), true, "history", "100302" },
                    { new Guid("1bb69e7b-1632-4d0d-bf38-e0dc522600f6"), false, "Sports Science", "X2009" },
                    { new Guid("1bbe2c87-e3b4-444e-a4b8-375cf0d8aa2b"), true, "cell zoology", "100881" },
                    { new Guid("1bbea5b6-97ce-4d20-9ab1-75d271e8c093"), true, "architectural engineering", "100120" },
                    { new Guid("1bcd45c8-003a-424a-960e-e554f7970882"), true, "archaeological sciences", "100384" },
                    { new Guid("1bd7237d-6f15-4624-99d2-768a3a19fe79"), true, "land management for recreation", "100990" },
                    { new Guid("1be1bdfa-31a6-45cf-b79b-7be417611777"), true, "environmental and public health", "101317" },
                    { new Guid("1be8bb34-4ebd-4dea-be6e-7777bc122632"), true, "office administration", "100868" },
                    { new Guid("1c199fdc-b262-4e8e-84de-45f999c2e3a5"), false, "Life Sciences", "C9100" },
                    { new Guid("1c295d1e-d9b4-42ac-99b4-9c66c292acf1"), true, "materials engineering", "100203" },
                    { new Guid("1c3682e5-9249-4419-b4c5-22dda62ed042"), true, "information services", "100916" },
                    { new Guid("1c3831cf-3676-46fd-b754-ef9d02d2134c"), true, "exploration geology", "101093" },
                    { new Guid("1c6c703c-9587-443f-9395-411221cc105b"), false, "Dyeing", "J4603" },
                    { new Guid("1c81fe60-8cd8-4a11-ab9c-27968bf29a0a"), true, "Turkish literature", "101434" },
                    { new Guid("1caaeaad-b11b-47e4-99ba-0c64035a4785"), true, "Aramaic language", "101417" },
                    { new Guid("1d0d6c47-a8af-490c-a497-1d52de98e693"), true, "history of music", "100664" },
                    { new Guid("1d13074c-2bac-471d-89ac-d8036701b081"), true, "finance", "100107" },
                    { new Guid("1d158b01-1f96-4748-a7b1-1d37f7384581"), true, "biochemical engineering", "100141" },
                    { new Guid("1d160a84-a129-4a17-9020-f29dbeb0c13c"), false, "Sport & exercise science not elsewhere classified", "C690" },
                    { new Guid("1d169844-189b-4f79-9bef-6f226ee3be71"), true, "cognitive modelling", "100989" },
                    { new Guid("1d18a3f5-35cf-4ff1-8fd2-7c9788155052"), false, "Engineering Science", "H1001" },
                    { new Guid("1d24c528-17be-4133-83b3-a09745452c7e"), true, "musical instrument manufacture", "100560" },
                    { new Guid("1d25bcc9-252b-46d8-b9c0-6423a4114604"), false, "Modern and Community Langs", "T2005" },
                    { new Guid("1d28a7a9-eb0b-42f2-aac0-2fa19a2945b3"), true, "crime history", "101436" },
                    { new Guid("1d3de640-4bc9-4209-a4a0-810f7a4ba525"), false, "Food & Nutrition", "B4003" },
                    { new Guid("1d5bf739-15e3-41c8-b79a-d34a9b33286e"), false, "Building Construction", "K2500" },
                    { new Guid("1d6b39fc-78ad-4225-b3e8-81a7e90c1411"), true, "space science", "101102" },
                    { new Guid("1da79a08-8ff9-4b45-8cfa-b9920181de29"), true, "theology", "100340" },
                    { new Guid("1dca3d18-2e0a-4584-a3ee-294238acab74"), false, "Russian", "R700" },
                    { new Guid("1dcbec92-e99d-4c7f-95ce-03e5b9a9eabc"), false, "History and Cultural Studies", "V9004" },
                    { new Guid("1df31e53-28fe-4958-8a96-6695295aec2d"), true, "intelligent systems", "100757" },
                    { new Guid("1e1bfca4-e469-47f5-99f7-2abfc80b5204"), true, "veterinary pharmacology", "100939" },
                    { new Guid("1e454430-1007-4e1f-b63a-0d9f81eea52f"), true, "computer systems engineering", "100162" },
                    { new Guid("1e48812c-f676-424d-b221-759bfeb59df5"), false, "Social Administration", "L4000" },
                    { new Guid("1ef74550-5ca3-4b8d-864e-df3214a6187a"), false, "Applied Social Science", "L5001" },
                    { new Guid("1f0500fb-526b-499b-bdbf-a528247f1605"), false, "French Studies", "Z0102" },
                    { new Guid("1f51429e-aea1-49ef-b2a1-8ab0d32de4fd"), true, "meteorology", "100382" },
                    { new Guid("1f599880-87cd-4198-9ee7-e38c93aa7791"), false, "Integrated Science", "F9601" },
                    { new Guid("1fa54f3d-066b-46cd-9e49-cad7a87551df"), true, "modern Middle Eastern studies", "101190" },
                    { new Guid("1fb57fe9-6d2f-44e7-a5e5-ca46744800c9"), true, "international economics", "100452" },
                    { new Guid("1fd99ccf-1bc7-43b1-aa0d-dfa5a857c57f"), true, "J. M. Coetzee studies", "101486" },
                    { new Guid("1febf25d-e304-4bbf-af6d-90ae5496e118"), true, "emergency and disaster management", "100823" },
                    { new Guid("1ff6d080-a941-4bcb-bc4c-9b2066bd3354"), true, "laser physics", "101076" },
                    { new Guid("1ffe77b3-028f-46c0-a3e5-7b67b94265de"), true, "computer animation and visual effects", "100363" },
                    { new Guid("200588a9-01bc-4ce2-918a-10243041ce05"), false, "Writing", "W4007" },
                    { new Guid("20256fc9-048f-45af-a9bb-cbc3a2653976"), false, "Portuguese", "R5000" },
                    { new Guid("207e1c41-80b2-47d1-8e71-4125f81f7ce4"), true, "printmaking", "100595" },
                    { new Guid("20974aa7-f47a-450c-8758-255012f981ee"), true, "French society and culture", "101133" },
                    { new Guid("20c2b60c-c0cc-4fc7-9f4d-cccfb2c09c6a"), true, "John Donne studies", "101468" },
                    { new Guid("210baa89-bb8a-46bb-8930-dbccee4648f6"), false, "Ancient Oriental Studies", "Q9700" },
                    { new Guid("21230c26-2e89-4e55-97fb-627c9f5df4fe"), true, "fine art", "100059" },
                    { new Guid("2135e569-c745-40b0-a2b3-e0a97a87f2f7"), true, "programming", "100956" },
                    { new Guid("2157c540-7f1f-49ee-a306-5811e2d2c1de"), true, "bioprocessing", "100135" },
                    { new Guid("217a0cf0-5de0-430b-8521-44e1b410d5cc"), true, "landscape studies", "100588" },
                    { new Guid("21cf21a0-b58a-41c2-ae13-3ff7fef9031b"), false, "English and Cummunications", "Q3009" },
                    { new Guid("21ebe2d3-a365-4aec-a9f1-1ef35695fc94"), true, "electrical power distribution", "101354" },
                    { new Guid("21ee43a3-3128-49b6-b0b0-2acc0df4c36b"), true, "affective neuroscience", "101382" },
                    { new Guid("220c233a-b047-46ed-b2d8-13b982d04796"), false, "Art, Craft & Design", "W2401" },
                    { new Guid("2215bf19-10e0-47fc-aa90-117fc2d977aa"), true, "Thai language", "101258" },
                    { new Guid("22373981-7b6d-4256-b092-d30e08689b0f"), false, "Science With Technology", "F9619" },
                    { new Guid("22431326-ab5d-4b4b-9b83-905b7ae19fad"), true, "history of mathematics", "100784" },
                    { new Guid("224bc083-b345-48ef-bb96-bf150cfeeab9"), true, "Latin language", "101420" },
                    { new Guid("227674ea-f3df-4b41-9c5f-30a599105527"), true, "publishing", "100925" },
                    { new Guid("228002ca-78e1-4669-bdae-b4fa2e9c7a6b"), true, "media production", "100443" },
                    { new Guid("2284a258-fe2b-4c3d-b808-639559f0fd5c"), true, "music therapy", "101241" },
                    { new Guid("229e90fb-51ec-44ba-8043-e55d49b65fd3"), true, "plant cell science", "100873" },
                    { new Guid("22a47345-9a09-4fca-a421-b5ad4427e2d6"), true, "British Sign Language studies", "100317" },
                    { new Guid("22ac0094-fbe3-44ef-a07e-cb55470932d6"), true, "agricultural sciences", "100516" },
                    { new Guid("22e1b12a-c0d1-4da9-bf52-bd26f804eda1"), false, "Human Nutrition", "B4004" },
                    { new Guid("22f3c277-60dd-492a-8e89-910793f6b6a1"), false, "Theology", "V8001" },
                    { new Guid("22f75afc-f4f1-4304-85e5-118a8c26edad"), true, "international agriculture", "101001" },
                    { new Guid("231e041e-6bd2-4291-b231-6bb81c3e378c"), false, "Celtic Studies", "Q5000" },
                    { new Guid("234301a0-132a-4590-92f2-6bcec408ed09"), true, "digital circuit engineering", "100546" },
                    { new Guid("234a27eb-0b3a-451e-8184-ae61d6ed85c4"), true, "data management", "100755" },
                    { new Guid("23567693-5534-4e72-b409-e87e895e45eb"), true, "Russian and East European studies", "100331" },
                    { new Guid("239efa09-2bb7-4082-b5a6-fa88a44b4cc1"), true, "event management", "100083" },
                    { new Guid("23a85c45-da70-4694-99aa-2c1842e27afe"), false, "Social Science", "L3200" },
                    { new Guid("24337f10-3ace-4d64-9c03-dc833d5060f1"), true, "nanotechnology", "101234" },
                    { new Guid("2450d889-9e85-4ace-a600-d3fe738c7df5"), true, "Turkish studies", "101195" },
                    { new Guid("2462fb70-abb7-469f-9044-d877fe67ee88"), true, "production and manufacturing engineering", "100209" },
                    { new Guid("249f7719-a095-4f06-9cd3-f40a750e7895"), false, "Textiles & Dress", "W2207" },
                    { new Guid("24afc417-713b-4fcc-93a4-9d6d115bae9d"), false, "C D T Incorp Tech & Info Tech", "W9926" },
                    { new Guid("24b884c4-e71c-4918-9cab-a0c958b3f698"), true, "nuclear and particle physics", "101077" },
                    { new Guid("24b8dd7c-dc76-4bb4-a698-a085cb55e208"), false, "Rural Environmental Science", "F9007" },
                    { new Guid("24be80d3-5dcb-46ac-aa2a-fa128a135084"), false, "Folklife Studies", "V3201" },
                    { new Guid("24bf7977-91bc-41ee-b714-9927fb755e18"), true, "Polish studies", "101152" },
                    { new Guid("24ee14c9-c5d6-4bee-a1d7-33d2178571af"), true, "dentistry", "100268" },
                    { new Guid("2504524a-1939-42af-81d2-aa38a697b3b9"), true, "information modelling", "100751" },
                    { new Guid("253ecffe-40f6-4bdb-8b12-9db37be36d97"), true, "engineering surveying", "100548" },
                    { new Guid("25486642-ed99-49ef-af00-4f3884a3873f"), true, "mathematics", "100403" },
                    { new Guid("2570cafb-7723-4f79-92f2-3749fdea7042"), true, "Chinese literature", "101166" },
                    { new Guid("25c61157-6b92-4bbf-a4ed-a7ea668f6a4c"), true, "development in Africa", "101358" },
                    { new Guid("25c67747-998b-4298-9ad3-c9de4e2250d4"), true, "applied statistics", "101030" },
                    { new Guid("25d3aa62-c0e8-40df-a301-7a68b2800fa2"), true, "history of photography", "100714" },
                    { new Guid("263ad86f-e308-4694-889b-37fbd201c381"), false, "Office Studies", "N7601" },
                    { new Guid("268239ce-58a9-4e93-9d1d-6dfe70f597fa"), false, "Applied Mathematics", "G1100" },
                    { new Guid("26993aae-c4c2-452a-858c-4becd04ef568"), true, "qualitative psychology", "101463" },
                    { new Guid("26c6261a-8d84-462e-867c-840a3d254c90"), true, "social sciences", "100471" },
                    { new Guid("26dc3264-29c7-47fc-9cbe-bcd920851dd3"), false, "Science With Chemistry", "F9626" },
                    { new Guid("26e9d312-2b21-4088-a89d-ab88fad7a000"), true, "ethnicity", "100624" },
                    { new Guid("2734f3b1-0704-4f2d-8728-756b4e2cd211"), true, "probability", "101032" },
                    { new Guid("274a3014-f40a-4aaa-b83e-2d7206044d6f"), false, "Bengali", "ZZ9001" },
                    { new Guid("278e9486-f2a5-41b3-ae3f-2dd87116ae96"), true, "systems thinking", "100743" },
                    { new Guid("279f4e4f-a9c9-4715-9d03-92d94f077db6"), false, "Business Education (Tech)", "N9701" },
                    { new Guid("27b8aa4b-f440-4ee1-9f67-3922c874bb68"), true, "engineering geology", "101106" },
                    { new Guid("27bfb349-fd7d-449d-be97-d5be5ff136b2"), false, "Geography Studies As A Science", "F8880" },
                    { new Guid("27e29434-3179-464d-a202-c00c44ae9af3"), true, "men's studies", "100623" },
                    { new Guid("27e40a69-cd31-4b59-9734-c86a76195077"), false, "Geology", "F6000" },
                    { new Guid("27ee2e03-b3cb-4626-8fd1-55d86b05303c"), true, "geographical information systems", "100369" },
                    { new Guid("28188a3f-0a2c-44f2-abe2-e673d76e9e90"), false, "Studies In History and Soc", "V5004" },
                    { new Guid("28310ae2-c47e-496a-a76e-8c2892f663ae"), true, "economics", "100450" },
                    { new Guid("28b0ba22-90c9-46d8-bd0d-b053053af3ac"), false, "Games and Sports", "X9020" },
                    { new Guid("28b80ac2-1abf-4ae1-8331-f943be0b5604"), true, "mentorship", "101322" },
                    { new Guid("28c5407c-1378-4de0-ba60-cdc8c54b2850"), true, "ballet", "100885" },
                    { new Guid("28c563cd-2af9-443c-a56e-aa2c0a131953"), false, "Technology Of Education", "X7000" },
                    { new Guid("28d2b3ea-b6cc-4798-a3aa-bbf7deed2986"), false, "Information and Communications Technology", "G500" },
                    { new Guid("28fcd605-8929-4ea9-9ddc-1fee09487e64"), true, "democracy", "100609" },
                    { new Guid("2907cf28-9074-4036-a61e-34820a95fda6"), false, "Technology and Mathematics", "G9004" },
                    { new Guid("2925db2f-c371-4829-8590-4326bc8efa44"), false, "Studies In Geog and Society", "L8004" },
                    { new Guid("29a80bbd-7c38-45d7-a0fb-e66dc6533f1d"), true, "Czech society and culture", "101501" },
                    { new Guid("29acfe2e-0518-4dd4-8292-aec30c66aab6"), true, "Russian society and culture", "101499" },
                    { new Guid("29b26c51-b494-4f61-b3d0-04a32837a5cc"), true, "podiatry", "100253" },
                    { new Guid("29babaa8-fe64-46e9-90de-6ca0e87ccc7d"), false, "Science: Environmental Science", "F9620" },
                    { new Guid("29c237c6-cde2-4036-b535-2c6a9566fb17"), true, "corrosion technology", "100545" },
                    { new Guid("29c7ee52-35b7-4283-acc9-daaf0628e596"), true, "rural estate management", "100977" },
                    { new Guid("29fbaaaf-3324-4d98-9294-156e47e89a29"), true, "crop protection", "100945" },
                    { new Guid("2a12c052-e652-4f5b-82c5-54ed9e338bc6"), true, "post compulsory education and training", "100508" },
                    { new Guid("2a42adb4-6937-4b4b-a5f7-94539c1dcb2c"), true, "welfare policy", "100649" },
                    { new Guid("2a4f0c02-0752-4eb3-a930-23a2ba23a597"), true, "Russian literature", "101157" },
                    { new Guid("2a5cd404-d58e-4dd4-b8b2-bc94d62f7c22"), true, "community ecology", "101457" },
                    { new Guid("2a62ac14-5ca9-4994-875f-cfb8cccc86e8"), true, "palliative care nursing", "100292" },
                    { new Guid("2b179d38-e596-4794-b67b-3dc3bb2669f7"), true, "community dance", "101454" },
                    { new Guid("2b26be9b-0cb9-4a90-ad98-6f03c238cdaf"), true, "dispute resolution", "101323" },
                    { new Guid("2b3281d0-600d-47e9-a839-39bae90b88c1"), true, "marine zoology", "100883" },
                    { new Guid("2b3aa4f9-c961-43d0-aa4c-e905c6a65ed4"), false, "Hindi", "ZZ9004" },
                    { new Guid("2b8070c7-2068-4934-b818-555d5ea6d1e1"), false, "Behavioural Sciences", "L7300" },
                    { new Guid("2b92b588-f93c-43c9-a8a7-b59a8d0763d5"), true, "maritime geography", "101065" },
                    { new Guid("2b9bbd90-4d71-4a01-a48a-4135fdb81434"), false, "Drama and Spoken English", "W4010" },
                    { new Guid("2b9be9ad-20ad-450d-8679-fc5927e5ec4f"), true, "theatre production", "100700" },
                    { new Guid("2bb0e5eb-feba-499a-81ef-da38a601f722"), true, "population ecology", "101458" },
                    { new Guid("2bbadac6-8ba7-45a0-9f04-531402cfc0c4"), false, "French Language & Studies", "R1101" },
                    { new Guid("2bc97e50-d388-42bf-a7c6-9141391a52f6"), true, "medical nursing", "100747" },
                    { new Guid("2bd3f2f1-d575-48ca-bc1f-76f931206344"), true, "veterinary pharmacy", "100941" },
                    { new Guid("2be7776d-3807-4411-8aa6-b2c2a12445a5"), true, "book-keeping", "100838" },
                    { new Guid("2bf3754c-289b-457f-b6e2-00360b77266d"), true, "hydrogeology", "101089" },
                    { new Guid("2bff5596-4179-432c-8957-e603ae8c05c5"), true, "music marketing", "100644" },
                    { new Guid("2c360bba-7777-4c41-a427-9b18c6018379"), true, "socialism", "101508" },
                    { new Guid("2c4a75db-5561-4c40-8412-1097db5a398f"), true, "Irish studies", "101315" },
                    { new Guid("2c5166c0-156e-4761-becf-ff1e979edd8b"), false, "Computing Science", "G5006" },
                    { new Guid("2c531d1d-5245-463e-acfe-02f61d117000"), true, "general science", "100390" },
                    { new Guid("2c5f8ac1-b53f-4934-bc9d-3f99f7eef7cc"), true, "quantitative psychology", "101462" },
                    { new Guid("2caa82a3-839f-417a-b8c5-0585f27ef9c2"), true, "Byzantine studies", "100774" },
                    { new Guid("2cb6598f-d5e9-4ae2-9956-fa4a9b2c4144"), true, "operational research", "100404" },
                    { new Guid("2cd80a11-888a-4753-ab2b-9dfbb22c660c"), true, "scriptwriting", "100729" },
                    { new Guid("2cd9a0dd-0170-4599-93e6-2fde2b6d67a2"), false, "Sport", "X2007" },
                    { new Guid("2d1a3209-2e2b-45a3-9c15-1da3991075e2"), false, "Ancient language studies not elsewhere classified", "Q490" },
                    { new Guid("2d4ff954-31ff-4fd8-9f34-2031db3e2257"), true, "German studies", "100324" },
                    { new Guid("2d5afbb0-e482-4096-835c-c6792e9e69aa"), true, "geotechnical engineering", "100551" },
                    { new Guid("2d79a348-30e2-45bd-aecc-5e26632a059b"), false, "Moral Education", "V7608" },
                    { new Guid("2d980b0a-5952-4721-ab83-16616d5c973e"), true, "blood sciences", "100912" },
                    { new Guid("2dbb43fa-74eb-4e15-95bc-ef8b5c2821dc"), true, "telecommunications engineering", "100159" },
                    { new Guid("2dc59122-9f7d-480a-8dd5-ded3f1159ca0"), false, "Wood Metal and Management", "W6101" },
                    { new Guid("2dd5b4ac-9916-4926-8815-e92a291b171b"), true, "creative management", "100811" },
                    { new Guid("2de6b266-34d9-494a-b791-7783ccc30af4"), false, "Ling,lit & Cult Herit-Welsh", "Q5207" },
                    { new Guid("2e15b175-5b8a-4962-90fa-7fc0f1cabcf6"), true, "modern languages", "100329" },
                    { new Guid("2e30b36e-31b7-4076-888e-1aaba600f20c"), true, "British history", "100758" },
                    { new Guid("2e345641-bc2b-486b-9445-12369e3c1614"), true, "stage design", "100708" },
                    { new Guid("2e52f992-1f2f-47c3-a7c2-a306ede8912d"), true, "Irish history", "100759" },
                    { new Guid("2e603ced-1727-4f90-97c8-02fae338438d"), false, "Science-Physics-Bath Ude", "F3012" },
                    { new Guid("2e646bcd-a058-4f3b-b365-bdfbed04c26a"), true, "applied linguistics", "100970" },
                    { new Guid("2e646f90-5d75-4e60-9488-bf92cf2bf71a"), true, "forensic science", "100388" },
                    { new Guid("2e93acd2-bb31-489b-bd81-ae048dd3eeab"), false, "English As A Second Language", "Q3003" },
                    { new Guid("2e9608ae-1f1e-4ed8-a112-1d4a7735b988"), false, "Chemistry With Science", "F9615" },
                    { new Guid("2e9a1a96-3e0a-4b81-81c7-f6e352d04093"), true, "graphic design", "100061" },
                    { new Guid("2ed9c4eb-3c98-4c43-baa4-3820b0ad1b92"), true, "technical theatre studies", "100702" },
                    { new Guid("2f3e4ce8-2f13-477c-8926-da703aee5a2f"), true, "real estate", "100218" },
                    { new Guid("2f5a7879-d9e4-4eb9-8b25-42b0fb956799"), true, "African studies", "101184" },
                    { new Guid("2f711074-58b6-4adf-9476-d20e65fb3b10"), true, "web and multimedia design", "100375" },
                    { new Guid("2fc5da28-052b-4024-8a6e-5b4437061039"), false, "Geography (Unspecified)", "L8001" },
                    { new Guid("2fca5663-efce-441e-a27b-7965ca027e83"), false, "Greek Civilisation", "V1006" },
                    { new Guid("3005b86b-422d-4754-a2ef-e9cc23726c0f"), true, "project management", "100812" },
                    { new Guid("302c3189-a93a-4e1e-bc7f-9b0132ea1e6d"), true, "animal physiology", "100937" },
                    { new Guid("308a311e-2adf-4fc4-b776-83e8754bcbba"), false, "Building Technology", "K2100" },
                    { new Guid("308db47c-7294-4efe-8cf2-9510206234f6"), false, "Biological sciences (Combined/General Sciences)", "C900" },
                    { new Guid("30d48719-a814-4754-8af6-1a774ecde2db"), false, "Urdu", "T5002" },
                    { new Guid("314f08d0-0ec4-490b-9155-bde3ef22319a"), false, "Sport and Active Leisure", "N890" },
                    { new Guid("316e5ff6-2726-4e7e-b24a-3d6e724e463a"), false, "Environmental Geography", "L8003" },
                    { new Guid("32267037-19d9-4bd9-8819-6caa93531139"), true, "health psychology", "100985" },
                    { new Guid("324e2215-43c5-49b3-a7ff-8825a6a56618"), false, "Sports Studies", "X2002" },
                    { new Guid("32771ba9-0091-4d11-afc8-3f9302c41c74"), true, "German history", "100763" },
                    { new Guid("3297cc84-ef28-459e-a677-98aeab40d6e2"), false, "Electrical Engineering", "H5000" },
                    { new Guid("3299f015-d343-4da4-bda3-eb52a8a84e31"), false, "Industrial Design", "W2302" },
                    { new Guid("32d38308-1410-462b-a54e-8864bab2afce"), false, "Human Sciences", "L3405" },
                    { new Guid("32daed5f-4d3e-4258-a995-2f4feabd3223"), true, "radio studies", "100921" },
                    { new Guid("3339922b-63a6-4b9e-90bd-8c6dd581ec1f"), true, "systems engineering", "100188" },
                    { new Guid("3347ecf7-5cdf-44d2-b54d-f7a9e423e635"), false, "Literature and Communications", "P4601" },
                    { new Guid("3354f48b-2324-47f2-a222-21c1eba171f8"), false, "Personnel and Social Education", "L8203" },
                    { new Guid("336c0a90-dcc2-4812-aabb-b4ba0fb08d0d"), false, "Literature & Communic Studies", "P4603" },
                    { new Guid("336c1b58-f68a-465b-a4bf-fcdf28e56f91"), false, "Swedish", "R7200" },
                    { new Guid("3377f679-bf3d-43a1-a87c-e7a894410869"), true, "molecular biology", "100354" },
                    { new Guid("33cd2078-bad8-4bbf-8e11-732f3f557f8e"), true, "Russian languages", "100330" },
                    { new Guid("33edfadf-4ca1-47fa-a4bc-d61a8ebd6caa"), false, "Foreign & Community Languages", "Q1301" },
                    { new Guid("33fcf7a4-a2a3-47c4-aad2-8c4a8f5a4c15"), true, "molecular genetics", "100900" },
                    { new Guid("3413aa59-0a14-4aea-9464-51af33fadcba"), true, "study skills", "101090" },
                    { new Guid("3453f5cc-a4c9-4888-8cd5-26fcf5e9b729"), true, "Egyptology", "100787" },
                    { new Guid("3462b0b8-edf2-4902-96dc-315ea21356e0"), true, "Japanese languages", "101169" },
                    { new Guid("346759df-c8d8-43e4-a1e1-4c1314743ff8"), false, "Czech", "T1400" },
                    { new Guid("348312fe-b121-42cd-b176-04810fb7b6c5"), false, "Bacteriology", "C5000" },
                    { new Guid("34c02db5-bd41-44eb-a17e-75fca7c678aa"), false, "Recreational Management", "X2001" },
                    { new Guid("34e627ec-864b-43cc-a955-e38e3c84db2d"), false, "Geography (Not As Physical Science)", "L8000" },
                    { new Guid("34eacf01-b9c1-49c4-ad05-c9d49e21edb6"), false, "Visual Studies", "W1504" },
                    { new Guid("34f89dce-cfd0-4e70-bbff-beec08b16549"), false, "Music In Education", "W9919" },
                    { new Guid("34fa8d5b-d5ba-4f4e-963b-f10e09446509"), false, "Political Science", "M1005" },
                    { new Guid("35015519-1f04-4102-b909-7e61de13e4b3"), true, "publicity studies", "100919" },
                    { new Guid("3542a8ce-5347-41e7-9173-319615d198b6"), true, "information systems", "100371" },
                    { new Guid("3568d883-64a0-4bd0-8023-0238447fd402"), false, "Fashion", "J4601" },
                    { new Guid("357306a9-01e3-49ec-a84c-b988996b7794"), false, "Nutrition", "B4000" },
                    { new Guid("35ddee6c-ef6c-414d-b482-e8781360e086"), false, "English and Drama", "Q3011" },
                    { new Guid("35fe2cfa-df05-4da3-ab16-aa60c437dfe1"), true, "psychology of memory and learning", "101342" },
                    { new Guid("360e84d7-f7b2-49d4-b3c0-a644cb0cd1bd"), false, "Home Economics", "N220" },
                    { new Guid("36682f4e-def2-4193-b69f-9e79ad830b92"), true, "Turkish society and culture studies", "101504" },
                    { new Guid("36885588-cafd-477e-a57d-dd513f378c11"), false, "Geography", "L700" },
                    { new Guid("369b957b-d9a7-46f4-8d65-402612c5904c"), true, "environmentalism", "101510" },
                    { new Guid("36b2d76b-3818-4add-b19b-1ee5a780d79a"), true, "Stone Age", "101437" },
                    { new Guid("36cade99-c6e0-48ba-bf3c-ebe32940c1b9"), true, "petroleum geology", "101105" },
                    { new Guid("37a477a0-a241-49f6-9a4f-c814c2cbfe53"), false, "Visual Art", "W1500" },
                    { new Guid("37da781a-3e55-4a57-b022-e45b18a1fffd"), false, "Russian With German", "R8206" },
                    { new Guid("382164d8-d363-475f-b664-f2667988ce06"), false, "Personal,social and Careers Ed", "X9010" },
                    { new Guid("385427b7-ac55-490a-a79c-59a0e49cc296"), false, "Handicraft Teachers Diploma", "10584" },
                    { new Guid("386aadb1-aaa1-4032-8358-a1eb15271dcb"), false, "Home Economics (Design & Tech)", "W9912" },
                    { new Guid("388f81a8-47e4-40f1-9cc6-064979e119ff"), true, "translation studies", "101130" },
                    { new Guid("38cd7540-feff-4444-a796-c23299c976c7"), true, "fashion", "100054" },
                    { new Guid("38dab944-d3cf-4bfa-b9e4-9cba848bb05b"), true, "optometry", "100036" },
                    { new Guid("38e8fe1a-2432-4769-b16e-0c6220f35439"), true, "primary teaching", "100511" },
                    { new Guid("38ea9ff7-58f6-4628-92a4-90da306a8e6b"), true, "phonetics and phonology", "100971" },
                    { new Guid("3917bcb4-5821-4158-a7be-7e652535c9d3"), true, "music production", "100223" },
                    { new Guid("391a7b71-6d81-46d7-b84f-a1e6e4b1b59f"), true, "satellite engineering", "100118" },
                    { new Guid("391adbf7-6c0e-4812-b74f-5b4e0ea14554"), true, "research methods in psychology", "100959" },
                    { new Guid("393a9694-85de-4e95-9480-4d5f50a2069a"), true, "environmental geoscience", "100380" },
                    { new Guid("39631f6c-67d4-4cb4-b020-be6eafd75d17"), true, "human genetics", "100898" },
                    { new Guid("39b9a251-fdb6-47de-94f8-eccf786fafc6"), true, "public policy", "100647" },
                    { new Guid("39c345fe-34a2-4d20-b0e0-eafdb8271909"), true, "safety engineering", "100185" },
                    { new Guid("39ea8e94-9ee1-4132-bf0a-97cb171a3c8f"), false, "Travel and Tourism", "N800" },
                    { new Guid("3a21e625-4ad8-4507-b4d2-fff1c8813232"), true, "children's nursing", "100280" },
                    { new Guid("3a3d37bd-826b-41d3-a40b-41f86449227c"), true, "environmental biotechnology", "100136" },
                    { new Guid("3a45a810-a5af-447f-b3d2-2c424ddef3f7"), false, "Metalwork", "W6100" },
                    { new Guid("3a4f748c-e038-4d47-994c-d375200f2c46"), true, "biblical studies", "100801" },
                    { new Guid("3a574eb1-219d-47e3-8a54-5bf4e3f1b60b"), true, "orthoptics", "100037" },
                    { new Guid("3a638c16-b253-4f12-9dfe-905af7cc3221"), true, "ultrasound", "101330" },
                    { new Guid("3aa251bc-cb76-4f3e-8549-5d6141fd2b15"), true, "environmental risk", "101048" },
                    { new Guid("3adaffc3-1ef7-4309-9c46-1b7d9c1e52ff"), true, "biomedical engineering", "100127" },
                    { new Guid("3adb78ae-7d6b-4b27-80e7-bb5700a0d65d"), true, "transcriptomics", "101377" },
                    { new Guid("3af3868f-4737-49d4-8f7b-964a4e791c16"), false, "General Art and Design", "W8890" },
                    { new Guid("3b25b54f-f2f5-495b-975e-a3ab76b84004"), true, "Scandinavian literature", "101425" },
                    { new Guid("3b34ef4b-48de-4598-b189-c96e49c28c3c"), false, "Environ Science & Outdoor Stud", "F9628" },
                    { new Guid("3b41e62a-6fe2-49cd-9d29-c6871fa239f6"), true, "sociology of science and technology", "100631" },
                    { new Guid("3b8f636e-56e5-4319-be11-0df6847c84d0"), false, "Science In The Enviroment", "F9022" },
                    { new Guid("3bda657a-4133-49f2-b4aa-866bc1c232df"), true, "motorcycle engineering", "100205" },
                    { new Guid("3be9f0cd-eef3-44fd-b242-0a1afd3e014d"), false, "General Biological Sciences", "C8890" },
                    { new Guid("3c07a450-006c-4979-8ea7-92644a20a363"), true, "South Asian studies", "101172" },
                    { new Guid("3c22b1c3-7895-4d00-ad61-ceb7afebe075"), false, "Physics and Science", "F9632" },
                    { new Guid("3c414f92-18e1-4a5e-b83b-c95887e4269d"), false, "Domestic Science", "N7501" },
                    { new Guid("3c67fd37-c855-4f18-8f31-8325f9b8e132"), false, "Jewish Studies", "V1409" },
                    { new Guid("3c7ad07f-2035-4471-b839-4152d1667efb"), true, "computational mathematics", "101029" },
                    { new Guid("3c8f4f25-7219-4092-97ef-0a069944eca8"), true, "Iberian studies", "100765" },
                    { new Guid("3cb2d0b2-7c70-47b8-a651-09dc8cb2de38"), true, "hair services", "101374" },
                    { new Guid("3cd9b0c1-3bc0-4661-ae9a-28f0052fac05"), true, "cultural studies", "101233" },
                    { new Guid("3cdf6ac7-ef7b-4623-b140-4d3bdfd953fb"), false, "Applied Biology", "C1100" },
                    { new Guid("3cec73b4-4a12-4476-8b02-e5bb6da83d9d"), true, "alternative medicines and therapies", "100234" },
                    { new Guid("3d025e1e-1322-4598-97a0-7bbf58f7e49c"), true, "sports coaching", "100095" },
                    { new Guid("3d09c78c-5a80-4eff-9bf6-ce99f49da31e"), true, "agricultural technology", "101006" },
                    { new Guid("3d10c47d-0d40-4ecd-8613-2e25073a42be"), false, "Afrikaans", "T7007" },
                    { new Guid("3d661752-70e8-4798-a671-674c8c25ad8c"), true, "geology", "100395" },
                    { new Guid("3d68cafd-9943-4478-b1e5-cf04749df428"), false, "Early Years", "X110" },
                    { new Guid("3d85e3e9-45b7-4115-8e34-28bc9f49e669"), false, "Modern Greek", "T2400" },
                    { new Guid("3dd1508e-4cfe-4d07-b2c9-2a05abfe43d2"), true, "transport engineering", "100154" },
                    { new Guid("3e05f006-d481-4af7-98af-17ff4199f0e4"), false, "Environmental Studies", "F9002" },
                    { new Guid("3e07d15f-be5c-440c-97dd-665fbca8d92a"), false, "Applied Biology", "C110" },
                    { new Guid("3e41820f-d187-4bc1-8b8e-3f39c9cc871f"), true, "Russian studies", "101151" },
                    { new Guid("3e605bdf-3fd6-4a0b-aadb-9babcd1b2815"), false, "Studies In Humanities", "V9005" },
                    { new Guid("3e7409e6-d66a-4506-8ad3-ef9e24ba4c08"), false, "Classical Studies", "Z0031" },
                    { new Guid("3ed0000f-80f8-4463-840c-50079ca5a3c0"), true, "volcanology", "101081" },
                    { new Guid("3ee501aa-8691-4f43-afc0-873215dfdb25"), true, "biomolecular science", "100948" },
                    { new Guid("3ee588ff-9e30-4021-ae11-b38ac59481e7"), false, "Recreational Studies", "X2006" },
                    { new Guid("3eee0122-51d4-4b42-83be-5eefdb8206e3"), false, "City and Guilds Farm Machinery", "10218" },
                    { new Guid("3efbe302-6f37-4bf2-a531-1ac9fca9b6a8"), false, "Handicraft: Overseas Quals. In Handicraf", "289" },
                    { new Guid("3f0f94f2-66ae-44f2-abfb-4f47f624da60"), true, "health studies", "100473" },
                    { new Guid("3f14daf8-a77c-476e-908b-c76015904356"), true, "planning", "100197" },
                    { new Guid("3f1d3ac4-f2d9-458d-9781-f48264fac94c"), false, "Music and Drama", "W3300" },
                    { new Guid("3f2dcbb4-42e8-47d4-a5c1-03203be5d20a"), false, "Pharmaceutical Chemistry", "B9400" },
                    { new Guid("3f5b01de-4f79-40bb-bcb7-4329439bc87d"), true, "environmental engineering", "100180" },
                    { new Guid("3f8565ba-426e-49f4-948e-b87bdb8e0c95"), false, "Environmental Biology", "C1600" },
                    { new Guid("3fcd12f3-07b3-4a93-b816-6fa84725ce82"), true, "agricultural geography", "101407" },
                    { new Guid("4014b436-25ed-46d4-9acd-58392e6483d3"), true, "manufacturing engineering", "100202" },
                    { new Guid("403432e9-9cf3-44a1-b0d3-0b8e9ed11324"), false, "Combustion Science", "H8601" },
                    { new Guid("4043db20-6edf-46ac-bc5e-6ff66f832ea4"), false, "Expressive Arts (Drama)", "W4004" },
                    { new Guid("404e89a5-86c5-4094-929f-d71a200696a0"), true, "negotiated studies", "101275" },
                    { new Guid("405dc7fc-e885-4395-b528-134fce190ee4"), true, "property development", "100586" },
                    { new Guid("40e061ea-ac5e-4d4c-be7f-7899809163e5"), false, "Rural Science", "F9008" },
                    { new Guid("4126e118-da61-4823-915d-6ea71075c0f7"), false, "Russian Language & Studies", "R8101" },
                    { new Guid("4141853b-ba3c-4103-b66f-791e53a3478c"), false, "Classical Languages", "Q8101" },
                    { new Guid("414f44ef-d513-4bdc-9a09-7114854c7b57"), true, "social psychology", "100498" },
                    { new Guid("41506fe7-2976-42de-a3c3-0a152036c2c8"), true, "aquatic biology", "100848" },
                    { new Guid("41acb1d6-0c23-4c4a-a5bc-a9f5a90070fd"), true, "diagnostic imaging", "100129" },
                    { new Guid("41bc9dc1-ab51-48d3-97f5-1af495b6e739"), true, "moving image techniques", "100887" },
                    { new Guid("41bd0fb3-1463-4142-ba02-c09b1b79c989"), true, "dance and culture", "101453" },
                    { new Guid("41d32719-868c-42d2-95df-a243210f9a0a"), true, "medical microbiology", "100907" },
                    { new Guid("41fe4f2e-ef27-4feb-b8ea-8edd9b5fd109"), true, "Latin American studies", "101199" },
                    { new Guid("4228b95a-5d26-48b6-9885-c8f13638acbe"), true, "aerodynamics", "100428" },
                    { new Guid("42435b6e-bc8c-4e40-b570-bb8c3d23a9a7"), true, "operating systems", "100735" },
                    { new Guid("425cbd91-96fe-4ad0-9a38-b0642b6714a8"), false, "Communication Studies", "P8830" },
                    { new Guid("428611ab-c8ca-47ac-ba2b-0c520d49225d"), true, "law", "100485" },
                    { new Guid("42a0a43b-202d-43e7-a4c2-e1188668c5ca"), true, "earth sciences", "100394" },
                    { new Guid("42dae18e-f586-46b4-8ed0-881860446113"), true, "cardiology", "100748" },
                    { new Guid("42e392da-9099-4f04-a53e-3a7c485f328c"), true, "computer vision", "100968" },
                    { new Guid("431dd705-e42c-4d7c-8d7d-eee22d445e9f"), true, "Latin American literature", "101201" },
                    { new Guid("432dd487-fb3a-4cc8-ab58-ea3798215396"), true, "Sanskrit studies", "101115" },
                    { new Guid("433b27c1-6427-4bdd-81f8-a29195820f3b"), true, "acting", "100067" },
                    { new Guid("4356579d-443b-4c92-84f8-f482d7dde009"), true, "European Union law", "100680" },
                    { new Guid("435b8cf2-a928-4462-aeb2-85a8314e4a8b"), false, "Animal Science", "D2200" },
                    { new Guid("435e6461-eead-403b-a7ca-a4b044efe05e"), true, "product design", "100050" },
                    { new Guid("438ca274-2324-478a-8af0-f190c118f445"), true, "avionics", "100117" },
                    { new Guid("43b4797b-cc46-48a9-bd88-c1a6915560fa"), true, "online publishing", "100927" },
                    { new Guid("43fc6a84-a915-4b95-84fe-968b01d6887a"), true, "early years education", "100463" },
                    { new Guid("440351ce-b3c4-4741-806c-f0e90586ffaa"), false, "English As A Second Or Other Language", "Q330" },
                    { new Guid("440dcd61-0e38-4e47-947d-4ffc31e894ad"), true, "Spanish society and culture", "101138" },
                    { new Guid("44363260-f784-4960-ad48-b77ed364c90f"), false, "Engineering (Tech: Science)", "H8701" },
                    { new Guid("443744a3-b0d6-45eb-bc4e-5d94b458936f"), false, "Photography", "W5500" },
                    { new Guid("444c8dca-dd6a-4576-a563-6df66d831391"), true, "psychopharmacology", "101464" },
                    { new Guid("44692e69-7ed2-44b9-b647-50b1863f824f"), false, "Outdoor Activities", "X2018" },
                    { new Guid("446e32dd-82a8-4913-9b01-9acce8bf79f6"), false, "Language and Communications", "Q1409" },
                    { new Guid("44ad6021-e2b0-457f-8e96-53bcc09514ab"), true, "marketing", "100075" },
                    { new Guid("44b618a8-cf12-4283-9ced-6449b530f44b"), true, "photography", "100063" },
                    { new Guid("44d8f33b-fd00-4837-a910-b70bf283d7db"), false, "Operational Rsearch Techniques", "G4500" },
                    { new Guid("44e3757e-64ac-416d-88c0-868857dd617f"), false, "Offshore Engineering", "H3600" },
                    { new Guid("453e5c45-f45c-427d-8288-9eb383885706"), true, "staff development", "100861" },
                    { new Guid("45553b52-4e17-4c22-bd41-898292a13d6f"), true, "fascism", "101509" },
                    { new Guid("45738b76-2673-44da-81f5-d083cfc62d2a"), true, "meat science", "101387" },
                    { new Guid("45a38648-6141-417a-baa0-83eee909ec92"), true, "clinical practice nursing", "100746" },
                    { new Guid("4663e668-63d8-4f19-92b1-05d8d8be11a8"), true, "metal work", "100721" },
                    { new Guid("46d3ea6a-fe49-48b7-8872-9d6fb138375a"), true, "clinical physiology", "100258" },
                    { new Guid("475abd69-35ca-48b6-b760-1f33ed40c5c6"), true, "French language", "100321" },
                    { new Guid("476dc800-47ca-4193-83f8-d303a143945f"), true, "Scottish history", "100311" },
                    { new Guid("476e1968-f1cf-48fe-8750-d243185de639"), true, "economic history", "100301" },
                    { new Guid("477b6d3a-21ff-4bb2-a1a0-2c25e1e6e338"), false, "Museum Studies", "P1500" },
                    { new Guid("478a448e-c3a4-41ea-84ab-e37db1132b55"), true, "Canadian studies", "101205" },
                    { new Guid("47b75dd9-62cd-4fd3-8f74-e6f7d19b6170"), false, "Primary Foundation", "X121" },
                    { new Guid("47c69434-0b15-4ce9-81dc-958a18eae221"), true, "Ukrainian language", "101429" },
                    { new Guid("47d53963-a316-459b-be64-45fda5a11a67"), true, "game keeping management", "100979" },
                    { new Guid("47f9bb11-9b64-424e-8684-54f1f762dee2"), true, "body awareness", "101452" },
                    { new Guid("48285645-14b8-44bb-83f2-ff5aa017d4b5"), true, "multimedia computing science", "100737" },
                    { new Guid("4853d8e8-7dcf-46ae-9faa-ebf05254e9f7"), false, "Design & Tech-Food & Textiles", "W9908" },
                    { new Guid("485c837a-1cf7-4aa4-9f98-71cd1982a07c"), true, "philosophy of science", "100338" },
                    { new Guid("48a093d9-c4b9-4816-bd00-50624b5b680b"), false, "General Technologies", "J8890" },
                    { new Guid("48bdcabb-54c9-42e9-87c8-d3a2b0e05cfc"), true, "work placement experience (personal learning)", "101276" },
                    { new Guid("48f58d75-2aa2-4f3d-8a31-9ca06c443a32"), true, "microeconomics", "101401" },
                    { new Guid("48f697fe-dea1-4327-a218-824316b553ee"), false, "Bilingual Education", "Q1411" },
                    { new Guid("491b0a6f-af41-4971-b576-03e3c78e03bb"), true, "medical genetics", "100899" },
                    { new Guid("493bb547-cb22-42e4-839b-e37bac538ec6"), false, "Design (General)", "W2004" },
                    { new Guid("49a92360-d5c1-4aa2-a874-29b02a5f45d7"), false, "Other", "Z000" },
                    { new Guid("49aa7c26-5773-413c-b671-bfb6c638cf75"), true, "Portuguese studies", "101141" },
                    { new Guid("49baaed7-fdba-4cc8-aa6a-5d332c0bb9db"), false, "Speech Therapy", "B9503" },
                    { new Guid("49bc87b2-3e2d-47a2-af40-ab998ab7968e"), true, "East Asian studies", "101271" },
                    { new Guid("4a0e49c9-0e6e-491c-8e3e-daca26954215"), false, "Business Studies", "N100" },
                    { new Guid("4a35a92c-64cd-48f5-aa96-49afafd4d826"), true, "social anthropology", "100437" },
                    { new Guid("4a5a0451-3233-43b8-be80-62ce9cf31317"), false, "Other Sciences", "ZZ9007" },
                    { new Guid("4a88f169-3c72-4f44-a9fc-3f52257e2649"), true, "business law", "100482" },
                    { new Guid("4a9b0a77-b3f3-4628-833e-73f60205b55e"), true, "natural sciences", "100391" },
                    { new Guid("4ab47749-da2a-4a70-935d-7aa72c53f277"), false, "Theology and Religious Studies", "V8880" },
                    { new Guid("4b444a5b-ba54-4bae-927b-e04d681115f9"), true, "paediatrics", "101325" },
                    { new Guid("4b574f13-25c8-4d72-9bcb-1b36dca347e3"), true, "food and beverage studies", "101017" },
                    { new Guid("4b954b11-8c87-4047-9ef3-f7c0c2ebde1f"), true, "electromagnetism", "101391" },
                    { new Guid("4bd2f549-1abe-48bc-a445-07637d6934fd"), true, "palaeontology", "100398" },
                    { new Guid("4be3fef8-8089-4bfa-987d-459551b48430"), true, "acoustics", "100427" },
                    { new Guid("4c7c502f-5ca6-4825-91e3-7df3f5a59faf"), true, "art psychotherapy", "101320" },
                    { new Guid("4c7fd913-9bd3-4f59-80ec-c3dba706e10f"), true, "adult education", "100454" },
                    { new Guid("4c91326d-0935-441c-acb1-93e53b02342b"), true, "theoretical physics", "100426" },
                    { new Guid("4cb166c6-3467-43dc-b30d-49def64a0f22"), false, "History Of Music", "W3200" },
                    { new Guid("4d066777-81b5-4d37-a41b-588c56884cd8"), false, "English Linguistic Studies", "Q1002" },
                    { new Guid("4d0fda31-4099-404a-9de2-e352c65be4b4"), true, "sales management", "100851" },
                    { new Guid("4d787ede-5554-4332-afa0-ead82e8596ad"), true, "Gaelic language", "101120" },
                    { new Guid("4d8a4f2b-dae3-4108-ba6c-c7d9b2f8c818"), true, "exercise for health", "101319" },
                    { new Guid("4dbb8509-87d1-42da-8af8-f6538d3b6656"), false, "Natural Philosophy", "F3001" },
                    { new Guid("4e5eef54-e052-453d-8be3-7ae3df6440ca"), false, "Spanish Studies (In Translation)", "R4100" },
                    { new Guid("4e61c720-032c-446b-837e-942ba7fc52d0"), true, "international marketing", "100853" },
                    { new Guid("4e6731e8-11f4-40b5-986d-882d4f735828"), true, "economic geography", "100665" },
                    { new Guid("4e7756cf-1532-4386-af10-16dc79f3afc5"), true, "pollution control", "101072" },
                    { new Guid("4ea17408-32d5-4419-beaf-289c047fbc62"), false, "Modern English Studies", "Q3100" },
                    { new Guid("4f20b6db-07c7-424d-8b89-c63660a47e9c"), true, "Welsh history", "100760" },
                    { new Guid("4f4bb434-3bde-43da-b9cb-0f609baae4c8"), false, "Spanish With French", "Q9709" },
                    { new Guid("4f5c56cc-76ce-455a-b03b-fc0cc75bf7b8"), false, "Geography and The Environment", "L8202" },
                    { new Guid("4f78c615-382c-4253-889d-2d16db76ab73"), true, "neonatal nursing", "100289" },
                    { new Guid("4f84d5f2-6045-41a3-8742-a9bc970d037b"), true, "plant sciences", "100355" },
                    { new Guid("4fa48204-19b4-4ba2-b6ff-f1b8c3c1d0eb"), true, "toxicology", "100277" },
                    { new Guid("4fb480a9-4a15-48d2-bf4e-476455a78fb8"), true, "directing for theatre", "100697" },
                    { new Guid("4ff3116a-2507-4b5e-89a1-ca2fac9e050a"), true, "child care", "100654" },
                    { new Guid("50432b25-e158-4b8c-aa22-05b423884474"), false, "Classical Greek Language", "Q710" },
                    { new Guid("5063eae9-1d19-4fdb-b79d-08bf1fb1967e"), true, "computer games graphics", "101019" },
                    { new Guid("50b6d73a-25b4-41fe-a5c2-7784f21343ab"), true, "film music and screen music", "100842" },
                    { new Guid("50c7b73d-793d-49c9-b0b9-dfc20072e558"), true, "financial risk", "100835" },
                    { new Guid("51419d04-4602-469c-8f2d-2acdb86d34e0"), true, "astronomy", "100414" },
                    { new Guid("5179e94b-3c9c-4fbb-b2ad-f4f9d67e2d89"), false, "Combined/General Science", "C000" },
                    { new Guid("519df0ff-b4be-4e82-b135-9be576756343"), true, "investment", "100828" },
                    { new Guid("51b1a620-1203-4b16-b645-3914aba8ca87"), true, "physician associate studies", "100750" },
                    { new Guid("51baac39-e6f1-4a78-b4f2-e1a27a59238a"), false, "Youth & Community Studies", "L5206" },
                    { new Guid("51d91e5a-c91d-4dd6-ad83-39d7d5b3c39c"), true, "applied mathematics", "100400" },
                    { new Guid("51f4fae2-ffd5-4856-99b1-8c147b1783fe"), true, "Hindi language", "101174" },
                    { new Guid("52286b44-0fe6-495a-aca0-6a45e992825a"), true, "Robert Louis Stevenson studies", "101489" },
                    { new Guid("523f9f94-d612-464f-9d67-4f1a0ee69657"), true, "mechanics", "100430" },
                    { new Guid("52798172-6ead-46f8-bdfd-643d75f1e9b5"), true, "politics", "100491" },
                    { new Guid("5327ea16-d703-4436-bae6-95ffc440ee53"), true, "theology and religious studies", "100794" },
                    { new Guid("53557635-d831-4a7e-be85-2ca829131153"), true, "furniture design and making", "100633" },
                    { new Guid("5366fe16-a2f6-412b-965a-b89facfec4df"), false, "Educational Computing", "X9011" },
                    { new Guid("5394f825-dba3-44dc-8fc7-0df0e56685db"), true, "English literature 1700 - 1900", "101095" },
                    { new Guid("53964d69-6263-47c3-979a-eb1bc87f7fbd"), false, "Creative Arts (Music)", "W3002" },
                    { new Guid("53ac82df-04a9-4d66-8526-9e91a9130113"), false, "Human Biology", "B1500" },
                    { new Guid("54449a21-4aca-4ac9-bd4c-26eabfd69f3b"), true, "combined studies", "101273" },
                    { new Guid("54547302-e37d-4417-a4a0-310263373eeb"), true, "food science", "100527" },
                    { new Guid("547bbce7-8493-4870-a8b7-9450ef068fa4"), false, "Outdoor and Science Education", "X9012" },
                    { new Guid("54bb102b-e6bd-4394-ad80-54d58bc7f8c7"), false, "Science In The Human Environment", "F9013" },
                    { new Guid("54e4ab48-c2e4-43bc-801b-1c663637d6d8"), false, "Interior Design", "W2300" },
                    { new Guid("5502f1f7-4698-4d3b-a906-755224c9a224"), true, "mining engineering", "100204" },
                    { new Guid("5522426b-1a63-4004-8050-afa10c46d977"), false, "Economics With Business Studs", "L1005" },
                    { new Guid("552796db-2b49-4c50-a378-8687d6f58dc0"), true, "forensic anthropology", "101218" },
                    { new Guid("55302d2d-8d73-43b7-ac84-119e8b2e1616"), true, "management and organisation of education", "100817" },
                    { new Guid("553308ae-51ff-4cb8-ab88-d67a2d88253b"), false, "Teach Eng -Speakers Other Lang", "Q9706" },
                    { new Guid("556f4eb6-5cbc-487e-9af2-cd15436ef417"), true, "urban and regional planning", "100199" },
                    { new Guid("55b2e32c-9c1d-4f31-aa07-256ab91924a2"), true, "petroleum engineering", "100178" },
                    { new Guid("55b34741-a989-4114-b47a-b3c03f9436b6"), false, "Expressive Arts(Visual Arts)", "W1505" },
                    { new Guid("55b708b0-a85d-4f3c-8f7c-5db56d387feb"), false, "Physics With Core Science", "F9613" },
                    { new Guid("55bec1cf-3528-4bd5-94d4-8953d219c699"), false, "Dance", "W500" },
                    { new Guid("55ce4249-81fc-4645-80cd-9452bcd5258e"), false, "Teacher Training", "X8810" },
                    { new Guid("55dd7f60-64f9-4964-aed3-ba4eda386895"), false, "Gen Asian Lang, Lit & Cult", "T8850" },
                    { new Guid("55f70db7-0d45-42b2-a4b1-694455afc9e7"), false, "Emotion and Motivation", "C8002" },
                    { new Guid("56587ef8-0534-44eb-84b2-1804a48bd063"), true, "African society and culture studies", "101189" },
                    { new Guid("565fd87c-c69a-4c60-b54f-d55626cd44e4"), true, "international history", "100778" },
                    { new Guid("568f1cbf-7a3d-4413-ac01-d6cd9504cd29"), false, "Music Studies", "W9920" },
                    { new Guid("56b3abea-cfec-4717-84cc-3027d731fd4a"), true, "plant physiology", "101460" },
                    { new Guid("56b8afc3-545d-4089-b4b6-aa12106ebec0"), true, "paramedic science", "100749" },
                    { new Guid("56c84480-02c3-4f84-9cc7-0c99b64b167a"), true, "corporate image", "100856" },
                    { new Guid("56c866d8-38ee-4c2a-81bb-784351350695"), false, "History Of Education", "X9007" },
                    { new Guid("56da3835-805c-4fdb-b326-3e6691c37f2a"), true, "Latin American history", "100769" },
                    { new Guid("56e31208-a3cb-4bb8-ab68-2ffa20cf3683"), true, "architecture", "100122" },
                    { new Guid("57170354-f7a4-4bef-bcce-e75704ea6879"), true, "museum studies", "100918" },
                    { new Guid("5721d3c3-1806-440f-ae30-69168c4a6b7d"), false, "Maths and Info. Technology", "G9006" },
                    { new Guid("573c51f4-b452-46a8-9ef6-a30880bb7cdc"), false, "Expressive Arts (Art)", "W1005" },
                    { new Guid("574eab5a-a458-41b9-b342-25b92f86ca3c"), false, "Russian Lang, Lit & Cult", "R8880" },
                    { new Guid("576400f1-4d1a-49e2-ab25-b3d556fcda2f"), false, "Environmental & Social Studies", "L8201" },
                    { new Guid("5766a04a-416c-45ed-b595-4870617b8835"), false, "Numeracy", "G9002" },
                    { new Guid("579e416c-93b0-4644-a27e-a758e6bdc8f5"), true, "veterinary public health", "100942" },
                    { new Guid("57b5a78f-71e0-4f85-81c7-0d9e51c4261b"), false, "American Studies", "Q4000" },
                    { new Guid("57cfe593-fed9-4fc4-8baa-d3b4854fbd0c"), true, "forensic biology", "100386" },
                    { new Guid("583434ba-6e54-4a9a-9e6e-319cbc2462b5"), true, "feminism", "101403" },
                    { new Guid("586f80c0-a796-46dc-bd8a-2f258d80a5e2"), false, "Business Education", "N1211" },
                    { new Guid("58830e60-73ec-463d-8fc5-dd50006880cf"), true, "remote sensing", "101056" },
                    { new Guid("58917db9-66a6-4bf0-ab9b-e6f84a697a03"), true, "accountancy", "100104" },
                    { new Guid("58bc51a7-9817-4d4d-8413-162117083123"), true, "Swedish language", "101148" },
                    { new Guid("58bef565-e0f5-4e6f-963e-7fc9ef70e4fa"), true, "international hospitality management", "100087" },
                    { new Guid("58d5d357-04d8-4a43-a4b1-c92e818e0fa1"), true, "philosophy", "100337" },
                    { new Guid("58ebe09f-cb1a-422b-87e4-7af97ca1cebb"), false, "Computer Education With Maths", "G5007" },
                    { new Guid("5925db8d-306c-43a5-be80-6fe72216b504"), true, "German literature", "101134" },
                    { new Guid("598ea4ef-f7fa-4d5c-aa7c-5d6b1a3e87f0"), false, "Organisation & Methods", "N2001" },
                    { new Guid("59917cbc-5ebe-4499-bd24-33b6116efdc3"), true, "artificial intelligence", "100359" },
                    { new Guid("59a25822-d064-4bb8-be7a-0a4bb7831596"), false, "Design & Tech (Cdt/Home Econ)", "W9904" },
                    { new Guid("59d77f50-752f-4f1f-8828-c02c7f1e5681"), true, "econometrics", "100604" },
                    { new Guid("59fa7ec1-29e4-4e5f-85ef-cfa01cdf30ee"), true, "Portuguese society and culture", "101144" },
                    { new Guid("5a728384-8cf9-4872-88f1-3505b5dd6c8b"), true, "classical Greek literature", "101423" },
                    { new Guid("5a877c5b-3809-4607-a214-9db06cffad7e"), true, "sport technology", "101379" },
                    { new Guid("5aa404eb-bb50-4157-8552-c82286519c0e"), true, "calligraphy", "101362" },
                    { new Guid("5adde787-4c4e-433e-940f-3361e5dfdd32"), false, "Science Education", "F9023" },
                    { new Guid("5b0cc7b1-4341-4706-9276-08cf8593c58c"), false, "Sculpture", "W1200" },
                    { new Guid("5b0cca8e-dd95-44d6-a155-278a6c73d69b"), false, "Rural & Env Sc (With Integ Sc)", "F9622" },
                    { new Guid("5b398a15-d5cc-42c5-9ce3-206683576887"), true, "applied science", "100392" },
                    { new Guid("5b3e925e-ff48-481e-958c-ca423e2b26af"), true, "food marketing", "101215" },
                    { new Guid("5b8976d3-916f-4d7e-8f75-3f74c9a16d77"), true, "pre-clinical dentistry", "100275" },
                    { new Guid("5bb25bcb-3502-47e8-a0ff-bb1b82222d33"), true, "liberalism", "101506" },
                    { new Guid("5bc8aa6c-ef41-4b52-be3d-468d7e9e4615"), true, "facilities management", "101308" },
                    { new Guid("5bcec838-7f13-4afd-a54d-fd1c04030e54"), true, "family history", "100779" },
                    { new Guid("5bfc7bbf-9b23-47ac-93c2-11dc21761d1f"), true, "Dylan Thomas studies", "101493" },
                    { new Guid("5c11c9bc-111e-496e-bfde-375c435caa2e"), true, "history of medicine", "100785" },
                    { new Guid("5c3764a8-8dd1-48d2-8054-e3adcaf9f329"), true, "animal nutrition", "100940" },
                    { new Guid("5c43ee62-a862-4696-95e5-e29f9b31974b"), true, "veterinary nursing", "100532" },
                    { new Guid("5c63d2e6-06a9-4d4b-964b-16bb9ff09320"), false, "Handicraft: City and Guilds Of London In", "211" },
                    { new Guid("5c6671a5-7035-4848-a127-9efc1b0b0fb5"), false, "General Studies In Arts", "Y3000" },
                    { new Guid("5c72de01-669e-46b1-9c42-d3807cc9cf6e"), true, "film directing", "100888" },
                    { new Guid("5c89579c-a863-4092-a640-f558dc7754fb"), false, "Applied Chemistry", "F1100" },
                    { new Guid("5c933a7e-45bc-41eb-8750-75974b52d19c"), true, "neuroscience", "100272" },
                    { new Guid("5ce88fd5-c56e-484e-8fd7-4a9db9fe04a8"), true, "clinical engineering", "100005" },
                    { new Guid("5cfb55a2-8bc1-447f-94db-1cfcb6ef7de6"), true, "security policy", "100652" },
                    { new Guid("5d093a20-86e5-4bd4-9af8-4b7d99b2b38a"), true, "sports therapy", "100475" },
                    { new Guid("5d165020-b3d5-49ed-96c7-823d670ac327"), false, "Physics (With Science)", "F9623" },
                    { new Guid("5d3344e7-2d76-420d-9a51-6fd9f099b0c7"), true, "clinical dentistry", "100266" },
                    { new Guid("5d5202b9-5551-4a13-b9d5-e7365af72872"), false, "Post-Graduate Certificate In Education()", "10886" },
                    { new Guid("5d8c415f-889d-4c7e-89bb-8712d8e6f451"), true, "Judaism", "100797" },
                    { new Guid("5db1e31c-0b2d-4cec-8948-d807587c6ffe"), true, "hospitality", "100891" },
                    { new Guid("5def97b7-415f-4474-9dd8-6b6f6fbbae99"), true, "English language", "100318" },
                    { new Guid("5defee3b-369c-4d11-850f-e1b221c854ae"), true, "Scandinavian studies", "101145" },
                    { new Guid("5e5bf2a6-7b34-472a-a976-5d487723ca6a"), true, "public administration", "100090" },
                    { new Guid("5e780b07-bc44-40a7-b568-363c60a8cd1c"), true, "older people nursing", "100291" },
                    { new Guid("5e8e1bc5-76fe-49f8-998d-87c7703b4306"), true, "social philosophy", "100792" },
                    { new Guid("5e914ab2-6ecc-4ccc-b4ab-74fab27cb4dd"), false, "Chemical Sciences", "F1001" },
                    { new Guid("5ef92d4f-ee77-4b21-a7eb-4ca8e11fbdd6"), false, "Ed For Those With Sn", "X8860" },
                    { new Guid("5efc50ee-821f-4f93-836e-15e2e00e9615"), false, "Tech: Business Studies", "W2504" },
                    { new Guid("5f2be574-e12e-41b9-8e2b-912b3e791d73"), false, "Liberal Studies", "V9001" },
                    { new Guid("5f3fa42f-8e4f-490b-831b-5a4b377dfe99"), true, "Indian society and culture studies", "101179" },
                    { new Guid("5f85f86f-64fb-4055-8c21-99a3d33797c0"), false, "Physical Education", "C600PE" },
                    { new Guid("600f089b-d010-4f0f-9b9a-e0454877cee8"), true, "Caribbean studies", "101207" },
                    { new Guid("6036321d-b9fe-4d08-bea6-cea5a74b366a"), false, "Creative Arts (Art & Design)", "W2002" },
                    { new Guid("604f7027-5e40-4b71-870e-0c0416111659"), false, "Performance Arts", "W4005" },
                    { new Guid("60634adb-265b-472d-a933-0aabf76e09ec"), true, "dietetics", "100744" },
                    { new Guid("60649a80-50cd-41c2-9331-cbd6e898363b"), true, "advice and guidance (personal learning)", "101279" },
                    { new Guid("60943501-351b-445b-9e9a-0b41908e10e1"), false, "Science With Mathematics", "G1501" },
                    { new Guid("60ce90cb-9e55-4735-8937-f0a26985b9d3"), false, "English as a second or other language", "999004" },
                    { new Guid("60fabe1f-e034-43a6-b78b-04b88ed80ef2"), false, "City and Guilds Engineering Planning, Es", "10216" },
                    { new Guid("61646813-5e24-451e-9e2b-805de2577d42"), false, "Environmental and Land-based Studies", "F750" },
                    { new Guid("6177667e-a305-492f-9e56-29d8a612d731"), false, "Fine Art", "W8810" },
                    { new Guid("61c7104e-62db-4afd-80ee-82b3beb80e51"), false, "Ancient Greek Lang & Lit", "Q8870" },
                    { new Guid("61de39af-ea4c-440f-b067-d141803da8ee"), false, "Teaching Diploma In Speech and Drama", "612" },
                    { new Guid("6227e2db-a8aa-4ef2-9dc4-50e8613abd9f"), true, "probation/after-care", "100662" },
                    { new Guid("623d6139-f6b8-4195-ba23-77532df794e3"), true, "dance", "100068" },
                    { new Guid("6252ed64-2767-40af-9958-a5b384a26eed"), true, "architectural technology", "100121" },
                    { new Guid("6272c51d-9606-4a26-8006-e06b80cbd23d"), true, "biological sciences", "100345" },
                    { new Guid("6273dbf8-6d1a-451c-94a2-1ef2aa187284"), true, "Latin American society and culture studies", "101202" },
                    { new Guid("6286ef89-fa59-4722-829b-6b6942d31b65"), false, "Mathematical Education", "G1401" },
                    { new Guid("62900ade-8d65-4998-ab0b-fd1e60137b8f"), true, "French studies", "100322" },
                    { new Guid("62a1db9e-7f37-4b0c-acc9-fd449405613b"), false, "English Lang and Literature", "Q9702" },
                    { new Guid("62aabba7-cf0b-4af1-8ecf-f390dbd19d6e"), true, "tourism", "100875" },
                    { new Guid("62ac6698-ef06-4e8b-a5cc-38c707fa1c8f"), true, "applied biology", "100343" },
                    { new Guid("632d41b6-c3e1-4bf0-ad45-852188e1c0e1"), true, "geological hazards", "101082" },
                    { new Guid("6350b867-dcd8-44e2-8aa9-5256be04c13f"), false, "English Studies", "Q3006" },
                    { new Guid("636969e1-02fe-4364-a66a-5536c03966bb"), true, "social policy", "100502" },
                    { new Guid("63a2dac9-cd6b-400f-a733-7800490ea4be"), true, "climate science", "100379" },
                    { new Guid("63cb6b6a-5a73-4a90-9a75-7fbe9efc45b6"), false, "Expressive Arts (Dance)", "W4502" },
                    { new Guid("644e6c3b-ad86-4765-b7f9-5ef50b8e7d17"), true, "history of art", "100306" },
                    { new Guid("649d7736-d301-4c42-873a-b24486fd35d7"), true, "Physical Education", "999002" },
                    { new Guid("64b0b917-5a26-4de6-a31f-4560da248ade"), true, "comparative literary studies", "101037" },
                    { new Guid("65221774-02a5-47e6-9871-3d2649a947e9"), true, "classical Arabic", "101114" },
                    { new Guid("656b86dd-4490-48a1-ad11-d7ad00f86e66"), true, "sport and exercise sciences", "100433" },
                    { new Guid("658abc1e-d723-430f-aa72-3d79cc53b9d0"), false, "Accountancy", "N4000" },
                    { new Guid("658fb64e-95f3-4cc7-9b02-044ea2842ef3"), false, "Applied ICT", "G510" },
                    { new Guid("65b5a0c6-9899-44e1-b4b3-8001f86cab5a"), true, "Celtic studies", "101118" },
                    { new Guid("65d29394-9eb2-4353-9952-2dec7a624468"), true, "applied microbiology", "100906" },
                    { new Guid("65d7207f-4e8b-4fd4-a96d-61a1a1b3ed58"), true, "Spanish language", "100332" },
                    { new Guid("65e0db6e-6a0f-4743-aa2d-8eb15270506f"), false, "Financial Management", "N3000" },
                    { new Guid("660f24a6-b4c9-4fb7-8280-2786c00f1832"), true, "analytical chemistry", "100413" },
                    { new Guid("660f7262-c9e0-4d2e-a7a3-819cd06d85f3"), true, "marine biology", "100351" },
                    { new Guid("6617e320-3731-4f5b-b94b-da23fdb0a417"), false, "Environmental Chemistry", "F140" },
                    { new Guid("663896c2-2b03-46a4-858b-edf0051e168d"), true, "research skills", "100962" },
                    { new Guid("663fbab6-5647-42c2-8526-92e56ec7e95a"), true, "human demography", "101408" },
                    { new Guid("668094a7-ce45-442b-94ea-627f429be5a9"), true, "civil engineering", "100148" },
                    { new Guid("6688e046-7a19-4b6b-906e-ceff1c3ef85e"), true, "Chaucer studies", "101472" },
                    { new Guid("6697f9e5-03f6-4d7f-8286-0a352a720b91"), false, "Food Science and Nutrition", "D4201" },
                    { new Guid("66c1d9da-76cb-4989-952e-08c5af70a880"), false, "Ecological Studies", "C9003" },
                    { new Guid("66fc85d8-15f5-4654-8a02-84f425e7a571"), false, "Social Psychology", "L7400" },
                    { new Guid("670545e3-63f4-46af-b37d-ddfceb22e7ce"), false, "Creative and Media", "P390" },
                    { new Guid("674b877f-875c-4bc4-a246-7f05e04b1d80"), false, "Biological Chemistry", "C7001" },
                    { new Guid("678e3fda-78fe-4967-81a0-3025f1b6cde2"), true, "clinical psychology", "100494" },
                    { new Guid("67c3c91c-1c02-412a-897d-4b31fdef325b"), false, "Hospitality", "N862" },
                    { new Guid("67d15e48-3ed4-4ad5-a753-4d03009ea056"), true, "offshore engineering", "100152" },
                    { new Guid("67d5028a-1f6d-4b29-bf13-7db7f01d7f10"), false, "Ancient History", "V1100" },
                    { new Guid("681077c5-e4f1-44d7-9904-fed8c3b61aec"), true, "pharmacy", "100251" },
                    { new Guid("682c5a3c-c854-4492-8f93-ceb19a011c6f"), true, "audio technology", "100222" },
                    { new Guid("68402b74-8a2b-4c7c-ae6f-61930e1bfe6c"), true, "phonology", "100973" },
                    { new Guid("686e15bb-1f60-4c73-a966-7861d016467b"), false, "Welsh and Welsh Studies", "Q5205" },
                    { new Guid("68740ca9-570a-4a1a-b494-94988959c3ad"), false, "Medicine", "A3002" },
                    { new Guid("6889c95d-53f7-43ab-b410-da4bb85b761b"), false, "Physics with Maths", "F390" },
                    { new Guid("688ed1a2-b598-420a-a0a5-10607ccf558e"), true, "political geography", "100668" },
                    { new Guid("689509dc-d929-47a7-b132-2e6b48016ba4"), false, "Religious & Moral Studies", "V8004" },
                    { new Guid("689b4033-c5de-463c-a34d-53dcf993d93d"), true, "Iron Age", "101439" },
                    { new Guid("68a48811-bbd4-4139-983c-5fb30054c8d0"), true, "theatre nursing", "100294" },
                    { new Guid("68d4e0e5-7154-41fd-91a2-143bb9b648d4"), true, "structural engineering", "100153" },
                    { new Guid("68dfd22c-ea5c-4a20-86c3-ce61c13e9ef2"), false, "Classical Studies (In Translation)", "Q8100" },
                    { new Guid("68edb66d-a78d-43ad-b363-21ec60687f96"), true, "social care", "100501" },
                    { new Guid("690b74e9-3e35-41a5-a03d-7c8801125552"), true, "economic policy", "100601" },
                    { new Guid("693acc49-a649-4027-8735-847f4c22783b"), true, "electrical and electronic engineering", "100163" },
                    { new Guid("695d5a9a-3caf-4547-9097-a3e298dd0b00"), false, "Applied Chemistry", "F110" },
                    { new Guid("69853a4b-ff4f-45f1-907a-a8cdb442d3fc"), true, "inorganic chemistry", "101043" },
                    { new Guid("69a562cd-3220-4f2f-bece-e96a51e45032"), false, "Brewing", "J8001" },
                    { new Guid("69f63eca-7315-49ef-9517-9b5edc7147d1"), true, "world history", "100777" },
                    { new Guid("69fd837e-1eaa-4cf8-9ca2-7386684989f8"), true, "medieval Latin language", "101421" },
                    { new Guid("6a3b4ecc-437f-48c0-95d0-ae49f2d3c57f"), true, "higher education", "100461" },
                    { new Guid("6a760f22-f973-4392-a874-49c8a1c008aa"), true, "international business", "100080" },
                    { new Guid("6b068c45-7422-42fd-9b46-8b3a2e032da3"), false, "Gen Modern Languages", "T8890" },
                    { new Guid("6b281757-4ed5-4873-b192-fe39ccca593b"), false, "Social Education", "L3403" },
                    { new Guid("6b5d6637-b315-4fb4-ad6d-9df9ebff903b"), true, "primary education", "100464" },
                    { new Guid("6b621dd5-58fc-4e79-9c2f-a3f245cb17b9"), false, "Engineering", "H900" },
                    { new Guid("6b62af01-bd46-4772-8797-695bfe62717b"), true, "geography", "100409" },
                    { new Guid("6b7ec776-9580-4a54-9538-415f884a0801"), true, "geophysics", "100396" },
                    { new Guid("6b80a66e-bbfb-4d8f-9ca7-8f5a1225ea97"), false, "Philosophy", "V7000" },
                    { new Guid("6b8b6116-0214-4d31-b3dc-1bf905a1dc15"), true, "Israeli studies", "101194" },
                    { new Guid("6b8d5921-caa9-43fc-b9fd-a5c429134dc4"), true, "local history", "100308" },
                    { new Guid("6bc8c948-9445-441c-afaf-e8da559e79cc"), false, "Design Related Activities", "W2011" },
                    { new Guid("6bd9a303-5764-4fb9-991c-6b305a3c662b"), true, "further education", "100460" },
                    { new Guid("6bebc908-fdca-40c6-a11d-5779dc0e08ea"), false, "Geography & Environmental Stds", "L8002" },
                    { new Guid("6c18b6e6-97c6-410a-befe-6d010a8aef7c"), false, "Greek (Classical)", "Q7000" },
                    { new Guid("6c49180a-4b10-46f3-a3d8-2ebd75a4aa21"), true, "Charles Dickens studies", "101477" },
                    { new Guid("6c5c4b8d-4775-40fd-bdd8-025bedc16fed"), false, "Science-Biology-Bath Ude", "C1003" },
                    { new Guid("6c7ece7a-95a8-4c72-ad9c-636e099e947d"), false, "B/Tec Nat Cert In Business & Finance", "11200" },
                    { new Guid("6c93c012-8f63-4b40-8380-4509b98952ff"), true, "heritage management", "100807" },
                    { new Guid("6ca63e8f-bcb7-4bfc-8e1f-8e1cbe2395b7"), false, "General Studies", "Y4001" },
                    { new Guid("6cae025c-9b7b-4035-b057-0147b7f31790"), true, "veterinary microbiology", "100908" },
                    { new Guid("6cbe0a30-195c-4737-9758-089f283aa65d"), false, "Music", "W300" },
                    { new Guid("6cd526c4-5964-41a2-ba47-edfb69c9235f"), false, "Finance", "N3001" },
                    { new Guid("6cdf11f1-0d6f-4220-b932-d8d2cb00d8ef"), false, "Physical Science", "F9602" },
                    { new Guid("6cf7abc1-1f45-4f7b-a1e0-f34cb047f710"), false, "Zoology", "C3000" },
                    { new Guid("6d0a531d-9f59-4d66-ba3d-7962207a2158"), true, "water quality control", "100573" },
                    { new Guid("6d2e7516-7f58-486e-b879-5f200eaf64f8"), false, "Brazilian Studies", "R6000" },
                    { new Guid("6d378de3-b16e-46bd-a901-981266aa3ecb"), true, "women's studies", "100622" },
                    { new Guid("6d7b6ee6-b092-4caa-8dde-08dfca8c1252"), false, "Humanities and Restricted Specialisms", "Y000" },
                    { new Guid("6da74eef-b0e3-4f04-be14-fda298b4ed80"), true, "Bengali language", "101177" },
                    { new Guid("6e9e7eea-e05e-45c5-85dc-563f78512d1d"), false, "Management Home Hotel & Institutio", "Z0066" },
                    { new Guid("6ead1d96-794a-49da-85a7-43d33e51d0d9"), true, "education policy", "100651" },
                    { new Guid("6ed905bd-9a9b-4f58-aa84-2c0921e58409"), false, "Combined Arts", "W9914" },
                    { new Guid("6ef4c7e7-0b72-4c09-bef8-80ab05eb0623"), true, "conservatism", "101507" },
                    { new Guid("6f233e09-5ccb-4f7d-a9ea-a572a81c89cd"), false, "Psychology (Solely As Social Study)", "L7001" },
                    { new Guid("6f2f390e-f2bd-4c8f-bd27-3aa650eb7480"), true, "applied social science", "101307" },
                    { new Guid("6f883bc9-01e3-4e13-a709-c157767747b4"), true, "cognitive psychology", "100993" },
                    { new Guid("6f8f8394-0a78-4696-abd1-2289e5ccceff"), true, "colour chemistry", "101042" },
                    { new Guid("702e1c9a-5965-4556-a408-763e6450dc07"), true, "television studies", "100920" },
                    { new Guid("7046d1ea-ad8c-4f70-b47d-a58e59f0254c"), true, "allergy", "101334" },
                    { new Guid("7048cb16-f1e4-4fdc-99ed-806fb7131ef3"), false, "Electronics", "H6000" },
                    { new Guid("7062b144-ca9e-41d7-b763-884820e9f390"), true, "Bob Dylan studies", "101492" },
                    { new Guid("707207b4-3be8-424c-86d8-7d014abba57e"), true, "applied zoology", "100880" },
                    { new Guid("70771493-508f-4927-8aa6-cb509ced6989"), false, "Literature and Drama", "Q2005" },
                    { new Guid("70ff558b-ba72-4407-8e3f-4c4de913867a"), true, "metallurgy", "100033" },
                    { new Guid("71753775-70d5-4e0b-81b8-2dee013c8b9f"), true, "folk music", "101447" },
                    { new Guid("717fb02a-d703-46f4-8872-c87cded72010"), true, "behavioural biology", "100829" },
                    { new Guid("71d2189c-ad28-4a2e-9b5e-a9bc9e21877d"), true, "engineering design", "100182" },
                    { new Guid("725b7561-ecff-4877-bf67-03ff8c2b401d"), true, "urban studies", "100594" },
                    { new Guid("72afa95b-6037-465b-8891-4fa93d58c942"), false, "Byzantine History", "V1003" },
                    { new Guid("72f6c7a4-c596-4b5a-8b82-6e6ce30b12f5"), false, "Behavioural Studies", "L7301" },
                    { new Guid("7308bdb2-cc93-4583-a359-8551b8ec1cdf"), true, "mathematical modelling", "100402" },
                    { new Guid("730de735-5303-474d-b79a-93ec566066dd"), false, "Design and Technology", "W200" },
                    { new Guid("731277ac-272b-4131-b6cc-7a8db657774d"), true, "speech and language therapy", "100255" },
                    { new Guid("732d9f0a-5d01-40ba-ad14-d26896d0ee88"), true, "history of religions", "100780" },
                    { new Guid("7330f94e-adc4-45f7-af24-9b0f26647cdd"), false, "Commercial Management", "N1207" },
                    { new Guid("734d465d-bc38-463f-9478-71b5b85d9e78"), true, "applied sociology", "100619" },
                    { new Guid("735c5632-3a83-4666-b47c-d59d71af41de"), true, "cardiovascular rehabilitation", "101291" },
                    { new Guid("738d4a8d-a2b1-4eb7-b5fb-faf4caa8652a"), false, "History and Social Studies", "V5003" },
                    { new Guid("73a3a509-a5ea-4652-b1a5-613bd35243b9"), false, "Latin American Languages", "R6001" },
                    { new Guid("73adcec9-11ea-4a37-a828-a383accdb329"), true, "Bronze Age", "101438" },
                    { new Guid("73cce602-e78f-437e-95d6-0054c03306d8"), true, "radiology", "100131" },
                    { new Guid("73ed8e44-7b34-4271-880a-087e89ca8045"), true, "crop nutrition", "100946" },
                    { new Guid("740922bd-503a-4666-bd26-caef2c61ddbc"), true, "digital media", "100440" },
                    { new Guid("741188bf-6d81-4e28-91ab-d25e335c04be"), false, "Community Studies", "L5202" },
                    { new Guid("742cabc6-092e-4966-947b-3d367b9b2af1"), false, "Applied Mechanics", "H3001" },
                    { new Guid("743fe59f-1324-40fe-882c-50a96f691118"), true, "electronic engineering", "100165" },
                    { new Guid("7477f9b2-9702-45b5-a34c-65c01add0fac"), true, "Finnish language", "101150" },
                    { new Guid("748bb4da-be4c-4d4b-98c0-b9b8f9517075"), true, "Oscar Wilde studies", "101471" },
                    { new Guid("74da0dee-6009-4bdb-b4a8-531c102b2949"), false, "Clinical Medicine", "A3000" },
                    { new Guid("74e0eb3c-6ceb-491f-89f2-a4e347132120"), true, "recreation and leisure studies", "100893" },
                    { new Guid("74e163bf-4f1c-4ac2-b58c-e703a7f6d857"), true, "modern Middle Eastern society and culture studies", "101197" },
                    { new Guid("750530a1-0439-42be-980d-540be3148a60"), false, "Computing and Science", "G5402" },
                    { new Guid("7518c8b2-be38-42c2-8ae3-aff25afb05b0"), false, "Danish", "R7300" },
                    { new Guid("75298232-873d-46d4-8073-8e6f44ccdb71"), false, "Food Science", "D4000" },
                    { new Guid("752e92ba-3447-444f-aa0a-c75cb4edf765"), true, "war and peace studies", "100617" },
                    { new Guid("75381907-195d-4a4c-9c53-ceca2bde5a22"), true, "medical law", "100693" },
                    { new Guid("753b54c2-2ee5-4140-9f8f-18e7affbddb9"), false, "Food Technology", "D600" },
                    { new Guid("75566116-9a45-41c8-9040-d5701a21a4e6"), true, "retail management", "100092" },
                    { new Guid("7588b46a-c049-47d7-8967-590e4c687d8e"), true, "aromatherapy", "100235" },
                    { new Guid("758fa901-b3a6-4010-93de-e5b471b6f0a6"), false, "Medieval History", "V1200" },
                    { new Guid("75a4b9f3-8347-4567-bf7b-d5d017649e4b"), true, "livestock", "100974" },
                    { new Guid("75d489dd-4f1a-4538-877f-ed79acdf97e0"), false, "Portuguese", "R500" },
                    { new Guid("75ecd4d7-fb5e-4c07-b787-e06b300a1653"), false, "Biological Studies", "C1005" },
                    { new Guid("75f08a2d-0f26-4585-8d61-a4bc56870f22"), false, "Dress & Textiles", "W2201" },
                    { new Guid("75f20edd-1664-4d50-bb6a-c779ad87a773"), true, "logistics", "100093" },
                    { new Guid("7603ab8c-968f-439b-bdbe-b782c073d7f4"), true, "oral history", "101435" },
                    { new Guid("7622a564-c52b-43b4-8114-84f8d9aaa78d"), true, "general practice nursing", "100285" },
                    { new Guid("7644effc-d801-4a02-bf5c-4559d4f9cada"), true, "management accountancy", "100836" },
                    { new Guid("7651639a-0aaa-489e-b09b-081123ae839d"), true, "psychology of religion", "101003" },
                    { new Guid("7667249e-1128-4d23-8e33-f0ba582b9245"), false, "Agriculture", "D2000" },
                    { new Guid("76967cd3-398c-4adb-a14c-df4356f45f8f"), false, "Creative Arts (Art)", "W1004" },
                    { new Guid("76a18376-e4c7-4373-932b-c46c6d722dc4"), true, "travel and tourism", "100101" },
                    { new Guid("76adae69-7eeb-4f10-8107-fe1836af820e"), true, "dermatology", "101339" },
                    { new Guid("76b13bdc-e0ee-4593-99d6-653ca2085ee2"), true, "enterprise and entrepreneurship", "101221" },
                    { new Guid("76f5b3ef-4e87-4eac-b7d1-290270544791"), true, "scholastic philosophy", "101443" },
                    { new Guid("76fa89ac-b753-4126-a0e1-e03c5562a60c"), true, "curatorial studies", "100914" },
                    { new Guid("7752124e-3ade-47de-a2fc-cd54f3600cf9"), false, "Chemical Technology", "F1600" },
                    { new Guid("77722912-7585-4bef-8116-dcec4718c98f"), true, "film production", "100441" },
                    { new Guid("7772800b-c730-4c55-8a0d-52f40908d7e0"), false, "Sport and Physical Activity", "X9015" },
                    { new Guid("77a4e585-aba4-44c7-93df-41cf9019acd6"), true, "Indonesian language", "101368" },
                    { new Guid("77a75042-cfb4-4789-82e8-6065b4909997"), false, "Metalwork Engineering", "J2002" },
                    { new Guid("77b5b603-a87d-420d-9568-c26cd991ab03"), true, "health visiting", "100295" },
                    { new Guid("7804d247-6417-4b4d-b0b0-ea4e9a3bd153"), true, "food safety", "101018" },
                    { new Guid("782256da-8eb7-48cf-a568-b9e438257fcf"), true, "Shakespeare studies", "101107" },
                    { new Guid("7848abf4-eabf-411e-9072-b206391c7d02"), true, "turbine technology", "101397" },
                    { new Guid("784f5fc8-c4fd-4b20-9665-a2f5ed98fa5b"), false, "Applied Art and Design", "W990" },
                    { new Guid("78562ea9-d848-45e1-8453-a7c28d2a3684"), false, "Psychology", "Z0058" },
                    { new Guid("78a5f339-40e0-4798-85e1-a6ea8a3b2e8b"), true, "software engineering", "100374" },
                    { new Guid("791e90b4-c278-456d-880c-a53b06c4eed4"), true, "health and social care", "100476" },
                    { new Guid("7934a041-f1f0-4cff-a28b-cc7ffdcb4ddf"), false, "Economic History", "V3000" },
                    { new Guid("79524ec2-3dbf-4d8a-af63-be18e915b953"), true, "statistical modelling", "101034" },
                    { new Guid("79989c94-096a-42e5-86e9-54496e2255bd"), true, "human geography", "100478" },
                    { new Guid("79a5cadc-4c8a-48d5-9589-c0f1f89b0cab"), true, "transport policy", "101406" },
                    { new Guid("79d58a88-8be7-433b-b0f9-b56cd59a6a14"), true, "Danish language", "101424" },
                    { new Guid("79ea2fd5-e049-470c-9299-a56b3cdf29a8"), true, "hydrography", "101073" },
                    { new Guid("7a7941ba-09f0-465b-870d-68c07a510b98"), true, "glaciology and cryospheric systems", "101394" },
                    { new Guid("7a7e8fd1-cf47-46e8-af43-33b4981bd0d5"), true, "radiation physics", "101074" },
                    { new Guid("7a8399e5-0d3d-4be5-9f32-764f8f417c2e"), true, "socio-economics", "100627" },
                    { new Guid("7a8eac2a-8349-43ea-9cca-71a8653b763a"), false, "Technical Graphics", "W9913" },
                    { new Guid("7aa3cf06-18d7-4399-8618-1efca6b071b5"), true, "highways engineering", "100156" },
                    { new Guid("7ab36e59-3c55-47e2-add4-ef1386707491"), false, "Chemistry", "F100" },
                    { new Guid("7aec7644-23c9-4373-aabf-f41b39306c29"), true, "dental technology", "100128" },
                    { new Guid("7b382d3c-c525-47f6-b3bd-a52112b9c3ab"), false, "Scandinavian Lang, Lit & Cult", "R8870" },
                    { new Guid("7b3ca799-036b-4e1f-a517-0748385b607f"), false, "Physics", "F300" },
                    { new Guid("7b63b14f-2467-4c72-930b-50d2918a2d18"), false, "Geography With Info Tech", "H8704" },
                    { new Guid("7bbdcb33-fedf-49f5-a0d8-fdb3ebf21efe"), true, "employability skills (personal learning)", "101278" },
                    { new Guid("7bda4429-a989-4aa1-8e0b-633e0cb35475"), true, "rural planning", "100593" },
                    { new Guid("7c1e887a-1a6b-4d24-9505-6a646d0aad1c"), true, "pathobiology", "100038" },
                    { new Guid("7c8452d9-b030-42cb-bef7-6ea0d7c3962d"), true, "environmental geography", "100408" },
                    { new Guid("7ca4ed44-ab25-4f35-9ff7-a2a9a5b5c179"), true, "Persian literature studies", "101433" },
                    { new Guid("7cb1ffcd-fadb-4971-9e70-5190f5bad7e9"), true, "quarrying", "100566" },
                    { new Guid("7cd7db73-e8fc-4c6c-93fa-a23003807f1b"), true, "creative arts and design", "101361" },
                    { new Guid("7cfe171e-221a-4490-bee3-d8fd833d0d76"), true, "drug and alcohol studies", "101332" },
                    { new Guid("7d3ad107-fd7e-47b4-b166-ec00fb93ff71"), true, "Welsh language", "100333" },
                    { new Guid("7d44306f-9b5c-4c22-a59d-e29233d645a0"), false, "Resource ManagementF9020Rural & Environmental ScienceF9007", "N1103" },
                    { new Guid("7d8f2a27-71d5-4b8e-8920-894e548b6c44"), true, "electronic publishing", "100926" },
                    { new Guid("7dafcdaf-24bd-4f17-a82a-fbcf8f251ab9"), false, "Social Science/Studies", "ZZ9010" },
                    { new Guid("7de02baa-59ce-4935-9c5c-c211f34ec4a6"), true, "military history", "100786" },
                    { new Guid("7de34469-0e70-4dac-b332-064d2c37673d"), false, "Business Policy", "N1205" },
                    { new Guid("7ded373a-9c34-43df-8e77-6c002e9a3d5f"), true, "management studies", "100089" },
                    { new Guid("7e1929b3-9a31-4c97-9796-263c5aeb8dff"), false, "Botany", "C2000" },
                    { new Guid("7ec9a7f3-761c-4a71-82a1-6092801f47ab"), true, "sonic arts", "100862" },
                    { new Guid("7ecab0ee-c41d-444d-9520-462f7ed202d8"), false, "Art and Design", "W2001" },
                    { new Guid("7ecba2a4-e38d-4696-8eae-d0472cc88846"), true, "computer and information security", "100376" },
                    { new Guid("7ef73381-837c-4ef2-821f-42829f02b129"), true, "modern Middle Eastern languages", "101191" },
                    { new Guid("7f309218-5591-474f-b04d-c2fd3895fb3f"), true, "financial economics", "100451" },
                    { new Guid("7f648961-a63d-4bde-b7f3-32f6b5861373"), false, "Craft, Design & Technology", "W2403" },
                    { new Guid("7f97b210-c796-40e1-8834-c363c6da9470"), true, "macroeconomics", "101402" },
                    { new Guid("7fac9220-8908-4eab-a244-bc06463da525"), false, "Gujarati", "T5005" },
                    { new Guid("7fb9a4f0-fb88-41d5-ba47-0155cc0ac060"), true, "evolution", "100858" },
                    { new Guid("8001cf73-50a1-4861-b14c-5c4d171879a0"), true, "community justice", "100659" },
                    { new Guid("802952d9-3e1c-4679-b39f-2af7a4fa5b98"), false, "History, Geog & Relig Studies", "Q9711" },
                    { new Guid("804f589c-ca50-495e-9195-2593f81a1a01"), false, "Health and Social Care", "L510" },
                    { new Guid("80852e3c-d1fd-47c9-a315-064394689c60"), true, "building services engineering", "100147" },
                    { new Guid("80862308-4c4a-4d36-9b9d-587794551d1b"), false, "Greek and Roman Civilisation", "V1016" },
                    { new Guid("8094d9b4-6b2d-4614-815e-0304d9c7f05b"), true, "torts", "100690" },
                    { new Guid("80a05f33-8225-44ea-8011-819dd9ca2daa"), true, "minerals technology", "100155" },
                    { new Guid("80cc809d-2d3c-44f4-ab8c-e244844ca236"), true, "stage management", "100703" },
                    { new Guid("80ce5893-9a79-4492-b2d2-944f0be923b9"), false, "Combined Science", "Y1001" },
                    { new Guid("8101c1fe-06c7-4fe9-9771-6a0a4457b5d6"), false, "Accounting", "N4001" },
                    { new Guid("81110b31-8b05-4d6b-8689-e15046947374"), true, "crystallography", "101044" },
                    { new Guid("812a7beb-a791-44bd-a183-80ab09aedac9"), true, "fluid mechanics", "100577" },
                    { new Guid("81713596-a4d8-426d-9531-7397709e4134"), true, "pre-clinical medicine", "100276" },
                    { new Guid("8176f6cf-87b2-4bd9-90b2-dc717d16defd"), true, "animal science", "100523" },
                    { new Guid("8186d2bd-9804-4cdd-a05b-ffa7222efa07"), false, "Spanish Lang, Lit & Cult", "R8840" },
                    { new Guid("81c20efd-aef9-4987-9113-31b30af14084"), true, "American history", "100767" },
                    { new Guid("8206fd68-d394-4255-a26f-91dfb5c07b51"), true, "veterinary dentistry", "101347" },
                    { new Guid("822ac69e-48e6-4543-9a6c-b55e23f75d41"), true, "dental nursing", "100283" },
                    { new Guid("825e8662-bc93-41b3-b994-0003aed3952d"), true, "interior design and architecture", "101316" },
                    { new Guid("8261de11-3fd0-4dd1-8397-6107b84499e7"), false, "Language Literacy & Literature", "Q1407" },
                    { new Guid("82621ad6-d432-425b-9aba-1b4ba9eb59c3"), true, "geological oceanography", "101086" },
                    { new Guid("8270e249-6f8a-45fe-b40c-8cd1cc55e264"), true, "veterinary epidemiology", "101220" },
                    { new Guid("82a6ad3f-3646-4c12-881a-de38bc893bbd"), false, "Design Technology", "W2505" },
                    { new Guid("82afc3cd-ffc4-4faf-af47-b3839d12190d"), true, "nuclear engineering", "100172" },
                    { new Guid("82baac60-977e-4778-9c92-6338b47a480a"), true, "physics", "100425" },
                    { new Guid("82be3d5f-af3e-402f-b613-b9be2dbb3c91"), true, "criminal justice", "100483" },
                    { new Guid("82c8090f-0073-467f-97c2-c4639253b50d"), true, "gastroenterology", "101331" },
                    { new Guid("82ee964d-b2f7-4692-a98a-4acf90a6227b"), false, "Religious Education", "V600" },
                    { new Guid("8300f384-ae4c-4dc2-8d11-8a31412adfc7"), false, "Literacy", "Q1405" },
                    { new Guid("8316caa5-6b1b-4e4b-a622-13c562ab6010"), true, "atmosphere-ocean interactions", "101351" },
                    { new Guid("8343aab3-e5bf-468c-8f3b-7cb1a6c92cf7"), false, "Applied Linguistics", "Q1100" },
                    { new Guid("8357f3c4-bd0d-41f1-af1c-f6e90888fac5"), false, "Science-Geology-Bath Ude", "F6005" },
                    { new Guid("8367b6fa-78f0-4e4e-b417-c7721212adf8"), true, "environmental biology", "100348" },
                    { new Guid("8375c130-9cb0-4d5e-b86a-7f6ade623b3b"), false, "Finnish", "T1300" },
                    { new Guid("8392b1a8-95bb-4947-b705-f9bc1268162b"), true, "cereal science", "101388" },
                    { new Guid("839464c3-199b-48e7-8156-e715abb971de"), false, "German Language & Studies", "R2103" },
                    { new Guid("840b0679-9168-4c39-8cdd-b53de203b53b"), true, "private law", "100686" },
                    { new Guid("843111ab-fcbf-45f1-92c8-2ee8dbfd5da8"), true, "theatre studies", "100698" },
                    { new Guid("843f3167-f954-4dcd-86e1-ea696892c90c"), true, "developmental psychology", "100952" },
                    { new Guid("8449bb92-7f10-45c2-8825-7f846ba37011"), true, "psychotherapy", "100254" },
                    { new Guid("84992cbd-0ee6-4166-b666-5d25f3a9e827"), false, "Engineering Technology", "H1002" },
                    { new Guid("84bbaa1c-524c-4925-a6b8-1bfbfc7fe861"), false, "Maths.Science and Technology", "G1502" },
                    { new Guid("84e5bc67-40d6-49cd-9b16-f24015626571"), true, "construction and the built environment", "100150" },
                    { new Guid("84f721f3-12e2-4c77-93e4-9a5f785a2797"), false, "Materials", "J500" },
                    { new Guid("8516bb0f-fee7-492a-872c-e93ed391ddb0"), true, "radio production", "100924" },
                    { new Guid("851b3ab2-2748-40a0-9d99-ea1b8a9f2c30"), false, "Organisation Studies", "N1202" },
                    { new Guid("85466ca4-c7ac-49d4-92ea-4dc2ab330852"), true, "theatrical wardrobe design", "100705" },
                    { new Guid("85626e72-57fc-4ee9-a84c-781fa7f2ae14"), true, "sustainable agriculture and landscape development", "100998" },
                    { new Guid("8574e03a-3f1d-4415-851a-887f1b298cb7"), false, "Design For Technology", "W9925" },
                    { new Guid("85b46862-d64d-4eec-a4fa-3120b6d9b60c"), true, "Russian and East European society and culture", "101158" },
                    { new Guid("85f6f283-6622-4f82-ae37-9aa499989e24"), true, "Milton studies", "101485" },
                    { new Guid("8618d636-62a3-484b-af8e-34b0853a9595"), false, "Applied Business", "N190" },
                    { new Guid("863d333f-9174-48a8-b054-6edb167144c1"), true, "automotive engineering", "100201" },
                    { new Guid("86bc6c04-2318-4b56-ba2a-aa66da2a632a"), false, "Modern History", "V1300" },
                    { new Guid("86c56323-86e3-44eb-92d1-50b061a15076"), false, "Printed Textile Design", "W2205" },
                    { new Guid("86d68f4d-a885-434b-970a-87751cae1070"), true, "secondary teaching", "100512" },
                    { new Guid("872c7132-4b7f-4443-b06e-4e7dc9ac8a41"), true, "ecosystem ecology and land use", "100864" },
                    { new Guid("877d6f14-19ea-4baf-8a81-f6c38b5ca4f8"), false, "Product Design and Technology", "W9910" },
                    { new Guid("8788eb1a-7148-459e-b190-138969009a87"), false, "Human Ecology", "C9005" },
                    { new Guid("878d634a-2cee-439f-a3c9-3f88b4a4e61e"), true, "librarianship", "100913" },
                    { new Guid("878e10ac-cffa-4b3a-bb5c-cc0d59ae8221"), false, "Drama and Media Studies", "W9924" },
                    { new Guid("87d68682-c0e8-4500-93c5-3f7ff456164e"), true, "linguistics", "100328" },
                    { new Guid("88007c7a-3457-4dd3-90e3-86922919bcd6"), true, "motorsport engineering", "100206" },
                    { new Guid("8811f5d8-179f-4cc7-897c-973ef91e337e"), false, "Secretarial Studies", "N9700" },
                    { new Guid("8835ae44-434f-4134-af1c-eb0e6c5fb817"), true, "soil science", "101067" },
                    { new Guid("884424ab-1eeb-450d-a6e0-cf2b69fdb539"), false, "Media Studies", "P4000" },
                    { new Guid("888bd3d5-86e6-41bb-83c4-ec3d31629ee4"), true, "physical geography", "100410" },
                    { new Guid("88946a0d-f633-4bac-945e-c6cc2c4c45b5"), false, "Information and Communications Technology", "I200" },
                    { new Guid("88bc0af5-32bd-40cd-9a66-8aa683487d62"), true, "human-computer interaction", "100736" },
                    { new Guid("89132be1-8ec1-4a69-bfc4-13d0a819c10b"), true, "organic chemistry", "100422" },
                    { new Guid("894e7409-da7a-4ef8-b374-ed20d67e1c44"), true, "planetary science", "101103" },
                    { new Guid("898e9e44-ba66-46b1-b5b4-7f01ea926292"), true, "anthropology", "100436" },
                    { new Guid("899aa4dd-74b5-4159-b722-48fdd4822a4b"), true, "footwear production", "100110" },
                    { new Guid("89afb187-1d2b-4d41-9e0b-8e3ed17c2c0c"), false, "Linguistics", "Q1000" },
                    { new Guid("89f96a83-60af-4e38-8f22-bff721d7ccda"), true, "community work", "100655" },
                    { new Guid("8a1675e3-881b-4c2d-a98f-52559e2497ac"), true, "parasitology", "100826" },
                    { new Guid("8a335abc-15e4-4ca7-ad61-01d5434f6f3a"), true, "computer games programming", "101020" },
                    { new Guid("8a52025c-5ad8-4c17-84d9-8f25a98e2007"), false, "English Literature", "Q3001" },
                    { new Guid("8a9fc70c-03a6-4b93-a9ea-55352bd19273"), true, "conducting", "100650" },
                    { new Guid("8aa2df3f-91c7-4949-b5d9-85368e8ae4bc"), false, "Home & Community Studies", "L5204" },
                    { new Guid("8aefeb1c-88ae-4f29-8c38-d889998f832f"), true, "horticulture", "100529" },
                    { new Guid("8af59050-5c7d-48d9-b6a5-1e71d79a83fc"), true, "sociolinguistics", "101016" },
                    { new Guid("8afd4cc6-6ee2-45c7-bf44-86b1dc36f1d4"), true, "Northern Irish law", "100677" },
                    { new Guid("8b09f020-7f69-4471-b25b-f3db7550df60"), true, "dementia studies", "101329" },
                    { new Guid("8b310878-d485-4bb2-9a50-35217398aba8"), true, "adult nursing", "100279" },
                    { new Guid("8b4b8c8b-2c88-4cd1-801a-777d1856677f"), true, "strategic management", "100810" },
                    { new Guid("8be27f5e-e082-40d3-8bb3-0d3ae2513d1d"), false, "Medieval Studies", "V1002" },
                    { new Guid("8c250855-95d2-49d1-ba53-7b91d29fee03"), true, "applied computing", "100358" },
                    { new Guid("8c273472-006e-4b5c-ba54-8a343526326a"), false, "Textiles", "J4100" },
                    { new Guid("8c2f07dc-c6ce-4fe4-83fa-df96ada34f9b"), false, "Eng. As A 2nd Or Foreign Lang.", "Z0034" },
                    { new Guid("8c3ed0a2-2f4d-4a4c-bb1e-a06c20194d2c"), true, "equine studies", "100519" },
                    { new Guid("8c7c72a7-fc62-45a8-a197-e866105008bd"), false, "Librarianship", "P1000" },
                    { new Guid("8c807866-feba-496e-937d-e7996c276366"), false, "French With Russian", "R8205" },
                    { new Guid("8cc05fe7-ed83-45b1-b333-ae708454d891"), true, "classical reception", "101129" },
                    { new Guid("8cda6df0-be6c-4d64-8636-7a2e3ce3d755"), true, "learning support", "100462" },
                    { new Guid("8cf23147-2506-46bd-b731-00b15527f686"), true, "naval architecture", "100207" },
                    { new Guid("8d00899b-c61a-4a25-a4d2-40a3e656d530"), true, "quantity surveying", "100217" },
                    { new Guid("8d02bfce-06d7-44ab-848d-a9db1a1bdadc"), true, "aeronautical engineering", "100114" },
                    { new Guid("8d1d9756-ef6b-416b-8d94-58077e1fd621"), true, "classical art and archaeology", "101440" },
                    { new Guid("8d30ed21-d0d0-419f-8b8b-856afbf71f2f"), true, "banking", "100827" },
                    { new Guid("8d324f1a-95e9-4478-8f71-7b59afa80edf"), true, "databases", "100754" },
                    { new Guid("8d48fd92-358f-40fe-91e0-299caee16019"), false, "Creative Arts (Dance)", "W4501" },
                    { new Guid("8d55d3aa-dd6f-42cc-8bed-0e905f398da5"), true, "healthcare science", "100260" },
                    { new Guid("8db70b38-3d87-4105-af92-1115d6e3e070"), true, "Korean studies", "101212" },
                    { new Guid("8dba7455-2fb1-4cfd-988c-e31d7d030a53"), false, "Gen Studies In Social Sciences", "Y2000" },
                    { new Guid("8dcc2dc9-e5b0-4f0e-aa2e-677c318bbd98"), false, "Expressive Arts (Physical Ed)", "W9917" },
                    { new Guid("8df64559-8b93-4c06-b12a-dbc7d9cf554b"), true, "agricultural machinery", "101010" },
                    { new Guid("8dfdcb39-7808-4a1c-94cc-91feaea89902"), true, "drawing", "100587" },
                    { new Guid("8e0eea77-c780-4dc9-a817-80195be0cda9"), false, "Outdoor & Envir Studies", "X9018" },
                    { new Guid("8e143dbd-6c89-499b-a21f-5b87feac2c06"), false, "Welsh and Drama", "Q5206" },
                    { new Guid("8e16865f-5f5b-47a8-9ea3-31f3ba670433"), true, "veterinary biochemistry", "101461" },
                    { new Guid("8e4bd930-f37b-4eb4-ac94-6996e1413374"), false, "Science-Bal Sc With Environ Sc", "F9610" },
                    { new Guid("8e750d7b-7be2-495a-9196-ce1700cd7671"), false, "Printmaking", "W6700" },
                    { new Guid("8e9b4527-04e3-4a2d-940a-7d6fe9071e9b"), false, "Historical & Geog Studies", "V9002" },
                    { new Guid("8f07558e-ce24-4435-a266-167d8ee7c572"), true, "veterinary pathology", "100938" },
                    { new Guid("8f0862ef-c776-481d-ba62-424cbd418ced"), true, "computer science", "100366" },
                    { new Guid("8f3f9881-03eb-4270-aab7-211b48ff1fe9"), false, "English History", "V1400" },
                    { new Guid("8f4d2d50-f246-46af-a380-acc469e45d6f"), true, "metaphysics", "101441" },
                    { new Guid("8f64afd7-3068-467a-8a3b-792a75a2a113"), false, "Mathematical Engineering", "J9201" },
                    { new Guid("8f652efe-7f41-40bd-b854-92c228fa7130"), true, "Gaelic literature", "101497" },
                    { new Guid("8f6c70cf-d87a-4b23-9477-bbbb41b40f6d"), false, "Language Arts", "Q1410" },
                    { new Guid("8f8bd196-37f8-4886-bcab-85cd10a4b8f4"), false, "Express. Arts(Music and Drama)", "W3006" },
                    { new Guid("8fae3d5d-bbf1-464d-8f30-ab4987b0a28a"), true, "Sumerian language", "101416" },
                    { new Guid("8fbb17b7-77fa-4395-bb5a-6fde9a6a0a68"), false, "Frenchlang and Contemp Studs", "R1104" },
                    { new Guid("8fcf9358-37aa-4674-8bb6-7982fc422350"), true, "Irish language", "101121" },
                    { new Guid("8fd4ea15-20b3-4bc8-a595-4524f028a3d7"), true, "religious writings", "100800" },
                    { new Guid("8fe42b9a-1e67-41fa-a069-2b1503593bf0"), true, "solid mechanics", "101396" },
                    { new Guid("8fff0509-d2b9-4384-809a-794092904692"), false, "Biology (With Science)", "C9702" },
                    { new Guid("90298a35-1f1c-4223-993d-a445726f08ab"), true, "metabolic biochemistry", "101380" },
                    { new Guid("90332e2d-8fa1-40f0-8f2a-63b10ac4ab1f"), true, "systems auditing", "100756" },
                    { new Guid("9078fa42-43b2-4f70-a41d-9c4a4fa39850"), false, "Technology (Home Economics)", "H8702" },
                    { new Guid("90d396ae-e34c-464d-a8fd-21fa9d69f04a"), true, "professional writing", "100731" },
                    { new Guid("90dda72e-0233-4a44-944e-54b02ad22fa8"), false, "Educational Drama", "W4008" },
                    { new Guid("90e76e2e-3753-40d8-8f98-b9f886e3d9e0"), false, "Dietetics", "B4001" },
                    { new Guid("910a8447-ccb7-4406-8201-10a84c726e39"), false, "Gen Euro Lang, Lit & Cult", "T8820" },
                    { new Guid("91270f41-d6c6-4424-b9ae-ee87aa5b7799"), false, "Welsh As A Modern Language", "Q5201" },
                    { new Guid("91277f6b-edce-4355-a10b-a594475f481e"), true, "applied physics", "101060" },
                    { new Guid("913129c6-f6ec-4984-8889-ade735df6f1c"), true, "gerontology", "101326" },
                    { new Guid("91428fec-6a8f-4bd5-9913-ebe7dff6dc31"), false, "Dutch", "T2200" },
                    { new Guid("916701ec-cf74-4862-bfed-914420c60d77"), false, "Creative Arts (Drama)", "W4001" },
                    { new Guid("919d8745-1326-460a-a3a7-e69758915fb7"), true, "cultural geography", "100671" },
                    { new Guid("91a78289-62df-4639-bded-fd2affe58447"), false, "Drama & Movement", "W4002" },
                    { new Guid("91fcfccc-53f5-483c-875b-c53bd743a36e"), true, "ecotoxicology", "101459" },
                    { new Guid("91ffd336-1220-4c87-be4d-114ac343ada5"), true, "nationalism", "101357" },
                    { new Guid("923de12e-cb98-4aa8-9e63-8499ba0e5264"), false, "Citizenship", "L230" },
                    { new Guid("9260b0bb-a8cb-4b3a-afb1-414e39b2736b"), false, "Provisions 16 - 19", "X9017" },
                    { new Guid("92884580-8d37-4161-964b-c160133e12cb"), false, "Maths.Stats. and Computing", "G5009" },
                    { new Guid("92ded31e-06ea-4932-9639-8ceecfd05e8b"), true, "plant pathology", "100874" },
                    { new Guid("92e25534-7fc2-4563-bf6d-c4e28fff52fb"), true, "diabetes", "101338" },
                    { new Guid("92e2dc3c-3101-4b8b-8270-f7f4e7b00459"), true, "dynamics", "100429" },
                    { new Guid("92fa27a2-726c-462a-b629-1bc53013b91f"), true, "computer architectures", "100734" },
                    { new Guid("92fb1a16-ff06-4425-a2e2-1c2cc09f48d8"), true, "ceramics", "100003" },
                    { new Guid("9305a485-bfc2-4c7f-9f24-6186d29b5d93"), false, "Materials Science", "F2000" },
                    { new Guid("931ed6bc-064e-4cb9-8151-bcbee30f25de"), true, "environmental management", "100469" },
                    { new Guid("936184ac-9b1a-4dea-87fd-c8fbdbeff8e8"), true, "pharmaceutical engineering", "100144" },
                    { new Guid("93c795fc-800b-4ffe-8c57-a2208764e71a"), true, "Polish language", "101153" },
                    { new Guid("93d2ad81-bcfc-4e10-b0af-c5e6ccafc1e2"), true, "Islamic studies", "100796" },
                    { new Guid("93e62708-ff41-42d8-ac7c-70136ae6480b"), false, "Agricultural Business Mngement", "N1000" },
                    { new Guid("944c021f-1019-4cb5-96ca-c052c86d1fea"), true, "music theory and analysis", "101449" },
                    { new Guid("945059d1-1bf5-4117-91f1-ee0ce9eaf694"), false, "French With Spanish", "R8204" },
                    { new Guid("9465ad42-2570-49c3-9214-7b8128a75dc1"), true, "biomechanics", "100126" },
                    { new Guid("948c9092-ff5d-43f9-ad24-c81de57c9379"), false, "Textile Design", "W2200" },
                    { new Guid("94937de5-91fa-4a35-8a84-7944928502a5"), true, "paper-based media studies", "100922" },
                    { new Guid("949c8474-c00a-4130-bb35-c7445cb1885d"), true, "teaching English as a foreign language", "100513" },
                    { new Guid("94e13788-d55d-4d0a-8c7a-ef6b0c94257b"), false, "Hair and Beauty", "B990" },
                    { new Guid("94f8b8a1-19ff-43da-b638-f8f4fa6e2ebf"), true, "Hungarian language", "101427" },
                    { new Guid("9505b86c-8d13-4353-a37c-919dd8b67100"), true, "industrial biotechnology", "100137" },
                    { new Guid("95154f00-abd0-4330-a5dd-a9450229461b"), false, "Science (With Biology)", "C9704" },
                    { new Guid("9542afff-82f3-4e50-a9a5-57d1d6992cef"), false, "Latin Amcn Lang, Lit & Cult", "R8860" },
                    { new Guid("95be21ee-9b90-4e4c-a104-ad413b9a3712"), false, "Science With Physics", "F9625" },
                    { new Guid("95df119c-a535-4163-acc2-7a3576720cb6"), true, "mycology", "100872" },
                    { new Guid("95ec5c7d-dd43-4f7e-9791-ee9daaa0a977"), false, "Biology", "C100" },
                    { new Guid("95f0064c-6ae0-4c44-a96d-744849d14d15"), true, "European Union politics", "100612" },
                    { new Guid("95f2bd8b-1d45-463e-aa30-2b0240783a88"), false, "Recreation & leisure studies", "N870" },
                    { new Guid("95f3bd36-3c5c-42ff-9222-2fc04bb6dc84"), true, "ophthalmic dispensing", "101511" },
                    { new Guid("9604bb69-2505-4b31-82b1-1bdc87baffcc"), true, "classical Greek studies", "101126" },
                    { new Guid("9647aef3-4848-46c1-93b5-f59a8b794691"), true, "medical biotechnology", "100138" },
                    { new Guid("96b84e54-4f40-44c9-96bb-71cf795acc06"), true, "archaeology", "100299" },
                    { new Guid("96be7eeb-a178-4958-87f8-5cf9ea086bec"), false, "Gaelic", "Q5001" },
                    { new Guid("96d77074-bef1-4522-8a42-aa03da8365c2"), true, "aerospace engineering", "100115" },
                    { new Guid("96db8451-abf0-49a6-bd18-24eb0b3731d2"), false, "European History", "V1001" },
                    { new Guid("96e593ee-6bb4-4c16-8276-bcd17439e309"), false, "Chemistry and Science", "F9631" },
                    { new Guid("971a7967-17e4-4807-9efa-57f5c57d36b8"), false, "Chemical Physics", "F3300" },
                    { new Guid("97756563-1c87-4e5a-9fd1-42b42bf3ec86"), true, "disability studies", "100625" },
                    { new Guid("9799ad2f-f611-488d-ab8e-d79c2caea3ad"), true, "Virginia Woolf studies", "101491" },
                    { new Guid("97ac7c7c-19f4-4414-a713-73872ddfa4de"), true, "promotion and advertising", "100855" },
                    { new Guid("97fa2cd9-fc3b-4d28-881d-fe9b2915fd93"), true, "garden design", "100590" },
                    { new Guid("98348fed-53ce-484f-9146-7ea03aae7b64"), false, "Performing Arts", "W4300" },
                    { new Guid("984c5341-1667-425f-8fba-d20df6b378bb"), true, "chemical engineering", "100143" },
                    { new Guid("987ae7ca-ab5f-46bf-870b-db3cce5b99a9"), true, "buddhism", "100798" },
                    { new Guid("98be8ccc-3ebc-4b4d-90d3-20fb5d97bb22"), true, "parallel computing", "101400" },
                    { new Guid("9918f4b4-b96d-47ff-a398-47412fe6e7a3"), false, "Education Of Children With Learning Difficulties", "X6001" },
                    { new Guid("996fbfb7-dc91-42d5-bb49-fd0ae10990da"), true, "North American literature studies", "101203" },
                    { new Guid("9984666b-9a9c-4ab3-8950-8bba1f733669"), true, "adult education teaching", "100507" },
                    { new Guid("99c1894e-8cb2-4c28-877b-db2dd835d705"), true, "Irish language literature", "101413" },
                    { new Guid("99c2f2e0-51b8-4dcc-b6bb-36a3a4ee19cb"), false, "Plant Science", "C2002" },
                    { new Guid("99cb1ea9-b4f9-44cd-a8e1-8847ac5dcde8"), true, "cell biology", "100822" },
                    { new Guid("99dc47e0-86d3-46bd-bab0-39e8b2821eab"), true, "quantum theory and applications", "101300" },
                    { new Guid("99f2ee9e-c767-4c83-99bf-9a334ea281e3"), false, "Greek History", "V1007" },
                    { new Guid("99ff97db-7ec4-42e2-a07e-f922c06a8967"), true, "complementary medicines and therapies", "100242" },
                    { new Guid("9a219104-6cf5-47c0-88b4-94bcbba724c0"), false, "Combined Studies", "Y4101" },
                    { new Guid("9a34ba9e-e6b0-4748-aa70-1e28ce3fd0f1"), false, "Society, Health and Development", "L990" },
                    { new Guid("9a4a99f7-7741-4cd2-b213-0f7b5bf574bc"), false, "Pure Science", "F9005" },
                    { new Guid("9a60669a-f507-4548-a361-8ef1f898635d"), false, "History", "V100" },
                    { new Guid("9acc22c4-c625-4381-b1f7-25c1af4e7779"), true, "design", "100048" },
                    { new Guid("9af48f73-514c-43c6-8141-0ebf5c3e634a"), true, "historical linguistics", "101410" },
                    { new Guid("9b36b8cf-3848-4009-8786-dbac9ab957e0"), true, "education studies", "100459" },
                    { new Guid("9b4ed7f0-ec01-4a51-9bd8-cf528e4b90bd"), true, "Latin studies", "101124" },
                    { new Guid("9b4f0fda-5942-4bc5-bd4e-8797caa79a37"), false, "Technology (C.D.T.)", "W2407" },
                    { new Guid("9b94f081-556e-4bc9-acf2-8794b0c33c03"), true, "social work", "100503" },
                    { new Guid("9bf0ea21-c8e5-4ed4-b59e-6ca39c9b389b"), false, "Textiles", "J420" },
                    { new Guid("9c1652bf-d3c4-43ec-9aab-bc877bd29e63"), true, "microwave engineering", "100177" },
                    { new Guid("9c7c9a16-d994-4610-9177-8259af7bc012"), false, "Engineering (General)", "H1000" },
                    { new Guid("9c9d0929-f580-4d8c-8d1a-896978445338"), false, "Irish", "Q5300" },
                    { new Guid("9caa584a-bb89-450d-8d8d-16ba0e84e28e"), true, "Citizenship", "999001" },
                    { new Guid("9cba6eae-16f8-4aa0-9d17-93bd12a8bda2"), true, "business psychology", "100954" },
                    { new Guid("9d1022ea-20f0-4fbc-9644-72fb283c54f9"), false, "Professional Studies", "N1209" },
                    { new Guid("9d325e86-1b3a-4ca3-b94a-47402533458b"), true, "neurological rehabilitation", "101290" },
                    { new Guid("9d33b786-b705-4473-9436-7d912cc0d3c0"), true, "Scots language", "101412" },
                    { new Guid("9d41db8e-6fcd-449d-869c-85d74ff0a3af"), true, "Chinese history", "100771" },
                    { new Guid("9d423006-3647-48cd-b629-698090ce4bc8"), true, "entomology", "100882" },
                    { new Guid("9d478b6e-01c4-451c-8728-11a9a6146ab8"), true, "aviation studies", "100229" },
                    { new Guid("9d4d906c-74b9-4ad8-9711-5c291ca33cac"), true, "Australasian studies", "101206" },
                    { new Guid("9d947c4c-0da8-4aa8-978c-9ae337585c0e"), true, "spa management", "100894" },
                    { new Guid("9d96fa44-b9b0-48b9-b65a-31403dc54e8e"), true, "environmental chemistry", "101045" },
                    { new Guid("9db68509-e882-4b0e-8e97-a7d09e51d059"), false, "Educational Psychology", "L7500" },
                    { new Guid("9dc4fdce-4205-4eb8-a6ce-6e8e7d9e1df8"), true, "choreography", "100711" },
                    { new Guid("9e195a7b-3d32-44d4-a889-a4ffce932cd2"), false, "Theatre Studies", "W4400" },
                    { new Guid("9e367893-cff1-4c02-b97c-890f0056618f"), false, "Technology With Science", "F9609" },
                    { new Guid("9e42f292-cee2-4b5d-a7ef-7cdd36631b47"), true, "musicianship and performance studies", "100637" },
                    { new Guid("9f38e273-14f9-4696-ba90-8f7b5dcb035d"), false, "Craft", "W2402" },
                    { new Guid("9f3ac919-3a14-4616-9117-0bc5d5c90fbc"), false, "Applied Psychology", "C8100" },
                    { new Guid("9f3cbcad-1008-40d4-9d24-09c1b7835160"), true, "pre-clinical veterinary medicine", "101384" },
                    { new Guid("9f542ac3-cc98-445e-be82-9c505c7aef6d"), false, "Psychology (Not Solely As Social S)", "C8000" },
                    { new Guid("9f62e88f-e34a-476a-b621-7142f116967c"), true, "agricultural chemistry", "101024" },
                    { new Guid("9f791e88-4732-4ec0-bedf-483439257833"), true, "medical sciences", "100270" },
                    { new Guid("a00c0994-009e-44c3-ba53-e74c3b4f5500"), true, "Italian history", "100764" },
                    { new Guid("a01b3467-d412-4845-a433-6f42e1b638f5"), true, "medical statistics", "101031" },
                    { new Guid("a0391990-0102-4bbf-8d9e-0b204d9c1716"), true, "ancient Egyptian studies", "101113" },
                    { new Guid("a0450f4d-5e80-4aaa-88fb-5cd74f9524ef"), true, "Scottish literature", "101111" },
                    { new Guid("a047ad19-8a9d-48cb-9f89-a91ced273443"), true, "music", "100070" },
                    { new Guid("a057cea6-b264-47d4-86c8-fe76260e33e1"), true, "genomics", "100901" },
                    { new Guid("a0b21914-e5aa-4c14-affd-163f7f6d5b14"), false, "Political Education", "M1004" },
                    { new Guid("a0dafbad-f44f-4a97-836d-7424a11fb281"), true, "European studies", "101159" },
                    { new Guid("a146ec86-4547-402c-a995-bb9419c29344"), true, "thermodynamics", "100431" },
                    { new Guid("a168b9b1-8eaa-46a9-9b1c-129427dbb8ca"), true, "forestry and arboriculture", "100520" },
                    { new Guid("a18c817a-6e6b-46ae-b7e1-4cae51fbefe6"), false, "Cultural Studies", "L6200" },
                    { new Guid("a1c4a34d-e37f-416e-8238-ebf7c1a4f88f"), false, "Criminology", "L3900" },
                    { new Guid("a1e129f5-629b-4585-8c5e-694aad18b37c"), true, "divinity", "100799" },
                    { new Guid("a1ff9aea-0b69-4a21-8d5c-83850a07e04f"), false, "German With French", "Q9708" },
                    { new Guid("a23a3eb5-e230-4603-9949-0172fcaa76fb"), true, "hypnotherapy", "101340" },
                    { new Guid("a24b1afb-6d15-476c-a5b2-c9550669883e"), true, "stone crafts", "101455" },
                    { new Guid("a28b3011-a907-4d7d-955b-1ec13937d1a9"), false, "Rehabilitation Studies", "L3404" },
                    { new Guid("a2916131-a665-424f-804e-36a4b2361c85"), true, "cognitive neuroscience", "101381" },
                    { new Guid("a29959a1-7c19-46a0-a1d2-392ba4934a0c"), true, "community forestry", "101014" },
                    { new Guid("a2c79ffc-c5d5-43b3-82d9-1eb540d57805"), false, "Italian Lang, Lit & Cult", "R8830" },
                    { new Guid("a2ccdaa6-bae0-48c3-9776-f49072c96dbb"), true, "policing", "100486" },
                    { new Guid("a3489136-86d4-4462-8d86-5516f2b5b4ae"), true, "Japanese society and culture studies", "101171" },
                    { new Guid("a36d1a53-a318-4965-be40-6be47c112b9b"), true, "jazz composition", "100870" },
                    { new Guid("a3a755bc-c46c-4db2-bb5d-3a2f517b5ee7"), false, "Microbiology", "C5001" },
                    { new Guid("a3f24be9-543c-4a60-8e87-1d2fbf909b7b"), false, "Combined/General Science", "F000" },
                    { new Guid("a42733d1-4a80-4ea9-b805-9fedbdac4d2a"), true, "condensed matter physics", "101223" },
                    { new Guid("a4ab58c0-a4c6-4e8e-ba44-cacd23036d06"), true, "Czech language", "101155" },
                    { new Guid("a50f5127-f0c7-4f7d-83fe-aa9fd6946f8c"), true, "psychology of music", "101363" },
                    { new Guid("a5293b76-fac0-4b46-a48c-07cf92fa5ef1"), true, "multimedia journalism", "100445" },
                    { new Guid("a53281e2-b459-47f8-81ec-4045cb1838e8"), false, "Information Technology", "G5601" },
                    { new Guid("a56a8c42-f1e4-4988-9950-f825a07f74bd"), true, "contemporary dance", "100886" },
                    { new Guid("a5a1ea09-44fd-4468-9f50-c3b74744781c"), true, "jazz performance", "100656" },
                    { new Guid("a5b6f114-cc46-481a-8e64-f9ff52f621a6"), true, "Polish society and culture", "101500" },
                    { new Guid("a5e0cd95-cbbb-4b8d-a8ad-99b81a50be8f"), true, "navigation", "100230" },
                    { new Guid("a622194e-c1df-4f27-a8f5-cf8c57fa5380"), true, "property management", "100820" },
                    { new Guid("a634a867-f53e-40ef-9f4a-e67d3a56af18"), false, "Creative Studies (Music)", "W3004" },
                    { new Guid("a6442ffc-d6de-4312-8d53-d657c3ab7ff9"), false, "Foreign Languages", "Q1303" },
                    { new Guid("a6474c68-6473-4b78-a9fc-96bb2c8531b5"), false, "Computer Science", "G5001" },
                    { new Guid("a64778bc-d332-4d40-aac8-b8212f74880f"), false, "Civilisation", "Q8201" },
                    { new Guid("a65eef4e-de3a-481e-8b0c-a2fd20201716"), false, "British History", "V1406" },
                    { new Guid("a686e272-60f9-473b-b1a8-cb5350b7fafa"), true, "Italian literature", "101137" },
                    { new Guid("a6cf9e94-88cd-4a2a-8f72-d92f517f7da2"), false, "Chemistry With Core Science", "F9616" },
                    { new Guid("a6e9bfcf-3870-496d-9cad-262d7c3c7216"), true, "health policy", "100648" },
                    { new Guid("a6ee40aa-94e6-4d61-afa5-2dfd0ee8baeb"), false, "Science:earth Science", "F9026" },
                    { new Guid("a6fab7ee-c355-49ae-aa68-19828dd7a84d"), true, "historical performance practice", "100661" },
                    { new Guid("a6fafb8b-2eb6-47ef-9d50-1aa0ff06df6b"), true, "research and study skills in education", "101088" },
                    { new Guid("a7042311-8fc8-40e9-920b-9937fb2facc0"), false, "Info Technlgy/Computing", "G5602" },
                    { new Guid("a715cc9d-04d4-46de-88fe-895967338509"), false, "Management In Education", "X8000" },
                    { new Guid("a725a810-2554-4f81-9d16-21a36b582e97"), false, "Industrial Studies", "N6100" },
                    { new Guid("a748cab6-aaf6-4388-8aa1-fd9d03d10919"), true, "South East Asian studies", "101372" },
                    { new Guid("a7ab895e-68f1-4dd2-b8f7-0cd50fc6be4f"), false, "Mathematical Studies", "G1400" },
                    { new Guid("a7c109ea-4260-4e9d-bd62-c741a4453031"), true, "D.H. Lawrence studies", "101476" },
                    { new Guid("a80875e8-f7b5-4855-ba3f-97ccaa937cfe"), true, "Swahili and other Bantu languages", "101366" },
                    { new Guid("a81f4263-4a75-4c22-b1c4-ef66a5a533d8"), false, "French Politics", "M1003" },
                    { new Guid("a8323393-5ec2-42e7-8d73-76be8aee33b3"), true, "advertising", "100074" },
                    { new Guid("a83b2c93-3809-415e-bda0-5310dcda734e"), false, "Social and Enviro Studies", "L3406" },
                    { new Guid("a856ddfa-fd83-4a10-9a00-59612905a114"), true, "Norwegian language", "101149" },
                    { new Guid("a89bfa9e-51a8-46b4-8ba7-7ad1e8d97072"), false, "Creative Design", "W2003" },
                    { new Guid("a8f56577-e884-4315-90cf-406eb15e5146"), false, "Craft & Design", "W2404" },
                    { new Guid("a9333780-bfb9-4dda-8319-bd6400d6f4ae"), true, "French history", "101248" },
                    { new Guid("a95c27b8-1a97-4a3b-a9f4-b18721063f1f"), true, "circus arts", "100707" },
                    { new Guid("a9f10176-2c1f-4dac-a0f5-24722e9797ab"), true, "comparative law", "100683" },
                    { new Guid("aa1a0627-dee0-4a2a-a770-53073d154c07"), false, "Applied Educationapplied Education", "X9004" },
                    { new Guid("aa46b694-7bfd-42d1-af4c-9125e263abe5"), true, "prosthetics and orthotics", "100130" },
                    { new Guid("aa4b190d-4376-415b-a78b-1cbd267cb923"), true, "Italian society and culture", "101136" },
                    { new Guid("aa549e15-f91e-4c4d-a12d-f3b2cd22e415"), true, "tourism management", "100100" },
                    { new Guid("aad8fe97-278a-424e-ae8f-50f0e5f3ac4e"), true, "ancient Hebrew language", "101117" },
                    { new Guid("ab2b34d0-5a97-4521-8083-301423063a6b"), false, "Vocational English", "Q1406" },
                    { new Guid("ab5520a6-1220-46bf-bbda-759395473b4e"), true, "applied geology", "101104" },
                    { new Guid("ab6818a1-2296-43d8-a925-d2f1df414a01"), true, "applied economics", "100597" },
                    { new Guid("abaaa9f1-353e-4941-90bd-0f8503f7f0c6"), false, "Modern Foreign Languages", "R8201" },
                    { new Guid("abd77df9-de8e-4fea-9630-69015aa691b9"), true, "tissue engineering and regenerative medicine", "100572" },
                    { new Guid("ac2069d5-c000-46cc-acdb-3c4a33fd87ad"), true, "paper technology", "101356" },
                    { new Guid("ac2c966c-3b76-4675-b015-af47a4c499ca"), true, "animation", "100057" },
                    { new Guid("ac8dd31d-bc56-4ed3-8311-277c0edc98a2"), true, "pharmacology", "100250" },
                    { new Guid("ac90e7af-c5f8-4d86-9f77-1f2911c24541"), false, "Computer and Information Tech", "G9009" },
                    { new Guid("acaabc4d-96fd-4b2a-9f2d-4b6224d0c3d7"), true, "aerospace propulsion systems", "100564" },
                    { new Guid("ace453a2-6786-4403-8c1e-941000b9cbe9"), false, "Creat Stud (Art, Move & Music)", "W9916" },
                    { new Guid("ad2f87ac-ee5c-40b7-a22c-153341060661"), true, "classical studies", "100300" },
                    { new Guid("ad5d3ba7-adc4-48f9-8bd0-4c51869a1789"), true, "American studies", "100316" },
                    { new Guid("ad7e9145-24d7-4f72-9a04-1ffefd3f45f0"), true, "transport planning", "100198" },
                    { new Guid("ad813942-10c1-41e8-87a0-ce14bc09e545"), true, "biomaterials", "101210" },
                    { new Guid("ad9f2bee-1b60-4f87-b0c0-e062dc5a3616"), false, "Human Physiology", "B1001" },
                    { new Guid("ade030c9-2808-40df-aa8a-3781856aa75d"), true, "careers guidance", "100658" },
                    { new Guid("ae12666d-f58b-4b99-9f75-a2ab22e3d3f2"), false, "Latin", "Q6000" },
                    { new Guid("ae3452ca-2a9e-435d-ba7d-c821dab5fd11"), false, "Time,place and Society", "F9024" },
                    { new Guid("ae815a15-5ebe-4932-b04b-2321a4a6ee45"), true, "biogeography", "101352" },
                    { new Guid("ae8c3f08-ebf0-4f68-976b-3b3f771ddaa6"), true, "integrated circuit design", "100553" },
                    { new Guid("af2645e9-c27f-4cc2-bdec-307e791a4cb4"), false, "Religion", "V8010" },
                    { new Guid("af4ac22b-45fc-4940-bdea-720f744b16a0"), false, "Psycholinguistics", "Q1600" },
                    { new Guid("af5b01b4-c7fc-4bcf-bfd9-0ef151682040"), true, "computer aided engineering", "100160" },
                    { new Guid("af689ace-b7b4-450e-b970-718023470056"), false, "Statistics", "G4000" },
                    { new Guid("af783163-5ab8-44e5-8d2a-5077fd167c10"), true, "Salman Rushdie studies", "101496" },
                    { new Guid("af8f92f3-c16a-4343-b752-f90481a0c6c6"), true, "Christian studies", "100795" },
                    { new Guid("afa5bf91-0e04-4a4e-ab0f-30507ab6ead9"), true, "space technology", "100116" },
                    { new Guid("afdf8059-99e7-47a6-b6dc-f344756ac6ce"), false, "Health Education", "B9900" },
                    { new Guid("b009b380-8868-48aa-9a43-0b64fc9490f8"), true, "French literature", "101132" },
                    { new Guid("b01a47d5-2738-466a-b5cb-1f900ae7fed3"), false, "English Politics", "M1002" },
                    { new Guid("b0369394-5dde-4ca7-8b9f-cfe8851d73d5"), true, "control systems", "100166" },
                    { new Guid("b045b29e-4748-4226-9067-df9bcfa8a4dd"), false, "General Primary", "X120" },
                    { new Guid("b075d0e6-13b4-40b4-95cd-226f8b2ca356"), false, "Leisure and Tourism", "N222" },
                    { new Guid("b08fa67b-bcc0-4a41-8898-32d34fdab49b"), true, "Arabic literature", "101432" },
                    { new Guid("b09200df-d8b9-4d28-a439-f525670bd8b0"), true, "drama", "100069" },
                    { new Guid("b0932dea-ee81-45fe-91f5-a6b34a59904d"), true, "modern history", "100310" },
                    { new Guid("b094c5a5-cf52-42c1-a548-84d350ffaab5"), true, "biotechnology", "100134" },
                    { new Guid("b0bad895-85bc-45c4-b7cb-7bc157384051"), true, "Estonian language", "101426" },
                    { new Guid("b0e1417a-1837-4b29-8ce5-f2a5922082cf"), true, "genetics", "100259" },
                    { new Guid("b0ee512e-c08f-4516-b544-b471fc341118"), true, "housing", "100196" },
                    { new Guid("b0f5d0a7-cc3c-4dc6-aeac-d53af7f67e12"), false, "Economics With Social Studs", "L1004" },
                    { new Guid("b10634d5-61e7-453e-b3b2-e36e9934500f"), true, "transpersonal psychology", "101343" },
                    { new Guid("b11d6292-01c0-40b5-b94e-3aea95d2e262"), false, "Communications", "P3002" },
                    { new Guid("b127fcac-23f4-40c4-a7ec-b3570e998874"), false, "Computer Education", "G5400" },
                    { new Guid("b1509c7e-0432-4d12-9f2a-f949b07dcbad"), true, "ethics", "100793" },
                    { new Guid("b1a33f92-1b9f-4fac-8a7a-ba90f9ad174b"), false, "Housing Studies", "N8004" },
                    { new Guid("b1d99dcc-9fd1-4671-919f-97cba4f4f972"), false, "Tefl/Tesl", "Q3008" },
                    { new Guid("b1dccac7-ed0c-4e0f-ba2e-6450e0de287e"), false, "Advanced Study Of Early Years", "X900" },
                    { new Guid("b1ddff20-7be7-4711-a2ec-eee463b72bea"), false, "Analytical Sciences", "F9607" },
                    { new Guid("b24fa313-7804-4469-8ab3-51c9986e2803"), true, "clock and watchmaking", "100726" },
                    { new Guid("b2d8ef59-6540-4746-ae2e-1daeb9e37c62"), true, "Chinese society and culture studies", "101167" },
                    { new Guid("b3438597-463c-4630-870d-19f7dfd9a6a5"), true, "construction management", "100151" },
                    { new Guid("b359efbe-c353-4c99-a31d-a17302f90fba"), false, "Political Economy", "L1101" },
                    { new Guid("b383dab6-7c3d-485c-b025-fbec5e5c8bb0"), true, "sociology of law", "101465" },
                    { new Guid("b3912cf9-c309-491d-b672-2cbedaf825b3"), true, "chiropractic", "100241" },
                    { new Guid("b393471a-c055-462a-9a72-9e6c702f5d68"), false, "Arabic", "T6200" },
                    { new Guid("b397a256-5a83-484e-ac4c-3821eba09c51"), false, "Mathematics", "G100" },
                    { new Guid("b3a2a73a-258c-4c62-a84d-b32745769072"), false, "Critical & Contextual Studies", "W9921" },
                    { new Guid("b3d1d72b-1a01-4827-ad7d-4c733b2d5f6d"), true, "bacteriology", "100909" },
                    { new Guid("b3ed7edc-48ed-4f47-815e-378279adef10"), false, "Religious Studies", "V8000" },
                    { new Guid("b46c9ebc-b827-4316-a398-314d84a7aa04"), false, "Portuguese Lang, Lit & Cult", "R8850" },
                    { new Guid("b48370f7-b865-4092-8b36-7096bb0fc3f2"), true, "history of design", "100783" },
                    { new Guid("b4dbdf28-2dbc-4926-b949-2899fbbdb04d"), true, "child psychology", "100953" },
                    { new Guid("b4ea95a5-f688-4fc9-a7c6-5b475dbe5670"), false, "Creative,express.Arts(Gen)", "W3005" },
                    { new Guid("b4ebaf34-b0b4-438a-ad63-d497db3267db"), true, "Japanese literature", "101170" },
                    { new Guid("b4edc1a9-02f0-48e5-ad18-e67ef5529c38"), false, "Educational Studies", "X9003" },
                    { new Guid("b500b5d3-2dbe-4248-a824-28e50e93c496"), false, "Education Of The Deaf", "X6002" },
                    { new Guid("b504b26d-45e9-48b2-86d1-ae28acac3f49"), true, "Shelley studies", "101484" },
                    { new Guid("b5932136-7a3c-47fe-8e35-6e16c8bc0b38"), true, "minerals processing", "100212" },
                    { new Guid("b5c0471b-f427-43c7-8568-0a2c38aa1013"), false, "Place and Society", "F9019" },
                    { new Guid("b5d46fde-5942-4c06-ba8c-826f28d30c5b"), false, "Minerals Estate Management", "N1002" },
                    { new Guid("b5e15d02-cb71-4d0c-989d-5b788a08b4c9"), true, "Wilkie Collins studies", "101482" },
                    { new Guid("b60a4d84-170f-4641-b735-71a5cb59561e"), false, "Social and Life Skills", "G3400" },
                    { new Guid("b61b6085-1d98-444d-b716-91da84214704"), false, "Curriculm Development In Schs", "X9008" },
                    { new Guid("b6e8291e-f76a-421b-9209-cee6f06e5c6a"), true, "school nursing", "100293" },
                    { new Guid("b6f2e64a-e6d1-4cb7-ac4d-14a463322f45"), false, "Drawing", "W1007" },
                    { new Guid("b714a270-b444-4286-90f0-2ff14f7de344"), true, "general or integrated engineering", "100184" },
                    { new Guid("b724c63f-204b-44b6-b0be-01634768609a"), false, "Art,design and Technology", "W2409" },
                    { new Guid("b741a185-53df-41f0-ac0d-7503812daf9f"), false, "Education (Other Than Bed UK)", "Z0093" },
                    { new Guid("b7595fa6-66e9-4b41-a000-e0a2f90f26a7"), true, "ethnomusicology and world music", "100674" },
                    { new Guid("b76efc44-6678-45c1-a40a-bbf962850ae2"), false, "Language/Literature", "Q3010" },
                    { new Guid("b77324be-caf5-475b-b069-ee77c3b147ce"), true, "sociology", "100505" },
                    { new Guid("b79bd83b-1bfb-4bf9-8dc4-a118e1540b04"), false, "English", "Q300" },
                    { new Guid("b7cd69d0-8574-4597-bdae-976128798734"), true, "performing arts", "100071" },
                    { new Guid("b7d74a9f-d676-4f58-ad82-e207060d0d7d"), true, "structural mechanics", "100579" },
                    { new Guid("b7e5969a-e774-407b-a5b0-c7a26362dfdf"), false, "Biblical Studies", "V8200" },
                    { new Guid("b81981d7-c0ee-4b9a-b876-8c828165f6a2"), true, "comparative religious studies", "100803" },
                    { new Guid("b86a0596-84e0-40fc-a583-e5b1bb4062cc"), true, "music and arts management", "100643" },
                    { new Guid("b8938139-d10c-47e4-a773-f81621a5a4de"), true, "business information technology", "100362" },
                    { new Guid("b8a3fb30-14c7-4c33-a646-9c5daedd026a"), true, "T.S. Eliot studies", "101469" },
                    { new Guid("b8ab13b6-f46b-404b-a849-37004f4ec893"), true, "typography", "100630" },
                    { new Guid("b90bd506-41bb-4466-a00e-3b78a0e15907"), false, "Micro-Computing", "G5005" },
                    { new Guid("b9399863-afd2-404a-9cd1-54b0ca54c273"), false, "Health", "B9904" },
                    { new Guid("b94f26df-8d52-47fa-a978-3a3fca9eaf06"), false, "Theoretical Physics", "F3201" },
                    { new Guid("b995f724-0440-4702-8959-44d4c38c0cc4"), true, "legal practice", "100692" },
                    { new Guid("b9b6c9d4-d093-4d6d-9a70-9679d019025e"), false, "Phse", "L390" },
                    { new Guid("b9bf8b25-342d-4fda-a24c-15fac5040265"), true, "atmospheric physics", "101068" },
                    { new Guid("b9ce2aab-8c81-494b-987e-c9331f1e43be"), true, "medical biochemistry", "100352" },
                    { new Guid("b9eaf229-74de-4bb6-9122-bd6cdf82b2c0"), false, "Austrian", "ZZ9000" },
                    { new Guid("ba17b076-2d26-43fc-9699-f3d465eb6822"), false, "Physical Education/Games", "X2016" },
                    { new Guid("ba1d854e-a771-40ea-990d-bddb42106084"), false, "Information Studies", "P2001" },
                    { new Guid("ba2f9096-fe30-423f-b9cb-b5fc36c04361"), false, "Handicraft: Other UK Quals. In Handicraf", "299" },
                    { new Guid("ba4aad2c-d49b-4b9b-ada9-441af75ec314"), true, "polymer chemistry", "101053" },
                    { new Guid("ba535897-21ef-4d3e-9c13-17b23c6ee4eb"), false, "Caribbean Studies", "T9001" },
                    { new Guid("ba7463e2-d78b-4a49-9d11-1cef8c53576d"), true, "Asian studies", "101180" },
                    { new Guid("bb59b1cd-dc87-4a34-b058-a3dea82bd8ac"), true, "urban geography", "100666" },
                    { new Guid("bb7e410b-2a29-40b3-bd0b-a000fcf9f3d3"), true, "community nursing", "100281" },
                    { new Guid("bb7fe797-d547-4598-a706-8d868a5b2a2a"), true, "taxation", "100831" },
                    { new Guid("bb85c7f1-514e-4ccd-8878-3e69e8392fe4"), true, "environmental history", "100670" },
                    { new Guid("bb8e7b5a-f635-4e88-b96d-d10303f954ba"), true, "television production", "100923" },
                    { new Guid("bbae8113-7dfb-4846-9443-b3bd1ee6fef6"), false, "Ceramics Technology", "J3200" },
                    { new Guid("bbbed8b2-85c7-4562-8fba-fc0b283e94fe"), false, "French With German", "R8203" },
                    { new Guid("bc117c81-102f-4b79-b6f3-b5f6bf313f9b"), false, "Property Surveying", "N8003" },
                    { new Guid("bc4b74cf-1262-49d2-9bb0-0d92482eba8b"), false, "Law", "M990" },
                    { new Guid("bc778b4d-8f6b-4c76-be8e-f0104078c2e3"), true, "endocrinology", "101337" },
                    { new Guid("bd2f5b46-76dd-49ea-8dd8-7a0a6edf5ed3"), true, "computer networks", "100365" },
                    { new Guid("bd4284d6-881f-4fd3-b55e-e237da6276f5"), false, "French and Spanish", "Q9704" },
                    { new Guid("bd6ba6a1-dd99-4d37-a4bd-cf2a42790cf5"), true, "Akkadian language", "101415" },
                    { new Guid("bd8c3173-8bc0-477f-a4d0-9c2034191ce8"), false, "Human Movement and Health Stds", "B9901" },
                    { new Guid("bd9b557c-ccf6-4de7-9a8a-dc92e9733863"), false, "French and German", "R1001" },
                    { new Guid("bdba8802-72c0-468f-8752-538461aa0d7d"), true, "applied chemistry", "101038" },
                    { new Guid("bdc0b764-fdf2-49f8-bd42-82ce39b58e0a"), true, "animal health", "100936" },
                    { new Guid("bde24b87-1127-48e0-8781-a67790cfaa29"), false, "Rural & Environmental Science", "F9020" },
                    { new Guid("be09de32-af0f-4ab9-aef3-3ccf11ac5517"), false, "Literary Studies", "Q2002" },
                    { new Guid("be12f399-6502-4b1e-bbb0-563a6e2d826f"), true, "anaesthesia", "101336" },
                    { new Guid("be1f7bd9-45b0-449c-9d2d-9b03796a5780"), false, "Movement Studies/Science", "F9630" },
                    { new Guid("be477e90-58b9-41cf-9bad-6a8cb6f02afb"), true, "accounting", "100105" },
                    { new Guid("be95ea6e-c145-4eba-a13e-db4ce46e513e"), true, "North American society and culture studies", "101204" },
                    { new Guid("bf13c995-0a10-4a93-81c7-3205da3e27c1"), false, "Language Studies & Philology", "Q8814" },
                    { new Guid("bf237e14-eefd-4df6-862d-cfb8676fd8c6"), false, "Geography: As Physical Sce", "L8880" },
                    { new Guid("bf7d5681-1bdb-4c77-8c75-9681e835dd5c"), false, "Health Studies", "B9003" },
                    { new Guid("bfa75e7f-2552-45ea-bb78-a5ea9c00310e"), false, "Training teachers - specialist", "X160" },
                    { new Guid("bfbb0798-8883-43bd-a939-e7cdce91bfe2"), false, "Mathematical Physics", "F3200" },
                    { new Guid("c0156b00-1ab6-4bbe-9d53-d2e5845723b9"), false, "Fine Art & Textile Design", "W2010" },
                    { new Guid("c03c244d-18fc-482e-808e-d68340e46fe2"), false, "Earth Studies", "F9201" },
                    { new Guid("c04980ab-ecec-4768-9190-352550d9d3d5"), false, "Combined Languages", "ZZ9003" },
                    { new Guid("c08fc7b8-2f1d-4397-b9d6-4f00715b7328"), false, "Mechanics", "F3007" },
                    { new Guid("c0b43a2a-ee16-49c8-866c-a7606303bf3b"), true, "electromechanical engineering", "100192" },
                    { new Guid("c0f9ceab-6821-4f25-a2f5-de645d8929cd"), true, "Spanish studies", "100325" },
                    { new Guid("c0fe602f-7947-4e21-a2c3-5a0f6173e8c5"), true, "public services", "100091" },
                    { new Guid("c1287c1f-f94c-4b6b-8455-6ab4844e6ed9"), false, "P E & Recreation Studies", "X9016" },
                    { new Guid("c16edb28-1e08-42cb-97f6-360d63b0e7db"), false, "Voice Production", "W4006" },
                    { new Guid("c1af3251-14cf-4a2a-9813-ee407aa5256a"), true, "general studies", "101274" },
                    { new Guid("c1d57bc5-5f95-46c1-9480-e54b47053b8e"), true, "humanities", "100314" },
                    { new Guid("c1df4012-55f0-48ba-b0f4-b7d19f78ab29"), true, "fashion design", "100055" },
                    { new Guid("c1e22bb8-533a-4689-8537-0a09b09d0aaa"), false, "Applied Physics", "F310" },
                    { new Guid("c1f0aa54-34f0-4413-b2fc-de970cc22afd"), false, "Home Management", "N7502" },
                    { new Guid("c23841d6-c106-4237-b2f0-37c7948fb80a"), false, "Classical Civilisation", "Q8200" },
                    { new Guid("c2794c3c-8bb2-4770-af97-693549b5393b"), false, "Art & Media", "W2501" },
                    { new Guid("c28fae23-0941-47c5-a786-64f48c6b51de"), true, "interactive and electronic design", "100636" },
                    { new Guid("c2b29f64-673a-4f0b-bc34-de35c8b02f27"), false, "Latin Language", "Q610" },
                    { new Guid("c31d8972-3acb-4561-82a9-c7802602d0a0"), true, "neural computing", "100966" },
                    { new Guid("c35f94e5-9870-479d-98be-559c6ece63d3"), true, "Scandinavian history", "101498" },
                    { new Guid("c361eeaa-9201-4463-bb9e-eb1c000d5088"), true, "applied botany", "101376" },
                    { new Guid("c389a18a-8355-4215-9b3c-598d2c24613c"), true, "Nepali language", "101371" },
                    { new Guid("c3c4100a-8b46-4112-ada9-61302b436f3b"), false, "Natural Sciences", "Y1600" },
                    { new Guid("c416fc0c-b976-47d1-853d-27a5600509ab"), true, "European history", "100762" },
                    { new Guid("c45b5cc2-0a39-44d9-ad2e-852e032c10bb"), true, "materials science", "100225" },
                    { new Guid("c47f33ec-bad8-4b1f-b93f-d5ff4cb97a37"), true, "clinical medicine", "100267" },
                    { new Guid("c47f90cc-2677-4c9b-9642-3d1cbaa57551"), false, "Child Developement", "X9002" },
                    { new Guid("c4985fa2-4f47-4d4b-ad04-e62c41528f69"), true, "South Asian history", "100772" },
                    { new Guid("c4c69938-9c5c-46dc-a528-ceb6791011de"), false, "Modern Languages", "T2004" },
                    { new Guid("c4e6c135-9bd1-450c-9b83-26aaa98b4f3c"), true, "jurisprudence", "100691" },
                    { new Guid("c4f93439-2c82-41c0-92b5-2b6b7837b367"), false, "Environmental Technologies", "K3400" },
                    { new Guid("c51568f3-30dc-445a-8b58-d322ebf7ec45"), true, "comparative politics", "100618" },
                    { new Guid("c5271e45-4ec0-49a1-9e38-291bee29c595"), true, "Indian literature studies", "101430" },
                    { new Guid("c53973ed-e05e-4bfa-b3b3-bc1031c481e9"), true, "optoelectronic engineering", "100169" },
                    { new Guid("c56f6875-4575-4518-bd70-fa64bbc0b0b1"), true, "sports development", "100096" },
                    { new Guid("c5b96d63-e4b1-4083-8179-7ab9eeb5cdee"), true, "modern Hebrew language", "101269" },
                    { new Guid("c5c21379-212c-4299-bd59-22b894c535f5"), false, "Studies In Technology", "W9907" },
                    { new Guid("c5d2f92b-dcbc-4688-acb5-21e0229497d4"), true, "applied music and musicianship", "101450" },
                    { new Guid("c5e49420-9dad-43d2-82d8-99a0fc1aa901"), false, "Mathematical Science", "G1500" },
                    { new Guid("c5ea4c27-88ee-4705-8001-2b24866b09b9"), true, "Thomas Hardy studies", "101470" },
                    { new Guid("c63056cf-3af9-4c5f-a121-13c0f78c312d"), false, "Biology and Science", "C9705" },
                    { new Guid("c6dc4d6d-e6e6-4da9-8dd3-aa63a23a6394"), true, "organometallic chemistry", "101389" },
                    { new Guid("c6def189-29db-4752-8a04-0665e2022c30"), true, "baking technology management", "101021" },
                    { new Guid("c6f1935a-5bcc-4623-a9fc-714a52f28b87"), true, "ophthalmology", "100261" },
                    { new Guid("c73984dd-572a-4eb1-91f7-12a6e01b9ba9"), false, "Law", "M200" },
                    { new Guid("c766a01a-f6a2-4072-966e-cb14dd605a15"), true, "salon management", "100896" },
                    { new Guid("c7f4b2c1-62f5-45b2-9bdb-1afe5d1e4ce7"), true, "Spanish literature", "101139" },
                    { new Guid("c7f8bae8-792d-47f4-9367-9d72db7fdb91"), true, "gemmology", "100550" },
                    { new Guid("c8012ead-438f-4d75-99f8-c1b9bc0c58ea"), true, "electrical power", "100581" },
                    { new Guid("c806d146-4e64-4d96-a054-c959d232ec47"), true, "quaternary studies", "101091" },
                    { new Guid("c84b5d7e-5b1e-4d12-b24a-6cf426bf3090"), true, "glass crafts", "100724" },
                    { new Guid("c88e2d61-af9e-48a3-8d76-0b06bcdb9599"), false, "Physiology", "B1000" },
                    { new Guid("c8b2d5a7-01a8-4b29-ae53-b5ff2d4a5ac9"), true, "professional practice in education", "101246" },
                    { new Guid("c8d7188a-2b91-4344-96a5-c9656ff65bc8"), true, "chemistry", "100417" },
                    { new Guid("c8db0527-5be2-40fd-b74b-26aeae0c29be"), true, "institutional management", "100815" },
                    { new Guid("c9018d7e-91e2-4834-bccd-13ab863f978f"), true, "aquaculture", "100976" },
                    { new Guid("c93002a8-46aa-4318-86ba-cb1ace5f0d7f"), false, "Env. Stud (Geog Hist Science)", "L8205" },
                    { new Guid("c9c395c7-dbd1-4a88-bef3-fdc7517e53d6"), true, "business and management", "100078" },
                    { new Guid("c9eea828-f4f6-442b-b4ab-4c768c9fbe08"), false, "Multi-Cultural Education", "X6007" },
                    { new Guid("ca0661f7-c906-469f-9e07-03611b63a732"), true, "brewing", "101022" },
                    { new Guid("ca2c8c0d-f51e-41f8-80e3-5b71e07bff09"), false, "Science & Environ Studies", "F9614" },
                    { new Guid("ca449a7c-7512-43bc-862a-6f1d838cd8d5"), true, "environmental impact assessment", "100549" },
                    { new Guid("ca4e606c-90b3-4a61-bf57-20f828e58fe2"), true, "music composition", "100695" },
                    { new Guid("ca74f0a9-1509-4cb5-bd7e-bc8a54f8f84f"), true, "physiotherapy", "100252" },
                    { new Guid("caa10698-4485-475a-9e87-c537fd8e4cc1"), true, "medicine", "100271" },
                    { new Guid("cab1f7ab-b05c-4bd3-95a2-801ccf1a51f3"), false, "Outdoor Education", "X2004" },
                    { new Guid("cad2a545-70fd-43d2-b3c9-98e334554912"), true, "financial reporting", "100845" },
                    { new Guid("caea9ab2-4db7-4ef9-a5a5-8dc1b0be9b8e"), true, "e-business", "100738" },
                    { new Guid("caeb3f43-056a-4b56-b94f-581f0696919c"), true, "architectural design", "100583" },
                    { new Guid("cb3e50c0-40a9-4b4e-9705-6f2008f56592"), true, "public health engineering", "100565" },
                    { new Guid("cb427f97-4d24-4cb6-86b9-86601c5b695b"), true, "sports management", "100097" },
                    { new Guid("cb4505df-5542-4907-9019-038b339fbc79"), true, "beauty therapy", "100739" },
                    { new Guid("cb992416-0831-47ca-8776-41edb36464a6"), true, "critical care nursing", "100282" },
                    { new Guid("cb9c27bf-2fb1-40c6-bde0-783a65f19c75"), true, "audiology", "100257" },
                    { new Guid("cbdc1c75-88f0-46b5-93eb-aca938af5ba0"), true, "industrial chemistry", "101041" },
                    { new Guid("cbe6d563-eae4-4c9d-b891-a02295fadd3e"), true, "creative computing", "100368" },
                    { new Guid("cbfcc9d2-7d8b-4e6b-83c9-e708ff39574f"), true, "medicinal chemistry", "100420" },
                    { new Guid("cc4495cf-5ad0-45db-a54d-9f15e3e00a9d"), false, "Natural Environmental Science", "F9003" },
                    { new Guid("cc74eb90-5bd1-4f61-8079-1ce19fdc7205"), false, "International Politics", "M1500" },
                    { new Guid("cce515d7-6e1b-403a-aee7-bd6a8684b891"), true, "opera", "101448" },
                    { new Guid("cd1bdfbf-0690-4dd4-93c4-17fb286344d0"), true, "evolutionary psychology", "101345" },
                    { new Guid("cd3637d6-0101-4506-8af0-148373a58bda"), false, "Land and Property Management", "N8000" },
                    { new Guid("cd3af8db-16de-4d40-9413-9edbbd632198"), true, "psychobiology", "101344" },
                    { new Guid("cddccad7-fe95-4f83-b1b5-bc08f9c0c394"), false, "Human Geography", "L8200" },
                    { new Guid("cdddf2a8-627c-4a16-8ce9-22a8e7771798"), true, "specialist teaching", "101085" },
                    { new Guid("ce15e970-f0e1-45b1-9628-fa5763dc19bf"), true, "Jane Austen studies", "101478" },
                    { new Guid("ce4a64c8-c238-42c1-8c59-c48ff7f46d9c"), true, "energy engineering", "100175" },
                    { new Guid("ce63a226-e135-4e7e-951c-53bffc456ef7"), false, "Archive Studies", "P1600" },
                    { new Guid("ce7d5f85-8830-4401-bd38-06a14831c799"), false, "Welsh Literature", "Q5203" },
                    { new Guid("ce8302b7-508b-435b-95ed-451613e03a96"), true, "transport geography", "100669" },
                    { new Guid("ce889e64-a01c-4f57-8121-52356fc9e096"), false, "Business Administration", "N1200" },
                    { new Guid("ce959395-4633-4e19-b001-9a475ae67653"), false, "Inorganic Chemistry", "F1901" },
                    { new Guid("cf08921b-e8f8-4b20-9a93-34a9224cc5f5"), true, "biomedical sciences", "100265" },
                    { new Guid("cf22fc43-3c60-4800-b742-d42dacccb33d"), true, "marine engineering", "100544" },
                    { new Guid("cf397d0a-66ee-40e5-a670-64156f1fb572"), true, "youth and community work", "100466" },
                    { new Guid("cf9f4ee7-1a37-48d9-8e83-4dbc75a18a0e"), true, "Persian society and culture studies", "101503" },
                    { new Guid("cfcf7ae5-3212-4ab3-a041-79d130b89112"), true, "Welsh studies", "100335" },
                    { new Guid("cfe56b8b-9460-43fb-9596-f5508d92d6f2"), false, "Mathematics and Science", "G9005" },
                    { new Guid("d005f440-b514-4fab-9143-5e21b45de47d"), true, "religion in society", "100626" },
                    { new Guid("d006549b-a1ec-4bd7-83d2-1c70bfc46c47"), true, "bioelectronics", "101216" },
                    { new Guid("d02b6e5c-5881-403f-a017-6d8ad9982d5c"), true, "textiles technology", "100214" },
                    { new Guid("d0690fdf-edbd-40bc-8eda-f320f645f05f"), true, "counselling", "100495" },
                    { new Guid("d08fddbb-996a-42de-b978-8e44b3fc3c64"), true, "travel management", "100102" },
                    { new Guid("d0b8e759-b2b5-4f51-affb-b768b2c26026"), false, "Personal and Social Education", "ZZ9008" },
                    { new Guid("d0c61f60-1c94-4c7c-a7a1-6ecb9639e516"), false, "Japanese", "T4000" },
                    { new Guid("d0cbbab4-7449-4a90-a7f6-b2caa02616f7"), true, "technical stage management", "100704" },
                    { new Guid("d0f0178e-8d9b-4fc9-9726-4ca51d2657d7"), true, "physical sciences", "100424" },
                    { new Guid("d0f6ed5c-cccf-4fd5-9c88-e4f60c42677b"), true, "business economics", "100449" },
                    { new Guid("d0f820e4-17a6-4a63-a626-b9dd0f660da8"), true, "Scots law", "100678" },
                    { new Guid("d13d8378-641b-487c-b5a2-afa8b218a655"), true, "hospitality management", "100084" },
                    { new Guid("d167eca9-1b6a-49bf-a1fb-c6b397a8823e"), false, "Biology With Core Science", "C9701" },
                    { new Guid("d188c3a7-499e-4cdd-b4ed-a34e6c0ce08f"), false, "Maths With Computer Science", "G9003" },
                    { new Guid("d1c608d4-3490-41d3-914f-89deff0aab74"), false, "Physical Geography", "F8400" },
                    { new Guid("d1e609da-a1b6-4c46-8665-3b9352c33734"), false, "Experimental Psychology", "C8001" },
                    { new Guid("d1fcfb05-ebe0-471d-ae42-7ec550bc2769"), true, "developmental biology", "100834" },
                    { new Guid("d204118b-41f8-462c-875a-73012272c126"), false, "Public Services", "L430" },
                    { new Guid("d217b5ec-2e3c-4a6c-90a4-a0f9d4776da9"), true, "agriculture", "100517" },
                    { new Guid("d21cd1d1-4b97-4114-9163-d4df6ae35ee5"), true, "Brazilian studies", "101143" },
                    { new Guid("d227cd41-132e-4143-a17c-40313d348af3"), false, "French", "R100" },
                    { new Guid("d23f7737-8ecd-4897-a8c5-c90fb6255c8d"), true, "information management", "100370" },
                    { new Guid("d24cd9b5-8134-4877-af1f-177a37bbc030"), false, "Development Studies", "M9200" },
                    { new Guid("d2554651-ecca-4991-b4ca-62f3a5290807"), true, "osteopathy", "100243" },
                    { new Guid("d272a428-a91e-44c3-bdf2-15e2f51ef5c8"), false, "Arts-General (Where Subject Not Spec)", "Z0079" },
                    { new Guid("d2b10830-695b-48f5-ac2a-51e30f5f0af9"), false, "Phonetics", "Q1300" },
                    { new Guid("d2b7f2cc-3d16-4afa-9e4a-b0bd62fdc187"), true, "archives and records management", "100915" },
                    { new Guid("d2f4140b-f3dc-4eef-946b-a9b909c8917f"), false, "Drama & Theatre Studies", "W4403" },
                    { new Guid("d30bb32c-07c2-4668-b257-f58d9dacbfb8"), true, "German society and culture", "101135" },
                    { new Guid("d30f867e-3767-479a-b4b6-caa6ef587033"), true, "gas engineering", "100176" },
                    { new Guid("d313d5fb-154d-4ffc-b012-408aa374b42d"), true, "information technology", "100372" },
                    { new Guid("d31e3eb4-4ea8-4881-b7c4-dc990bc207a0"), true, "machine learning", "100992" },
                    { new Guid("d33d5ac6-fec4-4ee6-bec9-78f70f63580b"), true, "coaching psychology", "101294" },
                    { new Guid("d34e49fd-01be-4f8e-8aca-e3be7bf356d3"), false, "Spanish (And Studies)", "Z0043" },
                    { new Guid("d35f3818-6992-4d32-be2c-c42c6fff61f8"), true, "childhood studies", "100456" },
                    { new Guid("d3b0d79b-4741-428b-824d-e98dd5d8d3c5"), true, "fine art conservation", "100599" },
                    { new Guid("d3d9d21f-fe99-4110-97e4-e0a8bc66880b"), false, "Economics", "L100" },
                    { new Guid("d403121c-79f6-4d3a-9088-149b0558e878"), true, "virology", "100910" },
                    { new Guid("d40c6393-f3a4-4282-9ec6-b912c1e56d7d"), true, "Chinese languages", "101165" },
                    { new Guid("d40d8390-8abb-4109-a025-b81e9bc2a41c"), false, "Human Development", "L7202" },
                    { new Guid("d4596df0-10aa-4eb5-9169-daaf7d83c6bd"), false, "French Literature", "R1102" },
                    { new Guid("d47f0c82-af98-47d6-a521-839a0b9b810b"), false, "Classics", "Q800" },
                    { new Guid("d49391e7-b1c0-4f5b-98b8-abc5513d15e8"), true, "leadership", "100088" },
                    { new Guid("d4a6eff9-fbad-4f0c-9a2b-ae8a6362c794"), false, "French Lang, Lit & Cult", "R8810" },
                    { new Guid("d4ed37cd-f314-4be1-a49a-9920389d8d20"), false, "Asian Languages", "T5016" },
                    { new Guid("d4f0b29c-59db-4948-908a-f3dd4eb5a91f"), true, "W. B. Yeats studies", "101494" },
                    { new Guid("d526c198-94dd-45b7-91b1-3c118c991072"), true, "timber engineering", "101013" },
                    { new Guid("d554c863-efe9-4aa6-9777-d3aa0d30fc76"), true, "genetic engineering", "101378" },
                    { new Guid("d56187ae-6c84-4854-bd2c-c62d47bdc185"), true, "spa and water-based therapies", "101375" },
                    { new Guid("d564a982-5196-4ad8-819d-a8ea17e85614"), false, "Science-Chemistry-Bath Ude", "F1004" },
                    { new Guid("d63080e2-0473-4b02-aa29-9bf7c93fa9da"), false, "German Literature", "R2101" },
                    { new Guid("d6612991-e472-4c52-910a-c2ff1111d326"), true, "African languages", "101185" },
                    { new Guid("d6642c9e-617d-44df-bd10-cc6f25600279"), true, "Christopher Marlowe studies", "101487" },
                    { new Guid("d6916503-d9da-4fdc-855f-b76c48dca98d"), false, "Hispanic", "R4001" },
                    { new Guid("d6c65afa-98c8-4bfe-939a-a3b2723483f9"), false, "Spanish Language & Studies", "R4101" },
                    { new Guid("d75bd09b-4bad-48c3-8080-5c4c3e98298f"), false, "Japanese Lang, Lit & Cult", "T8840" },
                    { new Guid("d7733b81-5436-44fe-9507-bf3b69789fdb"), false, "Contemporary Studies", "V9000" },
                    { new Guid("d7768f73-c0c7-4c16-a225-2a64c5575f26"), true, "population genetics", "100902" },
                    { new Guid("d7871045-0a1e-47fa-af33-ca0a252f06fe"), true, "statistics", "100406" },
                    { new Guid("d791d361-7477-498e-af46-6e1acf764418"), false, "Child Sexual Abuse child Sexu", "X9006" },
                    { new Guid("d81173c0-a593-4780-b913-9a9a75713d1c"), true, "knowledge and information systems", "100963" },
                    { new Guid("d833e4a3-d668-4a46-96a8-885faccbf38a"), true, "photonics and optical physics", "101075" },
                    { new Guid("d8936d39-17bc-4d18-984d-b9d06771755c"), false, "Physical Education With Dance", "X2005" },
                    { new Guid("d8e8516a-70cf-4f07-b8c1-fb1e0a34b4f2"), false, "Turkish", "T6800" },
                    { new Guid("d904bcf8-bd2c-499f-ab29-ed8ad11c048a"), false, "Business Studies & Info Tech", "N9703" },
                    { new Guid("d9489cd4-c99a-4619-84f5-c802546e8c2f"), false, "General Humanities", "V8890" },
                    { new Guid("d9505e72-082a-4772-97a9-4a1f060d2364"), true, "German language", "100323" },
                    { new Guid("d96d6983-7dab-44e4-8df7-ab4e90432a43"), true, "change management", "100813" },
                    { new Guid("d9a1e6c9-3afd-41cc-807d-ed197d03d3e3"), false, "European Studies", "T2000" },
                    { new Guid("d9c30724-4010-42dc-91e5-7f5293b57ce7"), true, "building technology", "100584" },
                    { new Guid("d9c38734-2b4d-45e2-bda4-bca3a44cf6f0"), false, "Design (C.D.T.)", "W2406" },
                    { new Guid("da17ea40-3fc7-45ea-9e4c-e6d056098294"), false, "Welsh", "Q560" },
                    { new Guid("da5c71cf-5011-452c-ac9c-e86c407b43f5"), false, "Divinity", "V8002" },
                    { new Guid("da5fc0bc-2ae9-4311-ad51-27d374c6b63e"), true, "Arabic languages", "101192" },
                    { new Guid("da70c3cc-2649-4d80-84f8-904aa033a478"), true, "internet technologies", "100373" },
                    { new Guid("da8723e5-e152-4255-8b17-935794b396f8"), false, "General Studies In Science", "Y1000" },
                    { new Guid("da8aaa46-2434-454d-b5a6-d052f37f85ed"), true, "psychology of communication", "101341" },
                    { new Guid("da9e69c6-bafe-43c9-ac7b-325521a17dc7"), true, "visual and audio effects", "100717" },
                    { new Guid("daaddafb-9e5b-407f-bbb8-ae64b3e77d32"), true, "freshwater biology", "100849" },
                    { new Guid("daba812b-6b8d-4985-8e44-ffe68a678c2e"), true, "Russian history", "100766" },
                    { new Guid("dac12299-4bc1-4043-8a91-829f51b2b4ec"), true, "audit management", "100840" },
                    { new Guid("dad16907-e3ef-455a-a409-4b51f92615f4"), false, "Chinese", "ZZ9002" },
                    { new Guid("dad1f2f0-26f0-40ff-9880-69869471f600"), false, "Drama and Education", "W9915" },
                    { new Guid("db15d068-5529-4242-8f85-0c18ecba5c9f"), false, "Sociology", "L3000" },
                    { new Guid("db700fd7-0464-4588-aed6-bd815d3abef5"), true, "English literature", "100319" },
                    { new Guid("db853097-40ad-4eec-942b-70ca70ed7a93"), true, "international social policy", "100645" },
                    { new Guid("dbe03c07-c7c4-4b2e-b799-67a62e4e9b6a"), false, "Design and Technology Ed", "H8703" },
                    { new Guid("dbf19c97-0aa5-4a00-a04a-e082b21b4da2"), false, "Analysis Of Science and Tech.", "F9604" },
                    { new Guid("dc40d307-15ac-49a5-8f83-9b0cc00cccdc"), false, "Env Studies (History & Geog)", "F9025" },
                    { new Guid("dc4f30ce-1f4c-4006-ba7c-9af5b6f1a6b7"), true, "midwifery", "100288" },
                    { new Guid("dc55af5e-cb8f-434c-bf04-7b1abbe7c084"), true, "plant biotechnology", "100139" },
                    { new Guid("dc8ab290-b5d3-4c0a-94b1-5775fc5af443"), true, "rail vehicle engineering", "101398" },
                    { new Guid("dc964b4a-3102-446b-be6d-27b6406bce49"), true, "the Qur'an and Islamic texts", "101445" },
                    { new Guid("dc977c5f-110e-4d31-a91f-8eb3a72e07c6"), true, "acoustics and vibration", "100580" },
                    { new Guid("dd303be9-b768-4f96-b0ef-62ee0ee05189"), true, "ancient history", "100298" },
                    { new Guid("dd4ab15d-8181-4745-9fef-cd40a7db2595"), true, "occupational therapy", "100249" },
                    { new Guid("dd51e366-6b2e-4df7-8a87-f6f882985cd9"), true, "phonetics", "100972" },
                    { new Guid("dd74d98d-279e-4d89-aeef-dba8242b1730"), false, "Fine Arts", "W1000" },
                    { new Guid("dd8326c9-1a45-4c8d-a324-c2c6f8ffd9e8"), true, "health risk", "101049" },
                    { new Guid("ddd0d723-8226-4b3a-81a7-1293474fc3c7"), false, "Other Modern Language", "R900" },
                    { new Guid("ddfc9379-0cd2-4d8f-8d60-9de421a6c5ae"), true, "English as a second language", "101109" },
                    { new Guid("de0ed51e-81ef-4d2a-b00f-a5494b320490"), true, "pastoral studies", "100802" },
                    { new Guid("de42b10d-c9d1-4a89-8e0f-64f53bbf9487"), false, "Primary Curriculum", "X9005" },
                    { new Guid("de552b6c-448e-4d7e-b158-8b087a380983"), false, "Human Movement Studies", "W4503" },
                    { new Guid("de6c0a92-1dde-45b7-8eee-4f6e9d8bc2e7"), true, "mechanical engineering", "100190" },
                    { new Guid("de87c813-c7fb-43ba-a4cc-ca0c73e75a29"), true, "UK government/parliamentary studies", "100610" },
                    { new Guid("de8a5425-60d9-43ee-a99a-1af8ea724d3d"), false, "Biology Botany", "Z0023" },
                    { new Guid("df00483a-ab33-4bb5-9ad6-f49bba14eb86"), false, "Architectural Studies", "K1003" },
                    { new Guid("df012151-2665-4b3a-b0e0-2e06b3c3ba8d"), true, "hair and make-up", "100706" },
                    { new Guid("df0ad2cc-2384-4a1e-9665-d6a5a017b9c9"), true, "graphic arts", "100060" },
                    { new Guid("df8ed884-d0a2-4ae1-8449-1c24c0134f75"), true, "media and communication studies", "100444" },
                    { new Guid("df9507ea-c998-4a39-86ff-eade84e8165f"), false, "Building Studies", "K2001" },
                    { new Guid("dfaa8799-02cc-425f-b77d-78fa16f83cbb"), true, "mental philosophy", "100791" },
                    { new Guid("dff9472f-9e0e-4194-9bfd-f1a3b47b1352"), true, "international politics", "100489" },
                    { new Guid("e03a0e07-f459-4632-8091-d476961c2924"), true, "anatomy", "100264" },
                    { new Guid("e03f20da-f100-4653-845d-5988fe9426c4"), false, "Theatre Arts", "W4402" },
                    { new Guid("e0445402-2733-4aab-aa8e-1d96a03b10b9"), false, "Information Science", "P2000" },
                    { new Guid("e07d21a3-fb20-44b6-8ad2-59dc555f3093"), false, "Computer Studies", "G5000" },
                    { new Guid("e0aea476-b488-4c6c-9dd1-83e18223b100"), true, "public accountancy", "100837" },
                    { new Guid("e0aef8be-fc21-4a5e-9304-dbe38a6e0055"), true, "ocean sciences", "100421" },
                    { new Guid("e0b8b9fa-8b02-4252-bed8-0b7f6f68df1d"), false, "Biological Science", "C1200" },
                    { new Guid("e0edc443-05cb-46ed-b3b6-02cd4ed01c48"), true, "English studies", "100320" },
                    { new Guid("e0ee6c0b-d81d-400f-8a12-9dc4eb6309a8"), false, "Computer Educ With Science", "G5401" },
                    { new Guid("e0f053bf-392b-42f6-aea9-60238c1c40f4"), true, "numerical analysis", "101027" },
                    { new Guid("e1090d4a-9c19-442c-bd8b-4f668a5346c4"), true, "illustration", "100062" },
                    { new Guid("e1148d89-8e02-495a-9d6c-aeb15c397c20"), true, "epistemology", "101442" },
                    { new Guid("e12ba69b-a002-4515-9e49-2c9b862efb88"), false, "English With Drama", "W4003" },
                    { new Guid("e15a93b7-5296-4233-8855-68d9a8a56551"), false, "Speech Training", "B9504" },
                    { new Guid("e167c3c7-cd26-42d1-9213-20a01c2e8c26"), false, "Careers Education", "L500" },
                    { new Guid("e1858c75-659d-4542-9068-aeba745fb72a"), true, "sacred music", "100844" },
                    { new Guid("e1a90632-070b-4d64-b02b-8452b667ca9f"), true, "exotic plants and crops", "101348" },
                    { new Guid("e1c6a94f-3e3e-4c7a-81b7-06e59fd0522c"), true, "construction", "100149" },
                    { new Guid("e20dbb12-143a-4f12-bef4-e60b0782ca70"), true, "public law", "100684" },
                    { new Guid("e235aa54-7c9a-4fa3-8e02-93d488b3f395"), true, "south Slavonic languages", "101428" },
                    { new Guid("e2c85faa-12d5-4bc2-814c-47a18bcef5aa"), false, "Literature & Media Studies", "P4600" },
                    { new Guid("e2d2c5a6-ea7f-47f9-aacd-37777f6eadef"), false, "Education Of The Disadvantaged", "X6004" },
                    { new Guid("e2e11be9-30ab-4674-b2a5-9e8a202003c6"), false, "Edn.Of Childn.With Sp.Needs", "X6401" },
                    { new Guid("e2e7be55-7a68-41b0-8957-ff49d8a7bcc3"), false, "Visual Communication", "W1501" },
                    { new Guid("e2eea4f0-5351-4ac0-a1fc-208944fa06bf"), true, "financial management", "100832" },
                    { new Guid("e3217a84-07a5-46d0-9f71-d88925c96a91"), true, "psycholinguistics", "101035" },
                    { new Guid("e36116d4-7dcb-4156-a262-ed5841db46fa"), false, "Language & Literacy", "Q1404" },
                    { new Guid("e3ec5822-212e-43a3-ac71-980258883a4e"), true, "anarchism", "101404" },
                    { new Guid("e3ee0872-032e-43a5-a653-050406f50d78"), true, "the Torah and Judaic texts", "101446" },
                    { new Guid("e3ef48af-7a82-4cd8-a7bf-3f15302179ad"), true, "surveying", "100219" },
                    { new Guid("e43d7f10-5ffb-4f73-9e94-e96df292c24e"), false, "Social Biology", "C1900" },
                    { new Guid("e476bedc-e9a0-4712-a9dd-fbbddcdb68e9"), false, "Physical Education and Dance", "X2017" },
                    { new Guid("e485602f-75f3-483b-a031-f98d40dc3992"), true, "therapeutic imaging", "100132" },
                    { new Guid("e4c2c604-ed14-401f-a6d6-27e54a870f8d"), false, "Business and Management Studies", "N8810" },
                    { new Guid("e4de772e-08be-4568-92b4-0c6b8c414bc6"), false, "Mfl(French, Spanish, German)", "Q9707" },
                    { new Guid("e5185420-9f8e-4c2a-b64e-075e95c74ae1"), true, "geochemistry", "101083" },
                    { new Guid("e5407292-d96b-4370-abbd-6b0cc7126904"), true, "creative writing", "100046" },
                    { new Guid("e54e98b3-21c2-45f5-b771-727162ff46b7"), true, "oil and gas chemistry", "101054" },
                    { new Guid("e5687a38-03cf-42da-b605-f1f989fe47d5"), true, "object-oriented programming", "100960" },
                    { new Guid("e5b6f308-b651-4a0f-a1f1-a0672dba004f"), true, "organic farming", "101004" },
                    { new Guid("e5e63d47-c866-4fd3-b1ef-ff2f3023e11e"), false, "Design Studies", "W2000" },
                    { new Guid("e60edb18-4647-41bd-a999-9a8d7a27ddfc"), true, "criminology", "100484" },
                    { new Guid("e65e88af-9a57-45c0-8ed4-06c86be83b05"), true, "physical chemistry", "101050" },
                    { new Guid("e6759210-0976-48a5-9e17-7d87c77f0a4e"), false, "Combd Science With Intens Phys", "Y1003" },
                    { new Guid("e678940a-8ef9-420b-aa7e-4675f9924d7a"), true, "nursing", "100290" },
                    { new Guid("e68cd222-b101-4fcd-b023-f513dc7e11b7"), false, "Academic Studies In Education", "X8830" },
                    { new Guid("e7796247-305f-43d3-8e4f-8ed4ed5f3707"), true, "actuarial science", "100106" },
                    { new Guid("e807718b-ccf8-4591-86b5-9210b45a9cf3"), false, "Theological Studies", "V8005" },
                    { new Guid("e850fc06-9ab9-47ef-b1ac-f8cd71aee48e"), false, "French Studies (In Translation)", "R1100" },
                    { new Guid("e8573983-9da2-4de7-905c-ed6e99171156"), true, "sport and exercise psychology", "100499" },
                    { new Guid("e87b5dff-9713-4ce6-8a3b-bbda20bf36c9"), true, "Byron studies", "101483" },
                    { new Guid("e894f3fa-6f5c-44c9-8be1-b69880a91087"), false, "Health and Movement", "B9902" },
                    { new Guid("e89f6e45-73b2-4edc-a0b9-b5cc33bf03b7"), false, "Home Science", "N7503" },
                    { new Guid("e8a2e040-b79c-497d-b06a-c398d274d805"), false, "Language", "Q1403" },
                    { new Guid("e8ca0225-809f-4e97-a345-5ccb589ebccd"), false, "Tech:design and Technology", "W2506" },
                    { new Guid("e8da5a30-bd90-40a1-a08c-3924989970db"), true, "countryside management", "100468" },
                    { new Guid("e8fa2a9d-39b4-46ec-b0b7-d0b4daa41498"), false, "Journalism", "P4500" },
                    { new Guid("e906cefa-7869-4c9d-a031-c9d0e057e330"), true, "bioinformatics", "100869" },
                    { new Guid("e92a3d96-7b7f-4778-8cf6-9ee96e75e4f8"), false, "Physical sciences (Combined/General Sciences)", "F900" },
                    { new Guid("e949b371-bd92-41c6-8b2b-496cc2ee6f39"), true, "English history", "100761" },
                    { new Guid("e9568024-ca71-48b4-9df1-0d7d7da77b47"), true, "poetry writing", "100730" },
                    { new Guid("e9752e40-4933-45ea-94c4-d430e505eb79"), true, "psychology", "100497" },
                    { new Guid("e9847152-9e38-4cfb-ae6e-54635d6e004a"), false, "General Science", "Y1002" },
                    { new Guid("e9bb45be-c962-4898-b85e-78e505beb3aa"), true, "clothing production", "100109" },
                    { new Guid("e9db1d2c-c092-4b96-959c-e6e49b3bf685"), false, "Technological Mathematics", "G5008" },
                    { new Guid("e9e124f8-4172-4dbc-a361-92514ce7edc7"), false, "Policy Making", "N1208" },
                    { new Guid("ea060f51-ec47-4f86-8d2b-16d0fb4f779d"), true, "early years teaching", "100510" },
                    { new Guid("ea1f1f16-b89d-4360-81de-dcdba78a10d1"), false, "Expressive Arts", "W9003" },
                    { new Guid("ea8b79de-7593-4ff8-93db-96dc03498c0e"), false, "Arts and Physical Education", "W1009" },
                    { new Guid("eab76fae-9f79-46c7-b45c-4fe045e2074d"), true, "Arab society and culture studies", "101198" },
                    { new Guid("eaeba795-c510-4905-afca-4fa71b2fda23"), true, "computing and information technology", "100367" },
                    { new Guid("eaed7f23-49c8-4a00-bf64-68b8255e49d5"), true, "acupuncture", "100233" },
                    { new Guid("eaef7076-30e4-4284-89fc-aa08b0ccf41a"), true, "journalism", "100442" },
                    { new Guid("eb162764-b8f3-4315-9d64-635d2bdc1959"), true, "African history", "101360" },
                    { new Guid("eb65cb20-1345-4c92-a318-63f394503a52"), true, "applied environmental sciences", "101078" },
                    { new Guid("eb79a44e-cd6e-4960-abda-a881e0851304"), false, "Personal, Social and Moral Ed", "L8206" },
                    { new Guid("eba29847-22e9-4e6b-8cf7-3919971aaec6"), true, "Walter Scott studies", "101488" },
                    { new Guid("eba938fc-cd46-4032-80f6-682bf0d06c6d"), false, "Ceramics", "J3001" },
                    { new Guid("ebb0c244-5a35-4a63-a3b7-d878648d774c"), false, "Co-Ordinated Sciences", "F9618" },
                    { new Guid("ebc3de0a-8a5e-4c35-8a55-53f12cd4110d"), false, "Ancient Greek", "Q7001" },
                    { new Guid("ebeadfa0-96d0-46ac-a6c6-015b2c37531c"), true, "Joseph Conrad studies", "101495" },
                    { new Guid("ec16d283-cd7c-49e2-a8d0-c82c7b1ec03a"), false, "Philology", "Q1402" },
                    { new Guid("ec17389d-4769-418b-a580-309f63b86abe"), true, "analogue circuit engineering", "101399" },
                    { new Guid("ec394be2-d98d-469d-a775-095ada1fadfd"), true, "organisational development", "100814" },
                    { new Guid("ec58dc4b-e68c-4978-9193-d399c0bc9d7c"), true, "Philip Larkin studies", "101475" },
                    { new Guid("ec8763d0-b9b3-4105-8009-0c5a12d114d0"), true, "strategic studies", "100616" },
                    { new Guid("ecb7ec68-3a57-4174-8c33-201d7b535f45"), false, "Catering & Institutional Management", "N8870" },
                    { new Guid("ece5432a-5102-466c-b17d-1e13f3d2a6c2"), true, "farm management", "100978" },
                    { new Guid("ece6efe0-9e8f-4a24-910a-3f7de2c3d606"), true, "shorthand and shorthand transcription", "101409" },
                    { new Guid("ed0911a4-c826-4336-a02a-9b84f25496d6"), false, "Music & Instrumental Teaching", "W9928" },
                    { new Guid("ed0e81ec-819f-419a-addc-ac25fdb9b4db"), true, "painting", "100589" },
                    { new Guid("ed23419a-1083-4d82-a58e-87e370cdd556"), true, "building surveying", "100216" },
                    { new Guid("ed247101-f5e0-4d4b-a731-1c45857886de"), true, "Samuel Beckett studies", "101481" },
                    { new Guid("ed4acaa0-8d31-4fd2-ad5f-4d7ae7abbe22"), false, "English Language", "Q3005" },
                    { new Guid("ed4f50d2-2780-4b13-9e19-28a82f602d31"), false, "Ecology", "C9000" },
                    { new Guid("ed7ea186-a794-4439-8667-cdf2d00cb2a7"), false, "Psychology", "C800" },
                    { new Guid("ed8d7fff-a9af-4fba-90e7-2f9e7ffe3a43"), true, "gender studies", "100621" },
                    { new Guid("ed969adf-13ba-4d70-99d7-fcbb28a29fe7"), false, "Art and Design Studies", "W9923" },
                    { new Guid("edd3e60e-676c-4967-bbe4-2c0f4b14ed71"), false, "Politics", "M1000" },
                    { new Guid("edf17c48-b549-441e-87e4-08507e0961f8"), false, "Metals Technology", "J2001" },
                    { new Guid("ee1d7f99-577b-4bce-9bf4-37e333a0302b"), false, "Environmental Science", "F9000" },
                    { new Guid("ee464a46-6110-4639-b36e-8211a642618f"), true, "cybernetics", "101355" },
                    { new Guid("ee571a97-0208-4d35-a555-799ec1bbcd4a"), true, "agricultural economics", "100600" },
                    { new Guid("ee6fb4fd-5e7b-44db-a13f-662881177d78"), false, "Modern Literature", "Q2004" },
                    { new Guid("ee82f812-1f99-4b75-85bb-ae8403ebdf60"), true, "public international law", "100681" },
                    { new Guid("eea5b18c-8165-4b32-87d9-ff93755609f6"), false, "Art", "W900" },
                    { new Guid("eec6ab17-135c-4053-ad5a-1defbbaf98cc"), false, "Integrated Physical Sciences", "F9606" },
                    { new Guid("ef282e34-12b1-416a-960d-882038d985f9"), true, "intellectual history", "100781" },
                    { new Guid("ef43f5b9-514e-4f23-8c3b-3dcf8842d5c4"), true, "European business studies", "100808" },
                    { new Guid("ef4ab814-d8d5-4ddf-b959-28cc8ddc8c75"), true, "forensic archaeology", "101219" },
                    { new Guid("ef4ca8c1-d5b9-45d8-a0a2-5e23870472cc"), false, "Environmental Issues", "F9612" },
                    { new Guid("ef5a08f1-06e8-493e-83a4-30ff51386132"), false, "Art Education", "X9000" },
                    { new Guid("ef89f5fc-4b03-4d29-95cc-91b7d2c5245d"), true, "landscape architecture and design", "100124" },
                    { new Guid("f047ad8b-5a1d-4593-b7c1-2fd5f956d4de"), true, "social theory", "100628" },
                    { new Guid("f07c735b-5cf5-48b8-94a9-c08f66888996"), true, "business computing", "100360" },
                    { new Guid("f0cb3e5b-2e19-4831-bb17-fcf0e823e7f9"), false, "Outdoor Education Studies", "X9013" },
                    { new Guid("f0df8572-edcc-498b-9d70-f8572d9c38dd"), true, "cinematography", "100716" },
                    { new Guid("f0e90200-11d9-4d22-a754-c698193f6ef8"), true, "zoology", "100356" },
                    { new Guid("f0eab488-e363-4323-995f-66f0326a2268"), false, "Three Dimensional Design", "W2408" },
                    { new Guid("f1116e13-0874-4326-816a-511fd77054f0"), true, "criminal law", "100685" },
                    { new Guid("f11b4e59-1fe5-4456-9b40-4184719df21d"), true, "exploration geophysics", "101084" },
                    { new Guid("f124603a-2278-4ccf-a4a6-461226d5657d"), false, "Pedology", "D9002" },
                    { new Guid("f135879a-5ca7-49e8-bc17-2433ce6278ff"), true, "development in the Americas", "101359" },
                    { new Guid("f143c67f-be3b-42c6-ac46-7df141adb5c7"), true, "insurance", "100830" },
                    { new Guid("f19a9050-00c4-4367-a447-70f927c6f0c6"), false, "Practical Theology", "V8006" },
                    { new Guid("f1b68d8d-2655-4bd3-a52e-2dc688c4db9e"), true, "textile design", "100051" },
                    { new Guid("f1cc0730-3a1e-4ea3-9237-90ed6a4ebc82"), true, "Brontës studies", "101473" },
                    { new Guid("f1efb698-4cd8-4664-af7f-c0d06fef4779"), false, "Greek Studies", "V1008" },
                    { new Guid("f205833e-389b-4f4d-8aed-0417fa4004d4"), true, "visual communication", "100632" },
                    { new Guid("f2065003-0b31-4e8a-aea3-851af15e0f1d"), true, "leather technology", "100210" },
                    { new Guid("f223dd10-8ef7-4278-bbe3-dc6cd99986a7"), true, "carpentry and joinery", "101505" },
                    { new Guid("f23449d0-38c9-4c35-8af9-c8543976aed3"), true, "quality management", "100213" },
                    { new Guid("f23cb04d-8a5e-4b98-a620-ed37367e2d88"), true, "systems analysis and design", "100753" },
                    { new Guid("f24805ed-2f25-43b7-8c96-12cbd465df94"), true, "marine technology", "100194" },
                    { new Guid("f25fc247-0efa-46e3-8e70-14d5b6192322"), false, "Physics With Technology", "F6007" },
                    { new Guid("f26da82b-6236-4f3e-9812-0d4a0bdaaec5"), true, "human resource management", "100085" },
                    { new Guid("f28e0b68-7eff-4685-8e6f-d66bf242aa37"), true, "childhood and youth studies", "100455" },
                    { new Guid("f2d770c9-5854-474d-a240-5b3f633b82bb"), true, "maintenance engineering", "100193" },
                    { new Guid("f302411a-8f80-4b2c-9450-c29c0e69accb"), true, "USA history", "100768" },
                    { new Guid("f305d9a6-04af-40f5-becd-7bc5cab5bc4f"), true, "property valuation and auctioneering", "100825" },
                    { new Guid("f318d464-ee13-4b24-b3a1-c8c382ac6a6f"), true, "Chinese studies", "101164" },
                    { new Guid("f329b47b-45ed-460f-8620-3bf30916e7ae"), false, "Horticulture", "D2500" },
                    { new Guid("f331ad21-e6b4-4925-a72d-300daf3e3f7e"), false, "Speech & Drama", "W4600" },
                    { new Guid("f3572d73-a2e8-4b7e-beb2-f1e2d1b59c69"), true, "immunology", "100911" },
                    { new Guid("f358a7ba-aade-4840-9e74-b1bc5753f570"), true, "ancient Middle Eastern languages", "101112" },
                    { new Guid("f35b6b01-4b43-481c-844d-c99d69df45f6"), false, "Craft & Technology", "W2405" },
                    { new Guid("f3673e45-c78c-4e1b-856e-9286efff456d"), false, "Graphics", "W210" },
                    { new Guid("f36b3d42-2f37-46dc-8f34-ac8e43cb83eb"), true, "nutrition", "100247" },
                    { new Guid("f3709dba-1031-42a7-939c-d8122b184c0d"), true, "agricultural irrigation and drainage", "101385" },
                    { new Guid("f385e867-e3fc-4bb9-9165-90fceb4e5c10"), true, "medical physics", "100419" },
                    { new Guid("f3921185-d2e0-4187-ab02-a80dbe5eb010"), true, "instrumental or vocal performance", "100639" },
                    { new Guid("f394e7c3-8cf4-4069-aaa1-2bc7cb4a0feb"), false, "Expressive Arts (Art & Design)", "W2006" },
                    { new Guid("f395edab-b225-4a69-a63e-2d323c8fc233"), false, "Science (Unspecified)", "F9603" },
                    { new Guid("f39b4fd5-6b5a-41f9-a376-c5b382def9a7"), false, "Drama and Spoken Language", "W4011" },
                    { new Guid("f39c191d-310f-47f5-9096-da9d404cdc01"), true, "pharmaceutical chemistry", "100423" },
                    { new Guid("f3a072d3-28f6-420a-8bfe-dd28194323ad"), true, "crime scene investigation", "101222" },
                    { new Guid("f3a70137-9dc2-427f-a3a8-8f303db6d799"), true, "victimology", "101405" },
                    { new Guid("f3b3f9f5-2d14-44a0-bd56-b797df3dd77c"), false, "Spanish", "R400" },
                    { new Guid("f3d33379-798b-4805-bd47-dfdcd21b4876"), true, "risk management", "101040" },
                    { new Guid("f3e91599-2a2e-4f81-b4e0-9098a1ce8ec7"), true, "Design and technology", "999003" },
                    { new Guid("f415b417-eb73-40dc-94a8-c6e6db1b3041"), true, "agricultural botany", "101025" },
                    { new Guid("f4261555-e189-4145-a386-d1b48bed1dce"), true, "animal behaviour", "100522" },
                    { new Guid("f42b34b9-e6e6-4da5-aadf-c7f21aa91b2f"), true, "electronic music", "100867" },
                    { new Guid("f436098c-d507-42e7-9e47-2125eb1e232f"), true, "land management", "100819" },
                    { new Guid("f4508692-a6fb-437f-a5f2-b369f4efc55f"), true, "dance performance", "100712" },
                    { new Guid("f495f25d-39c5-49fe-8382-c81ea52416d8"), false, "Movement Studies", "W4504" },
                    { new Guid("f4a62944-3672-4ea8-8a1a-e4a88079b03e"), true, "biology", "100346" },
                    { new Guid("f4b2b856-79db-4391-bef6-2c2c20064db9"), false, "Tech (Design & Info. Tech)", "W9901" },
                    { new Guid("f4c8804d-3cdd-4467-9cc3-c33e319579f4"), true, "jazz", "100843" },
                    { new Guid("f4e86f46-52fb-4029-9ea7-8f757ecb9e89"), true, "emergency and disaster technologies", "100186" },
                    { new Guid("f4e9f12b-7a7c-486a-bf1f-d9bfbcaf55c9"), true, "mental health nursing", "100287" },
                    { new Guid("f4fb8e30-75a5-478f-8562-99794366a5dd"), true, "heritage studies", "100805" },
                    { new Guid("f508fd2e-eea1-4360-ad42-3c5a29b8eb0c"), false, "Information Tech'ogy:computing", "G5604" },
                    { new Guid("f514142e-d9be-40ef-a2af-76377125c9d2"), true, "occupational psychology", "100950" },
                    { new Guid("f51e3ba8-7844-40c3-bbd5-8d17ddf439f0"), false, "Manufacturing and Product Design", "H790" },
                    { new Guid("f549136d-72e5-4597-a594-5449672c8dde"), true, "Turkish languages", "101431" },
                    { new Guid("f54b936d-a93d-46d2-9458-d7ce381707a0"), false, "Ed Of The Deaf & Part. Hearing", "X6003" },
                    { new Guid("f57675b4-6f0e-4de2-94eb-d1754d67aa59"), true, "Hausa language", "101367" },
                    { new Guid("f593283c-66d2-48dc-8e13-7f0fc76c2686"), true, "orthopaedics", "101324" },
                    { new Guid("f5a66fd5-5488-4462-8eee-6330640703d9"), false, "Marketing", "N5000" },
                    { new Guid("f5b2282e-e887-49d8-a9b8-38a030e662eb"), true, "silversmithing and goldsmithing", "100725" },
                    { new Guid("f5bd64ad-7941-47da-aed7-10470d49670c"), false, "Needlecraft", "W9005" },
                    { new Guid("f604d521-76cb-4bf5-bc6f-1c828a7ad4bd"), true, "oncology", "101327" },
                    { new Guid("f6107b8a-b2c4-43e7-9212-5bf7913bb590"), true, "Robert Burns studies", "101490" },
                    { new Guid("f629e740-71c4-4ea7-a0eb-3aa01dd911f3"), true, "business studies", "100079" },
                    { new Guid("f6462c63-b140-44ff-a1fb-ab94c55d1da5"), true, "fire safety engineering", "100183" },
                    { new Guid("f6ec2749-41f2-4d85-a3ea-c5f0fb877a49"), true, "special needs teaching", "101087" },
                    { new Guid("f7033e45-b877-4cc9-ab81-00be6d83f568"), true, "hinduism", "101444" },
                    { new Guid("f7168e43-3705-41a1-b27b-182b876f5e48"), true, "Dutch studies", "101161" },
                    { new Guid("f71d2954-affe-4a39-b3da-f9b50ab58af6"), false, "C D T With Computer Science", "W9909" },
                    { new Guid("f748042f-1d33-4f5c-b0eb-bec5f94f0925"), true, "classical church Greek", "101422" },
                    { new Guid("f797e941-4ac2-466d-b08d-1ebad9b7d178"), false, "Drama", "W400" },
                    { new Guid("f7b525ce-ee6f-422d-9005-6f686c4c83d2"), false, "Remedial Education", "X6400" },
                    { new Guid("f81ff0e0-3f39-48a8-bb70-6f6cefac57d8"), true, "health and welfare", "100653" },
                    { new Guid("f82e97f6-b2e8-427a-a18c-81e781cc03b3"), true, "composite materials", "101217" },
                    { new Guid("f8415194-0d45-48cd-b194-bc950f02ee6b"), true, "liberal arts", "100065" },
                    { new Guid("f896d8e4-7cb0-467f-b6ad-62daf8a45379"), true, "bioengineering", "101243" },
                    { new Guid("f89a7c97-f5a1-4fc6-af19-9e76b9cabfa1"), true, "educational psychology", "100496" },
                    { new Guid("f8c14f34-6d49-4a63-b5c6-589a10f349f9"), true, "conservation of buildings", "100585" },
                    { new Guid("f8db3950-de0e-44bc-9ce6-2dd27a17ceb8"), true, "marine chemistry", "101046" },
                    { new Guid("f90c044f-e0f7-4d74-adf0-d49a67d20e98"), false, "English, Drama, Media Studies", "Q9715" },
                    { new Guid("f94d3fe0-7619-4c5f-9b44-3bd3a25b3d2c"), true, "epidemiology", "101335" },
                    { new Guid("f9d879dc-6308-4c4e-9d45-7bed52ba8b7f"), true, "crafts", "100895" },
                    { new Guid("f9da902c-5e21-475e-9247-d740ada178fc"), true, "hair and beauty sciences", "101373" },
                    { new Guid("f9fcb607-1012-4a2c-9c14-2542bbc435e2"), false, "Creative Arts", "W9001" },
                    { new Guid("fa2bbcc3-4020-4518-82e1-fea9b664e810"), false, "Art and Music", "W2800" },
                    { new Guid("fa51c1b0-dc30-42c5-8213-9ddaeaff3e95"), true, "hydrology", "101079" },
                    { new Guid("fa58d436-c996-4f44-b4ec-4af1c580ed8d"), true, "sports studies", "100098" },
                    { new Guid("fa645de7-8767-49c5-9fd2-5a393c914755"), false, "Welsh Studies", "Q5204" },
                    { new Guid("fa7dda66-691e-455f-9cee-e269daa4b726"), true, "water resource management", "100986" },
                    { new Guid("fa804c1c-7c4d-443f-8e3c-abf263c6c62a"), true, "animal management", "100518" },
                    { new Guid("fae7e7d0-5a8a-409d-979c-869dec518f0a"), true, "higher education teaching", "100509" },
                    { new Guid("fae959a2-b028-4ba8-b05c-754e25798b2b"), true, "film studies", "100058" },
                    { new Guid("fb0e95e8-6c93-4700-a755-1f46b95b1e72"), false, "Social Policy", "L4200" },
                    { new Guid("fb1419d5-bac9-462f-9317-56e41239d5f0"), false, "Technology", "J9001" },
                    { new Guid("fb2a1c5c-94c5-4695-a4da-1a5adb8d2420"), false, "Institutional Management", "N7000" },
                    { new Guid("fb609ef8-b33a-4cf1-897e-95b3b512e51a"), true, "religious studies", "100339" },
                    { new Guid("fb69e684-ca64-4d1f-941c-198c896afe14"), true, "Latin literature", "101125" },
                    { new Guid("fba00902-86ed-4574-ac73-dbae282c67e6"), true, "early childhood studies", "100457" },
                    { new Guid("fbb5e2fe-b11b-46cf-a173-1e28ba95a9ec"), true, "Ruskin studies", "101467" },
                    { new Guid("fbeb8e3d-df5e-4620-8d22-68606f8fbce2"), true, "computational physics", "101071" },
                    { new Guid("fbee8811-9c44-4e0a-8f6b-d324cc337bb0"), false, "Welsh and Other Celtic Lang", "Z0046" },
                    { new Guid("fc044c98-d459-4f9e-98e6-9d1bcad5546d"), true, "reflexology", "100239" },
                    { new Guid("fc515e31-5e08-40f6-bf72-3aefbb5419d6"), false, "Construction and the Built Environment", "K290" },
                    { new Guid("fc75352d-d09e-4a31-9e2c-46681cb151e7"), true, "Czech studies", "101312" },
                    { new Guid("fc930191-3a70-4ebc-8364-b31d72960634"), false, "French With Italian", "R8202" },
                    { new Guid("fca2b0af-5ca1-48ee-829b-df31bb53bd5d"), false, "Social Sciences/Social Studies", "L900" },
                    { new Guid("fcc67ce9-d2f7-4ee9-bce4-b12f3a3b175c"), false, "Design and Craft", "W9902" },
                    { new Guid("fceb10d4-2df2-420b-bc6d-03bbbd0fa971"), true, "physical and biological anthropology", "100663" },
                    { new Guid("fd090339-2970-4c3d-bcd0-3e3f3d02c95e"), false, "Human Studies", "L3401" },
                    { new Guid("fd1477b8-5152-4800-a9ff-2dc1a0ead5bb"), true, "obstetrics and gynaecology", "101309" },
                    { new Guid("fd14d677-32d3-491e-8e9c-7d0860d3b00b"), true, "ergonomics", "100052" },
                    { new Guid("fd1b4b8b-ca2b-44d4-aa12-7f54db8cb24d"), true, "biodiversity conservation", "101318" },
                    { new Guid("fd44ffd8-b003-4fd1-ad70-b1dd58691efe"), true, "English law", "100676" },
                    { new Guid("fd45a0ac-3a7d-46c9-baf7-b1789e371c48"), true, "history of science", "100307" },
                    { new Guid("fd48f74f-41a6-460a-8334-de1ba115de4a"), true, "microbiology", "100353" },
                    { new Guid("fd64b17f-adb0-4377-b525-bfb4d1aff8cc"), false, "Cinema & Film Studio Work", "W5300" },
                    { new Guid("fd7f0d93-3fa3-4acd-938e-e182e8722909"), true, "mechatronics and robotics", "100170" },
                    { new Guid("fd7f7073-488a-4816-a1d4-336928ac62b7"), false, "Biophysical Science", "C6001" },
                    { new Guid("fe0ee47e-f87b-4651-9b05-5b3611fc9b40"), true, "surface decoration", "100728" },
                    { new Guid("fe84424c-f233-414b-a0f3-4d22c72c02fa"), true, "reproductive biology", "100847" },
                    { new Guid("febe507d-ebbd-4b75-a401-442d42c3f54f"), false, "Ethics", "V7603" },
                    { new Guid("feee2354-fb8a-442a-ac5d-9bc35582ce64"), false, "Computing", "G5003" },
                    { new Guid("ff3f23f1-d237-4028-8723-29d7c1e8f489"), true, "English literature 1200 - 1700", "101094" },
                    { new Guid("ff481306-7d5b-4122-b46b-c6fee34abc43"), true, "property law", "100689" },
                    { new Guid("ff8b2f4d-fa71-48d5-a648-3e1474aabfb2"), true, "cinematics", "101214" },
                    { new Guid("ffb6663f-7034-4592-a149-adf0d5d49ee5"), true, "fabrication", "100211" }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "user_id", "active", "api_roles", "client_id", "client_secret", "is_oidc_client", "name", "one_login_authentication_scheme_name", "one_login_client_id", "one_login_post_logout_redirect_uri_path", "one_login_private_key_pem", "one_login_redirect_uri_path", "post_logout_redirect_uris", "redirect_uris", "short_name", "user_type" },
                values: new object[] { new Guid("0f18f1ec-a102-4023-843f-1cadef3e6e14"), true, null, null, null, false, "NPQ", null, null, null, null, null, null, null, null, 2 });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "user_id", "active", "name", "user_type" },
                values: new object[] { new Guid("a81394d1-a498-46d8-af3e-e077596ab303"), true, "System", 3 });

            migrationBuilder.InsertData(
                table: "alert_types",
                columns: new[] { "alert_type_id", "alert_category_id", "display_order", "dqt_sanction_code", "internal_only", "is_active", "name" },
                values: new object[,]
                {
                    { new Guid("0740f9eb-ece3-4394-a230-453da224d337"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), null, "A16", true, false, "No Sanction - conviction for a relevant offence" },
                    { new Guid("0ae8d4b6-ec9b-47ca-9338-6dae9192afe5"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), null, "A4", true, false, "Reprimand - unacceptable professional conduct" },
                    { new Guid("12435c00-88cb-406b-b2b8-7400c1ced7b8"), new Guid("768c9eb4-355b-4491-bb20-67eb59a97579"), 2, "T10", true, true, "FOR INTERNAL USER ONLY – known duplicate record" },
                    { new Guid("17b4fe26-7468-4702-92e5-785b861cf0fa"), new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), null, "A24", false, false, "Suspension order - with conditions - (arising from breach of previous condition(s))" },
                    { new Guid("18e04dcb-fb86-4b05-8d5d-ff9c5da738dd"), new Guid("cbf7633f-3904-407d-8371-42a473fa641f"), null, "B2A", true, false, "Restricted by the Secretary of State - Permitted to work as teacher" },
                    { new Guid("1a2b06ae-7e9f-4761-b95d-397ca5da4b13"), new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), null, "A13", false, false, "Suspension order - unacceptable professional conduct - with conditions" },
                    { new Guid("1ebd1620-293d-4169-ba78-0b41a6413ad9"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), null, "A7", false, false, "Conditional Registration Order - serious professional incompetence" },
                    { new Guid("241eeb78-fac7-4c77-8059-c12e93dc2fae"), new Guid("38df5a00-94ab-486f-8905-d5b2eac04000"), 1, "T7", false, true, "Section 128 barring direction" },
                    { new Guid("2c496e3f-00d3-4f0d-81f3-21458fe707b3"), new Guid("b2b19019-b165-47a3-8745-3297ff152581"), null, "G2", true, false, "Formerly barred by the Independent Safeguarding Authority" },
                    { new Guid("2ca98658-1d5b-49d5-b05f-cc08c8b8502c"), new Guid("ee78d44d-abf8-44a9-b22b-87a821f8d3c9"), 1, "T8", true, true, "Teacher sanctioned in other EEA member state" },
                    { new Guid("33e00e46-6513-4136-adfd-1352cf34d8ec"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), null, "A22", true, false, "No Sanction - breach of condition(s)" },
                    { new Guid("3499860a-a0fb-43e3-878e-c226d14150b0"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), null, "A3", false, false, "Conditional Registration Order - unacceptable professional conduct" },
                    { new Guid("38db7946-2dbf-408e-bc48-1625829e7dfe"), new Guid("cbf7633f-3904-407d-8371-42a473fa641f"), null, "B2B", true, false, "Restricted by the Secretary of State - Not Permitted to work as teacher" },
                    { new Guid("3c5fc83b-10e1-4a15-83e6-794fce3e0b45"), new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), null, "A23", false, false, "Suspension order - without conditions - (arising from breach of previous condition(s))" },
                    { new Guid("3f7de5fd-05a8-404f-a97c-428f54e81322"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), null, "A8", true, false, "Reprimand - serious professional incompetence" },
                    { new Guid("40794ea8-eda2-40a8-a26a-5f447aae6c99"), new Guid("e8a9ee91-bf7f-4f70-bc66-a644d522384e"), 1, "G1", true, true, "A possible matching record was found. Please contact the DBS before employing this person" },
                    { new Guid("50508749-7a6b-4175-8538-9a1e55692efd"), new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), null, "A14", false, false, "Suspension order - serious professional incompetence - with conditions" },
                    { new Guid("50feafbc-5124-4189-b06c-6463c7ebb8a8"), new Guid("b2b19019-b165-47a3-8745-3297ff152581"), 3, "T3", false, true, "Prohibition by the Secretary of State - deregistered by GTC Scotland" },
                    { new Guid("552ee226-a3a9-4dc3-8d04-0b7e4f641b51"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), null, "A15", true, false, "For internal information only - historic GTC finding of unsuitable for registration" },
                    { new Guid("5aa21b8f-2069-43c9-8afd-05b34b02505f"), new Guid("b2b19019-b165-47a3-8745-3297ff152581"), 4, "T5", false, true, "Prohibition by the Secretary of State - refer to GTC Northern Ireland" },
                    { new Guid("5ea8bb68-4774-4ad8-b635-213a0cdda4c3"), new Guid("0ae0707b-1503-477d-bc0f-1505ed95dbdf"), null, "C3", false, false, "Restricted by the Secretary of State - failed probation - permitted to carry out specified work for a period equal in length to a statutory induction period only" },
                    { new Guid("651e1f56-3135-4961-bd7e-3f7b2c75cb04"), new Guid("0ae0707b-1503-477d-bc0f-1505ed95dbdf"), null, "C1", false, false, "Prohibited by the Secretary of State - failed probation" },
                    { new Guid("72e48b6a-e781-4bf3-910b-91f2d28f2eaa"), new Guid("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"), null, "A21B", false, false, "Prohibition Order - conviction of a relevant offence - eligible to reapply after specified time" },
                    { new Guid("78f88de2-9ec1-41b8-948a-33bdff223206"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), null, "A11", true, false, "No Sanction - unacceptable professional conduct" },
                    { new Guid("7924fe90-483c-49f8-84fc-674feddba848"), new Guid("227b75e5-bb98-496c-8860-1baea37aa5c6"), 1, "T6", false, true, "Secretary of State decision - no prohibition" },
                    { new Guid("872d7700-aa6f-435e-b5f9-821fb087962a"), new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), null, "A2", false, false, "Suspension order - unacceptable professional conduct - without conditions" },
                    { new Guid("8ef92c14-4b1f-4530-9189-779ad9f3cefd"), new Guid("b2b19019-b165-47a3-8745-3297ff152581"), null, "B3", false, false, "Prohibited by an Independent Schools Tribunal or Secretary of State" },
                    { new Guid("950d3eed-bef5-448a-b0f0-bf9c54f2103b"), new Guid("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"), null, "A21A", false, false, "Prohibition Order - conviction of a relevant offence - ineligible to reapply" },
                    { new Guid("993daa42-96cb-4621-bd9e-d4b195076bbe"), new Guid("b2b19019-b165-47a3-8745-3297ff152581"), null, "B6", true, false, "Formerly on List 99" },
                    { new Guid("9fafaa80-f9f8-44a0-b7b3-cffedcbe0298"), new Guid("0ae0707b-1503-477d-bc0f-1505ed95dbdf"), 1, "C2", false, true, "Failed induction" },
                    { new Guid("a414283f-7d5b-4587-83bf-f6da8c05b8d5"), new Guid("b2b19019-b165-47a3-8745-3297ff152581"), 1, "T2", false, true, "Interim prohibition by the Secretary of State" },
                    { new Guid("a5bd4352-2cec-4417-87a1-4b6b79d033c2"), new Guid("b2b19019-b165-47a3-8745-3297ff152581"), 5, "T4", false, true, "Prohibition by the Secretary of State - refer to the Education Workforce Council, Wales" },
                    { new Guid("a6f51ccc-a19c-4dc2-ba80-ffb7a95ff2ee"), new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), null, "A6", false, false, "Suspension order - serious professional incompetence - without conditions" },
                    { new Guid("a6fc9f2e-8923-4163-978e-93bd901d146f"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), null, "A18", false, false, "Conditional Registration Order - conviction of a relevant offence" },
                    { new Guid("ae3e385d-03f8-4f12-9ce2-006afe827d23"), new Guid("768c9eb4-355b-4491-bb20-67eb59a97579"), 1, "T9", true, true, "FOR INTERNAL INFORMATION ONLY - see alert details" },
                    { new Guid("af65c236-47a6-427b-8e4b-930de6d256f0"), new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), null, "A19", false, false, "Suspension order - conviction of a relevant offence - without conditions" },
                    { new Guid("b6c8d8f1-723e-49a5-9551-25805e3e29b9"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), null, "A12", true, false, "No Sanction - serious professional incompetence" },
                    { new Guid("c02bdc3a-7a19-4034-aa23-3a23c54e1d34"), new Guid("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"), null, "A5A", false, false, "Prohibition Order - serious professional incompetence - Ineligible to reapply" },
                    { new Guid("cac68337-3f95-4475-97cf-1381e6b74700"), new Guid("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"), null, "A5B", false, false, "Prohibition Order - serious professional incompetence - Eligible to reapply after specified time" },
                    { new Guid("d372fcfa-1c4a-4fed-84c8-4c7885575681"), new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), null, "A20", false, false, "Suspension order - conviction of a relevant offence - with conditions" },
                    { new Guid("e3658a61-bee2-4df1-9a26-e010681ee310"), new Guid("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"), null, "A1B", false, false, "Prohibition Order - unacceptable professional conduct - Eligible to reapply after specified time" },
                    { new Guid("eab8b66d-68d0-4cb9-8e4d-bbd245648fb6"), new Guid("b2b19019-b165-47a3-8745-3297ff152581"), null, "B1", true, false, "Barring by the Secretary of State" },
                    { new Guid("ed0cd700-3fb2-4db0-9403-ba57126090ed"), new Guid("b2b19019-b165-47a3-8745-3297ff152581"), 2, "T1", false, true, "Prohibition by the Secretary of State - misconduct" },
                    { new Guid("fa6bd220-61b0-41fc-9066-421b3b9d7885"), new Guid("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"), null, "A1A", false, false, "Prohibition Order - unacceptable professional conduct - Ineligible to reapply" },
                    { new Guid("fcff87d6-88f5-4fc5-ac81-5350b4fdd9e1"), new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), null, "A17", true, false, "Reprimand - conviction of a relevant offence" }
                });

            migrationBuilder.InsertData(
                table: "route_to_professional_status_types",
                columns: new[] { "route_to_professional_status_type_id", "degree_type_required", "holds_from_required", "induction_exemption_reason_id", "induction_exemption_required", "is_active", "name", "professional_status_type", "training_age_specialism_type_required", "training_country_required", "training_end_date_required", "training_provider_required", "training_start_date_required", "training_subjects_required" },
                values: new object[,]
                {
                    { new Guid("3604ef30-8f11-4494-8b52-a2f9c5371e03"), 2, 1, new Guid("3471ab35-e6e4-4fa9-a72b-b8bd113df591"), 1, true, "Northern Irish Recognition", 0, 0, 1, 2, 2, 2, 0 },
                    { new Guid("52835b1f-1f2e-4665-abc6-7fb1ef0a80bb"), 2, 1, new Guid("a112e691-1694-46a7-8f33-5ec5b845c181"), 1, true, "Scottish Recognition", 0, 0, 1, 2, 2, 2, 0 },
                    { new Guid("6f27bdeb-d00a-4ef9-b0ea-26498ce64713"), 2, 1, new Guid("4c97e211-10d2-4c63-8da9-b0fcebe7f2f9"), 1, true, "Apply for Qualified Teacher Status in England", 0, 0, 1, 2, 2, 2, 0 },
                    { new Guid("be6eaf8c-92dd-4eff-aad3-1c89c4bec18c"), 2, 1, new Guid("35caa6a3-49f2-4a63-bd5a-2ba5fa9dc5db"), 1, true, "QTLS and SET Membership", 0, 0, 0, 2, 0, 2, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_alert_categories_display_order",
                table: "alert_categories",
                column: "display_order",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_alert_types_alert_category_id",
                table: "alert_types",
                column: "alert_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_alert_types_display_order",
                table: "alert_types",
                columns: new[] { "alert_category_id", "display_order" },
                unique: true,
                filter: "display_order is not null and is_active = true");

            migrationBuilder.CreateIndex(
                name: "ix_alerts_alert_type_id",
                table: "alerts",
                column: "alert_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_alerts_person_id",
                table: "alerts",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_api_keys_application_user_id",
                table: "api_keys",
                column: "application_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_api_keys_key",
                table: "api_keys",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_establishment_establishment_source_id",
                table: "establishments",
                column: "establishment_source_id");

            migrationBuilder.CreateIndex(
                name: "ix_establishment_la_code_establishment_number",
                table: "establishments",
                columns: new[] { "la_code", "establishment_number" });

            migrationBuilder.CreateIndex(
                name: "ix_establishment_urn",
                table: "establishments",
                column: "urn");

            migrationBuilder.CreateIndex(
                name: "ix_events_event_name_created",
                table: "events",
                columns: new[] { "event_name", "created" })
                .Annotation("Npgsql:CreatedConcurrently", true);

            migrationBuilder.CreateIndex(
                name: "ix_events_person_id_event_name",
                table: "events",
                columns: new[] { "person_id", "event_name" },
                filter: "person_id is not null");

            migrationBuilder.CreateIndex(
                name: "ix_events_person_ids",
                table: "events",
                columns: new[] { "person_ids", "event_name" })
                .Annotation("Npgsql:CreatedConcurrently", true)
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_eyts_awarded_emails_job_items_personalization",
                table: "eyts_awarded_emails_job_items",
                column: "personalization")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_eyts_awarded_emails_job_items_trn",
                table: "eyts_awarded_emails_job_items",
                column: "trn");

            migrationBuilder.CreateIndex(
                name: "ix_eyts_awarded_emails_jobs_executed_utc",
                table: "eyts_awarded_emails_jobs",
                column: "executed_utc");

            migrationBuilder.CreateIndex(
                name: "ix_induction_completed_emails_job_items_personalization",
                table: "induction_completed_emails_job_items",
                column: "personalization")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_induction_completed_emails_job_items_trn",
                table: "induction_completed_emails_job_items",
                column: "trn");

            migrationBuilder.CreateIndex(
                name: "ix_induction_completed_emails_jobs_executed_utc",
                table: "induction_completed_emails_jobs",
                column: "executed_utc");

            migrationBuilder.CreateIndex(
                name: "ix_international_qts_awarded_emails_job_items_personalization",
                table: "international_qts_awarded_emails_job_items",
                column: "personalization")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_international_qts_awarded_emails_job_items_trn",
                table: "international_qts_awarded_emails_job_items",
                column: "trn");

            migrationBuilder.CreateIndex(
                name: "ix_international_qts_awarded_emails_jobs_executed_utc",
                table: "international_qts_awarded_emails_jobs",
                column: "executed_utc");

            migrationBuilder.CreateIndex(
                name: "ix_name_synonyms_name",
                table: "name_synonyms",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notes_person_id",
                table: "notes",
                column: "person_id")
                .Annotation("Npgsql:CreatedConcurrently", true);

            migrationBuilder.CreateIndex(
                name: "ix_oidc_applications_client_id",
                table: "oidc_applications",
                column: "client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_oidc_authorizations_application_id_status_subject_type",
                table: "oidc_authorizations",
                columns: new[] { "application_id", "status", "subject", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_oidc_scopes_name",
                table: "oidc_scopes",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_oidc_tokens_application_id_status_subject_type",
                table: "oidc_tokens",
                columns: new[] { "application_id", "status", "subject", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_oidc_tokens_reference_id",
                table: "oidc_tokens",
                column: "reference_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_person_search_attributes_attribute_type_and_value",
                table: "person_search_attributes",
                columns: new[] { "attribute_type", "attribute_value" });

            migrationBuilder.CreateIndex(
                name: "ix_person_search_attributes_person_id",
                table: "person_search_attributes",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_persons_dqt_contact_id",
                table: "persons",
                column: "dqt_contact_id",
                unique: true,
                filter: "dqt_contact_id is not null");

            migrationBuilder.CreateIndex(
                name: "ix_persons_merged_with_person_id",
                table: "persons",
                column: "merged_with_person_id",
                filter: "merged_with_person_id is not null");

            migrationBuilder.CreateIndex(
                name: "ix_persons_trn",
                table: "persons",
                column: "trn",
                unique: true,
                filter: "trn is not null");

            migrationBuilder.CreateIndex(
                name: "ix_persons_trn_date_of_birth_email_address_names_last_names_na",
                table: "persons",
                columns: new[] { "trn", "date_of_birth", "email_address", "names", "last_names", "national_insurance_numbers" })
                .Annotation("Npgsql:CreatedConcurrently", true)
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Relational:Collation", new[] { "case_insensitive" });

            migrationBuilder.CreateIndex(
                name: "ix_previous_names_person_id",
                table: "previous_names",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_process_events_person_ids",
                table: "process_events",
                column: "person_ids")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "ix_processes_person_ids",
                table: "processes",
                column: "person_ids")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "ix_processes_process_type",
                table: "processes",
                column: "process_type");

            migrationBuilder.CreateIndex(
                name: "ix_qts_awarded_emails_job_items_personalization",
                table: "qts_awarded_emails_job_items",
                column: "personalization")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_qts_awarded_emails_job_items_trn",
                table: "qts_awarded_emails_job_items",
                column: "trn");

            migrationBuilder.CreateIndex(
                name: "ix_qts_awarded_emails_jobs_executed_utc",
                table: "qts_awarded_emails_jobs",
                column: "executed_utc");

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

            migrationBuilder.CreateIndex(
                name: "ix_qualifications_person_id_source_application_user_id_source_",
                table: "qualifications",
                columns: new[] { "person_id", "source_application_user_id", "source_application_reference" },
                unique: true,
                filter: "source_application_user_id is not null and source_application_reference is not null");

            migrationBuilder.CreateIndex(
                name: "ix_route_migration_report_items_person_id",
                table: "route_migration_report_items",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_route_to_professional_status_induction_exemption_reason_id",
                table: "route_to_professional_status_types",
                column: "induction_exemption_reason_id");

            migrationBuilder.CreateIndex(
                name: "ix_support_tasks_one_login_user_subject",
                table: "support_tasks",
                column: "one_login_user_subject");

            migrationBuilder.CreateIndex(
                name: "ix_support_tasks_person_id",
                table: "support_tasks",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_tps_csv_extract_items_key",
                table: "tps_csv_extract_items",
                column: "key");

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

            migrationBuilder.CreateIndex(
                name: "ix_tps_establishments_la_code_establishment_number",
                table: "tps_establishments",
                columns: new[] { "la_code", "establishment_code" });

            migrationBuilder.CreateIndex(
                name: "ix_training_provider_ukprn",
                table: "training_providers",
                column: "ukprn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trn_ranges_unexhausted_trn_ranges",
                table: "trn_ranges",
                column: "from_trn",
                filter: "is_exhausted IS FALSE");

            migrationBuilder.CreateIndex(
                name: "ix_trn_request_metadata_email_address",
                table: "trn_request_metadata",
                column: "email_address");

            migrationBuilder.CreateIndex(
                name: "ix_trn_request_metadata_one_login_user_subject",
                table: "trn_request_metadata",
                column: "one_login_user_subject");

            migrationBuilder.CreateIndex(
                name: "ix_trn_requests_client_id_request_id",
                table: "trn_requests",
                columns: new[] { "client_id", "request_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_azure_ad_user_id",
                table: "users",
                column: "azure_ad_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_client_id",
                table: "users",
                column: "client_id",
                unique: true,
                filter: "client_id is not null");

            migrationBuilder.CreateIndex(
                name: "ix_users_one_login_authentication_scheme_name",
                table: "users",
                column: "one_login_authentication_scheme_name",
                unique: true,
                filter: "one_login_authentication_scheme_name is not null");

            migrationBuilder.CreateIndex(
                name: "ix_webhook_messages_next_delivery_attempt",
                table: "webhook_messages",
                column: "next_delivery_attempt",
                filter: "next_delivery_attempt is not null");

            migrationBuilder.InsertData(
                table: "establishment_sources",
                columns: new[] { "establishment_source_id", "name" },
                values: new object[,]
                {
                    { 1, "GIAS" },
                    { 2, "TPS" }
                });

            migrationBuilder.InsertData(
                table: "tps_establishment_types",
                columns: new[] { "tps_establishment_type_id", "description", "establishment_range_from", "establishment_range_to", "short_description" },
                values: new object[,]
                {
                    { 1, "Homes set up under the Children and Young Persons Act (e.g. Community Homes)", "0001", "0099", "Homes set up under the Children and Young Persons Act (e.g. Community Homes)" },
                    { 2, "Homes set up under the Children and Young Persons Act (e.g. Community Homes)", "0200", "0399", "Homes set up under the Children and Young Persons Act (e.g. Community Homes)" },
                    { 3, "Training and occupation centres and other DSS establishments (except day nurseries)", "0400", "0524", "Training and occupation centres and other DSS establishments (except day nurseries)" },
                    { 4, "Special Hospitals provided under Part VII of the Mental Health Act 1959", "0525", "0549", "Special Hospitals provided under Part VII of the Mental Health Act 1959" },
                    { 5, "Teachers' Superannuation (Army Civilian Lecturer) Scheme 1951 Schools or schools formerly under that Scheme", "0550", "0574", "Teachers' Superannuation (Army Civilian Lecturer) Scheme 1951 Schools or schools formerly under that Scheme" },
                    { 6, "Education Forum", "0575", "0599", "Education Forum" },
                    { 7, "CAY", "0600", "0600", "CAY" },
                    { 8, "PAY", "0601", "0601", "PAY" },
                    { 9, "DSS day nurseries", "0625", "0625", "DSS day nurseries" },
                    { 10, "Schools and institutions controlled by other government departments", "0626", "0674", "Schools and institutions controlled by other government departments" },
                    { 11, "Employment under voluntary youth organisations", "0675", "0750", "Employment under voluntary youth organisations" },
                    { 12, "Employment under voluntary youth organisations", "0100", "0199", "Employment under voluntary youth organisations" },
                    { 13, "Employment under adult and miscellaneous organisations", "0751", "0939", "Employment under adult and miscellaneous organisations" },
                    { 14, "Playing for Success Centres", "0940", "0949", "Playing for Success Centres" },
                    { 15, "LA nursery schools", "1000", "1099", "LA nursery schools" },
                    { 16, "Pupil referral units", "1100", "1150", "Pupil referral units" },
                    { 17, "Direct-grant nursery schools", "1800", "1899", "Direct-grant nursery schools" },
                    { 18, "Independent nursery education establishment recognised as efficient", "1900", "1999", "Independent nursery education establishment recognised as efficient" },
                    { 19, "Maintained primary schools and schools which have converted to academies", "2000", "3999", "Maintained primary schools and schools which have converted to academies" },
                    { 20, "Maintained secondary schools and schools which have converted to academies", "4000", "4999", "Maintained secondary schools and schools which have converted to academies" },
                    { 21, "Direct-grant schools (recorded up to October 1980)", "5000", "5099", "Direct-grant schools (recorded up to October 1980)" },
                    { 22, "Practical instruction centres (not all such centres have been allocated individual numbers but where a number has already been allocated its use is continued. All new centres are numbered 5199)", "5100", "5198", "Practical instruction centres" },
                    { 23, "Grant-maintained primary/middle deemed primary schools and schools which have converted to academies", "5200", "5299", "Grant-maintained primary/middle deemed primary schools and schools which have converted to academies" },
                    { 24, "Camps, holiday classes etc.", "5300", "5399", "Camps, holiday classes etc." },
                    { 25, "Grant-maintained secondary/middle deemed secondary schools and schools which have converted to academies", "5400", "5499", "Grant-maintained secondary/middle deemed secondary schools and schools which have converted to academies" },
                    { 26, "Immigrant centres", "5500", "5548", "Immigrant centres" },
                    { 27, "Grant-maintained primary, middle and secondary schools (overflow) schools which have converted to academies", "5601", "5899", "Grant-maintained primary, middle and secondary schools (overflow) schools which have converted to academies" },
                    { 28, "Grant-maintained schools (formally Independent) schools which have converted to academies", "5900", "5949", "Grant-maintained schools (formally Independent) schools which have converted to academies" },
                    { 29, "Grant-maintained special schools and schools which have converted to academies", "5950", "5999", "Grant-maintained special schools and schools which have converted to academies" },
                    { 30, "Independent schools", "6000", "6899", "Independent schools" },
                    { 31, "City technology colleges", "6900", "6904", "City technology colleges" },
                    { 32, "City Academies", "6905", "6924", "City Academies" },
                    { 33, "Special schools (except as below)", "7000", "7749", "Special schools" },
                    { 34, "Special schools for nursery age children", "7750", "7798", "Special schools for nursery age children" },
                    { 35, "Boarding homes for handicapped pupils", "7800", "7899", "Boarding homes for handicapped pupils" },
                    { 36, "Establishments for further education and training of disabled persons", "7900", "7999", "Establishments for further education and training of disabled persons" },
                    { 37, "Maintained and assisted major FE establishments (not included below)", "8000", "8149", "Maintained and assisted major FE establishments" },
                    { 38, "Maintained and assisted art establishments", "8150", "8199", "Maintained and assisted art establishments" },
                    { 39, "Direct-grant major FE establishments", "8200", "8219", "Direct-grant major FE establishments" },
                    { 40, "Independent (Efficient-Rules 16) FE establishments", "8220", "8269", "Independent (Efficient-Rules 16) FE establishments" },
                    { 41, "National colleges", "8270", "8284", "National colleges" },
                    { 42, "LA farm institutes", "8300", "8349", "LA farm institutes" },
                    { 43, "LA agricultural centres", "8350", "8389", "LA agricultural centres" },
                    { 44, "Direct-grant and independent agricultural establishments", "8390", "8399", "Direct-grant and independent agricultural establishments" },
                    { 45, "LA youth welfare", "8400", "8499", "LA youth welfare" },
                    { 46, "LA adult welfare", "8500", "8599", "LA adult welfare" },
                    { 47, "Sixth form colleges", "8600", "8699", "Sixth form colleges" },
                    { 48, "Polytechnics/New Style Universities", "8700", "8898", "Polytechnics/New Style Universities" },
                    { 49, "LA colleges of education", "9300", "9599", "LA colleges of education" },
                    { 50, "Voluntary colleges of education", "9600", "9899", "Voluntary colleges of education" },
                    { 51, "Teacher /organiser (employed primarily as a teacher)", "0950", "0950", "Teacher /organiser (employed primarily as a teacher)" },
                    { 52, "Divided service between Primary and Secondary Schools", "0951", "0951", "Divided service between Primary and Secondary Schools" },
                    { 53, "Divided service between Further Education and P & S Schools", "0952", "0952", "Divided service between Further Education and P & S Schools" },
                    { 54, "Adult Miscellaneous Organisation", "0954", "0954", "Adult Miscellaneous Organisation" },
                    { 55, "Divided service between Special Schools", "7799", "7799", "Divided service between Special Schools" },
                    { 56, "Divided service between FE establishments", "8999", "8999", "Divided service between FE establishments" },
                    { 57, "Adult Miscellaneous Organisation (not allocated an Estab No) - ie teacher paid under FE document, employed providing FE or Adult Education (eg Community College)", "0953", "0953", "Adult Miscellaneous Organisation (not allocated an Estab No)" },
                    { 58, "Teacher employed by Ministry of Defence (UK based)", "0955", "0955", "Teacher employed by Ministry of Defence (UK based)" },
                    { 59, "Unattached regular engagement in Primary Schools - ie Permanent 'supply' teacher under contract", "0960", "0960", "Unattached regular engagement in Primary Schools - ie Permanent 'supply' teacher under contract" },
                    { 60, "Unattached regular engagement in Secondary or P & S - ie Permanent 'supply' teacher under contract", "0961", "0961", "Unattached regular engagement in Secondary or P & S - ie Permanent 'supply' teacher under contract" },
                    { 61, "Visiting Teacher Primary - peripatetic teacher (eg specialist subject teacher visiting different schools)", "0962", "0962", "Visiting Teacher Primary - peripatetic teacher (eg specialist subject teacher visiting different schools)" },
                    { 62, "Visiting Teacher Secondary or P & S - peripatetic teacher (eg specialist subject teacher visiting different schools)", "0963", "0963", "Visiting Teacher Secondary or P & S - peripatetic teacher (eg specialist subject teacher visiting different schools)" },
                    { 63, "P & S teaching under Section 56 of Education Act 1944 - ie teaching other than at a school (eg at home or in a hospital, or teachers in penal establishments)", "0964", "0964", "P & S teaching under Section 56 of Education Act 1944 - ie teaching other than at a school" },
                    { 64, "Peripatetic support wholly for SEN or disabled not in a special school.", "0965", "0965", "Peripatetic support wholly for SEN or disabled not in a special school." },
                    { 65, "School supply teacher - whose contract is terminable without notice. Teacher who is employed temporarily in place of a regularly employed teacher. Teacher has made a part-time election.", "0966", "0966", "School supply teacher - whose contract is terminable without notice" },
                    { 66, "Full-time Organiser - employment involves the performance of duties in connection with the provision of education or service ancillary to education (accepted in TPS only if previously accepted under 1967 Teachers' Pension Regulations)", "0970", "0971", "Full-time Organiser" },
                    { 67, "Full and Part-Time Youth and Community Worker", "0972", "0972", "Full and Part-Time Youth and Community Worker" },
                    { 68, "Service as a teacher in Practical Instruction Centres - providing P & S education (previously allocated individual numbers in range 5100-5199)(not schools - unattached units). (Service other than as a teacher would have to be considered by the Department)", "5199", "5199", "Service as a teacher in Practical Instruction Centres - providing P & S education" },
                    { 69, "Service as a teacher in Remedial Centres and Support Units - providing P & S education (not schools - unattached units). (Service other than as a teacher would have to be considered by the Department", "5549", "5549", "Service as a teacher in Remedial Centres and Support Units - providing P & S education (not schools - unattached units)" },
                    { 70, "Service as a teacher in any other P & S Centre (ie not PI or Remedial)(eg Assessment Centres outdoor pursuit centres, Teacher Centres [if paid P & S]). Service other than as a teacher would have to be considered by Teachers' Pensions)", "5599", "5599", "Service as a teacher in any other P & S Centre (ie not PI or Remedial)" },
                    { 71, "Service in Intermediate Treatment Centres - providing P & S education (not schools - unattached units)", "5600", "5600", "Service in Intermediate Treatment Centres - providing P & S education (not schools - unattached units)" },
                    { 72, "Function Provider within a LEA", "9099", "9099", "Function Provider within a LEA" },
                    { 73, "Adult Education Service (residential adult education estabs have numbers allocated in range 8290-8294). Teacher Centres if paid on FE Scales, Adult Literacy Scheme staff (LA)", "8899", "8899", "Adult Education Service" },
                    { 74, "Service of any other kind (eg full-time educational officers in penal estabs Job Creation Schemes). Normally this code will not be used where a teacher is in receipt of a mandatory Burnham salary", "0999", "0999", "Service of any other kind (eg full-time educational officers in penal estabs Job Creation Schemes)" },
                    { 75, "LA nursery schools", "1151", "1799", "LA nursery schools" }
                });

            migrationBuilder.InsertData(
                table: "establishments",
                columns: new[] { "establishment_id", "address3", "county", "establishment_name", "establishment_number", "establishment_source_id", "establishment_status_code", "establishment_status_name", "establishment_type_code", "establishment_type_group_code", "establishment_type_group_name", "establishment_type_name", "free_school_meals_percentage", "la_code", "la_name", "locality", "number_of_pupils", "phase_of_education_code", "phase_of_education_name", "postcode", "street", "town", "urn" },
                values: new object[,]
                {
                    { new Guid("1653f9ce-7a0b-4ca7-9ab1-4a2128dec8a5"), null, null, "Newcastle Diocesan Education Board", "0758", 2, null, null, null, null, null, null, null, "391", null, null, null, null, null, null, null, null, null },
                    { new Guid("1b1fa51d-7131-4720-b4d0-74d378aa0137"), null, null, "The Peoples Learning Trust", "1576", 2, null, null, null, null, null, null, null, "751", null, null, null, null, null, null, null, null, null },
                    { new Guid("1c93bba7-afd2-4299-8d78-4b61197dd359"), null, null, "Maintained school under Essex local authority", "2460", 2, null, null, null, null, null, null, null, "915", null, null, null, null, null, null, null, null, null },
                    { new Guid("207144ef-ac5a-49a1-832c-5cdbc636d69a"), null, null, "Mersey View Learning Trust", "1578", 2, null, null, null, null, null, null, null, "751", null, null, null, null, null, null, null, null, null },
                    { new Guid("2b9b1c3e-8057-4a68-8250-2699368e2e98"), null, null, "The Collective Community Trust", "1570", 2, null, null, null, null, null, null, null, "751", null, null, null, null, null, null, null, null, null },
                    { new Guid("2bd072d8-6214-4a77-b9b4-e8d77d96a030"), null, null, "Multi-Academy Trusts", "0000", 2, null, null, null, null, null, null, null, "751", null, null, null, null, null, null, null, null, null },
                    { new Guid("3dd6c9f6-a738-4a78-ad2d-fab4e8104184"), null, null, "Bedfordshire", "0000", 2, null, null, null, null, null, null, null, "820", null, null, null, null, null, null, null, null, null },
                    { new Guid("571c41ae-a33a-4ac5-a2b2-467ea5c7c5c4"), null, null, "Workers Educational Association", "0751", 2, null, null, null, null, null, null, null, "383", null, null, null, null, null, null, null, null, null },
                    { new Guid("58352ea6-ce6d-4225-a221-2b6a080f5a9a"), null, null, "Dorset", "0000", 2, null, null, null, null, null, null, null, "835", null, null, null, null, null, null, null, null, null },
                    { new Guid("7310b62f-454a-4d2c-8183-124acd71fd7a"), null, null, "Mosaic Partnership Trust Ltd", "1573", 2, null, null, null, null, null, null, null, "751", null, null, null, null, null, null, null, null, null },
                    { new Guid("7c8a6d4d-e19f-4c08-a314-348f1e159d27"), null, null, "Maintained school under Northamptonshire local authority", "4091", 2, null, null, null, null, null, null, null, "928", null, null, null, null, null, null, null, null, null },
                    { new Guid("854d9780-b0cd-4459-a49d-df9a9502b33f"), null, null, "Essex Local Authority", "4452", 2, null, null, null, null, null, null, null, "915", null, null, null, null, null, null, null, null, null },
                    { new Guid("95a0d99b-0d4a-42a2-9528-816f5aa3b93a"), null, null, "Ambition Community Trust", "1592", 2, null, null, null, null, null, null, null, "751", null, null, null, null, null, null, null, null, null },
                    { new Guid("96c02ad6-fc23-48a0-8e68-f45b49d4f695"), null, null, "Poole Local Authority", "0000", 2, null, null, null, null, null, null, null, "836", null, null, null, null, null, null, null, null, null },
                    { new Guid("983195b9-dc5b-4f00-bf50-d44b7c87305d"), null, null, "Benedict Catholic Academy Trust", "1571", 2, null, null, null, null, null, null, null, "751", null, null, null, null, null, null, null, null, null },
                    { new Guid("a0f3a003-45ab-4009-8385-5358c4b16108"), null, null, "Aurora Rowan School", "6019", 2, null, null, null, null, null, null, null, "870", null, null, null, null, null, null, null, null, null },
                    { new Guid("a37d12cf-f155-4dbf-88a0-1d30ab10c561"), null, null, "Kristian Thomas Company", "9097", 2, null, null, null, null, null, null, null, "855", null, null, null, null, null, null, null, null, null },
                    { new Guid("a4900bec-839f-43ee-8bca-ef79fc6fe233"), null, null, "Fern Academy Trust", "1587", 2, null, null, null, null, null, null, null, "751", null, null, null, null, null, null, null, null, null },
                    { new Guid("a5d97ce7-0913-41df-ad36-bbe38ac5ab4b"), null, null, "Ascendance Partnership Trust", "1589", 2, null, null, null, null, null, null, null, "751", null, null, null, null, null, null, null, null, null },
                    { new Guid("b6c401ad-2cdc-407e-9fb5-9a524caeab60"), null, null, "A.R.T.S. Education", "6006", 2, null, null, null, null, null, null, null, "340", null, null, null, null, null, null, null, null, null },
                    { new Guid("cb7ef0ee-41ce-4c2d-87dc-91aa968ca76c"), null, null, "Synergy Education Trust Limited", "1572", 2, null, null, null, null, null, null, null, "751", null, null, null, null, null, null, null, null, null },
                    { new Guid("d00cb67f-5da9-4430-a9a1-047490fc4df0"), null, null, "Cheshire", "0000", 2, null, null, null, null, null, null, null, "875", null, null, null, null, null, null, null, null, null },
                    { new Guid("d7bb70f1-a29a-43ee-a896-f9cbb5ac9d45"), null, null, "Heritage Multi Academy Trust", "1599", 2, null, null, null, null, null, null, null, "751", null, null, null, null, null, null, null, null, null },
                    { new Guid("e8939604-4570-4a56-9b8e-f53bf285e59c"), null, null, "Pennine Alliance Learning Trust", "1591", 2, null, null, null, null, null, null, null, "751", null, null, null, null, null, null, null, null, null },
                    { new Guid("e9490ca5-a696-451a-9038-33e4b5d32885"), null, null, "Places Leisure", "9097", 2, null, null, null, null, null, null, null, "936", null, null, null, null, null, null, null, null, null },
                    { new Guid("ede07733-97db-4e38-bff1-f3bd73b08986"), null, null, "Archdiocese of Birmingham", "0750", 2, null, null, null, null, null, null, null, "330", null, null, null, null, null, null, null, null, null }
                });

            // ***Generated migration ends***

            migrationBuilder.Procedure("fn_delete_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_delete_previous_names_person_attrs_v1.sql");
            migrationBuilder.Procedure("fn_delete_previous_names_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_delete_tps_employments_person_attrs_v1.sql");
            migrationBuilder.Procedure("fn_delete_tps_employments_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_generate_trn_v1.sql");
            migrationBuilder.Procedure("fn_insert_person_attrs_v1.sql");
            migrationBuilder.Procedure("fn_insert_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_insert_previous_names_person_attrs_v1.sql");
            migrationBuilder.Procedure("fn_insert_previous_names_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_insert_tps_employments_person_attrs_v1.sql");
            migrationBuilder.Procedure("fn_insert_tps_employments_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_split_names_v3.sql");
            migrationBuilder.Procedure("fn_update_person_attrs_v1.sql");
            migrationBuilder.Procedure("fn_update_person_search_attributes_v3.sql");
            migrationBuilder.Procedure("fn_update_previous_names_person_attrs_v1.sql");
            migrationBuilder.Procedure("fn_update_tps_employments_person_attrs_v1.sql");
            migrationBuilder.Procedure("fn_update_tps_employments_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("p_refresh_person_names_v1.sql");
            migrationBuilder.Procedure("p_refresh_person_ninos_v1.sql");
            migrationBuilder.Procedure("p_refresh_person_search_attributes_v8.sql");
            migrationBuilder.Procedure("p_refresh_previous_names_person_search_attributes_v4.sql");
            migrationBuilder.Procedure("p_refresh_tps_employments_person_search_attributes_v2.sql");

            migrationBuilder.Trigger("trg_delete_person_search_attributes_v1.sql");
            migrationBuilder.Trigger("trg_delete_previous_names_person_search_attributes_v1.sql");
            migrationBuilder.Trigger("trg_delete_tps_employments_person_attrs_v1.sql");
            migrationBuilder.Trigger("trg_delete_tps_employments_person_search_attributes_v1.sql");
            migrationBuilder.Trigger("trg_insert_person_attrs_v1.sql");
            migrationBuilder.Trigger("trg_insert_person_search_attributes_v1.sql");
            migrationBuilder.Trigger("trg_insert_previous_names_person_attrs_v1.sql");
            migrationBuilder.Trigger("trg_insert_previous_names_person_search_attributes_v1.sql");
            migrationBuilder.Trigger("trg_insert_tps_employments_person_attrs_v1.sql");
            migrationBuilder.Trigger("trg_insert_tps_employments_person_search_attributes_v1.sql");
            migrationBuilder.Trigger("trg_update_person_attrs_v1.sql");
            migrationBuilder.Trigger("trg_update_person_search_attributes_v2.sql");
            migrationBuilder.Trigger("trg_update_previous_names_person_attrs_v1.sql");
            migrationBuilder.Trigger("trg_update_previous_names_person_search_attributes_v1.sql");
            migrationBuilder.Trigger("trg_update_tps_employments_person_attrs_v1.sql");
            migrationBuilder.Trigger("trg_update_tps_employments_person_search_attributes_v1.sql");

            migrationBuilder.Sql(
                "CREATE PUBLICATION dqt_rep_sync FOR TABLE alert_categories, alert_types, alerts, establishments, events, induction_statuses, notes, persons, previous_names, qualifications, route_migration_report_items, support_task_types, support_tasks, tps_employments, tps_establishments, training_providers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotImplementedException();
        }
    }
}
