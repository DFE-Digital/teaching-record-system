using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Services.TrsDataSync;

public class TrsDataSyncServiceFixture : IAsyncLifetime
{
    public TrsDataSyncServiceFixture(
        DbFixture dbFixture,
        IOrganizationServiceAsync2 organizationService,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator,
        ILoggerFactory loggerFactory)
    {
        DbFixture = dbFixture;
        Clock = new();
        AuditRepository = new();

        Helper = new TrsDataSyncHelper(
            dbFixture.GetDataSource(),
            organizationService,
            referenceDataCache,
            Clock,
            AuditRepository,
            loggerFactory.CreateLogger<TrsDataSyncHelper>(),
            BlobStorageFileService.Object);

        TestData = new TestData(
            dbFixture.GetDbContextFactory(),
            organizationService,
            referenceDataCache,
            Clock,
            trnGenerator,
            TestDataSyncConfiguration.Sync(Helper));
    }

    public TestableClock Clock { get; }

    public DbFixture DbFixture { get; }

    public TrsDataSyncHelper Helper { get; }

    public TestData TestData { get; }

    public TestableAuditRepository AuditRepository { get; }

    public Mock<IFileService> BlobStorageFileService { get; } = new Mock<IFileService>();

    public Task PublishChangedItemAndConsumeAsync(string modelType, IChangedItem changedItem)
    {
        var (entityLogicalname, _) = TrsDataSyncHelper.GetEntityInfoForModelType(modelType);

        return WithServiceAsync(entityLogicalname, async (service, changesObserver) =>
        {
            changesObserver.OnNext([changedItem]);
            var processTask = service.ProcessChangesForModelTypeAsync(modelType, CancellationToken.None);
            changesObserver.OnCompleted();
            await processTask;
        });
    }

    public async Task WithServiceAsync(string entityLogicalName, Func<TrsDataSyncService, IObserver<IChangedItem[]>, Task> action)
    {
        var options = Options.Create(new TrsDataSyncServiceOptions()
        {
            CrmConnectionString = "dummy",
            ModelTypes = [TrsDataSyncHelper.ModelTypes.Person],
            PollIntervalSeconds = 60,
            IgnoreInvalidData = false,
            RunService = true
        });

        using var crmEntityChangesService = new TestableCrmEntityChangesService();
        var changesObserver = crmEntityChangesService.GetChangedItemsObserver(entityLogicalName);

        var logger = new NullLogger<TrsDataSyncService>();

        var service = new TrsDataSyncService(
            crmEntityChangesService,
            Helper,
            options,
            logger);

        await action(service, changesObserver);
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.InitializeAsync() => DbFixture.DbHelper.ClearDataAsync();
}
