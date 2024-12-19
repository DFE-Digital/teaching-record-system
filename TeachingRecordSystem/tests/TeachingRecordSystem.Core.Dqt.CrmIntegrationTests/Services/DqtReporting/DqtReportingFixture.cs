using Medallion.Threading.FileSystem;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Core.Services.DqtReporting;
using TeachingRecordSystem.TestCommon;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.Services.DqtReporting;

public class DqtReportingFixture
{
    private readonly CrmClientFixture _crmClientFixture;

    public DqtReportingFixture(CrmClientFixture crmClientFixture)
    {
        _crmClientFixture = crmClientFixture;
        ReportingDbConnectionString = crmClientFixture.Configuration.GetRequiredValue("DqtReporting:ReportingDbConnectionString");

        var migrator = new Migrator(ReportingDbConnectionString);
        migrator.DropAllTables();
        migrator.MigrateDb();
    }

    public IClock Clock => _crmClientFixture.Clock;

    public DbFixture DbFixture => _crmClientFixture.DbFixture;

    public string ReportingDbConnectionString { get; }

    public string TrsDbReplicationSlotName { get; } = "dqt_rep_sync_slot_test";

    public CrmClientFixture.TestDataScope CreateTestDataScope(bool withSync) => _crmClientFixture.CreateTestDataScope(withSync);

    public Task PublishChangedItemsAndConsume(params IChangedItem[] changedItems) =>
        WithService(async (service, changesObserver) =>
        {
            await service.LoadEntityMetadataAsync();

            changesObserver.OnNext(changedItems);
            var processTask = service.ProcessCrmChangesForEntityTypeAsync(Contact.EntityLogicalName, CancellationToken.None);
            changesObserver.OnCompleted();
            await processTask;
        });

    public async Task WithService(Func<DqtReportingService, IObserver<IChangedItem[]>, Task> action)
    {
        var options = Options.Create(new DqtReportingOptions()
        {
            CrmConnectionString = "dummy",
            Entities = [Contact.EntityLogicalName],
            PollIntervalSeconds = 60,
            ProcessAllEntityTypesConcurrently = false,
            ReportingDbConnectionString = ReportingDbConnectionString,
            RunService = true,
            TrsDbReplicationSlotName = TrsDbReplicationSlotName
        });

        using var crmEntityChangesService = new TestableCrmEntityChangesService();
        var changesObserver = crmEntityChangesService.GetChangedItemsObserver(Contact.EntityLogicalName);

        var lockFileDirectory = Path.Combine(Path.GetTempPath(), "trstestlocks");
        var distributedLockProvider = new FileDistributedSynchronizationProvider(new DirectoryInfo(lockFileDirectory));

        var logger = new NullLogger<DqtReportingService>();

        var service = new DqtReportingService(
            options,
            crmEntityChangesService,
            _crmClientFixture.CreateQueryDispatcher(),
            distributedLockProvider,
            Clock,
            _crmClientFixture.Configuration,
            logger);

        await action(service, changesObserver);
    }
}
