using System.Collections.Concurrent;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Services.CrmEntityChanges;
using TeachingRecordSystem.Core.Dqt.Services.DqtReporting;

namespace TeachingRecordSystem.Core.Dqt.Tests.Services.DqtReporting;

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

    public Task PublishChangedItemAndConsume(IChangedItem changedItem) =>
        WithService(async (service, changesObserver) =>
        {
            await service.LoadEntityMetadata();

            changesObserver.OnNext(new IChangedItem[] { changedItem });
            var processTask = service.ProcessChangesForEntityType(Contact.EntityLogicalName, CancellationToken.None);
            changesObserver.OnCompleted();
            await processTask;
        });

    public async Task WithService(Func<DqtReportingService, IObserver<IChangedItem[]>, Task> action)
    {
        var options = Options.Create(new DqtReportingOptions()
        {
            CrmConnectionString = "dummy",
            Entities = new[] { Contact.EntityLogicalName },
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

    private class TestableCrmEntityChangesService : ICrmEntityChangesService, IDisposable
    {
        private readonly ConcurrentDictionary<string, System.Reactive.Subjects.ReplaySubject<IChangedItem[]>> _entityTypeSubjects = new();
        private bool _disposed = false;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            var subjects = _entityTypeSubjects.Values.ToArray();

            foreach (var s in subjects)
            {
                s.Dispose();
            }

            _entityTypeSubjects.Clear();
            _disposed = true;
        }

        public IObserver<IChangedItem[]> GetChangedItemsObserver(string entityLogicalName)
        {
            ThrowIfDisposed();

            return _entityTypeSubjects.GetOrAdd(entityLogicalName, _ => new System.Reactive.Subjects.ReplaySubject<IChangedItem[]>());
        }

        public IAsyncEnumerable<IChangedItem[]> GetEntityChanges(
            string changesKey,
            string serviceClientName,
            string entityLogicalName,
            ColumnSet columns,
            int pageSize = 1000,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            var subject = _entityTypeSubjects.GetOrAdd(entityLogicalName, _ => new System.Reactive.Subjects.ReplaySubject<IChangedItem[]>());
            return subject.ToAsyncEnumerable();
        }

        private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
