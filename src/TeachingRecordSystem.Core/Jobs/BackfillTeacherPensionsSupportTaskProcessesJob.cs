using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
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
/// back-filled event is attached to that same process. A process is only created for the rare event that no
/// matching one is found.
/// </summary>
public class BackfillTeacherPensionsSupportTaskProcessesJob(TrsDbContext dbContext)
{
    // This matches the EventName value stored in the events table for the legacy event.
    private static readonly string _legacyEventName = typeof(LegacySupportTaskCreatedEvent).Name;

    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Only migrate events that haven't already been back-filled so the job is idempotent.
        var legacyEvents = await dbContext.Events
            .FromSql($"""
                select e.* from events e
                join support_tasks st on st.support_task_reference = (e.payload -> 'SupportTask' ->> 'SupportTaskReference')
                where e.event_name = {_legacyEventName}
                and st.support_task_type = {(int)SupportTaskType.TeacherPensionsPotentialDuplicate}
                and not exists (select 1 from process_events pe where pe.process_event_id = e.event_id)
                order by e.created
                """)
            .ToListAsync(cancellationToken);

        foreach (var legacyEvent in legacyEvents)
        {
            var legacyEventData = (LegacySupportTaskCreatedEvent)legacyEvent.ToEventBase();

            IEvent newEvent = new SupportTaskCreatedEvent
            {
                EventId = legacyEventData.EventId,
                SupportTask = legacyEventData.SupportTask
            };

            var process =
                await FindImportProcessAsync(legacyEventData, legacyEvent.Created, cancellationToken) ??
                CreateProcess(legacyEventData, legacyEvent.Created);

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

    private Process CreateProcess(LegacySupportTaskCreatedEvent legacyEventData, DateTime created)
    {
        var process = new Process
        {
            ProcessId = Guid.NewGuid(),
            ProcessType = ProcessType.TeacherPensionsRecordImporting,
            CreatedOn = created,
            UpdatedOn = created,
            UserId = legacyEventData.RaisedBy.UserId,
            DqtUserId = legacyEventData.RaisedBy.DqtUserId,
            DqtUserName = legacyEventData.RaisedBy.DqtUserName,
            PersonIds = [],
            OneLoginUserSubjects = [],
            SupportTaskReferences = [],
            ChangeReason = null
        };

        dbContext.Processes.Add(process);

        return process;
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
