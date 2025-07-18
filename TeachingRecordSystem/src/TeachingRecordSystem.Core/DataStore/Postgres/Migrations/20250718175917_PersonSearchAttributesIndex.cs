using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class PersonSearchAttributesIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX ix_person_search_attributes_attribute_type_and_value");

            migrationBuilder.Sql(
                $"""
                 CREATE INDEX CONCURRENTLY ix_person_search_attributes_attribute_type_and_value
                     ON person_search_attributes
                     (attribute_type COLLATE public.case_insensitive, attribute_value COLLATE public.case_insensitive)
                 	INCLUDE (person_id);
                 """,
                suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotImplementedException();
        }
    }
}
