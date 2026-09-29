using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Attaches the 'TRN generated for NPQ' emails sent by the since-removed
/// <c>AllocateTrnsToOverseasNpqApplicantsJob</c> to the <see cref="Process"/> that created the person they
/// went to.
///
/// That job created the person, allocated their TRN and sent the email in a single pass, recording a legacy
/// <c>PersonCreatedEvent</c> and a legacy <c>EmailSentEvent</c> straight to the <c>events</c> table. The
/// person creation has since been back-filled onto a process; the email was left behind, and it is the last
/// thing keeping the legacy <c>EmailSentEvent</c> alive.
///
/// The email belongs with the creation because that is when the TRN it announces came into existence, and
/// the link between the two is exact rather than inferred from address and timing: the email's
/// personalization carries the TRN under the <c>trn</c> key, so the person is looked up from it directly.
/// This is deliberately not <see cref="SentEmailMatcher"/> — matching on template id and address would let
/// an email claim a send made by a different sender on the same template.
///
/// The NPQ TRN request journey sends the same template, but has always enqueued it with a process id, so
/// those sends already hold their event and are skipped.
/// </summary>
/// <remarks>
/// There are 45 of these in production, all from one run, so this reads them in a single pass and commits
/// once rather than paging: a cursor over a set this size would be ceremony without a purpose.
/// </remarks>
public class BackfillOverseasNpqTrnEmailSentEventsJob(TrsDbContext dbContext)
{
    // The key AllocateTrnsToOverseasNpqApplicantsJob wrote the allocated TRN under.
    private const string TrnPersonalizationKey = "trn";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Every email in the system writes this event, so the template has to be part of the query rather
        // than something we filter on after loading.
        var legacyEvents = (await dbContext.Events
                .FromSql(
                    $"""
                     select * from events
                     where event_name = {nameof(LegacyEvents.EmailSentEvent)}
                     and payload -> 'Email' ->> 'TemplateId' = {EmailTemplateIds.TrnGeneratedForNpq}
                     """)
                .AsNoTracking()
                .OrderBy(e => e.Created)
                .ToListAsync(cancellationToken))
            .Select(e => (Row: e, Payload: (LegacyEvents.EmailSentEvent)e.ToEventBase()))
            .ToArray();

        if (legacyEvents.Length == 0)
        {
            return;
        }

        // Two things make an email already done, and both have to be checked. An earlier run of this job
        // reused the legacy event's id, so that shows up by id; the journey's own sends were written with a
        // fresh id alongside the process event, so those only show up by the email they point at.
        var legacyEventIds = legacyEvents.Select(e => e.Row.EventId).ToArray();

        var alreadyBackfilledEventIds = (await dbContext.ProcessEvents
                .Where(pe => legacyEventIds.Contains(pe.ProcessEventId))
                .Select(pe => pe.ProcessEventId)
                .ToListAsync(cancellationToken))
            .ToHashSet();

        var emailIds = legacyEvents.Select(e => e.Payload.Email.EmailId.ToString()).ToArray();

        var alreadyAttachedEmailIds = (await dbContext.Database
                .SqlQuery<string>(
                    $"""
                     select pe.payload -> 'Email' ->> 'EmailId' as "Value" from process_events pe
                     where pe.event_name = {nameof(EmailSentEvent)}
                     and pe.payload -> 'Email' ->> 'EmailId' = any({emailIds})
                     """)
                .ToListAsync(cancellationToken))
            .Select(Guid.Parse)
            .ToHashSet();

        var toBackfill = legacyEvents
            .Where(e => !alreadyBackfilledEventIds.Contains(e.Row.EventId))
            .Where(e => !alreadyAttachedEmailIds.Contains(e.Payload.Email.EmailId))
            .ToArray();

        if (toBackfill.Length == 0)
        {
            return;
        }

        var trns = toBackfill
            .Select(e => GetTrn(e.Payload))
            .Where(trn => trn is not null)
            .Distinct()
            .ToArray();

        // The person may have been deactivated since the TRN was allocated.
        var personIdsByTrn = await dbContext.Persons
            .IgnoreQueryFilters([QueryFilterNames.Person.Deactivated])
            .Where(p => p.Trn != null && trns.Contains(p.Trn))
            .Select(p => new { p.Trn, p.PersonId })
            .ToDictionaryAsync(p => p.Trn!, p => p.PersonId, cancellationToken);

        var personIds = personIdsByTrn.Values.ToArray();

        // The process that created the person is the one holding their PersonCreatedEvent. A person with
        // more than one is ambiguous, so it's left alone rather than guessed at.
        var creatingProcessIdsByPersonId = (await dbContext.ProcessEvents
                .Where(pe => pe.EventName == nameof(PersonCreatedEvent))
                .Where(pe => pe.PersonIds.Any(id => personIds.Contains(id)))
                .Select(pe => new { pe.ProcessId, pe.PersonIds })
                .ToListAsync(cancellationToken))
            .SelectMany(pe => pe.PersonIds.Select(personId => (PersonId: personId, pe.ProcessId)))
            .GroupBy(pe => pe.PersonId)
            .ToDictionary(g => g.Key, g => g.Select(pe => pe.ProcessId).Distinct().ToArray());

        // Referencing the emails row rather than the payload's copy of it keeps the event pointing at
        // something that still exists; an email that has since been deleted is skipped.
        var emails = await dbContext.Emails
            .Where(e => emailIds.Contains(e.EmailId.ToString()))
            .ToDictionaryAsync(e => e.EmailId, cancellationToken);

        foreach (var (row, payload) in toBackfill)
        {
            if (GetTrn(payload) is not string trn ||
                !personIdsByTrn.TryGetValue(trn, out var personId) ||
                !creatingProcessIdsByPersonId.TryGetValue(personId, out var processIds) ||
                processIds.Length != 1 ||
                !emails.TryGetValue(payload.Email.EmailId, out var email))
            {
                continue;
            }

            IEvent newEvent = new EmailSentEvent
            {
                EventId = row.EventId,
                PersonId = personId,
                Email = EventModels.Email.FromModel(email)
            };

            dbContext.ProcessEvents.Add(new ProcessEvent
            {
                ProcessEventId = newEvent.EventId,
                ProcessId = processIds[0],
                EventName = newEvent.GetType().Name,
                Payload = newEvent,
                PersonIds = newEvent.PersonIds,
                OneLoginUserSubjects = newEvent.OneLoginUserSubjects,
                SupportTaskReferences = newEvent.SupportTaskReferences,
                CreatedOn = email.SentOn ?? row.Created
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    private static string? GetTrn(LegacyEvents.EmailSentEvent payload) =>
        payload.Email.Personalization.TryGetValue(TrnPersonalizationKey, out var trn) ? trn : null;
}
