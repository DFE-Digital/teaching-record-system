using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.WorkforceData;

public class TpsCsvExtractProcessor(
    IDbContextFactory<TrsDbContext> dbContextFactory,
    IClock clock)
{
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
        using var readDbContext = dbContextFactory.CreateDbContext();
        readDbContext.Database.SetCommandTimeout(600);
        using var writeDbContext = dbContextFactory.CreateDbContext();

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
            )
            SELECT
                x.tps_csv_extract_item_id,
                p.person_id,
                e.establishment_id,
                x.employment_start_date as start_date,
                x.employment_end_date as end_date,
                x.employment_type
            FROM
                    tps_csv_extract_items x
                JOIN
                    persons p ON x.trn = p.trn
                JOIN
                    unique_establishments e ON x.local_authority_code = e.la_code
                        AND (e.establishment_number = x.establishment_number
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
                                    pe.person_id = p.person_id
                                    AND pe.establishment_id = e.establishment_id
                                    AND pe.start_date = x.employment_start_date)
            """;

        int i = 0;
        var processedExtractItemIds = new List<Guid>();
        await foreach (var item in readDbContext.Database.SqlQuery<NewPersonEmployment>(querySql).AsAsyncEnumerable())
        {
            var personEmployment = new PersonEmployment
            {
                PersonEmploymentId = Guid.NewGuid(),
                PersonId = item.PersonId,
                EstablishmentId = item.EstablishmentId,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                EmploymentType = item.EmploymentType,
                CreatedOn = clock.UtcNow,
                UpdatedOn = clock.UtcNow
            };

            writeDbContext.PersonEmployments.Add(personEmployment);
            writeDbContext.AddEvent(new PersonEmploymentCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = item.PersonId,
                PersonEmployment = Core.Events.Models.PersonEmployment.FromModel(personEmployment),
                CreatedUtc = clock.UtcNow,
                RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId
            });

            processedExtractItemIds.Add(item.TpsCsvExtractItemId);

            i++;
            if (i % 1000 == 0)
            {
                await SaveChanges();
            }
        }

        if (writeDbContext.ChangeTracker.HasChanges())
        {
            await SaveChanges();
        }

        async Task SaveChanges()
        {
            await writeDbContext.SaveChangesAsync(cancellationToken);

            FormattableString updateSql =
                $"""
                UPDATE
                    tps_csv_extract_items
                SET
                    result = {TpsCsvExtractItemResult.ValidDataAdded}
                WHERE
                    tps_csv_extract_item_id = ANY ({processedExtractItemIds})
                """;
            await writeDbContext.Database.ExecuteSqlAsync(updateSql, cancellationToken);
            processedExtractItemIds!.Clear();
        }
    }

    public async Task ProcessUpdatedEmploymentHistory(Guid tpsCsvExtractId, CancellationToken cancellationToken)
    {
        using var readDbContext = dbContextFactory.CreateDbContext();
        readDbContext.Database.SetCommandTimeout(600);
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
                        ROW_NUMBER() OVER (PARTITION BY la_code, establishment_number, CASE WHEN establishment_number IS NULL THEN postcode ELSE NULL END ORDER BY translate(establishment_status_code::text, '1234', '1324'), urn desc) as row_number
                    FROM
                        establishments) e
                WHERE
                    e.row_number = 1
            )
            SELECT
                x.tps_csv_extract_item_id,
                pe.person_employment_id,
                pe.person_id,
                pe.establishment_id,
                pe.start_date,
                pe.end_date as current_end_date,
                pe.employment_type as current_employment_type,
                x.employment_end_date as end_date,
                x.employment_type
            FROM
                    tps_csv_extract_items x
                JOIN
                    persons p ON x.trn = p.trn
                JOIN
                    unique_establishments e ON x.local_authority_code = e.la_code
                        AND (e.establishment_number = x.establishment_number
                             OR (e.establishment_type_code = '29' 
                                 AND x.establishment_postcode = e.postcode
                                 AND NOT EXISTS (SELECT
                                                     1
                                                 FROM
                                                     unique_establishments e2
                                                 WHERE
                                                     e2.la_code = x.local_authority_code
                                                     AND e2.establishment_number = x.establishment_number)))
                JOIN
                    person_employments pe ON pe.person_id = p.person_id
                    AND pe.establishment_id = e.establishment_id
                    AND pe.start_date = x.employment_start_date
            WHERE
                x.tps_csv_extract_id = {tpsCsvExtractId}
                AND x.result IS NULL
            """;

        var updatedExtractItemIds = new List<Guid>();
        var noChangeExtractItemIds = new List<Guid>();
        var batchCommands = new List<NpgsqlBatchCommand>();

        await foreach (var item in readDbContext.Database.SqlQuery<UpdatedPersonEmployment>(querySql).AsAsyncEnumerable())
        {
            var changes = PersonEmploymentUpdatedEventChanges.None |
                (item.CurrentEndDate != item.EndDate ? PersonEmploymentUpdatedEventChanges.EndDate : PersonEmploymentUpdatedEventChanges.None) |
                (item.CurrentEmploymentType != item.EmploymentType ? PersonEmploymentUpdatedEventChanges.EmploymentType : PersonEmploymentUpdatedEventChanges.None);

            if (changes != PersonEmploymentUpdatedEventChanges.None)
            {
                var formattedEndDate = item.EndDate.HasValue ? $"to_date('{item.EndDate:yyyyMMdd}','YYYYMMDD')" : "NULL";
                var updatePersonEmploymentsCommand = new NpgsqlBatchCommand(
                    $"""
                    UPDATE
                        person_employments
                    SET
                        end_date = {formattedEndDate},
                        employment_type = {(int)item.EmploymentType}
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
                        EmploymentType = item.EmploymentType
                    },
                    OldPersonEmployment = new()
                    {
                        PersonEmploymentId = item.PersonEmploymentId,
                        PersonId = item.PersonId,
                        EstablishmentId = item.EstablishmentId,
                        StartDate = item.StartDate,
                        EndDate = item.CurrentEndDate,
                        EmploymentType = item.CurrentEmploymentType
                    },
                    Changes = changes,
                    CreatedUtc = clock.UtcNow,
                    RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId
                });

                updatedExtractItemIds.Add(item.TpsCsvExtractItemId);
                if (updatedExtractItemIds.Count == 1000)
                {
                    UpdateResult(updatedExtractItemIds, TpsCsvExtractItemResult.ValidDataUpdated);
                    updatedExtractItemIds.Clear();
                }
            }
            else
            {
                noChangeExtractItemIds.Add(item.TpsCsvExtractItemId);

                if (noChangeExtractItemIds.Count == 1000)
                {
                    UpdateResult(noChangeExtractItemIds, TpsCsvExtractItemResult.ValidNoChange);
                    noChangeExtractItemIds.Clear();
                }
            }

            if (batchCommands.Count == 50)
            {
                await SaveChanges();
            }
        }

        if (updatedExtractItemIds.Any())
        {
            UpdateResult(updatedExtractItemIds, TpsCsvExtractItemResult.ValidDataUpdated);
        }

        if (noChangeExtractItemIds.Any())
        {
            UpdateResult(noChangeExtractItemIds, TpsCsvExtractItemResult.ValidNoChange);
        }

        if (batchCommands.Any())
        {
            await SaveChanges();
        }

        void UpdateResult(IEnumerable<Guid> extractItemIds, TpsCsvExtractItemResult result)
        {
            var formattedExtractItemIds = string.Join(", ", extractItemIds.Select(id => $"'{id}'"));
            var updateResultCommand = new NpgsqlBatchCommand(
                $"""
                UPDATE
                    tps_csv_extract_items
                SET
                    result = {(int)result}
                WHERE
                    tps_csv_extract_item_id = ANY (ARRAY[{formattedExtractItemIds}]::uuid[])
                """);
            batchCommands.Add(updateResultCommand);
        }

        async Task SaveChanges()
        {
            if (writeDbContext.ChangeTracker.HasChanges())
            {
                await writeDbContext.SaveChangesAsync(cancellationToken);
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

    public async Task UpdateLatestEstablishmentVersions(CancellationToken cancellationToken)
    {
        using var readDbContext = dbContextFactory.CreateDbContext();
        readDbContext.Database.SetCommandTimeout(600);
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
                        ROW_NUMBER() OVER (PARTITION BY la_code, establishment_number, CASE WHEN establishment_number IS NULL THEN postcode ELSE NULL END ORDER BY translate(establishment_status_code::text, '1234', '1324'), urn desc) as row_number
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
                        establishment_id = '{item.EstablishmentId}'
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
                    EmploymentType = item.EmploymentType
                },
                OldPersonEmployment = new()
                {
                    PersonEmploymentId = item.PersonEmploymentId,
                    PersonId = item.PersonId,
                    EstablishmentId = item.CurrentEstablishmentId,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    EmploymentType = item.EmploymentType
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
}
