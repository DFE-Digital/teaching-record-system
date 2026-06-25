using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ProcessSupportTasksAndOneLogins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "one_login_user_subjects",
                table: "processes",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "support_task_references",
                table: "processes",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<string[]>(
                name: "support_task_references",
                table: "process_events",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.Sql(
                """
                update process_events set one_login_user_subjects = ARRAY[payload->'SupportTask'->>'OneLoginUserSubject']::varchar[]
                where event_name in ('SupportTaskCreatedEvent', 'SupportTaskDeletedEvent', 'SupportTaskUpdatedEvent')
                and payload->'SupportTask'->>'OneLoginUserSubject' is not null;

                update process_events set support_task_references = ARRAY[payload->'SupportTask'->>'SupportTaskReference']::varchar[]
                where event_name in ('SupportTaskCreatedEvent', 'SupportTaskDeletedEvent', 'SupportTaskUpdatedEvent');

                update processes set one_login_user_subjects = pe.one_login_user_subjects
                from (
                    select process_id, array_agg(distinct one_login_user_subject) one_login_user_subjects
                    from process_events, unnest(one_login_user_subjects) as one_login_user_subject
                	group by process_id
                ) as pe
                where processes.process_id = pe.process_id;

                update processes set support_task_references = pe.support_task_references
                from (
                    select process_id, array_agg(distinct support_task_reference) support_task_references
                    from process_events, unnest(support_task_references) as support_task_reference
                	group by process_id
                ) as pe
                where processes.process_id = pe.process_id;
                """);

            migrationBuilder.CreateIndex(
                name: "ix_processes_one_login_user_subjects",
                table: "processes",
                column: "one_login_user_subjects")
                .Annotation("Npgsql:CreatedConcurrently", true)
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "ix_processes_support_task_references",
                table: "processes",
                column: "support_task_references")
                .Annotation("Npgsql:CreatedConcurrently", true)
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "ix_process_events_support_task_references_event_name",
                table: "process_events",
                columns: new[] { "support_task_references", "event_name" })
                .Annotation("Npgsql:CreatedConcurrently", true)
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_processes_one_login_user_subjects",
                table: "processes");

            migrationBuilder.DropIndex(
                name: "ix_processes_support_task_references",
                table: "processes");

            migrationBuilder.DropIndex(
                name: "ix_process_events_support_task_references_event_name",
                table: "process_events");

            migrationBuilder.DropColumn(
                name: "one_login_user_subjects",
                table: "processes");

            migrationBuilder.DropColumn(
                name: "support_task_references",
                table: "processes");

            migrationBuilder.DropColumn(
                name: "support_task_references",
                table: "process_events");
        }
    }
}
