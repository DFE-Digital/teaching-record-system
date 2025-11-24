using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.Core.Jobs;

public class DeletePersonAndChildRecordsWithoutATrnJob(
    IOptions<DeletePersonAndChildRecordsWithoutATrnOptions> jobOptionsAccessor,
    TrsDbContext dbContext,
    IImportFileStorageService fileStorageService,
    IClock clock,
    ILogger<DeletePersonAndChildRecordsWithoutATrnJob> logger)
{
    public const string ContainerName = "delete-person-and-child-records-without-a-trn";
    public const string OutputFolderName = "output";
    public const string OutputFileNamePrefix = "delete-person-and-child-records-without-a-trn-";

    private readonly DeletePersonAndChildRecordsWithoutATrnOptions _options = jobOptionsAccessor.Value;

    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(30);

        var personIdsWithNoTrn = await dbContext.Persons
            .IgnoreQueryFilters()
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
                -- Store request ids that are going to be deleted

                CREATE TEMP TABLE trn_request_ids_to_delete AS
                SELECT trn_request_id FROM support_tasks
                WHERE person_id = ANY ({personIds});

                INSERT INTO trn_request_ids_to_delete
                SELECT request_id FROM trn_request_metadata
                WHERE resolved_person_id = ANY ({personIds});

                -- Clear references to person via merged_with_person_id

                UPDATE persons
                SET merged_with_person_id = NULL
                WHERE merged_with_person_id = ANY ({personIds});

                -- Clear references to trn_request_metadata of requests to be deleted from person via source_trn_request_id

                UPDATE persons
                SET source_trn_request_id = NULL
                WHERE source_trn_request_id IN (SELECT trn_request_id FROM trn_request_ids_to_delete);

                -- Delete support tasks referencing person (and accompanying requests/metadata)

                DELETE FROM support_tasks
                WHERE person_id = ANY ({personIds});

                DELETE FROM support_tasks
                WHERE trn_request_id IN (SELECT trn_request_id FROM trn_request_ids_to_delete);

                DELETE FROM trn_request_metadata
                WHERE request_id IN (SELECT trn_request_id FROM trn_request_ids_to_delete);

                DELETE FROM trn_requests
                WHERE request_id IN (SELECT trn_request_id FROM trn_request_ids_to_delete);

                -- Delete integration_transaction_records referencing person

                DELETE FROM integration_transaction_records
                WHERE person_id = ANY ({personIds});

                -- Clear references to person from one_login_users

                UPDATE one_login_users
                SET person_id = NULL
                WHERE person_id = ANY ({personIds});

                -- Delete person

                DELETE FROM persons
                WHERE person_id = ANY ({personIds})
                RETURNING person_id;

                -- Drop temp table

                DROP TABLE trn_request_ids_to_delete;
            """, cancellationToken);

            deletedPersonIds.AddRange(deletedPersonIdsInBatch);

            if (!dryRun)
            {
                await transaction.CommitAsync(cancellationToken);
            }
        }

        using var stream = await fileStorageService.WriteFileAsync(ContainerName, $"{OutputFolderName}/{OutputFileNamePrefix}{clock.UtcNow:yyyyMMddHHmmss}.csv", cancellationToken);
        using var writer = new StreamWriter(stream);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(deletedPersonIds.Select(p => new { PersonId = p }), cancellationToken);
        await writer.FlushAsync(cancellationToken);

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
