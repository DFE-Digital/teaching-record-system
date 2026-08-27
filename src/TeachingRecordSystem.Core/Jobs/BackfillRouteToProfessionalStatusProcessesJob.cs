using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="Process"/> and <see cref="ProcessEvent"/> records from the legacy route to professional
/// status created/updated/deleted/migrated events stored in the <c>events</c> table.
/// </summary>
/// <remarks>
/// There are millions of migrated events, so this doesn't round-trip payloads through the change tracker like the
/// other back-fill jobs do. Each batch is a single <c>INSERT ... SELECT</c> that rewrites the payload with jsonb
/// operators in Postgres, so no event is ever deserialized. Batches are walked one event name at a time on the
/// (created, event_id) key, which is what <c>ix_events_event_name_created</c> is ordered by.
/// </remarks>
public class BackfillRouteToProfessionalStatusProcessesJob(TrsDbContext dbContext, ILogger<BackfillRouteToProfessionalStatusProcessesJob> logger)
{
    private const int BatchSize = 5000;

    private static readonly EventMapping[] _mappings =
    [
        new(
            LegacyEventName: nameof(LegacyEvents.RouteToProfessionalStatusCreatedEvent),
            NewEventName: nameof(RouteToProfessionalStatusCreatedEvent),
            ProcessType: ProcessType.RouteToProfessionalStatusCreating,
            ReasonProperty: "ChangeReason",
            ReasonDetailProperty: "ChangeReasonDetail",
            RoutePayloadProperties: ["RouteToProfessionalStatus"],
            HasInduction: true),
        new(
            LegacyEventName: nameof(LegacyEvents.RouteToProfessionalStatusUpdatedEvent),
            NewEventName: nameof(RouteToProfessionalStatusUpdatedEvent),
            ProcessType: ProcessType.RouteToProfessionalStatusUpdating,
            ReasonProperty: "ChangeReason",
            ReasonDetailProperty: "ChangeReasonDetail",
            // The legacy Changes also carried the person-level flags from bit 24 up; those are now expressed by
            // the person events, so only the route's own flags are kept.
            RoutePayloadProperties: ["RouteToProfessionalStatus", "OldRouteToProfessionalStatus"],
            ChangesMask: 0x3FFF,
            HasInduction: true),
        new(
            LegacyEventName: nameof(LegacyEvents.RouteToProfessionalStatusDeletedEvent),
            NewEventName: nameof(RouteToProfessionalStatusDeletedEvent),
            ProcessType: ProcessType.RouteToProfessionalStatusDeleting,
            ReasonProperty: "DeletionReason",
            ReasonDetailProperty: "DeletionReasonDetail",
            RoutePayloadProperties: ["RouteToProfessionalStatus"],
            HasInduction: true),
        new(
            LegacyEventName: nameof(LegacyEvents.RouteToProfessionalStatusMigratedEvent),
            NewEventName: nameof(RouteToProfessionalStatusMigratedEvent),
            ProcessType: ProcessType.RouteToProfessionalStatusMigratingFromDqt,
            // The migration didn't record a reason, and these events carry the DQT records they came from
            // rather than an induction snapshot.
            ReasonProperty: null,
            ReasonDetailProperty: null,
            RoutePayloadProperties: [
                "RouteToProfessionalStatus",
                "DqtInitialTeacherTraining",
                "DqtQtsRegistration",
                "DqtQtlsDate",
                "DqtQtlsDateHasBeenSet"
            ],
            HasInduction: false)
    ];

    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        foreach (var mapping in _mappings)
        {
            var migrated = await BackfillEventTypeAsync(mapping, dryRun, cancellationToken);

            logger.LogInformation(
                "Back-filled {Migrated} {EventName} event(s).",
                migrated,
                mapping.LegacyEventName);

            if (dryRun)
            {
                return;
            }
        }
    }

    private async Task<long> BackfillEventTypeAsync(EventMapping mapping, bool dryRun, CancellationToken cancellationToken)
    {
        var lastCreated = DateTime.MinValue.ToUniversalTime();
        var lastEventId = Guid.Empty;
        long totalMigrated = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            // Find where this batch ends before writing anything, so the cursor advances over events that turn out
            // to have been migrated already rather than reading them again on the next pass.
            var batchEnd = await GetBatchEndAsync(mapping.LegacyEventName, lastCreated, lastEventId, cancellationToken);

            if (batchEnd is not var (batchEndCreated, batchEndEventId))
            {
                return totalMigrated;
            }

            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            var migrated = await dbContext.Database.ExecuteSqlRawAsync(
                BuildBackfillSql(mapping),
                [
                    new NpgsqlParameter("legacyEventName", mapping.LegacyEventName),
                    new NpgsqlParameter("processType", (int)mapping.ProcessType),
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
                return migrated;
            }

            await transaction.CommitAsync(cancellationToken);

            totalMigrated += migrated;
            lastCreated = batchEndCreated;
            lastEventId = batchEndEventId;

            logger.LogInformation(
                "Back-filled {Migrated} {EventName} event(s) so far; up to {LastCreated:O}.",
                totalMigrated,
                mapping.LegacyEventName,
                lastCreated);
        }

        return totalMigrated;
    }

    private async Task<(DateTime Created, Guid EventId)?> GetBatchEndAsync(
        string legacyEventName,
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
        command.Parameters.AddWithValue("legacyEventName", legacyEventName);
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

    // The process and its events are written by one statement: foreign keys are checked once the whole statement
    // has run, so process_events can reference the processes the same statement inserts.
    private static string BuildBackfillSql(EventMapping mapping) =>
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
                 {BuildPersonAttributesChangesSql()} AS person_attributes_changes,
                 {BuildInductionChangesSql(mapping)} AS induction_changes
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
                 {BuildChangeReasonSql(mapping)}
             FROM todo
             RETURNING process_id
         ),
         inserted_person_attributes_events AS (
             INSERT INTO process_events (
                 process_event_id, process_id, event_name, payload,
                 person_ids, one_login_user_subjects, support_task_references, created_on)
             SELECT
                 gen_random_uuid(),
                 todo.process_id,
                 '{nameof(PersonProfessionalStatusAttributesUpdatedEvent)}',
                 jsonb_build_object(
                     '$event-name', '{nameof(PersonProfessionalStatusAttributesUpdatedEvent)}',
                     'EventId', gen_random_uuid(),
                     'PersonId', todo.payload->'PersonId',
                     'PersonAttributes', todo.payload->'PersonAttributes',
                     'OldPersonAttributes', todo.payload->'OldPersonAttributes',
                     'Changes', todo.person_attributes_changes),
                 todo.person_ids,
                 ARRAY[]::text[],
                 ARRAY[]::text[],
                 todo.created
             FROM todo
             WHERE todo.person_attributes_changes <> 0
             RETURNING process_event_id
         ){BuildInductionInsertSql(mapping)}
         INSERT INTO process_events (
             process_event_id, process_id, event_name, payload,
             person_ids, one_login_user_subjects, support_task_references, created_on)
         SELECT
             todo.event_id,
             todo.process_id,
             '{mapping.NewEventName}',
             {BuildRoutePayloadSql(mapping)},
             todo.person_ids,
             ARRAY[]::text[],
             ARRAY[]::text[],
             todo.created
         FROM todo
         """;

    // The legacy events recorded which person-level fields moved in their own Changes flags, but those flags
    // weren't always set per-field, so the new events' flags are derived from the snapshots themselves.
    private static string BuildPersonAttributesChangesSql()
    {
        var flags = new (string Property, int Value)[]
        {
            ("QtsDate", (int)PersonProfessionalStatusAttributesUpdatedEventChanges.QtsDate),
            ("EytsDate", (int)PersonProfessionalStatusAttributesUpdatedEventChanges.EytsDate),
            ("HasEyps", (int)PersonProfessionalStatusAttributesUpdatedEventChanges.HasEyps),
            ("PqtsDate", (int)PersonProfessionalStatusAttributesUpdatedEventChanges.PqtsDate),
            ("QtlsStatus", (int)PersonProfessionalStatusAttributesUpdatedEventChanges.QtlsStatus)
        };

        return BuildChangesMaskSql("PersonAttributes", "OldPersonAttributes", flags);
    }

    private static string BuildInductionChangesSql(EventMapping mapping)
    {
        if (!mapping.HasInduction)
        {
            return "0";
        }

        var flags = new (string Property, int Value)[]
        {
            ("Status", (int)PersonInductionUpdatedEventChanges.InductionStatus),
            ("StartDate", (int)PersonInductionUpdatedEventChanges.InductionStartDate),
            ("CompletedDate", (int)PersonInductionUpdatedEventChanges.InductionCompletedDate),
            ("ExemptionReasonIds", (int)PersonInductionUpdatedEventChanges.InductionExemptionReasons),
            ("StatusWithoutExemption", (int)PersonInductionUpdatedEventChanges.InductionStatusWithoutExemption),
            ("InductionExemptWithoutReason", (int)PersonInductionUpdatedEventChanges.InductionExemptWithoutReason)
        };

        return BuildChangesMaskSql("Induction", "OldInduction", flags);
    }

    private static string BuildChangesMaskSql(string property, string oldProperty, (string Property, int Value)[] flags) =>
        string.Join(
            " + ",
            flags.Select(f =>
                $"(CASE WHEN e.payload->'{property}'->'{f.Property}' IS DISTINCT FROM e.payload->'{oldProperty}'->'{f.Property}' THEN {f.Value} ELSE 0 END)"));

    private static string BuildInductionInsertSql(EventMapping mapping)
    {
        if (!mapping.HasInduction)
        {
            return "";
        }

        return $"""
         ,
         inserted_induction_events AS (
             INSERT INTO process_events (
                 process_event_id, process_id, event_name, payload,
                 person_ids, one_login_user_subjects, support_task_references, created_on)
             SELECT
                 gen_random_uuid(),
                 todo.process_id,
                 '{nameof(PersonInductionUpdatedEvent)}',
                 jsonb_build_object(
                     '$event-name', '{nameof(PersonInductionUpdatedEvent)}',
                     'EventId', gen_random_uuid(),
                     'PersonId', todo.payload->'PersonId',
                     'Induction', todo.payload->'Induction',
                     'OldInduction', todo.payload->'OldInduction',
                     'Changes', todo.induction_changes),
                 todo.person_ids,
                 ARRAY[]::text[],
                 ARRAY[]::text[],
                 todo.created
             FROM todo
             WHERE todo.induction_changes <> 0
             RETURNING process_event_id
         )
         """;
    }

    // The reason, its detail, the evidence file and the additional information all move off the event and onto the
    // process, matching what the journeys now write.
    private static string BuildChangeReasonSql(EventMapping mapping)
    {
        if (mapping.ReasonProperty is null)
        {
            return "NULL";
        }

        // -> gives SQL NULL for an absent key but jsonb 'null' for one the serializer wrote as null, so both have
        // to count as "nothing was recorded" or every process would get an all-null change reason.
        return $"""
                CASE WHEN COALESCE(jsonb_typeof(todo.payload->'{mapping.ReasonProperty}'), 'null') = 'null'
                          AND COALESCE(jsonb_typeof(todo.payload->'{mapping.ReasonDetailProperty}'), 'null') = 'null'
                          AND COALESCE(jsonb_typeof(todo.payload->'EvidenceFile'), 'null') = 'null'
                          AND COALESCE(jsonb_typeof(todo.payload->'AdditionalInformation'), 'null') = 'null'
                     THEN NULL
                     ELSE jsonb_build_object(
                         '$change-reason-type', 'ChangeReasonWithDetailsAndEvidence',
                         'Reason', todo.payload->'{mapping.ReasonProperty}',
                         'Details', todo.payload->'{mapping.ReasonDetailProperty}',
                         'EvidenceFile', todo.payload->'EvidenceFile',
                         'AdditionalInformation', todo.payload->'AdditionalInformation')
                 END
                """;
    }

    private static string BuildRoutePayloadSql(EventMapping mapping)
    {
        var members = string.Join(
            ",\n                 ",
            mapping.RoutePayloadProperties.Select(p => $"'{p}', todo.payload->'{p}'"));

        var changes = mapping.ChangesMask is int mask
            ? $",\n                 'Changes', ((todo.payload->>'Changes')::int & {mask})"
            : "";

        return $"""
                jsonb_build_object(
                     '$event-name', '{mapping.NewEventName}',
                     'EventId', todo.payload->'EventId',
                     'PersonId', todo.payload->'PersonId',
                     {members}{changes})
                """;
    }

    private record EventMapping(
        string LegacyEventName,
        string NewEventName,
        ProcessType ProcessType,
        string? ReasonProperty,
        string? ReasonDetailProperty,
        string[] RoutePayloadProperties,
        bool HasInduction,
        int? ChangesMask = null);
}
