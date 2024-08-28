using System.Data;
using System.Diagnostics;
using System.Text;
using Medallion.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Npgsql;
using Npgsql.Replication;
using Npgsql.Replication.PgOutput;
using Npgsql.Replication.PgOutput.Messages;
using NpgsqlTypes;
using Polly;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.CrmEntityChanges;

namespace TeachingRecordSystem.Core.Services.DqtReporting;

public partial class DqtReportingService : BackgroundService
{
    public const string ChangesKey = "DqtReporting";
    public const string CrmClientName = "DqtReporting";
    public const string ProcessChangesOperationName = "DqtReporting: process changes";
    public const string TrsDbPublicationName = "dqt_rep_sync";

    private const int MaxParameters = 1024;
    private const int PageSize = 500;
    private const int MaxUpsertBatchSize = 100;
    private const int MaxEntityTypesToProcessConcurrently = 10;

    private static readonly ResiliencePipeline _resiliencePipeline = new ResiliencePipelineBuilder()
        .AddRetry(new Polly.Retry.RetryStrategyOptions()
        {
            BackoffType = DelayBackoffType.Linear,
            Delay = TimeSpan.FromSeconds(30),
            MaxRetryAttempts = 10
        })
        .Build();

    private readonly DqtReportingOptions _options;
    private readonly ICrmEntityChangesService _crmEntityChangesService;
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;
    private readonly IDistributedLockProvider _distributedLockProvider;
    private readonly IClock _clock;
    private readonly TelemetryClient _telemetryClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DqtReportingService> _logger;
    private readonly Dictionary<string, (EntityMetadata EntityMetadata, EntityTableMapping EntityTableMapping)> _entityMetadata = new();

    public DqtReportingService(
        IOptions<DqtReportingOptions> optionsAccessor,
        [FromKeyedServices(CrmClientName)] ICrmEntityChangesService crmEntityChangesService,
        [FromKeyedServices(CrmClientName)] ICrmQueryDispatcher crmQueryDispatcher,
        IDistributedLockProvider distributedLockProvider,
        IClock clock,
        TelemetryClient telemetryClient,
        IConfiguration configuration,
        ILogger<DqtReportingService> logger)
    {
        _options = optionsAccessor.Value;
        _crmEntityChangesService = crmEntityChangesService;
        _crmQueryDispatcher = crmQueryDispatcher;
        _distributedLockProvider = distributedLockProvider;
        _clock = clock;
        _telemetryClient = telemetryClient;
        _configuration = configuration;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.WhenAll(ProcessCrmChangesWrapper(), ProcessTrsChangesWrapper());

        async Task ProcessCrmChangesWrapper()
        {
            await LoadEntityMetadata();

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.PollIntervalSeconds));

            do
            {
                try
                {
                    await _resiliencePipeline.ExecuteAsync(async ct => await ProcessCrmChanges(ct), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                }
                catch (ProcessCrmChangesException ex)
                {
                    _logger.LogError(ex.InnerException, ex.Message);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed processing entity changes.");
                    return;
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        async Task ProcessTrsChangesWrapper()
        {
            try
            {
                await _resiliencePipeline.ExecuteAsync(async ct => await ProcessTrsChanges(observer: null, ct), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed processing TRS changes.");
                return;
            }
        }
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
                    var entityMetadata = await _crmQueryDispatcher.ExecuteQuery(
                        new GetEntityMetadataQuery(entity, EntityFilters.Default | EntityFilters.Attributes));

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

    internal async Task ProcessCrmChanges(CancellationToken cancellationToken)
    {
        using var operation = _telemetryClient.StartOperation<DependencyTelemetry>(ProcessChangesOperationName);

        await Parallel.ForEachAsync(
            _options.Entities,
            new ParallelOptions()
            {
                MaxDegreeOfParallelism = _options.ProcessAllEntityTypesConcurrently ? MaxEntityTypesToProcessConcurrently : 1,
                CancellationToken = cancellationToken
            },
            async (entityType, ct) =>
            {
                try
                {
                    await ProcessCrmChangesForEntityType(entityType, ct);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new ProcessCrmChangesException(entityType, ex);
                }
            });
    }

    internal async Task ProcessCrmChangesForEntityType(string entityLogicalName, CancellationToken cancellationToken)
    {
        var totalProcessed = 0;

        var columns = new ColumnSet(allColumns: true);

        // annotation has binary data in it that causes extremely large response messages.
        // We don't need that data so exclude the offending attribute.
        if (entityLogicalName == Annotation.EntityLogicalName)
        {
            columns = new ColumnSet(
                _entityMetadata[Annotation.EntityLogicalName].EntityMetadata.Attributes
                .Where(a => a.AttributeOf is null)
                .Select(a => a.LogicalName)
                .Except(new[] { Annotation.Fields.DocumentBody })
                .ToArray());
        }

        try
        {
            // We don't populate modifiedSince here since it's so slow to query in the reporting DB
            var changesEnumerable = _crmEntityChangesService.GetEntityChanges(ChangesKey, entityLogicalName, columns, modifiedSince: null, PageSize)
                .WithCancellation(cancellationToken);

            await foreach (var changes in changesEnumerable)
            {
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

                await HandleNewOrUpdatedItems(newOrUpdatedItems, cancellationToken);
                totalProcessed += newOrUpdatedItems.Count;

                // It's important deleted items are processed *after* upserts, otherwise we may resurrect a deleted record
                await HandleRemovedOrDeletedItems(removedOrDeletedItems, cancellationToken);
                totalProcessed += removedOrDeletedItems.Count;
            }
        }
        finally
        {
            _telemetryClient.TrackMetric(new MetricTelemetry()
            {
                Name = $"DqtReporting: updates processed",
                Sum = totalProcessed
            });
        }
    }

    private async Task HandleNewOrUpdatedItems(
        IReadOnlyCollection<NewOrUpdatedItem> newOrUpdatedItems,
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

        using var dataTable = CreateDataTable();

        using var conn = new SqlConnection(_options.ReportingDbConnectionString);
        await conn.OpenAsync();

        await CreateTempTable();

        foreach (var chunk in entities.Chunk(MaxUpsertBatchSize))
        {
            try
            {
                await UpsertRows(chunk);
            }
            catch (SqlException ex) when (ex.Number == 207)  // Likely means a column is missing
            {
                var missingColumns = ex.Message.Split("\n", StringSplitOptions.RemoveEmptyEntries)
                    .Where(line => line.StartsWith("Invalid column name "))
                    .Select(line => line.Split("'")[1])
                    .ToArray();

                if (missingColumns.Length == 0)
                {
                    throw;
                }

                await AddMissingColumns(missingColumns);

                await UpsertRows(chunk);
            }
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

            await mergeCommand.ExecuteNonQueryAsync(cancellationToken);

            await txn.CommitAsync(cancellationToken);
        }

        async Task DropTempTable()
        {
            var sql = $"drop table [{tempTableName}]";

            var command = new SqlCommand(sql);
            command.Connection = conn;

            await command.ExecuteNonQueryAsync();
        }

        async Task AddMissingColumns(string[] columnNames)
        {
            var attributes = entityTableMapping.Attributes.Where(a => a.ColumnDefinitions.Any(c => columnNames.Contains(c.ColumnName)));

            using var conn = new SqlConnection(_options.ReportingDbConnectionString);
            await conn.OpenAsync();

            foreach (var attr in attributes)
            {
                var sql = entityTableMapping.GetAddAttributeColumnsSql(attr);

                using var command = new SqlCommand(sql);
                command.Connection = conn;

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    private async Task HandleRemovedOrDeletedItems(
        IReadOnlyCollection<RemovedOrDeletedItem> removedOrDeletedItems,
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

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    internal async Task ProcessTrsChanges(
        IObserver<TrsReplicationStatus>? observer,
        CancellationToken cancellationToken)
    {
        await using var @lock = await _distributedLockProvider.TryAcquireLockAsync(DistributedLockKeys.DqtReportingReplicationSlot());
        if (@lock is null)
        {
            return;
        }

        await using var replicationConn = new LogicalReplicationConnection(_configuration.GetPostgresConnectionString());
        await replicationConn.Open();

        var slot = await GetReplicationSlot(replicationConn, cancellationToken);
        observer?.OnNext(TrsReplicationStatus.ReplicationSlotEstablished);

        var replicationOptions = new PgOutputReplicationOptions(TrsDbPublicationName, protocolVersion: 1, binary: true);

        await foreach (var message in replicationConn.StartReplication(slot, replicationOptions, cancellationToken))
        {
            if (message is BeginMessage or CommitMessage or RelationMessage)
            {
                replicationConn.SetReplicationStatus(message.WalEnd);
                continue;
            }

            if (message is TruncateMessage truncateMessage)
            {
                foreach (var relation in truncateMessage.Relations)
                {
                    var targetTableName = GetTargetTableName(relation);

                    await TruncateTableFromTrs(targetTableName);
                    PublishMessageConsumed();
                }

                replicationConn.SetReplicationStatus(message.WalEnd);
                continue;
            }

            {
                var (tuple, relation) = message switch
                {
                    InsertMessage insertMessage => (insertMessage.NewRow, insertMessage.Relation),
                    UpdateMessage updateMessage => (updateMessage.NewRow, updateMessage.Relation),
                    KeyDeleteMessage deleteMessage => (deleteMessage.Key, deleteMessage.Relation),
                    FullDeleteMessage deleteMessage => (deleteMessage.OldRow, deleteMessage.Relation),
                    _ => throw new NotSupportedException($"{message.GetType().Name} messages are not supported.")
                };

                var targetTableName = GetTargetTableName(relation);
                var values = await GetTupleValues(tuple);
                var columns = relation.Columns.ToArray();
                var idColumn = columns.Single(c => c.Flags == RelationMessage.Column.ColumnFlags.PartOfKey);
                var columnValues = columns.Zip(values, (c, v) => (Column: c, Value: v)).ToDictionary(t => t.Column.ColumnName, t => t.Value);
                var id = values[Array.IndexOf(columns, idColumn)]!;

                if (message is InsertMessage or UpdateMessage)
                {
                    await UpsertRowFromTrs(targetTableName, idColumn.ColumnName, id, columnValues);
                }
                else
                {
                    Debug.Assert(message is DeleteMessage);
                    await DeleteRowFromTrs(targetTableName, idColumn.ColumnName, id);
                }

                replicationConn.SetReplicationStatus(message.WalEnd);
                PublishMessageConsumed();
            }

            void PublishMessageConsumed() => observer?.OnNext(TrsReplicationStatus.MessageConsumed);
        }

        static string GetTargetTableName(RelationMessage relation) => $"trs_{relation.RelationName}";

        static ValueTask<object?[]> GetTupleValues(ReplicationTuple tuple)
        {
            return Core().ToArrayAsync();

            async IAsyncEnumerable<object?> Core()
            {
                await foreach (var value in tuple)
                {
                    yield return value.IsDBNull ? null : await value.Get();
                }
            }
        }
    }

    private async Task<PgOutputReplicationSlot> GetReplicationSlot(
        LogicalReplicationConnection replicationConn,
        CancellationToken cancellationToken)
    {
        await using var dataSource = NpgsqlDataSource.Create(_configuration.GetPostgresConnectionString());

        var slotName = _options.TrsDbReplicationSlotName;
        var startLsn = NpgsqlLogSequenceNumber.Invalid;

        await using (var cmd = dataSource.CreateCommand("SELECT confirmed_flush_lsn FROM pg_replication_slots WHERE slot_name = $1"))
        {
            cmd.Parameters.AddWithValue(slotName);

            await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                if (await reader.ReadAsync())
                {
                    startLsn = reader.GetFieldValue<NpgsqlLogSequenceNumber>(0);
                }
            }
        }

        if (startLsn == NpgsqlLogSequenceNumber.Invalid)
        {
            return await replicationConn.CreatePgOutputReplicationSlot(slotName);
        }
        else
        {
            return new PgOutputReplicationSlot(new ReplicationSlotOptions(slotName, startLsn));
        }
    }

    private async Task UpsertRowFromTrs(string targetTableName, string idColumnName, object id, IReadOnlyDictionary<string, object?> columnValues)
    {
        var parameters = new List<SqlParameter>();
        var columnNames = new List<string>();

        foreach (var (columnName, columnValue) in columnValues)
        {
            var parameterName = $"@p{parameters.Count + 1}";
            parameters.Add(new SqlParameter(parameterName, columnValue ?? DBNull.Value));
            columnNames.Add(columnName);
        }

        var parametersAndColumns = parameters.Zip(columnNames, (p, c) => (ParameterName: p.ParameterName, ColumnName: c)).ToArray();

        var nowParameterName = "@UtcNow";
        var idParameterName = "@id";

        var sql = $""""
            merge {targetTableName} as target
            using (
                select {(string.Join(",\n\t", parametersAndColumns.Select(p => $"{p.ParameterName} as {p.ColumnName}")))}
            ) as source
            on target.{idColumnName} = source.{idColumnName}
            when not matched then
                insert ({(string.Join(", ", parametersAndColumns.Select(p => p.ColumnName)))}, __Inserted, __Updated)
                values ({(string.Join(", ", parametersAndColumns.Select(p => $"source.{p.ColumnName}")))}, {nowParameterName}, {nowParameterName})
            when matched then
                update set {(string.Join(", ", parametersAndColumns.Where(p => p.ColumnName != idColumnName).Select(p => $"{p.ColumnName} = source.{p.ColumnName}")))}, __Updated = {nowParameterName}
            ;
            """";

        parameters.Add(new SqlParameter(nowParameterName, _clock.UtcNow));
        parameters.Add(new SqlParameter(idParameterName, id));

        using var conn = new SqlConnection(_options.ReportingDbConnectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters.ToArray());
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task DeleteRowFromTrs(string targetTableName, string idColumnName, object id)
    {
        var parameters = new List<SqlParameter>();

        var idParameterName = "@id";

        var sql = $"""
            delete from {targetTableName}
            where {idColumnName} = {idParameterName}
            """;

        parameters.Add(new SqlParameter(idParameterName, id));

        using var conn = new SqlConnection(_options.ReportingDbConnectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters.ToArray());
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task TruncateTableFromTrs(string targetTableName)
    {
        var sql = $"""
            truncate table {targetTableName}
            """;

        using var conn = new SqlConnection(_options.ReportingDbConnectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    internal enum TrsReplicationStatus
    {
        ReplicationSlotEstablished = 0,
        MessageConsumed = 1,
    }
}

file class ProcessCrmChangesException : Exception
{
    public ProcessCrmChangesException(string entityType, Exception innerException)
        : base(GetMessage(entityType), innerException)
    {
        EntityType = entityType;
    }

    public string EntityType { get; }

    private static string GetMessage(string entityType) =>
        $"Failed processing changes for '{entityType}' entity.";
}
