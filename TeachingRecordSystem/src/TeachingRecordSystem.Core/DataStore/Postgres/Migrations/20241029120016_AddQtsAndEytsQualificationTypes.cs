using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddQtsAndEytsQualificationTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "awarded_date",
                table: "qualifications",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "dqt_qts_registration_id",
                table: "qualifications",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "awarded_date",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "dqt_qts_registration_id",
                table: "qualifications");
        }
    }
}
