using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Services.TrsDataSync;

public class TrsDataSyncServiceFixture : IAsyncLifetime
{
    public TrsDataSyncServiceFixture(
        DbFixture dbFixture,
        IOrganizationServiceAsync2 organizationService,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator)
    {
        DbFixture = dbFixture;
        Clock = new();

        var dbContextFactory = dbFixture.GetDbContextFactory();

        Helper = new TrsDataSyncHelper(
            dbContextFactory,
            organizationService,
            referenceDataCache,
            Clock);

        TestData = new TestData(
            dbContextFactory,
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

    public Task PublishChangedItemAndConsume(string modelType, IChangedItem changedItem)
    {
        var (entityLogicalname, _) = TrsDataSyncHelper.GetEntityInfoForModelType(modelType);

        return WithService(entityLogicalname, async (service, changesObserver) =>
        {
            changesObserver.OnNext([changedItem]);
            var processTask = service.ProcessChangesForModelType(modelType, CancellationToken.None);
            changesObserver.OnCompleted();
            await processTask;
        });
    }

    public async Task WithService(string entityLogicalName, Func<TrsDataSyncService, IObserver<IChangedItem[]>, Task> action)
    {
        var options = Options.Create(new TrsDataSyncServiceOptions()
        {
            CrmConnectionString = "dummy",
            ModelTypes = [TrsDataSyncHelper.ModelTypes.Person, TrsDataSyncHelper.ModelTypes.MandatoryQualification],
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

    Task IAsyncLifetime.InitializeAsync() => DbFixture.DbHelper.ClearData();
}
