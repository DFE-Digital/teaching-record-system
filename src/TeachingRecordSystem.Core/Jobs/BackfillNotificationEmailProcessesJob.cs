using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="Process"/> and <see cref="ProcessEvent"/> records for the professional status and
/// induction emails sent before these process types existed. There are two eras, and they left behind
/// different things:
/// <list type="bullet">
/// <item>
/// The original per-status batch jobs wrote a typed legacy event — <c>QtsAwardedEmailSentEvent</c> and friends —
/// and sent through Notify directly, so there's no <see cref="Email"/> row to point at; one is created from the
/// batch job item the email was built from.
/// </item>
/// <item>
/// After the AYTQ rewire the same emails went through <see cref="SendEmailJob"/>, which created a real
/// <see cref="Email"/> row but recorded only the generic legacy <c>EmailSentEvent</c> and no process. Those
/// events point at the existing row; who the email went to is recovered from the TRN in its metadata, or for
/// the QTLS lapsed email from the QTLS expiry that caused it.
/// </item>
/// </list>
/// </summary>
public class BackfillNotificationEmailProcessesJob(TrsDbContext dbContext)
{
    // The award emails carry the TRN in their metadata, so the person they went to is recorded exactly.
    private static readonly string[] _templateIdsWithTrn =
    [
        EmailTemplateIds.QtsAwardedEmailConfirmation,
        EmailTemplateIds.InternationalQtsAwardedEmailConfirmation,
        EmailTemplateIds.EytsAwardedEmailConfirmation,
        EmailTemplateIds.QtlsPostLaunchForAllUsers
    ];

    // The QTLS lapsed email carries neither a TRN nor any personalization, so the person is recovered from the
    // QTLS expiry that caused it: the batch job emails people whose QtlsStatus moved Active -> Expired, a
    // configurable number of days after the event. Anything that doesn't come back to exactly one person is
    // skipped rather than guessed at.
    private static readonly TimeSpan _qtlsExpiryWindow = TimeSpan.FromDays(120);

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Emails sent by the original per-status batch jobs.
        await BackfillQtsAwardedAsync(cancellationToken);
        await BackfillInternationalQtsAwardedAsync(cancellationToken);
        await BackfillEytsAwardedAsync(cancellationToken);
        await BackfillInductionCompletedAsync(cancellationToken);

        // Emails sent after the AYTQ rewire but before these process types existed.
        await BackfillEmailsWithTrnAsync(cancellationToken);
        await BackfillQtlsLapsedEmailsAsync(cancellationToken);

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

            await BackfillFromJobItemAsync(
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

            await BackfillFromJobItemAsync(
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

            await BackfillFromJobItemAsync(
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

            await BackfillFromJobItemAsync(
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

    private async Task BackfillEmailsWithTrnAsync(CancellationToken cancellationToken)
    {
        var legacyEvents = await GetLegacyEmailSentEventsAsync(_templateIdsWithTrn, cancellationToken);

        var trns = legacyEvents
            .Select(e => GetTrn(e.Payload))
            .Where(trn => trn is not null)
            .Distinct()
            .ToArray();

        // The person may have been deactivated since the email was sent.
        var personIdsByTrn = await dbContext.Persons
            .IgnoreQueryFilters([QueryFilterNames.Person.Deactivated])
            .Where(p => p.Trn != null && trns.Contains(p.Trn))
            .ToDictionaryAsync(p => p.Trn!, p => p.PersonId, cancellationToken);

        foreach (var (row, payload) in legacyEvents)
        {
            if (GetTrn(payload) is not string trn || !personIdsByTrn.TryGetValue(trn, out var personId))
            {
                continue;
            }

            CreateProcessAndProcessEvent(
                row,
                payload.RaisedBy,
                personId,
                payload.Email,
                SendAytqInviteEmailJob.GetProcessType(payload.Email.TemplateId));

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task BackfillQtlsLapsedEmailsAsync(CancellationToken cancellationToken)
    {
        var legacyEvents = await GetLegacyEmailSentEventsAsync([EmailTemplateIds.QtlsLapsed], cancellationToken);

        if (legacyEvents.Length == 0)
        {
            return;
        }

        var expiries = (await dbContext.Database
            .SqlQuery<QtlsExpiryQueryResult>(
                $"""
                 select pid.person_id, pe.created_on from process_events pe
                 cross join lateral unnest(pe.person_ids) as pid(person_id)
                 where pe.event_name = {nameof(PersonProfessionalStatusAttributesUpdatedEvent)}
                 and pe.payload -> 'PersonAttributes' ->> 'QtlsStatus' = '1' --Expired
                 and pe.payload -> 'OldPersonAttributes' ->> 'QtlsStatus' = '2' --Active
                 """)
            .ToListAsync(cancellationToken))
            .GroupBy(e => e.person_id)
            .ToDictionary(g => g.Key, g => g.Select(e => e.created_on).ToArray());

        var emailAddresses = legacyEvents
            .Select(e => e.Payload.Email.EmailAddress.ToLowerInvariant())
            .Distinct()
            .ToArray();

        var personIdsByEmailAddress = (await dbContext.Persons
                .IgnoreQueryFilters([QueryFilterNames.Person.Deactivated])
                .Where(p => p.EmailAddress != null && emailAddresses.Contains(p.EmailAddress.ToLower()))
                .Select(p => new { p.PersonId, p.EmailAddress })
                .ToListAsync(cancellationToken))
            .GroupBy(p => p.EmailAddress!.ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.Select(p => p.PersonId).ToArray());

        foreach (var (row, payload) in legacyEvents)
        {
            if (!personIdsByEmailAddress.TryGetValue(payload.Email.EmailAddress.ToLowerInvariant(), out var candidates))
            {
                continue;
            }

            var sentOn = payload.Email.SentOn ?? row.Created;

            var matches = candidates
                .Where(personId => expiries.TryGetValue(personId, out var expiredOn) &&
                    expiredOn.Any(e => e <= sentOn && e >= sentOn - _qtlsExpiryWindow))
                .ToArray();

            if (matches.Length != 1)
            {
                continue;
            }

            CreateProcessAndProcessEvent(row, payload.RaisedBy, matches[0], payload.Email, ProcessType.NotifyingLapsedQtlsHolder);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    // Only migrate events that haven't already been back-filled so the job is idempotent.
    private Task<List<Event>> GetLegacyEventsAsync(string eventName, CancellationToken cancellationToken) =>
        dbContext.Events
            .Where(e => e.EventName == eventName)
            .Where(e => !dbContext.ProcessEvents.Any(pe => pe.ProcessEventId == e.EventId))
            .OrderBy(e => e.Created)
            .ToListAsync(cancellationToken);

    private async Task<(Event Row, LegacyEvents.EmailSentEvent Payload)[]> GetLegacyEmailSentEventsAsync(
        string[] templateIds,
        CancellationToken cancellationToken)
    {
        // Every email in the system writes this event, so the template has to be part of the query rather than
        // something we filter on after loading.
        var legacyEvents = await dbContext.Events
            .FromSql(
                $"""
                 select * from events
                 where event_name = {nameof(LegacyEvents.EmailSentEvent)}
                 and payload -> 'Email' ->> 'TemplateId' = any({templateIds})
                 """)
            .Where(e => !dbContext.ProcessEvents.Any(pe => pe.ProcessEventId == e.EventId))
            .OrderBy(e => e.Created)
            .ToListAsync(cancellationToken);

        return [.. legacyEvents.Select(e => (e, (LegacyEvents.EmailSentEvent)e.ToEventBase()))];
    }

    private static string? GetTrn(LegacyEvents.EmailSentEvent payload) =>
        payload.Email.Metadata.TryGetValue(SendAytqInviteEmailJob.JobMetadataKeys.Trn, out var trn)
            ? trn?.ToString()
            : null;

    private async Task BackfillFromJobItemAsync(
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

        CreateProcessAndProcessEvent(
            legacyEvent,
            legacyEventPayload.RaisedBy,
            personId,
            EventModels.Email.FromModel(email),
            processType);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private void CreateProcessAndProcessEvent(
        Event legacyEvent,
        EventModels.RaisedByUserInfo raisedBy,
        Guid personId,
        EventModels.Email email,
        ProcessType processType)
    {
        var processId = Guid.NewGuid();

        IEvent newEvent = new EmailSentEvent
        {
            EventId = legacyEvent.EventId,
            PersonId = personId,
            Email = email
        };

        dbContext.Processes.Add(new Process
        {
            ProcessId = processId,
            ProcessType = processType,
            CreatedOn = legacyEvent.Created,
            UpdatedOn = legacyEvent.Created,
            UserId = raisedBy.UserId,
            DqtUserId = raisedBy.DqtUserId,
            DqtUserName = raisedBy.DqtUserName,
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
    }

    private record JobItemDetails(string Trn, Dictionary<string, string> Personalization);

#pragma warning disable IDE1006 // Naming Styles
    private record QtlsExpiryQueryResult(Guid person_id, DateTime created_on);
#pragma warning restore IDE1006 // Naming Styles
}
