using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class BackfillProcessColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                update process_events set one_login_user_subjects = ARRAY[payload->'SupportTask'->>'OneLoginUserSubject']::varchar[]
                where event_name in ('SupportTaskCreatedEvent', 'SupportTaskDeletedEvent', 'SupportTaskUpdatedEvent')
                and payload->'SupportTask'->>'OneLoginUserSubject' is not null
                and array_length(one_login_user_subjects, 1) is null;
                """,
                suppressTransaction: true);

            migrationBuilder.Sql(
                """
                update process_events set support_task_references = ARRAY[payload->'SupportTask'->>'SupportTaskReference']::varchar[]
                where event_name in ('SupportTaskCreatedEvent', 'SupportTaskDeletedEvent', 'SupportTaskUpdatedEvent')
                and array_length(support_task_references, 1) is null;
                """,
                suppressTransaction: true);

            migrationBuilder.Sql(
                """
                update processes set one_login_user_subjects = pe.one_login_user_subjects
                from (
                    select process_id, array_agg(distinct one_login_user_subject) one_login_user_subjects
                    from process_events, unnest(one_login_user_subjects) as one_login_user_subject
                	group by process_id
                ) as pe
                where processes.process_id = pe.process_id
                and array_length(processes.one_login_user_subjects, 1) is null;
                """,
                suppressTransaction: true);

            migrationBuilder.Sql(
                """
                update processes set support_task_references = pe.support_task_references
                from (
                    select process_id, array_agg(distinct support_task_reference) support_task_references
                    from process_events, unnest(support_task_references) as support_task_reference
                	group by process_id
                ) as pe
                where processes.process_id = pe.process_id
                and array_length(processes.support_task_references, 1) is null;
                """,
                suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
