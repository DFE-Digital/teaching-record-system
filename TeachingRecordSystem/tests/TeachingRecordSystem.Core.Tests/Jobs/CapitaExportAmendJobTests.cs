using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection(nameof(DisableParallelization))]
public class CapitaExportAmendJobTests(CapitaExportAmendJobFixture Fixture) : IClassFixture<CapitaExportAmendJobFixture>, IAsyncLifetime
{
    private DbFixture DbFixture => Fixture.DbFixture;

    private IClock Clock => Fixture.Clock;

    private TestData TestData => Fixture.TestData;

    private const int EXPECTED_ROW_LENGTH = 86;

    [Fact]
    public async Task Execute_WithNoChanges_ReturnsExpectedCounts()
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var expectedSuccessCount = 0;
        var expectedFailureCount = 0;
        var expectedTotalCount = 0;
        var expectedDuplicateCount = 0;
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

        var user = await TestData.CreateApplicationUserAsync();

        var option = new CapitaTpsUserOption() { CapitaTpsUserId = user.UserId };
        var jobOption = Options.Create(option);
        var job = new CapitaExportAmendJob(blobServiceClientMock.Object, Fixture.Logger.Object, dbContext, Clock, jobOption);

        // Act
        var integrationTransactionJobId = await job.ExecuteAsync(CancellationToken.None);
        var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionJobId);

        // Assert
        Assert.NotNull(integrationTransaction);
        Assert.NotEmpty(integrationTransaction.FileName);
        Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedFailureCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedTotalCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedDuplicateCount, integrationTransaction.DuplicateCount);
        Assert.Empty(integrationTransaction.IntegrationTransactionRecords!);
        Assert.Equal(IntegrationTransactionImportStatus.Success, integrationTransaction.ImportStatus);
    }

    [Fact]
    public async Task Execute_WithDobChanges_ReturnsExpectedCounts()
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
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

        var user = await TestData.CreateApplicationUserAsync();

        var option = new CapitaTpsUserOption() { CapitaTpsUserId = user.UserId };
        var jobOption = Options.Create(option);
        var job = new CapitaExportAmendJob(blobServiceClientMock.Object, Fixture.Logger.Object, dbContext, Clock, jobOption);
        var jobMetaData = new JobMetadata()
        {
            JobName = nameof(CapitaExportAmendJob),
            Metadata = new Dictionary<string, string>
                {
                    {
                        "LastRunDate", Clock.UtcNow.AddDays(-3).AddHours(-2).ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                    }
                }
        };
        dbContext.JobMetadata.Add(jobMetaData);
        await dbContext.SaveChangesAsync();
        var expectedSuccessCount = 1;
        var expectedFailureCount = 0;
        var expectedTotalCount = 1;
        var expectedDuplicateCount = 0;
        var lastName = "xxxxxx";
        var updatedDateOfBirth = new DateOnly(1977, 04, 02);
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithLastName(lastName);
            x.WithDateOfBirth(new DateOnly(1983, 01, 07));
            x.WithGender(Gender.Male);
            x.WithCreatedByTps(true);
        });
        var trsPerson = dbContext.Persons.Single(x => x.PersonId == person.PersonId);
        var dobChangedEvent = trsPerson.UpdateDetails(person.FirstName, person.MiddleName, person.LastName, updatedDateOfBirth, null, null, person.Gender, Clock.UtcNow.AddHours(-1));
        var personUpdatedEvent = new PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow.AddHours(-1),
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            PersonAttributes = dobChangedEvent.PersonAttributes,
            OldPersonAttributes = dobChangedEvent.OldPersonAttributes,
            NameChangeReason = "",
            NameChangeEvidenceFile = null,
            DetailsChangeReason = null,
            DetailsChangeReasonDetail = null,
            DetailsChangeEvidenceFile = null,
            Changes = (PersonDetailsUpdatedEventChanges)dobChangedEvent.Changes
        };
        await dbContext.AddEventAndBroadcastAsync(personUpdatedEvent!);
        await dbContext.SaveChangesAsync();

        // Act
        var integrationTransactionJobId = await job.ExecuteAsync(CancellationToken.None);
        var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionJobId);

        // Assert
        Assert.NotNull(integrationTransaction);
        Assert.NotEmpty(integrationTransaction.FileName);
        Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedFailureCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedTotalCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedDuplicateCount, integrationTransaction.DuplicateCount);

        var expectedRow = $"{trsPerson.Trn}" +
                          $"{(int)trsPerson.Gender!}" +
                          $"//" +
                          $"{lastName}" +
                          $"{new string(' ', 1)}" +
                          $"{trsPerson.DateOfBirth!.Value:ddMMyy*}" +
                          $"{new string(' ', 1)}" +
                          $"{new string(' ', 9)}" +
                          $"{new string(' ', 44)}" +
                          $"1211ZE1*";
        Assert.NotNull(integrationTransaction);
        Assert.Equal(IntegrationTransactionImportStatus.Success, integrationTransaction.ImportStatus);
        Assert.NotEmpty(integrationTransaction.FileName);
        Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, r => MatchesExpectedRowData(r, expectedRow, trsPerson));
    }

    [Fact]
    public async Task Execute_WithNationalInsuranceNumberChanges_ReturnsExpectedCounts()
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
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

        var user = await TestData.CreateApplicationUserAsync();

        var option = new CapitaTpsUserOption() { CapitaTpsUserId = user.UserId };
        var jobOption = Options.Create(option);
        var job = new CapitaExportAmendJob(blobServiceClientMock.Object, Fixture.Logger.Object, dbContext, Clock, jobOption);
        var jobMetaData = new JobMetadata()
        {
            JobName = nameof(CapitaExportAmendJob),
            Metadata = new Dictionary<string, string>
                {
                    {
                        "LastRunDate", Clock.UtcNow.AddDays(-3).ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                    }
                }
        };
        dbContext.JobMetadata.Add(jobMetaData);
        await dbContext.SaveChangesAsync();
        var expectedSuccessCount = 1;
        var expectedFailureCount = 0;
        var expectedTotalCount = 1;
        var expectedDuplicateCount = 0;
        var lastName = "xxxxxx";
        var updatedNationalInsuranceNumber = NationalInsuranceNumber.Parse(Faker.Identification.UkNationalInsuranceNumber());
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithLastName(lastName);
            x.WithDateOfBirth(new DateOnly(1983, 01, 07));
            x.WithGender(Gender.Male);
            x.WithCreatedByTps(true);
        });
        var trsPerson = dbContext.Persons.Single(x => x.PersonId == person.PersonId);
        var changeEvent = trsPerson.UpdateDetails(person.FirstName, person.MiddleName, person.LastName, person.DateOfBirth, null, updatedNationalInsuranceNumber, person.Gender, Clock.UtcNow.AddHours(-1));
        var personUpdatedEvent = new PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow.AddHours(-1),
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            PersonAttributes = changeEvent.PersonAttributes,
            OldPersonAttributes = changeEvent.OldPersonAttributes,
            NameChangeReason = "",
            NameChangeEvidenceFile = null,
            DetailsChangeReason = null,
            DetailsChangeReasonDetail = null,
            DetailsChangeEvidenceFile = null,
            Changes = (PersonDetailsUpdatedEventChanges)changeEvent.Changes
        };
        await dbContext.AddEventAndBroadcastAsync(personUpdatedEvent!);
        await dbContext.SaveChangesAsync();

        // Act
        var integrationTransactionJobId = await job.ExecuteAsync(CancellationToken.None);
        var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionJobId);

        // Assert
        Assert.NotNull(integrationTransaction);
        Assert.NotEmpty(integrationTransaction.FileName);
        Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedFailureCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedTotalCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedDuplicateCount, integrationTransaction.DuplicateCount);
        var expectedRow = $"{trsPerson.Trn}" +
                          $"{(int)trsPerson.Gender!}" +
                          $"//" +
                          $"{lastName}" +
                          $"{new string(' ', 1)}" +
                          $"{new string(' ', 7)}" +
                          $"{new string(' ', 1)}" +
                          $"{trsPerson.NationalInsuranceNumber}" +
                          $"{new string(' ', 44)}" +
                          $" 321ZE2*";
        Assert.NotNull(integrationTransaction);
        Assert.Equal(IntegrationTransactionImportStatus.Success, integrationTransaction.ImportStatus);
        Assert.NotEmpty(integrationTransaction.FileName);
        Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, r => MatchesExpectedRowData(r, expectedRow, trsPerson));
    }

    [Fact]
    public async Task Execute_WithNiandDobUpdates_ReturnsExpectedCounts()
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
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

        var user = await TestData.CreateApplicationUserAsync();

        var option = new CapitaTpsUserOption() { CapitaTpsUserId = user.UserId };
        var jobOption = Options.Create(option);
        var job = new CapitaExportAmendJob(blobServiceClientMock.Object, Fixture.Logger.Object, dbContext, Clock, jobOption);
        var jobMetaData = new JobMetadata()
        {
            JobName = nameof(CapitaExportAmendJob),
            Metadata = new Dictionary<string, string>
                {
                    {
                        "LastRunDate", Clock.UtcNow.AddDays(-3).AddHours(-5).ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                    }
                }
        };
        dbContext.JobMetadata.Add(jobMetaData);
        await dbContext.SaveChangesAsync();
        var expectedSuccessCount = 2;
        var expectedFailureCount = 0;
        var expectedTotalCount = 2;
        var expectedDuplicateCount = 0;
        var lastName = "xxxxxx";
        var updatedNationalInsuranceNumber = NationalInsuranceNumber.Parse(Faker.Identification.UkNationalInsuranceNumber());
        var updatedDateOfBirth = new DateOnly(1977, 02, 15);
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithLastName(lastName);
            x.WithDateOfBirth(new DateOnly(1983, 01, 07));
            x.WithGender(Gender.Male);
            x.WithCreatedByTps(true);
        });
        var trsPerson = dbContext.Persons.Single(x => x.PersonId == person.PersonId);
        var changeEvent = trsPerson.UpdateDetails(person.FirstName, person.MiddleName, person.LastName, updatedDateOfBirth, null, updatedNationalInsuranceNumber, person.Gender, Clock.UtcNow.AddHours(-1));
        var personUpdatedEvent = new PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            PersonAttributes = changeEvent.PersonAttributes,
            OldPersonAttributes = changeEvent.OldPersonAttributes,
            NameChangeReason = "",
            NameChangeEvidenceFile = null,
            DetailsChangeReason = null,
            DetailsChangeReasonDetail = null,
            DetailsChangeEvidenceFile = null,
            Changes = (PersonDetailsUpdatedEventChanges)changeEvent.Changes
        };
        await dbContext.AddEventAndBroadcastAsync(personUpdatedEvent!);
        await dbContext.SaveChangesAsync();

        // Act
        var integrationTransactionJobId = await job.ExecuteAsync(CancellationToken.None);
        var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionJobId);

        // Assert
        Assert.NotNull(integrationTransaction);
        Assert.NotEmpty(integrationTransaction.FileName);
        Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
        Assert.Equal(expectedFailureCount, integrationTransaction.FailureCount);
        Assert.Equal(expectedTotalCount, integrationTransaction.TotalCount);
        Assert.Equal(expectedDuplicateCount, integrationTransaction.DuplicateCount);
        var expectedNIRow = $"{trsPerson.Trn}" +
                          $"{(int)trsPerson.Gender!}" +
                          $"//" +
                          $"{lastName}" +
                          $"{new string(' ', 1)}" +
                          $"{new string(' ', 7)}" +
                          $"{new string(' ', 1)}" +
                          $"{trsPerson.NationalInsuranceNumber}" +
                          $"{new string(' ', 44)}" +
                          $" 321ZE2*";

        var expectedDobRow = $"{trsPerson.Trn}" +
                          $"{(int)trsPerson.Gender!}" +
                          $"//" +
                          $"{lastName}" +
                          $"{new string(' ', 1)}" +
                          $"{trsPerson.DateOfBirth!.Value:ddMMyy*}" +
                          $"{new string(' ', 1)}" +
                          $"{new string(' ', 9)}" +
                          $"{new string(' ', 44)}" +
                          $"1211ZE1*";
        Assert.NotNull(integrationTransaction);
        Assert.Equal(IntegrationTransactionImportStatus.Success, integrationTransaction.ImportStatus);
        Assert.NotEmpty(integrationTransaction.FileName);
        Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, r => MatchesExpectedRowData(r, expectedNIRow, trsPerson));
        Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, r => MatchesExpectedRowData(r, expectedDobRow, trsPerson));
    }

    [Fact]
    public async Task GetPersonAmendedRow_WithLastNameExceedsSixCharacters_ReturnsExpectedContent()
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
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

        var user = await TestData.CreateApplicationUserAsync();

        var option = new CapitaTpsUserOption() { CapitaTpsUserId = user.UserId };
        var jobOption = Options.Create(option);
        var job = new CapitaExportAmendJob(blobServiceClientMock.Object, Fixture.Logger.Object, dbContext, Clock, jobOption);
        var lastName = new string('x', 25);
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithLastName(lastName);
            x.WithDateOfBirth(new DateOnly(1983, 01, 07));
            x.WithGender(Gender.Male);
        });
        var trsPerson = dbContext.Persons.Single(x => x.PersonId == person.PersonId);
        var amendedPerson = new CapitaExportAmendJobResult()
        {
            Trn = trsPerson.Trn!,
            LastName = trsPerson.LastName,
            DateOfBirth = trsPerson.DateOfBirth,
            NationalInsuranceNumber = null,
            PersonId = trsPerson.PersonId,
            Gender = trsPerson.Gender,
            ChangeType = PersonAttributesChanges.DateOfBirth,
            Created = DateTime.UtcNow
        };

        // Act
        var row = job.GetPersonAmendedRow(amendedPerson, CapitaAmendExportType.DateOfBirth);

        // Assert
        var lastNameFixed = lastName.Substring(0, 6);
        var expectedRow = $"{trsPerson.Trn}" +
                          $"{(int)trsPerson.Gender!}" +
                          $"//" +
                          $"{lastNameFixed}" +
                          $"{new string(' ', 1)}" +
                          $"{trsPerson.DateOfBirth!.Value:ddMMyy*}" +
                          $"{new string(' ', 1)}" +
                          $"{new string(' ', 9)}" +
                          $"{new string(' ', 44)}" +
                          $"1211ZE1*";
        Assert.Equal(EXPECTED_ROW_LENGTH, row.Length);
        Assert.Equal(expectedRow, row);
    }

    [Fact]
    public async Task GetPersonAmendedRow_WithLastNameLessThanSixCharacters_ReturnsExpectedContent()
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
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

        var user = await TestData.CreateApplicationUserAsync();

        var option = new CapitaTpsUserOption() { CapitaTpsUserId = user.UserId };
        var jobOption = Options.Create(option);
        var job = new CapitaExportAmendJob(blobServiceClientMock.Object, Fixture.Logger.Object, dbContext, Clock, jobOption);
        var lastName = "xx";
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithLastName(lastName);
            x.WithDateOfBirth(new DateOnly(1983, 01, 07));
            x.WithGender(Gender.Male);
        });
        var trsPerson = dbContext.Persons.Single(x => x.PersonId == person.PersonId);
        var amendedPerson = new CapitaExportAmendJobResult()
        {
            Trn = trsPerson.Trn!,
            LastName = trsPerson.LastName,
            DateOfBirth = trsPerson.DateOfBirth,
            NationalInsuranceNumber = null,
            PersonId = trsPerson.PersonId,
            Gender = trsPerson.Gender,
            ChangeType = PersonAttributesChanges.DateOfBirth,
            Created = DateTime.UtcNow
        };

        // Act
        var row = job.GetPersonAmendedRow(amendedPerson, CapitaAmendExportType.DateOfBirth);

        // Assert
        var lastNameFixed = lastName.PadRight(6);
        var expectedRow = $"{trsPerson.Trn}" +
                          $"{(int)trsPerson.Gender!}" +
                          $"//" +
                          $"{lastNameFixed}" +
                          $"{new string(' ', 1)}" +
                          $"{trsPerson.DateOfBirth!.Value:ddMMyy*}" +
                          $"{new string(' ', 1)}" +
                          $"{new string(' ', 9)}" +
                          $"{new string(' ', 44)}" +
                          $"1211ZE1*";
        Assert.Equal(EXPECTED_ROW_LENGTH, row.Length);
        Assert.Equal(expectedRow, row);
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    public async Task GetPersonAmendedRow_WithDateOfBirthExportType_ReturnsExpectedContent(Gender gender)
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
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

        var user = await TestData.CreateApplicationUserAsync();

        var option = new CapitaTpsUserOption() { CapitaTpsUserId = user.UserId };
        var jobOption = Options.Create(option);
        var job = new CapitaExportAmendJob(blobServiceClientMock.Object, Fixture.Logger.Object, dbContext, Clock, jobOption);
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithDateOfBirth(new DateOnly(1983, 01, 07));
            x.WithGender(gender);
        });
        var trsPerson = dbContext.Persons.Single(x => x.PersonId == person.PersonId);
        var amendedPerson = new CapitaExportAmendJobResult()
        {
            Trn = trsPerson.Trn!,
            LastName = trsPerson.LastName,
            DateOfBirth = trsPerson.DateOfBirth,
            NationalInsuranceNumber = null,
            PersonId = trsPerson.PersonId,
            Gender = trsPerson.Gender,
            ChangeType = PersonAttributesChanges.DateOfBirth,
            Created = DateTime.UtcNow
        };

        // Act
        var row = job.GetPersonAmendedRow(amendedPerson, CapitaAmendExportType.DateOfBirth);

        // Assert
        var lastNameFixed = trsPerson.LastName.Length > 6
            ? trsPerson.LastName.Substring(0, 6)
            : trsPerson.LastName.PadRight(6);
        var expectedRow = $"{trsPerson.Trn}" +
                          $"{(int)trsPerson.Gender!}" +
                          $"//" +
                          $"{lastNameFixed}" +
                          $"{new string(' ', 1)}" +
                          $"{trsPerson.DateOfBirth!.Value:ddMMyy*}" +
                          $"{new string(' ', 1)}" +
                          $"{new string(' ', 9)}" +
                          $"{new string(' ', 44)}" +
                          $"1211ZE1*";
        Assert.Equal(EXPECTED_ROW_LENGTH, row.Length);
        Assert.Equal(expectedRow, row);
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    public async Task GetPersonAmendedRow_WithNationalInsurance_ReturnsExpectedContent(Gender gender)
    {
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            // Arrange
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

            var user = await TestData.CreateApplicationUserAsync();

            var option = new CapitaTpsUserOption() { CapitaTpsUserId = user.UserId };
            var jobOption = Options.Create(option);
            var job = new CapitaExportAmendJob(blobServiceClientMock.Object, Fixture.Logger.Object, dbContext, Clock, jobOption);
            var person = await TestData.CreatePersonAsync(x =>
            {
                x.WithDateOfBirth(new DateOnly(1983, 01, 07));
                x.WithGender(gender);
                x.WithNationalInsuranceNumber(true);
            });
            var trsPerson = dbContext.Persons.Single(x => x.PersonId == person.PersonId);
            var amendedPerson = new CapitaExportAmendJobResult()
            {
                Trn = trsPerson.Trn!,
                LastName = trsPerson.LastName,
                DateOfBirth = trsPerson.DateOfBirth,
                NationalInsuranceNumber = person.NationalInsuranceNumber,
                PersonId = trsPerson.PersonId,
                Gender = trsPerson.Gender,
                ChangeType = PersonAttributesChanges.NationalInsuranceNumber,
                Created = DateTime.UtcNow
            };

            // Act
            var row = job.GetPersonAmendedRow(amendedPerson, CapitaAmendExportType.NINumber);

            // Assert
            var lastNameFixed = trsPerson.LastName.Length > 6
                ? trsPerson.LastName.Substring(0, 6)
                : trsPerson.LastName.PadRight(6);
            var expectedRow = $"{trsPerson.Trn}" +
                              $"{(int)trsPerson.Gender!}" +
                              $"//" +
                              $"{lastNameFixed}" +
                              $"{new string(' ', 1)}" +
                              $"{new string(' ', 7)}" +
                              $"{new string(' ', 1)}" +
                              $"{person.NationalInsuranceNumber}" +
                              $"{new string(' ', 44)}" +
                              $" 321ZE2*";
            Assert.Equal(EXPECTED_ROW_LENGTH, row.Length);
            Assert.Equal(expectedRow, row);
        });
    }

    [Theory]
    [InlineData(CapitaAmendExportType.DateOfBirth, "1211ZE1*")]
    [InlineData(CapitaAmendExportType.NINumber, " 321ZE2*")]
    public async Task GetUpdateCode_ReturnsExpectedContent(CapitaAmendExportType exportType, string expectedUpdateCode)
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
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

        var user = await TestData.CreateApplicationUserAsync();

        var option = new CapitaTpsUserOption() { CapitaTpsUserId = user.UserId };
        var jobOption = Options.Create(option);
        var job = new CapitaExportAmendJob(blobServiceClientMock.Object, Fixture.Logger.Object, dbContext, Clock, jobOption);

        // Act
        var updateCode = job.GetUpdateCode(exportType);

        // Assert
        Assert.Equal(expectedUpdateCode, updateCode);
    }

    [Fact]
    public async Task GetFileName_ReturnsExpectedFileName()
    {
        // Arrange
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
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

        var user = await TestData.CreateApplicationUserAsync();

        var option = new CapitaTpsUserOption() { CapitaTpsUserId = user.UserId };
        var jobOption = Options.Create(option);
        var job = new CapitaExportAmendJob(blobServiceClientMock.Object, Fixture.Logger.Object, dbContext, Clock, jobOption);
        var expectedFileName = $"Reg01_DTR_{Clock.UtcNow.ToString("yyyyMMdd")}_{Clock.UtcNow.ToString("HHmmss")}_Amend.txt"; ;

        // Act
        var fileName = job.GetFileName(Clock);

        // Assert
        Assert.Equal(expectedFileName, fileName);
    }

    bool MatchesExpectedRowData(IntegrationTransactionRecord record, string expectedRowData, Person person) =>
        record.PersonId == person.PersonId &&
        record.FailureMessage == null &&
        record.Duplicate == null &&
        record.RowData != null &&
        record.RowData.Length == EXPECTED_ROW_LENGTH &&
        record.RowData == expectedRowData &&
        record.Status == IntegrationTransactionRecordStatus.Success;

    public async Task InitializeAsync()
    {
        await DbFixture.DbHelper.ClearDataAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public class CapitaExportAmendJobFixture : IAsyncLifetime
{
    public CapitaExportAmendJobFixture(
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

        Logger = new Mock<ILogger<CapitaExportAmendJob>>();

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

    public Mock<ILogger<CapitaExportAmendJob>> Logger { get; }

    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    public IOrganizationServiceAsync2 OrganizationService { get; }

    public Mock<IFileService> BlobStorageFileService { get; } = new Mock<IFileService>();

    public Mock<BlobServiceClient> BlobServiceClient { get; } = new Mock<BlobServiceClient>();
}
