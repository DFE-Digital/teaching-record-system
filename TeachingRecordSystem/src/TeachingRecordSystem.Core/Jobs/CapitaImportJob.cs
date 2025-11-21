using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Azure.Storage.Files.DataLake;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.PersonMatching;


namespace TeachingRecordSystem.Core.Jobs;

public class CapitaImportJob([FromKeyedServices("sftpstorage")] DataLakeServiceClient dataLakeServiceClient, ILogger<CapitaImportJob> logger, TrsDbContext dbContext, IClock clock, IPersonMatchingService personMatchingService, IOptions<CapitaTpsUserOption> capitaUser)
{
    public const string JobSchedule = "0 4 * * *";
    public const string StorageContainer = "capita-integrations";
    public const string PickupFolder = "pickup";
    private const string ProcessedFolder = "capita/processed";
    public const string ArchivedContainer = "archived-integration-transactions";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var file in await GetImportFilesAsync(cancellationToken))
        {
            using (var downloadStream = await GetDownloadStreamAsync(file))
            using (var reader = new StreamReader(downloadStream))
            {
                await ImportAsync(reader, file);
                await ArchiveFileAsync(file, cancellationToken);
            }
        }
    }

    public async Task ArchiveFileAsync(string fileName, CancellationToken cancellationToken)
    {
        var fileSystemClient = dataLakeServiceClient.GetFileSystemClient(StorageContainer);
        var arhivedFileSystemClient = dataLakeServiceClient.GetFileSystemClient(ArchivedContainer);
        var sourceFile = fileSystemClient.GetFileClient(fileName);

        var fileNameParts = fileName.Split('/');
        var fileNameWithoutFolder = $"{DateTime.UtcNow:ddMMyyyyHHmm}-{fileNameParts.Last()}";
        var targetPath = $"{ProcessedFolder}/{fileNameWithoutFolder}";
        var targetFile = arhivedFileSystemClient.GetFileClient(targetPath);

        await targetFile.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        // Read the source file
        var readResponse = await sourceFile.ReadAsync(cancellationToken: cancellationToken);
        await using var sourceStream = readResponse.Value.Content;

        await using var memory = new MemoryStream();
        await sourceStream.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;

        await targetFile.AppendAsync(memory, offset: 0, cancellationToken: cancellationToken);
        await targetFile.FlushAsync(memory.Length, cancellationToken: cancellationToken);

        // Delete the original file
        await sourceFile.DeleteIfExistsAsync(cancellationToken: cancellationToken);
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
        var warningCount = 0;
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
            WarningCount = 0,
            FailureCount = 0,
            DuplicateCount = 0,
            FileName = fileName,
            CreatedDate = clock.UtcNow,
            IntegrationTransactionRecords = new List<IntegrationTransactionRecord>()
        };
        dbContext.IntegrationTransactions.Add(integrationJob);
        await dbContext.SaveChangesAsync();
        var integrationId = integrationJob.IntegrationTransactionId;
        var now = clock.UtcNow;

        foreach (var row in records)
        {
            try
            {
                var (errors, warnings, person) = await ValidateRowAsync(row);
                var personId = default(Guid?);
                var recordStatus = IntegrationTransactionRecordStatus.Success;
                var potentialDuplicate = false;
                var hasWarnings = warnings.Any();
                var rowFailureMessage = new StringBuilder();
                rowFailureMessage.Append(string.Concat(errors.Select(e => e + ",")));
                rowFailureMessage.Append(string.Concat(warnings.Select(e => e + ",")));

                if (errors.Any())
                {
                    recordStatus = IntegrationTransactionRecordStatus.Failure;
                    failureRowCount++;
                }
                else
                {
                    NationalInsuranceNumber.TryParse(row.NINumber, out var ni);
                    var potentialMatches = await GetPotentialMatchingPersonsAsync(row);
                    if (person is null)
                    {
                        //create person if incoming record is not known in trs
                        var (newPerson, personAttributes) = Person.Create(row.TRN!, row.GetFirstName()!, row.GetMiddleName()!, row.LastName!, row.GetDateOfBirth(), null, ni, (Gender?)row.Gender, clock.UtcNow, createdByTps: true);

                        if (!string.IsNullOrEmpty(row.DateOfDeath) && DateOnly.TryParseExact(row.DateOfDeath, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfDeath))
                        {
                            newPerson.SetStatus(PersonStatus.Deactivated, "Date of death received from capita import", null, null, SystemUser.Instance.UserId, clock.UtcNow, out var @event);
                            if (@event is not null)
                            {
                                await dbContext.AddEventAndBroadcastAsync(@event);
                                await dbContext.SaveChangesAsync();
                            }
                        }
                        dbContext.Persons.Add(newPerson);
                        personId = newPerson.PersonId;


                        potentialDuplicate = (potentialMatches.Outcome == TrnRequestMatchResultOutcome.PotentialMatches || potentialMatches.Outcome == TrnRequestMatchResultOutcome.DefiniteMatch);
                        var trnRequestMetadata = new TrnRequestMetadata()
                        {
                            ApplicationUserId = capitaUser.Value.CapitaTpsUserId,
                            RequestId = Guid.NewGuid().ToString(),
                            CreatedOn = now,
                            IdentityVerified = null,
                            OneLoginUserSubject = null,
                            Name = new[] { newPerson.FirstName, newPerson.MiddleName, newPerson.LastName }.GetNonEmptyValues(),
                            FirstName = newPerson.FirstName,
                            MiddleName = newPerson.MiddleName,
                            LastName = newPerson.LastName,
                            DateOfBirth = newPerson.DateOfBirth!.Value!,
                            EmailAddress = null,
                            NationalInsuranceNumber = newPerson.NationalInsuranceNumber,
                            Gender = newPerson.Gender,
                            PotentialDuplicate = potentialDuplicate,
                            Matches = new TrnRequestMatches() { MatchedPersons = potentialMatches.Outcome == TrnRequestMatchResultOutcome.PotentialMatches ? potentialMatches.PotentialMatchesPersonIds.Select(x => new TrnRequestMatchedPerson() { PersonId = x }).ToList() : [] }
                        };
                        dbContext.TrnRequestMetadata.Add(trnRequestMetadata);
                        var createdEvent = new LegacyEvents.PersonCreatedEvent
                        {
                            EventId = Guid.NewGuid(),
                            CreatedUtc = now,
                            RaisedBy = capitaUser.Value.CapitaTpsUserId,
                            PersonId = newPerson.PersonId,
                            PersonAttributes = personAttributes,
                            CreateReason = null,
                            CreateReasonDetail = null,
                            EvidenceFile = null,
                            TrnRequestMetadata = EventModels.TrnRequestMetadata.FromModel(trnRequestMetadata)
                        };
                        await dbContext.AddEventAndBroadcastAsync(createdEvent);

                        // create support task if imported record is a potential duplicate
                        if (potentialDuplicate)
                        {
                            var supportTask = SupportTask.Create(
                                SupportTaskType.TeacherPensionsPotentialDuplicate,
                                new Models.SupportTasks.TeacherPensionsPotentialDuplicateData()
                                {
                                    FileName = fileName,
                                    IntegrationTransactionId = integrationJob.IntegrationTransactionId
                                },
                                personId: personId.Value,
                                oneLoginUserSubject: null,
                                trnRequestApplicationUserId: capitaUser.Value.CapitaTpsUserId,
                                trnRequestId: trnRequestMetadata.RequestId,
                                createdBy: capitaUser.Value.CapitaTpsUserId,
                                now: now,
                                out var supportTaskCreatedEvent);

                            dbContext.SupportTasks.Add(supportTask);
                            await dbContext.AddEventAndBroadcastAsync(supportTaskCreatedEvent);
                            duplicateRowCount++;
                        }
                        else
                        {
                            successCount++;
                        }

                    }
                    else if (person is not null)
                    {
                        personId = person.PersonId;

                        // Deactivate person if date of death is provided
                        if (!string.IsNullOrEmpty(row.DateOfDeath) && DateOnly.TryParseExact(row.DateOfDeath, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfDeath))
                        {
                            person.DateOfDeath = dateOfDeath;
                            person.SetStatus(PersonStatus.Deactivated, "Date of death received from capita import", null, null, SystemUser.Instance.UserId, clock.UtcNow, out var @event);
                            if (@event is not null)
                            {
                                await dbContext.AddEventAndBroadcastAsync(@event);
                                await dbContext.SaveChangesAsync();
                            }
                        }

                        // update ni only if ni is not present && is valid.
                        if (ni is not null && person.NationalInsuranceNumber is null)
                        {
                            person.NationalInsuranceNumber = row.NINumber;
                        }
                        else if (ni is not null && person.NationalInsuranceNumber is not null && !person.NationalInsuranceNumber.Equals(row.NINumber))
                        {
                            rowFailureMessage.Append($"Warning: Attempted to update NationalInsuranceNumber from {person.NationalInsuranceNumber} to {row.NINumber}");
                            hasWarnings = true;
                        }

                        // Gender is different to incomming record.
                        if (person.Gender is not null && (int?)person.Gender != row.Gender)
                        {
                            rowFailureMessage.Append($"Warning: Attempted to update gender from {person.Gender} to {(Gender?)row.Gender},");
                            hasWarnings = true;
                        }

                        // lastname is different to incomming record
                        if (row.LastName is not null && !person.LastName.Equals(row.LastName, StringComparison.OrdinalIgnoreCase))
                        {
                            rowFailureMessage.Append($"Warning: Attempted to update lastname from {person.LastName} to {row.LastName},");
                            hasWarnings = true;
                        }
                        if (hasWarnings)
                        {
                            recordStatus = IntegrationTransactionRecordStatus.Warning;
                            warningCount++;
                        }
                        else
                        {
                            successCount++;
                        }
                    }
                }

                //write current person imported row to integrationtransactionrecord
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
        integrationJob.WarningCount = warningCount;
        integrationJob.FailureCount = failureRowCount;
        integrationJob.DuplicateCount = duplicateRowCount;
        integrationJob.ImportStatus = IntegrationTransactionImportStatus.Success;

        await dbContext.SaveChangesAsync();
        await txn.CommitAsync();
        return integrationId;
    }

    public async Task<(List<string> Errors, List<string> Warnings, Person? person)> ValidateRowAsync(CapitaImportRecord record)
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
            return (errors, warnings, null);
        }
        else if (!trnRegex.IsMatch(record.TRN))
        {
            errors.Add("Validation failed on field: TRN");
            return (errors, warnings, null);
        }

        // if a potential match is not found and the result of the import of this row would be to create a person
        // make sure that first name and last name are both present.
        var person = await dbContext.Persons.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Trn == record.TRN);
        if (person is null && string.IsNullOrEmpty(record.GetFirstName()))
        {
            errors.Add("Unable to create a new record without a firstname");
        }
        if (person is null && string.IsNullOrEmpty(record.LastName))
        {
            errors.Add("Unable to create a new record without a lastname");
        }

        if (person is not null && person.Status == PersonStatus.Deactivated)
        {
            errors.Add($"de-activated record exists for trn {record.TRN}");
        }

        //dob
        if (string.IsNullOrEmpty(record.DateOfBirth))
        {
            errors.Add("Missing required field: Date of birth");
            return (errors, warnings, person);
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
            return (errors, warnings, person);
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

        return (errors, warnings, person);
    }

    public async Task<TrnRequestMatchResult> GetPotentialMatchingPersonsAsync(CapitaImportRecord row)
    {
        var requestData = new TrnRequestMetadata()
        {
            ApplicationUserId = SystemUser.SystemUserId,
            RequestId = Guid.NewGuid().ToString(),
            CreatedOn = clock.UtcNow,
            IdentityVerified = null,
            EmailAddress = null,
            OneLoginUserSubject = null,
            FirstName = row.GetFirstName(),
            MiddleName = row.GetMiddleName(),
            LastName = row.LastName,
            PreviousFirstName = null,
            PreviousLastName = row.PreviousLastName,
            Name = [row.GetFirstName()!, row.GetMiddleName()!, row.LastName!],
            DateOfBirth = row.GetDateOfBirth()!.Value,
            NationalInsuranceNumber = row.NINumber,
            Gender = (Gender?)row.Gender
        };
        var matches = await personMatchingService.MatchFromTrnRequestAsync(requestData);
        return matches;
    }

    public async Task<Stream> GetDownloadStreamAsync(string fileName)
    {
        var fileSystemClient = dataLakeServiceClient.GetFileSystemClient(StorageContainer);
        var fileClient = fileSystemClient.GetFileClient(fileName);
        var readResponse = await fileClient.ReadAsync();
        return readResponse.Value.Content; // Stream, must be disposed by caller
    }

    private async Task<string[]> GetImportFilesAsync(CancellationToken cancellationToken)
    {
        var fileSystemClient = dataLakeServiceClient.GetFileSystemClient(StorageContainer);
        var fileNames = new List<string>();

        await foreach (var pathItem in fileSystemClient.GetPathsAsync($"{PickupFolder}/", recursive: false, cancellationToken: cancellationToken))
        {
            // Only add files, skip directories
            if (pathItem.IsDirectory == false)
            {
                fileNames.Add(pathItem.Name);
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
        return string.Empty;
    }

    public string? GetMiddleName()
    {
        var parts = FirstNameOrMiddleName?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts?.Length > 0)
            return string.Join(" ", parts, 1, parts.Length - 1);
        return string.Empty;
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
