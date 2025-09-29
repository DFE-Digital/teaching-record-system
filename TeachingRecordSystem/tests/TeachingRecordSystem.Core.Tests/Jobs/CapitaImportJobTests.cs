using System.Text;
using System.Text.Json;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.PersonMatching;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection(nameof(DisableParallelization))]
public class CapitaImportJobTests(CapitaImportJobFixture Fixture) : IClassFixture<CapitaImportJobFixture>, IAsyncLifetime
{
    private DbFixture DbFixture => Fixture.DbFixture;

    private IClock Clock => Fixture.Clock;

    private TestData TestData => Fixture.TestData;

    private CapitaImportJob Job => Fixture.Job;

    [Fact]
    public async Task Import_MatchesExistingRecordOnNameAndLastnameAndDob_RaisesDuplicateAndReturnsExpectedContent()
    {
        // Arrange
        var fileName = "SingleFile.txt";
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 0;
        var expectedDuplicateRowCount = 1;
        var expectedFailureRowCount = 0;
        var expectedNI = Faker.Identification.UkNationalInsuranceNumber();
        var expectedDob = new DateOnly(1981, 08, 20);
        var expectedGender = Gender.Male;
        var expectedLastName = Faker.Name.Last();
        var expectedFirstName = Faker.Name.First();
        var expectedStatus = IntegrationTransactionImportStatus.Success;
        var expectedDateOfDeath = Clock.UtcNow.AddDays(-1);
        var existingPerson = await TestData.CreatePersonAsync(item =>
        {
            item.WithGender(Gender.Male);
            item.WithDateOfBirth(expectedDob);
            item.WithFirstName(expectedFirstName);
            item.WithLastName(expectedLastName);
        });
        var expectedTrn = "1234567";
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var csvContent = $"{expectedTrn};{(int)expectedGender};{expectedLastName};{expectedFirstName};;{expectedDob.ToString("yyyyMMdd")};{expectedNI};;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);
        var expectedRow = $"{expectedTrn};" +
                          $"{(int)expectedGender};" +
                          $"{expectedLastName};" +
                          $"{expectedFirstName};" +
                          $";" +
                          $"{expectedDob.ToString("yyyyMMdd")};" +
                          $"{expectedNI};" +
                          $";" +
                          $";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, fileName);

        // Assert
        var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionId);
        var person = dbContext.Persons.FirstOrDefault(x => x.Trn == expectedTrn);
        Assert.NotNull(person);
        Assert.NotNull(person.NationalInsuranceNumber);

        Assert.NotNull(integrationTransaction);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.DuplicateCount);
        Assert.Equal(expectedStatus, integrationTransaction.ImportStatus);
        Assert.Equal(fileName, integrationTransaction.FileName);
        Assert.NotNull(integrationTransaction.IntegrationTransactionRecords);
        Assert.NotEmpty(integrationTransaction.IntegrationTransactionRecords);
        Assert.Collection(integrationTransaction.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.NotNull(item1.PersonId);
                    Assert.Equal(person.PersonId, item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Success, item1.Status);
                    Assert.Null(item1.HasActiveAlert);
                    Assert.True(item1.Duplicate);
                    Assert.NotNull(item1.RowData);
                    Assert.Equal(expectedRow, item1.RowData);
                    Assert.NotNull(item1.FailureMessage);
                    Assert.Empty(item1.FailureMessage);
                });
        var task = dbContext.SupportTasks.SingleOrDefault(x => x.SupportTaskType == SupportTaskType.TeacherPensionsPotentialDuplicate && x.PersonId == person.PersonId);
        Assert.NotNull(task);
        var trnRequest = dbContext.TrnRequestMetadata.Single(x => x.RequestId == task.TrnRequestId);
        Assert.NotNull(trnRequest);
        Assert.True(trnRequest.PotentialDuplicate);
        Assert.Contains(trnRequest.Matches?.MatchedPersons!, p => p.PersonId == existingPerson.PersonId);

        var createdEvent = await dbContext.Events
            .SingleAsync(e => e.EventName == nameof(PersonCreatedEvent) && e.PersonId == person.PersonId);
        Assert.NotNull(createdEvent);
    }

    [Fact]
    public async Task Import_TrnMatchesTrnOnDeactivatedRecord_ReturnsExpectedCounts()
    {
        // Arrange
        var fileName = "SingleFile.txt";
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 0;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 1;
        var expectedNI = Faker.Identification.UkNationalInsuranceNumber();
        var expectedDob = new DateOnly(1972, 01, 01);
        var expectedGender = Gender.Male;
        var expectedLastName = Faker.Name.Last();
        var expectedFirstName = Faker.Name.First();
        var expectedStatus = IntegrationTransactionImportStatus.Success;
        var expectedDateOfDeath = Clock.UtcNow.AddDays(-1);
        var existingPerson = await TestData.CreatePersonAsync(item =>
        {
            item.WithFirstName(Faker.Name.First());
            item.WithLastName(Faker.Name.Last());
            item.WithGender(Gender.Male);
            item.WithNationalInsuranceNumber(expectedNI);
            item.WithDateOfBirth(expectedDob);
        });
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var selectedPerson = dbContext.Persons.Single(x => x.Trn == existingPerson.Trn);
        selectedPerson.SetStatus(PersonStatus.Deactivated, "de-activate", "de-activated", null, SystemUser.SystemUserId, Clock.UtcNow, out var _);
        await dbContext.SaveChangesAsync();

        var csvContent = $"{existingPerson.Trn};{(int)expectedGender};{expectedLastName};{expectedFirstName};;{expectedDob.ToString("yyyyMMdd")};{expectedNI};;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);
        var expectedRow = $"{existingPerson.Trn};" +
                          $"{(int)expectedGender};" +
                          $"{expectedLastName};" +
                          $"{expectedFirstName};" +
                          $";" +
                          $"{expectedDob.ToString("yyyyMMdd")};" +
                          $"{expectedNI};" +
                          $";" +
                          $";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, fileName);

        // Assert
        var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionId);
        Assert.NotNull(integrationTransaction);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.DuplicateCount);
        Assert.Equal(expectedStatus, integrationTransaction.ImportStatus);
        Assert.Equal(fileName, integrationTransaction.FileName);
        Assert.NotNull(integrationTransaction.IntegrationTransactionRecords);
        Assert.NotEmpty(integrationTransaction.IntegrationTransactionRecords);
        Assert.Collection(integrationTransaction.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.Null(item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Failure, item1.Status);
                    Assert.Null(item1.HasActiveAlert);
                    Assert.False(item1.Duplicate);
                    Assert.NotNull(item1.RowData);
                    Assert.Equal(expectedRow, item1.RowData);
                    Assert.Contains($"de-activated record exists for trn {existingPerson.Trn}", item1.FailureMessage);
                });
    }

    [Fact]
    public async Task Import_MatchesExistingRecordOnNIAndDateOfBirth_RaisesDuplicateAndReturnsExpectedContent()
    {
        // Arrange
        var fileName = "SingleFile.txt";
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 0;
        var expectedDuplicateRowCount = 1;
        var expectedFailureRowCount = 0;
        var expectedNI = Faker.Identification.UkNationalInsuranceNumber();
        var expectedDob = new DateOnly(1972, 01, 01);
        var expectedGender = Gender.Male;
        var expectedLastName = Faker.Name.Last();
        var expectedFirstName = Faker.Name.First();
        var expectedStatus = IntegrationTransactionImportStatus.Success;
        var expectedDateOfDeath = Clock.UtcNow.AddDays(-1);
        var existingPerson = await TestData.CreatePersonAsync(item =>
        {
            item.WithFirstName(Faker.Name.First());
            item.WithLastName(Faker.Name.Last());
            item.WithGender(Gender.Male);
            item.WithNationalInsuranceNumber(expectedNI);
            item.WithDateOfBirth(expectedDob);
        });
        var expectedTrn = "8000038";
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var csvContent = $"{expectedTrn};{(int)expectedGender};{expectedLastName};{expectedFirstName};;{expectedDob.ToString("yyyyMMdd")};{expectedNI};;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);
        var expectedRow = $"{expectedTrn};" +
                          $"{(int)expectedGender};" +
                          $"{expectedLastName};" +
                          $"{expectedFirstName};" +
                          $";" +
                          $"{expectedDob.ToString("yyyyMMdd")};" +
                          $"{expectedNI};" +
                          $";" +
                          $";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, fileName);

        // Assert
        var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionId);
        var person = dbContext.Persons.FirstOrDefault(x => x.Trn == expectedTrn);
        Assert.NotNull(person);
        Assert.NotNull(person.NationalInsuranceNumber);

        Assert.NotNull(integrationTransaction);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.DuplicateCount);
        Assert.Equal(expectedStatus, integrationTransaction.ImportStatus);
        Assert.Equal(fileName, integrationTransaction.FileName);
        Assert.NotNull(integrationTransaction.IntegrationTransactionRecords);
        Assert.NotEmpty(integrationTransaction.IntegrationTransactionRecords);
        Assert.Collection(integrationTransaction.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.NotNull(item1.PersonId);
                    Assert.Equal(person.PersonId, item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Success, item1.Status);
                    Assert.Null(item1.HasActiveAlert);
                    Assert.True(item1.Duplicate);
                    Assert.NotNull(item1.RowData);
                    Assert.Equal(expectedRow, item1.RowData);
                    Assert.NotNull(item1.FailureMessage);
                    Assert.Empty(item1.FailureMessage);
                });
        var task = dbContext.SupportTasks.SingleOrDefault(x => x.SupportTaskType == SupportTaskType.TeacherPensionsPotentialDuplicate && x.PersonId == person.PersonId);
        Assert.NotNull(task);
        var trnRequest = dbContext.TrnRequestMetadata.Single(x => x.RequestId == task.TrnRequestId);
        Assert.NotNull(trnRequest);
        Assert.True(trnRequest.PotentialDuplicate);
        var createdEvent = await dbContext.Events
            .SingleAsync(e => e.EventName == nameof(PersonCreatedEvent) && e.PersonId == person.PersonId);
        Assert.NotNull(createdEvent);
    }

    [Fact]
    public async Task Import_MatchesOnTrnNoNinoWithDifferentLastName_UpdatesNinoAddsWarningMessageReturnsExpectedContent()
    {
        // Arrange
        var fileName = "SingleFile.txt";
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var expectedNI = Faker.Identification.UkNationalInsuranceNumber();
        var expectedDob = new DateOnly(1972, 01, 01);
        var expectedGender = Gender.Male;
        var expectedLastName = Faker.Name.Last();
        var expectedFirstName = Faker.Name.First();
        var expectedStatus = IntegrationTransactionImportStatus.Success;
        var expectedDateOfDeath = Clock.UtcNow.AddDays(-1);
        var existingPerson = await TestData.CreatePersonAsync(item =>
        {
            item.WithFirstName(Faker.Name.First());
            item.WithLastName(Faker.Name.Last());
            item.WithGender(Gender.Male);
            item.WithDateOfBirth(expectedDob);
        });
        var expectedTrn = existingPerson.Trn;
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var csvContent = $"{expectedTrn};{(int)expectedGender};{expectedLastName};{expectedFirstName};;{expectedDob.ToString("yyyyMMdd")};{expectedNI};;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);
        var expectedRow = $"{expectedTrn};" +
                          $"{(int)expectedGender};" +
                          $"{expectedLastName};" +
                          $"{expectedFirstName};" +
                          $";" +
                          $"{expectedDob.ToString("yyyyMMdd")};" +
                          $"{expectedNI};" +
                          $";" +
                          $";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, fileName);

        // Assert
        var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionId);
        var person = dbContext.Persons.FirstOrDefault(x => x.Trn == expectedTrn);
        Assert.NotNull(person);
        Assert.Equal(expectedNI, person.NationalInsuranceNumber);

        Assert.NotNull(integrationTransaction);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.DuplicateCount);
        Assert.Equal(expectedStatus, integrationTransaction.ImportStatus);
        Assert.Equal(fileName, integrationTransaction.FileName);
        Assert.NotNull(integrationTransaction.IntegrationTransactionRecords);
        Assert.NotEmpty(integrationTransaction.IntegrationTransactionRecords);
        Assert.Collection(integrationTransaction.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.NotNull(item1.PersonId);
                    Assert.Equal(person.PersonId, item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Success, item1.Status);
                    Assert.Null(item1.HasActiveAlert);
                    Assert.False(item1.Duplicate);
                    Assert.NotNull(item1.RowData);
                    Assert.Equal(expectedRow, item1.RowData);
                    Assert.NotNull(item1.FailureMessage);
                    Assert.Contains($"Warning: Attempted to update lastname from {person.LastName} to {expectedLastName}", item1.FailureMessage);
                });
        var task = dbContext.SupportTasks.SingleOrDefault(x => x.SupportTaskType == SupportTaskType.TeacherPensionsPotentialDuplicate && x.PersonId == person.PersonId);
        Assert.Null(task);
    }


    [Fact]
    public async Task Import_MatchesOnTrnNoNino_UpdatesNinoReturnsExpectedContent()
    {
        // Arrange
        var fileName = "SingleFile.txt";
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var expectedNI = Faker.Identification.UkNationalInsuranceNumber();
        var expectedDob = new DateOnly(1972, 01, 01);
        var expectedGender = Gender.Male;
        var expectedLastName = Faker.Name.Last();
        var expectedFirstName = Faker.Name.First();
        var expectedStatus = IntegrationTransactionImportStatus.Success;
        var expectedDateOfDeath = Clock.UtcNow.AddDays(-1);
        var existingPerson = await TestData.CreatePersonAsync(item =>
        {
            item.WithFirstName(Faker.Name.First());
            item.WithLastName(expectedLastName);
            item.WithGender(Gender.Male);
            item.WithDateOfBirth(expectedDob);
        });
        var expectedTrn = existingPerson.Trn;
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var csvContent = $"{expectedTrn};{(int)expectedGender};{expectedLastName};{expectedFirstName};;{expectedDob.ToString("yyyyMMdd")};{expectedNI};;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);
        var expectedRow = $"{expectedTrn};" +
                          $"{(int)expectedGender};" +
                          $"{expectedLastName};" +
                          $"{expectedFirstName};" +
                          $";" +
                          $"{expectedDob.ToString("yyyyMMdd")};" +
                          $"{expectedNI};" +
                          $";" +
                          $";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, fileName);

        // Assert
        var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionId);
        var person = dbContext.Persons.FirstOrDefault(x => x.Trn == expectedTrn);
        Assert.NotNull(person);
        Assert.Equal(expectedNI, person.NationalInsuranceNumber);

        Assert.NotNull(integrationTransaction);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.DuplicateCount);
        Assert.Equal(expectedStatus, integrationTransaction.ImportStatus);
        Assert.Equal(fileName, integrationTransaction.FileName);
        Assert.NotNull(integrationTransaction.IntegrationTransactionRecords);
        Assert.NotEmpty(integrationTransaction.IntegrationTransactionRecords);
        Assert.Collection(integrationTransaction.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.NotNull(item1.PersonId);
                    Assert.Equal(person.PersonId, item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Success, item1.Status);
                    Assert.Null(item1.HasActiveAlert);
                    Assert.False(item1.Duplicate);
                    Assert.NotNull(item1.RowData);
                    Assert.Equal(expectedRow, item1.RowData);
                    Assert.NotNull(item1.FailureMessage);
                    Assert.Empty(item1.FailureMessage);
                });
        var task = dbContext.SupportTasks.SingleOrDefault(x => x.SupportTaskType == SupportTaskType.TeacherPensionsPotentialDuplicate && x.PersonId == person.PersonId);
        Assert.Null(task);
    }

    [Fact]
    public async Task Import_AttemptToUpdateExistingRecordsGender_AddsWarningMessage()
    {
        // Arrange
        var fileName = "SingleFile.txt";
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var expectedNI = Faker.Identification.UkNationalInsuranceNumber();
        var expectedDob = new DateOnly(1972, 01, 01);
        var expectedGender = Gender.Female;
        var expectedLastName = Faker.Name.Last();
        var expectedFirstName = Faker.Name.First();
        var expectedStatus = IntegrationTransactionImportStatus.Success;
        var expectedDateOfDeath = Clock.UtcNow.AddDays(-1);
        var existingPerson = await TestData.CreatePersonAsync(item =>
        {
            item.WithFirstName(Faker.Name.First());
            item.WithLastName(expectedLastName);
            item.WithGender(Gender.Male);
            item.WithDateOfBirth(expectedDob);
        });
        var expectedTrn = existingPerson.Trn;
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var csvContent = $"{expectedTrn};{(int)expectedGender};{expectedLastName};{expectedFirstName};;{expectedDob.ToString("yyyyMMdd")};{expectedNI};;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);
        var expectedRow = $"{expectedTrn};" +
                          $"{(int)expectedGender};" +
                          $"{expectedLastName};" +
                          $"{expectedFirstName};" +
                          $";" +
                          $"{expectedDob.ToString("yyyyMMdd")};" +
                          $"{expectedNI};" +
                          $";" +
                          $";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, fileName);

        // Assert
        var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionId);
        var person = dbContext.Persons.FirstOrDefault(x => x.Trn == expectedTrn);
        Assert.NotNull(person);
        Assert.Equal(expectedNI, person.NationalInsuranceNumber);

        Assert.NotNull(integrationTransaction);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.DuplicateCount);
        Assert.Equal(expectedStatus, integrationTransaction.ImportStatus);
        Assert.Equal(fileName, integrationTransaction.FileName);
        Assert.NotNull(integrationTransaction.IntegrationTransactionRecords);
        Assert.NotEmpty(integrationTransaction.IntegrationTransactionRecords);
        Assert.Collection(integrationTransaction.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.NotNull(item1.PersonId);
                    Assert.Equal(person.PersonId, item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Success, item1.Status);
                    Assert.Null(item1.HasActiveAlert);
                    Assert.False(item1.Duplicate);
                    Assert.NotNull(item1.RowData);
                    Assert.Equal(expectedRow, item1.RowData);
                    Assert.NotNull(item1.FailureMessage);
                    Assert.Contains($"Warning: Attempted to update gender from {person.Gender} to {expectedGender}", item1.FailureMessage);
                });
        var task = dbContext.SupportTasks.SingleOrDefault(x => x.SupportTaskType == SupportTaskType.TeacherPensionsPotentialDuplicate && x.PersonId == person.PersonId);
        Assert.Null(task);
    }

    [Fact]
    public async Task Import_AttemptToUpdateExistingRecordsNationalInsuranceNumber_AddsWarningMessage()
    {
        // Arrange
        var fileName = "SingleFile.txt";
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var expectedNI = Faker.Identification.UkNationalInsuranceNumber();
        var expectedDob = new DateOnly(1972, 01, 01);
        var expectedGender = Gender.Female;
        var expectedLastName = Faker.Name.Last();
        var expectedFirstName = Faker.Name.First();
        var expectedStatus = IntegrationTransactionImportStatus.Success;
        var expectedDateOfDeath = Clock.UtcNow.AddDays(-1);
        var existingPerson = await TestData.CreatePersonAsync(item =>
        {
            item.WithFirstName(Faker.Name.First());
            item.WithLastName(expectedLastName);
            item.WithGender(Gender.Male);
            item.WithDateOfBirth(expectedDob);
            item.WithNationalInsuranceNumber();
        });
        var expectedTrn = existingPerson.Trn;
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var csvContent = $"{expectedTrn};{(int)expectedGender};{expectedLastName};{expectedFirstName};;{expectedDob.ToString("yyyyMMdd")};{expectedNI};;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);
        var expectedRow = $"{expectedTrn};" +
                          $"{(int)expectedGender};" +
                          $"{expectedLastName};" +
                          $"{expectedFirstName};" +
                          $";" +
                          $"{expectedDob.ToString("yyyyMMdd")};" +
                          $"{expectedNI};" +
                          $";" +
                          $";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, fileName);

        // Assert
        var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionId);
        var person = dbContext.Persons.FirstOrDefault(x => x.Trn == expectedTrn);
        Assert.NotNull(person);
        Assert.Equal(existingPerson.NationalInsuranceNumber, person.NationalInsuranceNumber);

        Assert.NotNull(integrationTransaction);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.DuplicateCount);
        Assert.Equal(expectedStatus, integrationTransaction.ImportStatus);
        Assert.Equal(fileName, integrationTransaction.FileName);
        Assert.NotNull(integrationTransaction.IntegrationTransactionRecords);
        Assert.NotEmpty(integrationTransaction.IntegrationTransactionRecords);
        Assert.Collection(integrationTransaction.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.NotNull(item1.PersonId);
                    Assert.Equal(person.PersonId, item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Success, item1.Status);
                    Assert.Null(item1.HasActiveAlert);
                    Assert.False(item1.Duplicate);
                    Assert.NotNull(item1.RowData);
                    Assert.Equal(expectedRow, item1.RowData);
                    Assert.NotNull(item1.FailureMessage);
                    Assert.Contains($"Warning: Attempted to update NationalInsuranceNumber from {person.NationalInsuranceNumber} to {expectedNI}", item1.FailureMessage);
                });
        var task = dbContext.SupportTasks.SingleOrDefault(x => x.SupportTaskType == SupportTaskType.TeacherPensionsPotentialDuplicate && x.PersonId == person.PersonId);
        Assert.Null(task);
    }

    [Fact]
    public async Task Import_MatchesExistingRecordOnNationalInsuranceNumber_RaisesDuplicateAndReturnsExpectedContent()
    {
        // Arrange
        var fileName = "SingleFile.txt";
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 0;
        var expectedDuplicateRowCount = 1;
        var expectedFailureRowCount = 0;
        var expectedNI = Faker.Identification.UkNationalInsuranceNumber();
        var expectedDob = new DateOnly(1981, 08, 20);
        var expectedGender = Gender.Male;
        var expectedLastName = Faker.Name.Last();
        var expectedFirstName = Faker.Name.First();
        var expectedStatus = IntegrationTransactionImportStatus.Success;
        var expectedDateOfDeath = Clock.UtcNow.AddDays(-1);
        var existingPerson = await TestData.CreatePersonAsync(item =>
        {
            item.WithGender(Gender.Male);
            item.WithNationalInsuranceNumber(expectedNI);
        });
        var expectedTrn = "1234567";
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var csvContent = $"{expectedTrn};{(int)expectedGender};{expectedLastName};{expectedFirstName};;{expectedDob.ToString("yyyyMMdd")};{expectedNI};;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);
        var expectedRow = $"{expectedTrn};" +
                          $"{(int)expectedGender};" +
                          $"{expectedLastName};" +
                          $"{expectedFirstName};" +
                          $";" +
                          $"{expectedDob.ToString("yyyyMMdd")};" +
                          $"{expectedNI};" +
                          $";" +
                          $";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, fileName);

        // Assert
        var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionId);
        var person = dbContext.Persons.FirstOrDefault(x => x.Trn == expectedTrn);
        Assert.NotNull(person);
        Assert.NotNull(person.NationalInsuranceNumber);

        Assert.NotNull(integrationTransaction);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.DuplicateCount);
        Assert.Equal(expectedStatus, integrationTransaction.ImportStatus);
        Assert.Equal(fileName, integrationTransaction.FileName);
        Assert.NotNull(integrationTransaction.IntegrationTransactionRecords);
        Assert.NotEmpty(integrationTransaction.IntegrationTransactionRecords);
        Assert.Collection(integrationTransaction.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.NotNull(item1.PersonId);
                    Assert.Equal(person.PersonId, item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Success, item1.Status);
                    Assert.Null(item1.HasActiveAlert);
                    Assert.True(item1.Duplicate);
                    Assert.NotNull(item1.RowData);
                    Assert.Equal(expectedRow, item1.RowData);
                    Assert.NotNull(item1.FailureMessage);
                    Assert.Empty(item1.FailureMessage);
                });
        var task = dbContext.SupportTasks.SingleOrDefault(x => x.SupportTaskType == SupportTaskType.TeacherPensionsPotentialDuplicate && x.PersonId == person.PersonId);
        Assert.NotNull(task);
        var trnRequest = dbContext.TrnRequestMetadata.Single(x => x.RequestId == task.TrnRequestId);
        Assert.NotNull(trnRequest);
        Assert.True(trnRequest.PotentialDuplicate);
        Assert.Contains(trnRequest.Matches?.MatchedPersons!, p => p.PersonId == existingPerson.PersonId);
    }

    [Fact]
    public async Task Import_ExistingPersonSetDateOfDeath_DeactivatesPersonAndReturnsExpectedContent()
    {
        // Arrange
        var fileName = "SingleFile.txt";
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var expectedNI = Faker.Identification.UkNationalInsuranceNumber();
        var expectedDob = new DateOnly(1981, 08, 20);
        var expectedGender = Gender.Male;
        var expectedLastName = Faker.Name.Last();
        var expectedFirstName = Faker.Name.First();
        var expectedStatus = IntegrationTransactionImportStatus.Success;
        var expectedDateOfDeath = Clock.UtcNow.AddDays(-1);
        var existingPerson = await TestData.CreatePersonAsync(item =>
        {
            item.WithNationalInsuranceNumber(expectedNI);
            item.WithDateOfBirth(expectedDob);
            item.WithLastName(expectedLastName);
        });
        var expectedTrn = existingPerson.Trn;
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var csvContent = $"{expectedTrn};{(int)expectedGender};{expectedLastName};{expectedFirstName};;{expectedDob.ToString("yyyyMMdd")};{expectedNI};{expectedDateOfDeath.ToString("yyyyMMdd")};;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);
        var expectedRow = $"{expectedTrn};" +
                          $"{(int)expectedGender};" +
                          $"{expectedLastName};" +
                          $"{expectedFirstName};" +
                          $";" +
                          $"{expectedDob.ToString("yyyyMMdd")};" +
                          $"{expectedNI};" +
                          $"{expectedDateOfDeath.ToString("yyyyMMdd")};" +
                          $";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, fileName);

        // Assert
        var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionId);
        var person = dbContext.Persons.FirstOrDefault(x => x.Trn == expectedTrn);
        var events = await dbContext.Events
            .Where(e => e.EventName == nameof(PersonStatusUpdatedEvent) && e.PersonIds.Contains(existingPerson.PersonId)).ToListAsync();
        var ev1 = events
            .Select(e => JsonSerializer.Deserialize<PersonStatusUpdatedEvent>(e.Payload)).Single();

        Assert.Null(person);
        Assert.NotNull(ev1);
        Assert.Equal("Date of death received from capita import", ev1.Reason);
        Assert.Equal(PersonStatus.Active, ev1.OldStatus);
        Assert.Equal(PersonStatus.Deactivated, ev1.Status);

        Assert.NotNull(integrationTransaction);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.DuplicateCount);
        Assert.Equal(expectedStatus, integrationTransaction.ImportStatus);
        Assert.Equal(fileName, integrationTransaction.FileName);
        Assert.NotNull(integrationTransaction.IntegrationTransactionRecords);
        Assert.NotEmpty(integrationTransaction.IntegrationTransactionRecords);
        Assert.Collection(integrationTransaction.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.NotNull(item1.PersonId);
                    Assert.Equal(existingPerson.PersonId, item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Success, item1.Status);
                    Assert.Null(item1.HasActiveAlert);
                    Assert.False(item1.Duplicate);
                    Assert.NotNull(item1.RowData);
                    Assert.Equal(expectedRow, item1.RowData);
                    Assert.NotNull(item1.FailureMessage);
                    Assert.Empty(item1.FailureMessage);
                });
    }

    [Fact]
    public async Task Import_NewPersonSetDateOfDeath_DeactivatesPersonAndReturnsExpectedContent()
    {
        // Arrange
        var fileName = "SingleFile.txt";
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var expectedNI = Faker.Identification.UkNationalInsuranceNumber();
        var expectedDob = new DateOnly(1981, 08, 20);
        var expectedGender = Gender.Male;
        var expectedLastName = Faker.Name.Last();
        var expectedFirstName = Faker.Name.First();
        var expectedStatus = IntegrationTransactionImportStatus.Success;
        var expectedDateOfDeath = Clock.UtcNow.AddDays(-1);
        var expectedTrn = "1234567"; ;
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var csvContent = $"{expectedTrn};{(int)expectedGender};{expectedLastName};{expectedFirstName};;{expectedDob.ToString("yyyyMMdd")};{expectedNI};{expectedDateOfDeath.ToString("yyyyMMdd")};;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);
        var expectedRow = $"{expectedTrn};" +
                          $"{(int)expectedGender};" +
                          $"{expectedLastName};" +
                          $"{expectedFirstName};" +
                          $";" +
                          $"{expectedDob.ToString("yyyyMMdd")};" +
                          $"{expectedNI};" +
                          $"{expectedDateOfDeath.ToString("yyyyMMdd")};" +
                          $";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, fileName);

        // Assert
        var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionId);
        var person = dbContext.Persons.FirstOrDefault(x => x.Trn == expectedTrn);
        Assert.Null(person);
        Assert.NotNull(integrationTransaction);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.DuplicateCount);
        Assert.Equal(expectedStatus, integrationTransaction.ImportStatus);
        Assert.Equal(fileName, integrationTransaction.FileName);
        Assert.NotNull(integrationTransaction.IntegrationTransactionRecords);
        Assert.NotEmpty(integrationTransaction.IntegrationTransactionRecords);
        Assert.Collection(integrationTransaction.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.NotNull(item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Success, item1.Status);
                    Assert.Null(item1.HasActiveAlert);
                    Assert.False(item1.Duplicate);
                    Assert.NotNull(item1.RowData);
                    Assert.Equal(expectedRow, item1.RowData);
                    Assert.NotNull(item1.FailureMessage);
                    Assert.Empty(item1.FailureMessage);
                });
    }

    [Fact]
    public async Task Import_WithoutMiddleName_ReturnsExpectedRecords()
    {
        // Arrange
        var fileName = "SingleFile.txt";
        var gender = Gender.Male;
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var expectedTrn = "9988776";
        var expectedNI = Faker.Identification.UkNationalInsuranceNumber();
        var expectedDob = new DateOnly(1981, 08, 20);
        var expectedGender = gender;
        var expectedLastName = Faker.Name.Last();
        var expectedFirstName = Faker.Name.First();
        var expectedMiddleName = Faker.Name.Middle();
        var expectedStatus = IntegrationTransactionImportStatus.Success;
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var csvContent = $"{expectedTrn};{(int)expectedGender};{expectedLastName};{expectedFirstName};;{expectedDob.ToString("yyyyMMdd")};{expectedNI};;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);
        var expectedRow = $"{expectedTrn};" +
                          $"{(int)gender};" +
                          $"{expectedLastName};" +
                          $"{expectedFirstName};" +
                          $";" +
                          $"{expectedDob.ToString("yyyyMMdd")};" +
                          $"{expectedNI};" +
                          $";" +
                          $";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, fileName);

        // Assert
        var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionId);
        var person = dbContext.Persons.FirstOrDefault(x => x.Trn == expectedTrn);
        Assert.NotNull(person);
        Assert.Equal(expectedTrn, person.Trn);
        Assert.Equal(expectedDob, person.DateOfBirth);
        Assert.Equal(expectedGender, person.Gender);
        Assert.Equal(expectedNI, person.NationalInsuranceNumber);
        Assert.Equal(expectedFirstName, person.FirstName);
        Assert.Empty(person.MiddleName);
        Assert.Equal(expectedLastName, person.LastName);
        Assert.True(person.CreatedByTps);

        Assert.NotNull(integrationTransaction);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.DuplicateCount);
        Assert.Equal(expectedStatus, integrationTransaction.ImportStatus);
        Assert.Equal(fileName, integrationTransaction.FileName);
        Assert.NotNull(integrationTransaction.IntegrationTransactionRecords);
        Assert.NotEmpty(integrationTransaction.IntegrationTransactionRecords);
        Assert.Collection(integrationTransaction.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.NotNull(item1.PersonId);
                    Assert.Equal(person.PersonId, item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Success, item1.Status);
                    Assert.Null(item1.HasActiveAlert);
                    Assert.False(item1.Duplicate);
                    Assert.NotNull(item1.RowData);
                    Assert.Equal(expectedRow, item1.RowData);
                    Assert.NotNull(item1.FailureMessage);
                    Assert.Empty(item1.FailureMessage);
                });
    }

    [Fact]
    public async Task Import_WithFirstAndMiddleNameCombined_ReturnsExpectedRecords()
    {
        // Arrange
        var fileName = "SingleFile.txt";
        var gender = Gender.Male;
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var expectedTrn = "9988776";
        var expectedNI = Faker.Identification.UkNationalInsuranceNumber();
        var expectedDob = new DateOnly(1981, 08, 20);
        var expectedGender = gender;
        var expectedLastName = Faker.Name.Last();
        var expectedFirstName = Faker.Name.First();
        var expectedMiddleName = Faker.Name.Middle();
        var expectedStatus = IntegrationTransactionImportStatus.Success;
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var csvContent = $"{expectedTrn};{(int)expectedGender};{expectedLastName};{expectedFirstName} {expectedMiddleName};;{expectedDob.ToString("yyyyMMdd")};{expectedNI};;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);
        var expectedRow = $"{expectedTrn};" +
                          $"{(int)gender};" +
                          $"{expectedLastName};" +
                          $"{expectedFirstName} {expectedMiddleName};" +
                          $";" +
                          $"{expectedDob.ToString("yyyyMMdd")};" +
                          $"{expectedNI};" +
                          $";" +
                          $";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, fileName);

        // Assert
        var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionId);
        var person = dbContext.Persons.FirstOrDefault(x => x.Trn == expectedTrn);
        Assert.NotNull(person);
        Assert.Equal(expectedTrn, person.Trn);
        Assert.Equal(expectedDob, person.DateOfBirth);
        Assert.Equal(expectedGender, person.Gender);
        Assert.Equal(expectedNI, person.NationalInsuranceNumber);
        Assert.Equal(expectedFirstName, person.FirstName);
        Assert.Equal(expectedMiddleName, person.MiddleName);
        Assert.Equal(expectedLastName, person.LastName);
        Assert.True(person.CreatedByTps);

        Assert.NotNull(integrationTransaction);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.DuplicateCount);
        Assert.Equal(expectedStatus, integrationTransaction.ImportStatus);
        Assert.Equal(fileName, integrationTransaction.FileName);
        Assert.NotNull(integrationTransaction.IntegrationTransactionRecords);
        Assert.NotEmpty(integrationTransaction.IntegrationTransactionRecords);
        Assert.Collection(integrationTransaction.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.NotNull(item1.PersonId);
                    Assert.Equal(person.PersonId, item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Success, item1.Status);
                    Assert.Null(item1.HasActiveAlert);
                    Assert.False(item1.Duplicate);
                    Assert.NotNull(item1.RowData);
                    Assert.Equal(expectedRow, item1.RowData);
                    Assert.NotNull(item1.FailureMessage);
                    Assert.Empty(item1.FailureMessage);
                });
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    public async Task Import_WithNoMatchesCreatesPerson_ReturnsExpectedRecords(Gender gender)
    {
        // Arrange
        var fileName = "SingleFile.txt";
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var expectedTrn = "9988776";
        var expectedNI = Faker.Identification.UkNationalInsuranceNumber();
        var expectedDob = new DateOnly(1981, 08, 20);
        var expectedGender = gender;
        var expectedLastName = Faker.Name.Last();
        var expectedFirstName = Faker.Name.First();
        var expectedStatus = IntegrationTransactionImportStatus.Success;
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var csvContent = $"{expectedTrn};{(int)expectedGender};{expectedLastName};{expectedFirstName};;{expectedDob.ToString("yyyyMMdd")};{expectedNI};;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);
        var expectedRow = $"{expectedTrn};" +
                          $"{(int)gender};" +
                          $"{expectedLastName};" +
                          $"{expectedFirstName};" +
                          $";" +
                          $"{expectedDob.ToString("yyyyMMdd")};" +
                          $"{expectedNI};" +
                          $";" +
                          $";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, fileName);

        // Assert
        var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionId);
        var person = dbContext.Persons.FirstOrDefault(x => x.Trn == expectedTrn);
        Assert.NotNull(person);
        Assert.Equal(expectedTrn, person.Trn);
        Assert.Equal(expectedDob, person.DateOfBirth);
        Assert.Equal(expectedGender, person.Gender);
        Assert.Equal(expectedNI, person.NationalInsuranceNumber);
        Assert.Equal(expectedFirstName, person.FirstName);
        Assert.Equal(expectedLastName, person.LastName);
        Assert.True(person.CreatedByTps);

        Assert.NotNull(integrationTransaction);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.DuplicateCount);
        Assert.Equal(expectedStatus, integrationTransaction.ImportStatus);
        Assert.Equal(fileName, integrationTransaction.FileName);
        Assert.NotNull(integrationTransaction.IntegrationTransactionRecords);
        Assert.NotEmpty(integrationTransaction.IntegrationTransactionRecords);
        Assert.Collection(integrationTransaction.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.NotNull(item1.PersonId);
                    Assert.Equal(person.PersonId, item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Success, item1.Status);
                    Assert.Null(item1.HasActiveAlert);
                    Assert.False(item1.Duplicate);
                    Assert.NotNull(item1.RowData);
                    Assert.Equal(expectedRow, item1.RowData);
                    Assert.NotNull(item1.FailureMessage);
                    Assert.Empty(item1.FailureMessage);
                });
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    public async Task Import_WithInvalidNino_CreatesPersonAndReturnsExpectedWarning(Gender gender)
    {
        // Arrange
        var fileName = "SingleFile.txt";
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var expectedTrn = "9988776";
        var expectedNI = "JL5618AAB";
        var expectedDob = new DateOnly(1981, 08, 20);
        var expectedGender = gender;
        var expectedLastName = Faker.Name.Last();
        var expectedFirstName = Faker.Name.First();
        var expectedStatus = IntegrationTransactionImportStatus.Success;
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var csvContent = $"{expectedTrn};{(int)expectedGender};{expectedLastName};{expectedFirstName};;{expectedDob.ToString("yyyyMMdd")};{expectedNI};;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);
        var expectedRow = $"{expectedTrn};" +
                          $"{(int)gender};" +
                          $"{expectedLastName};" +
                          $"{expectedFirstName};" +
                          $";" +
                          $"{expectedDob.ToString("yyyyMMdd")};" +
                          $"{expectedNI};" +
                          $";" +
                          $";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, fileName);

        // Assert
        var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionId);
        var person = dbContext.Persons.FirstOrDefault(x => x.Trn == expectedTrn);
        Assert.NotNull(person);
        Assert.Equal(expectedTrn, person.Trn);
        Assert.Equal(expectedDob, person.DateOfBirth);
        Assert.Equal(expectedGender, person.Gender);
        Assert.Null(person.NationalInsuranceNumber);
        Assert.Equal(expectedFirstName, person.FirstName);
        Assert.Equal(expectedLastName, person.LastName);
        Assert.True(person.CreatedByTps);

        Assert.NotNull(integrationTransaction);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.DuplicateCount);
        Assert.Equal(expectedStatus, integrationTransaction.ImportStatus);
        Assert.Equal(fileName, integrationTransaction.FileName);
        Assert.NotNull(integrationTransaction.IntegrationTransactionRecords);
        Assert.NotEmpty(integrationTransaction.IntegrationTransactionRecords);
        Assert.Collection(integrationTransaction.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.NotNull(item1.PersonId);
                    Assert.Equal(person.PersonId, item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Success, item1.Status);
                    Assert.Null(item1.HasActiveAlert);
                    Assert.False(item1.Duplicate);
                    Assert.NotNull(item1.RowData);
                    Assert.Equal(expectedRow, item1.RowData);
                    Assert.Contains("Invalid National Insurance number", item1.FailureMessage);
                });
    }

    [Fact]
    public async Task Import_WithNoData_ReturnsExpectedRecords()
    {
        // Arrange
        var fileName = "EmptyFile.csv";
        var expectedTotalRowCount = 0;
        var expectedSuccessCount = 0;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var expectedStatus = IntegrationTransactionImportStatus.Success;
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var csvContent = $"";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, fileName);

        // Assert
        var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionId);
        Assert.NotNull(integrationTransaction);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.DuplicateCount);
        Assert.Equal(expectedStatus, integrationTransaction.ImportStatus);
        Assert.Equal(fileName, integrationTransaction.FileName);
        Assert.NotNull(integrationTransaction.IntegrationTransactionRecords);
        Assert.Empty(integrationTransaction.IntegrationTransactionRecords);
    }

    [Fact]
    public async Task Validate_MissingTrn_ReturnsValidationError()
    {
        // Arrange
        var record = new CapitaImportRecord()
        {
            TRN = null,
            Gender = null,
            LastName = null,
            FirstNameOrMiddleName = null,
            PreviousLastName = null,
            DateOfBirth = null,
            NINumber = null,
            DateOfDeath = null
        };

        // Act
        var (errors, warnings, _) = await Job.ValidateRowAsync(record);

        // Assert
        Assert.Contains("Missing required field: TRN", errors);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("111111111")]
    [InlineData("111111a")]
    public async Task Validate_InvalidTrn_ReturnsValidationError(string trn)
    {
        // Arrange
        var record = new CapitaImportRecord()
        {
            TRN = trn,
            Gender = null,
            LastName = null,
            FirstNameOrMiddleName = null,
            PreviousLastName = null,
            DateOfBirth = null,
            NINumber = null,
            DateOfDeath = null
        };

        // Act
        var (errors, warnings, _) = await Job.ValidateRowAsync(record);

        // Assert
        Assert.Contains("Validation failed on field: TRN", errors);
    }

    [Fact]
    public async Task Validate_MissingGender_ReturnsValidationError()
    {
        // Arrange
        var record = new CapitaImportRecord()
        {
            TRN = "1234567",
            Gender = null,
            LastName = null,
            FirstNameOrMiddleName = null,
            PreviousLastName = null,
            DateOfBirth = null,
            NINumber = null,
            DateOfDeath = null
        };

        // Act
        var (errors, warnings, _) = await Job.ValidateRowAsync(record);

        // Assert
        Assert.Contains("Missing required field: Date of birth", errors);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(0)]
    [InlineData(-13)]
    public async Task Validate_InvalidGender_ReturnsValidationError(int gender)
    {
        // Arrange
        var record = new CapitaImportRecord()
        {
            TRN = "1234567",
            Gender = gender,
            LastName = null,
            FirstNameOrMiddleName = null,
            PreviousLastName = null,
            DateOfBirth = "20110101",
            NINumber = null,
            DateOfDeath = null
        };

        // Act
        var (errors, warnings, _) = await Job.ValidateRowAsync(record);

        // Assert
        Assert.Contains($"Invalid Gender: {record.Gender.Value}", errors);
    }

    [Theory]
    [InlineData("08011972")] //invalid month
    [InlineData("19900231")] //invalid day
    public async Task Validate_DateOfBirthIncorrectFormat_ReturnsValidationError(string dateOfBirth)
    {
        // Arrange
        var record = new CapitaImportRecord()
        {
            TRN = "1234567",
            Gender = (int)Gender.Male,
            LastName = null,
            FirstNameOrMiddleName = null,
            PreviousLastName = null,
            DateOfBirth = dateOfBirth,
            NINumber = null,
            DateOfDeath = null
        };

        // Act
        var (errors, warnings, _) = await Job.ValidateRowAsync(record);

        // Assert
        Assert.Contains($"Validation Failed: Invalid Date of Birth", errors);
    }

    [Fact]
    public async Task Validate_DateOfBirthMissing_ReturnsValidationError()
    {
        // Arrange
        var record = new CapitaImportRecord()
        {
            TRN = "1234567",
            Gender = null,
            LastName = null,
            FirstNameOrMiddleName = null,
            PreviousLastName = null,
            DateOfBirth = null,
            NINumber = null,
            DateOfDeath = null
        };

        // Act
        var (errors, warnings, _) = await Job.ValidateRowAsync(record);

        // Assert
        Assert.Contains("Missing required field: Date of birth", errors);
    }

    [Fact]
    public async Task Validate_CreateNewRecordWithoutFirstAndLastName_ReturnsValidationError()
    {
        // Arrange
        var dateOfBirth = Clock.UtcNow.AddDays(25);
        var person = await TestData.CreatePersonAsync();
        var record = new CapitaImportRecord()
        {
            TRN = "1234567",
            Gender = (int)Gender.Male,
            LastName = null,
            FirstNameOrMiddleName = null,
            PreviousLastName = null,
            DateOfBirth = dateOfBirth.ToDateOnlyWithDqtBstFix(isLocalTime: true).ToString("yyyyMMdd"),
            NINumber = null,
            DateOfDeath = null
        };

        // Act
        var (errors, warnings, _) = await Job.ValidateRowAsync(record);

        // Assert
        Assert.Contains($"Unable to create a new record without a firstname", errors);
        Assert.Contains($"Unable to create a new record without a lastname", errors);
    }

    [Fact]
    public async Task Validate_DateOfBirthInFuture_ReturnsValidationError()
    {
        // Arrange
        var dateOfBirth = Clock.UtcNow.AddDays(25);
        var record = new CapitaImportRecord()
        {
            TRN = "1234567",
            Gender = (int)Gender.Male,
            LastName = null,
            FirstNameOrMiddleName = null,
            PreviousLastName = null,
            DateOfBirth = dateOfBirth.ToDateOnlyWithDqtBstFix(isLocalTime: true).ToString("yyyyMMdd"),
            NINumber = null,
            DateOfDeath = null
        };

        // Act
        var (errors, warnings, _) = await Job.ValidateRowAsync(record);

        // Assert
        Assert.Contains($"Validation Failed: Date of Birth cannot be in the future", errors);
    }

    [Fact]
    public async Task Validate_DateOfDeathInFuture_ReturnsValidationError()
    {
        // Arrange
        var dateOfBirth = Clock.UtcNow.AddYears(-25);
        var dateOfDeath = Clock.UtcNow.AddDays(25);
        var record = new CapitaImportRecord()
        {
            TRN = "1234567",
            Gender = (int)Gender.Male,
            LastName = null,
            FirstNameOrMiddleName = null,
            PreviousLastName = null,
            DateOfBirth = dateOfBirth.ToDateOnlyWithDqtBstFix(isLocalTime: true).ToString("yyyyMMdd"),
            NINumber = null,
            DateOfDeath = dateOfDeath.ToDateOnlyWithDqtBstFix(isLocalTime: true).ToString("yyyyMMdd")
        };

        // Act
        var (errors, warnings, _) = await Job.ValidateRowAsync(record);

        // Assert
        Assert.Contains($"Validation Failed: Date of death cannot be in the future", errors);
    }

    [Theory]
    [InlineData("01022025")] //invalid month
    [InlineData("19900931")] //invalid day
    public async Task Validate_DateOfDeathInvalid_ReturnsValidationError(string dateOfDeath)
    {
        // Arrange
        var dateOfBirth = Clock.UtcNow.AddYears(-25);
        var record = new CapitaImportRecord()
        {
            TRN = "1234567",
            Gender = (int)Gender.Male,
            LastName = null,
            FirstNameOrMiddleName = null,
            PreviousLastName = null,
            DateOfBirth = dateOfBirth.ToDateOnlyWithDqtBstFix(isLocalTime: true).ToString("yyyyMMdd"),
            NINumber = null,
            DateOfDeath = dateOfDeath
        };

        // Act
        var (errors, warnings, _) = await Job.ValidateRowAsync(record);

        // Assert
        Assert.Contains($"Validation Failed: Invalid Date of death", errors);
    }

    [Fact]
    public async Task GetPotentialMatchingPersonsAsync_WithNoMatchingCriteria_ReturnsNoMatches()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var row = new CapitaImportRecord()
        {
            DateOfBirth = "19990101",
            DateOfDeath = null,
            Gender = (int)Gender.Male,
            FirstNameOrMiddleName = "Firstname",
            LastName = "Lastname",
            NINumber = "",
            PreviousLastName = null,
            TRN = person.Trn
        };

        // Act
        var results = await Job.GetPotentialMatchingPersonsAsync(row);

        // Assert
        Assert.Equal(TrnRequestMatchResultOutcome.NoMatches, results.Outcome);
    }

    [Fact]
    public async Task GetPotentialMatchingPersonsAsync_WithMatchingNationalInsuranceNumberAndDob_ReturnsDefiniteMatch()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());
        var row = new CapitaImportRecord()
        {
            DateOfBirth = person.DateOfBirth.ToString("yyyyMMdd"),
            DateOfDeath = null,
            Gender = (int)Gender.Male,
            FirstNameOrMiddleName = "Firstname",
            LastName = "Lastname",
            NINumber = person.NationalInsuranceNumber,
            PreviousLastName = null,
            TRN = person.Trn
        };

        // Act
        var results = await Job.GetPotentialMatchingPersonsAsync(row);

        // Assert
        Assert.Equal(TrnRequestMatchResultOutcome.DefiniteMatch, results.Outcome);
    }

    [Fact]
    public async Task GetPotentialMatchingPersonsAsync_WithMatchingFirstNameLastNameAndDateOfBirth_ReturnsPotentialMatches()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());
        var person2 = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());
        var row = new CapitaImportRecord()
        {
            DateOfBirth = person.DateOfBirth.ToString("yyyyMMdd"),
            DateOfDeath = null,
            Gender = (int)Gender.Male,
            FirstNameOrMiddleName = person.FirstName,
            LastName = person.LastName,
            NINumber = null,
            PreviousLastName = null,
            TRN = person2.Trn
        };

        // Act
        var results = await Job.GetPotentialMatchingPersonsAsync(row);

        // Assert
        Assert.Equal(TrnRequestMatchResultOutcome.PotentialMatches, results.Outcome);
    }


    [Fact]
    public async Task GetPotentialMatchingPersonsAsync_WithMatchingOnOnlyNINumber_ReturnsPotentialMatches()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());
        var person2 = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());
        var row = new CapitaImportRecord()
        {
            DateOfBirth = "20110101",
            DateOfDeath = null,
            Gender = (int)Gender.Male,
            FirstNameOrMiddleName = "Test",
            LastName = "test",
            NINumber = person.NationalInsuranceNumber,
            PreviousLastName = null,
            TRN = "1234567"
        };

        // Act
        var results = await Job.GetPotentialMatchingPersonsAsync(row);

        // Assert
        Assert.Equal(TrnRequestMatchResultOutcome.PotentialMatches, results.Outcome);
    }

    [Fact]
    public async Task GetPotentialMatchingPersonsAsync_WithMatchingOnlyDateOfBirth_ReturnsNoMatches()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());
        var person2 = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());
        var row = new CapitaImportRecord()
        {
            DateOfBirth = person.DateOfBirth.ToString("yyyyMMdd"),
            DateOfDeath = null,
            Gender = (int)Gender.Male,
            FirstNameOrMiddleName = "Test",
            LastName = "test",
            NINumber = Faker.Identification.UkNationalInsuranceNumber(),
            PreviousLastName = null,
            TRN = "1234567"
        };

        // Act
        var results = await Job.GetPotentialMatchingPersonsAsync(row);

        // Assert
        Assert.Equal(TrnRequestMatchResultOutcome.NoMatches, results.Outcome);
    }

    public async Task InitializeAsync() => await DbFixture.DbHelper.ClearDataAsync();

    public Task DisposeAsync() => Task.CompletedTask;
}

public class CapitaImportJobFixture : IAsyncLifetime
{
    public CapitaImportJobFixture(
        DbFixture dbFixture,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator,
        IServiceProvider provider)
    {
        DbFixture = dbFixture;
        Clock = new();

        Logger = new Mock<ILogger<CapitaImportJob>>();

        var blobServiceClientMock = new Mock<BlobServiceClient>();
        var blobContainerClientMock = new Mock<BlobContainerClient>();
        var blobClientMock = new Mock<BlobClient>();
        blobServiceClientMock
            .Setup(b => b.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(blobContainerClientMock.Object);

        blobContainerClientMock
            .Setup(c => c.CreateIfNotExistsAsync(
                PublicAccessType.None,
                null,
                null,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(Mock.Of<Response<BlobContainerInfo>>());

        blobContainerClientMock
            .Setup(c => c.GetBlobClient(It.IsAny<string>()))
            .Returns(blobClientMock.Object);

        blobClientMock
            .Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<BlobContentInfo>>());

        var matchingService = new PersonMatchingService(dbFixture.GetDbContextFactory().CreateDbContext());
        var user = new CapitaTpsUserOption() { CapitaTpsUserId = ApplicationUser.CapitaTpsImportUser.UserId };
        var option = Options.Create(user);

        Job = ActivatorUtilities.CreateInstance<CapitaImportJob>(provider, blobServiceClientMock.Object, Logger.Object, Clock, matchingService!, option);
        TestData = new TestData(
            dbFixture.GetDbContextFactory(),
            referenceDataCache,
            Clock,
            trnGenerator);
    }

    public DbFixture DbFixture { get; }

    public TestData TestData { get; }

    public TestableClock Clock { get; }

    public Mock<ILogger<CapitaImportJob>> Logger { get; }

    async Task IAsyncLifetime.InitializeAsync() => await DbFixture.DbHelper.ClearDataAsync();

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    public CapitaImportJob Job { get; }

    public Mock<IFileService> BlobStorageFileService { get; } = new Mock<IFileService>();

    public Mock<BlobServiceClient> BlobServiceClient { get; } = new Mock<BlobServiceClient>();
}
