using System.Globalization;
using System.Text;
using System.Text.Json;
using Azure.Storage.Blobs;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Jobs.EwcWalesImport;

public class CapitaExportNewJob(BlobServiceClient blobServiceClient, ILogger<EwcWalesImportJob> logger, TrsDbContext dbContext, IClock clock)
{
    public const string JobSchedule = "0 3 * * *";
    public const string LastRunDateKey = "LastRunDate";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var item = await dbContext.JobMetadata.SingleOrDefaultAsync(i => i.JobName == nameof(CapitaExportNewJob));
        var lastRunDate = default(DateTime?);

        //if the job has ran before - the lastrundate from metadata is used.
        if (item != null)
        {
            if (item.Metadata.TryGetValue(LastRunDateKey, out var obj) &&
                obj is JsonElement jsonElement &&
                jsonElement.ValueKind == JsonValueKind.String)
            {
                lastRunDate = jsonElement.GetDateTime(); ;
            }
        }


        var persons = dbContext.Persons.Where(x => x.UpdatedOn > lastRunDate);

        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true);
        using (var csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            //write header
            foreach (var person in persons)
            {

            }
        }


        // update last run date
        if (item != null)
        {
            item.Metadata = new Dictionary<string, object>
            {
                { LastRunDateKey, clock.UtcNow }
            };
        }
        await dbContext.SaveChangesAsync();
    }
}
