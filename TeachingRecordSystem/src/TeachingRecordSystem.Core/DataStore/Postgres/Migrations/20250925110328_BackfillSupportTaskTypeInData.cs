using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class BackfillSupportTaskTypeInData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                update support_tasks set data = jsonb_set(
                    data,
                    '{\$support-task-type}',
                     support_task_type::text::jsonb);

                update events set payload = jsonb_set(
                    payload,
                    '{SupportTask,Data,\$support-task-type}',
                     (payload->'SupportTask'->>'SupportTaskType')::jsonb)
                where event_name in ('SupportTaskCreatedEvent', 'SupportTaskUpdatedEvent');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
