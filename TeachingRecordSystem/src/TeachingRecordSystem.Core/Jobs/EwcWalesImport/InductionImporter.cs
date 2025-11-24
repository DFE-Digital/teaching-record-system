using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Jobs.EwcWalesImport;

public class InductionImporter
{
    private const string DateFormat = "dd/MM/yyyy";
    private readonly ILogger<InductionImporter> _logger;
    private readonly TrsDbContext _dbContext;
    private readonly IClock _clock;

    public InductionImporter(ILogger<InductionImporter> logger, TrsDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _logger = logger;
        _clock = clock;
    }

    public async Task<InductionImportResult> ImportAsync(StreamReader csvReaderStream, string fileName)
    {
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            TrimOptions = TrimOptions.Trim
        };
        using (var csv = new CsvReader(csvReaderStream, csvConfig))
        {
            await using var txn = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
            var integrationJob = new IntegrationTransaction()
            {
                IntegrationTransactionId = 0,
                InterfaceType = IntegrationTransactionInterfaceType.EwcWales,
                ImportStatus = IntegrationTransactionImportStatus.InProgress,
                TotalCount = 0,
                SuccessCount = 0,
                WarningCount = 0,
                FailureCount = 0,
                DuplicateCount = 0,
                FileName = fileName,
                CreatedDate = _clock.UtcNow,
                IntegrationTransactionRecords = new List<IntegrationTransactionRecord>()
            };
            _dbContext.IntegrationTransactions.Add(integrationJob);
            await _dbContext.SaveChangesAsync();
            var integrationId = integrationJob.IntegrationTransactionId;

            var records = csv.GetRecords<EwcWalesInductionImportData>().ToList();
            var validationMessages = new List<string>();

            var totalRowCount = 0;
            var successCount = 0;
            var duplicateRowCount = 0;
            var failureRowCount = 0;
            var failureMessage = new StringBuilder();
            foreach (var row in records)
            {
                totalRowCount++;
                Guid? personId = null;
                Guid itrId = Guid.NewGuid();
                var itrFailureMessage = new StringBuilder();

                try
                {
                    var lookupData = await GetLookupDataAsync(row);
                    var validationFailures = Validate(row, lookupData);
                    personId = lookupData.Person?.PersonId;
                    DateOnly? awardedDate = null;
                    DateOnly? startDate = null;
                    if (DateOnly.TryParseExact(row.PassedDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedQtsDate))
                    {
                        awardedDate = parsedQtsDate;
                    }

                    if (DateOnly.TryParseExact(row.StartDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parseStartDate))
                    {
                        startDate = parseStartDate;
                    }

                    //append non processable errors to list of failures that will be a line in
                    //the IntegrationTransaction (job) failuremessage field.
                    if (validationFailures.Errors.Count != 0)
                    {
                        failureRowCount++;
                        foreach (var error in validationFailures.Errors)
                        {
                            failureMessage.AppendLine(error);
                            itrFailureMessage.AppendLine(error);
                        }

                        foreach (var validationMessage in validationFailures.ValidationFailures)
                        {
                            itrFailureMessage.AppendLine($"{validationMessage};");
                            failureMessage.AppendLine($"{validationMessage};");
                        }
                    }
                    else
                    {
                        if (lookupData.Person != null)
                        {
                            lookupData.Person.TrySetWelshInductionStatus(
                                 awardedDate.HasValue,
                                 startDate,
                                 awardedDate,
                                  SystemUser.SystemUserId,
                                 _clock.UtcNow,
                                 out var updatedEvent);

                            if (updatedEvent is not null)
                            {
                                await _dbContext.AddEventAndBroadcastAsync(updatedEvent);
                            }
                        }

                        //soft validation errors can be appended to the IntegrationTransactionRecord Failure message
                        foreach (var validationMessage in validationFailures.ValidationFailures)
                        {
                            itrFailureMessage.AppendLine($"{validationMessage};");
                            failureMessage.AppendLine($"{validationMessage};");
                        }

                        //increase failurecount if row is processable or if there are validation failures
                        //else increase success counter
                        if (validationFailures.Errors.Count != 0)
                        {
                            failureRowCount++;
                        }
                        else
                        {
                            successCount++;
                        }
                    }

                    //create ITR row with status of import row
                    integrationJob.IntegrationTransactionRecords.Add(new IntegrationTransactionRecord()
                    {
                        IntegrationTransactionRecordId = 0,
                        CreatedDate = _clock.UtcNow,
                        RowData = ConvertToCsvString(row),
                        Status = validationFailures.Errors.Count == 0 ? IntegrationTransactionRecordStatus.Success : IntegrationTransactionRecordStatus.Failure,
                        PersonId = lookupData.Person != null ? lookupData.Person!.PersonId : null,
                        FailureMessage = itrFailureMessage.ToString(),
                        Duplicate = null,
                        HasActiveAlert = lookupData.HasActiveAlerts
                    });

                    //update IntegrationTransaction so that it's always up to date with
                    //counts of rows
                    integrationJob.TotalCount = totalRowCount;
                    integrationJob.FailureCount = failureRowCount;
                    integrationJob.SuccessCount = successCount;
                    integrationJob.DuplicateCount = duplicateRowCount;
                    await _dbContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    failureRowCount++;
                    _logger.LogError(e.ToString());
                }
            }

            //update integration transaction counts as job has finished
            integrationJob.TotalCount = totalRowCount;
            integrationJob.SuccessCount = successCount;
            integrationJob.FailureCount = failureRowCount;
            integrationJob.DuplicateCount = duplicateRowCount;
            integrationJob.ImportStatus = IntegrationTransactionImportStatus.Success;
            await _dbContext.SaveChangesAsync();
            await txn.CommitAsync();

            return new InductionImportResult(totalRowCount, successCount, duplicateRowCount, failureRowCount, failureMessage.ToString(), integrationId);
        }
    }

    public async Task<InductionImportLookupData> GetLookupDataAsync(EwcWalesInductionImportData row)
    {
        var (personMatchStatus, contact) = await FindMatchingTeacherRecordAsync(row);

        bool hasActiveAlerts = false;
        InductionStatus? inductionStatus = null;

        if (contact is not null)
        {
            hasActiveAlerts = await _dbContext.Alerts.AnyAsync(x => x.PersonId == contact.PersonId && x.IsOpen);

            inductionStatus = contact.InductionStatus;
        }

        var lookupData = new InductionImportLookupData
        {
            Person = contact,
            PersonMatchStatus = personMatchStatus,
            HasActiveAlerts = hasActiveAlerts,
            InductionStatus = inductionStatus
        };
        return lookupData;
    }

    public (List<string> ValidationFailures, List<string> Errors) Validate(EwcWalesInductionImportData row, InductionImportLookupData lookups)
    {
        var validationFailures = new List<string>();
        var errors = new List<string>();

        //ReferenceNumber/Trn
        if (String.IsNullOrEmpty(row.ReferenceNumber))
        {
            errors.Add("Missing Reference No");
        }

        //Date Of birth
        if (String.IsNullOrEmpty(row.DateOfBirth))
        {
            errors.Add("Missing Date of Birth");
        }
        else
        {
            if (!DateOnly.TryParseExact(row.DateOfBirth, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                errors.Add("Validation Failed: Invalid Date of Birth");
            }
        }

        //InductionStartDate
        DateOnly? startDate = null;
        if (String.IsNullOrEmpty(row.StartDate))
        {
            errors.Add("Missing Induction Start date");
        }
        else
        {
            if (!DateOnly.TryParseExact(row.StartDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedStartDate))
            {
                errors.Add("Validation Failed: Invalid Induction start date");
            }
            else
            {
                startDate = parsedStartDate;
            }
        }

        //InductionPassedDate
        DateOnly? passedDate = null;
        if (String.IsNullOrEmpty(row.PassedDate))
        {
            errors.Add("Missing Induction passed date");
        }
        else
        {
            if (!DateOnly.TryParseExact(row.PassedDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedPassedDate))
            {
                errors.Add("Validation Failed: Invalid Induction passed date");
            }
            else
            {
                passedDate = parsedPassedDate;
            }
        }

        //Induction passed date cannot be before start date
        if (passedDate.HasValue && startDate.HasValue && passedDate < startDate)
        {
            errors.Add("Induction passed date cannot be before start date");
        }

        switch (lookups.PersonMatchStatus)
        {
            case ContactLookupResult.NoAssociatedQts:
                break;
            case ContactLookupResult.NoMatch:
                errors.Add($"Teacher with TRN {row.ReferenceNumber} was not found.");
                break;
            case ContactLookupResult.TrnAndDateOfBirthMatchFailed:
                errors.Add($"For TRN {row.ReferenceNumber} Date of Birth does not match with the existing record.");
                break;
        }

        if (lookups.Person != null && lookups.Person!.QtsDate.HasValue && passedDate.HasValue && passedDate < lookups.Person.QtsDate)
        {
            errors.Add($"Induction passed date cannot be before Qts Date.");
        }

        if (startDate.HasValue && lookups.Person != null && startDate < lookups.Person.QtsDate)
        {
            errors.Add("Induction start date cannot be before qts date");
        }

        switch (lookups.InductionStatus)
        {
            case InductionStatus.Passed:
            case InductionStatus.Failed:
            case InductionStatus.FailedInWales:
            case InductionStatus.InProgress:
                errors.Add($"Teacher with TRN {row.ReferenceNumber} completed induction already or is progress.");
                break;
        }

        return (validationFailures, errors);
    }
    public string ConvertToCsvString(EwcWalesInductionImportData row)
    {
        using (var writer = new StringWriter())
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteHeader<EwcWalesInductionImportData>();
            csv.NextRecord();
            csv.WriteRecord(row);
            csv.NextRecord();
            return writer.ToString();
        }
    }

    public async Task<(ContactLookupResult, Person? contact)> FindMatchingTeacherRecordAsync(EwcWalesInductionImportData item)
    {
        var contact = await _dbContext.Persons.Include(x => x.Qualifications).SingleOrDefaultAsync(x => x.Trn == item.ReferenceNumber && x.Status == PersonStatus.Active);

        if (contact == null)
        {
            return (ContactLookupResult.NoMatch, null);
        }

        if (DateOnly.TryParseExact(item.DateOfBirth, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly dob) && contact!.DateOfBirth != dob)
        {
            return (ContactLookupResult.TrnAndDateOfBirthMatchFailed, null);
        }

        var personQtsDate = contact.QtsDate;
        if (personQtsDate.HasValue)
        {
            return (ContactLookupResult.TeacherHasQts, contact);
        }
        else
        {
            return (ContactLookupResult.NoAssociatedQts, contact);
        }
    }

    public class InductionImportLookupData
    {
        public required Person? Person { get; set; }
        public required ContactLookupResult? PersonMatchStatus { get; set; }
        public required bool HasActiveAlerts { get; set; }
        public required InductionStatus? InductionStatus { get; set; }
    }
}

public enum ContactLookupResult
{
    NoMatch,
    TrnAndDateOfBirthMatchFailed,
    NoAssociatedQts,
    TeacherHasQts
}
