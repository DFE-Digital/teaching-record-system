using System;
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
                await ImportAsync(reader, file);
            }
        }
    }

    public async Task<long> ImportAsync(StreamReader reader, string fileName)
    {
        await using var txn = await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
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
            InterfaceType = IntegrationTransactionInterfaceType.CapitaImport,
            ImportStatus = IntegrationTransactionImportStatus.InProgress,
            TotalCount = 0,
            SuccessCount = 0,
            FailureCount = 0,
            DuplicateCount = 0,
            FileName = fileName,
            CreatedDate = clock.UtcNow,
            IntegrationTransactionRecords = new List<IntegrationTransactionRecord>()
        };
        dbContext.IntegrationTransactions.Add(integrationJob);
        await dbContext.SaveChangesAsync();
        var integrationId = integrationJob.IntegrationTransactionId;


        foreach (var row in records)
        {
            try
            {

                //insert or update person
                //insert ITR
                var persons = await GetPersonAsync(row.TRN!);
                if(persons.Count() == 0)
                {
                    //inser
                }

                NationalInsuranceNumber.TryParse(row.NINumber, out var ni);
                var person = Person.Create(row.TRN!, row.FirstNameOrMiddleName!, row.FirstNameOrMiddleName!, row.LastName!, row.DateOfBirth, null, ni, row.Gender, clock.UtcNow);
                dbContext.Persons.Add(person.Person);

                //write current person exported row to integrationtransactionrecord
                integrationJob.IntegrationTransactionRecords.Add(new IntegrationTransactionRecord()
                {
                    IntegrationTransactionRecordId = 0,
                    CreatedDate = clock.UtcNow,
                    RowData = "",
                    Status = IntegrationTransactionRecordStatus.Success,
                    PersonId = person.Person.PersonId,
                    FailureMessage = null,
                    Duplicate = null,
                    HasActiveAlert = null
                });
                totalRowCount++;
                successCount++;
            }
            catch (Exception ex)
            {
                logger.LogInformation($"{nameof(CapitaImportJob)} - {ex.Message}");
                failureRowCount++;
            }
        }
        // mark job as complete
        integrationJob.TotalCount = totalRowCount;
        integrationJob.SuccessCount = successCount;
        integrationJob.FailureCount = failureRowCount;
        integrationJob.DuplicateCount = duplicateRowCount;
        integrationJob.ImportStatus = IntegrationTransactionImportStatus.Success;

        await dbContext.SaveChangesAsync();
        await txn.CommitAsync();
        return integrationId;
    }

    public async Task<List<Person>> GetPersonAsync(string trn)
    {
        var persons = await dbContext.Persons.Where(x => x.Trn == trn).ToListAsync();
        return persons;
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
        Map(m => m.DateOfBirth).Index(5)
            .TypeConverterOption.Format("yyyyMMdd")
            .TypeConverterOption.NullValues(string.Empty, null); 
        Map(m => m.NINumber).Index(6).Optional();
        Map(m => m.DateOfDeath).Index(7).Optional();
    }
}

public class CapitaImportRecord
{
    public required string? TRN { get; set; }
    public required Gender? Gender { get; set; }
    public required string? LastName { get; set; }
    public required string? FirstNameOrMiddleName { get; set; }
    public required string? PreviousLastName { get; set; }
    public required DateOnly? DateOfBirth { get; set; }
    public required string? NINumber { get; set; }
    public required string? DateOfDeath { get; set; }
}
