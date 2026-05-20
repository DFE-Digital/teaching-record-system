using System.CommandLine;
using System.Text;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Cli.Tests.CommandTests;

public class CorrectQualificationStartDateTests(IServiceProvider services) : CommandTestBase(services)
{
    [Fact]
    public async Task MissingImportFileNameOption_ReturnsError()
    {
        // Arrange

        var command = GetCommand();

        // Act
        var parseResult = command.Parse(["correct-start-dates"]);

        // Assert
        Assert.Contains(parseResult.Errors, e => e.Message == "Option '--input' is required.");
    }

    [Fact]
    public async Task MissingOutputFileNameOption_ReturnsError()
    {
        // Arrange

        var command = GetCommand();

        // Act
        var parseResult = command.Parse(["correct-start-dates"]);

        // Assert
        Assert.Contains(parseResult.Errors, e => e.Message == "Option '--output' is required.");
    }

    [Fact]
    public async Task ValidImportFile_StartDateMismatch_DoesNotCorrectStartDate()
    {
        // Arrange
        var command = GetCommand();

        var baseDir = AppContext.BaseDirectory;
        var importFilePath = Path.Combine(baseDir, $"update-trns-{Guid.NewGuid()}.csv");
        var outputFilePath = Path.Combine(baseDir, $"update-trns-{Guid.NewGuid()}.csv");
        var person = await TestData.CreatePersonAsync(x => x.WithQts());
        var personWithQualification = await WithDbContextAsync(x => x.Persons
            .Where(p => p.Trn == person.Trn)
            .Select(p => new
            {
                Person = p,
                Qualifications = p.Qualifications!.OfType<RouteToProfessionalStatus>().ToArray()
            })
            .FirstOrDefaultAsync());
        var qualification = personWithQualification!.Qualifications!.First();
        var existingStartDate = qualification.TrainingStartDate ?? throw new InvalidOperationException("Test data must have a training start date");
        var correctedStartDate = existingStartDate.AddDays(-45);
        var incorrectCurrentStartDate = existingStartDate.AddDays(-50);

        var importRows = new[]
        {
            new
            {
                TRN = person.Trn,
                CurrentStartDate = incorrectCurrentStartDate,
                CorrectStartDate = correctedStartDate
            },
        };

        var csv = new StringBuilder()
            .AppendLine(
                "TRN,CurrentStartDate,CorrectStartDate");

        foreach (var row in importRows)
        {
            csv.AppendLine(string.Join(',',
                row.TRN,
                row.CurrentStartDate.ToString("dd/MM/yyyy"),
                row.CorrectStartDate.ToString("dd/MM/yyyy")));
        }

        await File.WriteAllTextAsync(importFilePath, csv.ToString(), Encoding.UTF8);

        try
        {
            // Act
            var parseResult = command.Parse(new[]
            {
                "correct-start-dates", "--input", importFilePath, "--output", outputFilePath,
            });

            var exitCode = await parseResult.InvokeAsync();

            // Assert – command execution
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(outputFilePath));
            var updatedQualification = await WithDbContextAsync(x => x.Persons
                .Where(p => p.Trn == person.Trn)
                .SelectMany(p => p.Qualifications!.OfType<RouteToProfessionalStatus>())
                .SingleAsync());

            Assert.Equal(existingStartDate, updatedQualification.TrainingStartDate);
        }
        finally
        {
            if (File.Exists(importFilePath))
            {
                File.Delete(importFilePath);
            }

            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }
        }
    }

    [Fact]
    public async Task ValidImportFile_EachRowAmendsStartDate()
    {
        // Arrange
        var command = GetCommand();

        var baseDir = AppContext.BaseDirectory;
        var importFilePath = Path.Combine(baseDir, $"update-trns-{Guid.NewGuid()}.csv");
        var outputFilePath = Path.Combine(baseDir, $"update-trns-{Guid.NewGuid()}.csv");
        var person1 = await TestData.CreatePersonAsync(x => x.WithQts());
        var person2 = await TestData.CreatePersonAsync(x => x.WithQts());
        var personWithQualification1 = await WithDbContextAsync(x => x.Persons
            .Where(p => p.Trn == person1.Trn)
            .Select(p => new
            {
                Person = p,
                Qualifications = p.Qualifications!.OfType<RouteToProfessionalStatus>().ToArray()
            })
            .FirstOrDefaultAsync());
        var personWithQualification2 = await WithDbContextAsync(x => x.Persons
            .Where(p => p.Trn == person2.Trn)
            .Select(p => new
            {
                Person = p,
                Qualifications = p.Qualifications!.OfType<RouteToProfessionalStatus>().ToArray()
            })
            .FirstOrDefaultAsync());
        var qualification1 = personWithQualification1!.Qualifications!.First();
        var qualification2 = personWithQualification2!.Qualifications!.First();
        var existingStartDate1 = qualification1.TrainingStartDate ?? throw new InvalidOperationException("Test data must have a training start date");
        var correctedStartDate1 = existingStartDate1.AddDays(-45);
        var existingStartDate2 = qualification2.TrainingStartDate ?? throw new InvalidOperationException("Test data must have a training start date");
        var correctedStartDate2 = existingStartDate2.AddDays(-45);


        var importRows = new[]
        {
            new
            {
                TRN = person1.Trn,
                CurrentStartDate = qualification1.TrainingStartDate,
                CorrectStartDate = correctedStartDate1
            },
            new
            {
                TRN = person2.Trn,
                CurrentStartDate = qualification2.TrainingStartDate,
                CorrectStartDate = correctedStartDate2
            },
        };

        var csv = new StringBuilder()
            .AppendLine(
                "TRN,CurrentStartDate,CorrectStartDate");

        foreach (var row in importRows)
        {
            csv.AppendLine(string.Join(',',
                row.TRN,
                row.CurrentStartDate?.ToString("dd/MM/yyyy"),
                row.CorrectStartDate.ToString("dd/MM/yyyy")));
        }

        await File.WriteAllTextAsync(importFilePath, csv.ToString(), Encoding.UTF8);

        try
        {
            // Act
            var parseResult = command.Parse(new[]
            {
                "correct-start-dates", "--input", importFilePath, "--output", outputFilePath,
            });

            var exitCode = await parseResult.InvokeAsync();

            // Assert – command execution
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(outputFilePath));
            var updatedQualification1 = await WithDbContextAsync(x => x.Persons
                .Where(p => p.Trn == person1.Trn)
                .SelectMany(p => p.Qualifications!.OfType<RouteToProfessionalStatus>())
                .SingleAsync());
            var updatedQualification2 = await WithDbContextAsync(x => x.Persons
                .Where(p => p.Trn == person1.Trn)
                .SelectMany(p => p.Qualifications!.OfType<RouteToProfessionalStatus>())
                .SingleAsync());
            var @person1Event = await WithDbContextAsync(dbContext => dbContext.Events.SingleOrDefaultAsync(e => e.EventName == typeof(LegacyEvents.RouteToProfessionalStatusCreatedEvent).Name && e.PersonIds.Contains(person1.PersonId)));
            var @person2Event = await WithDbContextAsync(dbContext => dbContext.Events.SingleOrDefaultAsync(e => e.EventName == typeof(LegacyEvents.RouteToProfessionalStatusCreatedEvent).Name && e.PersonIds.Contains(person2.PersonId)));

            Assert.NotNull(@person1Event);
            Assert.NotNull(@person2Event);
            Assert.Equal(correctedStartDate1, updatedQualification1.TrainingStartDate);
            Assert.Equal(correctedStartDate2, updatedQualification2.TrainingStartDate);
        }
        finally
        {
            if (File.Exists(importFilePath))
            {
                File.Delete(importFilePath);
            }

            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }
        }
    }

    private Command GetCommand() => Commands.CorrectQualificationStartDate(Configuration);
}
