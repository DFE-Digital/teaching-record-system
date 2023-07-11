using System.Collections.Concurrent;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Services.TrnGenerationApi;
using TeachingRecordSystem.Dqt.Services.CrmEntityChanges;
using TeachingRecordSystem.Dqt.Services.DqtReporting;

namespace TeachingRecordSystem.Dqt.Tests.Services.DqtReporting;

public class DqtReportingFixture
{
    private readonly ServiceClient _serviceClient;
    private readonly IMemoryCache _memoryCache;

    public DqtReportingFixture(IConfiguration configuration, ServiceClient serviceClient, IMemoryCache memoryCache)
    {
        _serviceClient = serviceClient;
        _memoryCache = memoryCache;
        ReportingDbConnectionString = configuration.GetRequiredValue("DqtReporting:ReportingDbConnectionString");

        var migrator = new Migrator(ReportingDbConnectionString);
        migrator.DropAllTables();
        migrator.MigrateDb();
    }

    public IClock Clock { get; } = new TestableClock();

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

        var trnGenerationApiClient = Mock.Of<ITrnGenerationApiClient>();

        var dataverseAdapter = new DataverseAdapter(_serviceClient, Clock, _memoryCache, trnGenerationApiClient);

        var telemetryClient = new TelemetryClient(new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration());

        var logger = new NullLogger<DqtReportingService>();

        var service = new DqtReportingService(
            options,
            crmEntityChangesService,
            dataverseAdapter,
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
