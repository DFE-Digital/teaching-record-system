using Microsoft.ApplicationInsights;
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

    public string ReportingDbConnectionString { get; }

    public Task PublishChangedItemsAndConsume(params IChangedItem[] changedItems) =>
        WithService(async (service, changesObserver) =>
        {
            await service.LoadEntityMetadata();

            changesObserver.OnNext(changedItems);
            var processTask = service.ProcessChangesForEntityType(Contact.EntityLogicalName, CancellationToken.None);
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
            RunService = true
        });

        using var crmEntityChangesService = new TestableCrmEntityChangesService();
        var changesObserver = crmEntityChangesService.GetChangedItemsObserver(Contact.EntityLogicalName);

        var telemetryClient = new TelemetryClient(new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration());

        var logger = new NullLogger<DqtReportingService>();

        var service = new DqtReportingService(
            options,
            crmEntityChangesService,
            _crmClientFixture.CreateQueryDispatcher(),
            Clock,
            telemetryClient,
            logger);

        await action(service, changesObserver);
    }
}
