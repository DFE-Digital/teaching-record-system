using System.Globalization;
using System.Text;
using System.Text.Json;
using Azure.Storage.Blobs;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class CapitaExportAmendJob(BlobServiceClient blobServiceClient, ILogger<CapitaExportAmendJob> logger, TrsDbContext dbContext, IClock clock, IOptions<CapitaTpsUserOption> capitaUser)
{
    public const string JobSchedule = "0 3 * * *";
    public const string LastRunDateKey = "LastRunDate";
    public const string StorageContainer = "dqt-integrations";
    public const string EXPORTS_FOLDER = "capita/exports";

    public async Task<long> ExecuteAsync(CancellationToken cancellationToken)
    {
        var (lastRunDate, jobMetaData) = await GetLastRunDateAsync();
        var persons = await GetUpdatedPersonsAsync(lastRunDate, cancellationToken);

        // start job
        var fileName = GetFileName(clock);
        await using var txn = await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        var integrationJob = new IntegrationTransaction()
        {
            IntegrationTransactionId = 0,
            InterfaceType = IntegrationTransactionInterfaceType.CapitaExportAmend,
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

        // running counts for job
        var totalRowCount = 0;
        var successCount = 0;
        var duplicateRowCount = 0;
        var failureRowCount = 0;
        var failureMessage = new StringBuilder();

        // process file
        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true);
        using (var csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            try
            {
                //write header
                foreach (var person in persons)
                {
                    var amendedPersonRow = GetPersonAmendedRow(person.Item1, person.Item2);

                    // Existing name row
                    csvWriter.WriteField(amendedPersonRow);
                    csvWriter.NextRecord();

                    // write current person exported row to integrationtransactionrecord
                    integrationJob.IntegrationTransactionRecords.Add(new IntegrationTransactionRecord()
                    {
                        IntegrationTransactionRecordId = 0,
                        CreatedDate = clock.UtcNow,
                        RowData = amendedPersonRow,
                        Status = IntegrationTransactionRecordStatus.Success,
                        PersonId = person.Item1.PersonId,
                        FailureMessage = null,
                        Duplicate = null,
                        HasActiveAlert = null
                    });
                    totalRowCount++;
                    successCount++;


                    //update IntegrationTransaction so that it's always up to date with counts of rows
                    integrationJob.TotalCount = totalRowCount;
                    integrationJob.FailureCount = failureRowCount;
                    integrationJob.SuccessCount = successCount;
                    integrationJob.DuplicateCount = duplicateRowCount;
                    await dbContext.SaveChangesAsync();

                }
                streamWriter.Flush();

                // upload file contents to storage container
                memoryStream.Position = 0;
                await UploadFileAsync(memoryStream, fileName);
            }
            catch (Exception e)
            {
                failureRowCount++;
                logger.LogError(e.ToString());
            }
            finally
            {
                // mark job as complete
                integrationJob.TotalCount = totalRowCount;
                integrationJob.SuccessCount = successCount;
                integrationJob.FailureCount = failureRowCount;
                integrationJob.DuplicateCount = duplicateRowCount;
                integrationJob.ImportStatus = IntegrationTransactionImportStatus.Success;

                //update last run date
                if (jobMetaData != null)
                {
                    jobMetaData.Metadata = new Dictionary<string, object>
                    {
                        {
                            LastRunDateKey, clock.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                        }
                    };
                }
                else
                {
                    // insert a job metadata record if one isn't present
                    var job = new JobMetadata()
                    {
                        JobName = nameof(CapitaExportAmendJob),
                        Metadata = new Dictionary<string, object>
                        {
                            {
                                LastRunDateKey, clock.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                            }
                        }
                    };
                    dbContext.JobMetadata.Add(job);
                }
                await dbContext.SaveChangesAsync();
                await txn.CommitAsync();
            }
        }
        return integrationJob.IntegrationTransactionId;
    }

    public async Task UploadFileAsync(Stream fileContentStream, string fileName)
    {
        // Get the container client
        var containerClient = blobServiceClient!.GetBlobContainerClient(StorageContainer);
        await containerClient.CreateIfNotExistsAsync();

        var targetFileName = $"{EXPORTS_FOLDER}/{fileName}";

        // Get the blob client for the target file
        var blobClient = containerClient.GetBlobClient(targetFileName);

        // Upload the stream
        await blobClient.UploadAsync(fileContentStream, overwrite: true);
    }

    public string GetFileName(IClock now)
    {
        return $"Reg01_DTR_{now.UtcNow.ToString("yyyyMMdd")}_{now.UtcNow.ToString("HHmmss")}_Amend.txt";
    }

    public string GetPersonAmendedRow(CapitaExportAmendJobResult person, CapitaAmendExportType type)
    {
        var sb = new StringBuilder();
        var updateCode = GetUpdateCode(type);

        /*
         * Column 1:
         * Contains a concatination of the the TRN, gender and the first six letters of the surname. 
         * Gender should be suffixed onto the end of the TRN using the values 1 and 2 (1 = male, 2 = female). 
         * The gender and surname are separated by the values: //
         */
        var trn = person!.Trn;
        if (!string.IsNullOrEmpty(trn) && trn.Length != 7)
            throw new Exception("Person does not have a trn");
        var gender = " ";
        if (person.Gender.HasValue && (person.Gender == Gender.Male || person.Gender == Gender.Female))
        {
            gender = ((int)person.Gender.Value).ToString();
        }
        string lastName = new string(' ', 6); // fixed length of 17 spaces
        if (!string.IsNullOrEmpty(person.LastName))
        {
            string personLastName = person.LastName;
            lastName = personLastName.Length > 6 ? personLastName.Substring(0, 6) : personLastName.PadRight(6, ' ');
        }
        sb.Append(trn);
        sb.Append(gender);
        sb.Append("//");
        sb.Append(lastName);
        sb.Append(" ");

        /*
         * Column 2:
         * Contains DOB suffixed by the value: *
         * This column will be blank if the row relates to a change to NI number. Format should be: ddmmyy*
         */
        string dateOfBirth = new string(' ', 7);
        if (person.DateOfBirth.HasValue && type == CapitaAmendExportType.DateOfBirth)
        {
            dateOfBirth = person.DateOfBirth.Value.ToString("ddMMyy*");
        }
        sb.Append(dateOfBirth);
        sb.Append(" ");

        /*
         * Column 3:
         * Contains NI number. This column will be blank if the row relates to a change to surname
         */
        var niNumber = new string(' ', 9);
        if (!string.IsNullOrEmpty(person.NationalInsuranceNumber) && type == CapitaAmendExportType.NINumber)
        {
            niNumber = person.NationalInsuranceNumber;
        }
        sb.Append(niNumber);
        sb.Append(' ', 44);

        /*
         * Column 4: fixed length of 8 characters
         * Contains update code. 
         * For NI number changes the code is: 321ZE2*
         * For DOB chnages the code is: 1211ZE1*
         */
        sb.Append(updateCode);

        return sb.ToString();
    }

    public string GetUpdateCode(CapitaAmendExportType type) =>
        type switch
        {
            CapitaAmendExportType.NINumber => " 321ZE2*", // prefixed with a space to make it 8 characters long
            CapitaAmendExportType.DateOfBirth => "1211ZE1*",
            _ => throw new Exception("Invalid Amend Export Type")
        };

    public async Task<(DateTime?, JobMetadata?)> GetLastRunDateAsync()
    {
        var item = await dbContext.JobMetadata.AsNoTracking()
            .FirstOrDefaultAsync(i => i.JobName == nameof(CapitaExportAmendJob));

        DateTime? lastRunDate = null;

        if (item?.Metadata != null && item.Metadata.TryGetValue("LastRunDate", out var obj))
        {
            string? dateStr = obj switch
            {
                JsonElement json => json.ValueKind switch
                {
                    JsonValueKind.String => json.GetString(),
                    JsonValueKind.Number when json.TryGetDateTime(out var dt) => dt.ToString("o"),
                    _ => null
                },
                string str => str,
                _ => null
            };

            if (dateStr != null && DateTime.TryParse(dateStr, out var parsed))
            {
                lastRunDate = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
            }
        }

        return (lastRunDate, item);
    }

    public async Task<List<(CapitaExportAmendJobResult, CapitaAmendExportType)>> GetUpdatedPersonsAsync(DateTime? lastRunDate, CancellationToken cancellationToken)
    {
        var capitaUserId = capitaUser.Value.CapitaTpsUserId;
        var capitaPersonsWithEvents = new List<(CapitaExportAmendJobResult, CapitaAmendExportType)>();
        var combinedChangeFlags = PersonAttributesChanges.NationalInsuranceNumber | PersonAttributesChanges.DateOfBirth;
        var processedPersons = new List<Guid>();

        // eligibile events
        var eventNames = EventBase.GetEventNamesForBaseType(typeof(IEventWithPersonAttributes));
        var filteredGenders = new[] { Gender.Male, Gender.Female };
        var changeEvents = await dbContext.Database.SqlQuery<CapitaExportAmendJobResult>(
        $"""
            select 
                e.person_ids[1] as person_id,
                e.payload->'PersonAttributes'->>'NationalInsuranceNumber' as "national_insurance_number",
                (e.payload->'PersonAttributes'->>'DateOfBirth')::date as "date_of_birth",
                e.created as "Created",
                p.trn as "Trn",
                p.gender as "Gender",
                (e.payload->>'Changes')::int as "change_type",
                p.last_name
            from events e
            inner join persons p on p.person_id = e.person_ids[1]
            where event_name = any({eventNames})
                and p.trn is not null
                and(e.payload->'PersonAttributes'->> 'Gender')::int = any({filteredGenders})
                and(e.payload->> 'RaisedBy')::uuid != {capitaUserId}
                and ((e.payload->>'Changes')::int & {combinedChangeFlags}) != 0
                and e.created > {lastRunDate}
            order by "Created" desc, "Trn" desc
        """).ToListAsync(cancellationToken);

        foreach (var person in changeEvents)
        {
            //there can be more than one event for dob/ni changes that are returned since the last time the job was
            //run. The results are returned with most recent events first, if an event has already processed
            //skip it.
            if (processedPersons.Contains(person.PersonId))
                continue;

            processedPersons.Add(person.PersonId);
            if (person.ChangeType.HasFlag(PersonAttributesChanges.NationalInsuranceNumber))
            {
                capitaPersonsWithEvents.Add((person, CapitaAmendExportType.NINumber));
            }
            if (person.ChangeType.HasFlag(PersonAttributesChanges.DateOfBirth))
            {
                capitaPersonsWithEvents.Add((person, CapitaAmendExportType.DateOfBirth));
            }
        }
        return capitaPersonsWithEvents;
    }
}

public class CapitaExportAmendJobResult
{
    public required Guid PersonId { get; set; }
    public required string? NationalInsuranceNumber { get; set; }
    public required DateTime? Created { get; set; }
    public required string Trn { get; set; }
    public required DateOnly? DateOfBirth { get; set; }
    public required Gender? Gender { get; set; }
    public required PersonAttributesChanges ChangeType { get; set; }
    public required string LastName { get; set; }
}


public enum CapitaAmendExportType
{
    DateOfBirth,
    NINumber
}

public class CapitaTpsUserOption
{
    public required Guid CapitaTpsUserId { get; set; }
}

