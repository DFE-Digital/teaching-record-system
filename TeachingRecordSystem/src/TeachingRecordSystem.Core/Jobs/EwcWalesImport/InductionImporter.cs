using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

namespace TeachingRecordSystem.Core.Jobs.EwcWalesImport;

public class InductionImporter
{
    private const string DATE_FORMAT = "dd/MM/yyyy";
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;
    private readonly ILogger<InductionImporter> _logger;
    private readonly TrsDbContext _dbContext;

    public InductionImporter(ICrmQueryDispatcher crmQueryDispatcher, ILogger<InductionImporter> logger, TrsDbContext dbContext)
    {
        _crmQueryDispatcher = crmQueryDispatcher;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<InductionImportResult> ImportAsync(StreamReader csvReaderStream, string fileName)
    {
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            TrimOptions = TrimOptions.Trim
        };
        using (var csv = new CsvReader(csvReaderStream, csvConfig))
        {
            var integrationJob = new CreateIntegrationTransactionQuery()
            {
                StartDate = DateTime.Now,
                TypeId = (int)dfeta_IntegrationInterface.GTCWalesImport,
                FileName = fileName
            };
            var integrationId = await _crmQueryDispatcher.ExecuteQueryAsync(integrationJob);

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
                using var rowTransaction = _crmQueryDispatcher.CreateTransactionRequestBuilder();

                try
                {
                    var lookupData = await GetLookupDataAsync(row);
                    var validationFailures = Validate(row, lookupData);
                    personId = lookupData.Person?.ContactId;

                    //append non processable errors to list of failures that will be a line in
                    //the IntegrationTransaction (job) failuremessage field.
                    if (validationFailures.Errors.Any())
                    {
                        failureRowCount++;
                        foreach (var error in validationFailures.Errors)
                        {
                            failureMessage.AppendLine(error);
                            itrFailureMessage.AppendLine(error);
                        }

                        foreach (var validationMessage in validationFailures.ValidationFailures)
                        {
                            itrFailureMessage.AppendLine(validationMessage);
                            failureMessage.AppendLine(validationMessage);
                        }
                    }
                    else
                    {
                        rowTransaction.AppendQuery(new CreateDqtOutboxMessageTransactionalQuery(new AddInductionExemptionMessage
                        {
                            PersonId = lookupData.Person!.Id,
                            ExemptionReasonId = InductionExemptionReason.PassedInWalesId,
                            TrsUserId = DataStore.Postgres.Models.SystemUser.SystemUserId
                        }));

                        if (lookupData.HasActiveAlerts)
                        {
                            var query = new CreateTaskTransactionalQuery()
                            {
                                ContactId = lookupData.Person!.Id,
                                Category = "GTC Wales Import",
                                Description = "QTS/Induction update with Active Sanction",
                                Subject = "Notification for QTS Unit Team",
                                ScheduledEnd = DateTime.Now
                            };
                            rowTransaction.AppendQuery(query);
                        }

                        //soft validation errors can be appended to the IntegrationTransactionRecord Failure message
                        foreach (var validationMessage in validationFailures.ValidationFailures)
                        {
                            itrFailureMessage.AppendLine(validationMessage);
                            failureMessage.AppendLine(validationMessage);
                        }

                        //increase failurecount if row is processable or if there are validation failures
                        //else increase success counter
                        if (validationFailures.Errors.Any())
                        {
                            failureRowCount++;
                        }
                        else
                        {
                            successCount++;
                        }
                    }

                    //create ITR row with status of import row
                    var createIntegrationTransactionRecord = new CreateIntegrationTransactionRecordTransactionalQuery()
                    {
                        IntegrationTransactionId = integrationId,
                        Reference = totalRowCount.ToString(),
                        ContactId = personId,
                        InitialTeacherTrainingId = null,
                        QualificationId = null,
                        InductionId = null,
                        InductionPeriodId = null,
                        DuplicateStatus = null,
                        FailureMessage = itrFailureMessage.ToString(),
                        StatusCode = validationFailures.Errors.Count == 0 ? dfeta_integrationtransactionrecord_StatusCode.Success : dfeta_integrationtransactionrecord_StatusCode.Fail,
                        RowData = ConvertToCSVString(row),
                        FileName = fileName
                    };
                    rowTransaction.AppendQuery(createIntegrationTransactionRecord);

                    //update IntegrationTransaction so that it's always up to date with
                    //counts of rows
                    var updateIntegrationTransactionQuery = new UpdateIntegrationTransactionTransactionalQuery()
                    {
                        IntegrationTransactionId = integrationId,
                        EndDate = null,
                        TotalCount = totalRowCount,
                        SuccessCount = successCount,
                        DuplicateCount = 0,
                        FailureCount = failureRowCount,
                        FailureMessage = itrFailureMessage.ToString()
                    };
                    rowTransaction.AppendQuery(updateIntegrationTransactionQuery);
                    await rowTransaction.ExecuteAsync();
                }
                catch (Exception e)
                {
                    failureRowCount++;
                    _logger.LogError(e.ToString());
                }
            }

            var updateIntTrxQuery = new UpdateIntegrationTransactionTransactionalQuery()
            {
                IntegrationTransactionId = integrationId,
                EndDate = DateTime.Now,
                TotalCount = totalRowCount,
                SuccessCount = successCount,
                DuplicateCount = duplicateRowCount,
                FailureCount = failureRowCount,
                FailureMessage = failureMessage.ToString()
            };

            using var txn = _crmQueryDispatcher.CreateTransactionRequestBuilder();
            txn.AppendQuery(updateIntTrxQuery);
            await txn.ExecuteAsync();

            return new InductionImportResult(totalRowCount, successCount, duplicateRowCount, failureRowCount, failureMessage.ToString(), integrationId);
        }
    }

    public string ConvertToCSVString(EwcWalesInductionImportData row)
    {
        using (var writer = new StringWriter())
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecord(row);
            csv.NextRecord();
            return writer.ToString();
        }
    }

    public async Task<InductionImportLookupData> GetLookupDataAsync(EwcWalesInductionImportData row)
    {
        var (personMatchStatus, contact) = await FindMatchingTeacherRecordAsync(row);

        bool hasActiveAlerts = false;
        InductionStatus? inductionStatus = null;

        if (contact is not null)
        {
            hasActiveAlerts = await _dbContext.Alerts.AnyAsync(x => x.PersonId == contact.Id && x.IsOpen);

            inductionStatus = await _dbContext.Persons
                .Where(x => x.PersonId == contact.Id)
                .Select(p => p.InductionStatus)
                .SingleAsync();
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
            if (!DateOnly.TryParseExact(row.DateOfBirth, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                errors.Add("Validation Failed: Invalid Date of Birth");
            }
        }

        //InductionPassedDate
        if (String.IsNullOrEmpty(row.StartDate))
        {
            errors.Add("Missing Induction Start date");
        }
        else
        {
            if (!DateOnly.TryParseExact(row.StartDate, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                errors.Add("Validation Failed: Invalid Induction start date");
            }
        }

        //InductionPassedDate
        if (String.IsNullOrEmpty(row.PassedDate))
        {
            errors.Add("Missing Induction passed date");
        }
        else
        {
            if (!DateOnly.TryParseExact(row.PassedDate, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                errors.Add("Validation Failed: Invalid Induction passed date");
            }
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

        DateOnly.TryParseExact(row.PassedDate, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly passedDate);
        if (lookups.Person != null && lookups.Person!.dfeta_QTSDate.HasValue && passedDate < lookups.Person.dfeta_QTSDate?.ToDateOnlyWithDqtBstFix(isLocalTime: false))
        {
            errors.Add($"Induction passed date cannot be before Qts Date.");
        }

        if (lookups.Person != null && lookups.Person!.dfeta_qtlsdate.HasValue)
        {
            errors.Add("existing qtls; may need to update Induction Status");
        }

        switch (lookups.InductionStatus)
        {
            case InductionStatus.Passed:
            case InductionStatus.Exempt:
            case InductionStatus.Failed:
            case InductionStatus.FailedInWales:
            case InductionStatus.InProgress:
                errors.Add($"Teacher with TRN {row.ReferenceNumber} completed induction already or is progress.");
                break;
        }

        return (validationFailures, errors);
    }

    public async Task<(ContactLookupResult, Contact? contact)> FindMatchingTeacherRecordAsync(EwcWalesInductionImportData item)
    {
        var contact = await _crmQueryDispatcher.ExecuteQueryAsync(
            new GetActiveContactByTrnQuery(item.ReferenceNumber,
                new ColumnSet(
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.BirthDate,
                    Contact.Fields.dfeta_QTSDate,
                    Contact.Fields.dfeta_qtlsdate)));

        if (contact == null)
        {
            return (ContactLookupResult.NoMatch, null);
        }

        if (DateOnly.TryParseExact(item.DateOfBirth, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly dob) && contact!.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false) != dob)
        {
            return (ContactLookupResult.TrnAndDateOfBirthMatchFailed, null);
        }

        var qtsRegistrations = await _crmQueryDispatcher.ExecuteQueryAsync(
                new GetActiveQtsRegistrationsByContactIdsQuery(
                    new[] { contact!.ContactId!.Value },
                    new ColumnSet(
                        dfeta_qtsregistration.Fields.dfeta_PersonId,
                        dfeta_qtsregistration.Fields.dfeta_QTSDate,
                        dfeta_qtsregistration.Fields.dfeta_TeacherStatusId)
                    )
                );

        if (qtsRegistrations[contact.Id].Length > 0)
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
        public required Contact? Person { get; set; }
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
