using Azure.Storage.Blobs;
using CsvHelper;
using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Jobs;

public class CpdInductionImporterJob(BlobServiceClient blobServiceClient, IDbContextFactory<TrsDbContext> dbContextFactory, IClock clock)
{
    private const string StorageContainer = "cpd-inductions";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var now = clock.UtcNow.ToUniversalTime();
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(StorageContainer);

        using (var cpdInductionFileBlob = await blobContainerClient.GetBlobClient("cpd-induction-trns.csv").OpenReadAsync())
        using (var cpdInductionFileReader = new StreamReader(cpdInductionFileBlob))
        using (var cpdInductionCsvFileReader = new CsvReader(cpdInductionFileReader, System.Globalization.CultureInfo.CurrentCulture))
        {
            var records = cpdInductionCsvFileReader.GetRecords<CpdInductionRow>().ToList();

            using var dbContext = dbContextFactory.CreateDbContext();
            var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            //create temp table
            using (var command = new NpgsqlCommand(
                """
                CREATE TEMP TABLE temp_cpd_induction (
                    trn TEXT
                ) ON COMMIT DROP;
                """, connection, transaction))
            {
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            //copy rows to temp table
            using (var writer = await connection.BeginBinaryImportAsync(
                "COPY temp_cpd_induction (trn) FROM STDIN (FORMAT BINARY)"))
            {
                foreach (var record in records)
                {
                    writer.StartRow();
                    writer.Write(record.Trn, NpgsqlTypes.NpgsqlDbType.Text);
                }
                await writer.CompleteAsync(cancellationToken);
            }

            //update cpd induction
            using (var command = new NpgsqlCommand(
                """
                UPDATE persons
                SET cpd_induction_modified_on = @now
                FROM temp_cpd_induction
                WHERE persons.Trn = temp_cpd_induction.trn;
                """, connection, transaction))
            {
                command.Parameters.AddWithValue("@now", NpgsqlTypes.NpgsqlDbType.TimestampTz, now);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
    }
}

public class CpdInductionRow
{
    public required string Trn { get; init; }
}
