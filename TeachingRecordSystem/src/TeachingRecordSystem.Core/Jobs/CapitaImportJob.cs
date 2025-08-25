using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.PersonMatching;

namespace TeachingRecordSystem.Core.Jobs;

public class CapitaImportJob(BlobServiceClient blobServiceClient, ILogger<CapitaImportJob> logger, TrsDbContext dbContext, IClock clock, PersonMatchingService personMatchingService)
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
                var (errors, warnings) = ValidateRow(row);
                var personId = default(Guid?);
                var recordStatus = IntegrationTransactionRecordStatus.Success;
                var potentialDuplicate = false;
                var rowFailureMessage = new StringBuilder();
                rowFailureMessage.Append(string.Join(Environment.NewLine, errors));
                rowFailureMessage.Append(string.Join(Environment.NewLine, warnings));

                if (errors.Any())
                {
                    recordStatus = IntegrationTransactionRecordStatus.Failure;
                }
                else
                {
                    var persons = await GetPersonAsync(row);
                    if (persons.Outcome == TrnRequestMatchResultOutcome.NoMatches || persons.Outcome == TrnRequestMatchResultOutcome.PotentialMatches)
                    {
                        //create person if incoming record is not known in trs
                        NationalInsuranceNumber.TryParse(row.NINumber, out var ni);
                        var person = Person.Create(row.TRN!, row.GetFirstName()!, row.GetMiddletName()!, row.LastName!, row.GetDateOfBirth(), null, ni, (Gender?)row.Gender, clock.UtcNow);
                        person.Person.CreatedByTps = true;
                        if (!string.IsNullOrEmpty(row.DateOfDeath) && DateOnly.TryParseExact(row.DateOfDeath, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfDeath))
                        {
                            person.Person.SetStatus(PersonStatus.Deactivated, "Date of death received from capita import", null, null, DataStore.Postgres.Models.SystemUser.Instance.UserId, clock.UtcNow, out var @event);
                            if (@event is not null)
                            {
                                await dbContext.AddEventAndBroadcastAsync(@event);
                                await dbContext.SaveChangesAsync();
                            }
                        }
                        dbContext.Persons.Add(person.Person);
                        personId = person.Person.PersonId;

                        //create task
                        if (persons.Outcome == TrnRequestMatchResultOutcome.PotentialMatches)
                        {
                            potentialDuplicate = true;
                            var supportTask = SupportTask.Create(
                                SupportTaskType.CapitaImportPotentialDuplicate,
                                new Models.SupportTaskData.ApiTrnRequestData(),
                                personId: personId.Value,
                                null,
                                null,
                                null,
                                DataStore.Postgres.Models.SystemUser.SystemUserId,
                                clock.UtcNow,
                                out var createdEvent);
                            dbContext.SupportTasks.Add(supportTask);
                            await dbContext.AddEventAndBroadcastAsync(createdEvent);
                        }
                    }
                    else if (persons.Outcome == TrnRequestMatchResultOutcome.DefiniteMatch)
                    {
                        //Update person
                        var person = dbContext.Persons.First(x => x.Trn == persons.Trn);
                        personId = person.PersonId;

                        // Deactivate person if date of death is provided
                        if (!string.IsNullOrEmpty(row.DateOfDeath) && DateOnly.TryParseExact(row.DateOfDeath, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfDeath))
                        {
                            person.DateOfDeath = dateOfDeath;
                            person.SetStatus(PersonStatus.Deactivated, "Date of death received from capita import", null, null, DataStore.Postgres.Models.SystemUser.Instance.UserId, clock.UtcNow, out var @event);
                            if (@event is not null)
                            {
                                await dbContext.AddEventAndBroadcastAsync(@event);
                                await dbContext.SaveChangesAsync();
                            }
                        }
                    }
                    else if (persons.Outcome == TrnRequestMatchResultOutcome.PotentialMatches)
                    {

                    }
                }

                //write current person exported row to integrationtransactionrecord
                integrationJob.IntegrationTransactionRecords.Add(new IntegrationTransactionRecord()
                {
                    IntegrationTransactionRecordId = 0,
                    CreatedDate = clock.UtcNow,
                    RowData = row.ToString(),
                    Status = recordStatus,
                    PersonId = personId,
                    FailureMessage = rowFailureMessage.ToString(),
                    Duplicate = potentialDuplicate,
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

    public (List<string> Errors, List<string> Warnings) ValidateRow(CapitaImportRecord record)
    {
        //hard errors
        var errors = new List<string>();
        var warnings = new List<string>();

        var trnRegex = new Regex(@"^\d{7}$");
        var DATE_FORMAT = "yyyyMMdd";

        //trn
        if (string.IsNullOrEmpty(record.TRN))
        {
            errors.Add("Missing required field: TRN");
            return (errors, warnings);
        }
        else if (!trnRegex.IsMatch(record.TRN))
        {
            errors.Add("Validation failed on field: TRN");
            return (errors, warnings);
        }

        //dob
        if (string.IsNullOrEmpty(record.DateOfBirth))
        {
            errors.Add("Missing required field: Date of birth");
            return (errors, warnings);
        }
        else
        {
            if (!DateOnly.TryParseExact(record.DateOfBirth, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfBirth))
            {
                errors.Add("Validation Failed: Invalid Date of Birth");
            }
            else if (dateOfBirth > clock.UtcNow.ToDateOnlyWithDqtBstFix(isLocalTime: true))
            {
                errors.Add("Validation Failed: Date of Birth cannot be in the future");
            }
        }

        if (!string.IsNullOrEmpty(record.DateOfDeath))
        {
            if (!DateOnly.TryParseExact(record.DateOfDeath, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfDeath))
            {
                errors.Add("Validation Failed: Invalid Date of death");
            }
            else if (dateOfDeath > clock.UtcNow.ToDateOnlyWithDqtBstFix(isLocalTime: true))
            {
                errors.Add("Validation Failed: Date of death cannot be in the future");
            }
        }

        //gender
        if (!record.Gender.HasValue)
        {
            errors.Add("Missing required field: Gender");
            return (errors, warnings);
        }
        else
        {
            var validGendoers = new List<int> { (int)Gender.Male, (int)Gender.Female };
            if (!validGendoers.Any(x => x == record.Gender.Value))
            {
                errors.Add($"Invalid Gender: {record.Gender.Value}");
            }
        }

        //NI Number
        if (!string.IsNullOrEmpty(record.NINumber) && !NationalInsuranceNumber.TryParse(record.NINumber, out var ni))
        {
            warnings.Add("Invalid National Insurance number");
        }

        return (errors, warnings);
    }

    public async Task<TrnRequestMatchResult> GetPersonAsync(CapitaImportRecord row)
    {

        var requestData = new DataStore.Postgres.Models.CapitaImportRequest()
        {
            ApplicationUserId = DataStore.Postgres.Models.SystemUser.SystemUserId,
            RequestId = Guid.NewGuid().ToString(),
            CreatedOn = clock.UtcNow,
            IdentityVerified = null,
            EmailAddress = null,
            OneLoginUserSubject = null,
            FirstName = row.GetFirstName(),
            MiddleName = row.GetMiddletName(),
            LastName = row.LastName,
            PreviousFirstName = null,
            PreviousLastName = row.PreviousLastName,
            Name = [row.GetFirstName()!, row.GetMiddletName()!, row.LastName!],
            DateOfBirth = row.GetDateOfBirth()!.Value,
            NationalInsuranceNumber = row.NINumber,
            Trn = row.TRN
        };
        var matches = await personMatchingService.MatchFromCapitaTrnRequestAsync(requestData);
        return matches;
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
    public required int? Gender { get; set; }
    public required string? LastName { get; set; }
    public required string? FirstNameOrMiddleName { get; set; }
    public required string? PreviousLastName { get; set; }
    public required string? DateOfBirth { get; set; }
    public required string? NINumber { get; set; }
    public required string? DateOfDeath { get; set; }

    public string? GetFirstName()
    {
        var parts = FirstNameOrMiddleName?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts?.Length > 0)
            return parts[0];
        return null;
    }

    public string? GetMiddletName()
    {
        var parts = FirstNameOrMiddleName?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts?.Length > 0)
            return string.Join(" ", parts, 1, parts.Length - 1);
        return null;
    }

    public DateOnly? GetDateOfBirth()
    {
        if (DateOnly.TryParseExact(DateOfBirth, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfBirth))
            return dateOfBirth;
        return null;
    }

    public override string ToString()
    {
        var trn = TRN ?? string.Empty;
        var gender = Gender.HasValue ? Gender.Value.ToString() : string.Empty;
        var lastName = LastName ?? string.Empty;
        var firstOrMiddleName = FirstNameOrMiddleName ?? string.Empty;
        var previousLastName = PreviousLastName ?? string.Empty;
        var dateOfBirth = DateOfBirth ?? string.Empty;
        var niNumber = NINumber ?? string.Empty;
        var dateOfDeath = DateOfDeath ?? string.Empty;

        return $"{trn};{gender};{lastName};{firstOrMiddleName};{previousLastName};{dateOfBirth};{niNumber};{dateOfDeath};;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";
    }
}
