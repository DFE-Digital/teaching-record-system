using System;
using System.Collections.Generic;
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

public class DqtReportingService : BackgroundService
{
    public const string ChangesKey = "DqtReporting";
    public const string ProcessChangesOperationName = "DqtReporting: process changes";

    private const string IdColumnName = "Id";
    private const int MaxParameters = 1024;
    private const string MetricPrefix = "DqtReporting: ";

    private readonly DqtReportingOptions _options;
    private readonly ICrmEntityChangesService _crmEntityChangesService;
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<DqtReportingService> _logger;
    private readonly Dictionary<string, EntityMetadata> _entityMetadata = new();

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

            _entityMetadata[entity] = entityMetadata;
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

        var entityLogicalName = newOrUpdatedItems.First().NewOrUpdatedEntity.LogicalName;
        var entities = newOrUpdatedItems.Select(e => e.NewOrUpdatedEntity);

        using var conn = new SqlConnection(_options.ReportingDbConnectionString);
        await conn.OpenAsync();

        foreach (var entity in entities)
        {
            var command = CreateUpsertRowCommand(entity);
            command.Connection = conn;

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            await reader.ReadAsync(cancellationToken);
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

        Debug.Assert(newOrUpdatedItems.Count == insertedCount + updatedCount);

        return (insertedCount, updatedCount);
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
            var command = CreateDeleteRowsCommand(entityLogicalName, chunk);
            command.Connection = conn;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        return removedOrDeletedItems.Count;
    }

    private SqlCommand CreateDeleteRowsCommand(string entityLogicalName, IReadOnlyCollection<Guid> ids)
    {
        Debug.Assert(ids.Count <= MaxParameters);

        var tableName = entityLogicalName;
        var idParameters = ids.Select((id, i) => new SqlParameter($"@id{i}", id)).ToArray();

        var command = new SqlCommand(
            $"delete from [{tableName}] where [{IdColumnName}] in ({string.Join(", ", idParameters.Select(p => $"{p.ParameterName}"))})");

        command.Parameters.AddRange(idParameters);

        return command;
    }

    private SqlCommand CreateUpsertRowCommand(Entity entity)
    {
        var entityMetadata = _entityMetadata[entity.LogicalName];
        var tableName = entityMetadata.LogicalName;
        var primaryKeyColumnName = entityMetadata.PrimaryIdAttribute;

        var sortedAttrs = entityMetadata.Attributes.OrderBy(a => a.ColumnNumber).Select(a => a.LogicalName).ToArray();

        var parametersAndColumns = entity.Attributes
            .OrderBy(a => Array.IndexOf(sortedAttrs, a.Key))
            .Select((a, i) => (
                ColumnName: a.Key == primaryKeyColumnName ? IdColumnName : a.Key,
                Parameter: new SqlParameter($"@p{i}", MapCrmAttributeValueToParameterValue(a.Value))))
            .ToArray();

        var sqlBuilder = new StringBuilder();
        sqlBuilder.AppendLine($"merge into [{tableName}] as target");
        sqlBuilder.AppendLine("using (select");
        sqlBuilder.AppendJoin(",\n", parametersAndColumns.Select(a => $"\t{a.Parameter.ParameterName} as [{a.ColumnName}]"));
        sqlBuilder.AppendLine("\n)as source");
        sqlBuilder.AppendLine($"on source.[{IdColumnName}] = target.[{IdColumnName}]");
        sqlBuilder.AppendLine("when matched then update set");
        sqlBuilder.AppendJoin(",\n", parametersAndColumns.Where(p => p.ColumnName != IdColumnName).Select(p => $"\t[{p.ColumnName}] = {p.Parameter.ParameterName}"));
        sqlBuilder.AppendLine("\nwhen not matched then insert (");
        sqlBuilder.AppendJoin(",\n", parametersAndColumns.Select(p => $"\t{p.ColumnName}"));
        sqlBuilder.AppendLine("\n) values (");
        sqlBuilder.AppendJoin(",\n", parametersAndColumns.Select(p => $"\t{p.Parameter.ParameterName}"));
        sqlBuilder.AppendLine("\n)\noutput $action;");

        var command = new SqlCommand(sqlBuilder.ToString());
        command.Parameters.AddRange(parametersAndColumns.Select(p => p.Parameter).ToArray());

        return command;

        static object MapCrmAttributeValueToParameterValue(object value) => value switch
        {
            OptionSetValue optionSetValue => optionSetValue.Value,
            EntityReference entityReference => entityReference.Id,
            Money money => money.Value,
            _ => value
        };
    }
}
