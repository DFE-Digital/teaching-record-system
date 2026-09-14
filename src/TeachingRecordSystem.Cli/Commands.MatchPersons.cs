using System.Globalization;
using System.Linq.Expressions;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;
using File = System.IO.File;

namespace TeachingRecordSystem.Cli;

public static partial class Commands
{
    private const string TrnColumn = "TRN";

    public static Command CreateMatchPersonsCommand(IConfiguration configuration)
    {
        var inOption = new Option<string>("--in") { Required = true };
        var outOption = new Option<string>("--out") { Required = true };
        var connectionStringOption = new Option<string>("--connection-string") { Required = true };

        var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (configuredConnectionString is not null)
        {
            connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
        }

        var command = new Command(
            "match-persons",
            "Matches the people in a CSV of personal details against teaching records and writes the matched TRNs to another CSV.")
        {
            inOption,
            outOption,
            connectionStringOption
        };

        command.SetAction(
            async parseResult =>
            {
                var inputFileName = parseResult.GetRequiredValue(inOption);
                var outputFileName = parseResult.GetRequiredValue(outOption);
                var connectionString = parseResult.GetRequiredValue(connectionStringOption);

                var error = parseResult.InvocationConfiguration.Error;

                if (!File.Exists(inputFileName))
                {
                    error.WriteLine($"Input file was not found: {inputFileName}");
                    return 1;
                }

                var (rows, errors) = ReadMatchPersonsInputFile(inputFileName);

                if (errors.Length > 0)
                {
                    foreach (var e in errors)
                    {
                        error.WriteLine(e);
                    }

                    return 1;
                }

                var services = new ServiceCollection()
                    .AddTimeProvider()
                    .AddLogging()
                    .AddDatabase(connectionString)
                    .AddMemoryCache()
                    .AddWebhookMessageFactory()
                    .AddEventPublisher()
                    .AddPersonService()
                    .AddOneLoginService()
                    .AddSupportTaskServices()
                    .AddTrnRequestService(configuration)
                    // TrnRequestService's dependencies pull in a job scheduler that matching never uses;
                    // this one fails loudly rather than letting the command queue any work.
                    .AddSingleton<IBackgroundJobScheduler, UnavailableBackgroundJobScheduler>()
                    .BuildServiceProvider();

                using var scope = services.CreateScope();
                var trnRequestService = scope.ServiceProvider.GetRequiredService<TrnRequestService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<TrsDbContext>();
                var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
                var now = timeProvider.UtcNow;

                await using var writer = new StreamWriter(outputFileName);
                await using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);

                csvWriter.WriteField(TrnColumn);
                await csvWriter.NextRecordAsync();

                var matchedRowCount = 0;

                foreach (var row in rows)
                {
                    // The request is never added to the DbContext so nothing here is persisted; matching is read-only.
                    var request = new TrnRequestMetadata
                    {
                        ApplicationUserId = SystemUser.SystemUserId,
                        RequestId = Guid.NewGuid().ToString(),
                        CreatedOn = now,
                        IdentityVerified = null,
                        EmailAddress = null,
                        OneLoginUserSubject = null,
                        FirstName = row.FirstName,
                        MiddleName = row.MiddleName,
                        LastName = row.LastName,
                        PreviousLastName = row.PreviousLastName,
                        Name = new[] { row.FirstName, row.MiddleName, row.LastName }.GetNonEmptyValues(),
                        DateOfBirth = row.DateOfBirth
                    };

                    var matchResult = await trnRequestService.MatchPersonsAsync(request);

                    string? trn = null;

                    if (matchResult.Outcome is MatchPersonsResultOutcome.DefiniteMatch)
                    {
                        trn = matchResult.Trn;
                    }
                    else if (matchResult.Outcome is MatchPersonsResultOutcome.PotentialMatches && matchResult.Matches.Count == 1)
                    {
                        // A definite match needs a national insurance number, or an email address and gender, to match on
                        // and the input file has none of those; a single potential match on names and date of birth is the
                        // strongest signal available here. Rows that match more than one record are left blank for review.
                        var matchedPersonId = matchResult.Matches.Single().PersonId;

                        trn = await dbContext.Persons
                            .Where(p => p.PersonId == matchedPersonId)
                            .Select(p => p.Trn)
                            .SingleAsync();
                    }

                    if (trn is not null)
                    {
                        matchedRowCount++;
                    }

                    csvWriter.WriteField(trn ?? "");
                    await csvWriter.NextRecordAsync();
                }

                parseResult.InvocationConfiguration.Output.WriteLine(
                    $"Matched {matchedRowCount} of {rows.Length} rows.");
                return 0;
            });

        return command;
    }

    private static (MatchPersonsRow[] Rows, string[] Errors) ReadMatchPersonsInputFile(string fileName)
    {
        const string forenameColumn = "Forename";
        const string surnameColumn = "Surname";
        const string middleNamesColumn = "Middle names";
        const string previousSurnameColumn = "Previous surname";
        const string dateOfBirthColumn = "DOB";
        const string dateOfBirthFormat = "d/M/yyyy";

        var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            MissingFieldFound = null,
            HeaderValidated = null,
            TrimOptions = TrimOptions.Trim,
            PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant()
        };

        using var reader = new StreamReader(fileName);
        using var csv = new CsvReader(reader, csvConfiguration);

        if (!csv.Read() || !csv.ReadHeader())
        {
            return ([], ["The input file is empty."]);
        }

        var header = csv.HeaderRecord!;

        var missingColumns = new[] { forenameColumn, surnameColumn, dateOfBirthColumn }
            .Where(c => !header.Any(h => h.Trim().Equals(c, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        if (missingColumns.Length > 0)
        {
            return ([], [$"The input file is missing the following column(s): {string.Join(", ", missingColumns)}."]);
        }

        var rows = new List<MatchPersonsRow>();
        var errors = new List<string>();

        while (csv.Read())
        {
            var rowNumber = csv.Parser.Row;

            var forename = GetField(forenameColumn);
            var surname = GetField(surnameColumn);
            var middleNames = GetField(middleNamesColumn);
            var previousSurname = GetField(previousSurnameColumn);
            var dateOfBirth = GetField(dateOfBirthColumn);

            var rowErrorCount = errors.Count;

            if (forename is null)
            {
                errors.Add($"Row {rowNumber}: '{forenameColumn}' is missing.");
            }

            if (surname is null)
            {
                errors.Add($"Row {rowNumber}: '{surnameColumn}' is missing.");
            }

            var parsedDateOfBirth = default(DateOnly);

            if (dateOfBirth is null)
            {
                errors.Add($"Row {rowNumber}: '{dateOfBirthColumn}' is missing.");
            }
            else if (!DateOnly.TryParseExact(dateOfBirth, dateOfBirthFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateOfBirth))
            {
                errors.Add($"Row {rowNumber}: '{dateOfBirthColumn}' is not in the {dateOfBirthFormat} format.");
            }

            if (errors.Count == rowErrorCount)
            {
                rows.Add(new MatchPersonsRow(forename!, surname!, middleNames, previousSurname, parsedDateOfBirth));
            }

            string? GetField(string name)
            {
                var value = csv.GetField(name);
                return string.IsNullOrWhiteSpace(value) ? null : value;
            }
        }

        return (rows.ToArray(), errors.ToArray());
    }

    private record MatchPersonsRow(
        string FirstName,
        string LastName,
        string? MiddleName,
        string? PreviousLastName,
        DateOnly DateOfBirth);

    private class UnavailableBackgroundJobScheduler : IBackgroundJobScheduler
    {
        public Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> expression) where T : notnull =>
            throw new NotSupportedException("Background jobs cannot be scheduled from this command.");

        public Task<string> ContinueJobWithAsync<T>(string parentId, Expression<Func<T, Task>> expression) where T : notnull =>
            throw new NotSupportedException("Background jobs cannot be scheduled from this command.");

        public Task WaitForJobToCompleteAsync(string jobId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("Background jobs cannot be scheduled from this command.");
    }
}
