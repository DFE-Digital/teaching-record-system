using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="Process"/> and <see cref="ProcessEvent"/> records from the legacy induction migrated events
/// stored in the <c>events</c> table.
/// </summary>
/// <remarks>
/// There's one of these per person whose induction came over from DQT, so this doesn't round-trip payloads through
/// the change tracker like the smaller back-fill jobs do. Each batch is a single <c>INSERT ... SELECT</c> that
/// rewrites the payload with jsonb operators in Postgres, so no event is ever deserialized. Batches are walked on
/// the (created, event_id) key, which is what <c>ix_events_event_name_created</c> is ordered by.
/// </remarks>
public class BackfillInductionMigrationProcessesJob(TrsDbContext dbContext, ILogger<BackfillInductionMigrationProcessesJob> logger)
{
    private const int BatchSize = 5000;

    // This matches the EventName value stored in the events table for the legacy event; the new event happens to
    // share its name.
    private static readonly string _legacyEventName = nameof(LegacyEvents.InductionMigratedEvent);

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        var lastCreated = DateTime.MinValue.ToUniversalTime();
        var lastEventId = Guid.Empty;
        long totalMigrated = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            // Find where this batch ends before writing anything, so the cursor advances over events that turn out
            // to have been migrated already rather than reading them again on the next pass.
            var batchEnd = await GetBatchEndAsync(lastCreated, lastEventId, cancellationToken);

            if (batchEnd is not var (batchEndCreated, batchEndEventId))
            {
                break;
            }

            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            var migrated = await dbContext.Database.ExecuteSqlRawAsync(
                BackfillSql,
                [
                    new NpgsqlParameter("legacyEventName", _legacyEventName),
                    new NpgsqlParameter("processType", (int)ProcessType.InductionMigratingFromDqt),
                    new NpgsqlParameter("fromCreated", NpgsqlDbType.TimestampTz) { Value = lastCreated },
                    new NpgsqlParameter("fromEventId", NpgsqlDbType.Uuid) { Value = lastEventId },
                    new NpgsqlParameter("toCreated", NpgsqlDbType.TimestampTz) { Value = batchEndCreated },
                    new NpgsqlParameter("toEventId", NpgsqlDbType.Uuid) { Value = batchEndEventId }
                ],
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            totalMigrated += migrated;
            lastCreated = batchEndCreated;
            lastEventId = batchEndEventId;

            logger.LogInformation(
                "Back-filled {Migrated} {EventName} event(s) so far; up to {LastCreated:O}.",
                totalMigrated,
                _legacyEventName,
                lastCreated);
        }

        logger.LogInformation("Back-filled {Migrated} {EventName} event(s).", totalMigrated, _legacyEventName);
    }

    private async Task<(DateTime Created, Guid EventId)?> GetBatchEndAsync(
        DateTime lastCreated,
        Guid lastEventId,
        CancellationToken cancellationToken)
    {
        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();

        if (connection.State is not System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        // The inner LIMIT keeps this to one batch's worth of index entries; the outer ordering then picks the last
        // of them, whether or not there were a full batch's worth left.
        await using var command = new NpgsqlCommand(
            """
            SELECT created, event_id FROM (
                SELECT e.created, e.event_id
                FROM events e
                WHERE e.event_name = @legacyEventName
                  AND e.created >= @fromCreated
                  AND (e.created > @fromCreated OR e.event_id > @fromEventId)
                ORDER BY e.created, e.event_id
                LIMIT @batchSize
            ) batch
            ORDER BY batch.created DESC, batch.event_id DESC
            LIMIT 1
            """,
            connection);

        command.Transaction = (NpgsqlTransaction?)dbContext.Database.CurrentTransaction?.GetDbTransaction();
        command.CommandTimeout = 0;
        command.Parameters.AddWithValue("legacyEventName", _legacyEventName);
        command.Parameters.Add(new NpgsqlParameter("fromCreated", NpgsqlDbType.TimestampTz) { Value = lastCreated });
        command.Parameters.Add(new NpgsqlParameter("fromEventId", NpgsqlDbType.Uuid) { Value = lastEventId });
        command.Parameters.AddWithValue("batchSize", BatchSize);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return (reader.GetDateTime(0), reader.GetGuid(1));
    }

    // The process and its event are written by one statement: foreign keys are checked once the whole statement has
    // run, so process_events can reference the processes the same statement inserts. The todo CTE is MATERIALIZED so
    // gen_random_uuid() is evaluated once per row rather than once per reference.
    //
    // The new event keeps the legacy event's id, so the NOT EXISTS check below is what makes the job idempotent.
    //
    // The migration recorded no reason, so the processes get no change reason. The legacy event's Key is a
    // deduplication key for the migration itself and has no place on the new event.
    private static readonly string BackfillSql =
        $"""
         WITH todo AS MATERIALIZED (
             SELECT
                 e.event_id,
                 e.created,
                 e.payload,
                 gen_random_uuid() AS process_id,
                 ARRAY[(e.payload->>'PersonId')::uuid] AS person_ids,
                 CASE WHEN jsonb_typeof(e.payload->'RaisedBy') = 'object'
                     THEN NULL
                     ELSE (e.payload->>'RaisedBy')::uuid
                 END AS user_id,
                 CASE WHEN jsonb_typeof(e.payload->'RaisedBy') = 'object'
                     THEN (e.payload->'RaisedBy'->>'DqtUserId')::uuid
                 END AS dqt_user_id,
                 CASE WHEN jsonb_typeof(e.payload->'RaisedBy') = 'object'
                     THEN e.payload->'RaisedBy'->>'DqtUserName'
                 END AS dqt_user_name
             FROM events e
             WHERE e.event_name = @legacyEventName
               AND e.created >= @fromCreated
               AND (e.created > @fromCreated OR e.event_id > @fromEventId)
               AND e.created <= @toCreated
               AND (e.created < @toCreated OR e.event_id <= @toEventId)
               AND NOT EXISTS (SELECT 1 FROM process_events pe WHERE pe.process_event_id = e.event_id)
         ),
         inserted_processes AS (
             INSERT INTO processes (
                 process_id, process_type, created_on, updated_on,
                 user_id, dqt_user_id, dqt_user_name,
                 person_ids, one_login_user_subjects, support_task_references, change_reason)
             SELECT
                 todo.process_id,
                 @processType,
                 todo.created,
                 todo.created,
                 todo.user_id,
                 todo.dqt_user_id,
                 todo.dqt_user_name,
                 todo.person_ids,
                 ARRAY[]::text[],
                 ARRAY[]::text[],
                 NULL
             FROM todo
             RETURNING process_id
         )
         INSERT INTO process_events (
             process_event_id, process_id, event_name, payload,
             person_ids, one_login_user_subjects, support_task_references, created_on)
         SELECT
             todo.event_id,
             todo.process_id,
             '{nameof(InductionMigratedEvent)}',
             jsonb_build_object(
                 '$event-name', '{nameof(InductionMigratedEvent)}',
                 'EventId', todo.payload->'EventId',
                 'PersonId', todo.payload->'PersonId',
                 'InductionStartDate', todo.payload->'InductionStartDate',
                 'InductionCompletedDate', todo.payload->'InductionCompletedDate',
                 'InductionStatus', todo.payload->'InductionStatus',
                 'InductionExemptionReasonId', todo.payload->'InductionExemptionReasonId',
                 'DqtInduction', todo.payload->'DqtInduction',
                 'DqtInductionStatus', todo.payload->'DqtInductionStatus'),
             todo.person_ids,
             ARRAY[]::text[],
             ARRAY[]::text[],
             todo.created
         FROM todo
         """;
}
