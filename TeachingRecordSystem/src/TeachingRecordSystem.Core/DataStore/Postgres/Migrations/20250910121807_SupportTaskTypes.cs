using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class SupportTaskTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                update support_tasks set support_task_type_id = case support_task_type
                when 1 then '4b76f9b8-c60e-4076-ac4b-d173f395dc71'::uuid
                when 2 then '6bc82e72-7592-4b05-a4ae-822fb52cad8d'::uuid
                when 3 then 'b621cc79-b116-461e-be8d-593d6efd53cd'::uuid
                when 4 then '37c27275-829c-4aa0-a47c-62a0092d0a71'::uuid
                when 5 then '3ca684d4-15de-4f12-b0fb-c5386360b171'::uuid
                when 6 then '80adb2e0-199c-4629-b494-4d052230a248'::uuid
                when 7 then 'fdee6a10-6338-463a-b6df-e34a2b95a854'::uuid
                end
                where support_task_type_id is null;

                update events set payload = jsonb_insert(
                    payload,
                    '{SupportTask,SupportTaskTypeId}',
                    case payload->'SupportTask'->>'SupportTaskType'
                    when '1' then '"4b76f9b8-c60e-4076-ac4b-d173f395dc71"'
                    when '2' then '"6bc82e72-7592-4b05-a4ae-822fb52cad8d"'
                    when '3' then '"b621cc79-b116-461e-be8d-593d6efd53cd"'
                    when '4' then '"37c27275-829c-4aa0-a47c-62a0092d0a71"'
                    when '5' then '"3ca684d4-15de-4f12-b0fb-c5386360b171"'
                    when '6' then '"80adb2e0-199c-4629-b494-4d052230a248"'
                    when '7' then '"fdee6a10-6338-463a-b6df-e34a2b95a854"'
                    end::jsonb)
                where event_name in (
                    'SupportTaskCreatedEvent',
                    'ApiTrnRequestSupportTaskUpdatedEvent',
                    'ChangeDateOfBirthRequestSupportTaskApprovedEvent',
                    'ChangeDateOfBirthRequestSupportTaskCancelledEvent',
                    'ChangeDateOfBirthRequestSupportTaskRejectedEvent',
                    'ChangeNameRequestSupportTaskApprovedEvent',
                    'ChangeNameRequestSupportTaskCancelledEvent',
                    'ChangeNameRequestSupportTaskRejectedEvent',
                    'NpqTrnRequestSupportTaskRejectedEvent',
                    'NpqTrnRequestSupportTaskResolvedEvent');

                update events set payload = jsonb_insert(
                    payload,
                    '{SupportTask,SupportTaskTypeId}',
                    case payload->'OldSupportTask'->>'SupportTaskType'
                    when '1' then '"4b76f9b8-c60e-4076-ac4b-d173f395dc71"'
                    when '2' then '"6bc82e72-7592-4b05-a4ae-822fb52cad8d"'
                    when '3' then '"b621cc79-b116-461e-be8d-593d6efd53cd"'
                    when '4' then '"37c27275-829c-4aa0-a47c-62a0092d0a71"'
                    when '5' then '"3ca684d4-15de-4f12-b0fb-c5386360b171"'
                    when '6' then '"80adb2e0-199c-4629-b494-4d052230a248"'
                    when '7' then '"fdee6a10-6338-463a-b6df-e34a2b95a854"'
                    end::jsonb)
                where event_name in (
                    'ApiTrnRequestSupportTaskUpdatedEvent',
                    'ChangeDateOfBirthRequestSupportTaskApprovedEvent',
                    'ChangeDateOfBirthRequestSupportTaskCancelledEvent',
                    'ChangeDateOfBirthRequestSupportTaskRejectedEvent',
                    'ChangeNameRequestSupportTaskApprovedEvent',
                    'ChangeNameRequestSupportTaskCancelledEvent',
                    'ChangeNameRequestSupportTaskRejectedEvent',
                    'NpqTrnRequestSupportTaskRejectedEvent',
                    'NpqTrnRequestSupportTaskResolvedEvent');
                """);

            migrationBuilder.DropColumn(
                name: "support_task_type",
                table: "support_tasks");

            migrationBuilder.AlterColumn<Guid>(
                name: "support_task_type_id",
                table: "support_tasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "support_task_types",
                columns: table => new
                {
                    support_task_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_support_task_types", x => x.support_task_type_id);
                });

            migrationBuilder.InsertData(
                table: "support_task_types",
                columns: new[] { "support_task_type_id", "category", "name" },
                values: new object[,]
                {
                    { new Guid("37c27275-829c-4aa0-a47c-62a0092d0a71"), 3, "TRN request from API" },
                    { new Guid("3ca684d4-15de-4f12-b0fb-c5386360b171"), 3, "TRN request from NPQ" },
                    { new Guid("4b76f9b8-c60e-4076-ac4b-d173f395dc71"), 1, "connect GOV.UK One Login user to a teaching record" },
                    { new Guid("6bc82e72-7592-4b05-a4ae-822fb52cad8d"), 2, "change name request" },
                    { new Guid("80adb2e0-199c-4629-b494-4d052230a248"), 3, "manual checks needed" },
                    { new Guid("b621cc79-b116-461e-be8d-593d6efd53cd"), 2, "change date of birth request" },
                    { new Guid("fdee6a10-6338-463a-b6df-e34a2b95a854"), 4, "Capita import potential duplicate" }
                });

            migrationBuilder.AddForeignKey(
                name: "fk_support_tasks_support_task_types_support_task_type_id",
                table: "support_tasks",
                column: "support_task_type_id",
                principalTable: "support_task_types",
                principalColumn: "support_task_type_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_support_tasks_support_task_types_support_task_type_id",
                table: "support_tasks");

            migrationBuilder.DropTable(
                name: "support_task_types");

            migrationBuilder.AlterColumn<Guid>(
                name: "support_task_type_id",
                table: "support_tasks",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<int>(
                name: "support_task_type",
                table: "support_tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
