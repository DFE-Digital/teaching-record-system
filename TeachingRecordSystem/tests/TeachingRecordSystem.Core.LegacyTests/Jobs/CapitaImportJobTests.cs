using System.Text;
using System.Text.Json;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Hangfire.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.PersonMatching;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection(nameof(DisableParallelization))]
public class CapitaImportJobTests(CapitaImportJobFixture Fixture) : IClassFixture<CapitaImportJobFixture>, IAsyncLifetime
{
    [Fact]
    public async Task Import_WhenTrnMissing_DoesNothing_ReportsFailureWithValidationError()
    {
        // Arrange
        (var reader, var rowData) = BuildSingleRowCsv(null, "1", "Lastname", "Firstname", "19991201", "AB123456D");

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 1,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            PersonId = (Guid?)null,
            Status = IntegrationTransactionRecordStatus.Failure,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
        }, record);
        Assert.Contains("Missing required field: TRN", record.FailureMessage);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("111111111")]
    [InlineData("111111a")]
    public async Task Import_WhenTrnInvalid_DoesNothing_ReportsFailureWithValidationError(string trn)
    {
        // Arrange
        (var reader, var rowData) = BuildSingleRowCsv(trn, "1", "Lastname", "Firstname", "19991201", "AB123456D");

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 1,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            PersonId = (Guid?)null,
            Status = IntegrationTransactionRecordStatus.Failure,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
        }, record);
        Assert.Contains("Validation failed on field: TRN", record.FailureMessage);
    }

    [Fact]
    public async Task Import_WhenNoMiddleName_CreatesPerson_ReportsSuccess()
    {
        // Arrange
        var newTrn = "9988776";
        var firstName = Faker.Name.First();
        var lastName = Faker.Name.Last();
        var gender = Gender.Female;
        var dob = new DateOnly(1981, 08, 20);
        var nino = Faker.Identification.UkNationalInsuranceNumber();

        (var reader, var rowData) = BuildSingleRowCsv(newTrn, gender, lastName, firstName, dob, nino);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var newPerson = await AssertSinglePersonAsync(newTrn);
        AssertHasProperties(new
        {
            Trn = newTrn,
            DateOfBirth = dob,
            Gender = gender,
            NationalInsuranceNumber = nino,
            FirstName = firstName,
            MiddleName = "",
            LastName = lastName,
            CreatedByTps = true
        }, newPerson);

        await AssertSingleEventAsync<LegacyEvents.PersonCreatedEvent>(newPerson.PersonId);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 1,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            PersonId = newPerson.PersonId,
            Status = IntegrationTransactionRecordStatus.Success,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
            FailureMessage = ""
        }, record);
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    public async Task Import_WhenNoMatches_CreatesPerson_ReportsSuccess(Gender gender)
    {
        // Arrange
        var newTrn = "9988776";
        var firstName = Faker.Name.First();
        var lastName = Faker.Name.Last();
        var dob = new DateOnly(1981, 08, 20);
        var nino = Faker.Identification.UkNationalInsuranceNumber();

        (var reader, var rowData) = BuildSingleRowCsv(newTrn, gender, lastName, firstName, dob, nino);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var newPerson = await AssertSinglePersonAsync(newTrn);
        AssertHasProperties(new
        {
            Trn = newTrn,
            DateOfBirth = dob,
            Gender = gender,
            NationalInsuranceNumber = nino,
            FirstName = firstName,
            LastName = lastName,
            CreatedByTps = true
        }, newPerson);

        await AssertSingleEventAsync<LegacyEvents.PersonCreatedEvent>(newPerson.PersonId);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 1,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            PersonId = newPerson.PersonId,
            Status = IntegrationTransactionRecordStatus.Success,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
            FailureMessage = ""
        }, record);
    }

    [Fact]
    public async Task Import_WhenNoData_DoesNothing_ReportsZeroCounts()
    {
        // Arrange
        (var reader, var rowData) = BuildEmptyCsv();

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "EmptyFile.csv");

        // Assert
        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 0,
            SuccessCount = 0,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "EmptyFile.csv"
        }, transaction);

        await AssertNoIntegrationTransactionRecordAsync(integrationTransactionId);
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    public async Task Import_WhenNinoInvalid_CreatesPerson_ReportsSuccessWithFailureMessage(Gender gender)
    {
        // Arrange
        var newTrn = "9988776";
        var firstName = Faker.Name.First();
        var lastName = Faker.Name.Last();
        var dob = new DateOnly(1981, 08, 20);
        var nino = "JL5618AAB";

        (var reader, var rowData) = BuildSingleRowCsv(newTrn, gender, lastName, firstName, dob, nino);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var newPerson = await AssertSinglePersonAsync(newTrn);
        AssertHasProperties(new
        {
            Trn = newTrn,
            DateOfBirth = dob,
            Gender = gender,
            NationalInsuranceNumber = (string?)null,
            FirstName = firstName,
            LastName = lastName,
            CreatedByTps = true
        }, newPerson);

        await AssertSingleEventAsync<LegacyEvents.PersonCreatedEvent>(newPerson.PersonId);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 1,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            PersonId = newPerson.PersonId,
            Status = IntegrationTransactionRecordStatus.Success,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
        }, record);
        Assert.Contains("Invalid National Insurance number", record.FailureMessage);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("111111111")]
    [InlineData("111111a")]
    [InlineData("AB123456X")]
    [InlineData("AB12345D")]
    [InlineData("ABC12345D")]
    public async Task Import_WhenNinoInvalid_CreatesPerson_RecordsSuccessWithFailureMessage(string ni)
    {
        // Arrange
        var newTrn = "1234567";
        (var reader, var rowData) = BuildSingleRowCsv(newTrn, "1", "Lastname", "Firstname", "19991201", ni);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var newPerson = await AssertSinglePersonAsync(newTrn);

        await AssertSingleEventAsync<LegacyEvents.PersonCreatedEvent>(newPerson.PersonId);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 1,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            Status = IntegrationTransactionRecordStatus.Success,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
        }, record);
        Assert.NotNull(record.PersonId);
        Assert.Contains("Invalid National Insurance number", record.FailureMessage);
    }

    [Fact]
    public async Task Import_WhenMiddleNameCombinedWithFirstName_CreatesPerson_ReportsSuccess()
    {
        // Arrange
        var newTrn = "9988776";
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var gender = Gender.Male;
        var dob = new DateOnly(1981, 08, 20);
        var nino = Faker.Identification.UkNationalInsuranceNumber();

        (var reader, var rowData) = BuildSingleRowCsv(newTrn, gender, lastName, $"{firstName} {middleName}", dob, nino);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var newPerson = await AssertSinglePersonAsync(newTrn);
        AssertHasProperties(new
        {
            Trn = newTrn,
            DateOfBirth = dob,
            Gender = gender,
            NationalInsuranceNumber = nino,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            CreatedByTps = true
        }, newPerson);

        await AssertSingleEventAsync<LegacyEvents.PersonCreatedEvent>(newPerson.PersonId);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 1,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            PersonId = newPerson.PersonId,
            Status = IntegrationTransactionRecordStatus.Success,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
            FailureMessage = ""
        }, record);
    }

    [Fact]
    public async Task Import_WhenGenderMissing_DoesNothing_ReportsFailureWithValidationError()
    {
        // Arrange
        (var reader, var rowData) = BuildSingleRowCsv("1234567", (string?)null, "Lastname", "Firstname", "19991201", "AB123456D");

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 1,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            PersonId = (Guid?)null,
            Status = IntegrationTransactionRecordStatus.Failure,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
        }, record);
        Assert.Contains("Missing required field: Gender", record.FailureMessage);
    }

    [Theory]
    [InlineData("3")]
    [InlineData("0")]
    [InlineData("-13")]
    public async Task Import_WhenGenderInvalid_DoesNothing_ReportsFailureWithValidationMessage(string gender)
    {
        // Arrange
        (var reader, var rowData) = BuildSingleRowCsv("1234567", gender, "Lastname", "Firstname", "19991201", "AB123456D");

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 1,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            PersonId = (Guid?)null,
            Status = IntegrationTransactionRecordStatus.Failure,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
        }, record);
        Assert.Contains($"Invalid Gender: {gender}", record.FailureMessage);
    }

    [Fact]
    public async Task Import_WhenDateOfDeathProvidedForNewPerson_CreatesDeactivatedPerson_ReportsSuccess()
    {
        // Arrange
        var trn = "1234567";
        var firstName = Faker.Name.First();
        var lastName = Faker.Name.Last();
        var gender = Gender.Female;
        var dob = new DateOnly(1981, 08, 20);
        var nino = Faker.Identification.UkNationalInsuranceNumber();
        var dateOfDeath = DateOnly.FromDateTime(Clock.UtcNow.AddDays(-1));

        (var reader, var rowData) = BuildSingleRowCsv(trn, gender, lastName, firstName, dob, nino, dateOfDeath);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        await AssertNoPersonAsync(trn);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 1,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            Status = IntegrationTransactionRecordStatus.Success,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
            FailureMessage = ""
        }, record);
        Assert.NotNull(record.PersonId);
    }

    [Fact]
    public async Task Import_WhenDateOfDeathProvidedForExistingPerson_DeactivatesPerson_ReportsSuccess()
    {
        // Arrange
        var existingPerson = await TestData.CreatePersonAsync(p => p
            .WithNationalInsuranceNumber());

        var newFirstName = Faker.Name.First();
        var newGender = Gender.Female;
        var newDob = new DateOnly(1981, 08, 20);
        var dateOfDeath = DateOnly.FromDateTime(Clock.UtcNow.AddDays(-1));

        (var reader, var rowData) = BuildSingleRowCsv(existingPerson.Trn, newGender, existingPerson.LastName, newFirstName, existingPerson.DateOfBirth, existingPerson.NationalInsuranceNumber, dateOfDeath);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        await AssertNoPersonAsync(existingPerson.Trn);

        var evt = await AssertSingleEventAsync<PersonStatusUpdatedEvent>(existingPerson.PersonId);
        Assert.Equal("Date of death received from capita import", evt.Reason);
        Assert.Equal(PersonStatus.Active, evt.OldStatus);
        Assert.Equal(PersonStatus.Deactivated, evt.Status);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            FailureCount = 0,
            SuccessCount = 1,
            WarningCount = 0,
            DuplicateCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            Status = IntegrationTransactionRecordStatus.Success,
            PersonId = existingPerson.PersonId,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
            FailureMessage = ""
        }, record);
    }

    [Theory]
    [InlineData("01022025")] //invalid month
    [InlineData("19900931")] //invalid day
    public async Task Import_WhenDateOfDeathInvalid_DoesNothing_ReportsFailureWithValidationMessage(string dateOfDeath)
    {
        // Arrange
        var dateOfBirth = Clock.UtcNow.AddDays(-25).ToString("yyyyMMdd");
        (var reader, var rowData) = BuildSingleRowCsv("1234567", "1", "Lastname", "Firstname", dateOfBirth, "AB123456D", dateOfDeath);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 1,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            PersonId = (Guid?)null,
            Status = IntegrationTransactionRecordStatus.Failure,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
        }, record);
        Assert.Contains("Validation Failed: Invalid Date of death", record.FailureMessage);
    }

    [Fact]
    public async Task Import_WhenDateOfDeathInFuture_DoesNothing_ReportsFailureWithValidationMessage()
    {
        // Arrange
        var dateOfBirth = Clock.UtcNow.AddDays(-25).ToString("yyyyMMdd");
        var dateOfDeath = Clock.UtcNow.AddDays(25).ToString("yyyyMMdd");

        (var reader, var rowData) = BuildSingleRowCsv("1234567", "1", "Lastname", "Firstname", dateOfBirth, "AB123456D", dateOfDeath);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 1,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            PersonId = (Guid?)null,
            Status = IntegrationTransactionRecordStatus.Failure,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
        }, record);
        Assert.Contains("Validation Failed: Date of death cannot be in the future", record.FailureMessage);
    }

    [Fact]
    public async Task Import_WhenDateOfBirthMissing_DoesNothing_ReportsFailureWithValidationMessage()
    {
        // Arrange
        (var reader, var rowData) = BuildSingleRowCsv("1234567", "1", "Lastname", "Firstname", null, "AB123456D");

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 1,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            PersonId = (Guid?)null,
            Status = IntegrationTransactionRecordStatus.Failure,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
        }, record);
        Assert.Contains("Missing required field: Date of birth", record.FailureMessage);
    }

    [Fact]
    public async Task Import_WhenDateOfBirthInFuture_DoesNothing_ReportsFailureWithValidationMessage()
    {
        // Arrange
        var dateOfBirth = Clock.UtcNow.AddDays(25).ToString("yyyyMMdd");

        (var reader, var rowData) = BuildSingleRowCsv("1234567", "1", "Lastname", "Firstname", dateOfBirth, "AB123456D");

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 1,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            PersonId = (Guid?)null,
            Status = IntegrationTransactionRecordStatus.Failure,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
        }, record);
        Assert.Contains("Validation Failed: Date of Birth cannot be in the future", record.FailureMessage);
    }

    [Theory]
    [InlineData("08011972")] //invalid month
    [InlineData("19900231")] //invalid day
    public async Task Import_WhenDateOfBirthIncorrectFormat_DoesNothing_ReportsFailureWithValidationMessage(string dateOfBirth)
    {
        // Arrange
        (var reader, var rowData) = BuildSingleRowCsv("1234567", "1", "Lastname", "Firstname", dateOfBirth, "AB123456D");

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 1,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            PersonId = (Guid?)null,
            Status = IntegrationTransactionRecordStatus.Failure,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
        }, record);
        Assert.Contains("Validation Failed: Invalid Date of Birth", record.FailureMessage);
    }

    [Fact]
    public async Task Import_MatchesExistingPersonWithNoNinoOnTrnAndGenderAndLastNameAndDob_UpdatesExistingPersonNino_ReportsSuccess()
    {
        // Arrange
        var existingPerson = await TestData.CreatePersonAsync(p => p
            .WithGender(Gender.Male)
            .WithDateOfBirth(new DateOnly(1972, 01, 01)));

        var newFirstName = Faker.Name.First();
        var newNino = Faker.Identification.UkNationalInsuranceNumber();
        (var reader, var rowData) = BuildSingleRowCsv(existingPerson.Trn, existingPerson.Gender, existingPerson.LastName, newFirstName, existingPerson.DateOfBirth, newNino);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var samePerson = await AssertSinglePersonAsync(existingPerson.Trn);
        AssertHasProperties(new
        {
            NationalInsuranceNumber = newNino,
            LastName = existingPerson.LastName,
            FirstName = existingPerson.FirstName
        }, samePerson);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 1,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            Status = IntegrationTransactionRecordStatus.Success,
            PersonId = samePerson.PersonId,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
            FailureMessage = ""
        }, record);

        await AssertNoSupportTaskAsync(SupportTaskType.TeacherPensionsPotentialDuplicate, samePerson.PersonId);
    }

    [Fact]
    public async Task Import_MatchesExistingPersonWithNoNinoOnTrnAndGenderAndDobWithDifferentLastNameAndFirstName_UpdatesExistingPersonNino_ReportsWarningWithMessageAboutAttemptedUpdateToLastName()
    {
        // Arrange
        var existingPerson = await TestData.CreatePersonAsync(p => p
            .WithGender(Gender.Male)
            .WithDateOfBirth(new DateOnly(1972, 01, 01)));

        var newFirstName = Faker.Name.First();
        var newLastName = Faker.Name.Last();
        var newNino = Faker.Identification.UkNationalInsuranceNumber();
        (var reader, var rowData) = BuildSingleRowCsv(existingPerson.Trn, existingPerson.Gender, newLastName, newFirstName, existingPerson.DateOfBirth, newNino);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var samePerson = await AssertSinglePersonAsync(existingPerson.Trn);
        AssertHasProperties(new
        {
            NationalInsuranceNumber = newNino,
            LastName = existingPerson.LastName,
            FirstName = existingPerson.FirstName
        }, samePerson);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 0,
            WarningCount = 1,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            Status = IntegrationTransactionRecordStatus.Warning,
            PersonId = samePerson.PersonId,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
        }, record);
        Assert.Contains($"Warning: Attempted to update lastname from {samePerson.LastName} to {newLastName}", record.FailureMessage);

        await AssertNoSupportTaskAsync(SupportTaskType.TeacherPensionsPotentialDuplicate, samePerson.PersonId);
    }

    [Fact]
    public async Task Import_MatchesExistingPersonWithNoNinoOnTrnAndGender_WithDifferentLastNameAndFirstName_UpdatesExistingPersonNino_ReportsWarningWithMessageAboutAttemptedUpdateToLastName()
    {
        // Arrange
        var existingPerson = await TestData.CreatePersonAsync(p => p
            .WithGender(Gender.Male)
            .WithDateOfBirth(new DateOnly(1972, 01, 01)));

        var newFirstName = Faker.Name.First();
        var newLastName = Faker.Name.Last();
        var newNino = Faker.Identification.UkNationalInsuranceNumber();
        var newDob = new DateOnly(1980, 02, 02);
        (var reader, var rowData) = BuildSingleRowCsv(existingPerson.Trn, existingPerson.Gender, newLastName, newFirstName, newDob, newNino);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var samePerson = await AssertSinglePersonAsync(existingPerson.Trn);
        AssertHasProperties(new
        {
            NationalInsuranceNumber = newNino,
            LastName = existingPerson.LastName,
            FirstName = existingPerson.FirstName
        }, samePerson);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 0,
            WarningCount = 1,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            Status = IntegrationTransactionRecordStatus.Warning,
            PersonId = samePerson.PersonId,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
        }, record);
        Assert.Contains($"Warning: Attempted to update lastname from {samePerson.LastName} to {newLastName}", record.FailureMessage);

        await AssertNoSupportTaskAsync(SupportTaskType.TeacherPensionsPotentialDuplicate, samePerson.PersonId);
    }

    [Fact]
    public async Task Import_MatchesExistingPersonWithNoNinoOnTrn_WithDifferentLastName_UpdatesExistingPersonNino_ReportsWarningWithMessageAboutAttemptedUpdateToLastName()
    {
        // Arrange
        var existingPerson = await TestData.CreatePersonAsync(p => p
            .WithGender(Gender.Male)
            .WithDateOfBirth(new DateOnly(1972, 01, 01)));

        var newLastName = Faker.Name.Last();
        var newNino = Faker.Identification.UkNationalInsuranceNumber();
        (var reader, var rowData) = BuildSingleRowCsv(existingPerson.Trn, existingPerson.Gender, newLastName, existingPerson.FirstName, existingPerson.DateOfBirth, newNino);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var samePerson = await AssertSinglePersonAsync(existingPerson.Trn);
        AssertHasProperties(new
        {
            NationalInsuranceNumber = newNino,
            LastName = existingPerson.LastName,
            FirstName = existingPerson.FirstName
        }, samePerson);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 0,
            WarningCount = 1,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            Status = IntegrationTransactionRecordStatus.Warning,
            PersonId = samePerson.PersonId,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
        }, record);
        Assert.Contains($"Warning: Attempted to update lastname from {samePerson.LastName} to {newLastName}", record.FailureMessage);

        await AssertNoSupportTaskAsync(SupportTaskType.TeacherPensionsPotentialDuplicate, samePerson.PersonId);
    }

    [Fact]
    public async Task Import_MatchesExistingPersonWithNoNinoOnFirstNameAndLastNameAndDob_CreatesPersonAndCreatesPotentialDuplicateSupportTask_ReportsDuplicate()
    {
        // Arrange
        var existingPerson = await TestData.CreatePersonAsync(p => p
            .WithGender(Gender.Male)
            .WithDateOfBirth(new DateOnly(1981, 08, 20)));

        var newTrn = "1234567";
        var newNino = Faker.Identification.UkNationalInsuranceNumber();
        (var reader, var rowData) = BuildSingleRowCsv(newTrn, existingPerson.Gender, existingPerson.LastName, existingPerson.FirstName, existingPerson.DateOfBirth, newNino);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var newPerson = await AssertSinglePersonAsync(newTrn);
        AssertHasProperties(new
        {
            NationalInsuranceNumber = newNino,
            LastName = existingPerson.LastName,
            FirstName = existingPerson.FirstName
        }, newPerson);

        await AssertSingleEventAsync<LegacyEvents.PersonCreatedEvent>(newPerson.PersonId);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 1,
            WarningCount = 0,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            Status = IntegrationTransactionRecordStatus.Success,
            newPerson.PersonId,
            Duplicate = true,
            HasActiveAlert = (bool?)null,
            FailureMessage = ""
        }, record);

        await AssertNoSupportTaskAsync(SupportTaskType.TeacherPensionsPotentialDuplicate, existingPerson.PersonId);
        var task = await AssertSingleSupportTaskAsync(SupportTaskType.TeacherPensionsPotentialDuplicate, newPerson.PersonId);

        var trnRequest = await AssertSingleTrnRequestMetadataAsync(task.TrnRequestId);
        Assert.True(trnRequest.PotentialDuplicate);
        Assert.Contains(trnRequest.Matches?.MatchedPersons!, p => p.PersonId == existingPerson.PersonId);
    }

    [Fact]
    public async Task Import_MatchesExistingPersonWithNinoOnFirstNameAndLastNameAndDob_CreatesPersonAndCreatesPotentialDuplicateSupportTask_ReportsDuplicate()
    {
        // Arrange
        var existingPerson = await TestData.CreatePersonAsync(p => p
            .WithGender(Gender.Male)
            .WithNationalInsuranceNumber()
            .WithDateOfBirth(new DateOnly(1981, 08, 20)));

        var newTrn = "1234567";
        (var reader, var rowData) = BuildSingleRowCsv(newTrn, existingPerson.Gender, existingPerson.LastName, existingPerson.FirstName, existingPerson.DateOfBirth, null);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var newPerson = await AssertSinglePersonAsync(newTrn);
        AssertHasProperties(new
        {
            NationalInsuranceNumber = (string?)null,
            LastName = existingPerson.LastName,
            FirstName = existingPerson.FirstName
        }, newPerson);

        await AssertSingleEventAsync<LegacyEvents.PersonCreatedEvent>(newPerson.PersonId);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 1,
            WarningCount = 0,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            Status = IntegrationTransactionRecordStatus.Success,
            newPerson.PersonId,
            Duplicate = true,
            HasActiveAlert = (bool?)null,
            FailureMessage = ""
        }, record);

        await AssertNoSupportTaskAsync(SupportTaskType.TeacherPensionsPotentialDuplicate, existingPerson.PersonId);
        var task = await AssertSingleSupportTaskAsync(SupportTaskType.TeacherPensionsPotentialDuplicate, newPerson.PersonId);

        var trnRequest = await AssertSingleTrnRequestMetadataAsync(task.TrnRequestId);
        Assert.True(trnRequest.PotentialDuplicate);
        Assert.Contains(trnRequest.Matches?.MatchedPersons!, p => p.PersonId == existingPerson.PersonId);
    }

    [Fact]
    public async Task Import_MatchesExistingPersonOnTrnAndNinoAndDob_WithDifferentFirstNameAndLastName_DoesNothing_ReportsWarningWithMessageAboutAttemptToUpdateFirstPersonWithSecondPersonsDetails()
    {
        // Arrange
        var existingPerson = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());
        (var reader, var rowData) = BuildSingleRowCsv(existingPerson.Trn, Gender.Male, "Lastname", "Firstname", existingPerson.DateOfBirth, existingPerson.NationalInsuranceNumber);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var samePerson = await AssertSinglePersonAsync(existingPerson.Trn);
        AssertHasProperties(new
        {
            NationalInsuranceNumber = existingPerson.NationalInsuranceNumber,
            LastName = existingPerson.LastName,
            FirstName = existingPerson.FirstName
        }, samePerson);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 0,
            WarningCount = 1,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            Status = IntegrationTransactionRecordStatus.Warning,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
        }, record);
        Assert.NotNull(record.PersonId);
        Assert.Contains($"Attempted to update lastname from {existingPerson.LastName} to Lastname", record.FailureMessage);
    }

    [Fact]
    public async Task Import_MatchesExistingPersonOnTrnAndAnotherPersonOnFirstNameAndLastNameAndDob_DoesNothing_ReportsWarningWithMessageAboutAttemptToUpdateFirstPersonWithSecondPersonsDetails()
    {
        // Arrange
        var existingPerson = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());
        var otherPerson = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());

        (var reader, var rowData) = BuildSingleRowCsv(otherPerson.Trn, Gender.Male, existingPerson.LastName, existingPerson.FirstName, existingPerson.DateOfBirth, null);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var samePerson = await AssertSinglePersonAsync(existingPerson.Trn);
        AssertHasProperties(new
        {
            NationalInsuranceNumber = existingPerson.NationalInsuranceNumber,
            LastName = existingPerson.LastName,
            FirstName = existingPerson.FirstName
        }, samePerson);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 0,
            WarningCount = 1,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            Status = IntegrationTransactionRecordStatus.Warning,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
        }, record);
        Assert.NotNull(record.PersonId);
        Assert.Contains($"Attempted to update lastname from {otherPerson.LastName} to {existingPerson.LastName}", record.FailureMessage);
    }

    [Fact]
    public async Task Import_MatchesExistingPersonOnNinoAndGender_CreatesPersonAndCreatesPotentialDuplicateSupportTask_ReportsDuplicate()
    {
        // Arrange
        var existingPerson = await TestData.CreatePersonAsync(p => p
            .WithGender(Gender.Male)
            .WithNationalInsuranceNumber());

        var newTrn = "1234567";
        var newFirstName = Faker.Name.First();
        var newLastName = Faker.Name.Last();
        var newDob = new DateOnly(1981, 08, 20);
        (var reader, var rowData) = BuildSingleRowCsv(newTrn, existingPerson.Gender, newLastName, newFirstName, newDob, existingPerson.NationalInsuranceNumber);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var newPerson = await AssertSinglePersonAsync(newTrn);
        AssertHasProperties(new
        {
            NationalInsuranceNumber = existingPerson.NationalInsuranceNumber,
            LastName = newLastName,
            FirstName = newFirstName
        }, newPerson);

        await AssertSingleEventAsync<LegacyEvents.PersonCreatedEvent>(newPerson.PersonId);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 1,
            WarningCount = 0,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            Status = IntegrationTransactionRecordStatus.Success,
            PersonId = newPerson.PersonId,
            Duplicate = true,
            HasActiveAlert = (bool?)null,
            FailureMessage = ""
        }, record);

        await AssertNoSupportTaskAsync(SupportTaskType.TeacherPensionsPotentialDuplicate, existingPerson.PersonId);
        var task = await AssertSingleSupportTaskAsync(SupportTaskType.TeacherPensionsPotentialDuplicate, newPerson.PersonId);

        var trnRequest = await AssertSingleTrnRequestMetadataAsync(task.TrnRequestId);
        Assert.True(trnRequest.PotentialDuplicate);
        Assert.Contains(trnRequest.Matches?.MatchedPersons!, p => p.PersonId == existingPerson.PersonId);
    }

    [Fact]
    public async Task Import_MatchesExistingPersonOnNinoAndDobAndGender_CreatesPersonAndCreatesPotentialDuplicateSupportTask_ReportsDuplicate()
    {
        // Arrange
        var existingPerson = await TestData.CreatePersonAsync(p => p
            .WithGender(Gender.Male)
            .WithNationalInsuranceNumber()
            .WithDateOfBirth(new DateOnly(1972, 01, 01)));

        var newTrn = "8000038";
        var newFirstName = Faker.Name.First();
        var newLastName = Faker.Name.Last();
        (var reader, var rowData) = BuildSingleRowCsv(newTrn, existingPerson.Gender, newLastName, newFirstName, existingPerson.DateOfBirth, existingPerson.NationalInsuranceNumber);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var newPerson = await AssertSinglePersonAsync(newTrn);
        AssertHasProperties(new
        {
            NationalInsuranceNumber = existingPerson.NationalInsuranceNumber,
            DateOfBirth = existingPerson.DateOfBirth,
            LastName = newLastName,
            FirstName = newFirstName
        }, newPerson);

        await AssertSingleEventAsync<LegacyEvents.PersonCreatedEvent>(newPerson.PersonId);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 1,
            WarningCount = 0,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            Status = IntegrationTransactionRecordStatus.Success,
            PersonId = newPerson.PersonId,
            Duplicate = true,
            HasActiveAlert = (bool?)null,
            FailureMessage = ""
        }, record);

        await AssertNoSupportTaskAsync(SupportTaskType.TeacherPensionsPotentialDuplicate, existingPerson.PersonId);
        var task = await AssertSingleSupportTaskAsync(SupportTaskType.TeacherPensionsPotentialDuplicate, newPerson.PersonId);

        var trnRequest = await AssertSingleTrnRequestMetadataAsync(task.TrnRequestId);
        Assert.True(trnRequest.PotentialDuplicate);
        // Question: shouldn't this contain existingPerson.PersonId?
        Assert.Empty(trnRequest.Matches?.MatchedPersons!);
    }

    [Fact]
    public async Task Import_MatchesExistingPersonOnNinoAndDob_CreatesPersonAndCreatesPotentialDuplicateSupportTask_ReportsDuplicate()
    {
        // Arrange
        var existingPerson = await TestData.CreatePersonAsync(p => p
            .WithGender(Gender.Male)
            .WithNationalInsuranceNumber()
            .WithDateOfBirth(new DateOnly(1972, 01, 01)));

        var newTrn = "8000038";
        var newFirstName = Faker.Name.First();
        var newLastName = Faker.Name.Last();
        var newGender = Gender.Female;
        (var reader, var rowData) = BuildSingleRowCsv(newTrn, newGender, newLastName, newFirstName, existingPerson.DateOfBirth, existingPerson.NationalInsuranceNumber);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var newPerson = await AssertSinglePersonAsync(newTrn);
        Assert.Equal(existingPerson.NationalInsuranceNumber, newPerson.NationalInsuranceNumber);
        Assert.Equal(existingPerson.DateOfBirth, newPerson.DateOfBirth);
        Assert.Equal(newLastName, newPerson.LastName);
        Assert.Equal(newFirstName, newPerson.FirstName);

        await AssertSingleEventAsync<LegacyEvents.PersonCreatedEvent>(newPerson.PersonId);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 1,
            WarningCount = 0,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            Status = IntegrationTransactionRecordStatus.Success,
            PersonId = newPerson.PersonId,
            Duplicate = true,
            HasActiveAlert = (bool?)null,
            FailureMessage = ""
        }, record);

        await AssertNoSupportTaskAsync(SupportTaskType.TeacherPensionsPotentialDuplicate, existingPerson.PersonId);
        var task = await AssertSingleSupportTaskAsync(SupportTaskType.TeacherPensionsPotentialDuplicate, newPerson.PersonId);

        var trnRequest = await AssertSingleTrnRequestMetadataAsync(task.TrnRequestId);
        Assert.True(trnRequest.PotentialDuplicate);
        // Question: shouldn't this contain existingPerson.PersonId?
        Assert.Empty(trnRequest.Matches?.MatchedPersons!);
    }

    [Fact]
    public async Task Import_MatchesExistingPersonOnNino_CreatesPersonAndCreatesPotentialDuplicateSupportTask_ReportsDuplicate()
    {
        // Arrange
        var existingPerson = await TestData.CreatePersonAsync(p => p
            .WithGender(Gender.Male)
            .WithNationalInsuranceNumber());

        var newTrn = "1234567";
        var newFirstName = Faker.Name.First();
        var newLastName = Faker.Name.Last();
        var newDob = new DateOnly(1981, 08, 20);
        var newGender = Gender.Female;
        (var reader, var rowData) = BuildSingleRowCsv(newTrn, newGender, newLastName, newFirstName, newDob, existingPerson.NationalInsuranceNumber);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var newPerson = await AssertSinglePersonAsync(newTrn);
        Assert.Equal(existingPerson.NationalInsuranceNumber, newPerson.NationalInsuranceNumber);
        Assert.Equal(newLastName, newPerson.LastName);
        Assert.Equal(newFirstName, newPerson.FirstName);

        await AssertSingleEventAsync<LegacyEvents.PersonCreatedEvent>(newPerson.PersonId);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 1,
            WarningCount = 0,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            Status = IntegrationTransactionRecordStatus.Success,
            PersonId = newPerson.PersonId,
            Duplicate = true,
            HasActiveAlert = (bool?)null,
            FailureMessage = ""
        }, record);

        await AssertNoSupportTaskAsync(SupportTaskType.TeacherPensionsPotentialDuplicate, existingPerson.PersonId);
        var task = await AssertSingleSupportTaskAsync(SupportTaskType.TeacherPensionsPotentialDuplicate, newPerson.PersonId);

        var trnRequest = await AssertSingleTrnRequestMetadataAsync(task.TrnRequestId);
        Assert.True(trnRequest.PotentialDuplicate);
        Assert.Contains(trnRequest.Matches?.MatchedPersons!, p => p.PersonId == existingPerson.PersonId);
    }

    [Fact]
    public async Task Import_MatchesExistingPersonOnLastNameAndDob_CreatesPerson_ReportsSuccess()
    {
        // Arrange
        var existingPerson = await TestData.CreatePersonAsync(p => p
            .WithGender(Gender.Male)
            .WithDateOfBirth(new DateOnly(1981, 08, 20)));

        var newTrn = "1234567";
        var newFirstName = Faker.Name.First();
        var newNino = Faker.Identification.UkNationalInsuranceNumber();
        (var reader, var rowData) = BuildSingleRowCsv(newTrn, existingPerson.Gender, existingPerson.LastName, newFirstName, existingPerson.DateOfBirth, newNino);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var newPerson = await AssertSinglePersonAsync(newTrn);
        Assert.Equal(newNino, newPerson.NationalInsuranceNumber);
        Assert.Equal(existingPerson.LastName, newPerson.LastName);
        Assert.Equal(newFirstName, newPerson.FirstName);

        await AssertSingleEventAsync<LegacyEvents.PersonCreatedEvent>(newPerson.PersonId);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 1,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            Status = IntegrationTransactionRecordStatus.Success,
            newPerson.PersonId,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
            FailureMessage = ""
        }, record);

        await AssertNoSupportTaskAsync(SupportTaskType.TeacherPensionsPotentialDuplicate, newPerson.PersonId);
    }

    [Fact]
    public async Task Import_MatchesExistingPersonOnDob_CreatesPerson_RecordsSuccess()
    {
        // Arrange
        var existingPerson = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());

        var newTrn = "1234567";
        (var reader, var rowData) = BuildSingleRowCsv("1234567", Gender.Male, "test", "Test", existingPerson.DateOfBirth, Faker.Identification.UkNationalInsuranceNumber());

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var newPerson = await AssertSinglePersonAsync(newTrn);

        await AssertSingleEventAsync<LegacyEvents.PersonCreatedEvent>(newPerson.PersonId);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 1,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            Status = IntegrationTransactionRecordStatus.Success,
            PersonId = newPerson.PersonId,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
            FailureMessage = ""
        }, record);
    }

    [Fact]
    public async Task Import_MatchesDeactivatedPersonOnTrn_DoesNothing_ReportsFailure()
    {
        // Arrange
        var existingPerson = await TestData.CreatePersonAsync(p => p
            .WithGender(Gender.Male)
            .WithNationalInsuranceNumber()
            .WithDateOfBirth(new DateOnly(1972, 01, 01)));
        await DeactivatePerson(existingPerson.Trn);

        (var reader, var rowData) = BuildSingleRowCsv(existingPerson.Trn, existingPerson.Gender, existingPerson.LastName, existingPerson.FirstName, existingPerson.DateOfBirth, existingPerson.NationalInsuranceNumber);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 1,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            Status = IntegrationTransactionRecordStatus.Failure,
            PersonId = (Guid?)null,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
        }, record);
        Assert.Contains($"de-activated record exists for trn {existingPerson.Trn}", record.FailureMessage);
    }

    [Fact]
    public async Task Import_DoesNotMatchExistingPerson_CreatesPerson_ReportsSuccess()
    {
        // Arrange
        var existingPerson = await TestData.CreatePersonAsync();
        var newTrn = "1234567";
        (var reader, var rowData) = BuildSingleRowCsv(newTrn, "1", "Lastname", "Firstname", "19990101", "AB123456D");

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var newPerson = await AssertSinglePersonAsync(newTrn);

        await AssertSingleEventAsync<LegacyEvents.PersonCreatedEvent>(newPerson.PersonId);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 1,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            Status = IntegrationTransactionRecordStatus.Success,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
            FailureMessage = ""
        }, record);
        Assert.NotNull(record.PersonId);
    }

    [Fact]
    public async Task Import_AttemptToUpdateExistingRecordsNino_DoesNothing_ReportsFailureWithMessageAboutAttemptToUpdateNino()
    {
        // Arrange
        var existingPerson = await TestData.CreatePersonAsync(p => p
            .WithGender(Gender.Male)
            .WithDateOfBirth(new DateOnly(1972, 01, 01))
            .WithNationalInsuranceNumber());

        var newFirstName = Faker.Name.First();
        var newNino = Faker.Identification.UkNationalInsuranceNumber();
        (var reader, var rowData) = BuildSingleRowCsv(existingPerson.Trn, existingPerson.Gender, existingPerson.LastName, newFirstName, existingPerson.DateOfBirth, newNino);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var samePerson = await AssertSinglePersonAsync(existingPerson.Trn);
        AssertHasProperties(new
        {
            NationalInsuranceNumber = existingPerson.NationalInsuranceNumber,
            LastName = existingPerson.LastName,
            FirstName = existingPerson.FirstName
        }, samePerson);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 0,
            WarningCount = 1,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            Status = IntegrationTransactionRecordStatus.Warning,
            PersonId = samePerson.PersonId,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
        }, record);
        Assert.Contains($"Warning: Attempted to update NationalInsuranceNumber from {samePerson.NationalInsuranceNumber} to {newNino}", record.FailureMessage);

        await AssertNoSupportTaskAsync(SupportTaskType.TeacherPensionsPotentialDuplicate, samePerson.PersonId);
    }

    [Fact]
    public async Task Import_AttemptToUpdateExistingRecordsGender_DoesNothing_ReportsFailureWithMessageAboutAttemptToUpdateGender()
    {
        // Arrange
        var existingPerson = await TestData.CreatePersonAsync(p => p
            .WithGender(Gender.Male)
            .WithDateOfBirth(new DateOnly(1972, 01, 01)));

        var newFirstName = Faker.Name.First();
        var newGender = Gender.Female;
        var newNino = Faker.Identification.UkNationalInsuranceNumber();
        (var reader, var rowData) = BuildSingleRowCsv(existingPerson.Trn, newGender, existingPerson.LastName, newFirstName, existingPerson.DateOfBirth, newNino);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var samePerson = await AssertSinglePersonAsync(existingPerson.Trn);
        AssertHasProperties(new
        {
            NationalInsuranceNumber = newNino,
            LastName = existingPerson.LastName,
            FirstName = existingPerson.FirstName
        }, samePerson);

        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 0,
            WarningCount = 1,
            FailureCount = 0,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            Status = IntegrationTransactionRecordStatus.Warning,
            PersonId = samePerson.PersonId,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
        }, record);
        Assert.Contains($"Warning: Attempted to update gender from {samePerson.Gender} to {newGender}", record.FailureMessage);

        await AssertNoSupportTaskAsync(SupportTaskType.TeacherPensionsPotentialDuplicate, samePerson.PersonId);
    }

    [Fact]
    public async Task Import_AttemptToCreateNewRecordWithoutFirstAndLastName_DoesNothing_ReportsFailureWithValidationMessage()
    {
        // Arrange
        (var reader, var rowData) = BuildSingleRowCsv("1234567", "1", null, null, "19991201", "AB123456D");

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, "SingleFile.txt");

        // Assert
        var transaction = await AssertSingleIntegrationTransactionAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            TotalCount = 1,
            SuccessCount = 0,
            DuplicateCount = 0,
            WarningCount = 0,
            FailureCount = 1,
            ImportStatus = IntegrationTransactionImportStatus.Success,
            FileName = "SingleFile.txt"
        }, transaction);

        var record = await AssertSingleIntegrationTransactionRecordAsync(integrationTransactionId);
        AssertHasProperties(new
        {
            RowData = rowData,
            PersonId = (Guid?)null,
            Status = IntegrationTransactionRecordStatus.Failure,
            Duplicate = false,
            HasActiveAlert = (bool?)null,
        }, record);
        Assert.Contains($"Unable to create a new record without a firstname", record.FailureMessage);
        Assert.Contains($"Unable to create a new record without a lastname", record.FailureMessage);
    }

    private DbFixture DbFixture => Fixture.DbFixture;

    private IClock Clock => Fixture.Clock;

    private TestData TestData => Fixture.TestData;

    private CapitaImportJob Job => Fixture.Job;

    private TrsDbContext? DbContext { get; set; }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await DbFixture.DbHelper.ClearDataAsync();
        DbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    private (StreamReader, string) BuildEmptyCsv()
    {
        var row = "";
        var csvBytes = Encoding.UTF8.GetBytes(row);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        return (reader, row);
    }

    private (StreamReader, string) BuildSingleRowCsv(string? trn, Gender? gender, string? lastName, string? firstName, DateOnly? dob, string? ni, DateOnly? dateOfDeath = null)
    {
        var row = $"{trn};{(gender == null ? null : (int)gender)};{lastName};{firstName};;{dob?.ToString("yyyyMMdd")};{ni};{dateOfDeath?.ToString("yyyyMMdd")};;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";
        var csvBytes = Encoding.UTF8.GetBytes(row);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        return (reader, row);
    }

    private (StreamReader, string) BuildSingleRowCsv(string? trn, string? gender, string? lastName, string? firstName, string? dob, string? ni, string? dateOfDeath = null)
    {
        var row = $"{trn};{gender};{lastName};{firstName};;{dob};{ni};{dateOfDeath};;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";
        var csvBytes = Encoding.UTF8.GetBytes(row);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        return (reader, row);
    }

    private async Task DeactivatePerson(string? trn)
    {
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var selectedPerson = dbContext.Persons.Single(x => x.Trn == trn);
        selectedPerson.SetStatus(PersonStatus.Deactivated, "de-activate", "de-activated", null, SystemUser.SystemUserId, Clock.UtcNow, out var _);
        await dbContext.SaveChangesAsync();
    }

    private async Task<Person> AssertSinglePersonAsync(string? trn)
    {
        var person = await DbContext!.Persons.SingleOrDefaultAsync(x => x.Trn == trn);
        Assert.NotNull(person);

        return person;
    }

    private async Task AssertNoPersonAsync(string? trn)
    {
        var person = await DbContext!.Persons.SingleOrDefaultAsync(x => x.Trn == trn);
        Assert.Null(person);
    }

    private async Task<IntegrationTransaction> AssertSingleIntegrationTransactionAsync(long? integrationTransactionId)
    {
        var transaction = await DbContext!.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleOrDefaultAsync(x => x.IntegrationTransactionId == integrationTransactionId);
        Assert.NotNull(transaction);

        return transaction;
    }

    private async Task<IntegrationTransactionRecord> AssertSingleIntegrationTransactionRecordAsync(long? integrationTransactionId)
    {
        var transaction = await DbContext!.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleOrDefaultAsync(x => x.IntegrationTransactionId == integrationTransactionId);
        Assert.NotNull(transaction);
        Assert.NotNull(transaction.IntegrationTransactionRecords);

        var record = Assert.Single(transaction.IntegrationTransactionRecords);

        return record;
    }

    private async Task AssertNoIntegrationTransactionRecordAsync(long? integrationTransactionId)
    {
        var transaction = await DbContext!.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleOrDefaultAsync(x => x.IntegrationTransactionId == integrationTransactionId);
        Assert.NotNull(transaction);
        Assert.NotNull(transaction.IntegrationTransactionRecords);

        Assert.Empty(transaction.IntegrationTransactionRecords);
    }

    private async Task<SupportTask> AssertSingleSupportTaskAsync(SupportTaskType? taskType, Guid? personId)
    {
        var task = await DbContext!.SupportTasks.SingleOrDefaultAsync(x => x.SupportTaskType == taskType && x.PersonId == personId);
        Assert.NotNull(task);

        return task;
    }

    private async Task AssertNoSupportTaskAsync(SupportTaskType? taskType, Guid? personId)
    {
        var task = await DbContext!.SupportTasks.SingleOrDefaultAsync(x => x.SupportTaskType == taskType && x.PersonId == personId);
        Assert.Null(task);
    }

    private async Task<TrnRequestMetadata> AssertSingleTrnRequestMetadataAsync(string? trnRequestId)
    {
        var metadata = await DbContext!.TrnRequestMetadata.SingleOrDefaultAsync(x => x.RequestId == trnRequestId);
        Assert.NotNull(metadata);

        return metadata;
    }

    private async Task<TEvent> AssertSingleEventAsync<TEvent>(Guid personId)
    {
        var @event = await DbContext!.Events.SingleOrDefaultAsync(e => e.EventName == typeof(TEvent).Name && e.PersonIds.Contains(personId));
        Assert.NotNull(@event);

        var payload = JsonSerializer.Deserialize<TEvent>(@event.Payload);
        Assert.NotNull(payload);

        return payload;
    }

    private void AssertHasProperties<T>(object expected, T actual)
    {
        var expectedPropertyNames = expected.GetType().GetProperties().Select(p => p.Name);
        var actualPropertyNames = typeof(T).GetProperties().Select(p => p.Name);
        var excludePropertyNames = actualPropertyNames.Except(expectedPropertyNames).ToArray();
        Assert.EquivalentWithExclusions(expected, actual, excludePropertyNames);
    }
}

public class CapitaImportJobFixture : IAsyncLifetime
{
    public CapitaImportJobFixture(
        DbFixture dbFixture,
        ReferenceDataCache referenceDataCache,
        IServiceProvider provider)
    {
        DbFixture = dbFixture;
        Clock = new();

        Logger = new Mock<ILogger<CapitaImportJob>>();

        var dataLakeServiceClientMock = new Mock<DataLakeServiceClient>();
        var fileSystemClientMock = new Mock<DataLakeFileSystemClient>();
        var dataLakeFileClientMock = new Mock<DataLakeFileClient>();

        dataLakeServiceClientMock
            .Setup(s => s.GetFileSystemClient(It.IsAny<string>()))
            .Returns(fileSystemClientMock.Object);
        fileSystemClientMock
            .Setup(f => f.CreateIfNotExistsAsync(
                It.IsAny<DataLakeFileSystemCreateOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<Azure.Storage.Files.DataLake.Models.FileSystemInfo>>());
        fileSystemClientMock
            .Setup(f => f.GetFileClient(It.IsAny<string>()))
            .Returns(dataLakeFileClientMock.Object);

        dataLakeFileClientMock
            .Setup(f => f.UploadAsync(It.IsAny<Stream>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<PathInfo>>());

        var matchingService = new PersonMatchingService(dbFixture.DbContextFactory.CreateDbContext());
        var user = new CapitaTpsUserOption() { CapitaTpsUserId = ApplicationUser.CapitaTpsImportUser.UserId };
        var option = Options.Create(user);

        Job = ActivatorUtilities.CreateInstance<CapitaImportJob>(provider, dataLakeServiceClientMock.Object, Logger.Object, Clock, matchingService!, option);
        TestData = new TestData(
            dbFixture.DbContextFactory,
            referenceDataCache,
            Clock);
    }

    public DbFixture DbFixture { get; }

    public TestData TestData { get; }

    public TestableClock Clock { get; }

    public Mock<ILogger<CapitaImportJob>> Logger { get; }

    Task IAsyncLifetime.InitializeAsync() => DbFixture.DbHelper.ClearDataAsync();

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    public CapitaImportJob Job { get; }

    public Mock<IFileService> BlobStorageFileService { get; } = new Mock<IFileService>();

    public Mock<BlobServiceClient> BlobServiceClient { get; } = new Mock<BlobServiceClient>();
}
