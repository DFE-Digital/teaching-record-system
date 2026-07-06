using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using LegacyMandatoryQualificationCreatedEvent = TeachingRecordSystem.Core.Events.Legacy.MandatoryQualificationCreatedEvent;
using LegacyMandatoryQualificationDeletedEvent = TeachingRecordSystem.Core.Events.Legacy.MandatoryQualificationDeletedEvent;
using LegacyMandatoryQualificationUpdatedEvent = TeachingRecordSystem.Core.Events.Legacy.MandatoryQualificationUpdatedEvent;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="Process"/> and <see cref="ProcessEvent"/> records from the legacy mandatory
/// qualification created/updated/deleted events stored in the <c>events</c> table.
/// </summary>
public class BackfillMandatoryQualificationProcessesJob(TrsDbContext dbContext)
{
    // These match the EventName values stored in the events table for the legacy events.
    private static readonly string[] _legacyEventNames =
    [
        typeof(LegacyMandatoryQualificationCreatedEvent).Name,
        typeof(LegacyMandatoryQualificationUpdatedEvent).Name,
        typeof(LegacyMandatoryQualificationDeletedEvent).Name
    ];

    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Only migrate events that haven't already been back-filled so the job is idempotent.
        var legacyEvents = await dbContext.Events
            .Where(e => _legacyEventNames.Contains(e.EventName))
            .Where(e => !dbContext.ProcessEvents.Any(pe => pe.ProcessEventId == e.EventId))
            .OrderBy(e => e.Created)
            .ToListAsync(cancellationToken);

        foreach (var legacyEvent in legacyEvents)
        {
            var eventData = legacyEvent.ToEventBase();

            switch (eventData)
            {
                case LegacyMandatoryQualificationCreatedEvent created:
                    await CreateProcessAndProcessEventAsync(
                        legacyEvent,
                        new MandatoryQualificationCreatedEvent
                        {
                            EventId = created.EventId,
                            PersonId = created.PersonId,
                            MandatoryQualification = created.MandatoryQualification
                        },
                        ProcessType.MandatoryQualificationCreating,
                        new ChangeReasonWithDetailsAndEvidence
                        {
                            Reason = created.AddReason,
                            Details = created.AddReasonDetail,
                            EvidenceFile = created.EvidenceFile,
                            AdditionalInformation = created.AdditionalInformation
                        },
                        cancellationToken);
                    break;

                case LegacyMandatoryQualificationUpdatedEvent updated:
                    await CreateProcessAndProcessEventAsync(
                        legacyEvent,
                        new MandatoryQualificationUpdatedEvent
                        {
                            EventId = updated.EventId,
                            PersonId = updated.PersonId,
                            MandatoryQualification = updated.MandatoryQualification,
                            OldMandatoryQualification = updated.OldMandatoryQualification,
                            Changes = (MandatoryQualificationUpdatedEventChanges)(int)updated.Changes
                        },
                        ProcessType.MandatoryQualificationUpdating,
                        new ChangeReasonWithDetailsAndEvidence
                        {
                            Reason = updated.ChangeReason,
                            Details = updated.ChangeReasonDetail,
                            EvidenceFile = updated.EvidenceFile,
                            AdditionalInformation = updated.AdditionalInformation
                        },
                        cancellationToken);
                    break;

                case LegacyMandatoryQualificationDeletedEvent deleted:
                    await CreateProcessAndProcessEventAsync(
                        legacyEvent,
                        new MandatoryQualificationDeletedEvent
                        {
                            EventId = deleted.EventId,
                            PersonId = deleted.PersonId,
                            MandatoryQualification = deleted.MandatoryQualification
                        },
                        ProcessType.MandatoryQualificationDeleting,
                        new ChangeReasonWithDetailsAndEvidence
                        {
                            Reason = deleted.DeletionReason,
                            Details = deleted.DeletionReasonDetail,
                            EvidenceFile = deleted.EvidenceFile,
                            AdditionalInformation = deleted.AdditionalInformation
                        },
                        cancellationToken);
                    break;
            }
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

    private async Task CreateProcessAndProcessEventAsync(
        Event legacyEvent,
        IEvent newEvent,
        ProcessType processType,
        IChangeReasonInfo changeReason,
        CancellationToken cancellationToken)
    {
        var legacyEventData = legacyEvent.ToEventBase();
        var processId = Guid.NewGuid();

        var process = new Process
        {
            ProcessId = processId,
            ProcessType = processType,
            CreatedOn = legacyEvent.Created,
            UpdatedOn = legacyEvent.Created,
            UserId = legacyEventData.RaisedBy.UserId,
            DqtUserId = legacyEventData.RaisedBy.DqtUserId,
            DqtUserName = legacyEventData.RaisedBy.DqtUserName,
            PersonIds = [.. newEvent.PersonIds],
            OneLoginUserSubjects = [],
            SupportTaskReferences = [],
            ChangeReason = changeReason
        };

        dbContext.Processes.Add(process);

        var processEvent = new ProcessEvent
        {
            ProcessEventId = newEvent.EventId,
            ProcessId = processId,
            EventName = newEvent.GetType().Name,
            Payload = newEvent,
            PersonIds = newEvent.PersonIds,
            OneLoginUserSubjects = newEvent.OneLoginUserSubjects,
            SupportTaskReferences = newEvent.SupportTaskReferences,
            CreatedOn = legacyEvent.Created
        };

        dbContext.ProcessEvents.Add(processEvent);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
