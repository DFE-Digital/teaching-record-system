using System.Globalization;
using System.Text;
using System.Text.Json;
using Azure.Storage.Blobs;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class CapitaExportNewJob(BlobServiceClient blobServiceClient, ILogger<CapitaExportNewJob> logger, TrsDbContext dbContext, IClock clock)
{
    public const string JobSchedule = "0 3 * * *";
    public const string LastRunDateKey = "LastRunDate";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var lastRunDate = await GetLastRunDateAsync();
        var persons = await GetNewPersonsAsync(lastRunDate);

        // start job
        var fileName = $"Reg01_DTR_{clock.UtcNow.ToString("yyyyMMdd")}_{clock.UtcNow.ToString("HHmmss")}_New.txt";
        await using var txn = await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        var integrationJob = new IntegrationTransaction()
        {
            IntegrationTransactionId = 0,
            InterfaceType = IntegrationTransactionInterfaceType.CapitaExportNew,
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

        // process file
        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true);
        using (var csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            //write header
            foreach (var person in persons)
            {
                var row = GetPersonAsStringRow(person);
                csvWriter.WriteRecord(row);
            }
        }

        // mark job as complete
        integrationJob.TotalCount = 0;
        integrationJob.SuccessCount = 0;
        integrationJob.FailureCount = 0;
        integrationJob.DuplicateCount = 0;
        integrationJob.ImportStatus = IntegrationTransactionImportStatus.Success;
        await dbContext.SaveChangesAsync();
        await txn.CommitAsync();

        // update last run date
        //if (item != null)
        //{
        //    item.Metadata = new Dictionary<string, object>
        //    {
        //        { LastRunDateKey, clock.UtcNow }
        //    };
        //}
        //await dbContext.SaveChangesAsync();
    }

    public string GetPersonAsStringRow(Person person)
    {
        var builder = new StringBuilder();

        if (string.IsNullOrEmpty(person.Trn))
            throw new Exception("Person does not have a trn");

        // ssis job either puts gender as 1,2 or a padded empty string
        var gender = " ";
        if(person.Gender.HasValue && (person.Gender == Gender.Male || person.Gender == Gender.Female))
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
        builder.Append(" "); //TODO: 1 if there is a previous last name or empty strng if not


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

    public async Task<DateTime?> GetLastRunDateAsync()
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
        return lastRunDate;
    }

    public async Task<List<Person>> GetNewPersonsAsync(DateTime? lastRunDate)
    {
        var persons = await dbContext.Persons.Where(x => x.CapitaTrnChangedOn > lastRunDate && x.Trn != null).ToListAsync();
        return persons;
    }
}
