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
                columns: new[] { "establishment_id", "address3", "county", "establishment_name", "establishment_number", "establishment_source_id", "establishment_status_code", "establishment_status_name", "establishment_type_code", "establishment_type_group_code", "establishment_type_group_name", "establishment_type_name", "free_school_meals_percentage", "la_code", "la_name", "locality", "number_of_pupils", "phase_of_education_code", "phase_of_education_name", "postcode", "street", "town", "urn" },
                values: new object[,]
                {
                    { new Guid("837c3f31-980e-4393-8790-d38289d0deba"), null, null, "Tame River Educational Trust", "1600", 2, null, null, null, null, null, null, null, "751", null, null, null, null, null, null, null, null, null },
                    { new Guid("2a5b113f-04af-494e-b09f-b90ee04712f4"), null, null, "The Forge Brook Trust", "1601", 2, null, null, null, null, null, null, null, "751", null, null, null, null, null, null, null, null, null },
                    { new Guid("81d8d02f-35ad-4619-9a7a-15f3fb10acf7"), null, null, "Workers Educational Association", "0751", 2, null, null, null, null, null, null, null, "873", null, null, null, null, null, null, null, null, null },
                    { new Guid("e6b9bd0b-5a10-4a82-91f5-942b871a2d16"), null, null, "Worcester", "9450", 2, null, null, null, null, null, null, null, "918", null, null, null, null, null, null, null, null, null },
                    { new Guid("e71de82d-d56c-4e4e-9bdf-cc8bce890593"), null, null, "Northamptonshire", "0000", 2, null, null, null, null, null, null, null, "928", null, null, null, null, null, null, null, null, null },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
