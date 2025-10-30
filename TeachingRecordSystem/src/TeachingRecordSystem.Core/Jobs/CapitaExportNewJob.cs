using System.Globalization;
using System.Text;
using Azure.Storage.Files.DataLake;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;

public class CapitaExportNewJob([FromKeyedServices("sftpstorage")] DataLakeServiceClient dataLakeServiceClient, ILogger<CapitaExportNewJob> logger, TrsDbContext dbContext, IClock clock)
{
    public const string JobSchedule = "0 3 * * *";
    public const string LastRunDateKey = "LastRunDate";
    public const string StorageContainer = "capita-integrations";
    public const string ExportsFolder = "exports";

    public async Task<long> ExecuteAsync(CancellationToken cancellationToken)
    {
        var (lastRunDate, jobMetaData) = await GetLastRunDateAsync();
        var persons = await GetNewPersonsAsync(lastRunDate);

        // start job
        var fileName = GetFileName(clock);
        await using var txn = await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        var integrationJob = new IntegrationTransaction()
        {
            IntegrationTransactionId = 0,
            InterfaceType = IntegrationTransactionInterfaceType.CapitaExportNew,
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
                    var previousNameRow = await GetNewPersonWithPreviousLastNameAsStringRowAsync(person, cancellationToken);
                    var hasPreviousLastName = !string.IsNullOrEmpty(previousNameRow);

                    // Existing name row
                    var row = GetNewPersonAsStringRow(person, hasPreviousLastName);
                    csvWriter.WriteField(row);
                    csvWriter.NextRecord();

                    // write current person exported row to integrationtransactionrecord
                    integrationJob.IntegrationTransactionRecords.Add(new IntegrationTransactionRecord()
                    {
                        IntegrationTransactionRecordId = 0,
                        CreatedDate = clock.UtcNow,
                        RowData = row,
                        Status = IntegrationTransactionRecordStatus.Success,
                        PersonId = person.PersonId,
                        FailureMessage = null,
                        Duplicate = null,
                        HasActiveAlert = null
                    });
                    totalRowCount++;
                    successCount++;

                    // if there is a previous last name, append another row

                    if (!string.IsNullOrEmpty(previousNameRow))
                    {
                        csvWriter.WriteField(previousNameRow);
                        csvWriter.NextRecord();

                        // write person previous name exported row to integrationtransactionrecord
                        integrationJob.IntegrationTransactionRecords.Add(new IntegrationTransactionRecord()
                        {
                            IntegrationTransactionRecordId = 0,
                            CreatedDate = clock.UtcNow,
                            RowData = previousNameRow,
                            Status = IntegrationTransactionRecordStatus.Success,
                            PersonId = person.PersonId,
                            FailureMessage = null,
                            Duplicate = null,
                            HasActiveAlert = null
                        });
                        totalRowCount++;
                        successCount++;
                    }

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
                    jobMetaData.Metadata = new Dictionary<string, string>
                    {
                        {
                            LastRunDateKey, clock.UtcNow.ToString("s", CultureInfo.InvariantCulture)
                        }
                    };
                }
                else
                {
                    // insert a job metadata record if one isn't present
                    var job = new JobMetadata()
                    {
                        JobName = nameof(CapitaExportNewJob),
                        Metadata = new Dictionary<string, string>
                        {
                            {
                                LastRunDateKey, clock.UtcNow.ToString("s", CultureInfo.InvariantCulture)
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

    public async Task UploadFileAsync(Stream fileContentStream, string fileName, CancellationToken cancellationToken = default)
    {
        // Get the container client
        var fileSystemClient = dataLakeServiceClient.GetFileSystemClient(StorageContainer);
        await fileSystemClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var targetPath = $"{ExportsFolder}/{fileName}";
        var fileClient = fileSystemClient.GetFileClient(targetPath);

        await fileClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

        await fileClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        Stream uploadStream = fileContentStream;
        if (!fileContentStream.CanSeek)
        {
            var memory = new MemoryStream();
            await fileContentStream.CopyToAsync(memory, cancellationToken);
            memory.Position = 0;
            uploadStream = memory;
        }

        await fileClient.AppendAsync(uploadStream, offset: 0, cancellationToken: cancellationToken);
        await fileClient.FlushAsync(uploadStream.Length, cancellationToken: cancellationToken);

        // Dispose temporary memory stream if we created one
        if (uploadStream != fileContentStream)
        {
            await uploadStream.DisposeAsync();
        }
    }

    public string GetFileName(IClock now)
    {
        var gmt = now.UtcNow.ToGmt();
        return $"Reg01_DTR_{gmt.ToString("yyyyMMdd")}_{gmt.ToString("HHmmss", CultureInfo.InvariantCulture)}_New.txt";
    }

    /// <summary>
    /// Returns a string representation of a person who has a previous last name
    ///
    /// Always row2, accompanied with a row1 of the current persons name
    /// </summary>
    /// <param name="person"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<string> GetNewPersonWithPreviousLastNameAsStringRowAsync(Person person, CancellationToken cancellationToken)
    {
        var eventNames = EventBase.GetEventNamesForBaseType(typeof(IEventWithPersonAttributes));
        var lastNameChangeFlag = PersonAttributesChanges.LastName;

        var previousNameResult = await dbContext.Database.SqlQuery<CapitaExportNewJobResult>(
        $"""
        select
            person_ids[1] as person_id,
            payload->'OldPersonAttributes'->>'LastName' as "previous_last_name",
            created as "Created"
        from events
        where event_name = any({eventNames})
          and ((payload->>'Changes')::int & {lastNameChangeFlag}) = {lastNameChangeFlag}
          and person_id = {person.PersonId}
        order by created desc
        limit 1
        """)
       .ToListAsync(cancellationToken);

        var builder = new StringBuilder();
        var previousName = previousNameResult?.FirstOrDefault()?.PreviousLastName;

        if (string.IsNullOrEmpty(previousName))
            return string.Empty;

        if (string.IsNullOrEmpty(person.Trn))
            throw new Exception("Person does not have a trn");

        if (string.IsNullOrEmpty(previousName))
            throw new Exception($"Previous name not found in {nameof(PersonDetailsUpdatedEvent)} events.");

        var gender = " ";
        if (person.Gender.HasValue && (person.Gender == Gender.Male || person.Gender == Gender.Female))
        {
            gender = ((int)person.Gender.Value).ToString();
        }

        /*
         * Row 2, Column 1:
         * Contains a concatination of the the TRN and gender.
         * Gender should be suffixed onto the end of the TRN using the values 1 and 2 (1 = male, 2 = female)
         */
        builder.Append(person.Trn);
        builder.Append(gender);
        builder.Append(new string(' ', 9));

        /*
         * Row 2, Column 2:
         * Contains previous surname
         */
        var previousLastName = new string(' ', 54);
        if (!string.IsNullOrEmpty(previousName))
        {
            var lastName = previousName;
            previousLastName = lastName.Length > 54 ? lastName.Substring(0, 54) : lastName.PadRight(54, ' ');
        }
        builder.Append(previousLastName);
        builder.Append(new string(' ', 7));


        /*
         * Row 2, Column 3:
         * Contains update code. This should always be 2018Z981 for row 1 of a record.
         */
        builder.Append("2018Z981");

        return builder.ToString();
    }


    public string GetNewPersonAsStringRow(Person person, bool hasPreviousLastName)
    {
        var builder = new StringBuilder();

        if (string.IsNullOrEmpty(person.Trn))
            throw new Exception("Person does not have a trn");

        // ssis job either puts gender as 1,2 or a padded empty string
        var gender = " ";
        if (person.Gender.HasValue && (person.Gender == Gender.Male || person.Gender == Gender.Female))
        {
            gender = ((int)person.Gender.Value).ToString();
        }

        /**
         * Column 1:
         * Contains a concatination of the the TRN and gender.
         * Gender should be suffixed onto the end of the TRN using the values 1 and 2 (1 = male, 2 = female)
         */
        builder.Append(person.Trn);
        builder.Append(gender);
        builder.Append(new string(' ', 9));  //fixed length of 9 characters


        /*
         * Column 2:
         * Contains date of birth in format: ddmmyy
         */
        string dateOfBirth = new string(' ', 6);
        if (person.DateOfBirth.HasValue)
        {
            dateOfBirth = person.DateOfBirth.Value.ToString("ddMMyy"); // D365
        }
        builder.Append(dateOfBirth);
        builder.Append(" ");


        /*
         * Column 3:
         * contains surname
         */
        string lastName = new string(' ', 17); // fixed length of 17 spaces

        // Ensure fixed length of 17 characters, padded with spaces on the right
        if (!string.IsNullOrEmpty(person.LastName))
        {
            string personLastName = person.LastName;
            lastName = personLastName.Length > 17 ? personLastName.Substring(0, 17) : personLastName.PadRight(17, ' ');
        }
        builder.Append(lastName);
        builder.Append(hasPreviousLastName == true ? "1" : " ");

        /*
         * Column 4:
         * Contains firstname and any middle names.
         * If the record contains s previous surname the value '1' should appear before the First name.
         * The surname should appear in Row 2.
         */
        string firstName = person.FirstName;
        string middleName = person.MiddleName;
        string name = new string(' ', 35);
        if (!string.IsNullOrEmpty(string.Format("{0} {1}", firstName, middleName)))
        {
            var firstAndMiddleName = string.Format("{0} {1}", firstName, middleName);
            name = firstAndMiddleName.Length > 35 ? firstAndMiddleName.Substring(0, 35) : firstAndMiddleName.PadRight(35, ' ');
        }
        builder.Append(name);
        builder.Append(" ");


        /*
         * Column 5:
         * Contains update code. This should always be 1018Z981 for row 1 of a record.
         */
        builder.Append("1018Z981");

        return builder.ToString();
    }

    public async Task<(DateTime?, JobMetadata?)> GetLastRunDateAsync()
    {
        var item = await dbContext.JobMetadata.AsNoTracking()
            .FirstOrDefaultAsync(i => i.JobName == nameof(CapitaExportNewJob));

        DateTime? lastRunDate = null;

        if (item?.Metadata != null && item.Metadata.TryGetValue("LastRunDate", out var dateStr))
        {
            if (DateTime.TryParse(dateStr, out var parsed))
            {
                lastRunDate = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
            }
        }

        return (lastRunDate, item);
    }

    public async Task<List<Person>> GetNewPersonsAsync(DateTime? lastRunDate)
    {
        var persons = await dbContext.Persons.Where(x => x.CreatedOn > lastRunDate &&
            x.Trn != null &&
            x.CreatedByTps != true &&
            (x.Gender == Gender.Male || x.Gender == Gender.Female)).ToListAsync();
        return persons;
    }
}

public class CapitaExportNewJobResult
{
    public required Guid PersonId { get; set; }
    public required string PreviousLastName { get; set; }
    public required DateTime Created { get; set; }
}
