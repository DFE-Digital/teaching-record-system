using System.CommandLine;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Cli.Tests.CommandTests;

public class ImportEwcWalesTests(IServiceProvider services) : CommandTestBase(services)
{
    private const string Header = "TRN,Surname,Forename,DOB,NI Number";

    [Fact]
    public async Task MissingFileOption_ReturnsParseError()
    {
        // Arrange
        var command = GetSubcommand();

        // Act
        var parseResult = command.Parse(["--connection-string", "Host=localhost"]);

        // Assert
        Assert.Contains(parseResult.Errors, e => e.Message == "Option '--file' is required.");
    }

    [Fact]
    public async Task FileDoesNotExist_ReturnsError()
    {
        // Arrange
        var path = Path.Combine(Path.GetTempPath(), $"ewc-wales-{Guid.NewGuid()}.csv");

        // Act
        var (exitCode, _, error) = await InvokeAsync(path);

        // Assert
        Assert.Equal(1, exitCode);
        Assert.Contains("Input file not found", error);
    }

    [Fact]
    public async Task ValidFile_CreatesPersonsWithMatchingDetailsUnderASingleProcess()
    {
        // Arrange
        var trn1 = GenerateTrn();
        var trn2 = GenerateTrn(trn1);
        var trn3 = GenerateTrn(trn1, trn2);

        var csv = string.Join(
            Environment.NewLine,
            Header,
            $"{trn1},Smith,Alice,08/07/1965,AB123456C",
            $"{trn2},Jones,Bob,15/01/1970,",
            $"{trn3},Evans,Carys,05/11/1995,JK654321A");

        // Act
        var (exitCode, output, error) = await InvokeWithCsvAsync(csv);

        // Assert
        Assert.True(exitCode == 0, error);
        Assert.Contains("Imported 3 person(s).", output);

        await WithDbContextAsync(async dbContext =>
        {
            var persons = await dbContext.Persons
                .Where(p => new[] { trn1, trn2, trn3 }.Contains(p.Trn!))
                .ToDictionaryAsync(p => p.Trn!);

            Assert.Equal(3, persons.Count);

            var alice = persons[trn1];
            Assert.Equal("Alice", alice.FirstName);
            Assert.Equal("Smith", alice.LastName);
            Assert.Equal(new DateOnly(1965, 7, 8), alice.DateOfBirth);
            Assert.Equal("AB123456C", alice.NationalInsuranceNumber);

            var bob = persons[trn2];
            Assert.Equal("Bob", bob.FirstName);
            Assert.Equal("Jones", bob.LastName);
            Assert.Equal(new DateOnly(1970, 1, 15), bob.DateOfBirth);
            Assert.Null(bob.NationalInsuranceNumber);

            var personIds = persons.Values.Select(p => p.PersonId).ToArray();
            var processEvents = await dbContext.ProcessEvents
                .Where(pe => pe.EventName == nameof(PersonCreatedEvent) && pe.PersonIds.Any(id => personIds.Contains(id)))
                .ToListAsync();

            Assert.Equal(3, processEvents.Count);
            Assert.Single(processEvents.Select(pe => pe.ProcessId).Distinct());

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvents[0].ProcessId);
            Assert.Equal(ProcessType.PersonImportingFromEwcWales, process.ProcessType);
            Assert.Equal(SystemUser.SystemUserId, process.UserId);
        });
    }

    [Fact]
    public async Task HeadingsAreMatchedCaseInsensitively()
    {
        // Arrange
        var trn = GenerateTrn();
        var csv = string.Join(
            Environment.NewLine,
            "trn,surname,forename,dob,ni number",
            $"{trn},Smith,Alice,08/07/1965,AB123456C");

        // Act
        var (exitCode, _, _) = await InvokeWithCsvAsync(csv);

        // Assert
        Assert.Equal(0, exitCode);
        await AssertPersonExistsAsync(trn, exists: true);
    }

    [Theory]
    [InlineData("{TRN},Smith,Alice,1965-07-08,AB123456C", "invalid DOB")]
    [InlineData("{TRN},Smith,Alice,08/07/1965,NOT-AN-NI", "invalid NI Number")]
    [InlineData(",Smith,Alice,08/07/1965,AB123456C", "TRN, Surname and Forename are required")]
    [InlineData("{TRN},,Alice,08/07/1965,AB123456C", "TRN, Surname and Forename are required")]
    [InlineData("{TRN},Smith,,08/07/1965,AB123456C", "TRN, Surname and Forename are required")]
    public async Task InvalidRow_ReturnsErrorAndCreatesNoPersons(string rowTemplate, string expectedError)
    {
        // Arrange
        var validTrn = GenerateTrn();
        var invalidTrn = GenerateTrn(validTrn);
        var invalidRow = rowTemplate.Replace("{TRN}", invalidTrn);

        var csv = string.Join(
            Environment.NewLine,
            Header,
            $"{validTrn},Jones,Bob,15/01/1970,",
            invalidRow);

        // Act
        var (exitCode, _, error) = await InvokeWithCsvAsync(csv);

        // Assert
        Assert.Equal(1, exitCode);
        Assert.Contains(expectedError, error);
        Assert.Contains("Row 3", error);
        await AssertPersonExistsAsync(validTrn, exists: false);
    }

    [Fact]
    public async Task DuplicateTrnInFile_ReturnsErrorAndCreatesNoPersons()
    {
        // Arrange
        var trn = GenerateTrn();
        var csv = string.Join(
            Environment.NewLine,
            Header,
            $"{trn},Smith,Alice,08/07/1965,",
            $"{trn},Jones,Bob,15/01/1970,");

        // Act
        var (exitCode, _, error) = await InvokeWithCsvAsync(csv);

        // Assert
        Assert.Equal(1, exitCode);
        Assert.Contains($"TRN {trn} appears more than once in the file.", error);
        await AssertPersonExistsAsync(trn, exists: false);
    }

    [Fact]
    public async Task PersonWithTrnAlreadyExists_OutputsExistingTrnAndImportsRemainingRows()
    {
        // Arrange
        var existingPerson = await TestData.CreatePersonAsync();
        var newTrn = GenerateTrn(existingPerson.Trn!);

        var csv = string.Join(
            Environment.NewLine,
            Header,
            $"{newTrn},Smith,Alice,08/07/1965,",
            $"{existingPerson.Trn},Jones,Bob,15/01/1970,");

        // Act
        var (exitCode, output, _) = await InvokeWithCsvAsync(csv);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Contains("TRN\tIdentical", output);
        Assert.Contains($"{existingPerson.Trn}\tNo", output);
        await AssertPersonExistsAsync(newTrn, exists: true);
    }

    private Command GetSubcommand() =>
        Commands.CreatePersonCommand(Configuration).Subcommands.Single(c => c.Name == "import-ewc-wales");

    private async Task<(int ExitCode, string Output, string Error)> InvokeAsync(string path)
    {
        var output = new StringWriter();
        var error = new StringWriter();

        var parseResult = GetSubcommand().Parse(["--file", path]);
        var exitCode = await parseResult.InvokeAsync(new InvocationConfiguration { Output = output, Error = error });

        return (exitCode, output.ToString(), error.ToString());
    }

    private async Task<(int ExitCode, string Output, string Error)> InvokeWithCsvAsync(string csv)
    {
        var path = Path.Combine(Path.GetTempPath(), $"ewc-wales-{Guid.NewGuid()}.csv");
        await File.WriteAllTextAsync(path, csv);

        try
        {
            return await InvokeAsync(path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private Task AssertPersonExistsAsync(string trn, bool exists) =>
        WithDbContextAsync(async dbContext =>
        {
            var found = await dbContext.Persons.AnyAsync(p => p.Trn == trn);
            Assert.Equal(exists, found);
        });

    private static string GenerateTrn(params string[] exclude)
    {
        string trn;
        do
        {
            trn = Random.Shared.Next(1000000, 8000000).ToString();
        }
        while (exclude.Contains(trn));

        return trn;
    }
}





