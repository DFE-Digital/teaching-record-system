using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.Persons;
using PersonDetailsUpdatedEvent = TeachingRecordSystem.Core.Events.Legacy.PersonDetailsUpdatedEvent;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection(nameof(DisableParallelization))]
public class CapitaExportNewJobTests(CapitaExportNewJobFixture Fixture) : IClassFixture<CapitaExportNewJobFixture>, IAsyncLifetime
{
    private DbFixture DbFixture => Fixture.DbFixture;

    private IClock Clock => Fixture.Clock;

    private TestData TestData => Fixture.TestData;

    private const int EXPECTED_ROW_LENGTH = 86;


    [Fact]
    public async Task GetNewPersons_CreatedAfterLastRunDate_ReturnsExpectedRecords()
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var Job = new CapitaExportNewJob(Fixture.DataLakeServiceClient.Object, Fixture.Logger.Object, dbContext, Clock);
        var jobMetaData = new JobMetadata()
        {
            JobName = nameof(CapitaExportNewJob),
            Metadata = new Dictionary<string, string>
                {
                    {
                        "LastRunDate", Clock.UtcNow.AddDays(-3).ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                    }
                }
        };
        dbContext.JobMetadata.Add(jobMetaData);
        await dbContext.SaveChangesAsync();
        var lastRunDate = Clock.UtcNow.AddDays(-2);
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithGender(Gender.Male);
        });
        var person2 = await TestData.CreatePersonAsync(x =>
        {
            x.WithGender(Gender.Male);
        });

        // Act
        var newPersons = await Job.GetNewPersonsAsync(lastRunDate);

        // Assert
        Assert.Contains(newPersons, p => p.Trn == person1.Trn);
        Assert.Contains(newPersons, p => p.Trn == person2.Trn);
    }

    [Theory]
    [InlineData(Gender.NotAvailable)]
    [InlineData(Gender.Other)]
    public async Task GetNewPersons_WithNoneValidGenders_ReturnsExpectedRecords(Gender gender)
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var Job = new CapitaExportNewJob(Fixture.DataLakeServiceClient.Object, Fixture.Logger.Object, dbContext, Clock);
        var jobMetaData = new JobMetadata()
        {
            JobName = nameof(CapitaExportNewJob),
            Metadata = new Dictionary<string, string>
                {
                    {
                        "LastRunDate", Clock.UtcNow.AddDays(-3).ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                    }
                }
        };
        dbContext.JobMetadata.Add(jobMetaData);
        await dbContext.SaveChangesAsync();
        var lastRunDate = DateTime.UtcNow.AddDays(-2);
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithGender(gender);
        });

        // Act
        var newPersons = await Job.GetNewPersonsAsync(lastRunDate);

        // Assert
        Assert.DoesNotContain(newPersons, p => p.Trn == person1.Trn);
    }

    [Fact]
    public async Task GetNewPersons_NoRecordsCreatedAfterLastRunDate_ReturnsExpectedRecords()
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var Job = new CapitaExportNewJob(Fixture.DataLakeServiceClient.Object, Fixture.Logger.Object, dbContext, Clock);
        var lastRunDate = Clock.UtcNow.AddDays(1);

        // Act
        var newPersons = await Job.GetNewPersonsAsync(lastRunDate);

        // Assert
        Assert.Empty(newPersons);
    }

    [Fact]
    public async Task GetNewPersons_CreatedAferLastRunDateWithoutTrn_ReturnsExpectedRecords()
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var Job = new CapitaExportNewJob(Fixture.DataLakeServiceClient.Object, Fixture.Logger.Object, dbContext, Clock);
        var lastRunDate = DateTime.UtcNow.AddDays(-2);
        var person1 = await TestData.CreatePersonAsync();
        var person2 = await TestData.CreatePersonAsync();

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
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var Job = new CapitaExportNewJob(Fixture.DataLakeServiceClient.Object, Fixture.Logger.Object, dbContext, Clock);
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithGender(gender);
        });
        var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);

        // Act
        var rowString = Job.GetNewPersonAsStringRow(trsPerson, false);

        // Assert
        var name = $"{trsPerson.FirstName} {trsPerson.MiddleName}".PadRight(35, ' ');
        var expectedRowString = $"{trsPerson.Trn}" +
            $"{expectedGenderCode}" +
            $"{new string(' ', 9)}" +
            $"{trsPerson.DateOfBirth!.Value.ToString("ddMMyy")}" +
            $"{new string(' ', 1)}" +
            $"{trsPerson.LastName.PadRight(17, ' ')}" +
            $"{new string(' ', 1)}" +
            $"{name}" +
            $"{new string(' ', 1)}" +
            $"{"1018Z981"}";
        Assert.NotNull(rowString);
        Assert.Equal(expectedRowString, rowString);
        Assert.Equal(EXPECTED_ROW_LENGTH, rowString.Length);
    }

    [Theory]
    [InlineData(Gender.Male, "1")]
    [InlineData(Gender.Female, "2")]
    public async Task GetNewPersonAsStringRow_ValidPersonWithPreviousName_ReturnsExpectedContent(Gender gender, string expectedGenderCode)
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var Job = new CapitaExportNewJob(Fixture.DataLakeServiceClient.Object, Fixture.Logger.Object, dbContext, Clock);
        var newLastName = Faker.Name.Last();
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithGender(gender);
        });
        var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);
        var personService = new PersonService(
            dbContext,
            Clock,
            Mock.Of<IEventPublisher>());
        var processContext = new ProcessContext(ProcessType.PersonDetailsUpdating, Clock.UtcNow, SystemUser.SystemUserId);
        var updateresult1 = await personService.UpdatePersonDetailsAsync(new(
            person1.PersonId,
            new PersonDetails()
            {
                FirstName = person1.FirstName,
                MiddleName = person1.MiddleName,
                LastName = person1.LastName,
                DateOfBirth = person1.DateOfBirth,
                EmailAddress = null,
                NationalInsuranceNumber = null,
                Gender = person1.Gender
            }.UpdateAll(),
            null,
            null),
            processContext);
        var nameChangeEvent = new PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow.AddYears(-2),
            RaisedBy = SystemUser.SystemUserId,
            PersonId = trsPerson.PersonId,
            PersonAttributes = updateresult1.PersonDetails.ToEventModel(),
            OldPersonAttributes = updateresult1.OldPersonDetails.ToEventModel(),
            NameChangeReason = "",
            NameChangeEvidenceFile = null,
            DetailsChangeReason = null,
            DetailsChangeReasonDetail = null,
            DetailsChangeEvidenceFile = null,
            Changes = updateresult1.Changes.ToLegacyPersonDetailsUpdatedEventChanges()
        };
        dbContext.AddEventWithoutBroadcast(nameChangeEvent);
        await dbContext.SaveChangesAsync();

        // Act
        var rowString = Job.GetNewPersonAsStringRow(trsPerson, true);

        // Assert
        var name = $"{trsPerson.FirstName} {trsPerson.MiddleName}".PadRight(35, ' ');
        var expectedRowString = $"{trsPerson.Trn}" +
            $"{expectedGenderCode}" +
            $"{new string(' ', 9)}" +
            $"{trsPerson.DateOfBirth!.Value.ToString("ddMMyy")}" +
            $"{new string(' ', 1)}" +
            $"{trsPerson.LastName.PadRight(17, ' ')}" +
            $"{new string('1', 1)}" + //1 if has a previous name
            $"{name}" +
            $"{new string(' ', 1)}" +
            $"{"1018Z981"}";
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
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var Job = new CapitaExportNewJob(Fixture.DataLakeServiceClient.Object, Fixture.Logger.Object, dbContext, Clock);
        Gender gender = Gender.Male;
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithGender(gender);
            x.WithLastName(lastName);
        });
        var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);

        // Act
        var rowString = Job.GetNewPersonAsStringRow(trsPerson, false);

        // Assert
        var name = $"{trsPerson.FirstName} {trsPerson.MiddleName}".PadRight(35, ' ');
        var expectedLastName = trsPerson.LastName.Length > 17 ? trsPerson.LastName.Substring(0, 17) : trsPerson.LastName.PadRight(17, ' ');
        var expectedRowString = $"{trsPerson.Trn}" +
            $"{(int)gender}" +
            $"{new string(' ', 9)}" +
            $"{trsPerson.DateOfBirth!.Value.ToString("ddMMyy")}" +
            $"{new string(' ', 1)}" +
            $"{expectedLastName}" +
            $"{new string(' ', 1)}" +
            $"{name}{new string(' ', 1)}" +
            $"{"1018Z981"}";
        Assert.NotNull(rowString);
        Assert.Equal(expectedRowString, rowString);
        Assert.Equal(EXPECTED_ROW_LENGTH, rowString.Length);
    }

    [Fact]
    public async Task GetNewPersonAsStringRow_ValidPersonWithoutDateOfBirth_ReturnsExpectedContent()
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var Job = new CapitaExportNewJob(Fixture.DataLakeServiceClient.Object, Fixture.Logger.Object, dbContext, Clock);
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithGender(Gender.Male);
        });
        var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);
        trsPerson.DateOfBirth = null;

        // Act
        var rowString = Job.GetNewPersonAsStringRow(trsPerson, false);

        // Assert
        var name = $"{trsPerson.FirstName} {trsPerson.MiddleName}".PadRight(35, ' ');
        var expectedRowString = $"{trsPerson.Trn}" +
            $"{(int)Gender.Male}" +
            $"{new string(' ', 9)}" +
            $"{new string(' ', 6)}" +
            $"{new string(' ', 1)}" +
            $"{trsPerson.LastName.PadRight(17, ' ')}" +
            $"{new string(' ', 1)}" +
            $"{name}" +
            $"{new string(' ', 1)}" +
            $"{"1018Z981"}";
        Assert.NotNull(rowString);
        Assert.Equal(expectedRowString, rowString);
        Assert.Equal(EXPECTED_ROW_LENGTH, rowString.Length);
    }

    [Fact]
    public async Task GetNewPersonAsStringRow_ValidPersonWithoutMiddleName_ReturnsExpectedContent()
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var Job = new CapitaExportNewJob(Fixture.DataLakeServiceClient.Object, Fixture.Logger.Object, dbContext, Clock);
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithGender(Gender.Male);
        });
        var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);

        trsPerson.DateOfBirth = null;
        trsPerson.MiddleName = string.Empty;

        // Act
        var rowString = Job.GetNewPersonAsStringRow(trsPerson, false);

        // Assert
        var name = $"{trsPerson.FirstName}".PadRight(35, ' ');
        var expectedRowString = $"{trsPerson.Trn}" +
            $"{(int)Gender.Male}" +
            $"{new string(' ', 9)}" +
            $"{new string(' ', 6)}" +
            $"{new string(' ', 1)}" +
            $"{trsPerson.LastName.PadRight(17, ' ')}" +
            $"{new string(' ', 1)}" +
            $"{name}" +
            $"{new string(' ', 1)}" +
            $"{"1018Z981"}";
        Assert.NotNull(rowString);
        Assert.Equal(expectedRowString, rowString);
        Assert.Equal(EXPECTED_ROW_LENGTH, rowString.Length);
    }

    [Fact]
    public async Task GetNewPersonAsStringRow_ValidPersonWithoutMiddleName_ReturnsExpectedTrimmedMiddleName()
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var Job = new CapitaExportNewJob(Fixture.DataLakeServiceClient.Object, Fixture.Logger.Object, dbContext, Clock);
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithGender(Gender.Male);
        });
        var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);

        trsPerson.DateOfBirth = null;
        trsPerson.MiddleName = new string('c', 40);

        // Act
        var rowString = Job.GetNewPersonAsStringRow(trsPerson, false);

        // Assert
        var name = $"{trsPerson.FirstName} {trsPerson.MiddleName}".Substring(0, 35); //Name is trimmed to 35 chars
        var expectedRowString = $"{trsPerson.Trn}" +
            $"{(int)Gender.Male}" +
            $"{new string(' ', 9)}" +
            $"{new string(' ', 6)}" +
            $"{new string(' ', 1)}" +
            $"{trsPerson.LastName.PadRight(17, ' ')}" +
            $"{new string(' ', 1)}" +
            $"{name}" +
            $"{new string(' ', 1)}" +
            $"{"1018Z981"}";
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
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var Job = new CapitaExportNewJob(Fixture.DataLakeServiceClient.Object, Fixture.Logger.Object, dbContext, Clock);
        var newLastName = Faker.Name.Last();
        var originalastName = Faker.Name.Last();
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithGender(gender);
            x.WithLastName(originalastName);
        });
        var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);
        var personService = new PersonService(
            dbContext,
            Clock,
            Mock.Of<IEventPublisher>());
        var processContext = new ProcessContext(ProcessType.PersonDetailsUpdating, Clock.UtcNow, SystemUser.SystemUserId);
        var updateresult1 = await personService.UpdatePersonDetailsAsync(new(
            person1.PersonId,
            new PersonDetails()
            {
                FirstName = person1.FirstName,
                MiddleName = person1.MiddleName,
                LastName = newLastName,
                DateOfBirth = person1.DateOfBirth,
                EmailAddress = null,
                NationalInsuranceNumber = null,
                Gender = person1.Gender
            }.UpdateAll(),
            null,
            null),
            processContext);
        var nameChangeEvent = new PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow.AddYears(-2),
            RaisedBy = SystemUser.SystemUserId,
            PersonId = trsPerson.PersonId,
            PersonAttributes = updateresult1.PersonDetails.ToEventModel(),
            OldPersonAttributes = updateresult1.OldPersonDetails.ToEventModel(),
            NameChangeReason = "",
            NameChangeEvidenceFile = null,
            DetailsChangeReason = null,
            DetailsChangeReasonDetail = null,
            DetailsChangeEvidenceFile = null,
            Changes = updateresult1.Changes.ToLegacyPersonDetailsUpdatedEventChanges()
        };
        dbContext.AddEventWithoutBroadcast(nameChangeEvent);
        await dbContext.SaveChangesAsync();

        // Act
        var rowString = await Job.GetNewPersonWithPreviousLastNameAsStringRowAsync(trsPerson, CancellationToken.None);

        // Assert
        var expectedRow = $"{trsPerson.Trn}" +
            $"{(int)gender}" +
            $"{new string(' ', 9)}" +
            $"{originalastName.PadRight(54, ' ')}" +
            $"{new string(' ', 7)}" +
            $"{"2018Z981"}";
        Assert.Equal(expectedRow, rowString);
        Assert.Equal(EXPECTED_ROW_LENGTH, rowString.Length);
    }

    [Fact]
    public async Task GetNewPersonWithPreviousLastNameAsStringRow_WithoutGender_ReturnsExpectedContent()
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var Job = new CapitaExportNewJob(Fixture.DataLakeServiceClient.Object, Fixture.Logger.Object, dbContext, Clock);
        var previousNameCreatedOn = Clock.UtcNow.AddHours(-1);
        var newLastName = Faker.Name.Last();
        var originalastName = Faker.Name.Last();
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithLastName(originalastName);
        });
        var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);
        var personService = new PersonService(
            dbContext,
            Clock,
            Mock.Of<IEventPublisher>());
        var processContext = new ProcessContext(ProcessType.PersonDetailsUpdating, Clock.UtcNow, SystemUser.SystemUserId);
        var updateresult1 = await personService.UpdatePersonDetailsAsync(new(
            person1.PersonId,
            new PersonDetails()
            {
                FirstName = person1.FirstName,
                MiddleName = person1.MiddleName,
                LastName = newLastName,
                DateOfBirth = person1.DateOfBirth,
                EmailAddress = null,
                NationalInsuranceNumber = null,
                Gender = person1.Gender
            }.UpdateAll(),
            null,
            null),
            processContext);
        var nameChangeEvent = new PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow.AddYears(-2),
            RaisedBy = SystemUser.SystemUserId,
            PersonId = trsPerson.PersonId,
            PersonAttributes = updateresult1.PersonDetails.ToEventModel(),
            OldPersonAttributes = updateresult1.OldPersonDetails.ToEventModel(),
            NameChangeReason = "",
            NameChangeEvidenceFile = null,
            DetailsChangeReason = null,
            DetailsChangeReasonDetail = null,
            DetailsChangeEvidenceFile = null,
            Changes = updateresult1.Changes.ToLegacyPersonDetailsUpdatedEventChanges()
        };
        dbContext.AddEventWithoutBroadcast(nameChangeEvent);
        await dbContext.SaveChangesAsync();

        // Act
        var rowString = await Job.GetNewPersonWithPreviousLastNameAsStringRowAsync(trsPerson, CancellationToken.None);

        // Assert
        var expectedRow = $"{trsPerson.Trn}" +
            $"{new string(' ', 1)}" +
            $"{new string(' ', 9)}" +
            $"{originalastName.PadRight(54, ' ')}" +
            $"{new string(' ', 7)}" +
            $"{"2018Z981"}";
        Assert.Equal(expectedRow, rowString);
        Assert.Equal(EXPECTED_ROW_LENGTH, rowString.Length);
    }

    [Fact]
    public async Task GetNewPersonWithPreviousLastNameAsStringRow_WithLastNameExceedingMaxLength_ReturnsExpectedContent()
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var Job = new CapitaExportNewJob(Fixture.DataLakeServiceClient.Object, Fixture.Logger.Object, dbContext, Clock);
        var newLastName = new string('x', 60);
        var originalastName = new string('a', 75);
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithLastName(originalastName);
        });
        var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);
        var personService = new PersonService(
            dbContext,
            Clock,
            Mock.Of<IEventPublisher>());
        var processContext = new ProcessContext(ProcessType.PersonDetailsUpdating, Clock.UtcNow, SystemUser.SystemUserId);
        var updateresult1 = await personService.UpdatePersonDetailsAsync(new(
            person1.PersonId,
            new PersonDetails()
            {
                FirstName = person1.FirstName,
                MiddleName = person1.MiddleName,
                LastName = newLastName,
                DateOfBirth = person1.DateOfBirth,
                EmailAddress = null,
                NationalInsuranceNumber = null,
                Gender = person1.Gender
            }.UpdateAll(),
            null,
            null),
            processContext);
        var nameChangeEvent = new PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow.AddYears(-2),
            RaisedBy = SystemUser.SystemUserId,
            PersonId = trsPerson.PersonId,
            PersonAttributes = updateresult1.PersonDetails.ToEventModel(),
            OldPersonAttributes = updateresult1.OldPersonDetails.ToEventModel(),
            NameChangeReason = "",
            NameChangeEvidenceFile = null,
            DetailsChangeReason = null,
            DetailsChangeReasonDetail = null,
            DetailsChangeEvidenceFile = null,
            Changes = updateresult1.Changes.ToLegacyPersonDetailsUpdatedEventChanges()
        };
        dbContext.AddEventWithoutBroadcast(nameChangeEvent);
        await dbContext.SaveChangesAsync();

        // Act
        var rowString = await Job.GetNewPersonWithPreviousLastNameAsStringRowAsync(trsPerson, CancellationToken.None);

        // Assert
        var expectedRow = $"{trsPerson.Trn}" +
            $"{new string(' ', 1)}" +
            $"{new string(' ', 9)}" +
            $"{person1.LastName.Substring(0, 54)}" +
            $"{new string(' ', 7)}" +
            $"{"2018Z981"}";
        Assert.Equal(expectedRow, rowString);
        Assert.Equal(EXPECTED_ROW_LENGTH, rowString.Length);
    }

    [Fact]
    public async Task GetNewPersonWithPreviousLastNameAsStringRow_WithMultipleLastNameChanges_ReturnsExpectedContent()
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var Job = new CapitaExportNewJob(Fixture.DataLakeServiceClient.Object, Fixture.Logger.Object, dbContext, Clock);
        var updateLastName1 = Faker.Name.Last();
        var updateLastName2 = Faker.Name.Last();
        var updateLastName3 = Faker.Name.Last();
        var originalastName = new string('a', 75);
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithLastName(originalastName);
        });
        var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);

        // first marriage
        var personService = new PersonService(
            dbContext,
            Clock,
            Mock.Of<IEventPublisher>());
        var processContext = new ProcessContext(ProcessType.PersonDetailsUpdating, Clock.UtcNow.AddYears(-3), SystemUser.SystemUserId);
        var updateresult1 = await personService.UpdatePersonDetailsAsync(new(
            person1.PersonId,
            new PersonDetails()
            {
                FirstName = person1.FirstName,
                MiddleName = person1.MiddleName,
                LastName = updateLastName1,
                DateOfBirth = person1.DateOfBirth,
                EmailAddress = null,
                NationalInsuranceNumber = null,
                Gender = person1.Gender
            }.UpdateAll(),
            null,
            null),
            processContext);
        var nameChangeEvent1 = new PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow.AddYears(-3),
            RaisedBy = SystemUser.SystemUserId,
            PersonId = trsPerson.PersonId,
            PersonAttributes = updateresult1.PersonDetails.ToEventModel(),
            OldPersonAttributes = updateresult1.OldPersonDetails.ToEventModel(),
            NameChangeReason = "",
            NameChangeEvidenceFile = null,
            DetailsChangeReason = null,
            DetailsChangeReasonDetail = null,
            DetailsChangeEvidenceFile = null,
            Changes = updateresult1.Changes.ToLegacyPersonDetailsUpdatedEventChanges()
        };
        dbContext.AddEventWithoutBroadcast(nameChangeEvent1);

        // second marriage
        var processContext2 = new ProcessContext(ProcessType.PersonDetailsUpdating, Clock.UtcNow.AddYears(-1), SystemUser.SystemUserId);
        var updateresult2 = await personService.UpdatePersonDetailsAsync(new(
            person1.PersonId,
            new PersonDetails()
            {
                FirstName = person1.FirstName,
                MiddleName = person1.MiddleName,
                LastName = updateLastName2,
                DateOfBirth = person1.DateOfBirth,
                EmailAddress = null,
                NationalInsuranceNumber = null,
                Gender = person1.Gender
            }.UpdateAll(),
            null,
            null),
            processContext2);
        var nameChangeEvent2 = new PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow.AddYears(-2),
            RaisedBy = SystemUser.SystemUserId,
            PersonId = trsPerson.PersonId,
            PersonAttributes = updateresult2.PersonDetails.ToEventModel(),
            OldPersonAttributes = updateresult2.OldPersonDetails.ToEventModel(),
            NameChangeReason = "",
            NameChangeEvidenceFile = null,
            DetailsChangeReason = null,
            DetailsChangeReasonDetail = null,
            DetailsChangeEvidenceFile = null,
            Changes = updateresult2.Changes.ToLegacyPersonDetailsUpdatedEventChanges()
        };
        dbContext.AddEventWithoutBroadcast(nameChangeEvent2);

        // third marriage
        var processContext3 = new ProcessContext(ProcessType.PersonDetailsUpdating, Clock.UtcNow.AddYears(-1), SystemUser.SystemUserId);
        var updateresult3 = await personService.UpdatePersonDetailsAsync(new(
            person1.PersonId,
            new PersonDetails()
            {
                FirstName = person1.FirstName,
                MiddleName = person1.MiddleName,
                LastName = updateLastName3,
                DateOfBirth = person1.DateOfBirth,
                EmailAddress = null,
                NationalInsuranceNumber = null,
                Gender = person1.Gender
            }.UpdateAll(),
            null,
            null),
            processContext3);
        var nameChangeEvent3 = new PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = trsPerson.PersonId,
            PersonAttributes = updateresult3.PersonDetails.ToEventModel(),
            OldPersonAttributes = updateresult3.OldPersonDetails.ToEventModel(),
            NameChangeReason = "",
            NameChangeEvidenceFile = null,
            DetailsChangeReason = null,
            DetailsChangeReasonDetail = null,
            DetailsChangeEvidenceFile = null,
            Changes = updateresult3.Changes.ToLegacyPersonDetailsUpdatedEventChanges()
        };
        dbContext.AddEventWithoutBroadcast(nameChangeEvent3);
        await dbContext.SaveChangesAsync();

        // Act
        var rowString = await Job.GetNewPersonWithPreviousLastNameAsStringRowAsync(trsPerson, CancellationToken.None);

        // Assert
        var expectedRow = $"{trsPerson.Trn}" +
            $"{new string(' ', 1)}" +
            $"{new string(' ', 9)}" +
            $"{updateLastName2.PadRight(54, ' ')}" +
            $"{new string(' ', 7)}" +
            $"{"2018Z981"}";
        Assert.Equal(expectedRow, rowString);
        Assert.Equal(EXPECTED_ROW_LENGTH, rowString.Length);
    }

    [Fact]
    public async Task Execute_NewRecordWithNoPreviousName_ReturnsExpectedContent()
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var Job = new CapitaExportNewJob(Fixture.DataLakeServiceClient.Object, Fixture.Logger.Object, dbContext, Clock);
        await dbContext.IntegrationTransactionRecords.ExecuteDeleteAsync();
        var expectedTotalRowCount = 1;
        var expectedTotalSuccessCount = 1;
        var expectedFailureCount = 0;
        var expectedDuplicateCount = 0;
        var jobMetaData = new JobMetadata()
        {
            JobName = nameof(CapitaExportNewJob),
            Metadata = new Dictionary<string, string>
                {
                    {
                        "LastRunDate", Clock.UtcNow.AddDays(-2).AddHours(-2).ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                    }
                }
        };
        dbContext.JobMetadata.Add(jobMetaData);
        await dbContext.SaveChangesAsync();
        var originalastName = Faker.Name.Last();
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithLastName(originalastName);
            x.WithGender(Gender.Male);
        });
        var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);
        await dbContext.SaveChangesAsync();

        // Act
        var integrationTransactionJobId = await Job.ExecuteAsync(CancellationToken.None);

        // Assert
        var integrationTransaction = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleAsync(x => x.IntegrationTransactionId == integrationTransactionJobId);
        var expectedRowContent = $"{trsPerson.Trn}" +
            $"{(int)trsPerson.Gender!}" +
            $"{new string(' ', 9)}" +
            $"{trsPerson.DateOfBirth!.Value.ToString("ddMMyy")}" +
            $"{new string(' ', 1)}" +
            $"{trsPerson.LastName.PadRight(17)}" +
            $"{new string(' ', 1)}" +
            $"{trsPerson.FirstName} {trsPerson.MiddleName}".PadRight(35) +
            $"{new string(' ', 1)}" +
            $"{"1018Z981"}";
        Assert.NotNull(integrationTransaction);
        Assert.Equal(IntegrationTransactionImportStatus.Success, integrationTransaction.ImportStatus);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedTotalSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedFailureCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedDuplicateCount, integrationTransaction.DuplicateCount);
        Assert.NotEmpty(integrationTransaction.FileName);
        Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, record =>
        {
            Assert.Empty(record.FailureMessage!);
            Assert.Equal(person1.PersonId, record.PersonId);
            Assert.Null(record.Duplicate);
            Assert.NotNull(record.RowData);
            Assert.Equal(EXPECTED_ROW_LENGTH, record.RowData!.Length);
            Assert.Equal(expectedRowContent, record.RowData);
            Assert.Equal(IntegrationTransactionRecordStatus.Success, record.Status);

            return true;
        });
    }

    [Fact]
    public async Task Execute_WithNoNewRecords_ReturnsExpectedContent()
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var expectedTotalRowCount = 0;
        var expectedTotalSuccessCount = 0;
        var expectedFailureCount = 0;
        var expectedDuplicateCount = 0;
        var jobMetaData = new JobMetadata()
        {
            JobName = nameof(CapitaExportNewJob),
            Metadata = new Dictionary<string, string>
                {
                    {
                        "LastRunDate", Clock.UtcNow.AddDays(-1).ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                    }
                }
        };
        dbContext.JobMetadata.Add(jobMetaData);
        var originalastName = Faker.Name.Last();
        await dbContext.SaveChangesAsync();
        var Job = new CapitaExportNewJob(Fixture.DataLakeServiceClient.Object, Fixture.Logger.Object, dbContext, Clock);

        // Act
        var integrationTransactionJobId = await Job.ExecuteAsync(CancellationToken.None);

        // Assert
        var integrationTransaction = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleAsync(x => x.IntegrationTransactionId == integrationTransactionJobId);
        Assert.NotNull(integrationTransaction);
        Assert.Equal(IntegrationTransactionImportStatus.Success, integrationTransaction.ImportStatus);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedTotalSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedFailureCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedDuplicateCount, integrationTransaction.DuplicateCount);
        Assert.NotEmpty(integrationTransaction.FileName);
        Assert.Empty(integrationTransaction.IntegrationTransactionRecords!);
    }

    [Fact]
    public async Task Execute_NewRecordWithPreviousName_ReturnsExpectedContent()
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var expectedTotalRowCount = 2;
        var expectedTotalSuccessCount = 2;
        var expectedFailureCount = 0;
        var expectedDuplicateCount = 0;
        var updateLastName = Faker.Name.Last();
        var jobMetaData = new JobMetadata()
        {
            JobName = nameof(CapitaExportNewJob),
            Metadata = new Dictionary<string, string>
                {
                    {
                        "LastRunDate", Clock.UtcNow.AddDays(-2).ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                    }
                }
        };
        dbContext.JobMetadata.Add(jobMetaData);
        await dbContext.SaveChangesAsync();
        var Job = new CapitaExportNewJob(Fixture.DataLakeServiceClient.Object, Fixture.Logger.Object, dbContext, Clock);
        var originalastName = Faker.Name.Last();
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithLastName(originalastName);
            x.WithGender(Gender.Male);
        });
        var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);
        var personService = new PersonService(
            dbContext,
            Clock,
            Mock.Of<IEventPublisher>());
        var processContext = new ProcessContext(ProcessType.PersonDetailsUpdating, Clock.UtcNow, SystemUser.SystemUserId);
        var updateresult1 = await personService.UpdatePersonDetailsAsync(new(
            person1.PersonId,
            new PersonDetails()
            {
                FirstName = person1.FirstName,
                MiddleName = person1.MiddleName,
                LastName = updateLastName,
                DateOfBirth = person1.DateOfBirth,
                EmailAddress = null,
                NationalInsuranceNumber = null,
                Gender = person1.Gender
            }.UpdateAll(),
            null,
            null),
            processContext);
        var nameChangeEvent1 = new PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = trsPerson.PersonId,
            PersonAttributes = updateresult1.PersonDetails.ToEventModel(),
            OldPersonAttributes = updateresult1.OldPersonDetails.ToEventModel(),
            NameChangeReason = "",
            NameChangeEvidenceFile = null,
            DetailsChangeReason = null,
            DetailsChangeReasonDetail = null,
            DetailsChangeEvidenceFile = null,
            Changes = updateresult1.Changes.ToLegacyPersonDetailsUpdatedEventChanges()
        };
        dbContext.AddEventWithoutBroadcast(nameChangeEvent1);
        await dbContext.SaveChangesAsync();

        // Act
        var integrationTransactionJobId = await Job.ExecuteAsync(CancellationToken.None);

        // Assert
        var integrationTransaction = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleAsync(x => x.IntegrationTransactionId == integrationTransactionJobId);
        var expectedNewRowContent = $"{trsPerson.Trn}{(int)trsPerson.Gender!}{new string(' ', 9)}{trsPerson.DateOfBirth!.Value.ToString("ddMMyy")}{new string(' ', 1)}{trsPerson.LastName.PadRight(17)}{new string('1', 1)}{$"{trsPerson.FirstName} {trsPerson.MiddleName}".PadRight(35)}{new string(' ', 1)}{"1018Z981"}";

        var expectedNameChangeRow = $"{trsPerson.Trn}" +
            $"{(int)trsPerson.Gender}" +
            $"{new string(' ', 9)}" +
            $"{originalastName.PadRight(54, ' ')}" +
            $"{new string(' ', 7)}" +
            $"{"2018Z981"}";

        Assert.NotNull(integrationTransaction);
        Assert.Equal(IntegrationTransactionImportStatus.Success, integrationTransaction.ImportStatus);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedTotalSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedFailureCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedDuplicateCount, integrationTransaction.DuplicateCount);
        Assert.NotEmpty(integrationTransaction.FileName);
        Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, r => MatchesExpectedRowData(r, expectedNewRowContent, trsPerson));
        Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, r => MatchesExpectedRowData(r, expectedNameChangeRow, trsPerson));
    }

    [Fact]
    public async Task Execute_MultipleNewRecordsMultiplePreviousNames_ReturnsExpectedContent()
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
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
        var job = new CapitaExportNewJob(dataLakeServiceClientMock.Object, Fixture.Logger.Object, dbContext, Clock);
        var expectedTotalRowCount = 4;
        var expectedTotalSuccessCount = 4;
        var expectedFailureCount = 0;
        var expectedDuplicateCount = 0;
        var jobMetaData = new JobMetadata()
        {
            JobName = nameof(CapitaExportNewJob),
            Metadata = new Dictionary<string, string>
                {
                    {
                        "LastRunDate", Clock.UtcNow.AddDays(-5).AddHours(-2).ToString("o")
                    }
                }
        };
        dbContext.JobMetadata.Add(jobMetaData);
        await dbContext.SaveChangesAsync();
        var originalastName1 = Faker.Name.Last();
        var updateLastName1 = Faker.Name.Last();
        var person1 = await TestData.CreatePersonAsync(x =>
        {
            x.WithLastName(originalastName1);
            x.WithGender(Gender.Male);
        });
        var originalastName2 = Faker.Name.Last();
        var updateLastName2 = Faker.Name.Last();
        var person2 = await TestData.CreatePersonAsync(x =>
        {
            x.WithLastName(originalastName2);
            x.WithGender(Gender.Male);
        });
        var trsPerson1 = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);
        var trsPerson2 = await dbContext.Persons.FirstAsync(x => x.PersonId == person2.PersonId);
        var personService = new PersonService(
            dbContext,
            Clock,
            Mock.Of<IEventPublisher>());
        var processContext = new ProcessContext(ProcessType.PersonDetailsUpdating, Clock.UtcNow, SystemUser.SystemUserId);
        var updateresult1 = await personService.UpdatePersonDetailsAsync(new(
            person1.PersonId,
            new PersonDetails()
            {
                FirstName = person1.FirstName,
                MiddleName = person1.MiddleName,
                LastName = updateLastName1,
                DateOfBirth = person1.DateOfBirth,
                EmailAddress = null,
                NationalInsuranceNumber = null,
                Gender = person1.Gender
            }.UpdateAll(),
            new() { Reason = PersonNameChangeReason.DeedPollOrOtherLegalProcess },
            new() { Reason = PersonDetailsChangeReason.AnotherReason }),
            processContext);
        var nameChangeEvent1 = new PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person1.PersonId,
            PersonAttributes = updateresult1.PersonDetails.ToEventModel(),
            OldPersonAttributes = updateresult1.OldPersonDetails.ToEventModel(),
            NameChangeReason = "",
            NameChangeEvidenceFile = null,
            DetailsChangeReason = null,
            DetailsChangeReasonDetail = null,
            DetailsChangeEvidenceFile = null,
            Changes = updateresult1.Changes.ToLegacyPersonDetailsUpdatedEventChanges()
        };

        var processContext2 = new ProcessContext(ProcessType.PersonDetailsUpdating, Clock.UtcNow, SystemUser.SystemUserId);
        var updateresult2 = await personService.UpdatePersonDetailsAsync(new(
            person2.PersonId,
            new PersonDetails()
            {
                FirstName = person2.FirstName,
                MiddleName = person2.MiddleName,
                LastName = updateLastName2,
                DateOfBirth = person2.DateOfBirth,
                EmailAddress = null,
                NationalInsuranceNumber = null,
                Gender = person2.Gender
            }.UpdateAll(),
            new() { Reason = PersonNameChangeReason.DeedPollOrOtherLegalProcess },
            new() { Reason = PersonDetailsChangeReason.AnotherReason }),
            processContext2);
        var nameChangeEvent2 = new PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person2.PersonId,
            PersonAttributes = updateresult2.PersonDetails.ToEventModel(),
            OldPersonAttributes = updateresult2.OldPersonDetails.ToEventModel(),
            NameChangeReason = "",
            NameChangeEvidenceFile = null,
            DetailsChangeReason = null,
            DetailsChangeReasonDetail = null,
            DetailsChangeEvidenceFile = null,
            Changes = updateresult2.Changes.ToLegacyPersonDetailsUpdatedEventChanges()
        };
        dbContext.AddEventWithoutBroadcast(nameChangeEvent1);
        dbContext.AddEventWithoutBroadcast(nameChangeEvent2);
        await dbContext.SaveChangesAsync();

        // Act
        var integrationTransactionJobId = await job.ExecuteAsync(CancellationToken.None);

        // Assert
        var integrationTransaction = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleAsync(x => x.IntegrationTransactionId == integrationTransactionJobId);
        var expectedNewRowContent1 = $"{trsPerson1.Trn}" +
            $"{(int)trsPerson1.Gender!}" +
            $"{new string(' ', 9)}" +
            $"{trsPerson1.DateOfBirth!.Value.ToString("ddMMyy")}" +
            $"{new string(' ', 1)}" +
            $"{trsPerson1.LastName.PadRight(17)}" +
            $"{new string('1', 1)}" +
            $"{trsPerson1.FirstName} {trsPerson1.MiddleName}".PadRight(35) +
            $"{new string(' ', 1)}" +
            $"{"1018Z981"}";

        var expectedNameChangeRow1 = $"{trsPerson1.Trn}" +
            $"{(int)trsPerson1.Gender}" +
            $"{new string(' ', 9)}" +
            $"{originalastName1.PadRight(54, ' ')}" +
            $"{new string(' ', 7)}" +
            $"{"2018Z981"}";

        var expectedNewRowContent2 = $"{trsPerson2.Trn}" +
            $"{(int)trsPerson2.Gender!}" +
            $"{new string(' ', 9)}" +
            $"{trsPerson2.DateOfBirth!.Value.ToString("ddMMyy")}" +
            $"{new string(' ', 1)}" +
            $"{trsPerson2.LastName.PadRight(17)}" +
            $"{new string('1', 1)}" +
            $"{trsPerson2.FirstName} {trsPerson2.MiddleName}".PadRight(35) +
            $"{new string(' ', 1)}" +
            $"{"1018Z981"}";

        var expectedNameChangeRow2 = $"{trsPerson2.Trn}" +
            $"{(int)trsPerson2.Gender}" +
            $"{new string(' ', 9)}" +
            $"{originalastName2.PadRight(54, ' ')}" +
            $"{new string(' ', 7)}" +
            $"{"2018Z981"}";

        Assert.NotNull(integrationTransaction);
        Assert.Equal(IntegrationTransactionImportStatus.Success, integrationTransaction.ImportStatus);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedTotalSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedFailureCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedDuplicateCount, integrationTransaction.DuplicateCount);
        Assert.NotEmpty(integrationTransaction.FileName);
        Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, r => MatchesExpectedRowData(r, expectedNewRowContent1, trsPerson1));
        Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, r => MatchesExpectedRowData(r, expectedNameChangeRow1, trsPerson1));
        Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, r => MatchesExpectedRowData(r, expectedNewRowContent2, trsPerson2));
        Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, r => MatchesExpectedRowData(r, expectedNameChangeRow2, trsPerson2));
    }

    [Fact]
    public async Task GetFileName_ReturnsExpectedFileName()
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var Job = new CapitaExportNewJob(Fixture.DataLakeServiceClient.Object, Fixture.Logger.Object, dbContext, Clock);
        var expectedFileName = $"Reg01_DTR_{Clock.UtcNow.ToString("yyyyMMdd")}_{Clock.UtcNow.ToString("HHmmss")}_New.txt"; ;

        // Act
        var fileName = Job.GetFileName(Clock);

        // Assert
        Assert.Equal(expectedFileName, fileName);
    }

    bool MatchesExpectedRowData(IntegrationTransactionRecord record, string expectedRowData, Person person) =>
        record.PersonId == person.PersonId &&
        record.FailureMessage == string.Empty &&
        record.Duplicate == null &&
        record.RowData != null &&
        record.RowData.Length == EXPECTED_ROW_LENGTH &&
        record.RowData == expectedRowData &&
        record.Status == IntegrationTransactionRecordStatus.Success;

    public Task InitializeAsync() => DbFixture.DbHelper.ClearDataAsync();

    public Task DisposeAsync() => Task.CompletedTask;
}

public class CapitaExportNewJobFixture
{
    public CapitaExportNewJobFixture(
        DbFixture dbFixture,
        ReferenceDataCache referenceDataCache,
        IServiceProvider provider,
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        IClock clock)
    {
        DbFixture = dbFixture;
        Clock = clock;

        Logger = new Mock<ILogger<CapitaExportNewJob>>();

        DataLakeServiceClient = new Mock<DataLakeServiceClient>();
        var fileSystemClientMock = new Mock<DataLakeFileSystemClient>();
        var dataLakeFileClientMock = new Mock<DataLakeFileClient>();

        DataLakeServiceClient
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

        TestData = new TestData(
            DbFixture.DbHelper.DbContextFactory,
            referenceDataCache,
            Clock);
    }

    public DbFixture DbFixture { get; }

    public TestData TestData { get; }

    public IClock Clock { get; }

    public Mock<ILogger<CapitaExportNewJob>> Logger { get; }

    public Mock<DataLakeServiceClient> DataLakeServiceClient { get; }
}
