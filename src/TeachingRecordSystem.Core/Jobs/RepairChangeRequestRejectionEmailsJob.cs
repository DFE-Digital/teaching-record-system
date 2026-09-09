using Microsoft.Extensions.Logging;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Repairs the change request rejections that <see cref="BackfillChangeRequestEmailSentEventsJob"/> gave a
/// made-up email when it ran before it knew about the retired rejection templates.
/// </summary>
/// <remarks>
/// That job couldn't see the emails sent on the templates #2661 retired, so for a rejection processed before
/// the swap it took its "no email found" branch: it added an <see cref="Email"/> row stamped with the current
/// template id and a rejection reason the real email never carried, and pointed the
/// <see cref="EmailSentEvent"/> at that instead of the email that was actually sent. Re-running it fixes
/// nothing, because it skips any process that already has an <see cref="EmailSentEvent"/>.
///
/// A rejection processed before the swap whose event points at a post-swap template is the signature: those
/// template ids did not exist when it ran, so no real email can carry one. Each is repointed at the email that
/// was really sent and the fabricated row is deleted. Anything that can't be matched back to a real email is
/// left exactly as it is and reported — a wrong-but-present event is recoverable, a deleted row it still
/// references is not.
/// </remarks>
public class RepairChangeRequestRejectionEmailsJob(
    TrsDbContext dbContext,
    ILogger<RepairChangeRequestRejectionEmailsJob> logger)
{
    private static readonly ProcessType[] _processTypes =
    [
        ProcessType.ChangeOfNameRequestRejecting,
        ProcessType.ChangeOfDateOfBirthRequestRejecting
    ];

    private static readonly string[] _retiredTemplateIds =
    [
        RetiredEmailTemplateIds.ChangeOfNameRequestRejectedEmailConfirmation,
        RetiredEmailTemplateIds.ChangeOfDateOfBirthRequestRejectedEmailConfirmation
    ];

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var processes = await dbContext.Processes
            .Where(p => _processTypes.Contains(p.ProcessType))
            .Where(p => p.CreatedOn < RetiredEmailTemplateIds.RejectionTemplatesChangedOn)
            .Include(p => p.Events)
            .ToListAsync(cancellationToken);

        var emailMatcher = await SentEmailMatcher.CreateAsync(dbContext, _retiredTemplateIds, _processTypes, cancellationToken);

        var fabricatedEmailIds = new List<Guid>();
        var unmatched = 0;

        foreach (var process in processes)
        {
            var processEvent = process.Events!.SingleOrDefault(pe => pe.EventName == nameof(EmailSentEvent));

            if (processEvent?.Payload is not EmailSentEvent emailSentEvent ||
                GetRetiredTemplateId(emailSentEvent.Email.TemplateId) is not string retiredTemplateId)
            {
                // Either there's nothing here to repair or it already points at a retired template.
                continue;
            }

            var realEmail = emailMatcher.Match(retiredTemplateId, emailSentEvent.Email.EmailAddress, process.CreatedOn);

            if (realEmail is null)
            {
                logger.LogWarning(
                    "No email on template {TemplateId} found for process {ProcessId}; leaving it alone.",
                    retiredTemplateId,
                    process.ProcessId);

                unmatched++;
                continue;
            }

            dbContext.Entry(processEvent).Property(e => e.Payload).CurrentValue = new EmailSentEvent
            {
                EventId = emailSentEvent.EventId,
                PersonId = emailSentEvent.PersonId,
                Email = EventModels.Email.FromModel(realEmail)
            };

            dbContext.Entry(processEvent).Property(e => e.CreatedOn).CurrentValue = realEmail.SentOn ?? processEvent.CreatedOn;

            fabricatedEmailIds.Add(emailSentEvent.Email.EmailId);
        }

        // Only rows this repair has just orphaned, and only ones that still carry the fabricated signature.
        var toDelete = await dbContext.Emails
            .Where(e => fabricatedEmailIds.Contains(e.EmailId))
            .Where(e => e.SentOn < RetiredEmailTemplateIds.RejectionTemplatesChangedOn)
            .Where(e => !_retiredTemplateIds.Contains(e.TemplateId))
            .ToListAsync(cancellationToken);

        dbContext.Emails.RemoveRange(toDelete);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Repaired {Repaired} rejection emails and deleted {Deleted} fabricated rows; {Unmatched} left alone.",
            fabricatedEmailIds.Count,
            toDelete.Count,
            unmatched);
    }

    // A rejection whose event points at a current template was processed before that template existed, so the
    // email it should point at is the retired one for the same change type.
    private static string? GetRetiredTemplateId(string templateId) => templateId switch
    {
        EmailTemplateIds.GetAnIdentityChangeOfNameRejectedEmailConfirmation =>
            RetiredEmailTemplateIds.ChangeOfNameRequestRejectedEmailConfirmation,
        EmailTemplateIds.GetAnIdentityChangeOfDateOfBirthRejectedEmailConfirmation =>
            RetiredEmailTemplateIds.ChangeOfDateOfBirthRequestRejectedEmailConfirmation,
        _ => null
    };
}
