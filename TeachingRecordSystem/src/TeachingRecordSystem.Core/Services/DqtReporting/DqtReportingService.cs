using System.Data;
using System.Diagnostics;
using System.Text.Json;
using Medallion.Threading;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk.Metadata;
using Npgsql;
using Npgsql.Replication;
using Npgsql.Replication.PgOutput;
using Npgsql.Replication.PgOutput.Messages;
using NpgsqlTypes;
using Polly;

namespace TeachingRecordSystem.Core.Services.DqtReporting;

public class DqtReportingService : BackgroundService
{
    public const string TrsDbPublicationName = "dqt_rep_sync";

    private static readonly ResiliencePipeline _resiliencePipeline = new ResiliencePipelineBuilder()
        .AddRetry(new Polly.Retry.RetryStrategyOptions()
        {
            BackoffType = DelayBackoffType.Linear,
            Delay = TimeSpan.FromSeconds(30),
            MaxRetryAttempts = 10
        })
        .Build();

    private readonly DqtReportingOptions _options;
    private readonly IDistributedLockProvider _distributedLockProvider;
    private readonly IClock _clock;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DqtReportingService> _logger;
    private readonly Dictionary<string, (EntityMetadata EntityMetadata, EntityTableMapping EntityTableMapping)> _entityMetadata = new();

    public DqtReportingService(
        IOptions<DqtReportingOptions> optionsAccessor,
        IDistributedLockProvider distributedLockProvider,
        IClock clock,
        IConfiguration configuration,
        ILogger<DqtReportingService> logger)
    {
        _options = optionsAccessor.Value;
        _distributedLockProvider = distributedLockProvider;
        _clock = clock;
        _configuration = configuration;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return ProcessTrsChangesWrapperAsync();

        async Task ProcessTrsChangesWrapperAsync()
        {
            try
            {
                await _resiliencePipeline.ExecuteAsync(async ct => await ProcessTrsChangesAsync(observer: null, ct), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed processing TRS changes.");
            }
        }
    }

    internal async Task ProcessTrsChangesAsync(
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

        var slot = await GetReplicationSlotAsync(replicationConn, cancellationToken);
        observer?.OnNext(TrsReplicationStatus.ReplicationSlotEstablished);

        var replicationOptions = new PgOutputReplicationOptions(TrsDbPublicationName, PgOutputProtocolVersion.V1, binary: true);

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

                    await TruncateTableFromTrsAsync(targetTableName);
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
                var values = await GetTupleValuesAsync(tuple);
                var columns = relation.Columns.ToArray();
                var idColumn = columns.Single(c => c.Flags == RelationMessage.Column.ColumnFlags.PartOfKey);
                var columnValues = columns.Zip(values, (c, v) => (Column: c, Value: v)).ToDictionary(t => t.Column.ColumnName, t => t.Value);
                var id = values[Array.IndexOf(columns, idColumn)]!;

                if (message is InsertMessage or UpdateMessage)
                {
                    await UpsertRowFromTrsAsync(targetTableName, idColumn.ColumnName, id, columnValues);
                }
                else
                {
                    Debug.Assert(message is DeleteMessage);
                    await DeleteRowFromTrsAsync(targetTableName, idColumn.ColumnName, id);
                }

                replicationConn.SetReplicationStatus(message.WalEnd);
                PublishMessageConsumed();
            }

            void PublishMessageConsumed() => observer?.OnNext(TrsReplicationStatus.MessageConsumed);
        }

        static string GetTargetTableName(RelationMessage relation) => $"trs_{relation.RelationName}";

        static ValueTask<object?[]> GetTupleValuesAsync(ReplicationTuple tuple)
        {
            return CoreAsync().ToArrayAsync();

            async IAsyncEnumerable<object?> CoreAsync()
            {
                await foreach (var value in tuple)
                {
                    yield return value.IsDBNull ? null : await value.Get();
                }
            }
        }
    }

    private async Task<PgOutputReplicationSlot> GetReplicationSlotAsync(
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

    private async Task UpsertRowFromTrsAsync(string targetTableName, string idColumnName, object id, IReadOnlyDictionary<string, object?> columnValues)
    {
        var parameters = new List<SqlParameter>();
        var columnNames = new List<string>();

        foreach (var (columnName, columnValue) in columnValues)
        {
            object? value = columnValue;

            if (value is not null)
            {
                var columnValueType = value.GetType();
                if (columnValueType.IsArray)
                {
                    value = JsonSerializer.Serialize(columnValue);
                }
            }

            if (value is null)
            {
                value = DBNull.Value;
            }

            var parameterName = $"@p{parameters.Count + 1}";
            var parameter = new SqlParameter(parameterName, value);

            if (value is DateTime)
            {
                parameter.SqlDbType = SqlDbType.DateTime2;
            }

            parameters.Add(parameter);
            columnNames.Add(columnName);
        }

        var parametersAndColumns = parameters.Zip(columnNames, (p, c) => (ParameterName: p.ParameterName, ColumnName: c)).ToArray();

        var nowParameterName = "@UtcNow";
        var idParameterName = "@id";

        var sql = $""""
            merge {targetTableName} as target
            using (
                select {(string.Join(",\n\t", parametersAndColumns.Select(p => $"{p.ParameterName} as [{p.ColumnName}]")))}
            ) as source
            on target.[{idColumnName}] = source.[{idColumnName}]
            when not matched then
                insert ({(string.Join(", ", parametersAndColumns.Select(p => $"[{p.ColumnName}]")))}, __Inserted, __Updated)
                values ({(string.Join(", ", parametersAndColumns.Select(p => $"source.[{p.ColumnName}]")))}, {nowParameterName}, {nowParameterName})
            when matched then
                update set {(string.Join(", ", parametersAndColumns.Where(p => p.ColumnName != idColumnName).Select(p => $"[{p.ColumnName}] = source.[{p.ColumnName}]")))}, __Updated = {nowParameterName}
            ;
            """";

        parameters.Add(new SqlParameter(nowParameterName, _clock.UtcNow));
        parameters.Add(new SqlParameter(idParameterName, id));

        using var conn = new SqlConnection(_options.ReportingDbConnectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters.ToArray());

        try
        {
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            throw new Exception("Failed executing query:\n" + sql, e);
        }
    }

    private async Task DeleteRowFromTrsAsync(string targetTableName, string idColumnName, object id)
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

    private async Task TruncateTableFromTrsAsync(string targetTableName)
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
        MessageConsumed = 1
    }
}
