using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Core.Dqt.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Dqt.Tests.Services.TrsDataSync;

public class TrsDataSyncServiceFixture
{
    public TrsDataSyncServiceFixture(DbFixture dbFixture)
    {
        DbFixture = dbFixture;
        Clock = new TestableClock();
    }

    public DbFixture DbFixture { get; }

    public TestableClock Clock { get; }

    public Task PublishChangedItemAndConsume(IChangedItem changedItem) =>
        WithService(async (service, changesObserver) =>
        {
            changesObserver.OnNext(new IChangedItem[] { changedItem });
            var processTask = service.ProcessChangesForEntityType(Contact.EntityLogicalName, CancellationToken.None);
            changesObserver.OnCompleted();
            await processTask;
        });

    public async Task WithService(Func<TrsDataSyncService, IObserver<IChangedItem[]>, Task> action)
    {
        var options = Options.Create(new TrsDataSyncServiceOptions()
        {
            CrmConnectionString = "dummy",
            Entities = new[] { Contact.EntityLogicalName },
            PollIntervalSeconds = 60,
            ProcessAllEntityTypesConcurrently = false,
            IgnoreInvalidData = false,
            RunService = true
        });

        using var crmEntityChangesService = new TestableCrmEntityChangesService();
        var changesObserver = crmEntityChangesService.GetChangedItemsObserver(Contact.EntityLogicalName);

        var helper = new TrsDataSyncHelper(DbFixture.GetDbContextFactory(), Clock);

        var logger = new NullLogger<TrsDataSyncService>();

        var service = new TrsDataSyncService(
            crmEntityChangesService,
            helper,
            options,
            logger);

        await action(service, changesObserver);
    }
}
