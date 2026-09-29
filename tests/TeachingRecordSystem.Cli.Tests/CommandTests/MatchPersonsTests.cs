using System.CommandLine;
using System.Globalization;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.TestCommon;

namespace TeachingRecordSystem.Cli.Tests.CommandTests;

public class MatchPersonsTests(IServiceProvider services) : CommandTestBase(services)
{
    [Fact]
    public async Task MatchPersons_InputFileDoesNotExist_ReturnsError()
    {
        // Arrange
        var inputFile = GetTempFileName();
        var outputFile = GetTempFileName();
        var error = new StringWriter();

        // Act
        var result = await InvokeAsync(inputFile, outputFile, error: error);

        // Assert
        Assert.Equal(1, result);
        Assert.Contains($"Input file was not found: {inputFile}", error.ToString());
        Assert.False(File.Exists(outputFile));
    }

    [Fact]
    public async Task MatchPersons_InputFileIsMissingRequiredColumns_ReturnsError()
    {
        // Arrange
        var inputFile = CreateInputFile("Surname,Middle names");
        var outputFile = GetTempFileName();
        var error = new StringWriter();

        // Act
        var result = await InvokeAsync(inputFile, outputFile, error: error);

        // Assert
        Assert.Equal(1, result);
        Assert.Contains("The input file is missing the following column(s): Forename, DOB.", error.ToString());
        Assert.False(File.Exists(outputFile));
    }

    [Fact]
    public async Task MatchPersons_InputFileIsEmpty_ReturnsError()
    {
        // Arrange
        var inputFile = CreateInputFile();
        var outputFile = GetTempFileName();
        var error = new StringWriter();

        // Act
        var result = await InvokeAsync(inputFile, outputFile, error: error);

        // Assert
        Assert.Equal(1, result);
        Assert.Contains("The input file is empty.", error.ToString());
        Assert.False(File.Exists(outputFile));
    }

    [Fact]
    public async Task MatchPersons_RowIsMissingRequiredValues_ReturnsErrorForEveryOffendingRow()
    {
        // Arrange
        var inputFile = CreateInputFile(
            "Surname,Forename,Middle names,Previous surname,DOB",
            ",Joe,,,1/2/1990",
            "Bloggs,,,,1/2/1990",
            "Bloggs,Joe,,,",
            "Bloggs,Joe,,,1/2/1990");
        var outputFile = GetTempFileName();
        var error = new StringWriter();

        // Act
        var result = await InvokeAsync(inputFile, outputFile, error: error);

        // Assert
        Assert.Equal(1, result);
        Assert.Contains("Row 2: 'Surname' is missing.", error.ToString());
        Assert.Contains("Row 3: 'Forename' is missing.", error.ToString());
        Assert.Contains("Row 4: 'DOB' is missing.", error.ToString());
        Assert.False(File.Exists(outputFile));
    }

    [Fact]
    public async Task MatchPersons_RowHasDateOfBirthInWrongFormat_ReturnsError()
    {
        // Arrange
        var inputFile = CreateInputFile(
            "Surname,Forename,Middle names,Previous surname,DOB",
            "Bloggs,Joe,,,1990-02-01");
        var outputFile = GetTempFileName();
        var error = new StringWriter();

        // Act
        var result = await InvokeAsync(inputFile, outputFile, error: error);

        // Assert
        Assert.Equal(1, result);
        Assert.Contains("Row 2: 'DOB' is not in the d/M/yyyy format.", error.ToString());
        Assert.False(File.Exists(outputFile));
    }

    [Fact]
    public async Task MatchPersons_InputFileHasOnlyRequiredColumns_MatchesRows()
    {
        // Arrange
        var person = await CreatePersonAsync();

        var inputFile = CreateInputFile(
            "Surname,Forename,DOB",
            $"{person.LastName},{person.FirstName},{FormatDateOfBirth(person.DateOfBirth!.Value)}");
        var outputFile = GetTempFileName();

        // Act
        var result = await InvokeAsync(inputFile, outputFile);

        // Assert
        Assert.Equal(0, result);
        Assert.Equal([person.Trn!], await ReadOutputFileAsync(outputFile));
    }

    [Fact]
    public async Task MatchPersons_RowMatchesASingleRecord_WritesThatRecordsTrn()
    {
        // Arrange
        var person = await CreatePersonAsync();

        var inputFile = CreateInputFile(
            "Surname,Forename,Middle names,Previous surname,DOB",
            $"{person.LastName},{person.FirstName},{person.MiddleName},,{FormatDateOfBirth(person.DateOfBirth!.Value)}");
        var outputFile = GetTempFileName();
        var output = new StringWriter();

        // Act
        var result = await InvokeAsync(inputFile, outputFile, output);

        // Assert
        Assert.Equal(0, result);
        Assert.Equal([person.Trn!], await ReadOutputFileAsync(outputFile));
        Assert.Contains("Matched 1 of 1 rows.", output.ToString());
    }

    [Fact]
    public async Task MatchPersons_RowMatchesOnPreviousSurname_WritesThatRecordsTrn()
    {
        // Arrange
        var person = await CreatePersonAsync();
        var previousLastName = TestData.GenerateChangedLastName(person.LastName);

        var inputFile = CreateInputFile(
            "Surname,Forename,Middle names,Previous surname,DOB",
            $"{previousLastName},{person.FirstName},{person.MiddleName},{person.LastName},{FormatDateOfBirth(person.DateOfBirth!.Value)}");
        var outputFile = GetTempFileName();

        // Act
        var result = await InvokeAsync(inputFile, outputFile);

        // Assert
        Assert.Equal(0, result);
        Assert.Equal([person.Trn!], await ReadOutputFileAsync(outputFile));
    }

    [Fact]
    public async Task MatchPersons_RowMatchesMoreThanOneRecord_WritesAnEmptyTrn()
    {
        // Arrange
        var firstName = TestData.GenerateFirstName();
        var lastName = TestData.GenerateLastName();
        var dateOfBirth = TestData.GenerateDateOfBirth();

        await CreatePersonAsync(firstName, lastName, dateOfBirth);
        await CreatePersonAsync(firstName, lastName, dateOfBirth);

        var inputFile = CreateInputFile(
            "Surname,Forename,Middle names,Previous surname,DOB",
            $"{lastName},{firstName},,,{FormatDateOfBirth(dateOfBirth)}");
        var outputFile = GetTempFileName();

        // Act
        var result = await InvokeAsync(inputFile, outputFile);

        // Assert
        Assert.Equal(0, result);
        Assert.Equal([""], await ReadOutputFileAsync(outputFile));
    }

    [Fact]
    public async Task MatchPersons_RowDoesNotMatchAnyRecords_WritesAnEmptyTrn()
    {
        // Arrange
        var inputFile = CreateInputFile(
            "Surname,Forename,Middle names,Previous surname,DOB",
            $"{TestData.GenerateLastName()},{TestData.GenerateFirstName()},,,{FormatDateOfBirth(TestData.GenerateDateOfBirth())}");
        var outputFile = GetTempFileName();
        var output = new StringWriter();

        // Act
        var result = await InvokeAsync(inputFile, outputFile, output);

        // Assert
        Assert.Equal(0, result);
        Assert.Equal([""], await ReadOutputFileAsync(outputFile));
        Assert.Contains("Matched 0 of 1 rows.", output.ToString());
    }

    [Fact]
    public async Task MatchPersons_WritesOneRowPerInputRowInTheSameOrder()
    {
        // Arrange
        var firstPerson = await CreatePersonAsync();
        var secondPerson = await CreatePersonAsync();

        var unknownFirstName = TestData.GenerateFirstName();
        var unknownLastName = TestData.GenerateLastName();
        var unknownDateOfBirth = TestData.GenerateDateOfBirth();

        var inputFile = CreateInputFile(
            "Surname,Forename,Middle names,Previous surname,DOB",
            $"{firstPerson.LastName},{firstPerson.FirstName},{firstPerson.MiddleName},,{FormatDateOfBirth(firstPerson.DateOfBirth!.Value)}",
            $"{unknownLastName},{unknownFirstName},,,{FormatDateOfBirth(unknownDateOfBirth)}",
            $"{secondPerson.LastName},{secondPerson.FirstName},{secondPerson.MiddleName},,{FormatDateOfBirth(secondPerson.DateOfBirth!.Value)}");
        var outputFile = GetTempFileName();

        // Act
        var result = await InvokeAsync(inputFile, outputFile);

        // Assert
        Assert.Equal(0, result);
        Assert.Equal([firstPerson.Trn!, "", secondPerson.Trn!], await ReadOutputFileAsync(outputFile));
    }

    [Fact]
    public async Task MatchPersons_DoesNotWriteAnythingToTheDatabase()
    {
        // Arrange
        var person = await CreatePersonAsync();

        var inputFile = CreateInputFile(
            "Surname,Forename,Middle names,Previous surname,DOB",
            $"{person.LastName},{person.FirstName},{person.MiddleName},,{FormatDateOfBirth(person.DateOfBirth!.Value)}");
        var outputFile = GetTempFileName();

        var trnRequestCount = await WithDbContextAsync(dbContext => dbContext.TrnRequestMetadata.CountAsync());
        var supportTaskCount = await WithDbContextAsync(dbContext => dbContext.SupportTasks.CountAsync());

        // Act
        var result = await InvokeAsync(inputFile, outputFile);

        // Assert
        Assert.Equal(0, result);
        Assert.Equal(trnRequestCount, await WithDbContextAsync(dbContext => dbContext.TrnRequestMetadata.CountAsync()));
        Assert.Equal(supportTaskCount, await WithDbContextAsync(dbContext => dbContext.SupportTasks.CountAsync()));
    }

    private Task<int> InvokeAsync(string inputFile, string outputFile, TextWriter? output = null, TextWriter? error = null)
    {
        var command = Commands.CreateMatchPersonsCommand(Configuration);
        var parseResult = command.Parse(["--in", inputFile, "--out", outputFile]);

        return parseResult.InvokeAsync(
            new InvocationConfiguration
            {
                Output = output ?? TextWriter.Null,
                Error = error ?? TextWriter.Null
            });
    }

    private Task<Person> CreatePersonAsync(string? firstName = null, string? lastName = null, DateOnly? dateOfBirth = null) =>
        TestData.CreatePersonAsync(p =>
        {
            p.WithFirstName(firstName ?? TestData.GenerateFirstName());
            p.WithMiddleName(TestData.GenerateMiddleName());
            p.WithLastName(lastName ?? TestData.GenerateLastName());
            p.WithDateOfBirth(dateOfBirth ?? TestData.GenerateDateOfBirth());
        });

    private static string FormatDateOfBirth(DateOnly dateOfBirth) =>
        dateOfBirth.ToString("d/M/yyyy", CultureInfo.InvariantCulture);

    private static string CreateInputFile(params string[] lines)
    {
        var fileName = GetTempFileName();
        File.WriteAllLines(fileName, lines);
        return fileName;
    }

    private static string GetTempFileName() =>
        Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csv");

    // The output file's header is dropped; what's returned is the TRN for each input row, in order.
    private static async Task<string[]> ReadOutputFileAsync(string fileName)
    {
        var lines = await File.ReadAllLinesAsync(fileName);
        Assert.Equal("TRN", lines[0]);
        return lines[1..];
    }
}
