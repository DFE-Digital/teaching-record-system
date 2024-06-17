using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.WorkforceData;

public class TpsCsvExtractProcessor(
    IDbContextFactory<TrsDbContext> dbContextFactory,
    IClock clock)
{
    private const string TempEventsTableSuffix = "tps_extract_events";

    public async Task ProcessNonMatchingTrns(Guid tpsCsvExtractId, CancellationToken cancellationToken)
    {
        int i = 0;
        using var dbContext = dbContextFactory.CreateDbContext();
        foreach (var item in dbContext.TpsCsvExtractItems.Where(r => r.TpsCsvExtractId == tpsCsvExtractId && !dbContext.Persons.Any(p => p.Trn == r.Trn)))
        {
            item.Result = TpsCsvExtractItemResult.InvalidTrn;
            i++;
            if (i % 1000 == 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        if (dbContext.ChangeTracker.HasChanges())
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ProcessNonMatchingEstablishments(Guid tpsCsvExtractId, CancellationToken cancellationToken)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        dbContext.Database.SetCommandTimeout(300);

        FormattableString updateSql =
            $"""
            WITH unique_establishments AS (
                SELECT
                    establishment_id,
                    la_code,
                    establishment_number,
                    establishment_name,
                    establishment_type_code,
                    postcode
                FROM
                    (SELECT
                        establishment_id,
                        la_code,
                        establishment_number,
                        establishment_name,
                        establishment_type_code,
                        postcode,
                        ROW_NUMBER() OVER (PARTITION BY la_code, establishment_number, CASE WHEN establishment_number IS NULL THEN postcode ELSE NULL END ORDER BY translate(establishment_status_code::text, '1234', '1324'), urn desc) as row_number
                    FROM
                        establishments) e
                    WHERE
                        e.row_number = 1
            )
            UPDATE
                tps_csv_extract_items x
            SET
                result = {TpsCsvExtractItemResult.InvalidEstablishment}
            WHERE
                x.tps_csv_extract_id = {tpsCsvExtractId}
                AND EXISTS (SELECT
                                1
                            FROM
                                persons p
                            WHERE
                                p.trn = x.trn)
                AND NOT EXISTS (SELECT
                                    1
                                FROM
                                    unique_establishments e
                                WHERE
                                    x.local_authority_code = e.la_code
                                    AND (e.establishment_number = x.establishment_number
                                         OR (e.establishment_type_code = '29' 
                                             AND x.establishment_postcode = e.postcode
                                             AND NOT EXISTS (SELECT
                                                                1
                                                             FROM
                                                                unique_establishments e2
                                                             WHERE
                                                                e2.la_code = x.local_authority_code
                                                                AND e2.establishment_number = x.establishment_number))))
            """;

        await dbContext.Database.ExecuteSqlAsync(updateSql, cancellationToken);
    }

    public async Task ProcessNewEmploymentHistory(Guid tpsCsvExtractId, CancellationToken cancellationToken)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        FormattableString querySql =
            $"""
            WITH unique_establishments AS (
                SELECT
                    establishment_id,
                    la_code,
                    establishment_number,
                    establishment_name,
                    establishment_type_code,
                    postcode
                FROM
                    (SELECT
                        establishment_id,
                        la_code,
                        establishment_number,
                        establishment_name,
                        establishment_type_code,
                        postcode,
                        ROW_NUMBER() OVER (PARTITION BY la_code, establishment_number, CASE WHEN establishment_number IS NULL THEN postcode ELSE NULL END ORDER BY translate(establishment_status_code::text, '1234', '1324'), urn desc) as row_number
                    FROM
                        establishments) e
                    WHERE
                        e.row_number = 1
            ),
            update_extract_items AS (
                SELECT
                    x.tps_csv_extract_item_id,
                    gen_random_uuid() as person_employment_id,
                    p.person_id,
                    e.establishment_id,
                    x.employment_start_date as start_date,
                    x.employment_end_date as last_known_employed_date,
                    x.employment_type,
                    x.extract_date as last_extract_date,
                    x.key,
                    x.national_insurance_number,
                    x.member_postcode as person_postcode
                FROM
                        tps_csv_extract_items x
                    JOIN
                        persons p ON x.trn = p.trn
                    JOIN
                        unique_establishments e ON x.local_authority_code = e.la_code
                            AND (x.establishment_number = e.establishment_number
                                 OR (e.establishment_type_code = '29' 
                                     AND x.establishment_postcode = e.postcode
                                     AND NOT EXISTS (SELECT
                                                         1
                                                     FROM
                                                         unique_establishments e2
                                                     WHERE
                                                         e2.la_code = x.local_authority_code
                                                         AND e2.establishment_number = x.establishment_number)))
                WHERE
                    x.tps_csv_extract_id = {tpsCsvExtractId}
                    AND x.result IS NULL
                    AND NOT EXISTS (SELECT
                                        1
                                    FROM
                                        person_employments pe
                                    WHERE
                                        pe.key = x.key)
                LIMIT 1000
            ),
            new_person_employments AS (
                UPDATE
                    tps_csv_extract_items x
                SET
                    result = 1
                FROM
                    update_extract_items u
                WHERE
                    x.tps_csv_extract_item_id = u.tps_csv_extract_item_id
                RETURNING
                    u.person_employment_id,
                    u.person_id,
                    u.establishment_id,
                    u.start_date,
                    u.employment_type,
                    u.last_extract_date,
                    u.last_known_employed_date,
                    u.key,
                    u.national_insurance_number,
                    u.person_postcode
            )
            INSERT INTO person_employments
                (
                    person_employment_id,
                    person_id,
                    establishment_id,
                    start_date,
                    employment_type,
                    last_extract_date,
                    last_known_employed_date,
                    key,
                    national_insurance_number,
                    person_postcode,
                    created_on,
                    updated_on
                )
            SELECT
                *,
                {clock.UtcNow} as created_on,
                {clock.UtcNow} as updated_on
            FROM
                new_person_employments
            RETURNING
                person_employment_id,
                person_id,
                establishment_id,
                start_date,
                employment_type,
                last_extract_date,
                last_known_employed_date,
                key,
                national_insurance_number,
                person_postcode,
                created_on,
                updated_on
            """;

        bool hasRecordsToUpdate = false;
        var events = new List<EventBase>();

        do
        {
            hasRecordsToUpdate = false;
            using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            dbContext.Database.UseTransaction(transaction);
            await foreach (var item in dbContext.Database.SqlQuery<NewPersonEmployment>(querySql).AsAsyncEnumerable())
            {
                hasRecordsToUpdate = true;
                var personEmployment = new PersonEmployment
                {
                    PersonEmploymentId = item.PersonEmploymentId,
                    PersonId = item.PersonId,
                    EstablishmentId = item.EstablishmentId,
                    StartDate = item.StartDate,
                    EndDate = null,
                    LastKnownEmployedDate = item.LastKnownEmployedDate,
                    LastExtractDate = item.LastExtractDate,
                    EmploymentType = item.EmploymentType,
                    CreatedOn = item.CreatedOn,
                    UpdatedOn = item.UpdatedOn,
                    Key = item.Key,
                    NationalInsuranceNumber = item.NationalInsuranceNumber,
                    PersonPostcode = item.PersonPostcode
                };

                var createdEvent = new PersonEmploymentCreatedEvent
                {
                    EventId = Guid.NewGuid(),
                    PersonId = item.PersonId,
                    PersonEmployment = Core.Events.Models.PersonEmployment.FromModel(personEmployment),
                    CreatedUtc = clock.UtcNow,
                    RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId
                };

                events.Add(createdEvent);
            }

            await transaction.SaveEvents(events, TempEventsTableSuffix, clock, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            events.Clear();
        }
        while (hasRecordsToUpdate);
    }

    public async Task ProcessUpdatedEmploymentHistory(Guid tpsCsvExtractId, CancellationToken cancellationToken)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        FormattableString querySql =
            $"""
            WITH extract_items AS (
                SELECT
                    x.tps_csv_extract_item_id,
                    pe.person_employment_id,
                    pe.employment_type as current_employment_type,
                    pe.last_known_employed_date as current_last_known_employed_date,
                    pe.last_extract_date as current_last_extract_date,
                    pe.national_insurance_number as current_national_insurance_number,
                    pe.person_postcode as current_person_postcode,
                    x.employment_type as new_employment_type,
                    x.employment_end_date as new_last_known_employed_date,
                    x.extract_date as new_last_extract_date,
                    x.national_insurance_number as new_national_insurance_number,
                    x.member_postcode as new_person_postcode,
                    x.key,
                    CASE WHEN pe.employment_type != x.employment_type OR pe.last_known_employed_date != x.employment_end_date OR pe.last_extract_date != x.extract_date THEN 2 ELSE 0 END as result
                FROM
                        tps_csv_extract_items x
                    JOIN
                        person_employments pe ON pe.key = x.key
                WHERE
                    x.tps_csv_extract_id = {tpsCsvExtractId}
                    AND x.result IS NULL
                LIMIT 1000),
            changes AS (
                UPDATE
                    tps_csv_extract_items x
                SET
                    result = u.result
                FROM
                    extract_items u
                WHERE
                    x.tps_csv_extract_item_id = u.tps_csv_extract_item_id
                RETURNING
                    u.person_employment_id,
                    u.current_employment_type,
                    u.current_last_known_employed_date,
                    u.current_last_extract_date,
                    u.current_national_insurance_number,
                    u.current_person_postcode,
                    u.new_employment_type,
                    u.new_last_known_employed_date,
                    u.new_last_extract_date,
                    u.new_national_insurance_number,
                    u.new_person_postcode
            )
            UPDATE
                person_employments pe
            SET
                employment_type = changes.new_employment_type,
                last_known_employed_date = changes.new_last_known_employed_date,
                last_extract_date = changes.new_last_extract_date,
                national_insurance_number = changes.new_national_insurance_number,
                person_postcode = changes.new_person_postcode,
                updated_on = {clock.UtcNow}
            FROM
                changes
            WHERE
                changes.person_employment_id = pe.person_employment_id
            RETURNING
                pe.person_employment_id,
                pe.person_id,
                pe.establishment_id,
                pe.start_date,
                pe.end_date,
                changes.current_employment_type,
                changes.current_last_known_employed_date,
                changes.current_last_extract_date,
                changes.current_national_insurance_number,
                changes.current_person_postcode,
                changes.new_employment_type,
                changes.new_last_known_employed_date,
                changes.new_last_extract_date,
                changes.new_national_insurance_number,
                changes.new_person_postcode,
                pe.key
            """;

        bool hasRecordsToUpdate = false;
        var events = new List<EventBase>();

        do
        {
            hasRecordsToUpdate = false;
            using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            dbContext.Database.UseTransaction(transaction);
            await foreach (var item in dbContext.Database.SqlQuery<UpdatedPersonEmployment>(querySql).AsAsyncEnumerable())
            {
                hasRecordsToUpdate = true;
                var changes = PersonEmploymentUpdatedEventChanges.None |
                    (item.CurrentEmploymentType != item.NewEmploymentType ? PersonEmploymentUpdatedEventChanges.EmploymentType : PersonEmploymentUpdatedEventChanges.None) |
                    (item.CurrentLastKnownEmployedDate != item.NewLastKnownEmployedDate ? PersonEmploymentUpdatedEventChanges.LastKnownEmployedDate : PersonEmploymentUpdatedEventChanges.None) |
                    (item.CurrentLastExtractDate != item.NewLastExtractDate ? PersonEmploymentUpdatedEventChanges.LastExtractDate : PersonEmploymentUpdatedEventChanges.None) |
                    (item.CurrentNationalInsuranceNumber != item.NewNationalInsuranceNumber ? PersonEmploymentUpdatedEventChanges.NationalInsuranceNumber : PersonEmploymentUpdatedEventChanges.None) |
                    (item.CurrentPersonPostcode != item.NewPersonPostcode ? PersonEmploymentUpdatedEventChanges.PersonPostcode : PersonEmploymentUpdatedEventChanges.None);

                if (changes != PersonEmploymentUpdatedEventChanges.None)
                {
                    var updatedEvent = new PersonEmploymentUpdatedEvent
                    {
                        EventId = Guid.NewGuid(),
                        PersonId = item.PersonEmploymentId,
                        PersonEmployment = new()
                        {
                            PersonEmploymentId = item.PersonEmploymentId,
                            PersonId = item.PersonId,
                            EstablishmentId = item.EstablishmentId,
                            StartDate = item.StartDate,
                            EndDate = item.EndDate,
                            EmploymentType = item.NewEmploymentType,
                            LastKnownEmployedDate = item.NewLastKnownEmployedDate,
                            LastExtractDate = item.NewLastExtractDate,
                            NationalInsuranceNumber = item.NewNationalInsuranceNumber,
                            PersonPostcode = item.NewPersonPostcode,
                            Key = item.Key
                        },
                        OldPersonEmployment = new()
                        {
                            PersonEmploymentId = item.PersonEmploymentId,
                            PersonId = item.PersonId,
                            EstablishmentId = item.EstablishmentId,
                            StartDate = item.StartDate,
                            EndDate = item.EndDate,
                            EmploymentType = item.CurrentEmploymentType,
                            LastKnownEmployedDate = item.CurrentLastKnownEmployedDate,
                            LastExtractDate = item.CurrentLastExtractDate,
                            NationalInsuranceNumber = item.CurrentNationalInsuranceNumber,
                            PersonPostcode = item.CurrentPersonPostcode,
                            Key = item.Key
                        },
                        Changes = changes,
                        CreatedUtc = clock.UtcNow,
                        RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId
                    };

                    events.Add(updatedEvent);
                }
            }

            await transaction.SaveEvents(events, TempEventsTableSuffix, clock, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            events.Clear();
        }
        while (hasRecordsToUpdate);
    }

    public async Task UpdateLatestEstablishmentVersions(CancellationToken cancellationToken)
    {
        using var readDbContext = dbContextFactory.CreateDbContext();
        readDbContext.Database.SetCommandTimeout(5400);
        using var writeDbContext = dbContextFactory.CreateDbContext();
        var connection = (NpgsqlConnection)writeDbContext.Database.GetDbConnection();
        await connection.OpenAsync(CancellationToken.None);

        FormattableString querySql =
            $"""
            WITH unique_establishments AS (
                SELECT
                    establishment_id,
                    la_code,
                    establishment_number,
                    establishment_name,
                    establishment_type_code,
                    postcode
                FROM
                    (SELECT
                        establishment_id,
                        la_code,
                        establishment_number,
                        establishment_name,
                        establishment_type_code,
                        postcode,
                        ROW_NUMBER() OVER (PARTITION BY la_code, establishment_number, CASE WHEN establishment_number IS NULL THEN postcode ELSE NULL END ORDER BY translate(COALESCE(establishment_status_code, 1)::text, '1234', '1324'), urn desc) as row_number
                    FROM
                        establishments) e
                    WHERE
                        e.row_number = 1
            ),
            establishment_changes AS (
                SELECT
                    *
                FROM
                    person_employments pe
                WHERE
                    NOT EXISTS (SELECT
                                    1
                                FROM
                                    unique_establishments e
                                WHERE
                                    e.establishment_id = pe.establishment_id)
            )
            select
                ec.person_employment_id,
                ec.person_id,
                ec.establishment_id as current_establishment_id,
                ec.start_date,
                ec.end_date,
                ec.employment_type,
                ec.last_known_employed_date,
                ec.last_extract_date,
                ec.national_insurance_number,
                ec.person_postcode,
                ec.key,                
                ue.establishment_id
            from
                    establishment_changes ec
                JOIN
                    establishments e ON e.establishment_id = ec.establishment_id
                JOIN
                    unique_establishments ue ON ue.la_code = e.la_code
                        AND (ue.establishment_number = e.establishment_number
                             OR (e.establishment_type_code = '29' 
                                 AND ue.postcode = e.postcode
                                 AND NOT EXISTS (SELECT
                                                    1
                                                 FROM
                                                    unique_establishments e2
                                                 WHERE
                                                    e2.la_code = e.la_code
                                                    AND e2.establishment_number = e.establishment_number)))
            """;

        var batchCommands = new List<NpgsqlBatchCommand>();

        await foreach (var item in readDbContext.Database.SqlQuery<UpdatedPersonEmploymentEstablishment>(querySql).AsAsyncEnumerable())
        {
            var updatePersonEmploymentsCommand = new NpgsqlBatchCommand(
                $"""
                    UPDATE
                        person_employments
                    SET
                        establishment_id = '{item.EstablishmentId}',
                        updated_on = now()
                    WHERE
                        person_employment_id = '{item.PersonEmploymentId}'
                    """);

            batchCommands.Add(updatePersonEmploymentsCommand);
            writeDbContext.AddEvent(new PersonEmploymentUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = item.PersonEmploymentId,
                PersonEmployment = new()
                {
                    PersonEmploymentId = item.PersonEmploymentId,
                    PersonId = item.PersonId,
                    EstablishmentId = item.EstablishmentId,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    EmploymentType = item.EmploymentType,
                    LastKnownEmployedDate = item.LastKnownEmployedDate,
                    LastExtractDate = item.LastExtractDate,
                    NationalInsuranceNumber = item.NationalInsuranceNumber,
                    PersonPostcode = item.PersonPostcode,
                    Key = item.Key
                },
                OldPersonEmployment = new()
                {
                    PersonEmploymentId = item.PersonEmploymentId,
                    PersonId = item.PersonId,
                    EstablishmentId = item.CurrentEstablishmentId,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    EmploymentType = item.EmploymentType,
                    LastKnownEmployedDate = item.LastKnownEmployedDate,
                    LastExtractDate = item.LastExtractDate,
                    NationalInsuranceNumber = item.NationalInsuranceNumber,
                    PersonPostcode = item.PersonPostcode,
                    Key = item.Key
                },
                Changes = PersonEmploymentUpdatedEventChanges.EstablishmentId,
                CreatedUtc = clock.UtcNow,
                RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId
            });

            if (batchCommands.Count == 50)
            {
                await SaveChanges();
            }
        }

        if (batchCommands.Any())
        {
            await SaveChanges();
        }

        async Task SaveChanges()
        {
            if (writeDbContext.ChangeTracker.HasChanges())
            {
                await writeDbContext.SaveChangesAsync(cancellationToken);
                writeDbContext.ChangeTracker.Clear();
            }

            if (batchCommands.Count > 0)
            {
                using var batch = new NpgsqlBatch(connection);
                foreach (var command in batchCommands)
                {
                    batch.BatchCommands.Add(command);
                }

                await batch.ExecuteNonQueryAsync(cancellationToken);
                batchCommands.Clear();
            }
        }
    }

    public async Task ProcessEndedEmployments(CancellationToken cancellationToken)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        FormattableString querySql =
            $"""
            WITH changes AS (
               SELECT
                    person_employment_id,
                    end_date as current_end_date,
                    last_known_employed_date as new_end_date
                FROM
                    person_employments
                WHERE
                    end_date IS NULL
                    AND AGE(last_extract_date, last_known_employed_date) > INTERVAL '5 months'
                LIMIT 1000
            )
            UPDATE
                person_employments pe
            SET
                end_date = new_end_date,
                updated_on = {clock.UtcNow}
            FROM
                changes 
            WHERE
                pe.person_employment_id = changes.person_employment_id
            RETURNING
                pe.person_employment_id,
                pe.person_id,
                pe.establishment_id,
                pe.start_date,
                changes.current_end_date,
                pe.employment_type,
                pe.last_known_employed_date,
                pe.last_extract_date,
                pe.national_insurance_number,
                pe.person_postcode,
                pe.key,
                changes.new_end_date
            """;

        bool hasRecordsToUpdate = false;
        var events = new List<EventBase>();

        do
        {
            hasRecordsToUpdate = false;
            using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            dbContext.Database.UseTransaction(transaction);
            await foreach (var item in dbContext.Database.SqlQuery<UpdatedPersonEmploymentEndDate>(querySql).AsAsyncEnumerable())
            {
                hasRecordsToUpdate = true;
                var updatedEvent = new PersonEmploymentUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    PersonId = item.PersonEmploymentId,
                    PersonEmployment = new()
                    {
                        PersonEmploymentId = item.PersonEmploymentId,
                        PersonId = item.PersonId,
                        EstablishmentId = item.EstablishmentId,
                        StartDate = item.StartDate,
                        EndDate = item.CurrentEndDate,
                        EmploymentType = item.EmploymentType,
                        LastKnownEmployedDate = item.LastKnownEmployedDate,
                        LastExtractDate = item.LastExtractDate,
                        NationalInsuranceNumber = item.NationalInsuranceNumber,
                        PersonPostcode = item.PersonPostcode,
                        Key = item.Key
                    },
                    OldPersonEmployment = new()
                    {
                        PersonEmploymentId = item.PersonEmploymentId,
                        PersonId = item.PersonId,
                        EstablishmentId = item.EstablishmentId,
                        StartDate = item.StartDate,
                        EndDate = item.NewEndDate,
                        EmploymentType = item.EmploymentType,
                        LastKnownEmployedDate = item.LastKnownEmployedDate,
                        LastExtractDate = item.LastExtractDate,
                        NationalInsuranceNumber = item.NationalInsuranceNumber,
                        PersonPostcode = item.PersonPostcode,
                        Key = item.Key
                    },
                    Changes = PersonEmploymentUpdatedEventChanges.EndDate,
                    CreatedUtc = clock.UtcNow,
                    RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId
                };

                events.Add(updatedEvent);
            }

            await transaction.SaveEvents(events, TempEventsTableSuffix, clock, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            events.Clear();
        }
        while (hasRecordsToUpdate);
    }

    public async Task BackfillNinoAndPersonPostcodeInEmploymentHistory(CancellationToken cancellationToken)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        FormattableString querySql =
            $"""
            WITH changes AS (
                SELECT
                    pe.person_employment_id,
                    pe.national_insurance_number as current_national_insurance_number,
                    pe.person_postcode as current_person_postcode,
                    x.national_insurance_number as new_national_insurance_number,
                    x.member_postcode as new_person_postcode		
                FROM
                        tps_csv_extract_items x
                    JOIN
                        person_employments pe ON pe.key = x.key
                                                 AND pe.last_extract_date = x.extract_date
                WHERE
                    pe.national_insurance_number IS NULL
                LIMIT 1000)
                UPDATE
                    person_employments pe
                SET
                    national_insurance_number = changes.new_national_insurance_number,
                    person_postcode = changes.new_person_postcode,
                    updated_on = {clock.UtcNow}
                FROM
                    changes
                WHERE
                    changes.person_employment_id = pe.person_employment_id
                RETURNING
                    pe.person_employment_id,
                    pe.person_id,
                    pe.establishment_id,
                    pe.start_date,
                    pe.end_date,
                    pe.employment_type,
                    pe.last_known_employed_date,
                    pe.last_extract_date,
                    changes.current_national_insurance_number,
                    changes.current_person_postcode,
                    changes.new_national_insurance_number,
                    changes.new_person_postcode,
                    pe.key
            """;

        bool hasRecordsToUpdate = false;
        var events = new List<EventBase>();

        do
        {
            hasRecordsToUpdate = false;
            using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            dbContext.Database.UseTransaction(transaction);
            await foreach (var item in dbContext.Database.SqlQuery<UpdatedPersonEmploymentNationalInsuranceNumberAndPersonPostcode>(querySql).AsAsyncEnumerable())
            {
                hasRecordsToUpdate = true;
                var changes = PersonEmploymentUpdatedEventChanges.None |
                    (item.CurrentNationalInsuranceNumber != item.NewNationalInsuranceNumber ? PersonEmploymentUpdatedEventChanges.NationalInsuranceNumber : PersonEmploymentUpdatedEventChanges.None) |
                    (item.CurrentPersonPostcode != item.NewPersonPostcode ? PersonEmploymentUpdatedEventChanges.PersonPostcode : PersonEmploymentUpdatedEventChanges.None);

                if (changes != PersonEmploymentUpdatedEventChanges.None)
                {
                    var updatedEvent = new PersonEmploymentUpdatedEvent
                    {
                        EventId = Guid.NewGuid(),
                        PersonId = item.PersonEmploymentId,
                        PersonEmployment = new()
                        {
                            PersonEmploymentId = item.PersonEmploymentId,
                            PersonId = item.PersonId,
                            EstablishmentId = item.EstablishmentId,
                            StartDate = item.StartDate,
                            EndDate = item.EndDate,
                            EmploymentType = item.EmploymentType,
                            LastKnownEmployedDate = item.LastKnownEmployedDate,
                            LastExtractDate = item.LastExtractDate,
                            NationalInsuranceNumber = item.NewNationalInsuranceNumber,
                            PersonPostcode = item.NewPersonPostcode,
                            Key = item.Key
                        },
                        OldPersonEmployment = new()
                        {
                            PersonEmploymentId = item.PersonEmploymentId,
                            PersonId = item.PersonId,
                            EstablishmentId = item.EstablishmentId,
                            StartDate = item.StartDate,
                            EndDate = item.EndDate,
                            EmploymentType = item.EmploymentType,
                            LastKnownEmployedDate = item.LastKnownEmployedDate,
                            LastExtractDate = item.LastExtractDate,
                            NationalInsuranceNumber = item.CurrentNationalInsuranceNumber,
                            PersonPostcode = item.CurrentPersonPostcode,
                            Key = item.Key
                        },
                        Changes = changes,
                        CreatedUtc = clock.UtcNow,
                        RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId
                    };

                    events.Add(updatedEvent);
                }
            }

            await transaction.SaveEvents(events, TempEventsTableSuffix, clock, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            events.Clear();
        }
        while (hasRecordsToUpdate);
    }
}
