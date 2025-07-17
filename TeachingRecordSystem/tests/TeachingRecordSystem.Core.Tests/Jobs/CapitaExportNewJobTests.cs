using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Jobs;
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


    [Fact]
    public async Task GetPersons_WithoutLatRunDate_ReturnsReturnsAllPersons()
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync();

        // Act
        var newPersons = await Job.GetNewPersonsAsync(null);

        // Assert
        Assert.Collection(newPersons, p1 =>
        {
            Assert.Equal(person1.Trn, p1.Trn);
        });
    }

    //created date after lastRunDate - returns new persons
    //created date before lastRunDate - returns no persons
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

        var blobServiceClient = new Mock<BlobServiceClient>();
        Logger = new Mock<ILogger<CapitaExportNewJob>>();
        Job = ActivatorUtilities.CreateInstance<CapitaExportNewJob>(provider, blobServiceClient.Object, Logger.Object, Clock);
        TestData = new TestData(
            dbFixture.GetDbContextFactory(),
            OrganizationService,
            referenceDataCache,
            Clock,
            trnGenerator,
            TestDataSyncConfiguration.Sync(Helper));
    }

    public DbFixture DbFixture { get; }

    public TestData TestData { get; }

    public TestableClock Clock { get; }

    public TrsDataSyncHelper Helper { get; }

    public Mock<ILogger<CapitaExportNewJob>> Logger { get; }

    Task IAsyncLifetime.InitializeAsync() => DbFixture.WithDbContextAsync(dbContext => dbContext.Events.ExecuteDeleteAsync());

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    public IOrganizationServiceAsync2 OrganizationService { get; }

    public CapitaExportNewJob Job { get; }

    public Mock<IFileService> BlobStorageFileService { get; } = new Mock<IFileService>();
}
