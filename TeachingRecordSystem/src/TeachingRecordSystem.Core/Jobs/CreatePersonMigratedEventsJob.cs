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
                        'PersonId', p.person_id,
                        'Trn', p.trn,
                        'PersonAttributes', json_build_object(
                            'FirstName', p.first_name,
                            'MiddleName', p.middle_name,
                            'LastName', p.last_name,
                            'DateOfBirth', p.date_of_birth,
                            'EmailAddress', p.email_address,
                            'NationalInsuranceNumber', p.national_insurance_number,
                            'Gender', p.gender
                        )
                    ) as payload,
                    gen_random_uuid() as event_id,
                    (now() at time zone 'utc') as inserted,
                    p.person_id,
                    ARRAY[p.person_id] as person_ids,
                    false as published
                from persons p
                left join events e on p.person_id = e.person_id and e.event_name = 'PersonMigratedEvent'
                where e.person_id is null
                limit 5000
                """);
            cmd.CommandTimeout = 0;
            eventsCreated = await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        while (eventsCreated > 0);
    }
}
