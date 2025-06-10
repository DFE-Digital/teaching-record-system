using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class CountryMandatoryOnAllRoutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("ef46ff51-8dc0-481e-b158-61ccea9943d9"),
                column: "training_country_required",
                value: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "route_to_professional_status_types",
                keyColumn: "route_to_professional_status_type_id",
                keyValue: new Guid("ef46ff51-8dc0-481e-b158-61ccea9943d9"),
                column: "training_country_required",
                value: 2);
        }
    }
}
