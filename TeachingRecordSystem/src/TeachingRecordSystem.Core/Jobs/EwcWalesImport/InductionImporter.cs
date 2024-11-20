using System.Globalization;
using System.Text;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Jobs.EwcWalesImport;

public class InductionImporter(ICrmQueryDispatcher crmQueryDispatcher, ILogger<InductionImporter> logger)
{
    public async Task<(int TotalCount, int SuccessCount, int DuplicateCount, int FailureCount, string FailureMessage)> ImportAsync(StreamReader csvReaderStream, Guid IntegrationTransactionId, string fileName)
    {
        using (var csv = new CsvReader(csvReaderStream, CultureInfo.InvariantCulture))
        {
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
                using var rowTransaction = crmQueryDispatcher.CreateTransactionRequestBuilder();

                try
                {
                    var createIntegrationTransactionRecord = new CreateIntegrationTransactionRecordQuery()
                    {
                        Id = itrId,
                        IntegrationTransactionId = IntegrationTransactionId,
                        Reference = totalRowCount.ToString(),
                        PersonId = null,
                        InitialTeacherTrainingId = null,
                        QualificationId = null,
                        InductionId = null,
                        InductionPeriodId = null,
                        DuplicateStatus = null,
                        FileName = fileName
                    };
                    rowTransaction.AppendQuery(createIntegrationTransactionRecord);

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
                            var createInductionQuery = new CreateInductionQuery()
                            {
                                Id = inductionId.Value,
                                PersonId = personId,
                                StartDate = DateTime.ParseExact(row.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None),
                                CompletionDate = DateTime.ParseExact(row.PassedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None),
                                InductionStatus = dfeta_InductionStatus.PassedinWales
                            };
                            rowTransaction.AppendQuery(createInductionQuery);
                        }
                        else
                        {
                            var updateInductionQuery = new UpdateInductionQuery()
                            {
                                InductionId = inductionId.Value,
                                CompletionDate = !string.IsNullOrEmpty(row.PassedDate) ? DateTime.ParseExact(row.PassedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None) : null,
                                InductionStatus = dfeta_InductionStatus.PassedinWales
                            };
                            rowTransaction.AppendQuery(updateInductionQuery);
                        }

                        //if an induction period is not found - create one
                        //else if an induction period is found - update it
                        if (lookupData.InductionPeriod is null)
                        {
                            inductionPeriodId = Guid.NewGuid();
                            var queryInductionPeriod = new CreateInductionPeriodQuery()
                            {
                                Id = inductionPeriodId.Value,
                                InductionId = inductionId.Value,
                                AppropriateBodyId = lookupData.OrganisationId,
                                InductionStartDate = DateTime.ParseExact(row.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None),
                                InductionEndDate = DateTime.ParseExact(row.PassedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None),
                            };
                            rowTransaction.AppendQuery(queryInductionPeriod);
                        }
                        else
                        {
                            var updateInductionPeriodQuery = new UpdateInductionPeriodQuery()
                            {
                                Id = lookupData.InductionPeriod.Id,
                                AppropriateBodyId = lookupData.OrganisationId,
                                InductionStartDate = DateTime.ParseExact(row.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None),
                                InductionEndDate = DateTime.ParseExact(row.PassedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None),
                            };
                            rowTransaction.AppendQuery(updateInductionPeriodQuery);
                        }

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

                    //update ITR row with status of import row
                    var updateIntegrationTransactionRecordQuery = new UpdateIntegrationTransactionRecordQuery()
                    {
                        IntegrationTransactionRecordId = itrId,
                        IntegrationTransactionId = IntegrationTransactionId,
                        Reference = totalRowCount.ToString(),
                        PersonId = personId,
                        InitialTeacherTrainingId = null,
                        QualificationId = null,
                        InductionId = inductionId,
                        InductionPeriodId = inductionPeriodId,
                        DuplicateStatus = null,
                        FailureMessage = itrFailureMessage.ToString(),
                        StatusCode = string.IsNullOrEmpty(itrFailureMessage.ToString()) ? dfeta_integrationtransactionrecord_StatusCode.Success : dfeta_integrationtransactionrecord_StatusCode.Fail,
                        RowData = ConvertToCSVString(row),
                    };
                    rowTransaction.AppendQuery(updateIntegrationTransactionRecordQuery);

                    //update IntegrationTransaction so that it's always up to date with
                    //counts of rows
                    var updateIntegrationTransactionQuery = new UpdateIntegrationTransactionQuery()
                    {
                        IntegrationTransactionId = IntegrationTransactionId,
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
                    logger.LogError(e.ToString());
                }

            }
            return (totalRowCount, successCount, duplicateRowCount, failureRowCount, failureMessage.ToString());
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
        var (orgMatchStatus, organisationId) = await FindMatchingOrganisationsRecordAsync(row.EmployerName);
        EwcWalesMatchStatus? inductionMatchStatus = null;
        dfeta_induction? induction = null;
        dfeta_inductionperiod? inductionPeriod = null;
        EwcWalesMatchStatus? inductionPeriodMatchStatus = null;
        if (contact != null)
        {
            var (indStatus, ind) = await FindActiveInductionByPersonAsync(contact.ContactId!.Value);
            inductionMatchStatus = indStatus;
            induction = ind?.Induction;

            if (ind!.InductionPeriods?.Length == 1)
            {
                inductionPeriodMatchStatus = EwcWalesMatchStatus.OneMatch;
                inductionPeriod = ind.InductionPeriods.First();
            }
            else if (ind.InductionPeriods?.Length > 1)
            {
                inductionPeriodMatchStatus = EwcWalesMatchStatus.MultipleMatchesFound;
                inductionPeriod = null;
            }
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
            OrganisationId = organisationId
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
            if (!DateOnly.TryParseExact(row.DateOfBirth, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
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
            if (!DateOnly.TryParseExact(row.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
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
            if (!DateOnly.TryParseExact(row.PassedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                errors.Add("Validation Failed: Invalid Induction passed date");
            }
        }

        switch (lookups.PersonMatchStatus)
        {
            case EwcWalesMatchStatus.NoAssociatedQts:
                break;
            case EwcWalesMatchStatus.NoMatch:
                errors.Add($"Teacher with TRN {row.ReferenceNumber} was not found.");
                break;
            case EwcWalesMatchStatus.TrnAndDateOfBirthMatchFailed:
                errors.Add($"For TRN {row.ReferenceNumber} Date of Birth does not match with the existing record."); //No Test
                break;
            case EwcWalesMatchStatus.MultipleTrnMatched:
                errors.Add($"TRN {row.ReferenceNumber} was matched to more than one record in the system.");
                break;
            case EwcWalesMatchStatus.TeacherInactive:
                errors.Add($"Teacher with TRN {row.ReferenceNumber} is inactive."); //No Test
                break;
            case EwcWalesMatchStatus.TeacherHasQts:
                errors.Add($"Teacher with TRN {row.ReferenceNumber} has QTS already.");
                break;
        }

        if (!string.IsNullOrEmpty(row.EmployerCode))
        {
            switch (lookups.OrganisationMatchStatus)
            {
                case EwcWalesMatchStatus.NoMatch:
                    validationFailures.Add($"Organisation with Induction Body Code {row.EmployerCode} was not found.");
                    break;
                case EwcWalesMatchStatus.MultipleMatchesFound:
                    validationFailures.Add($"Multiple organisations with Induction Body Code {row.EmployerCode} found.");
                    break;
            }
        }

        //if teacher is exempt via set and doesn't have an induction
        if (lookups.InductionMatchStatus == EwcWalesMatchStatus.NoMatch && lookups.Person != null && lookups.Person!.dfeta_qtlsdate.HasValue)
        {
            errors.Add("may need to update either/both the 'TRA induction status' and 'Overall induction status");
        }

        //if teacher is exempt via set and inductionstatus is not in permitted updatabe statuses
        if (lookups.InductionMatchStatus == EwcWalesMatchStatus.OneMatch && lookups.Person != null && lookups.Person!.dfeta_qtlsdate.HasValue)
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
                    errors.Add($"Teacher with TRN {row.ReferenceNumber} completed induction already.");
                    break;
            }
        }

        return (validationFailures, errors);
    }
    public async Task<(EwcWalesMatchStatus, Guid? OrganisationId)> FindMatchingOrganisationsRecordAsync(string OrgNumber)
    {
        var query = new FindActiveOrganisationsByOrgNumberQuery()
        {
            OrganisationNumber = OrgNumber
        };
        var results = await crmQueryDispatcher.ExecuteQueryAsync(query);

        if (results.Length == 0)
        {
            return (EwcWalesMatchStatus.NoMatch, null);
        }

        if (results.Length > 1)
        {
            return (EwcWalesMatchStatus.MultipleMatchesFound, null);
        }

        var organisationId = results.First().Id;
        return (EwcWalesMatchStatus.OneMatch, organisationId);
    }

    public async Task<(EwcWalesMatchStatus, InductionRecord?)> FindActiveInductionByPersonAsync(Guid personId)
    {
        var query = new GetActiveInductionByContactIdQuery(personId);
        var result = await crmQueryDispatcher.ExecuteQueryAsync(query);

        if (result is null)
        {
            return (EwcWalesMatchStatus.NoMatch, null);
        }

        return (EwcWalesMatchStatus.OneMatch, result);
    }

    public async Task<(EwcWalesMatchStatus, Contact? contact)> FindMatchingTeacherRecordAsync(EwcWalesInductionImportData item)
    {
        var contacts = await crmQueryDispatcher.ExecuteQueryAsync(
                    new GetActiveContactsByTrnsQuery(
                        new[] { item.ReferenceNumber },
                        new ColumnSet(
                            Contact.Fields.dfeta_TRN,
                            Contact.Fields.BirthDate,
                            Contact.Fields.dfeta_QTSDate,
                            Contact.Fields.dfeta_qtlsdate)));

        if (contacts.Count == 0 || contacts.First().Value == null)
        {
            return (EwcWalesMatchStatus.NoMatch, null);
        }

        if (contacts.Count > 1)
        {
            return (EwcWalesMatchStatus.MultipleTrnMatched, null);
        }

        var contact = contacts.First().Value!;
        if (DateOnly.TryParseExact(item.DateOfBirth, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly dob))
        {
            if (contact.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false) != dob)
            {
                return (EwcWalesMatchStatus.TrnAndDateOfBirthMatchFailed, null);
            }
        }

        var qtsRegistrations = await crmQueryDispatcher.ExecuteQueryAsync(
            new GetActiveQtsRegistrationsByContactIdsQuery(
                new[] { contact.ContactId!.Value },
                new ColumnSet(
                    dfeta_qtsregistration.Fields.dfeta_PersonId,
                    dfeta_qtsregistration.Fields.dfeta_QTSDate,
                    dfeta_qtsregistration.Fields.dfeta_TeacherStatusId)
                )
            );

        if (qtsRegistrations[contact.Id].Length > 0)
        {
            return (EwcWalesMatchStatus.TeacherHasQts, contact);
        }
        else
        {
            return (EwcWalesMatchStatus.NoAssociatedQts, contact);
        }
    }

    public class InductionImportLookupData
    {
        public required Contact? Person { get; set; }
        public required EwcWalesMatchStatus? PersonMatchStatus { get; set; }
        public required dfeta_induction? Induction { get; set; }
        public required EwcWalesMatchStatus? InductionMatchStatus { get; set; }
        public required dfeta_inductionperiod? InductionPeriod { get; set; }
        public required EwcWalesMatchStatus? InductionPeriodMatchStatus { get; set; }
        public required Guid? OrganisationId { get; set; }
        public required EwcWalesMatchStatus? OrganisationMatchStatus { get; set; }
    }
}
