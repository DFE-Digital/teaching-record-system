using System.Data;
using System.Diagnostics;
using System.ServiceModel;
using System.Text;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.Services.CrmEntityChanges;

namespace QualifiedTeachersApi.Services.DqtReporting;

public partial class DqtReportingService : BackgroundService
{
    public const string ChangesKey = "DqtReporting";
    public const string CrmClientName = "DqtReporting";
    public const string ProcessChangesOperationName = "DqtReporting: process changes";

    private const int MaxParameters = 1024;
    private const int PageSize = 250;
    private const int MaxUpsertBatchSize = 100;
    private const string MetricPrefix = "DqtReporting: ";
    private const int MaxEntityTypesToProcessConcurrently = 10;

    private readonly DqtReportingOptions _options;
    private readonly ICrmEntityChangesService _crmEntityChangesService;
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly IClock _clock;
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<DqtReportingService> _logger;
    private readonly Dictionary<string, (EntityMetadata EntityMetadata, EntityTableMapping EntityTableMapping)> _entityMetadata = new();

    public DqtReportingService(
        IOptions<DqtReportingOptions> optionsAccessor,
        ICrmEntityChangesService crmEntityChangesService,
        IDataverseAdapter dataverseAdapter,
        IClock clock,
        TelemetryClient telemetryClient,
        ILogger<DqtReportingService> logger)
    {
        _options = optionsAccessor.Value;
        _crmEntityChangesService = crmEntityChangesService;
        _dataverseAdapter = dataverseAdapter;
        _clock = clock;
        _telemetryClient = telemetryClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await LoadEntityMetadata();

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.PollIntervalSeconds));

        do
        {
            try
            {
                await ProcessChanges(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlException && sqlException.IsTransient)
                {
                    _logger.LogWarning(ex, "Transient SQL exception thrown.");
                    continue;
                }

                // If we've hit CRM API limits, back off and retry later
                if (ex.IsCrmRateLimitException(out var retryAfter))
                {
                    _logger.LogWarning(ex, "Hit CRM rate limit error.");

                    await Task.Delay(retryAfter, stoppingToken);
                    continue;
                }

                _logger.LogError(ex, "Failed processing entity changes.");
                return;
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    internal async Task LoadEntityMetadata()
    {
        foreach (var entity in _options.Entities)
        {
            const int maxAttempts = 5;
            var attempts = 0;

            while (true)
            {
                attempts++;

                try
                {
                    var entityMetadata = await _dataverseAdapter.GetEntityMetadata(entity, EntityFilters.Default | EntityFilters.Attributes);

                    if (entityMetadata.ChangeTrackingEnabled != true)
                    {
                        throw new Exception($"Entity '{entity}' does not have change tracking enabled.");
                    }

                    var entityTableMapping = EntityTableMapping.Create(entityMetadata);

                    _entityMetadata[entity] = (entityMetadata, entityTableMapping);
                    break;
                }
                catch (Exception ex)
                {
                    if (ex.IsCrmRateLimitException(out var retryAfter) && attempts < maxAttempts)
                    {
                        _logger.LogWarning(ex, "Failed retrieving metadata from CRM.");

                        await Task.Delay(retryAfter);
                        continue;
                    }

                    throw;
                }
            }
        }
    }

    internal async Task ProcessChanges(CancellationToken cancellationToken)
    {
        using var operation = _telemetryClient.StartOperation<DependencyTelemetry>(ProcessChangesOperationName);

        var metricsGate = new object();
        var totalUpdates = 0;

        try
        {
            await Parallel.ForEachAsync(
                _options.Entities,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = _options.ProcessAllEntityTypesConcurrently ? MaxEntityTypesToProcessConcurrently : 1,
                    CancellationToken = cancellationToken
                },
                async (entityType, ct) =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    int processedForEntityType = 0;

                    try
                    {
                        await ProcessChangesForEntityType(entityType, processedCount => processedForEntityType = processedCount, ct);
                    }
                    catch (Exception ex)
                    {
                        if (!(ex is OperationCanceledException && ct.IsCancellationRequested))
                        {
                            _telemetryClient.TrackException(new ExceptionTelemetry()
                            {
                                Exception = ex,
                                Properties =
                                {
                                    { "Entity type", entityType }
                                }
                            });
                        }

                        throw;
                    }
                    finally
                    {
                        stopwatch.Stop();

                        _telemetryClient.TrackMetric(new MetricTelemetry()
                        {
                            Name = $"{MetricPrefix}Batch processing time - {entityType}",
                            Sum = stopwatch.Elapsed.TotalSeconds
                        });

                        lock (metricsGate)
                        {
                            totalUpdates += processedForEntityType;
                        }
                    }
                });
        }
        finally
        {
            _telemetryClient.TrackMetric(new MetricTelemetry()
            {
                Name = $"{MetricPrefix}Total updates processed",
                Sum = totalUpdates
            });
        }
    }

    internal async Task ProcessChangesForEntityType(string entityLogicalName, Action<int> onProcessedCountUpdated, CancellationToken cancellationToken)
    {
        var totalProcessed = 0;
        var insertedCount = 0;
        var updatedCount = 0;
        var deletedCount = 0;

        var columns = new ColumnSet(allColumns: true);

        try
        {
            var changesEnumerable = _crmEntityChangesService.GetEntityChanges(ChangesKey, CrmClientName, entityLogicalName, columns, PageSize)
                .WithCancellation(cancellationToken);

            await foreach (var changes in changesEnumerable)
            {
                totalProcessed += changes.Length;

                var newOrUpdatedItems = new List<NewOrUpdatedItem>();
                var removedOrDeletedItems = new List<RemovedOrDeletedItem>();

                foreach (var change in changes)
                {
                    if (change is NewOrUpdatedItem newOrUpdatedItem)
                    {
                        newOrUpdatedItems.Add(newOrUpdatedItem);
                    }
                    else if (change is RemovedOrDeletedItem removedOrDeletedItem)
                    {
                        removedOrDeletedItems.Add(removedOrDeletedItem);
                    }
                    else
                    {
                        throw new Exception($"Received unknown change type: '{change.GetType().Name}'.");
                    }
                }

                await HandleNewOrUpdatedItems(newOrUpdatedItems, c => insertedCount += c, c => updatedCount += c, cancellationToken);

                // It's important deleted items are processed *after* upserts, otherwise we may resurrect a deleted record
                await HandleRemovedOrDeletedItems(removedOrDeletedItems, c => deletedCount += c, cancellationToken);
            }
        }
        finally
        {
            if (totalProcessed > 0)
            {
                _telemetryClient.TrackMetric(new MetricTelemetry()
                {
                    Name = $"{MetricPrefix}Updates processed - {entityLogicalName}",
                    Sum = totalProcessed
                });

                _telemetryClient.TrackMetric(new MetricTelemetry()
                {
                    Name = $"{MetricPrefix}Records added - {entityLogicalName}",
                    Sum = insertedCount
                });

                _telemetryClient.TrackMetric(new MetricTelemetry()
                {
                    Name = $"{MetricPrefix}Records updated - {entityLogicalName}",
                    Sum = updatedCount
                });

                _telemetryClient.TrackMetric(new MetricTelemetry()
                {
                    Name = $"{MetricPrefix}Records deleted - {entityLogicalName}",
                    Sum = deletedCount
                });

                onProcessedCountUpdated(totalProcessed);
            }
        }
    }

    private async Task HandleNewOrUpdatedItems(
        IReadOnlyCollection<NewOrUpdatedItem> newOrUpdatedItems,
        Action<int> onInsertedCounterUpdated,
        Action<int> onUpdatedCounterUpdated,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (newOrUpdatedItems.Count == 0)
        {
            return;
        }

        var entities = newOrUpdatedItems.Select(e => e.NewOrUpdatedEntity);
        var entityLogicalName = entities.First().LogicalName;
        var entityTableMapping = _entityMetadata[entityLogicalName].EntityTableMapping;
        string tempTableName = $"#import-{entityLogicalName}";

        var dataTable = CreateDataTable();

        using var conn = new SqlConnection(_options.ReportingDbConnectionString);
        await conn.OpenAsync();

        await CreateTempTable();

        foreach (var chunk in entities.Chunk(MaxUpsertBatchSize))
        {
            await UpsertRows(chunk);
        }

        await DropTempTable();

        DataTable CreateDataTable()
        {
            var dataTable = new DataTable();

            foreach (var attr in entityTableMapping.Attributes)
            {
                foreach (var column in attr.ColumnDefinitions)
                {
                    dataTable.Columns.Add(column.ColumnName, column.Type);
                }
            }

            return dataTable;
        }

        async Task CreateTempTable()
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.AppendFormat("create table [{0}] (\n", tempTableName);
            int columnIndex = 0;

            foreach (var attr in entityTableMapping.Attributes)
            {
                foreach (var column in attr.ColumnDefinitions)
                {
                    AddColumn($"[{column.ColumnName}] {column.ColumnDefinition}");
                }
            }

            sqlBuilder.Append("\n)");
            var sql = sqlBuilder.ToString();

            var command = new SqlCommand(sql);
            command.Connection = conn;

            await command.ExecuteNonQueryAsync();

            void AddColumn(string definition)
            {
                if (columnIndex > 0)
                {
                    sqlBuilder.Append(",\n");
                }

                sqlBuilder.AppendFormat("\t{0}", definition);

                columnIndex++;
            }
        }

        async Task UpsertRows(Entity[] entities)
        {
            dataTable.Rows.Clear();

            var insertedCount = 0;
            var updatedCount = 0;

            foreach (var entity in entities)
            {
                var rowData = new object?[entityTableMapping.ColumnCount];
                var i = 0;

                foreach (var attr in entityTableMapping.Attributes)
                {
                    foreach (var column in attr.ColumnDefinitions)
                    {
                        rowData[i++] = entity.Attributes.TryGetValue(attr.AttributeName, out var attrValue) ?
                            column.GetColumnValueFromAttribute(attrValue) :
                            null;
                    }
                }

                dataTable.Rows.Add(rowData);
            }

            using var txn = conn.BeginTransaction();

            using (var sqlBulkCopy = new SqlBulkCopy(conn, new SqlBulkCopyOptions(), txn))
            {
                foreach (DataColumn column in dataTable.Columns)
                {
                    sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column.ColumnName, column.ColumnName));
                }

                sqlBulkCopy.BulkCopyTimeout = 0;
                sqlBulkCopy.DestinationTableName = tempTableName;

                await sqlBulkCopy.WriteToServerAsync(dataTable, cancellationToken);
            }

            var mergeCommand = entityTableMapping.GetMergeSqlCommand(tempTableName, _clock);
            mergeCommand.Connection = conn;
            mergeCommand.Transaction = txn;

            using (var reader = await mergeCommand.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    var action = reader.GetString(0);

                    if (action == "UPDATE")
                    {
                        updatedCount++;
                    }
                    else
                    {
                        Debug.Assert(action == "INSERT");
                        insertedCount++;
                    }
                }
            }

            await txn.CommitAsync(cancellationToken);

            onInsertedCounterUpdated(insertedCount);
            onUpdatedCounterUpdated(updatedCount);
        }

        async Task DropTempTable()
        {
            var sql = $"drop table [{tempTableName}]";

            var command = new SqlCommand(sql);
            command.Connection = conn;

            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task HandleRemovedOrDeletedItems(
        IReadOnlyCollection<RemovedOrDeletedItem> removedOrDeletedItems,
        Action<int> onDeletedCounterUpdated,
        CancellationToken cancellationToken)
    {
        if (removedOrDeletedItems.Count == 0)
        {
            return;
        }

        var entityLogicalName = removedOrDeletedItems.First().RemovedItem.LogicalName;
        var entityTableMapping = _entityMetadata[entityLogicalName].EntityTableMapping;
        var ids = removedOrDeletedItems.Select(e => e.RemovedItem.Id).ToArray();

        using var conn = new SqlConnection(_options.ReportingDbConnectionString);
        await conn.OpenAsync();

        foreach (var chunk in ids.Chunk(MaxParameters))
        {
            var command = entityTableMapping.GetDeleteSqlCommand(chunk, _clock);
            command.Connection = conn;

            var deleted = await command.ExecuteNonQueryAsync(cancellationToken);

            onDeletedCounterUpdated(deleted);
        }
    }
}

file static class ExceptionExtensions
{
    public static bool IsCrmRateLimitException(this Exception exception, out TimeSpan retryAfter)
    {
        if (exception is FaultException<OrganizationServiceFault> fault &&
            fault.Detail.ErrorDetails.TryGetValue("Retry-After", out var retryAfterObj) &&
            retryAfterObj is TimeSpan retryAfterTs)
        {
            retryAfter = retryAfterTs;
            return true;
        }

        retryAfter = default;
        return false;
    }
}
