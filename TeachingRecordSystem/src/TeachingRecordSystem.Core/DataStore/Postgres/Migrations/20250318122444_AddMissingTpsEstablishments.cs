using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingTpsEstablishments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "establishments",
                columns: new[] { "establishment_id", "urn", "la_code", "la_name", "establishment_number", "establishment_name", "establishment_type_code", "establishment_type_name", "establishment_type_group_code", "establishment_type_group_name", "establishment_status_code", "establishment_status_name", "phase_of_education_code", "phase_of_education_name", "number_of_pupils", "free_school_meals_percentage", "street", "locality", "address3", "town", "county", "postcode", "establishment_source_id" },
                values: new object[,]
                {
                    { new Guid("ede07733-97db-4e38-bff1-f3bd73b08986"), null, "330", null, "0750", "Archdiocese of Birmingham", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("b6c401ad-2cdc-407e-9fb5-9a524caeab60"), null, "340", null, "6006", "A.R.T.S. Education", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("571c41ae-a33a-4ac5-a2b2-467ea5c7c5c4"), null, "383", null, "0751", "Workers Educational Association", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("1b1fa51d-7131-4720-b4d0-74d378aa0137"), null, "751", null, "1576", "The Peoples Learning Trust", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("a4900bec-839f-43ee-8bca-ef79fc6fe233"), null, "751", null, "1587", "Fern Academy Trust", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("a5d97ce7-0913-41df-ad36-bbe38ac5ab4b"), null, "751", null, "1589", "Ascendance Partnership Trust", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("3dd6c9f6-a738-4a78-ad2d-fab4e8104184"), null, "820", null, "0000", "Bedfordshire", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("58352ea6-ce6d-4225-a221-2b6a080f5a9a"), null, "835", null, "0000", "Dorset", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("a37d12cf-f155-4dbf-88a0-1d30ab10c561"), null, "855", null, "9097", "Kristian Thomas Company", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("d00cb67f-5da9-4430-a9a1-047490fc4df0"), null, "875", null, "0000", "Cheshire", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("854d9780-b0cd-4459-a49d-df9a9502b33f"), null, "915", null, "4452", "Essex Local Authority", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("e9490ca5-a696-451a-9038-33e4b5d32885"), null, "936", null, "9097", "Places Leisure", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
