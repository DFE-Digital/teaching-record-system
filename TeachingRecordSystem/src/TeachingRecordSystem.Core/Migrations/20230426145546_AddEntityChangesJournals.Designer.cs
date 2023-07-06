﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TeachingRecordSystem.Core.DataStore.Postgres;

#nullable disable

namespace TeachingRecordSystem.Api.Migrations
{
    [DbContext(typeof(TrsDbContext))]
    [Migration("20230426145546_AddEntityChangesJournals")]
    partial class AddEntityChangesJournals
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
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

                    b.Property<Guid?>("TeacherId")
                        .HasColumnType("uuid")
                        .HasColumnName("teacher_id");

                    b.HasKey("TrnRequestId")
                        .HasName("pk_trn_requests");

                    b.HasIndex("ClientId", "RequestId")
                        .IsUnique()
                        .HasDatabaseName("ix_trn_requests_client_id_request_id");

                    b.ToTable("trn_requests", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
