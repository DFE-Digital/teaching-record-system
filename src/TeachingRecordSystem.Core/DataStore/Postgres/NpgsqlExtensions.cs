using System.Text.Json;
using Npgsql;
using NpgsqlTypes;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Events.Legacy;

namespace TeachingRecordSystem.Core.DataStore.Postgres;

public static class NpgsqlExtensions
{
    public static void WriteValueOrNull<T>(this NpgsqlBinaryImporter writer, T? value, NpgsqlDbType dbType)
        where T : struct
    {
        if (value.HasValue)
        {
            writer.Write(value.Value, dbType);
        }
        else
        {
            writer.WriteNull();
        }
    }

    public static void WriteValueOrNull<T>(this NpgsqlBinaryImporter writer, T? value, NpgsqlDbType dbType)
        where T : notnull
    {
        if (value is null)
        {
            writer.WriteNull();
        }
        else
        {
            writer.Write(value, dbType);
        }
    }

    public static async Task<int> SaveEventsAsync(
        this NpgsqlTransaction transaction,
        IReadOnlyCollection<EventBase> events,
        string tempTableSuffix,
        TimeProvider timeProvider,
        CancellationToken cancellationToken,
        int? timeoutSeconds = null)
    {
        if (events.Count == 0)
        {
            return 0;
        }

        var tempTableName = $"temp_{tempTableSuffix}";
        var tableName = "events";

        var columnNames = new[]
        {
            "event_id",
            "event_name",
            "created",
            "inserted",
            "payload",
            "person_id",
            "person_ids",
            "qualification_id",
            "alert_id"
        };

        var columnList = string.Join(", ", columnNames);

        var createTempTableStatement = $"""
            CREATE TEMP TABLE {tempTableName} (
                event_id UUID NOT NULL,
                event_name VARCHAR(200) NOT NULL,
                created TIMESTAMP WITH TIME ZONE NOT NULL,
                inserted TIMESTAMP WITH TIME ZONE NOT NULL,
                payload JSONB NOT NULL,
                person_id UUID,
                person_ids UUID[],
                qualification_id UUID,
                alert_id UUID
            )
            ON COMMIT DROP
            """;

        var copyStatement = $"COPY {tempTableName} ({columnList}) FROM STDIN (FORMAT BINARY)";

        var insertStatement =
            $"""
            INSERT INTO {tableName} ({columnList}, published)
            SELECT {columnList}, false FROM {tempTableName}
            ON CONFLICT DO NOTHING
            """;

        using (var createTempTableCommand = transaction.Connection!.CreateCommand())
        {
            createTempTableCommand.CommandText = createTempTableStatement;
            createTempTableCommand.Transaction = transaction;
            await createTempTableCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        using var writer = await transaction.Connection!.BeginBinaryImportAsync(copyStatement, cancellationToken);

        foreach (var e in events.Select(e => Event.FromEventBase(e, inserted: timeProvider.UtcNow)))
        {
            writer.StartRow();
            writer.WriteValueOrNull(e.EventId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(e.EventName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(e.Created, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(e.Inserted, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(e.Payload, NpgsqlDbType.Jsonb);
            writer.WriteValueOrNull(e.PersonId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(e.PersonIds, NpgsqlDbType.Uuid | NpgsqlDbType.Array);
            writer.WriteValueOrNull(e.QualificationId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(e.AlertId, NpgsqlDbType.Uuid);
        }

        await writer.CompleteAsync(cancellationToken);
        await writer.CloseAsync(cancellationToken);

        using (var mergeCommand = transaction.Connection!.CreateCommand())
        {
            mergeCommand.CommandText = insertStatement;
            if (timeoutSeconds.HasValue)
            {
                mergeCommand.CommandTimeout = timeoutSeconds.Value;
            }
            mergeCommand.Parameters.Add(new NpgsqlParameter("@now", timeProvider.UtcNow));
            mergeCommand.Transaction = transaction;
            await mergeCommand.ExecuteNonQueryAsync();
        }

        return events.Count;
    }

    // Bulk equivalent of writing a Process and its ProcessEvents through EventPublisher; the processes are COPYed
    // into temp tables and inserted in one statement each, in the order the foreign key needs.
    public static async Task<int> SaveProcessesAsync(
        this NpgsqlTransaction transaction,
        IReadOnlyCollection<Process> processes,
        string tempTableSuffix,
        CancellationToken cancellationToken,
        int? timeoutSeconds = null)
    {
        if (processes.Count == 0)
        {
            return 0;
        }

        var processesTempTableName = $"temp_{tempTableSuffix}_processes";
        var processEventsTempTableName = $"temp_{tempTableSuffix}_process_events";

        var processColumnNames = new[]
        {
            "process_id",
            "process_type",
            "created_on",
            "updated_on",
            "user_id",
            "dqt_user_id",
            "dqt_user_name",
            "person_ids",
            "one_login_user_subjects",
            "support_task_references",
            "change_reason"
        };

        var processEventColumnNames = new[]
        {
            "process_event_id",
            "process_id",
            "event_name",
            "payload",
            "person_ids",
            "one_login_user_subjects",
            "support_task_references",
            "created_on"
        };

        var processColumnList = string.Join(", ", processColumnNames);
        var processEventColumnList = string.Join(", ", processEventColumnNames);

        var createTempTablesStatement =
            $"""
            CREATE TEMP TABLE {processesTempTableName} (
                process_id UUID NOT NULL,
                process_type INTEGER NOT NULL,
                created_on TIMESTAMP WITH TIME ZONE NOT NULL,
                updated_on TIMESTAMP WITH TIME ZONE NOT NULL,
                user_id UUID,
                dqt_user_id UUID,
                dqt_user_name TEXT,
                person_ids UUID[] NOT NULL,
                one_login_user_subjects TEXT[] NOT NULL,
                support_task_references TEXT[] NOT NULL,
                change_reason JSONB
            )
            ON COMMIT DROP;

            CREATE TEMP TABLE {processEventsTempTableName} (
                process_event_id UUID NOT NULL,
                process_id UUID NOT NULL,
                event_name VARCHAR(200) NOT NULL,
                payload JSONB NOT NULL,
                person_ids UUID[] NOT NULL,
                one_login_user_subjects VARCHAR(255)[] NOT NULL,
                support_task_references TEXT[] NOT NULL,
                created_on TIMESTAMP WITH TIME ZONE NOT NULL
            )
            ON COMMIT DROP;
            """;

        using (var createTempTablesCommand = transaction.Connection!.CreateCommand())
        {
            createTempTablesCommand.CommandText = createTempTablesStatement;
            createTempTablesCommand.Transaction = transaction;
            await createTempTablesCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        using (var writer = await transaction.Connection!.BeginBinaryImportAsync(
            $"COPY {processesTempTableName} ({processColumnList}) FROM STDIN (FORMAT BINARY)",
            cancellationToken))
        {
            foreach (var process in processes)
            {
                writer.StartRow();
                writer.WriteValueOrNull(process.ProcessId, NpgsqlDbType.Uuid);
                writer.WriteValueOrNull((int)process.ProcessType, NpgsqlDbType.Integer);
                writer.WriteValueOrNull(process.CreatedOn, NpgsqlDbType.TimestampTz);
                writer.WriteValueOrNull(process.UpdatedOn, NpgsqlDbType.TimestampTz);
                writer.WriteValueOrNull(process.UserId, NpgsqlDbType.Uuid);
                writer.WriteValueOrNull(process.DqtUserId, NpgsqlDbType.Uuid);
                writer.WriteValueOrNull(process.DqtUserName, NpgsqlDbType.Text);
                writer.WriteValueOrNull(process.PersonIds.ToArray(), NpgsqlDbType.Uuid | NpgsqlDbType.Array);
                writer.WriteValueOrNull(process.OneLoginUserSubjects.ToArray(), NpgsqlDbType.Text | NpgsqlDbType.Array);
                writer.WriteValueOrNull(process.SupportTaskReferences.ToArray(), NpgsqlDbType.Text | NpgsqlDbType.Array);
                writer.WriteValueOrNull(
                    process.ChangeReason is not null
                        ? JsonSerializer.Serialize(process.ChangeReason, IChangeReasonInfo.SerializerOptions)
                        : null,
                    NpgsqlDbType.Jsonb);
            }

            await writer.CompleteAsync(cancellationToken);
            await writer.CloseAsync(cancellationToken);
        }

        var processEventCount = 0;

        using (var writer = await transaction.Connection!.BeginBinaryImportAsync(
            $"COPY {processEventsTempTableName} ({processEventColumnList}) FROM STDIN (FORMAT BINARY)",
            cancellationToken))
        {
            foreach (var processEvent in processes.SelectMany(p => p.Events ?? []))
            {
                processEventCount++;
                writer.StartRow();
                writer.WriteValueOrNull(processEvent.ProcessEventId, NpgsqlDbType.Uuid);
                writer.WriteValueOrNull(processEvent.ProcessId, NpgsqlDbType.Uuid);
                writer.WriteValueOrNull(processEvent.EventName, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(
                    JsonSerializer.Serialize(processEvent.Payload, IEvent.SerializerOptions),
                    NpgsqlDbType.Jsonb);
                writer.WriteValueOrNull(processEvent.PersonIds.ToArray(), NpgsqlDbType.Uuid | NpgsqlDbType.Array);
                writer.WriteValueOrNull(processEvent.OneLoginUserSubjects.ToArray(), NpgsqlDbType.Varchar | NpgsqlDbType.Array);
                writer.WriteValueOrNull(processEvent.SupportTaskReferences.ToArray(), NpgsqlDbType.Text | NpgsqlDbType.Array);
                writer.WriteValueOrNull(processEvent.CreatedOn, NpgsqlDbType.TimestampTz);
            }

            await writer.CompleteAsync(cancellationToken);
            await writer.CloseAsync(cancellationToken);
        }

        var insertStatement =
            $"""
            INSERT INTO processes ({processColumnList})
            SELECT {processColumnList} FROM {processesTempTableName}
            ON CONFLICT DO NOTHING;

            INSERT INTO process_events ({processEventColumnList})
            SELECT {processEventColumnList} FROM {processEventsTempTableName}
            ON CONFLICT DO NOTHING;
            """;

        using (var insertCommand = transaction.Connection!.CreateCommand())
        {
            insertCommand.CommandText = insertStatement;
            if (timeoutSeconds.HasValue)
            {
                insertCommand.CommandTimeout = timeoutSeconds.Value;
            }
            insertCommand.Transaction = transaction;
            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        return processEventCount;
    }
}
