using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="Process"/> and <see cref="ProcessEvent"/> records from the legacy person induction updated
/// events stored in the <c>events</c> table.
/// </summary>
/// <remarks>
/// There are millions of these events, so this doesn't round-trip payloads through the change tracker like the
/// smaller back-fill jobs do. Each batch is a single <c>INSERT ... SELECT</c> that rewrites the payload with jsonb
/// operators in Postgres, so no event is ever deserialized. Batches are walked on the (created, event_id) key, which
/// is what <c>ix_events_event_name_created</c> is ordered by.
/// </remarks>
public class BackfillPersonInductionProcessesJob(TrsDbContext dbContext, ILogger<BackfillPersonInductionProcessesJob> logger)
{
    private const int BatchSize = 5000;

    // This matches the EventName value stored in the events table for the legacy event; the new event happens to
    // share its name.
    private static readonly string _legacyEventName = nameof(LegacyEvents.PersonInductionUpdatedEvent);

    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
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
                    new NpgsqlParameter("cpdProcessType", (int)ProcessType.PersonCpdInductionUpdating),
                    new NpgsqlParameter("welshProcessType", (int)ProcessType.PersonWelshInductionUpdating),
                    new NpgsqlParameter("ewcWalesInterfaceType", (int)IntegrationTransactionInterfaceType.EwcWales),
                    new NpgsqlParameter("processType", (int)ProcessType.PersonInductionUpdating),
                    new NpgsqlParameter("fromCreated", NpgsqlDbType.TimestampTz) { Value = lastCreated },
                    new NpgsqlParameter("fromEventId", NpgsqlDbType.Uuid) { Value = lastEventId },
                    new NpgsqlParameter("toCreated", NpgsqlDbType.TimestampTz) { Value = batchEndCreated },
                    new NpgsqlParameter("toEventId", NpgsqlDbType.Uuid) { Value = batchEndEventId }
                ],
                cancellationToken);

            if (dryRun)
            {
                // Rolling back every batch would leave the loop with the same work to do forever, so a dry run
                // covers the first batch only.
                await transaction.RollbackAsync(cancellationToken);
                return;
            }

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
    // Its Changes flags sit at the same bit positions as the legacy ones, so they carry over untouched.
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
                 END AS dqt_user_name,
                 {ProcessTypeSql} AS process_type
             FROM events e
             -- Joined on text so the object form of RaisedBy (a DQT user) simply matches nothing, rather than
             -- putting a uuid cast that would throw on it behind a condition Postgres is free to reorder.
             LEFT JOIN users u ON u.user_id::text = e.payload->>'RaisedBy'
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
                 todo.process_type,
                 todo.created,
                 todo.created,
                 todo.user_id,
                 todo.dqt_user_id,
                 todo.dqt_user_name,
                 todo.person_ids,
                 ARRAY[]::text[],
                 ARRAY[]::text[],
                 {ChangeReasonSql}
             FROM todo
             RETURNING process_id
         )
         INSERT INTO process_events (
             process_event_id, process_id, event_name, payload,
             person_ids, one_login_user_subjects, support_task_references, created_on)
         SELECT
             todo.event_id,
             todo.process_id,
             '{nameof(PersonInductionUpdatedEvent)}',
             jsonb_build_object(
                 '$event-name', '{nameof(PersonInductionUpdatedEvent)}',
                 'EventId', todo.payload->'EventId',
                 'PersonId', todo.payload->'PersonId',
                 'Induction', todo.payload->'Induction',
                 'OldInduction', todo.payload->'OldInduction',
                 'Changes', COALESCE(todo.payload->'Changes', '0'::jsonb)),
             todo.person_ids,
             ARRAY[]::text[],
             ARRAY[]::text[],
             todo.created
         FROM todo
         """;

    // Each write path has its own process type now, and a legacy payload can be traced back to two of them.
    //
    // CpdInductionCpdModifiedOn is touched by the CPD path and by nothing else, so a moved timestamp says CPD. It's
    // a one-way tell - CPD resending the same CpdModifiedOn alongside a new status leaves the timestamp unmoved - so
    // the client's API role backs it up: the operation runs as the client's application user, and the role that lets
    // it be called is on the user row. Roles can be taken away, which is why the timestamp is tried first.
    //
    // The EWC Wales import is found through the integration transaction it wrote alongside the induction change.
    // The system user it runs as would not be enough on its own - the DQT outbox handlers wrote these events as the
    // system user too whenever their message carried no DQT or TRS user - but the import records a row per person
    // in the same loop iteration and the same transaction, so an induction change with a matching row is the
    // import's. The file name is what the job itself routes on ("IND" for induction, "QTS" for qualifications), so
    // it keeps the QTS import out. The row is written just after the induction change, hence the one-sided window.
    //
    // There is no tell for the Welsh API operation, but it has never been called, so there is nothing to find.
    private const string ProcessTypeSql =
        $"""
        CASE WHEN e.payload->'Induction'->'CpdCpdModifiedOn'
                  IS DISTINCT FROM e.payload->'OldInduction'->'CpdCpdModifiedOn'
             THEN @cpdProcessType
             WHEN '{ApiRoles.SetCpdInduction}' = ANY(u.api_roles) THEN @cpdProcessType
             WHEN EXISTS (
                 SELECT 1
                 FROM integration_transaction_records itr
                 JOIN integration_transactions it
                     ON it.integration_transaction_id = itr.integration_transaction_id
                 WHERE itr.person_id = (e.payload->>'PersonId')::uuid
                   AND it.interface_type = @ewcWalesInterfaceType
                   AND it.file_name ILIKE 'IND%'
                   AND e.created <= itr.created_date
                   AND e.created > itr.created_date - interval '1 minute')
             THEN @welshProcessType
             ELSE @processType
         END
        """;

    // The reason, its detail, the evidence file and the additional information all move off the event and onto the
    // process, matching what the Edit induction journey now writes.
    //
    // -> gives SQL NULL for an absent key but jsonb 'null' for one the serializer wrote as null, so both have to
    // count as "nothing was recorded" or every process would get an all-null change reason.
    private const string ChangeReasonSql =
        """
        CASE WHEN COALESCE(jsonb_typeof(todo.payload->'ChangeReason'), 'null') = 'null'
                  AND COALESCE(jsonb_typeof(todo.payload->'ChangeReasonDetail'), 'null') = 'null'
                  AND COALESCE(jsonb_typeof(todo.payload->'EvidenceFile'), 'null') = 'null'
                  AND COALESCE(jsonb_typeof(todo.payload->'AdditionalInformation'), 'null') = 'null'
             THEN NULL
             ELSE jsonb_build_object(
                 '$change-reason-type', 'ChangeReasonWithDetailsAndEvidence',
                 'Reason', todo.payload->'ChangeReason',
                 'Details', todo.payload->'ChangeReasonDetail',
                 'EvidenceFile', todo.payload->'EvidenceFile',
                 'AdditionalInformation', todo.payload->'AdditionalInformation')
         END
        """;
}
