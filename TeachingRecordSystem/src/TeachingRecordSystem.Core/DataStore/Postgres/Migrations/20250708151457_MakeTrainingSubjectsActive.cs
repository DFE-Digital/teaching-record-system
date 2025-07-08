using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class MakeTrainingSubjectsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("649d7736-d301-4c42-873a-b24486fd35d7"),
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9caa584a-bb89-450d-8d8d-16ba0e84e28e"),
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f3e91599-2a2e-4f81-b4e0-9098a1ce8ec7"),
                column: "is_active",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("649d7736-d301-4c42-873a-b24486fd35d7"),
                column: "is_active",
                value: false);

            migrationBuilder.UpdateData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9caa584a-bb89-450d-8d8d-16ba0e84e28e"),
                column: "is_active",
                value: false);

            migrationBuilder.UpdateData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f3e91599-2a2e-4f81-b4e0-9098a1ce8ec7"),
                column: "is_active",
                value: false);
        }
    }
}
