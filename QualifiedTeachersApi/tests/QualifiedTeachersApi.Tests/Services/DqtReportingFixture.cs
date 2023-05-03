using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.Services.CrmEntityChanges;
using QualifiedTeachersApi.Services.DqtReporting;
using QualifiedTeachersApi.Services.TrnGenerationApi;
using QualifiedTeachersApi.Tests.Infrastructure;

namespace QualifiedTeachersApi.Tests.Services;

public class DqtReportingFixture
{
    private readonly ServiceClient _serviceClient;
    private readonly IMemoryCache _memoryCache;

    public DqtReportingFixture(TestConfiguration testConfiguration, ServiceClient serviceClient, IMemoryCache memoryCache)
    {
        _serviceClient = serviceClient;
        _memoryCache = memoryCache;
        ReportingDbConnectionString = testConfiguration.Configuration["DqtReporting:ReportingDbConnectionString"] ??
            throw new Exception("DqtReporting:ReportingDbConnectionString configuration key is missing.");

        var migrator = new Migrator(ReportingDbConnectionString);
        migrator.DropAllTables();
        migrator.MigrateDb();
    }

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
            Entities = new[] { Contact.EntityLogicalName },
            PollIntervalSeconds = 60,
            ProcessAllEntityTypesConcurrently = false,
            ReportingDbConnectionString = ReportingDbConnectionString,
            RunService = true
        });

        using var crmEntityChangesService = new TestableCrmEntityChangesService();
        var changesObserver = crmEntityChangesService.GetChangedItemsObserver(Contact.EntityLogicalName);

        var trnGenerationApiClient = Mock.Of<ITrnGenerationApiClient>();

        var dataverseAdapter = new DataverseAdapter(_serviceClient, new TestableClock(), _memoryCache, trnGenerationApiClient);

        var telemetryClient = new TelemetryClient(new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration());

        var logger = new NullLogger<DqtReportingService>();

        var service = new DqtReportingService(
            options,
            crmEntityChangesService,
            dataverseAdapter,
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
            string key,
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
