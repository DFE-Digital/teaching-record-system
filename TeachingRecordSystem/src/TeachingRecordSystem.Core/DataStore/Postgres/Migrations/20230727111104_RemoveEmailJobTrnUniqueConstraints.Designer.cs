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
    [Migration("20230727111104_RemoveEmailJobTrnUniqueConstraints")]
    partial class RemoveEmailJobTrnUniqueConstraints
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.8")
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
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("data_token");

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

                    b.Property<string>("AzureAdSubject")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("azure_ad_subject");

                    b.Property<string>("Email")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("email");

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

                    b.ToTable("users", (string)null);
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
