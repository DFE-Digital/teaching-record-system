using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingTpsEstablishmentsFromNov25 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "establishments",
                columns: new[] { "establishment_id", "address3", "county", "establishment_name", "establishment_number", "establishment_source_id", "establishment_status_code", "establishment_status_name", "establishment_type_code", "establishment_type_group_code", "establishment_type_group_name", "establishment_type_name", "free_school_meals_percentage", "la_code", "la_name", "locality", "number_of_pupils", "phase_of_education_code", "phase_of_education_name", "postcode", "street", "town", "urn" },
                values: new object[,]
                {
                    { new Guid("019d868e-5ad4-4ebe-b8b0-f12e450f348a"), null, null, "Thornton Grove Academy", "2032", 2, null, null, null, null, null, null, null, "845", null, null, null, null, null, null, null, null, null },
                    { new Guid("28d30e56-c104-4b9f-8059-774fe3bc18dc"), null, null, "People Plus Group Ltd.", "0753", 2, null, null, null, null, null, null, null, "801", null, null, null, null, null, null, null, null, null },
                    { new Guid("86f4c057-6385-4b05-a794-9837c9a3d428"), null, null, "Lighthouse Multi Academy Trust", "1613", 2, null, null, null, null, null, null, null, "751", null, null, null, null, null, null, null, null, null },
                    { new Guid("d36d3900-74ef-431c-a537-4471702f79bd"), null, null, "Chester Diocesan Learning Trust", "1615", 2, null, null, null, null, null, null, null, "751", null, null, null, null, null, null, null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "establishments",
                keyColumn: "establishment_id",
                keyValue: new Guid("019d868e-5ad4-4ebe-b8b0-f12e450f348a"));

            migrationBuilder.DeleteData(
                table: "establishments",
                keyColumn: "establishment_id",
                keyValue: new Guid("28d30e56-c104-4b9f-8059-774fe3bc18dc"));

            migrationBuilder.DeleteData(
                table: "establishments",
                keyColumn: "establishment_id",
                keyValue: new Guid("86f4c057-6385-4b05-a794-9837c9a3d428"));

            migrationBuilder.DeleteData(
                table: "establishments",
                keyColumn: "establishment_id",
                keyValue: new Guid("d36d3900-74ef-431c-a537-4471702f79bd"));
        }
    }
}
