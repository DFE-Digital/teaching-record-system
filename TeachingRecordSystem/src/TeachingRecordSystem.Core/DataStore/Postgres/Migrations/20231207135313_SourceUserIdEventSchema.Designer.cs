﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TeachingRecordSystem.Core.DataStore.Postgres;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    [DbContext(typeof(TrsDbContext))]
    [Migration("20231207135313_SourceUserIdEventSchema")]
    partial class SourceUserIdEventSchema
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.EntityChangesJournal", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("text")
                        .HasColumnName("key");

                    b.Property<string>("EntityLogicalName")
                        .HasColumnType("text")
                        .HasColumnName("entity_logical_name");

                    b.Property<string>("DataToken")
                        .HasColumnType("text")
                        .HasColumnName("data_token");

                    b.Property<DateTime?>("LastUpdated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_updated");

                    b.Property<string>("LastUpdatedBy")
                        .HasColumnType("text")
                        .HasColumnName("last_updated_by");

                    b.Property<int?>("NextQueryPageNumber")
                        .HasColumnType("integer")
                        .HasColumnName("next_query_page_number");

                    b.Property<int?>("NextQueryPageSize")
                        .HasColumnType("integer")
                        .HasColumnName("next_query_page_size");

                    b.Property<string>("NextQueryPagingCookie")
                        .HasColumnType("text")
                        .HasColumnName("next_query_paging_cookie");

                    b.HasKey("Key", "EntityLogicalName")
                        .HasName("pk_entity_changes_journals");

                    b.ToTable("entity_changes_journals", (string)null);
                });

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.Event", b =>
                {
                    b.Property<long>("EventId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("event_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("EventId"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created");

                    b.Property<string>("EventName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("event_name");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("payload");

                    b.Property<bool>("Published")
                        .HasColumnType("boolean")
                        .HasColumnName("published");

                    b.HasKey("EventId")
                        .HasName("pk_events");

                    b.HasIndex("Payload")
                        .HasDatabaseName("ix_events_payload");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Payload"), "gin");

                    b.ToTable("events", (string)null);
                });

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.EytsAwardedEmailsJob", b =>
                {
                    b.Property<Guid>("EytsAwardedEmailsJobId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("eyts_awarded_emails_job_id");

                    b.Property<DateTime>("AwardedToUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("awarded_to_utc");

                    b.Property<DateTime>("ExecutedUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("executed_utc");

                    b.HasKey("EytsAwardedEmailsJobId")
                        .HasName("pk_eyts_awarded_emails_jobs");

                    b.HasIndex("ExecutedUtc")
                        .HasDatabaseName("ix_eyts_awarded_emails_jobs_executed_utc");

                    b.ToTable("eyts_awarded_emails_jobs", (string)null);
                });

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.EytsAwardedEmailsJobItem", b =>
                {
                    b.Property<Guid>("EytsAwardedEmailsJobId")
                        .HasColumnType("uuid")
                        .HasColumnName("eyts_awarded_emails_job_id");

                    b.Property<Guid>("PersonId")
                        .HasColumnType("uuid")
                        .HasColumnName("person_id");

                    b.Property<string>("EmailAddress")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("email_address");

                    b.Property<bool>("EmailSent")
                        .HasColumnType("boolean")
                        .HasColumnName("email_sent");

                    b.Property<string>("Personalization")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("personalization");

                    b.Property<string>("Trn")
                        .IsRequired()
                        .HasMaxLength(7)
                        .HasColumnType("character(7)")
                        .HasColumnName("trn")
                        .IsFixedLength();

                    b.HasKey("EytsAwardedEmailsJobId", "PersonId")
                        .HasName("pk_eyts_awarded_emails_job_items");

                    b.HasIndex("Personalization")
                        .HasDatabaseName("ix_eyts_awarded_emails_job_items_personalization");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Personalization"), "gin");

                    b.ToTable("eyts_awarded_emails_job_items", (string)null);
                });

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.InductionCompletedEmailsJob", b =>
                {
                    b.Property<Guid>("InductionCompletedEmailsJobId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("induction_completed_emails_job_id");

                    b.Property<DateTime>("AwardedToUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("awarded_to_utc");

                    b.Property<DateTime>("ExecutedUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("executed_utc");

                    b.HasKey("InductionCompletedEmailsJobId")
                        .HasName("pk_induction_completed_emails_jobs");

                    b.HasIndex("ExecutedUtc")
                        .HasDatabaseName("ix_induction_completed_emails_jobs_executed_utc");

                    b.ToTable("induction_completed_emails_jobs", (string)null);
                });

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.InductionCompletedEmailsJobItem", b =>
                {
                    b.Property<Guid>("InductionCompletedEmailsJobId")
                        .HasColumnType("uuid")
                        .HasColumnName("induction_completed_emails_job_id");

                    b.Property<Guid>("PersonId")
                        .HasColumnType("uuid")
                        .HasColumnName("person_id");

                    b.Property<string>("EmailAddress")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("email_address");

                    b.Property<bool>("EmailSent")
                        .HasColumnType("boolean")
                        .HasColumnName("email_sent");

                    b.Property<string>("Personalization")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("personalization");

                    b.Property<string>("Trn")
                        .IsRequired()
                        .HasMaxLength(7)
                        .HasColumnType("character(7)")
                        .HasColumnName("trn")
                        .IsFixedLength();

                    b.HasKey("InductionCompletedEmailsJobId", "PersonId")
                        .HasName("pk_induction_completed_emails_job_items");

                    b.HasIndex("Personalization")
                        .HasDatabaseName("ix_induction_completed_emails_job_items_personalization");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Personalization"), "gin");

                    b.ToTable("induction_completed_emails_job_items", (string)null);
                });

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.InternationalQtsAwardedEmailsJob", b =>
                {
                    b.Property<Guid>("InternationalQtsAwardedEmailsJobId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("international_qts_awarded_emails_job_id");

                    b.Property<DateTime>("AwardedToUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("awarded_to_utc");

                    b.Property<DateTime>("ExecutedUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("executed_utc");

                    b.HasKey("InternationalQtsAwardedEmailsJobId")
                        .HasName("pk_international_qts_awarded_emails_jobs");

                    b.HasIndex("ExecutedUtc")
                        .HasDatabaseName("ix_international_qts_awarded_emails_jobs_executed_utc");

                    b.ToTable("international_qts_awarded_emails_jobs", (string)null);
                });

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.InternationalQtsAwardedEmailsJobItem", b =>
                {
                    b.Property<Guid>("InternationalQtsAwardedEmailsJobId")
                        .HasColumnType("uuid")
                        .HasColumnName("international_qts_awarded_emails_job_id");

                    b.Property<Guid>("PersonId")
                        .HasColumnType("uuid")
                        .HasColumnName("person_id");

                    b.Property<string>("EmailAddress")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("email_address");

                    b.Property<bool>("EmailSent")
                        .HasColumnType("boolean")
                        .HasColumnName("email_sent");

                    b.Property<string>("Personalization")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("personalization");

                    b.Property<string>("Trn")
                        .IsRequired()
                        .HasMaxLength(7)
                        .HasColumnType("character(7)")
                        .HasColumnName("trn")
                        .IsFixedLength();

                    b.HasKey("InternationalQtsAwardedEmailsJobId", "PersonId")
                        .HasName("pk_international_qts_awarded_emails_job_items");

                    b.HasIndex("Personalization")
                        .HasDatabaseName("ix_international_qts_awarded_emails_job_items_personalization");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Personalization"), "gin");

                    b.ToTable("international_qts_awarded_emails_job_items", (string)null);
                });

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.JourneyState", b =>
                {
                    b.Property<string>("InstanceId")
                        .HasMaxLength(300)
                        .HasColumnType("character varying(300)")
                        .HasColumnName("instance_id");

                    b.Property<DateTime?>("Completed")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("completed");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created");

                    b.Property<string>("State")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("state");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.HasKey("InstanceId")
                        .HasName("pk_journey_states");

                    b.ToTable("journey_states", (string)null);
                });

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.Person", b =>
                {
                    b.Property<Guid>("PersonId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("person_id");

                    b.Property<DateOnly?>("DateOfBirth")
                        .HasColumnType("date")
                        .HasColumnName("date_of_birth");

                    b.Property<Guid?>("DqtContactId")
                        .HasColumnType("uuid")
                        .HasColumnName("dqt_contact_id");

                    b.Property<DateTime?>("DqtCreatedOn")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("dqt_created_on");

                    b.Property<string>("DqtFirstName")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("dqt_first_name")
                        .UseCollation("case_insensitive");

                    b.Property<DateTime?>("DqtFirstSync")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("dqt_first_sync");

                    b.Property<string>("DqtLastName")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("dqt_last_name")
                        .UseCollation("case_insensitive");

                    b.Property<DateTime?>("DqtLastSync")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("dqt_last_sync");

                    b.Property<string>("DqtMiddleName")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("dqt_middle_name")
                        .UseCollation("case_insensitive");

                    b.Property<DateTime?>("DqtModifiedOn")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("dqt_modified_on");

                    b.Property<int?>("DqtState")
                        .HasColumnType("integer")
                        .HasColumnName("dqt_state");

                    b.Property<string>("EmailAddress")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("email_address")
                        .UseCollation("case_insensitive");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("first_name")
                        .UseCollation("case_insensitive");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("last_name")
                        .UseCollation("case_insensitive");

                    b.Property<string>("MiddleName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("middle_name")
                        .UseCollation("case_insensitive");

                    b.Property<string>("NationalInsuranceNumber")
                        .HasMaxLength(9)
                        .HasColumnType("character(9)")
                        .HasColumnName("national_insurance_number")
                        .IsFixedLength();

                    b.Property<string>("Trn")
                        .IsRequired()
                        .HasMaxLength(7)
                        .HasColumnType("character(7)")
                        .HasColumnName("trn")
                        .IsFixedLength();

                    b.HasKey("PersonId")
                        .HasName("pk_persons");

                    b.HasIndex("DqtContactId")
                        .IsUnique()
                        .HasDatabaseName("ix_persons_dqt_contact_id")
                        .HasFilter("dqt_contact_id is not null");

                    b.HasIndex("Trn")
                        .IsUnique()
                        .HasDatabaseName("ix_persons_trn")
                        .HasFilter("trn is not null");

                    b.ToTable("persons", (string)null);
                });

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.QtsAwardedEmailsJob", b =>
                {
                    b.Property<Guid>("QtsAwardedEmailsJobId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("qts_awarded_emails_job_id");

                    b.Property<DateTime>("AwardedToUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("awarded_to_utc");

                    b.Property<DateTime>("ExecutedUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("executed_utc");

                    b.HasKey("QtsAwardedEmailsJobId")
                        .HasName("pk_qts_awarded_emails_jobs");

                    b.HasIndex("ExecutedUtc")
                        .HasDatabaseName("ix_qts_awarded_emails_jobs_executed_utc");

                    b.ToTable("qts_awarded_emails_jobs", (string)null);
                });

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.QtsAwardedEmailsJobItem", b =>
                {
                    b.Property<Guid>("QtsAwardedEmailsJobId")
                        .HasColumnType("uuid")
                        .HasColumnName("qts_awarded_emails_job_id");

                    b.Property<Guid>("PersonId")
                        .HasColumnType("uuid")
                        .HasColumnName("person_id");

                    b.Property<string>("EmailAddress")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("email_address");

                    b.Property<bool>("EmailSent")
                        .HasColumnType("boolean")
                        .HasColumnName("email_sent");

                    b.Property<string>("Personalization")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("personalization");

                    b.Property<string>("Trn")
                        .IsRequired()
                        .HasMaxLength(7)
                        .HasColumnType("character(7)")
                        .HasColumnName("trn")
                        .IsFixedLength();

                    b.HasKey("QtsAwardedEmailsJobId", "PersonId")
                        .HasName("pk_qts_awarded_emails_job_items");

                    b.HasIndex("Personalization")
                        .HasDatabaseName("ix_qts_awarded_emails_job_items_personalization");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Personalization"), "gin");

                    b.ToTable("qts_awarded_emails_job_items", (string)null);
                });

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.TrnRequest", b =>
                {
                    b.Property<long>("TrnRequestId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("trn_request_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("TrnRequestId"));

                    b.Property<string>("ClientId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("client_id");

                    b.Property<Guid?>("IdentityUserId")
                        .HasColumnType("uuid")
                        .HasColumnName("identity_user_id");

                    b.Property<bool>("LinkedToIdentity")
                        .HasColumnType("boolean")
                        .HasColumnName("linked_to_identity");

                    b.Property<string>("RequestId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("request_id");

                    b.Property<Guid>("TeacherId")
                        .HasColumnType("uuid")
                        .HasColumnName("teacher_id");

                    b.Property<string>("TrnToken")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("trn_token");

                    b.HasKey("TrnRequestId")
                        .HasName("pk_trn_requests");

                    b.HasIndex("ClientId", "RequestId")
                        .IsUnique()
                        .HasDatabaseName("ix_trn_requests_client_id_request_id");

                    b.ToTable("trn_requests", (string)null);
                });

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.User", b =>
                {
                    b.Property<Guid>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.Property<bool>("Active")
                        .HasColumnType("boolean")
                        .HasColumnName("active");

                    b.Property<string>("AzureAdUserId")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("azure_ad_user_id");

                    b.Property<string>("Email")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("email")
                        .UseCollation("case_insensitive");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("name");

                    b.Property<string[]>("Roles")
                        .IsRequired()
                        .HasColumnType("varchar[]")
                        .HasColumnName("roles");

                    b.Property<int>("UserType")
                        .HasColumnType("integer")
                        .HasColumnName("user_type");

                    b.HasKey("UserId")
                        .HasName("pk_users");

                    b.HasIndex("AzureAdUserId")
                        .IsUnique()
                        .HasDatabaseName("ix_users_azure_ad_user_id");

                    b.ToTable("users", (string)null);

                    b.HasData(
                        new
                        {
                            UserId = new Guid("a81394d1-a498-46d8-af3e-e077596ab303"),
                            Active = true,
                            Name = "System",
                            Roles = new[] { "Administrator" },
                            UserType = 2
                        });
                });

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.EytsAwardedEmailsJobItem", b =>
                {
                    b.HasOne("TeachingRecordSystem.Core.DataStore.Postgres.Models.EytsAwardedEmailsJob", "EytsAwardedEmailsJob")
                        .WithMany("JobItems")
                        .HasForeignKey("EytsAwardedEmailsJobId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_eyts_awarded_emails_job_items_eyts_awarded_emails_jobs_eyts");

                    b.Navigation("EytsAwardedEmailsJob");
                });

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.InductionCompletedEmailsJobItem", b =>
                {
                    b.HasOne("TeachingRecordSystem.Core.DataStore.Postgres.Models.InductionCompletedEmailsJob", "InductionCompletedEmailsJob")
                        .WithMany("JobItems")
                        .HasForeignKey("InductionCompletedEmailsJobId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_induction_completed_emails_job_items_induction_completed_em");

                    b.Navigation("InductionCompletedEmailsJob");
                });

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.InternationalQtsAwardedEmailsJobItem", b =>
                {
                    b.HasOne("TeachingRecordSystem.Core.DataStore.Postgres.Models.InternationalQtsAwardedEmailsJob", "InternationalQtsAwardedEmailsJob")
                        .WithMany("JobItems")
                        .HasForeignKey("InternationalQtsAwardedEmailsJobId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_international_qts_awarded_emails_job_items_international_qt");

                    b.Navigation("InternationalQtsAwardedEmailsJob");
                });

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.QtsAwardedEmailsJobItem", b =>
                {
                    b.HasOne("TeachingRecordSystem.Core.DataStore.Postgres.Models.QtsAwardedEmailsJob", "QtsAwardedEmailsJob")
                        .WithMany("JobItems")
                        .HasForeignKey("QtsAwardedEmailsJobId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_qts_awarded_emails_job_items_qts_awarded_emails_jobs_qts_aw");

                    b.Navigation("QtsAwardedEmailsJob");
                });

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.EytsAwardedEmailsJob", b =>
                {
                    b.Navigation("JobItems");
                });

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.InductionCompletedEmailsJob", b =>
                {
                    b.Navigation("JobItems");
                });

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.InternationalQtsAwardedEmailsJob", b =>
                {
                    b.Navigation("JobItems");
                });

            modelBuilder.Entity("TeachingRecordSystem.Core.DataStore.Postgres.Models.QtsAwardedEmailsJob", b =>
                {
                    b.Navigation("JobItems");
                });
#pragma warning restore 612, 618
        }
    }
}
