using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class SupportTaskReferenceSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("create sequence support_task_reference start 100000");

            migrationBuilder.AlterColumn<string>(
                name: "support_task_reference",
                table: "support_tasks",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValueSql: "'TRS-' || nextval('support_task_reference')::text",
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "support_task_reference",
                table: "support_tasks",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16,
                oldDefaultValueSql: "'TRS-' || nextval('support_task_reference')::text");

            migrationBuilder.Sql("drop sequence support_task_reference");
        }
    }
}
