using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillPersonAttributesJob(TrsDbContext dbContext)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        var sql =
            $"""
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
                    (first_name is not null or last_name is not null or national_insurance_number is not null) limit 10000
                )
            	group by person_id
            )
            update persons set
            	names = fn_split_names(ARRAY[merged_data.first_names, merged_data.middle_names, merged_data.last_names]::varchar[] COLLATE "default"),
            	last_names = fn_split_names(ARRAY[merged_data.last_names]::varchar[] COLLATE "default"),
            	national_insurance_numbers = array_remove(merged_data.national_insurance_numbers, NULL)
            from merged_data
            where merged_data.person_id = persons.person_id
            """;

        int updated;
        do
        {
            updated = await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }
        while (updated > 0);
    }
}
