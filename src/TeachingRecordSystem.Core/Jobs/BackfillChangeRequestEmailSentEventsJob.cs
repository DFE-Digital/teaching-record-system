using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks.ChangeRequests;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Adds the missing <see cref="EmailSentEvent"/> to change request approval and rejection
/// <see cref="Process"/>es. Both journeys sent their confirmation email without a process context
/// until recently, so the email never made it onto the process.
/// </summary>
public class BackfillChangeRequestEmailSentEventsJob(TrsDbContext dbContext)
{
    private static readonly ProcessType[] _processTypes =
    [
        ProcessType.ChangeOfNameRequestApproving,
        ProcessType.ChangeOfDateOfBirthRequestApproving,
        ProcessType.ChangeOfNameRequestRejecting,
        ProcessType.ChangeOfDateOfBirthRequestRejecting
    ];

    private static readonly string[] _emailTemplateIds =
    [
        EmailTemplateIds.GetAnIdentityChangeOfNameApprovedEmailConfirmation,
        EmailTemplateIds.GetAnIdentityChangeOfDateOfBirthApprovedEmailConfirmation,
        EmailTemplateIds.GetAnIdentityChangeOfNameRejectedEmailConfirmation,
        EmailTemplateIds.GetAnIdentityChangeOfDateOfBirthRejectedEmailConfirmation
    ];

    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var emailMatcher = await SentEmailMatcher.CreateAsync(dbContext, _emailTemplateIds, _processTypes, cancellationToken);

        // Only processes that don't already have the event, so the job is idempotent.
        var processes = await dbContext.Processes
            .Where(p => _processTypes.Contains(p.ProcessType))
            .Where(p => !dbContext.ProcessEvents.Any(pe => pe.ProcessId == p.ProcessId && pe.EventName == nameof(EmailSentEvent)))
            .Include(p => p.Events)
            .OrderBy(p => p.CreatedOn)
            .ToListAsync(cancellationToken);

        var personIds = processes.SelectMany(p => p.PersonIds).Distinct().ToArray();

        // The person may have been deactivated since the change request was resolved.
        var persons = await dbContext.Persons
            .IgnoreQueryFilters()
            .Where(p => personIds.Contains(p.PersonId))
            .ToDictionaryAsync(p => p.PersonId, cancellationToken);

        var emailAddressHistory = await PersonEmailAddressHistory.CreateAsync(dbContext, personIds, cancellationToken);

        foreach (var process in processes)
        {
            var payloads = process.Events!.Select(e => e.Payload).ToArray();

            var supportTaskUpdatedEvent = payloads.OfType<SupportTaskUpdatedEvent>().SingleOrDefault();
            if (supportTaskUpdatedEvent?.SupportTask.PersonId is not Guid personId)
            {
                continue;
            }

            persons.TryGetValue(personId, out var person);

            var supportTask = supportTaskUpdatedEvent.SupportTask;
            var personDetailsUpdatedEvent = payloads.OfType<PersonDetailsUpdatedEvent>().SingleOrDefault();
            var isApproval = process.ProcessType is ProcessType.ChangeOfNameRequestApproving or ProcessType.ChangeOfDateOfBirthRequestApproving;

            var (requestEmailAddress, templateId) = supportTask.Data switch
            {
                ChangeNameRequestData data => (
                    data.EmailAddress,
                    isApproval
                        ? EmailTemplateIds.GetAnIdentityChangeOfNameApprovedEmailConfirmation
                        : EmailTemplateIds.GetAnIdentityChangeOfNameRejectedEmailConfirmation),
                ChangeDateOfBirthRequestData data => (
                    data.EmailAddress,
                    isApproval
                        ? EmailTemplateIds.GetAnIdentityChangeOfDateOfBirthApprovedEmailConfirmation
                        : EmailTemplateIds.GetAnIdentityChangeOfDateOfBirthRejectedEmailConfirmation),
                _ => (null, null)
            };

            if (templateId is null)
            {
                continue;
            }

            // The address on the record may have changed since, so fall back to the one it held at the time
            // rather than whatever is on it now. A process that changed the person's details carries that
            // address itself.
            var emailAddress = !string.IsNullOrEmpty(requestEmailAddress)
                ? requestEmailAddress
                : personDetailsUpdatedEvent?.PersonDetails.EmailAddress
                    ?? emailAddressHistory.GetEmailAddressAt(personId, process.CreatedOn);

            // Nothing to send to means nothing was sent.
            if (string.IsNullOrEmpty(emailAddress))
            {
                continue;
            }

            // An approval applies the change before emailing, so the person is addressed by their new name.
            var firstName = personDetailsUpdatedEvent?.PersonDetails.FirstName ?? person?.FirstName;

            if (firstName is null)
            {
                continue;
            }

            var email = emailMatcher.Match(templateId, emailAddress, process.CreatedOn);

            if (email is null)
            {
                email = new Email
                {
                    EmailId = Guid.NewGuid(),
                    TemplateId = templateId,
                    EmailAddress = emailAddress,
                    Personalization = CreatePersonalization(firstName, isApproval ? null : supportTaskUpdatedEvent.RejectionReason),
                    SentOn = process.CreatedOn
                };

                dbContext.Emails.Add(email);
                emailMatcher.Claim(email.EmailId);
            }

            IEvent emailSentEvent = new EmailSentEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = personId,
                Email = EventModels.Email.FromModel(email)
            };

            dbContext.ProcessEvents.Add(new ProcessEvent
            {
                ProcessEventId = emailSentEvent.EventId,
                ProcessId = process.ProcessId,
                EventName = emailSentEvent.GetType().Name,
                Payload = emailSentEvent,
                PersonIds = emailSentEvent.PersonIds,
                OneLoginUserSubjects = emailSentEvent.OneLoginUserSubjects,
                SupportTaskReferences = emailSentEvent.SupportTaskReferences,
                CreatedOn = email.SentOn ?? process.CreatedOn
            });

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

    private static Dictionary<string, string> CreatePersonalization(string firstName, string? rejectionReasonDisplayName)
    {
        var personalization = new Dictionary<string, string>
        {
            { ChangeRequestEmailConstants.FirstNameEmailPersonalisationKey, firstName }
        };

        // The event only holds the reason's display name, so map it back to get the wording the email used.
        var rejectionReason = Enum.GetValues<ChangeRequestRejectReason>()
            .Cast<ChangeRequestRejectReason?>()
            .SingleOrDefault(r => r!.Value.GetDisplayName() == rejectionReasonDisplayName);

        // A request the user no longer needs is cancelled rather than rejected and has no rejection email.
        if (rejectionReason is ChangeRequestRejectReason reason and not ChangeRequestRejectReason.ChangeNoLongerRequired)
        {
            personalization[ChangeRequestEmailConstants.RejectionReasonEmailPersonalisationKey] =
                ChangeRequestSupportTaskService.GetRejectionReasonEmailText(reason);
        }

        return personalization;
    }
}
