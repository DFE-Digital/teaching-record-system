using System.ComponentModel.Design;
using System.Globalization;
using System.Text;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Jobs.EWCWalesImport;

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
        using (var csv = new CsvReader(csvReaderStream, CultureInfo.InvariantCulture))
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
                Guid? inductionId = null;
                Guid? inductionPeriodId = null;
                Guid itrId = Guid.NewGuid();
                var itrFailureMessage = new StringBuilder();
                using var rowTransaction = _crmQueryDispatcher.CreateTransactionRequestBuilder();

                try
                {
                    var lookupData = await GetLookupDataAsync(row);
                    var validationFailures = Validate(row, lookupData);
                    personId = lookupData.Person?.ContactId;
                    inductionId = lookupData.Induction?.Id;

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
                        // if contact does not have an associated induction - create one with the data from the imported file row
                        // else if there is an associated induction update status & passed date with the data from the imported file row
                        if (!inductionId.HasValue)
                        {
                            inductionId = Guid.NewGuid();
                            var createInductionQuery = new CreateInductionTransactionalQuery()
                            {
                                Id = inductionId.Value,
                                ContactId = personId!.Value,
                                StartDate = DateTime.ParseExact(row.StartDate, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None),
                                CompletionDate = DateTime.ParseExact(row.PassedDate, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None),
                                InductionStatus = dfeta_InductionStatus.PassedinWales
                            };
                            rowTransaction.AppendQuery(createInductionQuery);
                        }
                        else
                        {
                            var updateInductionQuery = new UpdateInductionTransactionalQuery()
                            {
                                InductionId = inductionId.Value,
                                CompletionDate = !string.IsNullOrEmpty(row.PassedDate) ? DateTime.ParseExact(row.PassedDate, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None) : null,
                                InductionStatus = dfeta_InductionStatus.PassedinWales
                            };
                            rowTransaction.AppendQuery(updateInductionQuery);
                        }

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

                        //if an induction period is not found - create one
                        //else if an induction period is found - update it
                        if (lookupData.InductionPeriod is null)
                        {
                            inductionPeriodId = Guid.NewGuid();
                            var queryInductionPeriod = new CreateInductionPeriodTransactionalQuery()
                            {
                                Id = inductionPeriodId.Value,
                                InductionId = inductionId.Value,
                                AppropriateBodyId = lookupData.OrganisationId,
                                InductionStartDate = DateTime.ParseExact(row.StartDate, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None),
                                InductionEndDate = DateTime.ParseExact(row.PassedDate, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None),
                            };
                            rowTransaction.AppendQuery(queryInductionPeriod);
                        }
                        //else
                        //{
                        //    inductionPeriodId = lookupData.InductionPeriod.dfeta_inductionperiodId;
                        //    var updateInduction = new UpdateInductionTransactionalQuery()
                        //    {
                        //        InductionId = inductionId!.Value,
                        //        CompletionDate = DateTime.ParseExact(row.PassedDate, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None),
                        //        InductionStatus = lookupData.Induction!.dfeta_InductionStatus!.Value
                        //    };
                        //    rowTransaction.AppendQuery(updateInduction);

                        //    var updateInductionPeriodQuery = new UpdateInductionPeriodTransactionalQuery()
                        //    {
                        //        InductionPeriodId = inductionPeriodId!.Value,
                        //        AppropriateBodyId = lookupData.OrganisationId,
                        //        InductionStartDate = lookupData.InductionPeriod.dfeta_StartDate,
                        //        InductionEndDate = DateTime.ParseExact(row.PassedDate, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None),
                        //    };
                        //    rowTransaction.AppendQuery(updateInductionPeriodQuery);
                        //}

                        //soft validation errors can be appended to the IntegrationTransactionRecord Failure message
                        foreach (var validationMessage in validationFailures.ValidationFailures)
                        {
                            itrFailureMessage.AppendLine(validationMessage);
                            failureMessage.AppendLine(validationMessage);
                        }

                        //increase failurecount if row is processable or if there are validation failures
                        //else increase success counter
                        if (validationFailures.ValidationFailures.Any() || validationFailures.Errors.Any())
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
                        InductionId = inductionId,
                        InductionPeriodId = inductionPeriodId,
                        DuplicateStatus = null,
                        FailureMessage = itrFailureMessage.ToString(),
                        StatusCode = string.IsNullOrEmpty(itrFailureMessage.ToString()) ? dfeta_integrationtransactionrecord_StatusCode.Success : dfeta_integrationtransactionrecord_StatusCode.Fail,
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
        var (orgMatchStatus, organisationId) = await FindMatchingOrganisationsRecordAsync(row.EmployerCode);
        InductionLookupResult? inductionMatchStatus = null;
        dfeta_induction? induction = null;
        dfeta_inductionperiod? inductionPeriod = null;
        InductionPeriodLookupResult? inductionPeriodMatchStatus = null;
        var hasActiveAlerts = false;
        if (contact != null)
        {
            var (indStatus, ind) = await FindActiveInductionByContactAsync(contact.ContactId!.Value);
            inductionMatchStatus = indStatus;
            induction = ind?.Induction;

            if (ind?.InductionPeriods?.Length > 0)
            {
                var periods = ind?.InductionPeriods.Where(x => !x.dfeta_EndDate.HasValue).ToList();
                if (periods?.Count() == 1)
                { 
                    inductionPeriodMatchStatus = InductionPeriodLookupResult.OneMatch;
                    inductionPeriod = periods.First();
                }
                else if(periods?.Count() > 1)
                {
                    inductionPeriodMatchStatus = InductionPeriodLookupResult.MultipleMatchesFound;
                    inductionPeriod = null;
                }
            }

            hasActiveAlerts = _dbContext.Alerts.Where(x => x.PersonId == contact.Id && x.IsOpen).Count() > 0;
        }

        var lookupData = new InductionImportLookupData
        {
            Person = contact,
            PersonMatchStatus = personMatchStatus,
            Induction = induction,
            InductionMatchStatus = inductionMatchStatus,
            InductionPeriod = inductionPeriod,
            InductionPeriodMatchStatus = inductionPeriodMatchStatus,
            OrganisationMatchStatus = orgMatchStatus,
            OrganisationId = organisationId,
            HasActiveAlerts = hasActiveAlerts,
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

        if (!string.IsNullOrEmpty(row.EmployerCode))
        {
            switch (lookups.OrganisationMatchStatus)
            {
                case OrganisationLookupResult.NoMatch:
                    validationFailures.Add($"Organisation with Induction Body Code {row.EmployerCode} was not found.");
                    break;
                case OrganisationLookupResult.MultipleMatchesFound:
                    validationFailures.Add($"Multiple organisations with Induction Body Code {row.EmployerCode} found.");
                    break;
            }
        }

        DateOnly.TryParseExact(row.PassedDate, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly passedDate);
        if (lookups.Person != null && lookups.Person!.dfeta_QTSDate.HasValue && passedDate < lookups.Person.dfeta_QTSDate?.ToDateOnlyWithDqtBstFix(isLocalTime: false))
        {
            errors.Add($"Induction passed date cannot be before Qts Date.");
        }

        //if teacher is exempt via set and doesn't have an induction
        if (lookups.InductionMatchStatus == InductionLookupResult.NoMatch && lookups.Person != null && lookups.Person!.dfeta_qtlsdate.HasValue)
        {
            errors.Add("may need to update either/both the 'TRA induction status' and 'Overall induction status");
        }

        //if teacher is exempt via set and inductionstatus is not in permitted updatabe statuses
        if (lookups.InductionMatchStatus == InductionLookupResult.OneMatch && lookups.Person != null && lookups.Person!.dfeta_qtlsdate.HasValue)
        {
            errors.Add("may need to update either/both the 'TRA induction status' and 'Overall induction status'");
        }

        if (lookups.Induction != null)
        {
            switch (lookups.Induction.GetAttributeValue<OptionSetValue>(dfeta_induction.Fields.dfeta_InductionStatus).Value)
            {
                case (int)dfeta_InductionStatus.Pass:
                case (int)dfeta_InductionStatus.PassedinWales:
                case (int)dfeta_InductionStatus.Exempt:
                case (int)dfeta_InductionStatus.Fail:
                case (int)dfeta_InductionStatus.FailedinWales:
                case (int)dfeta_InductionStatus.InProgress:
                    errors.Add($"Teacher with TRN {row.ReferenceNumber} completed induction already or is progress.");
                    break;
            }
        }

        return (validationFailures, errors);
    }
    public async Task<(OrganisationLookupResult, Guid? OrganisationId)> FindMatchingOrganisationsRecordAsync(string OrgNumber)
    {
        var query = new FindActiveOrganisationsByLaSchoolCodeQuery(OrgNumber);
        var results = await _crmQueryDispatcher.ExecuteQueryAsync(query);

        if (results.Length == 0)
        {
            return (OrganisationLookupResult.NoMatch, null);
        }

        if (results.Length > 1)
        {
            return (OrganisationLookupResult.MultipleMatchesFound, null);
        }

        var organisationId = results.First().Id;
        return (OrganisationLookupResult.OneMatch, organisationId);
    }

    public async Task<(InductionLookupResult, InductionRecord?)> FindActiveInductionByContactAsync(Guid personId)
    {
        var query = new GetActiveInductionByContactIdQuery(personId);
        var result = await _crmQueryDispatcher.ExecuteQueryAsync(query);

        if (result is null)
        {
            return (InductionLookupResult.NoMatch, null);
        }

        return (InductionLookupResult.OneMatch, result);
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
        public required dfeta_induction? Induction { get; set; }
        public required InductionLookupResult? InductionMatchStatus { get; set; }
        public required dfeta_inductionperiod? InductionPeriod { get; set; }
        public required InductionPeriodLookupResult? InductionPeriodMatchStatus { get; set; }
        public required Guid? OrganisationId { get; set; }
        public required OrganisationLookupResult? OrganisationMatchStatus { get; set; }
        public required bool HasActiveAlerts { get; set; }
    }
}

public enum InductionLookupResult
{
    NoMatch,
    OneMatch
}

public enum ContactLookupResult
{
    NoMatch,
    TrnAndDateOfBirthMatchFailed,
    NoAssociatedQts,
    TeacherHasQts
}

public enum InductionPeriodLookupResult
{
    NoMatch,
    OneMatch,
    MultipleMatchesFound
}

public enum OrganisationLookupResult
{
    NoMatch,
    OneMatch,
    MultipleMatchesFound
}
