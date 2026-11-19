using System.Globalization;
using System.Transactions;
using CsvHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.Core.Services.RoutesToProfessionalStatus;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.Core.Services.Webhooks;
using File = System.IO.File;

namespace TeachingRecordSystem.Cli;

public partial class Commands
{
    public static Command CreatePersonCommand(IConfiguration configuration)
    {
        return new Command("person", "Commands for managing person records.")
        {
            CreateUnmergeCommand(),
            CreateImportEwcWalesCommand()
        };

        Command CreateImportEwcWalesCommand()
        {
            var fileOption = new Option<string>("--file") { Required = true };
            var connectionStringOption = new Option<string>("--connection-string") { Required = true };

            var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
            if (configuredConnectionString is not null)
            {
                connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
            }

            var command = new Command("import-ewc-wales", "Imports person records from an EWC Wales CSV file (TRN, Surname, Forename, DOB, NI Number).")
            {
                fileOption,
                connectionStringOption
            };

            command.SetAction(
                async parseResult =>
                {
                    var fileName = parseResult.GetRequiredValue(fileOption);
                    var connectionString = parseResult.GetRequiredValue(connectionStringOption);

                    if (!File.Exists(fileName))
                    {
                        parseResult.InvocationConfiguration.Error.WriteLine($"Input file not found: {fileName}");
                        return 1;
                    }

                    var environment = new HostingEnvironment { EnvironmentName = Environments.Production };

                    var services = new ServiceCollection()
                        .AddTimeProvider()
                        .AddDatabase(connectionString)
                        .AddTrnRequestService(configuration)
                        .AddPersonService()
                        .AddOneLoginService()
                        .AddSupportTaskService()
                        .AddWebhookOptions(configuration)
                        .AddWebhookDeliveryService(configuration)
                        .AddWebhookMessageFactory()
                        .AddMemoryCache()
                        .AddEventPublisher()
                        .AddBackgroundJobScheduler(environment)
                        .AddHangfire(environment)
                        .AddSingleton<ReferenceDataCache>()
                        .AddRoutesToProfessionalStatusService()
                        .BuildServiceProvider();

                    // Parse and validate the whole file before touching the database
                    var rows = new List<(int RowNumber, string Trn, string LastName, string FirstName, DateOnly? DateOfBirth, NationalInsuranceNumber? NationalInsuranceNumber)>();
                    var errors = new List<string>();

                    using (var reader = new StreamReader(fileName))
                    using (var csv = CsvReader.Create(reader, new CsvReaderOptions { Separator = ',', SkipEmptyLines = true }))
                    {
                        var rowNumber = 1;
                        foreach (var record in csv.GetRecords<CsvDynamicRecord>())
                        {
                            rowNumber++;
                            var values = record.ToDictionary().ToDictionary(k => k.Key.Trim(), v => (object?)v.Value, StringComparer.OrdinalIgnoreCase);

                            var trn = GetStringValue(values, "TRN")?.Trim();
                            var lastName = GetStringValue(values, "Surname")?.Trim();
                            var firstName = GetStringValue(values, "Forename")?.Trim();
                            var dobStr = GetStringValue(values, "DOB")?.Trim();
                            var niStr = GetStringValue(values, "NI Number")?.Trim();

                            if (string.IsNullOrEmpty(trn) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName))
                            {
                                errors.Add($"Row {rowNumber}: TRN, Surname and Forename are required.");
                                continue;
                            }

                            DateOnly? dob = null;
                            if (!string.IsNullOrEmpty(dobStr))
                            {
                                if (!DateOnly.TryParseExact(dobStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDob))
                                {
                                    errors.Add($"Row {rowNumber}: invalid DOB '{dobStr}' (expected dd/MM/yyyy).");
                                    continue;
                                }

                                dob = parsedDob;
                            }

                            NationalInsuranceNumber? ni = null;
                            if (!string.IsNullOrEmpty(niStr))
                            {
                                if (!NationalInsuranceNumber.TryParse(niStr, out var parsedNi))
                                {
                                    errors.Add($"Row {rowNumber}: invalid NI Number '{niStr}'.");
                                    continue;
                                }

                                ni = parsedNi;
                            }

                            rows.Add((rowNumber, trn, lastName, firstName, dob, ni));
                        }
                    }

                    var duplicateTrns = rows.GroupBy(r => r.Trn).Where(g => g.Count() > 1).Select(g => g.Key);
                    errors.AddRange(duplicateTrns.Select(t => $"TRN {t} appears more than once in the file."));

                    if (errors.Count > 0)
                    {
                        foreach (var error in errors)
                        {
                            parseResult.InvocationConfiguration.Error.WriteLine(error);
                        }

                        return 1;
                    }

                    using var scope = services.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<TrsDbContext>();
                    var personService = scope.ServiceProvider.GetRequiredService<PersonService>();
                    var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
                    var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

                    // Resolve this up front so Hangfire's storage (which may prepare its schema on first use) is
                    // initialised outside the transaction below; a failure there would abort the ambient transaction.
                    _ = scope.ServiceProvider.GetRequiredService<IBackgroundJobScheduler>();

                    using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                    var allTrns = rows.Select(r => r.Trn).ToArray();
                    var existingTrns = await dbContext.Persons
                        .IgnoreQueryFilters([QueryFilterNames.Person.Deactivated])
                        .Where(p => allTrns.Contains(p.Trn!))
                        .Select(p => p.Trn)
                        .ToListAsync();

                    if (existingTrns.Count > 0)
                    {
                        parseResult.InvocationConfiguration.Error.WriteLine(
                            $"Persons already exist with TRN(s): {string.Join(", ", existingTrns)}");
                        return 1;
                    }

                    var processContext = new ProcessContext(ProcessType.PersonImportingFromEwcWales, timeProvider.UtcNow, SystemUser.SystemUserId);

                    // The event scope runs its handlers when it's disposed, so it has to close before the transaction
                    // is completed.
                    await using (var eventScope = eventPublisher.GetOrCreateEventScope(processContext))
                    {
                        foreach (var row in rows)
                        {
                            await personService.CreatePersonAsync(
                                new CreatePersonOptions
                                {
                                    Trn = Optional.Option.Some(row.Trn),
                                    SourceTrnRequest = null,
                                    FirstName = row.FirstName,
                                    MiddleName = string.Empty,
                                    LastName = row.LastName,
                                    DateOfBirth = row.DateOfBirth,
                                    EmailAddress = null,
                                    NationalInsuranceNumber = row.NationalInsuranceNumber,
                                    Gender = null
                                },
                                processContext);
                        }
                    }

                    transaction.Complete();

                    parseResult.InvocationConfiguration.Output.WriteLine($"Imported {rows.Count} person(s).");
                    return 0;
                });

            return command;
        }

        Command CreateUnmergeCommand()
        {
            var trnOption = new Option<string>("--trn") { Required = true };
            var connectionStringOption = new Option<string>("--connection-string") { Required = true };

            var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
            if (configuredConnectionString is not null)
            {
                connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
            }

            var command = new Command("unmerge", "Un-merges a record that was deactivated by a merge from the record it was merged into.")
            {
                trnOption,
                connectionStringOption
            };

            command.SetAction(
                async parseResult =>
                {
                    var trn = parseResult.GetRequiredValue(trnOption);
                    var connectionString = parseResult.GetRequiredValue(connectionStringOption);

                    var services = new ServiceCollection()
                        .AddTimeProvider()
                        .AddDatabase(connectionString)
                        .AddMemoryCache()
                        .AddWebhookMessageFactory()
                        .AddEventPublisher()
                        .BuildServiceProvider();

                    using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                    using var scope = services.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<TrsDbContext>();
                    var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

                    var person = await dbContext.Persons
                        .IgnoreQueryFilters([QueryFilterNames.Person.Deactivated])
                        .SingleOrDefaultAsync(p => p.Trn == trn);

                    if (person is null)
                    {
                        parseResult.InvocationConfiguration.Error.WriteLine("Person was not found");
                        return 1;
                    }

                    if (person.MergedWithPersonId is not Guid mergedWithPersonId)
                    {
                        parseResult.InvocationConfiguration.Error.WriteLine("Person has not been merged with another record");
                        return 1;
                    }

                    var mergedWithPersonTrn = await dbContext.Persons
                        .IgnoreQueryFilters([QueryFilterNames.Person.Deactivated])
                        .Where(p => p.PersonId == mergedWithPersonId)
                        .Select(p => p.Trn)
                        .SingleAsync();

                    var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

                    var now = timeProvider.UtcNow;

                    var processContext = new ProcessContext(ProcessType.PersonUnmerging, now, SystemUser.SystemUserId);

                    // The event scope runs its handlers when it's disposed, so it has to close before the transaction
                    // is completed.
                    await using (var eventScope = eventPublisher.GetOrCreateEventScope(processContext))
                    {
                        // Only the deactivation and the link back to the retained record are reversed; the details the
                        // merge copied onto the retained record and any One Login users it re-pointed are left alone.
                        person.Status = PersonStatus.Active;
                        person.MergedWithPersonId = null;
                        person.UpdatedOn = now;

                        await dbContext.SaveChangesAsync();

                        await eventScope.PublishEventAsync(
                            new PersonUnmergedEvent
                            {
                                EventId = Guid.NewGuid(),
                                PersonId = person.PersonId,
                                UnmergedFromPersonId = mergedWithPersonId
                            });
                    }

                    transaction.Complete();

                    parseResult.InvocationConfiguration.Output.WriteLine(
                        $"Un-merged TRN {trn} from TRN {mergedWithPersonTrn}.");
                    parseResult.InvocationConfiguration.Output.WriteLine(
                        $"WARNING: any One Login users that were linked to TRN {trn} before the merge remain linked to TRN {mergedWithPersonTrn} and have not been moved back.");
                    return 0;
                });

            return command;
        }
    }
}
