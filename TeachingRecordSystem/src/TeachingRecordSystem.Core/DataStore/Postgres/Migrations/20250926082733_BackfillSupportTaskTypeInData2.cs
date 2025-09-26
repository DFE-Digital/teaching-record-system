using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class BackfillSupportTaskTypeInData2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                update events set
                    payload = jsonb_set(
                        payload,
                        '{SupportTask,Data,\$support-task-type}',
                         (payload->'SupportTask'->>'SupportTaskType')::jsonb)
                where event_name in (
                    'ApiTrnRequestSupportTaskUpdatedEvent',
                    'ChangeDateOfBirthRequestSupportTaskApprovedEvent',
                    'ChangeDateOfBirthRequestSupportTaskCancelledEvent',
                    'ChangeDateOfBirthRequestSupportTaskRejectedEvent',
                    'ChangeNameRequestSupportTaskApprovedEvent',
                    'ChangeNameRequestSupportTaskCancelledEvent',
                    'ChangeNameRequestSupportTaskRejectedEvent',
                    'NpqTrnRequestSupportTaskRejectedEvent',
                    'NpqTrnRequestSupportTaskResolvedEvent',
                    'TeacherPensionsPotentialDuplicateSupportTaskResolvedEvent'
                );

                update events set
                    payload = jsonb_set(
                        payload,
                        '{OldSupportTask,Data,\$support-task-type}',
                         (payload->'OldSupportTask'->>'SupportTaskType')::jsonb)
                where event_name in (
                    'ApiTrnRequestSupportTaskUpdatedEvent',
                    'ChangeDateOfBirthRequestSupportTaskApprovedEvent',
                    'ChangeDateOfBirthRequestSupportTaskCancelledEvent',
                    'ChangeDateOfBirthRequestSupportTaskRejectedEvent',
                    'ChangeNameRequestSupportTaskApprovedEvent',
                    'ChangeNameRequestSupportTaskCancelledEvent',
                    'ChangeNameRequestSupportTaskRejectedEvent',
                    'NpqTrnRequestSupportTaskRejectedEvent',
                    'NpqTrnRequestSupportTaskResolvedEvent',
                    'TeacherPensionsPotentialDuplicateSupportTaskResolvedEvent'
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
