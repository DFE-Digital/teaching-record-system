using System.Transactions;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.Core.Jobs;

public class SendAytqInviteEmailJob(
    INotificationSender notificationSender,
    TrsDbContext dbContext,
    IEventPublisher eventPublisher,
    IOptions<AccessYourTeachingQualificationsOptions> aytqOptions,
    TrnRequestService trnRequestService,
    TimeProvider timeProvider) :
    SendEmailJob(dbContext, eventPublisher, notificationSender, timeProvider)
{
    private const string MagicLinkPersonalizationKey = "link to access your teaching qualifications service";

    public class JobMetadataKeys
    {
        public const string Trn = "Trn";
        public const string PersonId = "PersonId";
    }

    public override async Task ExecuteAsync(Guid emailId)
    {
        using var txn = new TransactionScope(
            TransactionScopeOption.RequiresNew,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        var email = await GetEmailByIdAsync(emailId);

        // Ensure we've got the magic link personalization set
        if (!email.Personalization.ContainsKey(MagicLinkPersonalizationKey))
        {

            var trn = email.Metadata[JobMetadataKeys.Trn].ToString() ??
                throw new InvalidOperationException("TRN is missing from email metadata.");
            var emailAddress = email.EmailAddress;
            var trnToken = await trnRequestService.CreateTrnTokenAsync(trn, emailAddress);

            email.Personalization[MagicLinkPersonalizationKey] =
                $"{aytqOptions.Value.BaseAddress}{aytqOptions.Value.StartUrlPath}?trn_token={trnToken}";

            await DbContext.SaveChangesAsync();
        }

        await SendEmailAsync(email);

        // Emails queued before the process was introduced don't record who they went to, so there's no
        // person to hang a process off; the email has still been sent.
        if (email.Metadata.TryGetValue(JobMetadataKeys.PersonId, out var personIdValue) &&
            Guid.TryParse(personIdValue?.ToString(), out var personId))
        {
            var processContext = new ProcessContext(
                ProcessType.NotifyingProfessionalStatusAwardee,
                email.SentOn!.Value,
                SystemUser.SystemUserId);

            await EventPublisher.PublishSingleEventAsync(
                new EmailSentEvent
                {
                    PersonId = personId,
                    Email = EventModels.Email.FromModel(email)
                },
                processContext);
        }

        txn.Complete();
    }
}
