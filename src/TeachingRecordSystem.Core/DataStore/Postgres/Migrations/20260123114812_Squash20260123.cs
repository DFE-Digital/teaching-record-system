using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Squash20260123 : Migration
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

            migrationBuilder.Procedure("fn_generate_trn_v1.sql");


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
                    warning_count = table.Column<int>(type: "integer", nullable: false),
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
                    use_shared_one_login_signing_keys = table.Column<bool>(type: "boolean", nullable: true),
                    one_login_private_key_pem = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    one_login_authentication_scheme_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    one_login_redirect_uri_path = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    one_login_post_logout_redirect_uri_path = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dqt_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dqt_user_name = table.Column<string>(type: "text", nullable: true),
                    person_ids = table.Column<List<Guid>>(type: "uuid[]", nullable: false)
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
                    one_login_user_subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    first_name = table.Column<string>(type: "text", nullable: true),
                    middle_name = table.Column<string>(type: "text", nullable: true),
                    last_name = table.Column<string>(type: "text", nullable: true),
                    previous_first_name = table.Column<string>(type: "text", nullable: true),
                    previous_middle_name = table.Column<string>(type: "text", nullable: true),
                    previous_last_name = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string[]>(type: "text[]", nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: false),
                    potential_duplicate = table.Column<bool>(type: "boolean", nullable: false),
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
                    status = table.Column<int>(type: "integer", nullable: false),
                    npq_working_in_educational_setting = table.Column<bool>(type: "boolean", nullable: true),
                    npq_application_id = table.Column<string>(type: "text", nullable: true),
                    npq_name = table.Column<string>(type: "text", nullable: true),
                    npq_training_provider = table.Column<string>(type: "text", nullable: true),
                    npq_evidence_file_id = table.Column<Guid>(type: "uuid", nullable: true),
                    npq_evidence_file_name = table.Column<string>(type: "text", nullable: true),
                    work_email_address = table.Column<string>(type: "text", nullable: true)
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
                    person_ids = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    trn = table.Column<string>(type: "character(7)", fixedLength: true, maxLength: 7, nullable: false, defaultValueSql: "fn_generate_trn()"),
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
                    email_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    matched_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "support_tasks",
                columns: table => new
                {
                    support_task_reference = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    support_task_type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    one_login_user_subject = table.Column<string>(type: "character varying(255)", nullable: true),
                    person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    trn_request_application_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    trn_request_id = table.Column<string>(type: "character varying(100)", nullable: true),
                    data = table.Column<string>(type: "jsonb", nullable: false),
                    resolve_journey_saved_state = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_support_tasks", x => x.support_task_reference);
                    table.ForeignKey(
                        name: "fk_support_tasks_one_login_users_one_login_user_subject",
                        column: x => x.one_login_user_subject,
                        principalTable: "one_login_users",
                        principalColumn: "subject");
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
                name: "ix_process_events_person_ids_event_name",
                table: "process_events",
                columns: new[] { "person_ids", "event_name" })
                .Annotation("Npgsql:CreatedConcurrently", true)
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "ix_process_events_process_id",
                table: "process_events",
                column: "process_id")
                .Annotation("Npgsql:CreatedConcurrently", true);

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


            // ***Generated migration ends***

            migrationBuilder.Procedure("fn_delete_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_delete_previous_names_person_attrs_v1.sql");
            migrationBuilder.Procedure("fn_delete_previous_names_person_search_attributes_v1.sql");
            migrationBuilder.Procedure("fn_delete_tps_employments_person_attrs_v1.sql");
            migrationBuilder.Procedure("fn_delete_tps_employments_person_search_attributes_v1.sql");
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
            migrationBuilder.Procedure("fn_resolve_record_by_trn_v1.sql");

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
                "CREATE PUBLICATION dqt_rep_sync FOR TABLE alert_categories, alert_types, alerts, establishments, events, induction_statuses, notes, persons, previous_names, qualifications, route_migration_report_items, support_task_types, support_tasks, tps_employments, tps_establishments, training_providers, processes, process_events, trn_request_metadata, users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException();
        }
    }
}
