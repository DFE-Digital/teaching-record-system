using Npgsql;

namespace TeachingRecordSystem.Core.Jobs;

public class CreatePersonMigratedEventsJob(NpgsqlDataSource dataSource)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        int eventsCreated;
        do
        {
            using var cmd = dataSource.CreateCommand(
                """
                insert into events (event_name, created, payload, event_id, inserted, person_id, person_ids, published)
                select
                    'PersonMigratedEvent' as event_name,
                    (now() at time zone 'utc') as created,
                    json_build_object(
                        'PersonId', person_id,
                        'Trn', 'trn',
                        'PersonAttributes', json_build_object(
                            'FirstName', first_name,
                            'MiddleName', middle_name,
                            'LastName', last_name,
                            'DateOfBirth', date_of_birth,
                            'EmailAddress', email_address,
                            'NationalInsuranceNumber', national_insurance_number,
                            'Gender', gender
                        )
                    ) as payload,
                    gen_random_uuid() as event_id,
                    (now() at time zone 'utc') as inserted,
                    person_id,
                    ARRAY[person_id] as person_ids,
                    false as published
                from persons
                where person_id not in (
                    select person_id from events where event_name = 'PersonMigratedEvent'
                )
                limit 5000
                """);
            cmd.CommandTimeout = 0;
            eventsCreated = await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        while (eventsCreated > 0);
    }
}
