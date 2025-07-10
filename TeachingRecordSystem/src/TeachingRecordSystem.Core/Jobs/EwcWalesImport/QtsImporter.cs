using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Core.Jobs.EwcWalesImport;

public class QtsImporter
{
    public const string DATE_FORMAT = "dd/MM/yyyy";
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;
    private readonly ILogger<InductionImporter> _logger;
    private readonly TrsDbContext _dbContext;
    private readonly ReferenceDataCache _cache;
    public DateOnly _ecDirectiveQualifiedTeacherRegsChangeDate => new DateOnly(2023, 02, 01);
    private readonly IClock _clock;

    public QtsImporter(ICrmQueryDispatcher crmQueryDispatcher, ILogger<InductionImporter> logger, TrsDbContext dbContext, ReferenceDataCache cache, IClock clock)
    {
        _crmQueryDispatcher = crmQueryDispatcher;
        _dbContext = dbContext;
        _logger = logger;
        _cache = cache;
        _clock = clock;
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

        await using var txn = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        var integrationJob = new IntegrationTransaction()
        {
            IntegrationTransactionId = 0,
            InterfaceType = IntegrationTransactionInterfaceType.EwcWales,
            ImportStatus = IntegrationTransactionImportStatus.InProgress,
            TotalCount = 0,
            SuccessCount = 0,
            FailureCount = 0,
            DuplicateCount = 0,
            FileName = fileName,
            CreatedDate = _clock.UtcNow,
            IntegrationTransactionRecords = new List<IntegrationTransactionRecord>()
        };
        _dbContext.IntegrationTransactions.Add(integrationJob);
        await _dbContext.SaveChangesAsync();
        var integrationId = integrationJob.IntegrationTransactionId;

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

                    var routeToProfessionalStatusTypeId = RouteToProfessionalStatusType.WelshRId;
                    if (awardedDate < _ecDirectiveQualifiedTeacherRegsChangeDate && row.QtsStatus.Equals("67", StringComparison.InvariantCultureIgnoreCase))
                    {
                        routeToProfessionalStatusTypeId = RouteToProfessionalStatusType.ECDirective;
                    }

                    //create route for teacher
                    if (lookupData.Person != null)
                    {
                        var allRoutes = await _cache.GetRouteToProfessionalStatusTypesAsync();
                        var route = RouteToProfessionalStatus.Create(
                           person: lookupData.Person,
                           allRouteTypes: allRoutes,
                           routeToProfessionalStatusTypeId: routeToProfessionalStatusTypeId,
                           sourceApplicationUserId: null,
                           sourceApplicationReference: null,
                           status: RouteToProfessionalStatusStatus.Holds,
                           holdsFrom: awardedDate,
                           trainingStartDate: null,
                           trainingEndDate: null,
                           trainingSubjectIds: null,
                           trainingAgeSpecialismType: null,
                           trainingAgeSpecialismRangeFrom: null,
                           trainingAgeSpecialismRangeTo: null,
                           trainingCountryId: null,
                           trainingProviderId: null,
                           degreeTypeId: null,
                           isExemptFromInduction: null,
                           createdBy: DataStore.Postgres.Models.SystemUser.SystemUserId,
                           now: _clock.UtcNow,
                           changeReason: null,
                           changeReasonDetail: null,
                           evidenceFile: null,
                           out var routeevent);

                        await _dbContext.AddEventAndBroadcastAsync(routeevent);

                        _dbContext.RouteToProfessionalStatuses.Add(route);
                    }

                    //soft validation errors can be appended to the IntegrationTransactionRecord Failure message
                    foreach (var validationMessage in validationFailures.ValidationFailures)
                    {
                        itrFailureMessage.AppendLine(validationMessage);
                        failureMessage.AppendLine(validationMessage);
                    }
                }

                //create ITR row with status of import row
                integrationJob.IntegrationTransactionRecords.Add(new IntegrationTransactionRecord()
                {
                    IntegrationTransactionRecordId = 0,
                    CreatedDate = _clock.UtcNow,
                    RowData = ConvertToCsvString(row),
                    Status = validationFailures.Errors.Count == 0 ? IntegrationTransactionRecordStatus.Success : IntegrationTransactionRecordStatus.Failure,
                    PersonId = lookupData.Person != null ? lookupData.Person.PersonId : null,
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

        //update integration transaction counts as job has finished
        integrationJob.TotalCount = totalRowCount;
        integrationJob.SuccessCount = successCount;
        integrationJob.FailureCount = failureRowCount;
        integrationJob.DuplicateCount = duplicateRowCount;
        integrationJob.ImportStatus = IntegrationTransactionImportStatus.Success;
        await _dbContext.SaveChangesAsync();
        await txn.CommitAsync();

        return new QtsImportResult(totalRowCount, successCount, duplicateRowCount, failureRowCount, failureMessage.ToString(), integrationId);
    }

    public string ConvertToCsvString(EwcWalesQtsFileImportData row)
    {
        using (var writer = new StringWriter())
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteHeader<EwcWalesQtsFileImportData>();
            csv.NextRecord();
            csv.WriteRecord(row);
            csv.NextRecord();
            return writer.ToString();
        }
    }

    public async Task<QtsImportLookupData> GetLookupDataAsync(EwcWalesQtsFileImportData row)
    {
        //contact
        var (personMatchStatus, person) = await FindMatchingTeacherRecordAsync(row);
        var (teacherStatusMatchStatus, teacherStatusId) = await GetTeacherStatusAsync(row.QtsStatus);
        var hasActiveAlerts = false;

        if (person != null)
        {

            hasActiveAlerts = _dbContext.Alerts.Where(x => x.PersonId == person.PersonId && x.IsOpen).Count() > 0;
        }

        var lookupData = new QtsImportLookupData
        {
            Person = person,
            PersonMatchStatus = personMatchStatus,
            TeacherStatusId = teacherStatusId,
            TeacherStatusMatchStatus = teacherStatusMatchStatus,
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
            if (qtsDate > _clock.Today)
            {
                errors.Add("Qts date cannot be set in the future");
            }

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

            if (lookups.Person != null)
            {
                var welshrRoutes = lookups.Person!.Qualifications?
                    .OfType<RouteToProfessionalStatus>()
                    .Where(p => p.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.WelshRId && p.Status == RouteToProfessionalStatusStatus.Holds)
                    .ToArray();

                if (welshrRoutes is not null && welshrRoutes.Any(x => x.HoldsFrom == qtsDate))
                {

                    errors.Add($"{lookups.Person.Trn} already holds welshr route with holdsfrom {qtsDate}");
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
        }
        return (validationFailures, errors);
    }

    public async Task<(EwcWalesMatchStatus, Guid? OrganisationId)> FindMatchingOrganisationsRecordAsync(string OrgNumber)
    {
        var results = await _dbContext.TrainingProviders.Where(x => x.Ukprn == OrgNumber).ToArrayAsync();

        if (results.Length == 0)
        {
            return (EwcWalesMatchStatus.NoMatch, null);
        }

        if (results.Length > 1)
        {
            return (EwcWalesMatchStatus.MultipleMatchesFound, null);
        }

        var organisationId = results.First().TrainingProviderId;
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

    public async Task<(EwcWalesMatchStatus, Person? person)> FindMatchingTeacherRecordAsync(EwcWalesQtsFileImportData item)
    {
        var person = await _dbContext.Persons.Include(x => x.Qualifications).FirstOrDefaultAsync(x => x.Trn == item.QtsRefNo && x.Status == PersonStatus.Active);

        if (person == null)
        {
            return (EwcWalesMatchStatus.NoMatch, null);
        }

        if ((DateOnly.TryParseExact(item.DateOfBirth, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly dob)) &&
            person.DateOfBirth != dob)
        {
            return (EwcWalesMatchStatus.TrnAndDateOfBirthMatchFailed, null);
        }

        var personQtsDate = person.QtsDate;
        if (personQtsDate.HasValue)
        {
            return (EwcWalesMatchStatus.TeacherHasQts, person!);
        }
        else
        {
            return (EwcWalesMatchStatus.NoAssociatedQts, person!);
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
        public required Person? Person { get; set; }
        public required EwcWalesMatchStatus? PersonMatchStatus { get; set; }
        public required Guid? TeacherStatusId { get; set; }
        public required EwcWalesMatchStatus? TeacherStatusMatchStatus { get; set; }
        public required bool HasActiveAlerts { get; set; }
    }
}
