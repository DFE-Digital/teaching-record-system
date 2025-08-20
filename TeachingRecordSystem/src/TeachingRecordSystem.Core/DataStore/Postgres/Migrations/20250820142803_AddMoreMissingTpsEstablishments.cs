using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreMissingTpsEstablishments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "establishments",
                columns: new[] { "establishment_id", "urn", "la_code", "la_name", "establishment_number", "establishment_name", "establishment_type_code", "establishment_type_name", "establishment_type_group_code", "establishment_type_group_name", "establishment_status_code", "establishment_status_name", "phase_of_education_code", "phase_of_education_name", "number_of_pupils", "free_school_meals_percentage", "street", "locality", "address3", "town", "county", "postcode", "establishment_source_id" },
                values: new object[,]
                {
                    { new Guid("1653f9ce-7a0b-4ca7-9ab1-4a2128dec8a5"), null, "391", null, "0758", "Newcastle Diocesan Education Board", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("983195b9-dc5b-4f00-bf50-d44b7c87305d"), null, "751", null, "1571", "Benedict Catholic Academy Trust", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("207144ef-ac5a-49a1-832c-5cdbc636d69a"), null, "751", null, "1578", "Mersey View Learning Trust", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("e8939604-4570-4a56-9b8e-f53bf285e59c"), null, "751", null, "1591", "Pennine Alliance Learning Trust", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("95a0d99b-0d4a-42a2-9528-816f5aa3b93a"), null, "751", null, "1592", "Ambition Community Trust", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("d7bb70f1-a29a-43ee-a896-f9cbb5ac9d45"), null, "751", null, "1599", "Heritage Multi Academy Trust", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("96c02ad6-fc23-48a0-8e68-f45b49d4f695"), null, "836", null, "0000", "Poole Local Authority", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("a0f3a003-45ab-4009-8385-5358c4b16108"), null, "870", null, "6019", "Aurora Rowan School", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("1c93bba7-afd2-4299-8d78-4b61197dd359"), null, "915", null, "2460", "Maintained school under Essex local authority", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                    { new Guid("7c8a6d4d-e19f-4c08-a314-348f1e159d27"), null, "928", null, "4091", "Maintained school under Northamptonshire local authority", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 2 },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
