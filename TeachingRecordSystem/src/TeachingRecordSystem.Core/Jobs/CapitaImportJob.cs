using System.Globalization;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Jobs;

public class CapitaImportJob(BlobServiceClient blobServiceClient, ILogger<CapitaImportJob> logger, TrsDbContext dbContext, IClock clock)
{
    public const string JobSchedule = "0 3 * * *";
    public const string StorageContainer = "dqt-integrations";
    public const string PICKUP_FOLDER = "capita/pickup";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var file in await GetImportFilesAsync(cancellationToken))
        {
            using (var downloadStream = await GetDownloadStreamAsync(file))
            using (var reader = new StreamReader(downloadStream))
            {
                await ImportAsync(reader);
            }
        }
    }

    public async Task<long> ImportAsync(StreamReader reader)
    {
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            TrimOptions = TrimOptions.Trim,
            HasHeaderRecord = false,
            MissingFieldFound = null,
            IgnoreBlankLines = true
        };
        using var csv = new CsvReader(reader, csvConfig);
        csv.Context.RegisterClassMap<CapitaImportMap>();
        var records = csv.GetRecords<CapitaImportRecord>().ToList();
        var totalRowCount = 0;
        var successCount = 0;
        var duplicateRowCount = 0;
        var failureRowCount = 0;
        var failureMessage = new StringBuilder();

        var integrationJob = new IntegrationTransaction()
        {
            IntegrationTransactionId = 0,
            InterfaceType = IntegrationTransactionInterfaceType.EwcWales,
            ImportStatus = IntegrationTransactionImportStatus.InProgress,
            TotalCount = 0,
            SuccessCount = 0,
            FailureCount = 0,
            DuplicateCount = 0,
            FileName = "fileName.txt",
            CreatedDate = clock.UtcNow,
            IntegrationTransactionRecords = new List<IntegrationTransactionRecord>()
        };
        dbContext.IntegrationTransactions.Add(integrationJob);
        await dbContext.SaveChangesAsync();
        var integrationId = integrationJob.IntegrationTransactionId;

        foreach (var row in records)
        {
            //insert or update person
            //insert ITR

        }
        return integrationId;
    }

    public async Task<Stream> GetDownloadStreamAsync(string fileName)
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(StorageContainer);
        BlobClient blobClient = containerClient.GetBlobClient($"{fileName}");
        var streamingResult = await blobClient.DownloadStreamingAsync();
        return streamingResult.Value.Content;
    }

    private async Task<string[]> GetImportFilesAsync(CancellationToken cancellationToken)
    {
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(StorageContainer);
        var fileNames = new List<string>();
        var resultSegment = blobContainerClient.GetBlobsByHierarchyAsync(prefix: PICKUP_FOLDER, delimiter: "", cancellationToken: cancellationToken).AsPages();
        await foreach (Azure.Page<BlobHierarchyItem> blobPage in resultSegment)
        {
            foreach (BlobHierarchyItem blobhierarchyItem in blobPage.Values)
            {
                if (blobhierarchyItem.IsBlob)
                {
                    fileNames.Add(blobhierarchyItem.Blob.Name);
                }
            }
        }
        return fileNames.ToArray();
    }
}

public class CapitaImportMap : ClassMap<CapitaImportRecord>
{
    public CapitaImportMap()
    {
        Map(m => m.TRN).Index(0).Optional();
        Map(m => m.Gender).Index(1).Optional();
        Map(m => m.LastName).Index(2).Optional();
        Map(m => m.FirstNameOrMiddleName).Index(3).Optional();
        Map(m => m.PreviousLastName).Index(4).Optional();
        Map(m => m.DateOfBirth).Index(5).Optional();
        Map(m => m.NINumber).Index(6).Optional();
        Map(m => m.DateOfDeath).Index(7).Optional();
    }
}

public class CapitaImportRecord
{
    public required string? TRN { get; set; }
    public required string? Gender { get; set; }
    public required string? LastName { get; set; }
    public required string? FirstNameOrMiddleName { get; set; }
    public required string? PreviousLastName { get; set; }
    public required string? DateOfBirth { get; set; }
    public required string? NINumber { get; set; }
    public required string? DateOfDeath { get; set; }
}
