using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class PersonsGin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                with merged_data as (
                	select
                		person_id,
                		string_agg(first_name, ' ') first_names,
                        string_agg(middle_name, ' ') middle_names,
                        string_agg(last_name, ' ') last_names,
                		array_agg(national_insurance_number) national_insurance_numbers
                	from (
                		select person_id, first_name, middle_name, last_name, national_insurance_number from persons
                		union all
                		select person_id, first_name, middle_name, last_name, null from previous_names where deleted_on is null
                		union all
                		select person_id, null, null, null, national_insurance_number from tps_employments
                	)
                	where person_id in (
                	    select person_id from persons
                	    where (names is null or national_insurance_numbers is null) and
                	    (first_name is not null or last_name is not null or national_insurance_number is not null)
                    )
                	group by person_id
                )
                update persons set
                	names = fn_split_names(ARRAY[merged_data.first_names, merged_data.middle_names, merged_data.last_names]::varchar[] COLLATE "default"),
                	last_names = fn_split_names(ARRAY[merged_data.last_names]::varchar[] COLLATE "default"),
                	national_insurance_numbers = array_remove(merged_data.national_insurance_numbers, null)
                from merged_data
                where merged_data.person_id = persons.person_id
                """,
                suppressTransaction: true);

            migrationBuilder.CreateIndex(
                name: "ix_persons_trn_date_of_birth_email_address_names_last_names_na",
                table: "persons",
                columns: new[] { "trn", "date_of_birth", "email_address", "names", "last_names", "national_insurance_numbers" })
                .Annotation("Npgsql:CreatedConcurrently", true)
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Relational:Collation", new[] { "case_insensitive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_persons_trn_date_of_birth_email_address_names_last_names_na",
                table: "persons");
        }
    }
}
