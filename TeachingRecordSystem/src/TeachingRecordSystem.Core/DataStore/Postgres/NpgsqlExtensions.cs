using Npgsql;
using NpgsqlTypes;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

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
        IClock clock,
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
            "key",
            "person_id",
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
                key VARCHAR(200),
                person_id UUID,
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

        foreach (var e in events.Select(e => Event.FromEventBase(e, inserted: clock.UtcNow)))
        {
            writer.StartRow();
            writer.WriteValueOrNull(e.EventId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(e.EventName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(e.Created, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(e.Inserted, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(e.Payload, NpgsqlDbType.Jsonb);
            writer.WriteValueOrNull(e.Key, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(e.PersonId, NpgsqlDbType.Uuid);
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
            mergeCommand.Parameters.Add(new NpgsqlParameter("@now", clock.UtcNow));
            mergeCommand.Transaction = transaction;
            await mergeCommand.ExecuteNonQueryAsync();
        }

        return events.Count;
    }
}
