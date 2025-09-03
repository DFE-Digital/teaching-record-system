using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.WorkforceData;

public class TpsCsvExtractProcessor(
    IDbContextFactory<TrsDbContext> dbContextFactory,
    IClock clock)
{
    private const string TempEventsTableSuffix = "tps_extract_events";

    public async Task ProcessNonMatchingTrnsAsync(Guid tpsCsvExtractId, CancellationToken cancellationToken)
    {
        int i = 0;
        using var dbContext = dbContextFactory.CreateDbContext();
        var invalidTrns = await dbContext.TpsCsvExtractItems
            .Where(r => r.TpsCsvExtractId == tpsCsvExtractId && !dbContext.Persons.IgnoreQueryFilters().Any(p => p.Trn == r.Trn))
            .ToListAsync();

        foreach (var item in invalidTrns)
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

    public async Task ProcessNonMatchingEstablishmentsAsync(Guid tpsCsvExtractId, CancellationToken cancellationToken)
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

    public async Task ProcessNewEmploymentHistoryAsync(Guid tpsCsvExtractId, CancellationToken cancellationToken)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        dbContext.Database.SetCommandTimeout(300);
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
                    gen_random_uuid() as tps_employment_id,
                    p.person_id,
                    e.establishment_id,
                    x.employment_start_date as start_date,
                    CASE WHEN x.withdrawal_indicator = 'W' OR AGE(x.extract_date, least(x.employment_end_date, x.extract_date)) > INTERVAL '5 months' THEN x.employment_end_date ELSE NULL END as end_date,
                    least(x.employment_end_date, x.extract_date) as last_known_tps_employed_date,
                    x.employment_type,
                    CASE WHEN x.withdrawal_indicator = 'W' THEN TRUE ELSE FALSE END as withdrawal_confirmed,
                    x.extract_date as last_extract_date,
                    x.key,
                    x.national_insurance_number,
                    x.member_postcode as person_postcode,
                    x.member_email_address as person_email_address,
                    x.establishment_postcode as employer_postcode,
                    x.establishment_email_address as employer_email_address
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
                                        tps_employments te
                                    WHERE
                                        te.key = x.key)
                LIMIT 1000
            ),
            new_tps_employments AS (
                UPDATE
                    tps_csv_extract_items x
                SET
                    result = 1
                FROM
                    update_extract_items u
                WHERE
                    x.tps_csv_extract_item_id = u.tps_csv_extract_item_id
                RETURNING
                    u.tps_employment_id,
                    u.person_id,
                    u.establishment_id,
                    u.start_date,
                    u.end_date,
                    u.last_known_tps_employed_date,
                    u.employment_type,
                    u.withdrawal_confirmed,
                    u.last_extract_date,                    
                    u.key,
                    u.national_insurance_number,
                    u.person_postcode,
                    u.person_email_address,
                    u.employer_postcode,
                    u.employer_email_address
            )
            INSERT INTO tps_employments
                (
                    tps_employment_id,
                    person_id,
                    establishment_id,
                    start_date,
                    end_date,
                    last_known_tps_employed_date,
                    employment_type,
                    withdrawal_confirmed,
                    last_extract_date,                 
                    key,
                    national_insurance_number,
                    person_postcode,
                    person_email_address,
                    employer_postcode,
                    employer_email_address,
                    created_on,
                    updated_on
                )
            SELECT
                *,
                {clock.UtcNow} as created_on,
                {clock.UtcNow} as updated_on
            FROM
                new_tps_employments
            RETURNING
                tps_employment_id,
                person_id,
                establishment_id,
                start_date,
                end_date,
                last_known_tps_employed_date,
                employment_type,
                withdrawal_confirmed,
                last_extract_date,
                key,
                national_insurance_number,
                person_postcode,
                person_email_address,
                employer_postcode,
                employer_email_address,
                created_on,
                updated_on
            """;

        bool hasRecordsToUpdate = false;
        var events = new List<EventBase>();

        do
        {
            hasRecordsToUpdate = false;
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            dbContext.Database.UseTransaction(transaction);
            await foreach (var item in dbContext.Database.SqlQuery<NewTpsEmployment>(querySql).AsAsyncEnumerable())
            {
                hasRecordsToUpdate = true;
                var personEmployment = new TpsEmployment
                {
                    TpsEmploymentId = item.TpsEmploymentId,
                    PersonId = item.PersonId,
                    EstablishmentId = item.EstablishmentId,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    LastKnownTpsEmployedDate = item.LastKnownTpsEmployedDate,
                    EmploymentType = item.EmploymentType,
                    WithdrawalConfirmed = item.WithdrawalConfirmed,
                    LastExtractDate = item.LastExtractDate,
                    CreatedOn = item.CreatedOn,
                    UpdatedOn = item.UpdatedOn,
                    Key = item.Key,
                    NationalInsuranceNumber = item.NationalInsuranceNumber,
                    PersonPostcode = item.PersonPostcode,
                    PersonEmailAddress = item.PersonEmailAddress,
                    EmployerPostcode = item.EmployerPostcode,
                    EmployerEmailAddress = item.EmployerEmailAddress
                };

                var createdEvent = new TpsEmploymentCreatedEvent
                {
                    EventId = Guid.NewGuid(),
                    PersonId = item.PersonId,
                    TpsEmployment = Core.Events.Models.TpsEmployment.FromModel(personEmployment),
                    CreatedUtc = clock.UtcNow,
                    RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId
                };

                events.Add(createdEvent);
            }

            await transaction.SaveEventsAsync(events, TempEventsTableSuffix, clock, cancellationToken, 120);
            await transaction.CommitAsync(cancellationToken);
            events.Clear();
        }
        while (hasRecordsToUpdate);
    }

    public async Task ProcessUpdatedEmploymentHistoryAsync(Guid tpsCsvExtractId, CancellationToken cancellationToken)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        dbContext.Database.SetCommandTimeout(300);
        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        FormattableString querySql =
            $"""
            WITH extract_items AS (
                SELECT
                    x.tps_csv_extract_item_id,
                    te.tps_employment_id,
                    te.employment_type as current_employment_type,
                    te.end_date as current_end_date,
                    te.last_known_tps_employed_date as current_last_known_tps_employed_date,
                    te.withdrawal_confirmed as current_withdrawal_confirmed,
                    te.last_extract_date as current_last_extract_date,
                    te.national_insurance_number as current_national_insurance_number,
                    te.person_postcode as current_person_postcode,
                    te.person_email_address as current_person_email_address,
                    te.employer_postcode as current_employer_postcode,
                    te.employer_email_address as current_employer_email_address,
                    x.employment_type as new_employment_type,
                    CASE WHEN x.withdrawal_indicator = 'W' OR AGE(x.extract_date, least(x.employment_end_date, x.extract_date)) > INTERVAL '5 months' THEN x.employment_end_date ELSE NULL END as new_end_date,
                    least(x.employment_end_date, x.extract_date) as new_last_known_tps_employed_date,
                    CASE WHEN x.withdrawal_indicator = 'W' THEN TRUE ELSE FALSE END as new_withdrawal_confirmed,
                    x.extract_date as new_last_extract_date,
                    x.national_insurance_number as new_national_insurance_number,
                    x.member_postcode as new_person_postcode,
                    x.member_email_address as new_person_email_address,
                    x.establishment_postcode as new_employer_postcode,
                    x.establishment_email_address as new_employer_email_address,
                    x.key,
                    CASE WHEN te.employment_type != x.employment_type OR te.last_known_tps_employed_date != least(x.employment_end_date, x.extract_date) OR te.last_extract_date != x.extract_date THEN 2 ELSE 0 END as result
                FROM
                        tps_csv_extract_items x
                    JOIN
                        tps_employments te ON te.key = x.key
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
                    u.tps_employment_id,                    
                    u.current_end_date,
                    u.current_last_known_tps_employed_date,
                    u.current_employment_type,
                    u.current_withdrawal_confirmed,
                    u.current_last_extract_date,
                    u.current_national_insurance_number,
                    u.current_person_postcode,
                    u.current_person_email_address,
                    u.current_employer_postcode,
                    u.current_employer_email_address,
                    u.new_end_date,
                    u.new_last_known_tps_employed_date,
                    u.new_employment_type,
                    u.new_withdrawal_confirmed,
                    u.new_last_extract_date,
                    u.new_national_insurance_number,
                    u.new_person_postcode,
                    u.new_person_email_address,
                    u.new_employer_postcode,
                    u.new_employer_email_address
            )
            UPDATE
                tps_employments te
            SET
                end_date = changes.new_end_date,
                last_known_tps_employed_date = changes.new_last_known_tps_employed_date,
                employment_type = changes.new_employment_type,
                withdrawal_confirmed = changes.new_withdrawal_confirmed,
                last_extract_date = changes.new_last_extract_date,
                national_insurance_number = changes.new_national_insurance_number,
                person_postcode = changes.new_person_postcode,
                person_email_address = changes.new_person_email_address,
                employer_postcode = changes.new_employer_postcode,
                employer_email_address = changes.new_employer_email_address,
                updated_on = {clock.UtcNow}
            FROM
                changes
            WHERE
                changes.tps_employment_id = te.tps_employment_id
            RETURNING
                te.tps_employment_id,
                te.person_id,
                te.establishment_id,
                te.start_date,
                changes.current_end_date,
                changes.current_last_known_tps_employed_date,
                changes.current_employment_type,            
                changes.current_withdrawal_confirmed,
                changes.current_last_extract_date,
                changes.current_national_insurance_number,
                changes.current_person_postcode,
                changes.current_person_email_address,
                changes.current_employer_postcode,
                changes.current_employer_email_address,
                changes.new_end_date,
                changes.new_last_known_tps_employed_date,
                changes.new_employment_type,
                changes.new_withdrawal_confirmed,
                changes.new_last_extract_date,
                changes.new_national_insurance_number,
                changes.new_person_postcode,
                changes.new_person_email_address,
                changes.new_employer_postcode,
                changes.new_employer_email_address,
                te.key
            """;

        bool hasRecordsToUpdate = false;
        var events = new List<EventBase>();

        do
        {
            hasRecordsToUpdate = false;
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            dbContext.Database.UseTransaction(transaction);
            await foreach (var item in dbContext.Database.SqlQuery<UpdatedTpsEmployment>(querySql).AsAsyncEnumerable())
            {
                hasRecordsToUpdate = true;
                var changes = TpsEmploymentUpdatedEventChanges.None |
                    (item.CurrentEmploymentType != item.NewEmploymentType ? TpsEmploymentUpdatedEventChanges.EmploymentType : TpsEmploymentUpdatedEventChanges.None) |
                    (item.CurrentEndDate != item.NewEndDate ? TpsEmploymentUpdatedEventChanges.EndDate : TpsEmploymentUpdatedEventChanges.None) |
                    (item.CurrentLastKnownTpsEmployedDate != item.NewLastKnownTpsEmployedDate ? TpsEmploymentUpdatedEventChanges.LastKnownTpsEmployedDate : TpsEmploymentUpdatedEventChanges.None) |
                    (item.CurrentWithdrawalConfirmed != item.NewWithdrawalConfirmed ? TpsEmploymentUpdatedEventChanges.WithdrawalConfirmed : TpsEmploymentUpdatedEventChanges.None) |
                    (item.CurrentLastExtractDate != item.NewLastExtractDate ? TpsEmploymentUpdatedEventChanges.LastExtractDate : TpsEmploymentUpdatedEventChanges.None) |
                    (item.CurrentNationalInsuranceNumber != item.NewNationalInsuranceNumber ? TpsEmploymentUpdatedEventChanges.NationalInsuranceNumber : TpsEmploymentUpdatedEventChanges.None) |
                    (item.CurrentPersonPostcode != item.NewPersonPostcode ? TpsEmploymentUpdatedEventChanges.PersonPostcode : TpsEmploymentUpdatedEventChanges.None) |
                    (item.CurrentPersonEmailAddress != item.NewPersonEmailAddress ? TpsEmploymentUpdatedEventChanges.PersonEmailAddress : TpsEmploymentUpdatedEventChanges.None) |
                    (item.CurrentEmployerPostcode != item.NewEmployerPostcode ? TpsEmploymentUpdatedEventChanges.EmployerPostcode : TpsEmploymentUpdatedEventChanges.None) |
                    (item.CurrentEmployerEmailAddress != item.NewEmployerEmailAddress ? TpsEmploymentUpdatedEventChanges.EmployerEmailAddress : TpsEmploymentUpdatedEventChanges.None);

                if (changes != TpsEmploymentUpdatedEventChanges.None)
                {
                    var updatedEvent = new TpsEmploymentUpdatedEvent
                    {
                        EventId = Guid.NewGuid(),
                        PersonId = item.PersonId,
                        TpsEmployment = new()
                        {
                            PersonEmploymentId = item.TpsEmploymentId,
                            PersonId = item.PersonId,
                            EstablishmentId = item.EstablishmentId,
                            StartDate = item.StartDate,
                            EndDate = item.NewEndDate,
                            LastKnownTpsEmployedDate = item.NewLastKnownTpsEmployedDate,
                            EmploymentType = item.NewEmploymentType,
                            WithdrawalConfirmed = item.NewWithdrawalConfirmed,
                            LastExtractDate = item.NewLastExtractDate,
                            NationalInsuranceNumber = item.NewNationalInsuranceNumber,
                            PersonPostcode = item.NewPersonPostcode,
                            PersonEmailAddress = item.NewPersonEmailAddress,
                            EmployerPostcode = item.NewEmployerPostcode,
                            EmployerEmailAddress = item.NewEmployerEmailAddress,
                            Key = item.Key
                        },
                        OldTpsEmployment = new()
                        {
                            PersonEmploymentId = item.TpsEmploymentId,
                            PersonId = item.PersonId,
                            EstablishmentId = item.EstablishmentId,
                            StartDate = item.StartDate,
                            EndDate = item.CurrentEndDate,
                            LastKnownTpsEmployedDate = item.CurrentLastKnownTpsEmployedDate,
                            EmploymentType = item.CurrentEmploymentType,
                            WithdrawalConfirmed = item.CurrentWithdrawalConfirmed,
                            LastExtractDate = item.CurrentLastExtractDate,
                            NationalInsuranceNumber = item.CurrentNationalInsuranceNumber,
                            PersonPostcode = item.CurrentPersonPostcode,
                            PersonEmailAddress = item.CurrentPersonEmailAddress,
                            EmployerPostcode = item.CurrentEmployerPostcode,
                            EmployerEmailAddress = item.CurrentEmployerEmailAddress,
                            Key = item.Key
                        },
                        Changes = changes,
                        CreatedUtc = clock.UtcNow,
                        RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId
                    };

                    events.Add(updatedEvent);
                }
            }

            await transaction.SaveEventsAsync(events, TempEventsTableSuffix, clock, cancellationToken, 120);
            await transaction.CommitAsync(cancellationToken);
            events.Clear();
        }
        while (hasRecordsToUpdate);
    }

    public async Task UpdateLatestEstablishmentVersionsAsync(CancellationToken cancellationToken)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        dbContext.Database.SetCommandTimeout(300);
        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
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
                    te.tps_employment_id,
                    e.establishment_id as current_establishment_id,
                    e.establishment_number,
                    e.la_code,
                    e.establishment_type_code,
                    e.postcode
                FROM
                        tps_employments te
                    JOIN
                        establishments e ON e.establishment_id = te.establishment_id
                WHERE
                    NOT EXISTS (SELECT
                                    1
                                FROM
                                    unique_establishments e
                                WHERE
                                    e.establishment_id = te.establishment_id)
                LIMIT 1000
            )
            UPDATE
                tps_employments te
            SET
                establishment_id = changes.new_establishment_id,
                updated_on = {clock.UtcNow}
            FROM                
                (SELECT
                    ec.tps_employment_id,
                    ec.current_establishment_id,
                    ue.establishment_id as new_establishment_id				 
                FROM
                        establishment_changes ec                
                    JOIN
                        unique_establishments ue ON ue.la_code = ec.la_code
                            AND (ue.establishment_number = ec.establishment_number
                                OR (ec.establishment_type_code = '29' 
                                    AND ue.postcode = ec.postcode
                                    AND NOT EXISTS (SELECT
                                                        1
                                                    FROM
                                                        unique_establishments e2
                                                    WHERE
                                                        e2.la_code = ec.la_code
                                                        AND e2.establishment_number = ec.establishment_number)))) changes
            WHERE
                te.tps_employment_id = changes.tps_employment_id
            RETURNING
                te.tps_employment_id,
                te.person_id,
                changes.current_establishment_id,
                te.start_date,
                te.end_date,
                te.employment_type,
                te.withdrawal_confirmed,
                te.last_known_tps_employed_date,
                te.last_extract_date,
                te.national_insurance_number,
                te.person_postcode,
                te.person_email_address,
                te.employer_postcode,
                te.employer_email_address,
                te.key,
                changes.new_establishment_id            
            """;

        bool hasRecordsToUpdate = false;
        var events = new List<EventBase>();

        do
        {
            hasRecordsToUpdate = false;
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            dbContext.Database.UseTransaction(transaction);
            await foreach (var item in dbContext.Database.SqlQuery<UpdatedTpsEmploymentEstablishment>(querySql).AsAsyncEnumerable())
            {
                var updatedEvent = new TpsEmploymentUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    PersonId = item.PersonId,
                    TpsEmployment = new()
                    {
                        PersonEmploymentId = item.TpsEmploymentId,
                        PersonId = item.PersonId,
                        EstablishmentId = item.NewEstablishmentId,
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,
                        LastKnownTpsEmployedDate = item.LastKnownTpsEmployedDate,
                        WithdrawalConfirmed = item.WithdrawalConfirmed,
                        EmploymentType = item.EmploymentType,
                        LastExtractDate = item.LastExtractDate,
                        NationalInsuranceNumber = item.NationalInsuranceNumber,
                        PersonPostcode = item.PersonPostcode,
                        PersonEmailAddress = item.PersonEmailAddress,
                        EmployerPostcode = item.EmployerPostcode,
                        EmployerEmailAddress = item.EmployerEmailAddress,
                        Key = item.Key
                    },
                    OldTpsEmployment = new()
                    {
                        PersonEmploymentId = item.TpsEmploymentId,
                        PersonId = item.PersonId,
                        EstablishmentId = item.CurrentEstablishmentId,
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,
                        LastKnownTpsEmployedDate = item.LastKnownTpsEmployedDate,
                        EmploymentType = item.EmploymentType,
                        WithdrawalConfirmed = item.WithdrawalConfirmed,
                        LastExtractDate = item.LastExtractDate,
                        NationalInsuranceNumber = item.NationalInsuranceNumber,
                        PersonPostcode = item.PersonPostcode,
                        PersonEmailAddress = item.PersonEmailAddress,
                        EmployerPostcode = item.EmployerPostcode,
                        EmployerEmailAddress = item.EmployerEmailAddress,
                        Key = item.Key
                    },
                    Changes = TpsEmploymentUpdatedEventChanges.EstablishmentId,
                    CreatedUtc = clock.UtcNow,
                    RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId
                };

                events.Add(updatedEvent);
            }

            await transaction.SaveEventsAsync(events, TempEventsTableSuffix, clock, cancellationToken, 120);
            await transaction.CommitAsync(cancellationToken);
            events.Clear();
        }
        while (hasRecordsToUpdate);
    }

    public async Task ProcessEndedEmploymentsAsync(CancellationToken cancellationToken)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        dbContext.Database.SetCommandTimeout(300);
        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        FormattableString querySql =
            $"""
            WITH changes AS (
               SELECT
                    tps_employment_id,
                    end_date as current_end_date,
                    last_known_tps_employed_date as new_end_date
                FROM
                    tps_employments
                WHERE
                    end_date IS NULL
                    AND AGE(last_extract_date, last_known_tps_employed_date) > INTERVAL '5 months'
                LIMIT 1000
            )
            UPDATE
                tps_employments te
            SET
                end_date = new_end_date,
                updated_on = {clock.UtcNow}
            FROM
                changes 
            WHERE
                te.tps_employment_id = changes.tps_employment_id
            RETURNING
                te.tps_employment_id,
                te.person_id,
                te.establishment_id,
                te.start_date,
                changes.current_end_date,
                te.employment_type,
                te.withdrawal_confirmed,
                te.last_known_tps_employed_date,
                te.last_extract_date,
                te.national_insurance_number,
                te.person_postcode,
                te.person_email_address,
                te.employer_postcode,
                te.employer_email_address,
                te.key,
                changes.new_end_date
            """;

        bool hasRecordsToUpdate = false;
        var events = new List<EventBase>();

        do
        {
            hasRecordsToUpdate = false;
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            dbContext.Database.UseTransaction(transaction);
            await foreach (var item in dbContext.Database.SqlQuery<UpdatedTpsEmploymentEndDate>(querySql).AsAsyncEnumerable())
            {
                hasRecordsToUpdate = true;
                var updatedEvent = new TpsEmploymentUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    PersonId = item.PersonId,
                    TpsEmployment = new()
                    {
                        PersonEmploymentId = item.TpsEmploymentId,
                        PersonId = item.PersonId,
                        EstablishmentId = item.EstablishmentId,
                        StartDate = item.StartDate,
                        EndDate = item.CurrentEndDate,
                        LastKnownTpsEmployedDate = item.LastKnownTpsEmployedDate,
                        EmploymentType = item.EmploymentType,
                        WithdrawalConfirmed = item.WithdrawalConfirmed,
                        LastExtractDate = item.LastExtractDate,
                        NationalInsuranceNumber = item.NationalInsuranceNumber,
                        PersonPostcode = item.PersonPostcode,
                        PersonEmailAddress = item.PersonEmailAddress,
                        EmployerPostcode = item.EmployerPostcode,
                        EmployerEmailAddress = item.EmployerEmailAddress,
                        Key = item.Key
                    },
                    OldTpsEmployment = new()
                    {
                        PersonEmploymentId = item.TpsEmploymentId,
                        PersonId = item.PersonId,
                        EstablishmentId = item.EstablishmentId,
                        StartDate = item.StartDate,
                        EndDate = item.NewEndDate,
                        LastKnownTpsEmployedDate = item.LastKnownTpsEmployedDate,
                        EmploymentType = item.EmploymentType,
                        WithdrawalConfirmed = item.WithdrawalConfirmed,
                        LastExtractDate = item.LastExtractDate,
                        NationalInsuranceNumber = item.NationalInsuranceNumber,
                        PersonPostcode = item.PersonPostcode,
                        PersonEmailAddress = item.PersonEmailAddress,
                        EmployerPostcode = item.EmployerPostcode,
                        EmployerEmailAddress = item.EmployerEmailAddress,
                        Key = item.Key
                    },
                    Changes = TpsEmploymentUpdatedEventChanges.EndDate,
                    CreatedUtc = clock.UtcNow,
                    RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId
                };

                events.Add(updatedEvent);
            }

            await transaction.SaveEventsAsync(events, TempEventsTableSuffix, clock, cancellationToken, 120);
            await transaction.CommitAsync(cancellationToken);
            events.Clear();
        }
        while (hasRecordsToUpdate);
    }

    public async Task BackfillEmployerEmailAddressInEmploymentHistoryAsync(CancellationToken cancellationToken)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        FormattableString querySql =
            $"""
            WITH changes AS (
                SELECT
                    te.tps_employment_id,
                    te.employer_email_address as current_employer_email_address,
                    x.establishment_email_address as new_employer_email_address
                FROM
                        tps_csv_extract_items x
                    JOIN
                        tps_employments te ON te.key = x.key
                                              AND te.last_extract_date = x.extract_date
                WHERE
                    te.employer_email_address IS NULL
                    AND x.establishment_email_address IS NOT NULL
                LIMIT 1000)
                UPDATE
                    tps_employments te
                SET
                    employer_email_address = changes.new_employer_email_address,
                    updated_on = {clock.UtcNow}
                FROM
                    changes
                WHERE
                    changes.tps_employment_id = te.tps_employment_id
                RETURNING
                    te.tps_employment_id,
                    te.person_id,
                    te.establishment_id,
                    te.start_date,
                    te.end_date,
                    te.employment_type,
                    te.withdrawal_confirmed,
                    te.last_known_tps_employed_date,
                    te.last_extract_date,
                    te.national_insurance_number,
                    te.person_postcode,
                    te.person_email_address,
                    te.employer_postcode,
                    changes.current_employer_email_address,
                    changes.new_employer_email_address,
                    te.key
            """;

        bool hasRecordsToUpdate = false;
        var events = new List<EventBase>();

        do
        {
            hasRecordsToUpdate = false;
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            dbContext.Database.UseTransaction(transaction);
            await foreach (var item in dbContext.Database.SqlQuery<UpdatedTpsEmploymentEmployerEmailAddress>(querySql).AsAsyncEnumerable())
            {
                hasRecordsToUpdate = true;
                var changes = TpsEmploymentUpdatedEventChanges.None |
                    (item.CurrentEmployerEmailAddress != item.NewEmployerEmailAddress ? TpsEmploymentUpdatedEventChanges.EmployerEmailAddress : TpsEmploymentUpdatedEventChanges.None);

                if (changes != TpsEmploymentUpdatedEventChanges.None)
                {
                    var updatedEvent = new TpsEmploymentUpdatedEvent
                    {
                        EventId = Guid.NewGuid(),
                        PersonId = item.PersonId,
                        TpsEmployment = new()
                        {
                            PersonEmploymentId = item.TpsEmploymentId,
                            PersonId = item.PersonId,
                            EstablishmentId = item.EstablishmentId,
                            StartDate = item.StartDate,
                            EndDate = item.EndDate,
                            LastKnownTpsEmployedDate = item.LastKnownTpsEmployedDate,
                            EmploymentType = item.EmploymentType,
                            WithdrawalConfirmed = item.WithdrawalConfirmed,
                            LastExtractDate = item.LastExtractDate,
                            NationalInsuranceNumber = item.NationalInsuranceNumber,
                            PersonPostcode = item.PersonPostcode,
                            PersonEmailAddress = item.PersonEmailAddress,
                            EmployerPostcode = item.EmployerPostcode,
                            EmployerEmailAddress = item.CurrentEmployerEmailAddress,
                            Key = item.Key
                        },
                        OldTpsEmployment = new()
                        {
                            PersonEmploymentId = item.TpsEmploymentId,
                            PersonId = item.PersonId,
                            EstablishmentId = item.EstablishmentId,
                            StartDate = item.StartDate,
                            EndDate = item.EndDate,
                            LastKnownTpsEmployedDate = item.LastKnownTpsEmployedDate,
                            EmploymentType = item.EmploymentType,
                            WithdrawalConfirmed = item.WithdrawalConfirmed,
                            LastExtractDate = item.LastExtractDate,
                            NationalInsuranceNumber = item.NationalInsuranceNumber,
                            PersonPostcode = item.PersonPostcode,
                            PersonEmailAddress = item.PersonEmailAddress,
                            EmployerPostcode = item.EmployerPostcode,
                            EmployerEmailAddress = item.NewEmployerEmailAddress,
                            Key = item.Key
                        },
                        Changes = changes,
                        CreatedUtc = clock.UtcNow,
                        RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId
                    };

                    events.Add(updatedEvent);
                }
            }

            await transaction.SaveEventsAsync(events, TempEventsTableSuffix, clock, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            events.Clear();
        }
        while (hasRecordsToUpdate);
    }
}
