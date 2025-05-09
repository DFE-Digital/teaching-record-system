using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

namespace TeachingRecordSystem.Core.Jobs.EwcWalesImport;

public class QtsImporter
{
    public const string DATE_FORMAT = "dd/MM/yyyy";
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;
    private readonly ILogger<InductionImporter> _logger;
    private readonly TrsDbContext _dbContext;
    private readonly ReferenceDataCache _cache;
    public DateOnly _ecDirectiveQualifiedTeacherRegsChangeDate => new DateOnly(2023, 02, 01);

    public QtsImporter(ICrmQueryDispatcher crmQueryDispatcher, ILogger<InductionImporter> logger, TrsDbContext dbContext, ReferenceDataCache cache)
    {
        _crmQueryDispatcher = crmQueryDispatcher;
        _dbContext = dbContext;
        _logger = logger;
        _cache = cache;
    }

    public async Task<QtsImportResult> ImportAsync(StreamReader csvReaderStream, string fileName)
    {
        // There are two file types for qts files, both with different headings.
        // We don't validate the headings,but reference them by index.
        //
        //file1: REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,CODE,QTS_DATE
        //file2: QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,
        //       ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,
        //       ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,
        //       ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max
        //       Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,
        //       PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,
        //       PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            TrimOptions = TrimOptions.Trim,
            HasHeaderRecord = true,
            MissingFieldFound = null
        };
        using var csv = new CsvReader(csvReaderStream, csvConfig);
        csv.Context.RegisterClassMap<EwcWalesQtsFileImportDataMapper>();
        var records = csv.GetRecords<EwcWalesQtsFileImportData>().ToList();
        var totalRowCount = 0;
        var successCount = 0;
        var duplicateRowCount = 0;
        var failureRowCount = 0;
        var failureMessage = new StringBuilder();

        var integrationJob = new CreateIntegrationTransactionQuery()
        {
            StartDate = DateTime.Now,
            TypeId = (int)dfeta_IntegrationInterface.GTCWalesImport,
            FileName = fileName
        };
        var integrationId = await _crmQueryDispatcher.ExecuteQueryAsync(integrationJob);

        foreach (var row in records)
        {
            totalRowCount++;
            var itrFailureMessage = new StringBuilder();
            using var rowTransaction = _crmQueryDispatcher.CreateTransactionRequestBuilder();

            try
            {
                var lookupData = await GetLookupDataAsync(row);
                var validationFailures = Validate(row, lookupData);

                //append non processable errors to list of failures that will be a line in
                //the IntegrationTransaction (job) failuremessage field.
                if (validationFailures.Errors.Any())
                {
                    foreach (var error in validationFailures.Errors)
                    {
                        failureMessage.AppendLine(error);
                        itrFailureMessage.AppendLine(error);
                    }
                }
                else
                {
                    DateOnly? awardedDate = null;
                    if (DateOnly.TryParseExact(row.QtsDate, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedQtsDate))
                    {
                        awardedDate = parsedQtsDate;
                    }

                    List<Guid?> Subjects = new List<Guid?>() { lookupData.IttSubject1Id, lookupData.IttSubject2Id };
                    rowTransaction.AppendQuery(new CreateDqtOutboxMessageTransactionalQuery(new AddWelshRMessage
                    {
                        PersonId = lookupData.PersonId!.Value,
                        AwardedDate = awardedDate,
                        TrainingCountryId = null,
                        Subjects = Subjects.Where(x => x.HasValue).Select(y => y!.Value).ToList(),
                        TrainingProviderId = lookupData.IttEstabCodeId,
                        TrainingAgeSpecialismRangeFrom = null,
                        TrainingAgeSpecialismRangeTo = null,
                        TrainingEndDate = null,
                        TrainingStartDate = null,
                        TrainingAgeSpecialismType = null
                    }));


                    //soft validation errors can be appended to the IntegrationTransactionRecord Failure message
                    foreach (var validationMessage in validationFailures.ValidationFailures)
                    {
                        itrFailureMessage.AppendLine(validationMessage);
                        failureMessage.AppendLine(validationMessage);
                    }


                    // Set Induction status to 'Required to complete' if
                    //  Qts status is Qualified teacher: under the EC Directive (67)  with qts date before regs changed, set induction status to
                    //  Qts status is Qualified teacher: Following at least one term's service on the Graduate Teacher Programme (49)
                    //  Qts status is Qualified teacher (trained) (71)
                    if (row.QtsStatus == "67" &&
                        DateOnly.TryParseExact(row.QtsDate, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly qtsDate) &&
                        qtsDate < _ecDirectiveQualifiedTeacherRegsChangeDate ||
                        row.QtsStatus == "71" ||
                        row.QtsStatus == "49")
                    {
                        rowTransaction.AppendQuery(new CreateDqtOutboxMessageTransactionalQuery(new SetInductionRequiredToCompleteMessage()
                        {
                            PersonId = lookupData.PersonId.Value,
                            TrsUserId = DataStore.Postgres.Models.SystemUser.SystemUserId
                        }));
                    }


                    if (lookupData.HasActiveAlerts)
                    {
                        var query = new CreateTaskTransactionalQuery()
                        {
                            ContactId = lookupData.PersonId.Value,
                            Category = "GTC Wales Import",
                            Description = "QTS/Induction update with Active Sanction",
                            Subject = "Notification for QTS Unit Team",
                            ScheduledEnd = DateTime.Now
                        };
                        rowTransaction.AppendQuery(query);
                    }
                }

                //create ITR row with status of import row
                var createIntegrationTransactionRecord = new CreateIntegrationTransactionRecordTransactionalQuery()
                {
                    IntegrationTransactionId = integrationId,
                    Reference = totalRowCount.ToString(),
                    ContactId = lookupData.PersonId,
                    InitialTeacherTrainingId = null,
                    QualificationId = null,
                    InductionId = null,
                    InductionPeriodId = null,
                    DuplicateStatus = null,
                    FailureMessage = itrFailureMessage.ToString(),
                    StatusCode = validationFailures.Errors.Count == 0 ? dfeta_integrationtransactionrecord_StatusCode.Success : dfeta_integrationtransactionrecord_StatusCode.Fail,
                    RowData = ConvertToCsvString(row),
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


        return new QtsImportResult(totalRowCount, successCount, duplicateRowCount, failureRowCount, failureMessage.ToString(), integrationId);
    }

    public string ConvertToCsvString(EwcWalesQtsFileImportData row)
    {
        using (var writer = new StringWriter())
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecord(row);
            csv.NextRecord();
            return writer.ToString();
        }
    }

    public async Task<QtsImportLookupData> GetLookupDataAsync(EwcWalesQtsFileImportData row)
    {
        //contact
        var (personMatchStatus, personId) = await FindMatchingTeacherRecordAsync(row);
        var (ittEstabCodeStatus, ittEstabCodeId) = await FindMatchingOrganisationsRecordAsync(row.IttEstabCode);
        var (ittQualCodeStatus, ittQualificationId) = await GetIttQualificationAsync(row.IttQualCode);
        var (ittSubjectCode1MatchStatus, ittSubjectCode1Id) = await FindIttSubjectAsync(row.IttSubjectCode1);
        var (ittSubjectCode2MatchStatus, ittSubjectCode2Id) = await FindIttSubjectAsync(row.IttSubjectCode2);
        var (iqEstabCodeMatchStatus, iqEstaId) = await FindMatchingOrganisationsRecordAsync(row.PqEstabCode);
        var (pqCountryMatchStatus, pqCountryId) = await GetMatchingCountryAsync(row.Country);
        var (heQualificationMatchStatus, heQualificationId) = await GetHEQualificationAsync(row.PqQualCode);
        var (pqSubjectCode1MatchStatus, pqSubjectCode1Id) = await GetMatchingHESubjectAsync(row.PqSubjectCode1);
        var (pqSubjectCode2MatchStatus, pqSubjectCode2Id) = await GetMatchingHESubjectAsync(row.PqSubjectCode2);
        var (pqSubjectCode3MatchStatus, pqSubjectCode3Id) = await GetMatchingHESubjectAsync(row.PqSubjectCode3);
        var (teacherStatusMatchStatus, teacherStatusId) = await GetTeacherStatusAsync(row.QtsStatus);
        var heClassDivision = GetHEClassDivision(row.PqClassCode);
        var hasActiveAlerts = false;

        if (personId.HasValue)
        {

            hasActiveAlerts = _dbContext.Alerts.Where(x => x.PersonId == personId.Value && x.IsOpen).Count() > 0;
        }

        var lookupData = new QtsImportLookupData
        {
            PersonId = personId,
            PersonMatchStatus = personMatchStatus,
            IttEstabCodeId = ittEstabCodeId,
            IttEstabCodeMatchStatus = ittEstabCodeStatus,
            IttQualificationId = ittQualificationId,
            IttQualificationMatchStatus = ittQualCodeStatus,
            IttSubject1Id = ittSubjectCode1Id,
            IttSubject1MatchStatus = ittSubjectCode1MatchStatus,
            IttSubject2Id = ittSubjectCode2Id,
            IttSubject2MatchStatus = ittSubjectCode2MatchStatus,
            PqEstablishmentId = iqEstaId,
            PqEstablishmentMatchStatus = iqEstabCodeMatchStatus,
            PQCountryId = pqCountryId,
            PQCountryMatchStatus = pqCountryMatchStatus,
            PQHEQualificationId = heQualificationId,
            PQHEQualificationMatchStatus = heQualificationMatchStatus,
            PQSubject1Id = pqSubjectCode1Id,
            PQSubject1MatchStatus = pqSubjectCode1MatchStatus,
            PQSubject2Id = pqSubjectCode2Id,
            PQSubject2MatchStatus = pqSubjectCode2MatchStatus,
            PQSubject3Id = pqSubjectCode3Id,
            PQSubject3MatchStatus = pqSubjectCode3MatchStatus,
            TeacherStatusId = teacherStatusId,
            TeacherStatusMatchStatus = teacherStatusMatchStatus,
            ClassDivision = heClassDivision,
            HasActiveAlerts = hasActiveAlerts
        };
        return lookupData;
    }

    public (List<string> ValidationFailures, List<string> Errors) Validate(EwcWalesQtsFileImportData row, QtsImportLookupData lookups)
    {
        var validationFailures = new List<string>();
        var errors = new List<string>();

        //QTS REF
        if (string.IsNullOrEmpty(row.QtsRefNo))
        {
            errors.Add("Missing QTS Ref Number");
        }

        //Date Of birth
        if (string.IsNullOrEmpty(row.DateOfBirth))
        {
            errors.Add("Missing Date of Birth");
        }
        else if (!DateOnly.TryParseExact(row.DateOfBirth, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            errors.Add($"Validation Failed: Invalid Date of Birth {row.DateOfBirth}");
        }

        // qts status
        if (string.IsNullOrEmpty(row.QtsStatus))
        {
            errors.Add("Qts status is missing");
        }

        if (lookups.TeacherStatusMatchStatus == EwcWalesMatchStatus.NoMatch)
        {
            errors.Add("Qts Status must be 49,67, 68,69, 71 or 102 ");
        }

        //QTS Date
        if (string.IsNullOrEmpty(row.QtsDate))
        {
            errors.Add("Misssing QTS Date");
        }
        else if (!DateOnly.TryParseExact(row.QtsDate, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            errors.Add($"Validation Failed: Invalid QTS Date {row.QtsDate}");
        }
        else if (!string.IsNullOrEmpty(row.QtsStatus) &&
            DateOnly.TryParseExact(row.QtsDate, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly qtsDate)
        )
        {
            if (qtsDate >= _ecDirectiveQualifiedTeacherRegsChangeDate)
            {
                var validStatuses = new List<string>() { "49", "71" };
                if (!validStatuses.Any(val => string.Equals(val, row.QtsStatus, StringComparison.InvariantCultureIgnoreCase)))
                {
                    errors.Add($"Qts Status can only be 71 or 49 when qts date is on or past {_ecDirectiveQualifiedTeacherRegsChangeDate.ToString(DATE_FORMAT)}");
                }
            }
            else
            {
                var validStatuses = new List<string>() { "67", "68", "69", "71", "102", "49", "71" };
                if (!validStatuses.Any(val => string.Equals(val, row.QtsStatus, StringComparison.InvariantCultureIgnoreCase)))
                {
                    errors.Add($"Qts Status can only be 67, 68, 69, 71, 102, 49 or 71 when qts date is on or before 31/01/2023");
                }
            }

        }

        switch (lookups.PersonMatchStatus)
        {
            case EwcWalesMatchStatus.NoAssociatedQts:
                break;
            case EwcWalesMatchStatus.NoMatch:
                errors.Add($"Teacher with TRN {row.QtsRefNo} was not found.");
                break;
            case EwcWalesMatchStatus.TrnAndDateOfBirthMatchFailed:
                errors.Add($"For TRN {row.QtsRefNo} Date of Birth does not match with the existing record."); //No Test
                break;
            case EwcWalesMatchStatus.MultipleTrnMatched:
                errors.Add($"TRN {row.QtsRefNo} was matched to more than one record in the system.");
                break;
            case EwcWalesMatchStatus.TeacherInactive:
                errors.Add($"Teacher with TRN {row.QtsRefNo} is inactive."); //No Test
                break;
            case EwcWalesMatchStatus.TeacherHasQts:
                errors.Add($"Teacher with TRN {row.QtsRefNo} has QTS already.");
                break;
        }

        //IttEstabCode
        if (!string.IsNullOrEmpty(row.IttEstabCode))
        {
            if (lookups.IttEstabCodeMatchStatus == EwcWalesMatchStatus.NoMatch)
            {
                validationFailures.Add($"Organisation with ITT Establishment Code {row.IttEstabCode} was not found.");
            }
            else if (lookups.IttEstabCodeMatchStatus == EwcWalesMatchStatus.MultipleMatchesFound)
            {
                validationFailures.Add($"Multiple organisations with ITT Establishment Code {row.IttEstabCode} found.");
            }
        }

        // ITT Qualification
        if (!string.IsNullOrEmpty(row.IttQualCode))
        {
            if (lookups.IttQualificationMatchStatus == EwcWalesMatchStatus.NoMatch)
            {
                validationFailures.Add($"ITT qualification with code {row.IttQualCode} was not found.");
            }
            else if (lookups.IttQualificationMatchStatus == EwcWalesMatchStatus.MultipleMatchesFound)
            {
                validationFailures.Add($"Multiple ITT qualifications with code {row.IttQualCode} found.");
            }
        }

        // IIT Subject 1
        if (!string.IsNullOrEmpty(row.IttSubjectCode1))
        {
            if (lookups.IttSubject1MatchStatus == EwcWalesMatchStatus.NoMatch)
            {
                validationFailures.Add($"ITT subject with code {row.IttSubjectCode1} was not found.");
            }
            else if (lookups.IttSubject1MatchStatus == EwcWalesMatchStatus.MultipleMatchesFound)
            {
                validationFailures.Add($"Multiple ITT subjects with code {row.IttSubjectCode1} found.");
            }
        }

        // IIT Subject 2
        if (!string.IsNullOrEmpty(row.IttSubjectCode2))
        {
            if (lookups.IttSubject1MatchStatus == EwcWalesMatchStatus.NoMatch)
            {
                validationFailures.Add($"ITT subject with code {row.IttSubjectCode2} was not found.");
            }
            else if (lookups.IttSubject1MatchStatus == EwcWalesMatchStatus.MultipleMatchesFound)
            {
                validationFailures.Add($"Multiple ITT subjects with code {row.IttSubjectCode2} found.");
            }
        }

        // PQ Establishment
        if (!string.IsNullOrEmpty(row.PqEstabCode))
        {
            if (lookups.PqEstablishmentMatchStatus == EwcWalesMatchStatus.NoMatch)
            {
                validationFailures.Add($"Organisation with PQ Establishment Code {row.PqEstabCode} was not found.");
            }
            else if (lookups.PqEstablishmentMatchStatus == EwcWalesMatchStatus.MultipleMatchesFound)
            {
                validationFailures.Add($"Multiple organisations with PQ Establishment Code {row.PqEstabCode} found.");
            }
        }

        // PQ Country
        if (!string.IsNullOrEmpty(row.Country))
        {
            if (lookups.PQCountryMatchStatus == EwcWalesMatchStatus.NoMatch)
            {
                validationFailures.Add($"Country with PQ Country Code {row.Country} was not found.");
            }
            else if (lookups.PQCountryMatchStatus == EwcWalesMatchStatus.MultipleMatchesFound)
            {
                validationFailures.Add($"Multiple countries with PQ Country Code {row.Country} found.");
            }
        }

        // PQ HE Qualification
        if (!string.IsNullOrEmpty(row.PqQualCode))
        {
            if (lookups.PQHEQualificationMatchStatus == EwcWalesMatchStatus.NoMatch)
            {
                validationFailures.Add($"Qualification with PQ Qualification Code {row.PqQualCode} was not found.");
            }
            else if (lookups.PQHEQualificationMatchStatus == EwcWalesMatchStatus.MultipleMatchesFound)
            {
                validationFailures.Add($"Multiple qualifications with PQ Qualification Code {row.PqQualCode} found.");
            }
        }

        // PQ Subject 1
        if (!string.IsNullOrEmpty(row.PqSubjectCode1))
        {
            if (lookups.PQSubject1MatchStatus == EwcWalesMatchStatus.NoMatch)
            {
                validationFailures.Add($"Subject with PQ Subject Code {row.PqSubjectCode1} was not found.");
            }
            else if (lookups.PQSubject1MatchStatus == EwcWalesMatchStatus.MultipleMatchesFound)
            {
                validationFailures.Add($"Multiple subjects with PQ Subject Code {row.PqSubjectCode1} found.");
            }
        }

        // PQ Subject 2
        if (!string.IsNullOrEmpty(row.PqSubjectCode2))
        {
            if (lookups.PQSubject2MatchStatus == EwcWalesMatchStatus.NoMatch)
            {
                validationFailures.Add($"Subject with PQ Subject Code {row.PqSubjectCode2} was not found.");
            }
            else if (lookups.PQSubject2MatchStatus == EwcWalesMatchStatus.MultipleMatchesFound)
            {
                validationFailures.Add($"Multiple subjects with PQ Subject Code {row.PqSubjectCode2} found.");
            }
        }

        // PQ Subject 3
        if (!string.IsNullOrEmpty(row.PqSubjectCode3))
        {
            if (lookups.PQSubject3MatchStatus == EwcWalesMatchStatus.NoMatch)
            {
                validationFailures.Add($"Subject with PQ Subject Code {row.PqSubjectCode3} was not found.");
            }
            else if (lookups.PQSubject3MatchStatus == EwcWalesMatchStatus.MultipleMatchesFound)
            {
                validationFailures.Add($"Multiple subjects with PQ Subject Code {row.PqSubjectCode3} found.");
            }
        }

        return (validationFailures, errors);
    }

    public async Task<(EwcWalesMatchStatus, Guid? OrganisationId)> FindMatchingOrganisationsRecordAsync(string OrgNumber)
    {
        var query = new FindActiveOrganisationsByAccountNumberQuery(OrgNumber);
        var results = await _crmQueryDispatcher.ExecuteQueryAsync(query);

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

    public async Task<(EwcWalesMatchStatus, Guid? HeQualificationId)> GetHEQualificationAsync(string qualificationCode)
    {
        var results = await _cache.GetHeQualificationsAsync();
        var qualifications = results.Where(x => x.dfeta_Value == qualificationCode).ToArray();

        if (qualifications.Length == 0)
        {
            return (EwcWalesMatchStatus.NoMatch, null);
        }

        if (qualifications.Length > 1)
        {
            return (EwcWalesMatchStatus.MultipleMatchesFound, null);
        }

        var subjectId = qualifications.First().Id;
        return (EwcWalesMatchStatus.OneMatch, subjectId);
    }

    public async Task<(EwcWalesMatchStatus, Guid? SubjectId)> GetMatchingHESubjectAsync(string subjectCode)
    {
        var results = await _cache.GetHeSubjectsAsync();
        var subjects = results.Where(x => x.dfeta_Value == subjectCode).ToArray();

        if (subjects.Length == 0)
        {
            return (EwcWalesMatchStatus.NoMatch, null);
        }

        if (subjects.Length > 1)
        {
            return (EwcWalesMatchStatus.MultipleMatchesFound, null);
        }

        var subjectId = subjects.First().Id;
        return (EwcWalesMatchStatus.OneMatch, subjectId);
    }

    public async Task<(EwcWalesMatchStatus, Guid? SubjectId)> GetTeacherStatusAsync(string qtsStatus)
    {
        var results = await _cache.GetTeacherStatusesAsync();

        if (qtsStatus != null && (qtsStatus.Equals("67", StringComparison.InvariantCultureIgnoreCase)))
        {
            var status = results.Single(x => x.dfeta_Value == qtsStatus); //67 - Qualified Teacher: under the EC Directive
            return (EwcWalesMatchStatus.OneMatch, status.Id);
        }
        else if (qtsStatus != null && (qtsStatus.Equals("71", StringComparison.InvariantCultureIgnoreCase) ||
            qtsStatus.Equals("49", StringComparison.InvariantCultureIgnoreCase)))
        {
            var status = results.Single(x => x.dfeta_Value == "213"); //213 - Qualified Teacher: QTS awarded in Wales
            return (EwcWalesMatchStatus.OneMatch, status.Id);
        }
        else if (qtsStatus != null && (qtsStatus.Equals("68", StringComparison.InvariantCultureIgnoreCase)))
        {
            var status = results.Single(x => x.dfeta_Value == qtsStatus); //68 - Qualified Teacher: Teachers trained/registered in Scotland
            return (EwcWalesMatchStatus.OneMatch, status.Id);
        }
        else if (qtsStatus != null && (qtsStatus.Equals("69", StringComparison.InvariantCultureIgnoreCase)))
        {
            var status = results.Single(x => x.dfeta_Value == qtsStatus); //69 - Qualified Teacher: Teachers trained/recognised by the Department of Education for Northern Ireland (DENI)
            return (EwcWalesMatchStatus.OneMatch, status.Id);
        }
        //else if (qtsStatus != null && (qtsStatus.Equals("102", StringComparison.InvariantCultureIgnoreCase)))
        //{
        //    var status = results.Single(x => x.dfeta_Value == qtsStatus); //102 (NOT IN CRM)
        //    return (EwcWalesMatchStatus.OneMatch, status.Id);
        //}

        else
        {
            return (EwcWalesMatchStatus.NoMatch, null);
        }
    }

    public dfeta_classdivision? GetHEClassDivision(string classCode)
    {
        switch (classCode)
        {
            case "1":
            case "01":
                return dfeta_classdivision.Firstclasshonours;
            case "2":
            case "02":
                return dfeta_classdivision.Uppersecondclasshonours;
            case "3":
            case "03":
                return dfeta_classdivision.Lowersecondclasshonours;
            case "4":
            case "04":
                return dfeta_classdivision.Undividedsecondclasshonours;
            case "5":
            case "05":
                return dfeta_classdivision.Thirdclasshonours;
            case "6":
            case "06":
                return dfeta_classdivision.Fourthclasshonours;
            case "7":
            case "07":
                return dfeta_classdivision.Unclassifiedhonours;
            case "8":
            case "08":
                return dfeta_classdivision.Aegrotat_whethertohonoursorpass;
            case "9":
            case "09":
                return dfeta_classdivision.Passdegreeawardedwithouthonoursfollowinganhonourscourse;
            case "10":
                return dfeta_classdivision.Ordinary_includingdivisionsofordinaryifanydegreeawardedafterfollowinganonhonourscourse;
            case "11":
                return dfeta_classdivision.GeneralDegreedegreeawardedafterfollowinganonhonourscoursedegreethatwasnotavailabletobeclassified;
            case "12":
                return dfeta_classdivision.Distinction;
            case "13":
                return dfeta_classdivision.Merit;
            case "14":
                return dfeta_classdivision.Pass;
            case "98":
                return dfeta_classdivision.Notapplicable;
            case "99":
                return dfeta_classdivision.NotKnown;
            default:
                return null;
        }
    }

    public async Task<(EwcWalesMatchStatus, Guid? IttQualificationId)> GetIttQualificationAsync(string ittQualificationcode)
    {
        var results = await _cache.GetIttQualificationsAsync();
        var qualifications = results.Where(x => x.dfeta_Value == ittQualificationcode).ToArray();

        if (qualifications.Length == 0)
        {
            return (EwcWalesMatchStatus.NoMatch, null);
        }

        if (qualifications.Length > 1)
        {
            return (EwcWalesMatchStatus.MultipleMatchesFound, null);
        }

        var qualificationId = qualifications.First().Id;
        return (EwcWalesMatchStatus.OneMatch, qualificationId);
    }

    public async Task<(EwcWalesMatchStatus, Guid? SubjectId)> FindIttSubjectAsync(string subjectCode)
    {
        var subject = await _cache.GetIttSubjectBySubjectCodeAsync(subjectCode);

        if (subject is null)
        {
            return (EwcWalesMatchStatus.NoMatch, null);
        }

        return (EwcWalesMatchStatus.OneMatch, subject.Id);
    }

    public async Task<(EwcWalesMatchStatus, Guid? SubjectId)> GetMatchingCountryAsync(string countryCode)
    {
        var country = await _cache.GetCountryByCountryCodeAsync(countryCode);
        if (country is null)
        {
            return (EwcWalesMatchStatus.NoMatch, null);
        }

        return (EwcWalesMatchStatus.OneMatch, country.Id);
    }

    public async Task<(EwcWalesMatchStatus, Guid? PersonId)> FindMatchingTeacherRecordAsync(EwcWalesQtsFileImportData item)
    {
        var contact = await _crmQueryDispatcher.ExecuteQueryAsync(
            new GetActiveContactByTrnQuery(item.QtsRefNo,
                new ColumnSet(
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.BirthDate,
                    Contact.Fields.dfeta_QTSDate,
                    Contact.Fields.dfeta_qtlsdate)));

        if (contact == null)
        {
            return (EwcWalesMatchStatus.NoMatch, null);
        }

        if ((DateOnly.TryParseExact(item.DateOfBirth, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly dob)) &&
            contact.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false) != dob)
        {
            return (EwcWalesMatchStatus.TrnAndDateOfBirthMatchFailed, null);
        }

        var professionalStatsues = _dbContext.ProfessionalStatuses.Where(x => x.PersonId == contact.Id &&
            x.QualificationType == QualificationType.ProfessionalStatus &&
            x.RouteToProfessionalStatus != null &&
            x.RouteToProfessionalStatus.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus &&
            x.AwardedDate != null).ToArray();

        if (professionalStatsues.Length > 0)
        {
            return (EwcWalesMatchStatus.TeacherHasQts, contact.ContactId!);
        }
        else
        {
            return (EwcWalesMatchStatus.NoAssociatedQts, contact.ContactId!);
        }
    }

    public class EwcWalesQtsFileImportDataMapper : ClassMap<EwcWalesQtsFileImportData>
    {
        public EwcWalesQtsFileImportDataMapper()
        {
            Map(m => m.QtsRefNo).Index(0).Optional();
            Map(m => m.Forename).Index(1).Optional();
            Map(m => m.Surname).Index(2).Optional();
            Map(m => m.DateOfBirth).Index(3).Optional();
            Map(m => m.QtsStatus).Index(4).Optional();
            Map(m => m.QtsDate).Index(5).Optional();
            Map(m => m.IttStartMonth).Index(6).Optional();
            Map(m => m.IttStartYear).Index(7).Optional();
            Map(m => m.IttEndDate).Index(8).Optional();
            Map(m => m.ITTCourseLength).Index(9).Optional();
            Map(m => m.IttEstabLeaCode).Index(10).Optional();
            Map(m => m.IttEstabCode).Index(11).Optional();
            Map(m => m.IttQualCode).Index(12).Optional();
            Map(m => m.IttClassCode).Index(13).Optional();
            Map(m => m.IttSubjectCode1).Index(14).Optional();
            Map(m => m.IttSubjectCode2).Index(15).Optional();
            Map(m => m.IttMinAgeRange).Index(16).Optional();
            Map(m => m.IttMaxAgeRange).Index(17).Optional();
            Map(m => m.IttMinSpAgeRange).Index(18).Optional();
            Map(m => m.IttMaxSpAgeRange).Index(19).Optional();
            Map(m => m.PqCourseLength).Index(20).Optional();
            Map(m => m.PqYearOfAward).Index(21).Optional();
            Map(m => m.Country).Index(22).Optional();
            Map(m => m.PqEstabCode).Index(23).Optional();
            Map(m => m.PqQualCode).Index(24).Optional();
            Map(m => m.Honours).Index(25).Optional();
            Map(m => m.PqClassCode).Index(26).Optional();
            Map(m => m.PqSubjectCode1).Index(27).Optional();
            Map(m => m.PqSubjectCode2).Index(28).Optional();
            Map(m => m.PqSubjectCode3).Index(29).Optional();
        }
    }

    public class QtsImportLookupData
    {
        public required Guid? PersonId { get; set; }
        public required EwcWalesMatchStatus? PersonMatchStatus { get; set; }
        public required Guid? IttEstabCodeId { get; set; }
        public required EwcWalesMatchStatus? IttEstabCodeMatchStatus { get; set; }
        public required Guid? IttQualificationId { get; set; }
        public required EwcWalesMatchStatus? IttQualificationMatchStatus { get; set; }
        public required Guid? IttSubject1Id { get; set; }
        public required EwcWalesMatchStatus? IttSubject1MatchStatus { get; set; }
        public required Guid? IttSubject2Id { get; set; }
        public required EwcWalesMatchStatus? IttSubject2MatchStatus { get; set; }
        public required Guid? PqEstablishmentId { get; set; }
        public required EwcWalesMatchStatus? PqEstablishmentMatchStatus { get; set; }
        public required Guid? PQCountryId { get; set; }
        public required EwcWalesMatchStatus? PQCountryMatchStatus { get; set; }
        public required Guid? PQHEQualificationId { get; set; }
        public required EwcWalesMatchStatus? PQHEQualificationMatchStatus { get; set; }
        public required Guid? PQSubject1Id { get; set; }
        public required EwcWalesMatchStatus? PQSubject1MatchStatus { get; set; }
        public required Guid? PQSubject2Id { get; set; }
        public required EwcWalesMatchStatus? PQSubject2MatchStatus { get; set; }
        public required Guid? PQSubject3Id { get; set; }
        public required EwcWalesMatchStatus? PQSubject3MatchStatus { get; set; }
        public required Guid? TeacherStatusId { get; set; }
        public required EwcWalesMatchStatus? TeacherStatusMatchStatus { get; set; }
        public required dfeta_classdivision? ClassDivision { get; set; }
        public required bool HasActiveAlerts { get; set; }
    }
}
