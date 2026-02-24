using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentity;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.Core.Services.Webhooks;
using File = System.IO.File;

namespace TeachingRecordSystem.Cli;

public static partial class Commands
{
    public static Command CreateImportClaimTestDataCommand(IConfiguration configuration)
    {
        var fileOption = new Option<string>("--file") { Required = true };
        var outputOption = new Option<string>("--output") { Required = true };
        var connectionStringOption = new Option<string>("--connection-string") { Required = true };
        var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (configuredConnectionString is not null)
        {
            connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
        }

        var command =
            new Command("import-claim-test-data",
                "Imports claim test-data CSV, creates persons/qts routes and outputs a CSV with TRNs populated.")
            {
                fileOption, outputOption, connectionStringOption
            };

        command.SetAction(async parseResult =>
        {
            var fileName = parseResult.GetRequiredValue(fileOption);
            var outputFile = parseResult.GetRequiredValue(outputOption);
            var connectionString = parseResult.GetRequiredValue(connectionStringOption);

            var environment = new HostingEnvironment { EnvironmentName = Environments.Production };

            var services = new ServiceCollection()
                .AddClock()
                .AddDatabase(connectionString)
                .AddTrnRequestService(configuration)
                .AddPersonService()
                .AddOneLoginService()
                .AddSupportTaskService()
                .AddWebhookOptions(configuration)
                .AddWebhookDeliveryService(configuration)
                .AddWebhookMessageFactory()
                .AddMemoryCache()
                .AddIdentityApi(configuration)
                .AddEventPublisher()
                .AddBackgroundJobScheduler(environment)
                .AddHangfire(environment);

            services.AddDbContext<IdDbContext>(
                options => options.UseInMemoryDatabase("TeacherAuthId"),
                contextLifetime: ServiceLifetime.Transient);

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var personService = scope.ServiceProvider.GetRequiredService<PersonService>();
            var dbContext = scope.ServiceProvider.GetRequiredService<TrsDbContext>();
            var processContext = new ProcessContext(processType: ProcessType.PersonCreating, now: DateTime.UtcNow,
                SystemUser.SystemUserId);

            // throw error if input file doesn't exist
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException($"Input file not found: {fileName}", fileName);
            }

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                HeaderValidated = null,
                BadDataFound = null,
                IgnoreBlankLines = true
            };

            using var reader = new StreamReader(fileName);
            using var csv = new CsvReader(reader, csvConfig);
            var records = csv.GetRecords<dynamic>().ToList();
            var outputRows = new List<IDictionary<string, object?>>();
            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            var subjects = dbContext.TrainingSubjects.ToArray();
            var allRoutes = await dbContext.RouteToProfessionalStatusTypes.AsNoTracking().ToArrayAsync();
            foreach (var rec in records)
            {
                var dict = ((IDictionary<string, object?>)rec)
                    .ToDictionary(k => k.Key?.ToString() ?? string.Empty, v => v.Value);
                var firstName = GetStringValue(dict, "FIRST_NAME") ?? GetStringValue(dict, "FORENAME") ?? "";
                var middleName = GetStringValue(dict, "MIDDLE_NAME") ?? string.Empty;
                var lastName = GetStringValue(dict, "LAST_NAME") ?? GetStringValue(dict, "SURNAME") ?? "";
                var dobStr = GetStringValue(dict, "DATE_OF_BIRTH") ?? GetStringValue(dict, "DOB");
                var niNumber = GetStringValue(dict, "NATIONAL_INSURANCE_NUMBER") ??
                               GetStringValue(dict, "NATIONAL_INSURANCE_NUMBER");
                var ni = NationalInsuranceNumber.Parse(niNumber!);
                var qtsDateStr = GetStringValue(dict, "QTS_DATE");
                var inductionStatusStr = GetStringValue(dict, "INDUCTION_STATUS");
                var ittSubject1 = GetStringValue(dict, "ITT_SUBJECT_1") ?? GetStringValue(dict, "ITT_SUBJECT_1") ?? "";
                var startDateStr = GetStringValue(dict, "ITT_START_DATE") ??
                                   GetStringValue(dict, "ITT_START_DATE") ?? "";

                DateOnly? dob = ParseDateFlexible(dobStr);
                DateOnly? startDate = ParseDateFlexible(startDateStr);
                DateOnly? qtsDate = ParseDateFlexible(qtsDateStr);

                var createdPerson = await personService.CreatePersonAsync(
                    new CreatePersonOptions
                    {
                        Trn = default,
                        SourceTrnRequest = null,
                        FirstName = firstName,
                        MiddleName = middleName,
                        LastName = lastName,
                        DateOfBirth = dob,
                        EmailAddress = null,
                        NationalInsuranceNumber = ni,
                        Gender = null
                    },
                    processContext);
                var person = dbContext.Persons
                    .Include(x => x.Qualifications)
                    .Single(x => x.PersonId == createdPerson.PersonId);

                var subjectIds = subjects
                    .Where(s => s.Name.Equals(ittSubject1, StringComparison.OrdinalIgnoreCase))
                    .Select(s => s.TrainingSubjectId)
                    .ToArray();

                var routeId = RouteToProfessionalStatusType.AssessmentOnlyRouteId;
                var routeStatus = RouteToProfessionalStatusStatus.Holds;
                DateOnly? holdsFrom = qtsDate;
                var professionalStatus = RouteToProfessionalStatus.Create(
                    person,
                    allRouteTypes: allRoutes,
                    routeToProfessionalStatusTypeId: routeId,
                    sourceApplicationUserId: SystemUser.SystemUserId,
                    sourceApplicationReference: nameof(CreateImportClaimTestDataCommand),
                    status: routeStatus,
                    holdsFrom: qtsDate,
                    trainingStartDate: startDate,
                    trainingEndDate: null,
                    trainingSubjectIds: subjectIds,
                    trainingAgeSpecialismType: null,
                    trainingAgeSpecialismRangeFrom: null,
                    trainingAgeSpecialismRangeTo: null,
                    trainingCountryId: null,
                    trainingProviderId: null,
                    degreeTypeId: null,
                    isExemptFromInduction: false,
                    createdBy: SystemUser.SystemUserId,
                    now: DateTime.UtcNow,
                    changeReason: null,
                    changeReasonDetail: null,
                    evidenceFile: null,
                    @event: out var @event);

                dbContext.Qualifications.Add(professionalStatus);
                await dbContext.AddEventAndBroadcastAsync(@event);

                if (!string.IsNullOrWhiteSpace(inductionStatusStr))
                {
                    if (!Enum.TryParse<InductionStatus>(inductionStatusStr, ignoreCase: true, out var parsedInduction))
                    {
                        parsedInduction = inductionStatusStr?.Trim().ToLower() switch
                        {
                            "none" => InductionStatus.None,
                            "required" => InductionStatus.RequiredToComplete,
                            "requiredtocomplete" => InductionStatus.RequiredToComplete,
                            "inprogress" => InductionStatus.InProgress,
                            "pass" => InductionStatus.Passed,
                            "failed" => InductionStatus.Failed,
                            "exempt" => InductionStatus.Exempt,
                            _ => InductionStatus.None
                        };
                    }

                    person.SetInductionStatus(parsedInduction, startDate, null, exemptionReasonIds: [],
                        changeReason: "Imported", changeReasonDetail: null, evidenceFile: null,
                        updatedBy: SystemUser.SystemUserId, now: DateTime.UtcNow, out var inductionEvent);
                    if (inductionEvent is not null)
                    {
                        dbContext.AddEventWithoutBroadcast(inductionEvent);
                    }
                }
                await dbContext.SaveChangesAsync();

                var outRow = dict.ToDictionary(k => k.Key, v => v.Value);
                outRow["teacher_reference_number"] = person.Trn;
                outputRows.Add(outRow);
            }

            await transaction.CommitAsync();

            // Write csv with trn
            using var writer = new StreamWriter(outputFile);
            using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
            if (outputRows.Count > 0)
            {
                var header = outputRows[0].Keys.ToArray();
                foreach (var h in header)
                {
                    csvWriter.WriteField(h);
                }

                await csvWriter.NextRecordAsync();

                foreach (var row in outputRows)
                {
                    foreach (var h in header)
                    {
                        csvWriter.WriteField(row.TryGetValue(h, out var v) && v is not null
                            ? v.ToString()
                            : string.Empty);
                    }

                    await csvWriter.NextRecordAsync();
                }
            }

            return 0;
        });

        return command;
    }

    private static string? GetStringValue(IDictionary<string, object?> dict, string key)
    {
        var kv = dict.FirstOrDefault(k => string.Equals(k.Key, key, StringComparison.OrdinalIgnoreCase));
        return kv.Value?.ToString();
    }

    private static DateOnly? ParseDateFlexible(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return null;
        }

        s = s.Trim();

        if (DateOnly.TryParse(s, out var d))
        {
            return d;
        }

        if (DateOnly.TryParseExact(s, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
        {
            return d;
        }

        if (DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
        {
            return d;
        }

        if (DateTime.TryParse(s, out var dt))
        {
            return DateOnly.FromDateTime(dt);
        }

        return null;
    }

    private record Result(int? Value);
}
