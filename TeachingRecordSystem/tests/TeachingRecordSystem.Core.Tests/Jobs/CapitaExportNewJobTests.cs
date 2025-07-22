using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.TrsDataSync;
using Xunit.DependencyInjection;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[DisableParallelization()]
public class CapitaExportNewJobTests : IClassFixture<CapitaExportNewJobFixture>
{
    public CapitaExportNewJobTests(CapitaExportNewJobFixture fixture)
    {
        Fixture = fixture;
    }

    private DbFixture DbFixture => Fixture.DbFixture;

    private IClock Clock => Fixture.Clock;

    private TestData TestData => Fixture.TestData;

    private CapitaExportNewJobFixture Fixture { get; }

    private CapitaExportNewJob Job => Fixture.Job;

    private const int EXPECTED_ROW_LENGTH = 86;

    private Mock<BlobServiceClient> BlobServiceClient => Fixture.BlobServiceClient;


    [Fact]
    public async Task GetPersons_CreatedAferLastRunDate_ReturnsExpectedRecords()
    {
        // Arrange
        await DbFixture.WithDbContextAsync(dbContext => dbContext.Persons.Where(x => x.CapitaTrnChangedOn != null).ExecuteDeleteAsync());
        var lastRunDate = DateTime.UtcNow.AddDays(-2);
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithCapitaTrnChangedOn(lastRunDate.AddDays(1));
            x.WithTrn();
        });
        var person2 = await TestData.CreatePersonAsync(x =>
        {
            x.WithCapitaTrnChangedOn(lastRunDate.AddHours(4));
            x.WithTrn();
        });

        // Act
        var newPersons = await Job.GetNewPersonsAsync(lastRunDate);

        // Assert
        Assert.Contains(newPersons, p => p.Trn == person1.Trn);
        Assert.Contains(newPersons, p => p.Trn == person2.Trn);
    }

    [Fact]
    public async Task GetPersons_NoRecordsCreatedAfterLastRunDate_ReturnsExpectedRecords()
    {
        // Arrange
        await DbFixture.WithDbContextAsync(dbContext => dbContext.Persons.Where(x => x.CapitaTrnChangedOn != null).ExecuteDeleteAsync());
        var lastRunDate = DateTime.UtcNow.AddDays(-2);
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            // before last run date
            x.WithCapitaTrnChangedOn(lastRunDate.AddDays(-3));
            x.WithTrn();
        });

        // Act
        var newPersons = await Job.GetNewPersonsAsync(lastRunDate);

        // Assert
        Assert.Empty(newPersons);
    }

    [Fact]
    public async Task GetPersons_CreatedAferLastRunDateWithoutTrn_ReturnsExpectedRecords()
    {
        // Arrange
        await DbFixture.WithDbContextAsync(dbContext => dbContext.Persons.Where(x => x.CapitaTrnChangedOn != null).ExecuteDeleteAsync());
        var lastRunDate = DateTime.UtcNow.AddDays(-2);
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithCapitaTrnChangedOn(lastRunDate.AddDays(1));
        });
        var person2 = await TestData.CreatePersonAsync(x =>
        {
            x.WithCapitaTrnChangedOn(lastRunDate.AddHours(4));
        });

        // Act
        var newPersons = await Job.GetNewPersonsAsync(lastRunDate);

        // Assert
        Assert.Empty(newPersons);
    }

    [Theory]
    [InlineData(Gender.Male, "1")]
    [InlineData(Gender.Female, "2")]
    [InlineData(Gender.Other, " ")]
    [InlineData(Gender.NotAvailable, " ")]
    public async Task GetNewPersonAsStringRow_ValidPersonWithGender_ReturnsExpectedContent(Gender gender, string expectedGenderCode)
    {
        // Arrange
        await DbFixture.WithDbContextAsync(dbContext => dbContext.Persons.Where(x => x.CapitaTrnChangedOn != null).ExecuteDeleteAsync());
        var lastRunDate = DateTime.UtcNow.AddDays(-2);
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithCapitaTrnChangedOn(lastRunDate.AddDays(1));
            x.WithTrn();
            x.WithGender(gender);
        });
        var trsPerson = await DbFixture.WithDbContextAsync(async dbContext => await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId));

        // Act
        var rowString = Job.GetNewPersonAsStringRow(trsPerson);

        // Assert
        var name = $"{trsPerson.FirstName} {trsPerson.MiddleName}".PadRight(35, ' ');
        var expectedRowString = $"{trsPerson.Trn}{expectedGenderCode}{new string(' ', 9)}{trsPerson.DateOfBirth!.Value.ToString("ddMMyy")}{new string(' ', 1)}{trsPerson.LastName.PadRight(17, ' ')}{new string(' ', 1)}{name}{new string(' ', 1)}{"1018Z981"}";
        Assert.NotNull(rowString);
        Assert.Equal(expectedRowString, rowString);
        Assert.Equal(EXPECTED_ROW_LENGTH, rowString.Length);
    }

    [Theory]
    [InlineData("lastName")]
    [InlineData("superduperlonglengthlastname")]
    public async Task GetNewPersonAsStringRow_LastNameLengths_ReturnsExpectedContent(string lastName)
    {
        // Arrange
        await DbFixture.WithDbContextAsync(dbContext => dbContext.Persons.Where(x => x.CapitaTrnChangedOn != null).ExecuteDeleteAsync());
        Gender gender = Gender.Male;
        var lastRunDate = DateTime.UtcNow.AddDays(-2);
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithCapitaTrnChangedOn(lastRunDate.AddDays(1));
            x.WithTrn();
            x.WithGender(gender);
            x.WithLastName(lastName);
        });
        var trsPerson = await DbFixture.WithDbContextAsync(async dbContext => await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId));

        // Act
        var rowString = Job.GetNewPersonAsStringRow(trsPerson);

        // Assert
        var name = $"{trsPerson.FirstName} {trsPerson.MiddleName}".PadRight(35, ' ');
        var expectedLastName = trsPerson.LastName.Length > 17 ? trsPerson.LastName.Substring(0, 17) : trsPerson.LastName.PadRight(17, ' ');
        var expectedRowString = $"{trsPerson.Trn}{(int)gender}{new string(' ', 9)}{trsPerson.DateOfBirth!.Value.ToString("ddMMyy")}{new string(' ', 1)}{expectedLastName}{new string(' ', 1)}{name}{new string(' ', 1)}{"1018Z981"}";
        Assert.NotNull(rowString);
        Assert.Equal(expectedRowString, rowString);
        Assert.Equal(EXPECTED_ROW_LENGTH, rowString.Length);
    }

    [Fact]
    public async Task GetNewPersonAsStringRow_ValidPersonWithoutDateOfBirth_ReturnsExpectedContent()
    {
        // Arrange
        await DbFixture.WithDbContextAsync(dbContext => dbContext.Persons.Where(x => x.CapitaTrnChangedOn != null).ExecuteDeleteAsync());
        var lastRunDate = DateTime.UtcNow.AddDays(-2);
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithCapitaTrnChangedOn(lastRunDate.AddDays(1));
            x.WithTrn();
            x.WithGender(Gender.Male);
        });
        var trsPerson = await DbFixture.WithDbContextAsync(async dbContext => await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId));
        trsPerson.DateOfBirth = null;

        // Act
        var rowString = Job.GetNewPersonAsStringRow(trsPerson);

        // Assert
        var name = $"{trsPerson.FirstName} {trsPerson.MiddleName}".PadRight(35, ' ');
        var expectedRowString = $"{trsPerson.Trn}{(int)Gender.Male}{new string(' ', 9)}{new string(' ', 6)}{new string(' ', 1)}{trsPerson.LastName.PadRight(17, ' ')}{new string(' ', 1)}{name}{new string(' ', 1)}{"1018Z981"}";
        Assert.NotNull(rowString);
        Assert.Equal(expectedRowString, rowString);
        Assert.Equal(EXPECTED_ROW_LENGTH, rowString.Length);
    }

    [Fact]
    public async Task GetNewPersonAsStringRow_ValidPersonWithoutMiddleName_ReturnsExpectedContent()
    {
        // Arrange
        await DbFixture.WithDbContextAsync(dbContext => dbContext.Persons.Where(x => x.CapitaTrnChangedOn != null).ExecuteDeleteAsync());
        var lastRunDate = DateTime.UtcNow.AddDays(-2);
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithCapitaTrnChangedOn(lastRunDate.AddDays(1));
            x.WithTrn();
            x.WithGender(Gender.Male);
        });
        var trsPerson = await DbFixture.WithDbContextAsync(async dbContext => await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId));

        trsPerson.DateOfBirth = null;
        trsPerson.MiddleName = string.Empty;

        // Act
        var rowString = Job.GetNewPersonAsStringRow(trsPerson);

        // Assert
        var name = $"{trsPerson.FirstName}".PadRight(35, ' ');
        var expectedRowString = $"{trsPerson.Trn}{(int)Gender.Male}{new string(' ', 9)}{new string(' ', 6)}{new string(' ', 1)}{trsPerson.LastName.PadRight(17, ' ')}{new string(' ', 1)}{name}{new string(' ', 1)}{"1018Z981"}";
        Assert.NotNull(rowString);
        Assert.Equal(expectedRowString, rowString);
        Assert.Equal(EXPECTED_ROW_LENGTH, rowString.Length);
    }

    [Fact]
    public async Task GetNewPersonAsStringRow_ValidPersonWithoutMiddleName_ReturnsExpectedTrimmedMiddleName()
    {
        // Arrange
        await DbFixture.WithDbContextAsync(dbContext => dbContext.Persons.Where(x => x.CapitaTrnChangedOn != null).ExecuteDeleteAsync());
        var lastRunDate = DateTime.UtcNow.AddDays(-2);
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithCapitaTrnChangedOn(lastRunDate.AddDays(1));
            x.WithTrn();
            x.WithGender(Gender.Male);
        });
        var trsPerson = await DbFixture.WithDbContextAsync(async dbContext => await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId));

        trsPerson.DateOfBirth = null;
        trsPerson.MiddleName = new string('c', 40);

        // Act
        var rowString = Job.GetNewPersonAsStringRow(trsPerson);

        // Assert
        var name = $"{trsPerson.FirstName} {trsPerson.MiddleName}".Substring(0, 35); //Name is trimmed to 35 chars
        var expectedRowString = $"{trsPerson.Trn}{(int)Gender.Male}{new string(' ', 9)}{new string(' ', 6)}{new string(' ', 1)}{trsPerson.LastName.PadRight(17, ' ')}{new string(' ', 1)}{name}{new string(' ', 1)}{"1018Z981"}";
        Assert.NotNull(rowString);
        Assert.Equal(expectedRowString, rowString);
        Assert.Equal(EXPECTED_ROW_LENGTH, rowString.Length);
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    public async Task GetNewPersonWithPreviousLastNameAsStringRow_WithValidPreviousName_ReturnsExpectedContent(Gender gender)
    {
        // Arrange
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            await dbContext.Persons.Where(x => x.CapitaTrnChangedOn != null).ExecuteDeleteAsync();
            var lastRunDate = DateTime.UtcNow.AddDays(-2);
            var newLastName = Faker.Name.Last();
            var originalastName = Faker.Name.Last();
            var person1 = await TestData.CreatePersonAsync(x =>
            {
                x.WithCapitaTrnChangedOn(lastRunDate.AddDays(1));
                x.WithTrn();
                x.WithGender(gender);
                x.WithLastName(originalastName);
            });
            var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);
            trsPerson.UpdateDetails(person1.FirstName, person1.MiddleName, newLastName, null, null, null, null, gender, "testing", null, "testing", null, null, SystemUser.SystemUserId, Clock.UtcNow, out var @nameChangeEvent);
            await dbContext.AddEventAndBroadcastAsync(@nameChangeEvent!);
            await dbContext.SaveChangesAsync();

            // Act
            var rowString = Job.GetNewPersonWithPreviousLastNameAsStringRow(trsPerson);

            // Assert
            var expectedRow = $"{trsPerson.Trn}{(int)gender}{new string(' ', 9)}{originalastName.PadRight(54, ' ')}{new string(' ', 7)}{"2018Z981"}";
            Assert.Equal(expectedRow, rowString);
            Assert.Equal(EXPECTED_ROW_LENGTH, rowString.Length);
        });
    }

    [Fact]
    public async Task GetNewPersonWithPreviousLastNameAsStringRow_WithoutGender_ReturnsExpectedContent()
    {
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            // Arrange
            await dbContext.Persons.Where(x => x.CapitaTrnChangedOn != null).ExecuteDeleteAsync();
            var lastRunDate = DateTime.UtcNow.AddDays(-2);
            var previousNameCreatedOn = Clock.UtcNow.AddHours(-1);
            var newLastName = Faker.Name.Last();
            var originalastName = Faker.Name.Last();
            var person1 = await TestData.CreatePersonAsync(x =>
            {
                x.WithCapitaTrnChangedOn(lastRunDate.AddDays(1));
                x.WithTrn();
                x.WithLastName(originalastName);
            });
            var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);
            trsPerson.UpdateDetails(person1.FirstName, person1.MiddleName, newLastName, null, null, null, null, null, "testing", null, "testing", null, null, SystemUser.SystemUserId, Clock.UtcNow, out var @nameChangeEvent);
            trsPerson.Gender = null;
            await dbContext.AddEventAndBroadcastAsync(@nameChangeEvent!);
            await dbContext.SaveChangesAsync();

            // Act
            var rowString = Job.GetNewPersonWithPreviousLastNameAsStringRow(trsPerson);

            // Assert
            var expectedRow = $"{trsPerson.Trn}{new string(' ', 1)}{new string(' ', 9)}{originalastName.PadRight(54, ' ')}{new string(' ', 7)}{"2018Z981"}";
            Assert.Equal(expectedRow, rowString);
            Assert.Equal(EXPECTED_ROW_LENGTH, rowString.Length);
        });
    }

    [Fact]
    public async Task GetNewPersonWithPreviousLastNameAsStringRow_WithLastNameExceedingMaxLength_ReturnsExpectedContent()
    {
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            // Arrange
            await dbContext.Persons.Where(x => x.CapitaTrnChangedOn != null).ExecuteDeleteAsync();
            var lastRunDate = DateTime.UtcNow.AddDays(-2);
            var newLastName = new string('x', 60);
            var originalastName = new string('a', 75);
            var person1 = await TestData.CreatePersonAsync(x =>
            {
                x.WithCapitaTrnChangedOn(lastRunDate.AddDays(1));
                x.WithTrn();
                x.WithLastName(originalastName);
            });
            var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);
            trsPerson.UpdateDetails(person1.FirstName, person1.MiddleName, newLastName, null, null, null, null, null, "testing", null, "testing", null, null, SystemUser.SystemUserId, Clock.UtcNow, out var @nameChangeEvent);
            await dbContext.AddEventAndBroadcastAsync(@nameChangeEvent!);
            await dbContext.SaveChangesAsync();

            // Act
            var rowString = Job.GetNewPersonWithPreviousLastNameAsStringRow(trsPerson);

            // Assert
            var expectedRow = $"{trsPerson.Trn}{new string(' ', 1)}{new string(' ', 9)}{person1.LastName.Substring(0, 54)}{new string(' ', 7)}{"2018Z981"}";
            Assert.Equal(expectedRow, rowString);
            Assert.Equal(EXPECTED_ROW_LENGTH, rowString.Length);
        });
    }

    [Fact]
    public async Task GetNewPersonWithPreviousLastNameAsStringRow_WithMultipleLastNameChanges_ReturnsExpectedContent()
    {
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            // Arrange
            await dbContext.Persons.Where(x => x.CapitaTrnChangedOn != null).ExecuteDeleteAsync();
            var lastRunDate = Clock.UtcNow.AddDays(-2);
            var updateLastName1 = Faker.Name.Last();
            var updateLastName2 = Faker.Name.Last();
            var updateLastName3 = Faker.Name.Last();
            var originalastName = new string('a', 75);
            var person1 = await TestData.CreatePersonAsync(x =>
            {
                x.WithCapitaTrnChangedOn(lastRunDate.AddDays(1));
                x.WithTrn();
                x.WithLastName(originalastName);
            });
            var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);

            // first marriage
            trsPerson.UpdateDetails(person1.FirstName, person1.MiddleName, updateLastName1, null, null, null, null, null, "testing", null, "testing", null, null, SystemUser.SystemUserId, Clock.UtcNow.AddYears(-3), out var @nameChangeEvent1);
            await dbContext.AddEventAndBroadcastAsync(@nameChangeEvent1!);

            // second marriage
            trsPerson.UpdateDetails(person1.FirstName, person1.MiddleName, updateLastName2, null, null, null, null, null, "testing", null, "testing", null, null, SystemUser.SystemUserId, Clock.UtcNow.AddYears(-1), out var @nameChangeEvent2);
            await dbContext.AddEventAndBroadcastAsync(@nameChangeEvent2!);

            // third marriage
            trsPerson.UpdateDetails(person1.FirstName, person1.MiddleName, updateLastName3, null, null, null, null, null, "testing", null, "testing", null, null, SystemUser.SystemUserId, Clock.UtcNow.AddHours(-10), out var @nameChangeEvent3);
            await dbContext.AddEventAndBroadcastAsync(@nameChangeEvent3!);
            await dbContext.SaveChangesAsync();

            // Act
            var rowString = Job.GetNewPersonWithPreviousLastNameAsStringRow(trsPerson);

            // Assert
            var expectedRow = $"{trsPerson.Trn}{new string(' ', 1)}{new string(' ', 9)}{updateLastName2.PadRight(54, ' ')}{new string(' ', 7)}{"2018Z981"}";
            Assert.Equal(expectedRow, rowString);
            Assert.Equal(EXPECTED_ROW_LENGTH, rowString.Length);
        });
    }
}

public class CapitaExportNewJobFixture : IAsyncLifetime
{
    public CapitaExportNewJobFixture(
        DbFixture dbFixture,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator,
        IServiceProvider provider,
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
    {
        OrganizationService = provider.GetService<IOrganizationServiceAsync2>()!;
        DbFixture = dbFixture;
        Clock = new();
        Helper = new TrsDataSyncHelper(
            dbFixture.GetDataSource(),
            OrganizationService,
            referenceDataCache,
            Clock,
            new TestableAuditRepository(),
            loggerFactory.CreateLogger<TrsDataSyncHelper>(),
            BlobStorageFileService.Object,
            configuration);

        Logger = new Mock<ILogger<CapitaExportNewJob>>();
        Job = ActivatorUtilities.CreateInstance<CapitaExportNewJob>(provider, BlobServiceClient.Object, Logger.Object, Clock);
        TestData = new TestData(
            dbFixture.GetDbContextFactory(),
            OrganizationService,
            referenceDataCache,
            Clock,
            trnGenerator,
            TestDataPersonDataSource.CrmAndTrs);
    }

    public DbFixture DbFixture { get; }

    public TestData TestData { get; }

    public TestableClock Clock { get; }

    public TrsDataSyncHelper Helper { get; }

    public Mock<ILogger<CapitaExportNewJob>> Logger { get; }

    Task IAsyncLifetime.InitializeAsync() => DbFixture.WithDbContextAsync(dbContext => dbContext.Persons.ExecuteDeleteAsync());

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    public IOrganizationServiceAsync2 OrganizationService { get; }

    public CapitaExportNewJob Job { get; }

    public Mock<IFileService> BlobStorageFileService { get; } = new Mock<IFileService>();

    public Mock<BlobServiceClient> BlobServiceClient { get; } = new Mock<BlobServiceClient>();
}
