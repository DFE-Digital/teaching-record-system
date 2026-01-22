using System.CommandLine;
using System.Text;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Cli.Tests.CommandTests;

public class CreateImportClaimTestDataTests(IServiceProvider services) : CommandTestBase(services)
{
    [Fact]
    public async Task MissingImportFileNameOption_ReturnsError()
    {
        // Arrange

        var command = GetCommand();

        // Act
        var parseResult = command.Parse(["import-claim-test-data"]);

        // Assert
        Assert.Contains(parseResult.Errors, e => e.Message == "Option '--file' is required.");
    }

    [Fact]
    public async Task MissingOutputFileNameOption_ReturnsError()
    {
        // Arrange

        var command = GetCommand();

        // Act
        var parseResult = command.Parse(["import-claim-test-data"]);

        // Assert
        Assert.Contains(parseResult.Errors, e => e.Message == "Option '--output' is required.");
    }


    [Fact]
    public async Task ValidImportFile_EachRowCreatesPersonWithMatchingDetails()
    {
        // Arrange
        var command = GetCommand();

        var baseDir = AppContext.BaseDirectory;
        var importFilePath = Path.Combine(baseDir, $"import-claim-test-data-{Guid.NewGuid()}.csv");
        var outputFilePath = Path.Combine(baseDir, $"import-claim-test-data-output-{Guid.NewGuid()}.csv");

        var importRows = new[]
        {
            new
            {
                FirstName = Faker.Name.First(),
                LastName = Faker.Name.Last(),
                MiddleName = string.Empty,
                DateOfBirth = new DateOnly(1965, 7, 8),
                NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber(),
                CsvInductionStatus = "Pass",
                ExpectedInductionStatus = InductionStatus.Passed,
                IttSubject1 = "applied computing",
                QtsDate = new DateOnly(2019,02,17)
            },
            new
            {
                FirstName = Faker.Name.First(),
                LastName = Faker.Name.Last(),
                MiddleName = string.Empty,
                DateOfBirth = new DateOnly(1970, 1, 15),
                NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber(),
                CsvInductionStatus = "inprogress",
                ExpectedInductionStatus = InductionStatus.InProgress,
                IttSubject1 = "applied computing",
                QtsDate = new DateOnly(2013,04,5)
            },
            new
            {
                FirstName = Faker.Name.First(),
                LastName = Faker.Name.Last(),
                MiddleName = string.Empty,
                DateOfBirth = new DateOnly(1980, 3, 20),
                NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber(),
                CsvInductionStatus = "failed",
                ExpectedInductionStatus = InductionStatus.Failed,
                IttSubject1 = "applied computing",
                QtsDate = new DateOnly(2021,04,01)
            },
            new
            {
                FirstName = Faker.Name.First(),
                LastName = Faker.Name.Last(),
                MiddleName = string.Empty,
                DateOfBirth = new DateOnly(1990, 6, 5),
                NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber(),
                CsvInductionStatus = "Pass",
                ExpectedInductionStatus = InductionStatus.Passed,
                IttSubject1 = "applied computing",
                QtsDate = new DateOnly(2017,04,7)
            },
            new
            {
                FirstName = Faker.Name.First(),
                LastName = Faker.Name.Last(),
                MiddleName = string.Empty,
                DateOfBirth = new DateOnly(1995, 11, 30),
                NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber(),
                CsvInductionStatus = "Exempt",
                ExpectedInductionStatus = InductionStatus.Exempt,
                IttSubject1 = "applied computing",
                QtsDate = new DateOnly(2011,01,04)
            }
        };

        var csv = new StringBuilder()
            .AppendLine(
                "first_name,last_name,date_of_birth,national_insurance_number,qts_date,induction_status,route_type,itt_subject_1,itt_start_date,itt_qualification_type,active_alert");

        foreach (var row in importRows)
        {
            csv.AppendLine(string.Join(',',
                row.FirstName,
                row.LastName,
                row.DateOfBirth.ToString("dd/MM/yyyy"),
                row.NationalInsuranceNumber,
                row.QtsDate,
                row.CsvInductionStatus,
                "undergraduate_itt",
                row.IttSubject1,
                "2024-09-01",
                "BA",
                "false"));
        }

        await File.WriteAllTextAsync(importFilePath, csv.ToString(), Encoding.UTF8);

        try
        {
            // Act
            var parseResult = command.Parse(new[]
            {
                "import-claim-test-data", "--file", importFilePath, "--output", outputFilePath,
            });

            var exitCode = await parseResult.InvokeAsync();

            // Assert – command execution
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(outputFilePath));

            var outputLines = await File.ReadAllLinesAsync(outputFilePath);
            Assert.Equal(importRows.Length + 1, outputLines.Length);

            var header = outputLines[0].Split(',');
            var trnIndex = Array.IndexOf(header, "teacher_reference_number");
            Assert.True(trnIndex >= 0);

            await WithDbContextAsync(async dbContext =>
            {
                for (var i = 0; i < importRows.Length; i++)
                {
                    var expected = importRows[i];
                    var trn = outputLines[i + 1].Split(',')[trnIndex];

                    Assert.False(string.IsNullOrWhiteSpace(trn));

                    var person = await dbContext.Persons
                        .Include(p => p.Qualifications)
                        .SingleAsync(p => p.Trn == trn);

                    Assert.NotNull(person);
                    Assert.Equal(expected.FirstName, person.FirstName);
                    Assert.Equal(expected.LastName, person.LastName);
                    Assert.Equal(expected.MiddleName, person.MiddleName ?? string.Empty);
                    Assert.Equal(expected.DateOfBirth, person.DateOfBirth);
                    Assert.Equal(expected.NationalInsuranceNumber, person.NationalInsuranceNumber!);
                    Assert.Equal(expected.ExpectedInductionStatus, person.InductionStatus);
                    var qts = person.Qualifications!
                        .OfType<RouteToProfessionalStatus>()
                        .Single();

                    Assert.Equal(expected.QtsDate, qts.HoldsFrom);
                }
            });
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


    private Command GetCommand() => Commands.CreateImportClaimTestDataCommand(Configuration);
}
