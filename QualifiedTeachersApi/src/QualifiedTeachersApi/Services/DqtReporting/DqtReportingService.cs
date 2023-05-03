using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
    public const string ProcessChangesOperationName = "DqtReporting: process changes";

    private const int MaxParameters = 1024;
    private const int MaxUpsertBatchSize = 500;
    private const string MetricPrefix = "DqtReporting: ";

    private readonly DqtReportingOptions _options;
    private readonly ICrmEntityChangesService _crmEntityChangesService;
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<DqtReportingService> _logger;
    private readonly Dictionary<string, (EntityMetadata EntityMetadata, EntityTableMapping EntityTableMapping)> _entityMetadata = new();

    public DqtReportingService(
        IOptions<DqtReportingOptions> optionsAccessor,
        ICrmEntityChangesService crmEntityChangesService,
        IDataverseAdapter dataverseAdapter,
        TelemetryClient telemetryClient,
        ILogger<DqtReportingService> logger)
    {
        _options = optionsAccessor.Value;
        _crmEntityChangesService = crmEntityChangesService;
        _dataverseAdapter = dataverseAdapter;
        _telemetryClient = telemetryClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await LoadEntityMetadata();

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.PollIntervalSeconds));

        while (await timer.WaitForNextTickAsync(stoppingToken))
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
                _logger.LogError(ex, "Failed processing entity changes.");

                // We assume non-transient SqlExceptions are bugs; log the error, and stop the service gracefully.
                if (ex is SqlException sqlException && !sqlException.IsTransient)
                {
                    return;
                }

                throw;
            }
        }
    }

    internal async Task LoadEntityMetadata()
    {
        foreach (var entity in _options.Entities)
        {
            var entityMetadata = await _dataverseAdapter.GetEntityMetadata(entity, EntityFilters.Default | EntityFilters.Attributes);

            if (entityMetadata.ChangeTrackingEnabled != true)
            {
                throw new Exception($"Entity '{entity}' does not have change tracking enabled.");
            }

            var entityTableMapping = EntityTableMapping.Create(entityMetadata);

            _entityMetadata[entity] = (entityMetadata, entityTableMapping);
        }
    }

    internal async Task ProcessChanges(CancellationToken cancellationToken)
    {
        using var operation = _telemetryClient.StartOperation<DependencyTelemetry>(ProcessChangesOperationName);

        var metricsGate = new object();
        var totalUpdates = 0;

        await Parallel.ForEachAsync(
            _options.Entities,
            new ParallelOptions()
            {
                MaxDegreeOfParallelism = _options.ProcessAllEntityTypesConcurrently ? _options.Entities.Length : 1,
                CancellationToken = cancellationToken
            },
            async (entityType, ct) =>
            {
                try
                {
                    var stopwatch = Stopwatch.StartNew();
                    var updatesForEntityType = await ProcessChangesForEntityType(entityType, ct);
                    stopwatch.Stop();

                    _telemetryClient.TrackMetric(new MetricTelemetry()
                    {
                        Name = $"{MetricPrefix}Batch processing time - {entityType}",
                        Sum = stopwatch.Elapsed.TotalSeconds
                    });

                    lock (metricsGate)
                    {
                        totalUpdates += updatesForEntityType;
                    }
                }
                catch (Exception ex)
                {
                    _telemetryClient.TrackException(new ExceptionTelemetry()
                    {
                        Exception = ex,
                        Properties =
                        {
                            { "Entity type", entityType }
                        }
                    });

                    throw;
                }
            });

        _telemetryClient.TrackMetric(new MetricTelemetry()
        {
            Name = $"{MetricPrefix}Total updates processed",
            Sum = totalUpdates
        });
    }

    internal async Task<int> ProcessChangesForEntityType(string entityLogicalName, CancellationToken cancellationToken)
    {
        var totalUpdates = 0;
        var insertedCount = 0;
        var updatedCount = 0;
        var deletedCount = 0;

        var columns = new ColumnSet(allColumns: true);

        await foreach (var changes in _crmEntityChangesService.GetEntityChanges(ChangesKey, entityLogicalName, columns).WithCancellation(cancellationToken))
        {
            totalUpdates += changes.Length;

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

            var batchNewOrUpdatedCount = await HandleNewOrUpdatedItems(newOrUpdatedItems, cancellationToken);
            insertedCount += batchNewOrUpdatedCount.InsertedCount;
            updatedCount += batchNewOrUpdatedCount.UpdatedCount;

            // It's important deleted items are processed *after* upserts, otherwise we may resurrect a deleted record
            deletedCount += await HandleRemovedOrDeletedItems(removedOrDeletedItems, cancellationToken);
        }

        Debug.Assert(totalUpdates == insertedCount + updatedCount + deletedCount);

        _telemetryClient.TrackMetric(new MetricTelemetry()
        {
            Name = $"{MetricPrefix}Updates processed - {entityLogicalName}",
            Sum = totalUpdates
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

        return totalUpdates;
    }

    private async Task<(int InsertedCount, int UpdatedCount)> HandleNewOrUpdatedItems(
        IReadOnlyCollection<NewOrUpdatedItem> newOrUpdatedItems,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (newOrUpdatedItems.Count == 0)
        {
            return (0, 0);
        }

        var insertedCount = 0;
        var updatedCount = 0;

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

        Debug.Assert(newOrUpdatedItems.Count == insertedCount + updatedCount);

        await DropTempTable();

        return (insertedCount, updatedCount);

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

                await sqlBulkCopy.WriteToServerAsync(dataTable);
            }

            var mergeSql = entityTableMapping.GetMergeSql(tempTableName);

            var mergeCommand = new SqlCommand(mergeSql)
            {
                Connection = conn,
                Transaction = txn
            };

            using (var reader = await mergeCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
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

            await txn.CommitAsync();
        }

        async Task DropTempTable()
        {
            var sql = $"drop table [{tempTableName}]";

            var command = new SqlCommand(sql);
            command.Connection = conn;

            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task<int> HandleRemovedOrDeletedItems(
        IReadOnlyCollection<RemovedOrDeletedItem> removedOrDeletedItems,
        CancellationToken cancellationToken)
    {
        if (removedOrDeletedItems.Count == 0)
        {
            return removedOrDeletedItems.Count;
        }

        var entityLogicalName = removedOrDeletedItems.First().RemovedItem.LogicalName;
        var ids = removedOrDeletedItems.Select(e => e.RemovedItem.Id).ToArray();

        using var conn = new SqlConnection(_options.ReportingDbConnectionString);
        await conn.OpenAsync();

        foreach (var chunk in ids.Chunk(MaxParameters))
        {
            var tableName = entityLogicalName;
            var idParameters = ids.Select((id, i) => new SqlParameter($"@id{i}", id)).ToArray();

            var command = new SqlCommand(
                $"delete from [{tableName}] where [{EntityTableMapping.IdColumnName}] in ({string.Join(", ", idParameters.Select(p => $"{p.ParameterName}"))})");

            command.Connection = conn;
            command.Parameters.AddRange(idParameters);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        return removedOrDeletedItems.Count;
    }
}
