using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddSupportTaskSubjectNamesTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Procedure("p_refresh_support_task_subject_names_v1.sql");
            migrationBuilder.Procedure("fn_insert_support_task_subject_names_v1.sql");
            migrationBuilder.Procedure("fn_update_support_task_subject_names_v1.sql");
            migrationBuilder.Trigger("trg_insert_support_task_subject_names_v1.sql");
            migrationBuilder.Trigger("trg_update_support_task_subject_names_v1.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER trg_insert_support_task_subject_names ON support_tasks;");
            migrationBuilder.Sql("DROP TRIGGER trg_update_support_task_subject_names ON support_tasks;");
            migrationBuilder.Sql("DROP FUNCTION fn_insert_support_task_subject_names;");
            migrationBuilder.Sql("DROP FUNCTION fn_update_support_task_subject_names;");
            migrationBuilder.Sql("DROP PROCEDURE p_refresh_support_task_subject_names;");
        }
    }
}
