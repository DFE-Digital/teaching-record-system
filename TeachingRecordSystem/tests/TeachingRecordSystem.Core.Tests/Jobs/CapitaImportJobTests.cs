using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection(nameof(DisableParallelization))]
public class CapitaImportJobTests(CapitaImportJobFixture Fixture) : IClassFixture<CapitaImportJobFixture>, IAsyncLifetime
{
    private DbFixture DbFixture => Fixture.DbFixture;

    private IClock Clock => Fixture.Clock;

    private TestData TestData => Fixture.TestData;

    private CapitaImportJob Job => Fixture.Job;


    [Fact]
    public async Task Import_WithNoMatchesCreatesPerson_ReturnsExpectedRecords()
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
        var expectedGender = Gender.Female;
        var expectedLastName = Faker.Name.Last();
        var expectedStatus = IntegrationTransactionImportStatus.Success;
        await using var dbContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        var csvContent = $"{expectedTrn};{(int)expectedGender};{expectedLastName};;;{expectedDob.ToString("yyyyMMdd")};{expectedNI};;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync(reader, fileName);

        // Assert
        var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionId);
        var person = dbContext.Persons.FirstOrDefault(x => x.Trn == expectedTrn);
        Assert.NotNull(person);
        Assert.Equal(expectedTrn, person.Trn);
        Assert.Equal(expectedDob, person.DateOfBirth);
        Assert.Equal(expectedGender, person.Gender);
        Assert.Equal(expectedLastName, person.LastName);
        Assert.Equal(expectedNI, person.NationalInsuranceNumber);
        //FirstName
        //MiddleName

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
                    //Assert.NotNull(item1.RowData);
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
        var integrationTransaction = dbContext.IntegrationTransactions.Include(x=>x.IntegrationTransactionRecords).Single(x=>x.IntegrationTransactionId == integrationTransactionId);
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

    public async Task InitializeAsync() => await DbFixture.DbHelper.ClearDataAsync();

    public Task DisposeAsync() => Task.CompletedTask;
}

public class CapitaImportJobFixture : IAsyncLifetime
{
    public CapitaImportJobFixture(
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

        Job = ActivatorUtilities.CreateInstance<CapitaImportJob>(provider, blobServiceClientMock.Object, Logger.Object, Clock);
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

    public Mock<ILogger<CapitaImportJob>> Logger { get; }

    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    public IOrganizationServiceAsync2 OrganizationService { get; }

    public CapitaImportJob Job { get; }

    public Mock<IFileService> BlobStorageFileService { get; } = new Mock<IFileService>();

    public Mock<BlobServiceClient> BlobServiceClient { get; } = new Mock<BlobServiceClient>();
}
