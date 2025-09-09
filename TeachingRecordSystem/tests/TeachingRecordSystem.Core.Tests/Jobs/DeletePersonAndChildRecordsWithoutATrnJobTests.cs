using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.Files;
using Xunit.Abstractions;
using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection(nameof(DisableParallelization))]
public class DeletePersonAndChildRecordsWithoutATrnJobTests(
        DeletePersonAndChildRecordsWithoutATrnJobFixture fixture,
        ITestOutputHelper outputHelper)
    : IClassFixture<DeletePersonAndChildRecordsWithoutATrnJobFixture>, IAsyncLifetime
{
    public TrsDbContext DbContext => fixture.DbContext;
    public DbFixture DbFixture => fixture.DbFixture;
    public TestData TestData => fixture.TestData;
    public TestableClock Clock => fixture.Clock;
    public ILoggerFactory LoggerFactory => fixture.LoggerFactory;
    public Mock<IFileService> FileService => fixture.FileServiceMock;
    public ILogger<DeletePersonAndChildRecordsWithoutATrnJob> TestOutputLogger { get; } = new TestOutputLogger(outputHelper);

    public async Task InitializeAsync()
    {
        await DbFixture.DbHelper.ClearDataAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Execute_WithDryRunTrue_DoesNotDeleteAnyPersons()
    {
        // Arrange
        var personsWithNoTrn = await CreatePersonsWithoutTrnAsync(3);
        var personsWithTrn = await CreatePersonsWithTrnAsync(3);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(true, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();

        AssertEx.ContainsAll(personsWithNoTrn, personsAfterDelete);

        AssertEx.ContainsAll(personsWithTrn, personsAfterDelete);
    }

    [Fact]
    public async Task Execute_WithDryRunFalse_OnlyDeletesPersonsWithNullTrn()
    {
        // Arrange
        var personsWithNoTrn = await CreatePersonsWithoutTrnAsync(3);
        var personsWithTrn = await CreatePersonsWithTrnAsync(3);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();

        AssertEx.DoesNotContainAny(personsWithNoTrn, personsAfterDelete);

        AssertEx.ContainsAll(personsWithTrn, personsAfterDelete);
    }

    [Fact]
    public async Task Execute_WithDryRunTrue_UploadsFileWithPersonIdsThatWouldHaveBeenDeleted()
    {
        // Arrange
        var personsWithNoTrn = await CreatePersonsWithoutTrnAsync(3);
        var personsWithTrn = await CreatePersonsWithTrnAsync(3);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 2);

        // Act
        await job.ExecuteAsync(true, CancellationToken.None);

        // Assert
        var file = fixture.GetLastUploadedFile();
        Assert.NotNull(file);

        var deleted = ReadAsCsvRows(file.Contents);
        Assert.Equivalent(personsWithNoTrn.Select(p => new CsvRow(p)), deleted);
    }

    [Fact]
    public async Task Execute_WithDryRunFalse_UploadsFileWithPersonIdsThatWereDeleted()
    {
        // Arrange
        var personsWithNoTrn = await CreatePersonsWithoutTrnAsync(3);
        var personsWithTrn = await CreatePersonsWithTrnAsync(3);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 2);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var file = fixture.GetLastUploadedFile();
        Assert.NotNull(file);

        var deleted = ReadAsCsvRows(file.Contents);
        Assert.Equivalent(personsWithNoTrn.Select(p => new CsvRow(p)), deleted);
    }

    [Fact]
    public async Task Execute_WhenPersonsDirectlyReferencedBySupportTask_DeletesSupportTasksForPersonsWithNoTrn()
    {
        // Arrange
        var (personWithNoTrn, _, _, _) = await CreatePersonWithNoTrnAsync();
        var (personWithTrn, _, _, _) = await CreatePersonWithTrnAsync();

        var supportTasksForPersonWithNoTrn = await CreateSupportTasksReferencingPersonViaPersonIdAsync(personWithNoTrn);
        var supportTasksForPersonWithTrn = await CreateSupportTasksReferencingPersonViaPersonIdAsync(personWithTrn);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var supportTasksAfterDelete = await GetSupportTasksAsync();

        Assert.DoesNotContain(personWithNoTrn, personsAfterDelete);
        AssertEx.DoesNotContainAny(supportTasksForPersonWithNoTrn, supportTasksAfterDelete);

        Assert.Contains(personWithTrn, personsAfterDelete);
        AssertEx.ContainsAll(supportTasksForPersonWithTrn, supportTasksAfterDelete);
    }

    [Fact]
    public async Task Execute_WhenPersonsReferencedByTrnRequestMetadataViaResolvedPersonId_DeletesSupportTasksRequestsAndRequestMetadataForPersonsWithNoTrn()
    {
        // Arrange
        var (personWithNoTrn, _, _, _) = await CreatePersonWithNoTrnAsync();
        var (personWithTrn, _, _, _) = await CreatePersonWithTrnAsync();

        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTasksForPersonWithNoTrn, requestIdsForPersonWithNoTrn) = await CreateSupportTasksReferencingPersonViaResolvedPersonIdAsync(personWithNoTrn, applicationUser.UserId);
        var (supportTasksForPersonWithTrn, requestIdsForPersonWithTrn) = await CreateSupportTasksReferencingPersonViaResolvedPersonIdAsync(personWithTrn, applicationUser.UserId);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var supportTasksAfterDelete = await GetSupportTasksAsync();
        var trnRequestsAfterDelete = await GetTrnRequestsAsync();
        var trnRequestMetaDataAfterDelete = await GetTrnRequestMetadataAsync();

        Assert.DoesNotContain(personWithNoTrn, personsAfterDelete);
        AssertEx.DoesNotContainAny(supportTasksForPersonWithNoTrn, supportTasksAfterDelete);
        AssertEx.DoesNotContainAny(requestIdsForPersonWithNoTrn, trnRequestsAfterDelete);
        AssertEx.DoesNotContainAny(requestIdsForPersonWithNoTrn, trnRequestMetaDataAfterDelete);

        Assert.Contains(personWithTrn, personsAfterDelete);
        AssertEx.ContainsAll(supportTasksForPersonWithTrn, supportTasksAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonWithTrn, trnRequestsAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonWithTrn, trnRequestMetaDataAfterDelete);

        Assert.Contains(applicationUser.UserId, await GetApplicationUsersAsync());
    }

    [Fact]
    public async Task Execute_WhenPersonsReferencedByTrnRequestMetadataViaMatchedPersons_DoesNotDeleteSupportTaskOrMetadataForPersonsWithNoTrn()
    {
        // Arrange
        var (personWithNoTrn, _, _, _) = await CreatePersonWithNoTrnAsync();
        var (personWithTrn, _, _, _) = await CreatePersonWithTrnAsync();

        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTasksForPersonWithNoTrn, requestIdsForPersonWithNoTrn) = await CreateSupportTasksReferencingPersonViaMatchedPersonsAsync(personWithNoTrn, applicationUser.UserId);
        var (supportTasksForPersonWithTrn, requestIdsForPersonWithTrn) = await CreateSupportTasksReferencingPersonViaMatchedPersonsAsync(personWithTrn, applicationUser.UserId);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var supportTasksAfterDelete = await GetSupportTasksAsync();
        var trnRequestsAfterDelete = await GetTrnRequestsAsync();
        var trnRequestMetaDataAfterDelete = await GetTrnRequestMetadataAsync();

        Assert.DoesNotContain(personWithNoTrn, personsAfterDelete);
        AssertEx.ContainsAll(supportTasksForPersonWithNoTrn, supportTasksAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonWithNoTrn, trnRequestsAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonWithNoTrn, trnRequestMetaDataAfterDelete);

        Assert.Contains(personWithTrn, personsAfterDelete);
        AssertEx.ContainsAll(supportTasksForPersonWithTrn, supportTasksAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonWithTrn, trnRequestsAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonWithTrn, trnRequestMetaDataAfterDelete);

        Assert.Contains(applicationUser.UserId, await GetApplicationUsersAsync());
    }

    [Fact]
    public async Task Execute_WhenPersonsMergedWithOtherPersonsWithTrn_DoesNotDeleteOtherPersonsWithTrn()
    {
        // Arrange
        var (otherPersonWithTrn, _, _, _) = await CreatePersonWithTrnAsync();
        var (personWithNoTrn, _, _, _) = await CreatePersonWithNoTrnAsync(mergedWithPersonId: otherPersonWithTrn);
        var (personWithTrn, _, _, _) = await CreatePersonWithTrnAsync(mergedWithPersonId: otherPersonWithTrn);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();

        Assert.DoesNotContain(personWithNoTrn, personsAfterDelete);

        AssertEx.ContainsAll([personWithTrn, otherPersonWithTrn], personsAfterDelete);
    }

    [Fact]
    public async Task Execute_WhenPersonsMergedWithOtherPersonsWithNoTrn_DoesNotDeleteOtherPersonsWithTrn()
    {
        // Arrange
        var (otherPersonWithNoTrn, _, _, _) = await CreatePersonWithNoTrnAsync();
        var (personWithNoTrn, _, _, _) = await CreatePersonWithNoTrnAsync(mergedWithPersonId: otherPersonWithNoTrn);
        var (personWithTrn, _, _, _) = await CreatePersonWithTrnAsync(mergedWithPersonId: otherPersonWithNoTrn);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();

        AssertEx.DoesNotContainAny([personWithNoTrn, otherPersonWithNoTrn], personsAfterDelete);

        Assert.Contains(personWithTrn, personsAfterDelete);
    }

    [Fact]
    public async Task Execute_WhenOtherPersonsWithTrnMergedWithPersonsWithTrn_DoesNotDeleteOtherPersonsWithTrn()
    {
        // Arrange
        var (personWithNoTrn, _, _, _) = await CreatePersonWithNoTrnAsync();
        var (personWithTrn, _, _, _) = await CreatePersonWithTrnAsync();
        var (otherPersonWithTrn1, _, _, _) = await CreatePersonWithTrnAsync(mergedWithPersonId: personWithNoTrn);
        var (otherPersonWithTrn2, _, _, _) = await CreatePersonWithTrnAsync(mergedWithPersonId: personWithTrn);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();

        Assert.DoesNotContain(personWithNoTrn, personsAfterDelete);

        AssertEx.ContainsAll([personWithTrn, otherPersonWithTrn1, otherPersonWithTrn2], personsAfterDelete);
    }

    [Fact]
    public async Task Execute_WhenOtherPersonsWithNoTrnMergedWithPersonsWithTrn_DoesNotDeleteOtherPersonsWithTrn()
    {
        // Arrange
        var (personWithNoTrn, _, _, _) = await CreatePersonWithNoTrnAsync();
        var (personWithTrn, _, _, _) = await CreatePersonWithTrnAsync();
        var (otherPersonWithNoTrn1, _, _, _) = await CreatePersonWithNoTrnAsync(mergedWithPersonId: personWithNoTrn);
        var (otherPersonWithNoTrn2, _, _, _) = await CreatePersonWithNoTrnAsync(mergedWithPersonId: personWithTrn);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();

        AssertEx.DoesNotContainAny([personWithNoTrn, otherPersonWithNoTrn1, otherPersonWithNoTrn2], personsAfterDelete);

        Assert.Contains(personWithTrn, personsAfterDelete);
    }

    [Fact]
    public async Task Execute_WhenPersonsReferencedByIntegrationTransactionRecord_DeletesIntegrationTransactionRecordsForPersonsWithNoTrn()
    {
        // Arrange
        var (personWithNoTrn, _, _, _) = await CreatePersonWithNoTrnAsync();
        var (personWithTrn, _, _, _) = await CreatePersonWithTrnAsync();

        var itrForPersonWithNoTrn = await CreateIntegrationTransactionRecordAsync(personWithNoTrn);
        var itrForPersonWithTrn = await CreateIntegrationTransactionRecordAsync(personWithTrn);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var itrsAfterDelete = await GetIntegrationTransactionRecordsAsync();

        Assert.DoesNotContain(personWithNoTrn, personsAfterDelete);
        Assert.DoesNotContain(itrForPersonWithNoTrn, itrsAfterDelete);

        Assert.Contains(personWithTrn, personsAfterDelete);
        Assert.Contains(itrForPersonWithTrn, itrsAfterDelete);
    }

    [Fact]
    public async Task Execute_WhenPersonsReferencedByOneLoginUsers_DoesNotDeleteOneLoginUsers()
    {
        // Arrange
        var (personWithNoTrn, _, _, _) = await CreatePersonWithNoTrnAsync();
        var (personWithTrn, _, _, _) = await CreatePersonWithTrnAsync();

        var applicationUser = await TestData.CreateApplicationUserAsync();

        var oluForPersonWithNoTrn = await CreateOneLoginUserAsync(personWithNoTrn, applicationUser.UserId);
        var oluForPersonWithTrn = await CreateOneLoginUserAsync(personWithTrn, applicationUser.UserId);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var olusAfterDelete = await GetOneLoginUsersAsync();

        Assert.DoesNotContain(personWithNoTrn, personsAfterDelete);
        Assert.Contains(oluForPersonWithNoTrn, olusAfterDelete);

        Assert.Contains(personWithTrn, personsAfterDelete);
        Assert.Contains(oluForPersonWithTrn, olusAfterDelete);

        Assert.Contains(applicationUser.UserId, await GetApplicationUsersAsync());
    }

    [Fact]
    public async Task Execute_WhenPersonsHaveAlerts_DeletesAlertsForPersonsWithNoTrn()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();

        var (personWithNoTrn, alertsForPersonsWithNoTrn, _, _) = await CreatePersonWithNoTrnAsync(p => p
            .WithAlert(a => a
                .WithAlertTypeId(AlertType.DbsAlertTypeId)
                .WithCreatedByUser(user.UserId)
                .WithStartDate(DateOnly.Parse("1 Jan 2000"))));

        var (personWithTrn, alertsForPersonsWithTrn, _, _) = await CreatePersonWithTrnAsync(p => p
            .WithAlert(a => a
                .WithAlertTypeId(AlertType.DbsAlertTypeId)
                .WithCreatedByUser(user.UserId)
                .WithStartDate(DateOnly.Parse("1 Jan 2000"))));

        var allPersons = new[] { personWithNoTrn };
        var allAlertTypes = await GetAlertTypesAsync();

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var alertsAfterDelete = await GetAlertsAsync();

        Assert.DoesNotContain(personWithNoTrn, personsAfterDelete);
        AssertEx.DoesNotContainAny(alertsForPersonsWithNoTrn, alertsAfterDelete);

        Assert.Contains(personWithTrn, personsAfterDelete);
        AssertEx.ContainsAll(alertsForPersonsWithTrn, alertsAfterDelete);

        Assert.Equivalent(allAlertTypes, await GetAlertTypesAsync());

        Assert.Contains(user.UserId, await GetUsersAsync());
    }

    [Fact]
    public async Task Execute_WhenPersonsHaveNotes_DeletesNotesForPersonsWithNoTrn()
    {
        // Arrange
        var (personWithNoTrn, _, _, _) = await CreatePersonWithNoTrnAsync();
        var (personWithTrn, _, _, _) = await CreatePersonWithTrnAsync();

        var user = await TestData.CreateUserAsync();

        var noteForPersonWithNoTrn = await CreateNoteAsync(personWithNoTrn, user.UserId);
        var noteForPersonWithTrn = await CreateNoteAsync(personWithTrn, user.UserId);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var notesAfterDelete = await GetNotesAsync();

        Assert.DoesNotContain(personWithNoTrn, personsAfterDelete);
        Assert.DoesNotContain(noteForPersonWithNoTrn, notesAfterDelete);

        Assert.Contains(personWithTrn, personsAfterDelete);
        Assert.Contains(noteForPersonWithTrn, notesAfterDelete);

        Assert.Contains(user.UserId, await GetUsersAsync());
    }

    [Fact]
    public async Task Execute_WhenPersonsHavePreviousNames_DeletesPreviousNamesForPersonsWithNoTrn()
    {
        var date = DateTime.Parse("1 Jan 2000").ToUniversalTime();
        // Arrange
        var (personWithNoTrn, _, previousNamesForPersonWithNoTrn, _) = await CreatePersonWithNoTrnAsync(p => p
            .WithPreviousNames([("Joan", "Of", "Arc", date), ("Winnie", "The", "Pooh", date)]));
        var (personWithTrn, _, previousNamesForPersonWithTrn, _) = await CreatePersonWithTrnAsync(p => p
            .WithPreviousNames([("Joan", "Of", "Arc", date), ("Winnie", "The", "Pooh", date)]));

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var previousNamesAfterDelete = await GetPreviousNamesAsync();

        Assert.DoesNotContain(personWithNoTrn, personsAfterDelete);
        AssertEx.DoesNotContainAny(previousNamesForPersonWithNoTrn, previousNamesAfterDelete);

        Assert.Contains(personWithTrn, personsAfterDelete);
        AssertEx.ContainsAll(previousNamesForPersonWithTrn, previousNamesAfterDelete);
    }

    [Fact]
    public async Task Execute_WhenPersonsHaveQualifications_DeletesQualificationsForPersonsWithNoTrn()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();

        var (personWithNoTrn, _, _, qualificationsForPersonWithNoTrn) = await CreatePersonWithNoTrnAsync(p => p
            .WithMandatoryQualification(mq => mq
                .WithCreatedByUser(RaisedByUserInfo.FromUserId(user.UserId))));
        var (personWithTrn, _, _, qualificationsForPersonWithTrn) = await CreatePersonWithTrnAsync(p => p
            .WithMandatoryQualification(mq => mq
                .WithCreatedByUser(RaisedByUserInfo.FromUserId(user.UserId))));

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var qualificationsAfterDelete = await GetQualificationsAsync();

        Assert.DoesNotContain(personWithNoTrn, personsAfterDelete);
        AssertEx.DoesNotContainAny(qualificationsForPersonWithNoTrn, qualificationsAfterDelete);

        Assert.Contains(personWithTrn, personsAfterDelete);
        AssertEx.ContainsAll(qualificationsForPersonWithTrn, qualificationsAfterDelete);
    }

    [Fact]
    public async Task Execute_WhenPersonsHaveTpsEmployments_DeletesTpsEmploymentsForPersonsWithNoTrn()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();

        var (personWithNoTrn, _, _, _) = await CreatePersonWithNoTrnAsync();
        var (personWithTrn, _, _, _) = await CreatePersonWithTrnAsync();

        var establishment = await TestData.CreateEstablishmentAsync("001");

        var employmentForPersonWithNoTrn = await CreateEmploymentAsync(establishment.EstablishmentId, personWithNoTrn);
        var employmentForPersonWithTrn = await CreateEmploymentAsync(establishment.EstablishmentId, personWithTrn);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var employmentsAfterDelete = await GetTpsEmploymentsAsync();
        var establishmentsAfterDelete = await GetEstablishmentsAsync();

        Assert.DoesNotContain(personWithNoTrn, personsAfterDelete);
        Assert.DoesNotContain(employmentForPersonWithNoTrn, employmentsAfterDelete);

        Assert.Contains(personWithTrn, personsAfterDelete);
        Assert.Contains(employmentForPersonWithTrn, employmentsAfterDelete);

        Assert.Contains(establishment.EstablishmentId, establishmentsAfterDelete);
    }

    private DeletePersonAndChildRecordsWithoutATrnJob CreateDeletePersonAndChildRecordsWithoutATrnJob(int batchSize) =>
        new DeletePersonAndChildRecordsWithoutATrnJob(
            CreateJobOptions(batchSize),
            DbContext,
            FileService.Object,
            Clock,
            TestOutputLogger);

    private IOptions<DeletePersonAndChildRecordsWithoutATrnOptions> CreateJobOptions(int batchSize) =>
        Options.Create(
            new DeletePersonAndChildRecordsWithoutATrnOptions
            {
                BatchSize = batchSize
            });

    private List<CsvRow> ReadAsCsvRows(string fileContents)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContents));
        using var sr = new StreamReader(stream);
        using var csv = new CsvHelper.CsvReader(sr, System.Globalization.CultureInfo.InvariantCulture);

        return csv.GetRecords<CsvRow>().ToList();
    }

    private async Task<IEnumerable<Guid>> CreatePersonsWithTrnAsync(int count)
    {
        List<Guid> personIds = [];

        for (var i = 0; i < count; i++)
        {
            var person = await TestData.CreatePersonAsync();
            personIds.Add(person.PersonId);
        }

        return personIds;
    }

    private async Task<IEnumerable<Guid>> CreatePersonsWithoutTrnAsync(int count)
    {
        List<Person> persons = [];

        for (var i = 0; i < count; i++)
        {
            var person = await TestData.CreatePersonAsync();
            persons.Add(person.Person);
        }

        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            foreach (var p in persons)
            {
                dbContext.Attach(p);
                p.Trn = null;
            }

            await dbContext.SaveChangesAsync();

            return persons;
        });

        return persons.Select(p => p.PersonId);
    }

    private async Task<(Guid, IEnumerable<Guid>, IEnumerable<Guid>, IEnumerable<Guid>)> CreatePersonWithTrnAsync(Action<CreatePersonBuilder>? configure = null, Guid? mergedWithPersonId = null)
    {
        var person = await TestData.CreatePersonAsync(configure);

        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);

            if (mergedWithPersonId is Guid id)
            {
                person.Person.MergedWithPersonId = id;
            }

            await dbContext.SaveChangesAsync();
        });

        return (person.PersonId, person.Alerts.Select(a => a.AlertId), person.PreviousNames.Select(p => p.PreviousNameId), person.MandatoryQualifications.Select(q => q.QualificationId));
    }

    private async Task<(Guid, IEnumerable<Guid>, IEnumerable<Guid>, IEnumerable<Guid>)> CreatePersonWithNoTrnAsync(Action<CreatePersonBuilder>? configure = null, Guid? mergedWithPersonId = null)
    {
        var person = await TestData.CreatePersonAsync(configure);

        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Trn = null;

            if (mergedWithPersonId is Guid id)
            {
                person.Person.MergedWithPersonId = id;
            }

            await dbContext.SaveChangesAsync();
        });

        return (person.PersonId, person.Alerts.Select(a => a.AlertId), person.PreviousNames.Select(p => p.PreviousNameId), person.MandatoryQualifications.Select(q => q.QualificationId));
    }

    private async Task<IEnumerable<string>> CreateSupportTasksReferencingPersonViaPersonIdAsync(Guid personId)
    {
        return new[] {
            await TestData.CreateChangeNameRequestSupportTaskAsync(
                personId,
                b => b.WithFirstName("XXX")),
            await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
                personId,
                b => b.WithDateOfBirth(DateOnly.Parse("1 Jan 1900")))
        }.Select(st => st.SupportTaskReference);
    }

    private async Task<(IEnumerable<string>, IEnumerable<string?>)> CreateSupportTasksReferencingPersonViaResolvedPersonIdAsync(Guid personId, Guid applicationUserId)
    {
        var supportTask1 = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUserId, t => t
                .WithCreatedOn(DateTime.Parse("1 Jan 2000"))
                .WithResolvedPersonId(personId));
        var supportTask2 = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUserId, t => t
                .WithCreatedOn(DateTime.Parse("1 Jan 2000")));

        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(supportTask2);

            supportTask2.TrnRequestMetadata!.SetResolvedPerson(personId, TrnRequestStatus.Completed);
            await dbContext.SaveChangesAsync();
        });

        return ([supportTask1.SupportTaskReference, supportTask2.SupportTaskReference], [supportTask1.TrnRequestId, supportTask2.TrnRequestId]);
    }

    private async Task<(IEnumerable<string>, IEnumerable<string?>)> CreateSupportTasksReferencingPersonViaMatchedPersonsAsync(Guid personId, Guid applicationUserId)
    {
        var supportTask1 = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUserId, t => t
               .WithMatchedPersons(personId));
        var supportTask2 = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUserId, t => t
                .WithMatchedPersons(personId));

        return ([supportTask1.SupportTaskReference, supportTask2.SupportTaskReference], [supportTask1.TrnRequestId, supportTask2.TrnRequestId]);
    }

    private async Task<long> CreateIntegrationTransactionRecordAsync(Guid personId)
    {
        return await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var itr = new IntegrationTransactionRecord
            {
                IntegrationTransactionRecordId = Faker.RandomNumber.Next(1, long.MaxValue),
                CreatedDate = Clock.UtcNow,
                Duplicate = false,
                FailureMessage = "TEST",
                HasActiveAlert = false,
                PersonId = personId,
                RowData = "XXX",
                Status = IntegrationTransactionRecordStatus.Failure
            };

            dbContext.IntegrationTransactionRecords.Add(itr);

            await dbContext.SaveChangesAsync();

            return itr.IntegrationTransactionRecordId;
        });
    }

    private async Task<string> CreateOneLoginUserAsync(Guid personId, Guid applicationUserId)
    {
        return await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId, verifiedInfo: (["XXX"], DateOnly.Parse("1 Jan 2000")));

            dbContext.Attach(oneLoginUser);
            oneLoginUser.SetVerified(Clock.UtcNow, OneLoginUserVerificationRoute.External, applicationUserId, null, null);

            await dbContext.SaveChangesAsync();

            return oneLoginUser.Subject;
        });
    }

    private async Task<Guid> CreateNoteAsync(Guid personId, Guid userId)
    {
        return await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var note = new Core.DataStore.Postgres.Models.Note
            {
                NoteId = Guid.NewGuid(),
                PersonId = personId,
                Content = "TEST",
                FileId = null,
                OriginalFileName = null,
                CreatedByUserId = userId,
                CreatedOn = Clock.UtcNow,
                UpdatedOn = Clock.UtcNow
            };

            dbContext.Notes.Add(note);

            await dbContext.SaveChangesAsync();

            return note.NoteId;
        });
    }

    private async Task<Guid> CreateEmploymentAsync(Guid establishmentId, Guid personId)
    {
        return await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var employment = new Core.DataStore.Postgres.Models.TpsEmployment()
            {
                CreatedOn = Clock.UtcNow,
                EmployerEmailAddress = null,
                EmployerPostcode = null,
                EmploymentType = EmploymentType.FullTime,
                EndDate = null,
                EstablishmentId = establishmentId,
                Key = "TEST",
                LastExtractDate = DateOnly.Parse("1 Jan 2000"),
                LastKnownTpsEmployedDate = DateOnly.Parse("1 Jan 2000"),
                NationalInsuranceNumber = null,
                PersonEmailAddress = null,
                PersonId = personId,
                PersonPostcode = null,
                StartDate = DateOnly.Parse("1 Jan 2000"),
                TpsEmploymentId = Guid.NewGuid(),
                UpdatedOn = Clock.UtcNow,
                WithdrawalConfirmed = false
            };

            dbContext.TpsEmployments.Add(employment);

            await dbContext.SaveChangesAsync();

            return employment.TpsEmploymentId;
        });
    }

    private async Task<IEnumerable<Guid>> GetPersonsAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.Persons
            .Select(p => p.PersonId)
            .ToArrayAsync());

    private async Task<IEnumerable<Guid>> GetAlertsAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.Alerts
            .Select(a => a.AlertId)
            .ToArrayAsync());

    private async Task<IEnumerable<Guid>> GetNotesAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.Notes
            .Select(n => n.NoteId)
            .ToArrayAsync());

    private async Task<IEnumerable<Guid>> GetPreviousNamesAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.PreviousNames
            .Select(p => p.PreviousNameId)
            .ToArrayAsync());

    private async Task<IEnumerable<Guid>> GetQualificationsAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.Qualifications
            .Select(p => p.QualificationId)
            .ToArrayAsync());

    private async Task<IEnumerable<Guid>> GetTpsEmploymentsAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.TpsEmployments
            .Select(e => e.TpsEmploymentId)
            .ToArrayAsync());

    private async Task<IEnumerable<Guid>> GetEstablishmentsAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.Establishments
            .Select(e => e.EstablishmentId)
            .ToArrayAsync());

    private async Task<IEnumerable<Guid>> GetAlertTypesAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.AlertTypes
            .Select(a => a.AlertTypeId)
            .ToArrayAsync());

    private async Task<IEnumerable<string>> GetSupportTasksAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.SupportTasks
            .Select(s => s.SupportTaskReference)
            .ToArrayAsync());

    private async Task<IEnumerable<string>> GetTrnRequestsAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.TrnRequestMetadata
            .Select(m => m.RequestId)
            .ToArrayAsync());

    private async Task<IEnumerable<string>> GetTrnRequestMetadataAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.TrnRequestMetadata
            .Select(m => m.RequestId)
            .ToArrayAsync());

    private async Task<IEnumerable<long>> GetIntegrationTransactionRecordsAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.IntegrationTransactionRecords
            .Select(itr => itr.IntegrationTransactionRecordId)
            .ToArrayAsync());

    private async Task<IEnumerable<string>> GetOneLoginUsersAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.OneLoginUsers
            .Select(olu => olu.Subject)
            .ToArrayAsync());

    private async Task<IEnumerable<Guid>> GetUsersAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.Users
            .Select(u => u.UserId)
            .ToArrayAsync());

    private async Task<IEnumerable<Guid>> GetApplicationUsersAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.ApplicationUsers
            .Select(u => u.UserId)
            .ToArrayAsync());
}

public class DeletePersonAndChildRecordsWithoutATrnJobFixture : IAsyncLifetime
{
    List<UploadedFile> _uploadedFiles = [];

    public DeletePersonAndChildRecordsWithoutATrnJobFixture(
        DbFixture dbFixture,
        IOrganizationServiceAsync2 organizationService,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator,
        ILoggerFactory loggerFactory)
    {
        DbFixture = dbFixture;
        LoggerFactory = loggerFactory;
        Clock = new();

        TestData = new TestData(
            dbFixture.GetDbContextFactory(),
            organizationService,
            referenceDataCache,
            Clock,
            trnGenerator,
            TestDataPersonDataSource.Trs);

        FileServiceMock.Setup(mock => mock.UploadFileAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string?>()))
            .Callback(async (string fileName, Stream stream, string? contentType) =>
            {
                using (stream)
                using (var sr = new StreamReader(stream))
                {
                    var file = await sr.ReadToEndAsync();
                    _uploadedFiles.Add(new(fileName, file, contentType));
                }
            });
    }

    public TestableClock Clock { get; }
    public DbFixture DbFixture { get; }
    public ILoggerFactory LoggerFactory { get; }
    public TestData TestData { get; }
    public Mock<IFileService> FileServiceMock { get; } = new Mock<IFileService>();
    public TrsDbContext DbContext = null!;

    public async Task InitializeAsync()
    {
        await DbFixture.DbHelper.ClearDataAsync();

        DbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        FileServiceMock.Invocations.Clear();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public UploadedFile? GetLastUploadedFile() => _uploadedFiles.LastOrDefault();
}

public class TestOutputLogger(ITestOutputHelper outputHelper) : ILogger<DeletePersonAndChildRecordsWithoutATrnJob>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => new Scope();

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        outputHelper.WriteLine($"{logLevel}: {formatter(state, exception)}");
    }

    public class Scope : IDisposable
    {
        public void Dispose()
        {
        }
    }
}

public record UploadedFile(string FileName, string Contents, string? ContentType);

public record CsvRow(Guid PersonId);
