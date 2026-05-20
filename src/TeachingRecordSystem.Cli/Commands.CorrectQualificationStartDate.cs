using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using File = System.IO.File;

namespace TeachingRecordSystem.Cli;

public static partial class Commands
{
    public static Command CorrectQualificationStartDate(IConfiguration configuration)
    {
        var inputOption = new Option<string>("--input") { Required = true };
        var outputOption = new Option<string>("--output") { Required = true };
        var connectionStringOption = new Option<string>("--connection-string") { Required = true };

        var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (configuredConnectionString is not null)
        {
            connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
        }

        var command = new Command(
            "correct-start-dates",
            "Corrects training start dates from CSV and outputs results.")
        {
            inputOption,
            outputOption,
            connectionStringOption
        };

        command.SetAction(async parseResult =>
        {
            var inputFile = parseResult.GetRequiredValue(inputOption);
            var outputFile = parseResult.GetRequiredValue(outputOption);
            var connectionString = parseResult.GetRequiredValue(connectionStringOption);

            if (!File.Exists(inputFile))
            {
                throw new FileNotFoundException($"Input file not found: {inputFile}", inputFile);
            }

            var services = new ServiceCollection()
                .AddClock()
                .AddDatabase(connectionString);

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TrsDbContext>();
            var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

            // fetch all routes
            var allRouteTypes = await dbContext.RouteToProfessionalStatusTypes.ToListAsync();
            var systemUser = SystemUser.SystemUserId;

            var results = new List<TrnCorrectionResult>();
            var updatedCount = 0;

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                HeaderValidated = null,
                BadDataFound = null,
                IgnoreBlankLines = true
            };

            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            using (var stream = File.OpenRead(inputFile))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            using (var csv = new CsvReader(reader, csvConfig))
            {
                var records = csv.GetRecordsAsync<CsvTrnStartDateRecord>();

                await foreach (var record in records)
                {
                    if (string.IsNullOrWhiteSpace(record.TRN))
                    {
                        Console.WriteLine("Skipping row with empty TRN");
                        continue;
                    }

                    var trn = record.TRN.Trim();

                    var person = await dbContext.Persons
                        .Where(p => p.Trn == trn)
                        .Select(p => new
                        {
                            Person = p,
                            Qualifications = p.Qualifications!.OfType<RouteToProfessionalStatus>().ToArray()
                        })
                        .FirstOrDefaultAsync();

                    if (person is null)
                    {
                        Console.WriteLine($"Skipping TRN {trn}: person not found");
                        continue;
                    }

                    var hasCurrent = TryParseDateOnly(record.CurrentStartDate, out var currentStart);
                    var hasCorrect = TryParseDateOnly(record.CorrectStartDate, out var correctStart);

                    if (!hasCurrent)
                    {
                        Console.WriteLine($"Skipping TRN {trn}: invalid current start date '{record.CurrentStartDate}'");
                        continue;
                    }

                    if (!hasCorrect)
                    {
                        Console.WriteLine($"Skipping TRN {trn}: invalid correct start date '{record.CorrectStartDate}'");
                        continue;
                    }

                    var matchingRoutes = person.Qualifications
                        .Where(q =>
                            q.TrainingStartDate.HasValue &&
                            currentStart.HasValue &&
                            q.TrainingStartDate.Value == currentStart.Value)
                        .ToArray();

                    if (matchingRoutes.Length == 0)
                    {
                        Console.WriteLine($"Skipping TRN {trn}: no qualification found with start date {currentStart}");
                        continue;
                    }

                    if (matchingRoutes.Length > 1)
                    {
                        Console.WriteLine($"Skipping TRN {trn}: multiple qualifications found with start date {currentStart}");
                        continue;
                    }

                    var route = matchingRoutes.Single();
                    var prevStart = route.TrainingStartDate;

                    route.Update(
                        allRouteTypes,
                        r => r.TrainingStartDate = correctStart,
                        changeReason: "Bulk correction",
                        changeReasonDetail: $"Corrected via CLI from {prevStart:yyyy-MM-dd} to {correctStart:yyyy-MM-dd}",
                        evidenceFile: null,
                        updatedBy: systemUser,
                        now: timeProvider.UtcNow,
                        out var updateEvent);

                    if (updateEvent is null)
                    {
                        Console.WriteLine($"Skipping TRN {trn}: no effective change");
                        continue;
                    }
                    dbContext.AddEventWithoutBroadcast(@updateEvent);

                    results.Add(new TrnCorrectionResult
                    {
                        TRN = trn,
                        PreviousStartDate = prevStart,
                        CurrentStartDate = route.TrainingStartDate,
                        QualificationId = route.QualificationId
                    });

                    updatedCount++;
                }
            }

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            using (var writer = new StreamWriter(outputFile))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteField("TRN");
                csvWriter.WriteField("PreviousStartDate");
                csvWriter.WriteField("CurrentStartDate");
                csvWriter.WriteField("QualificationId");
                await csvWriter.NextRecordAsync();

                foreach (var r in results)
                {
                    csvWriter.WriteField(r.TRN);
                    csvWriter.WriteField(r.PreviousStartDate?.ToString("dd/MM/yyyy"));
                    csvWriter.WriteField(r.CurrentStartDate?.ToString("dd/MM/yyyy"));
                    csvWriter.WriteField(r.QualificationId);
                    await csvWriter.NextRecordAsync();
                }
            }

            Console.WriteLine($"Updated {updatedCount} records.");
            Console.WriteLine($"Output written to {outputFile}");

            return 0;
        });

        return command;
    }

    private static bool TryParseDateOnly(string? input, out DateOnly? date)
    {
        date = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var s = input.Trim();

        if (DateOnly.TryParseExact(
                s,
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
        {
            date = parsed;
            return true;
        }

        return false;
    }
}

public class CsvTrnStartDateRecord
{
    [Name("TRN")]
    public string? TRN { get; set; }

    [Name("CurrentStartDate")]
    public string? CurrentStartDate { get; set; }

    [Name("CorrectStartDate")]
    public string? CorrectStartDate { get; set; }
}

public class TrnCorrectionResult
{
    [Name("TRN")]
    public string TRN { get; set; } = null!;

    [Name("PreviousStartDate")]
    public DateOnly? PreviousStartDate { get; set; }

    [Name("CurrentStartDate")]
    public DateOnly? CurrentStartDate { get; set; }

    [Name("QualificationId")]
    public Guid QualificationId { get; set; }
}
