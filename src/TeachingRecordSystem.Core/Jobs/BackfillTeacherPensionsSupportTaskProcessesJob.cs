using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using LegacySupportTaskCreatedEvent = TeachingRecordSystem.Core.Events.Legacy.SupportTaskCreatedEvent;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="ProcessEvent"/> records for the TPS potential duplicate support tasks raised by
/// <see cref="CapitaImportJob"/>. Historically the job wrote the legacy <c>SupportTaskCreatedEvent</c> straight
/// to the <c>events</c> table rather than going through the event pipeline, so the task's creation was never
/// recorded against a process.
///
/// The import creates the person and raises the task within a single row, under one
/// <see cref="ProcessType.TeacherPensionsRecordImporting"/> process, so — as with the current code — the
/// back-filled event is attached to that same process. Every task is expected to have one; the job throws if
/// no matching process is found.
/// </summary>
/// <remarks>
/// The import built the legacy event from a <see cref="SupportTask"/> it hadn't saved yet, so once references
/// moved onto a database sequence (#2861, January 2026) the payload's <c>SupportTaskReference</c> was null.
/// The task is recovered from the person the event names and the reference stamped onto the back-filled event,
/// which is what the support task's change history is keyed on.
/// </remarks>
public class BackfillTeacherPensionsSupportTaskProcessesJob(TrsDbContext dbContext)
{
    // This matches the EventName value stored in the events table for the legacy event.
    private static readonly string _legacyEventName = typeof(LegacySupportTaskCreatedEvent).Name;

    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Only migrate events that haven't already been back-filled so the job is idempotent.
        // The task type is read from the payload rather than from a join to support_tasks: the reference the join
        // would need is missing from most of these events.
        var legacyEvents = await dbContext.Events
            .FromSql($"""
                select e.* from events e
                where e.event_name = {_legacyEventName}
                and (e.payload -> 'SupportTask' ->> 'SupportTaskType')::int = {(int)SupportTaskType.TeacherPensionsPotentialDuplicate}
                and not exists (select 1 from process_events pe where pe.process_event_id = e.event_id)
                order by e.created
                """)
            .ToListAsync(cancellationToken);

        foreach (var legacyEvent in legacyEvents)
        {
            var legacyEventData = (LegacySupportTaskCreatedEvent)legacyEvent.ToEventBase();

            var supportTask = await FindSupportTaskAsync(legacyEventData, cancellationToken);

            IEvent newEvent = new SupportTaskCreatedEvent
            {
                EventId = legacyEventData.EventId,
                SupportTask = legacyEventData.SupportTask with { SupportTaskReference = supportTask.SupportTaskReference }
            };

            var process =
                await FindImportProcessAsync(legacyEventData, legacyEvent.Created, cancellationToken) ??
                throw new InvalidOperationException(
                    $"No {ProcessType.TeacherPensionsRecordImporting} process found for support task " +
                    $"'{legacyEventData.SupportTask.SupportTaskReference}'.");

            AddProcessEvent(process, newEvent, legacyEvent.Created);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (dryRun)
        {
            await transaction.RollbackAsync(cancellationToken);
        }
        else
        {
            await transaction.CommitAsync(cancellationToken);
        }
    }

    private async Task<SupportTask> FindSupportTaskAsync(
        LegacySupportTaskCreatedEvent legacyEventData,
        CancellationToken cancellationToken)
    {
        var reference = legacyEventData.SupportTask.SupportTaskReference;

        if (!string.IsNullOrEmpty(reference))
        {
            return await dbContext.SupportTasks
                .IgnoreQueryFilters([QueryFilterNames.Deleted])
                .SingleOrDefaultAsync(t => t.SupportTaskReference == reference, cancellationToken) ??
                throw new InvalidOperationException($"Support task '{reference}' does not exist.");
        }

        if (legacyEventData.SupportTask.PersonId is not { } personId)
        {
            throw new InvalidOperationException(
                $"Legacy {nameof(SupportTaskCreatedEvent)} '{legacyEventData.EventId}' has neither a support task " +
                $"reference nor a person id.");
        }

        var candidates = await dbContext.SupportTasks
            .IgnoreQueryFilters([QueryFilterNames.Deleted])
            .Where(t => t.SupportTaskType == SupportTaskType.TeacherPensionsPotentialDuplicate && t.PersonId == personId)
            .ToListAsync(cancellationToken);

        // The import raises the task against the person it has just created, so a person has one of these tasks.
        // Should a record somehow have several, the import the event came from picks out the right one.
        var supportTask = candidates.Count <= 1
            ? candidates.SingleOrDefault()
            : candidates.SingleOrDefault(t =>
                t.GetData<TeacherPensionsPotentialDuplicateData>().IntegrationTransactionId ==
                (legacyEventData.SupportTask.Data as TeacherPensionsPotentialDuplicateData)?.IntegrationTransactionId);

        return supportTask ??
            throw new InvalidOperationException(
                $"No {SupportTaskType.TeacherPensionsPotentialDuplicate} support task found for person '{personId}'.");
    }

    private async Task<Process?> FindImportProcessAsync(
        LegacySupportTaskCreatedEvent legacyEventData,
        DateTime created,
        CancellationToken cancellationToken)
    {
        if (legacyEventData.SupportTask.PersonId is not { } personId)
        {
            return null;
        }

        var candidates = await dbContext.Processes
            .Where(p => p.ProcessType == ProcessType.TeacherPensionsRecordImporting && p.PersonIds.Contains(personId))
            .OrderBy(p => p.CreatedOn)
            .ToListAsync(cancellationToken);

        // The import only ever creates a person once, so a person should appear in a single import process.
        // Should a person somehow appear in several, the timestamp the events in a file share picks out the right one.
        return candidates.Count <= 1
            ? candidates.SingleOrDefault()
            : candidates.FirstOrDefault(p => p.CreatedOn == created);
    }

    private void AddProcessEvent(Process process, IEvent newEvent, DateTime created)
    {
        process.UpdatedOn = created;

        foreach (var personId in newEvent.PersonIds.Except(process.PersonIds))
        {
            process.PersonIds.Add(personId);
        }

        foreach (var oneLoginUserSubject in newEvent.OneLoginUserSubjects.Except(process.OneLoginUserSubjects))
        {
            process.OneLoginUserSubjects.Add(oneLoginUserSubject);
        }

        foreach (var supportTaskReference in newEvent.SupportTaskReferences.Except(process.SupportTaskReferences))
        {
            process.SupportTaskReferences.Add(supportTaskReference);
        }

        dbContext.ProcessEvents.Add(new ProcessEvent
        {
            ProcessEventId = newEvent.EventId,
            ProcessId = process.ProcessId,
            EventName = newEvent.GetType().Name,
            Payload = newEvent,
            PersonIds = newEvent.PersonIds,
            OneLoginUserSubjects = newEvent.OneLoginUserSubjects,
            SupportTaskReferences = newEvent.SupportTaskReferences,
            CreatedOn = created
        });
    }
}
