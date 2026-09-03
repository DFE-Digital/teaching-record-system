using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="Process"/> and <see cref="ProcessEvent"/> records from the legacy
/// <c>QtsAwardedEmailSentEvent</c>, <c>InternationalQtsAwardedEmailSentEvent</c>, <c>EytsAwardedEmailSentEvent</c>
/// and <c>InductionCompletedEmailSentEvent</c>s stored in the <c>events</c> table. The batch jobs that wrote them
/// sent their emails through Notify directly, so there's no <see cref="Email"/> row to point at; one is created
/// from the batch job item the email was built from.
/// </summary>
public class BackfillNotificationEmailProcessesJob(TrsDbContext dbContext)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await BackfillQtsAwardedAsync(cancellationToken);
        await BackfillInternationalQtsAwardedAsync(cancellationToken);
        await BackfillEytsAwardedAsync(cancellationToken);
        await BackfillInductionCompletedAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    private async Task BackfillQtsAwardedAsync(CancellationToken cancellationToken)
    {
        var legacyEvents = await GetLegacyEventsAsync(nameof(LegacyEvents.QtsAwardedEmailSentEvent), cancellationToken);
        var payloads = legacyEvents.Select(e => (Row: e, Payload: (LegacyEvents.QtsAwardedEmailSentEvent)e.ToEventBase())).ToArray();
        var jobIds = payloads.Select(e => e.Payload.QtsAwardedEmailsJobId).Distinct().ToArray();

        var jobItems = await dbContext.QtsAwardedEmailsJobItems
            .Where(i => jobIds.Contains(i.QtsAwardedEmailsJobId))
            .Select(i => new { Key = i.QtsAwardedEmailsJobId, i.PersonId, i.Trn, i.Personalization })
            .ToDictionaryAsync(i => (i.Key, i.PersonId), i => new JobItemDetails(i.Trn, i.Personalization), cancellationToken);

        foreach (var (row, payload) in payloads)
        {
            jobItems.TryGetValue((payload.QtsAwardedEmailsJobId, payload.PersonId), out var jobItem);

            await CreateProcessAndProcessEventAsync(
                row,
                payload,
                payload.PersonId,
                payload.EmailAddress,
                EmailTemplateIds.QtsAwardedEmailConfirmation,
                ProcessType.NotifyingQtsAwardee,
                jobItem,
                cancellationToken);
        }
    }

    private async Task BackfillInternationalQtsAwardedAsync(CancellationToken cancellationToken)
    {
        var legacyEvents = await GetLegacyEventsAsync(nameof(LegacyEvents.InternationalQtsAwardedEmailSentEvent), cancellationToken);
        var payloads = legacyEvents.Select(e => (Row: e, Payload: (LegacyEvents.InternationalQtsAwardedEmailSentEvent)e.ToEventBase())).ToArray();
        var jobIds = payloads.Select(e => e.Payload.InternationalQtsAwardedEmailsJobId).Distinct().ToArray();

        var jobItems = await dbContext.InternationalQtsAwardedEmailsJobItems
            .Where(i => jobIds.Contains(i.InternationalQtsAwardedEmailsJobId))
            .Select(i => new { Key = i.InternationalQtsAwardedEmailsJobId, i.PersonId, i.Trn, i.Personalization })
            .ToDictionaryAsync(i => (i.Key, i.PersonId), i => new JobItemDetails(i.Trn, i.Personalization), cancellationToken);

        foreach (var (row, payload) in payloads)
        {
            jobItems.TryGetValue((payload.InternationalQtsAwardedEmailsJobId, payload.PersonId), out var jobItem);

            await CreateProcessAndProcessEventAsync(
                row,
                payload,
                payload.PersonId,
                payload.EmailAddress,
                EmailTemplateIds.InternationalQtsAwardedEmailConfirmation,
                ProcessType.NotifyingInternationalQtsAwardee,
                jobItem,
                cancellationToken);
        }
    }

    private async Task BackfillEytsAwardedAsync(CancellationToken cancellationToken)
    {
        var legacyEvents = await GetLegacyEventsAsync(nameof(LegacyEvents.EytsAwardedEmailSentEvent), cancellationToken);
        var payloads = legacyEvents.Select(e => (Row: e, Payload: (LegacyEvents.EytsAwardedEmailSentEvent)e.ToEventBase())).ToArray();
        var jobIds = payloads.Select(e => e.Payload.EytsAwardedEmailsJobId).Distinct().ToArray();

        var jobItems = await dbContext.EytsAwardedEmailsJobItems
            .Where(i => jobIds.Contains(i.EytsAwardedEmailsJobId))
            .Select(i => new { Key = i.EytsAwardedEmailsJobId, i.PersonId, i.Trn, i.Personalization })
            .ToDictionaryAsync(i => (i.Key, i.PersonId), i => new JobItemDetails(i.Trn, i.Personalization), cancellationToken);

        foreach (var (row, payload) in payloads)
        {
            jobItems.TryGetValue((payload.EytsAwardedEmailsJobId, payload.PersonId), out var jobItem);

            await CreateProcessAndProcessEventAsync(
                row,
                payload,
                payload.PersonId,
                payload.EmailAddress,
                EmailTemplateIds.EytsAwardedEmailConfirmation,
                ProcessType.NotifyingEytsAwardee,
                jobItem,
                cancellationToken);
        }
    }

    private async Task BackfillInductionCompletedAsync(CancellationToken cancellationToken)
    {
        var legacyEvents = await GetLegacyEventsAsync(nameof(LegacyEvents.InductionCompletedEmailSentEvent), cancellationToken);
        var payloads = legacyEvents.Select(e => (Row: e, Payload: (LegacyEvents.InductionCompletedEmailSentEvent)e.ToEventBase())).ToArray();
        var jobIds = payloads.Select(e => e.Payload.InductionCompletedEmailsJobId).Distinct().ToArray();

        var jobItems = await dbContext.InductionCompletedEmailsJobItems
            .Where(i => jobIds.Contains(i.InductionCompletedEmailsJobId))
            .Select(i => new { Key = i.InductionCompletedEmailsJobId, i.PersonId, i.Trn, i.Personalization })
            .ToDictionaryAsync(i => (i.Key, i.PersonId), i => new JobItemDetails(i.Trn, i.Personalization), cancellationToken);

        foreach (var (row, payload) in payloads)
        {
            jobItems.TryGetValue((payload.InductionCompletedEmailsJobId, payload.PersonId), out var jobItem);

            await CreateProcessAndProcessEventAsync(
                row,
                payload,
                payload.PersonId,
                payload.EmailAddress,
                EmailTemplateIds.InductionCompletedEmailConfirmation,
                ProcessType.NotifyingInductionCompletee,
                jobItem,
                cancellationToken);
        }
    }

    // Only migrate events that haven't already been back-filled so the job is idempotent.
    private Task<List<Event>> GetLegacyEventsAsync(string eventName, CancellationToken cancellationToken) =>
        dbContext.Events
            .Where(e => e.EventName == eventName)
            .Where(e => !dbContext.ProcessEvents.Any(pe => pe.ProcessEventId == e.EventId))
            .OrderBy(e => e.Created)
            .ToListAsync(cancellationToken);

    private async Task CreateProcessAndProcessEventAsync(
        Event legacyEvent,
        LegacyEvents.EventBase legacyEventPayload,
        Guid personId,
        string emailAddress,
        string templateId,
        ProcessType processType,
        JobItemDetails? jobItem,
        CancellationToken cancellationToken)
    {
        // The legacy event recorded only who the email went to, so the wording it was sent with comes from the
        // batch job item it was built from. A job item that's since been removed leaves an email with no
        // personalization rather than no email at all.
        var email = new Email
        {
            EmailId = Guid.NewGuid(),
            TemplateId = templateId,
            EmailAddress = emailAddress,
            Personalization = jobItem?.Personalization ?? new Dictionary<string, string>(),
            Metadata = jobItem is not null
                ? new Dictionary<string, object> { { SendAytqInviteEmailJob.JobMetadataKeys.Trn, jobItem.Trn } }
                : new Dictionary<string, object>(),
            SentOn = legacyEvent.Created
        };

        dbContext.Emails.Add(email);

        var processId = Guid.NewGuid();

        IEvent newEvent = new EmailSentEvent
        {
            EventId = legacyEvent.EventId,
            PersonId = personId,
            Email = EventModels.Email.FromModel(email)
        };

        dbContext.Processes.Add(new Process
        {
            ProcessId = processId,
            ProcessType = processType,
            CreatedOn = legacyEvent.Created,
            UpdatedOn = legacyEvent.Created,
            UserId = legacyEventPayload.RaisedBy.UserId,
            DqtUserId = legacyEventPayload.RaisedBy.DqtUserId,
            DqtUserName = legacyEventPayload.RaisedBy.DqtUserName,
            PersonIds = [.. newEvent.PersonIds],
            OneLoginUserSubjects = [.. newEvent.OneLoginUserSubjects],
            SupportTaskReferences = [.. newEvent.SupportTaskReferences],
            ChangeReason = null
        });

        dbContext.ProcessEvents.Add(new ProcessEvent
        {
            ProcessEventId = newEvent.EventId,
            ProcessId = processId,
            EventName = newEvent.GetType().Name,
            Payload = newEvent,
            PersonIds = newEvent.PersonIds,
            OneLoginUserSubjects = newEvent.OneLoginUserSubjects,
            SupportTaskReferences = newEvent.SupportTaskReferences,
            CreatedOn = legacyEvent.Created
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private record JobItemDetails(string Trn, Dictionary<string, string> Personalization);
}
