using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.Core.Jobs;

public class DeletePersonAndChildRecordsWithoutATrnJob(
    IOptions<DeletePersonAndChildRecordsWithoutATrnOptions> jobOptionsAccessor,
    TrsDbContext dbContext,
    IFileService fileService,
    IClock clock,
    ILogger<DeletePersonAndChildRecordsWithoutATrnJob> logger)
{
    private readonly DeletePersonAndChildRecordsWithoutATrnOptions _options = jobOptionsAccessor.Value;

    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(300);

        var personIdsWithNoTrn = await dbContext.Persons
            .Where(p => p.Trn == null)
            .Select(p => p.PersonId)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Deleting Person records without a TRN, and associated records.");

        if (dryRun)
        {
            logger.LogInformation("(Dry run: records will not actually be deleted.)");
        }

        logger.LogInformation($"Found {personIdsWithNoTrn.Count} Person records with no TRN.");

        var batches = personIdsWithNoTrn.Chunk(_options.BatchSize);

        List<Guid> deletedPersonIds = [];

        foreach (var (batch, index) in batches.Select((b, i) => (b, i)))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Operation cancelled.");
                break;
            }

            logger.LogInformation($"Deleting records {(index * _options.BatchSize) + 1} to {Math.Min(personIdsWithNoTrn.Count, (index + 1) * _options.BatchSize)}...");

            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            var personIds = batch.ToArray();

            var deletedPersonIdsInBatch = await ExecuteSqlAsync<Guid>(
            $"""
                -- Delete support tasks referencing person (and accompanying requests/metadata)

                DELETE FROM trn_request_metadata AS m
                USING support_tasks AS s
                WHERE s.trn_request_application_user_id = m.application_user_id AND s.trn_request_id = m.request_id
                AND s.person_id = ANY ({personIds});

                DELETE FROM trn_requests AS r
                USING support_tasks AS s, trn_request_metadata AS m
                WHERE r.request_id = m.request_id
                AND s.trn_request_application_user_id = m.application_user_id AND s.trn_request_id = m.request_id
                AND s.person_id = ANY ({personIds});

                DELETE FROM support_tasks
                WHERE person_id = ANY ({personIds});

                -- Delete requests/metadata referencing person (and accompanying support tasks)

                DELETE FROM support_tasks AS s
                USING trn_request_metadata AS m
                WHERE s.trn_request_application_user_id = m.application_user_id AND s.trn_request_id = m.request_id
                AND m.resolved_person_id = ANY ({personIds});

                DELETE FROM trn_requests AS r
                USING support_tasks AS s, trn_request_metadata AS m
                WHERE r.request_id = m.request_id
                AND s.trn_request_application_user_id = m.application_user_id AND s.trn_request_id = m.request_id
                AND m.resolved_person_id = ANY ({personIds});

                DELETE FROM trn_request_metadata
                WHERE resolved_person_id = ANY ({personIds});

                -- Delete integration_transaction_records referencing person
            
                DELETE FROM integration_transaction_records
                WHERE person_id = ANY ({personIds});

                -- Clear references to person from one_login_users

                UPDATE one_login_users
                SET person_id = NULL
                WHERE person_id = ANY ({personIds});

                -- Clear references to person via merged_with_person_id
            
                UPDATE persons
                SET merged_with_person_id = NULL
                WHERE merged_with_person_id = ANY ({personIds});

                -- Delete person

                DELETE FROM persons
                WHERE person_id = ANY ({personIds})
                RETURNING person_id;
            """, cancellationToken);

            deletedPersonIds.AddRange(deletedPersonIdsInBatch);

            if (!dryRun)
            {
                await transaction.CommitAsync(cancellationToken);
            }
        }

        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
        using var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(deletedPersonIds.Select(p => new { PersonId = p }), cancellationToken);
        await writer.FlushAsync();
        stream.Position = 0;

        await fileService.UploadFileAsync($"allocatetrntopersonswitheyps{clock.UtcNow:yyyyMMddHHmmss}.csv", stream, "text/csv");

        if (!dryRun)
        {
            logger.LogInformation($"Done. Deleted {deletedPersonIds.Count} records.");
        }
        else
        {
            logger.LogInformation($"Done. {deletedPersonIds.Count} records would have been deleted.");
        }
    }

    private async Task<IEnumerable<TReturning>> ExecuteSqlAsync<TReturning>(FormattableString sql, CancellationToken cancellationToken)
    {
        logger.LogDebug(sql.ToString());

        var returning = await dbContext.Database.SqlQuery<TReturning>(sql)
            .ToArrayAsync(cancellationToken);

        return returning;
    }
}
