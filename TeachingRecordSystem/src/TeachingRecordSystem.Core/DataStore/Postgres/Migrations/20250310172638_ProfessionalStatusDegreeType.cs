using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ProfessionalStatusDegreeType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "degree_type_id",
                table: "qualifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_qualifications_degree_types_degree_type_id",
                table: "qualifications",
                column: "degree_type_id",
                principalTable: "degree_types",
                principalColumn: "degree_type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_qualifications_degree_types_degree_type_id",
                table: "qualifications");

            migrationBuilder.DropColumn(
                name: "degree_type_id",
                table: "qualifications");
        }
    }
}
