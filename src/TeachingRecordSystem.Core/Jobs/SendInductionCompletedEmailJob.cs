using System.Transactions;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.Core.Jobs;

public class SendInductionCompletedEmailJob(
    INotificationSender notificationSender,
    TrsDbContext dbContext,
    IEventPublisher eventPublisher,
    IOptions<AccessYourTeachingQualificationsOptions> accessYourTeachingQualificationsOptions,
    TimeProvider timeProvider,
    TrnRequestService trnRequestService)
{
    private const string LinkToAccessYourQualificationsServicePersonalisationKey = "link to access your teaching qualifications service";

    private readonly AccessYourTeachingQualificationsOptions _accessYourTeachingQualificationsOptions = accessYourTeachingQualificationsOptions.Value;

    public async Task ExecuteAsync(Guid inductionCompletedEmailsJobId, Guid personId)
    {
        using var txn = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        var item = await dbContext.InductionCompletedEmailsJobItems.SingleAsync(i => i.InductionCompletedEmailsJobId == inductionCompletedEmailsJobId && i.PersonId == personId);

        if (!item.Personalization.ContainsKey(LinkToAccessYourQualificationsServicePersonalisationKey))
        {
            var trn = item.Trn;
            var email = item.EmailAddress;
            var trnToken = await trnRequestService.CreateTrnTokenAsync(trn, email);

            item.Personalization[LinkToAccessYourQualificationsServicePersonalisationKey] = $"{_accessYourTeachingQualificationsOptions.BaseAddress}{_accessYourTeachingQualificationsOptions.StartUrlPath}?trn_token={trnToken}";
        }

        await notificationSender.SendEmailAsync(EmailTemplateIds.InductionCompletedEmailConfirmation, item.EmailAddress, item.Personalization);
        item.EmailSent = true;

        var sentOn = timeProvider.UtcNow;

        var sentEmail = new Email
        {
            EmailId = Guid.NewGuid(),
            TemplateId = EmailTemplateIds.InductionCompletedEmailConfirmation,
            EmailAddress = item.EmailAddress,
            Personalization = new Dictionary<string, string>(item.Personalization),
            Metadata = new Dictionary<string, object> { { SendAytqInviteEmailJob.JobMetadataKeys.Trn, item.Trn } },
            SentOn = sentOn
        };

        dbContext.Emails.Add(sentEmail);

        await dbContext.SaveChangesAsync();

        var processContext = new ProcessContext(ProcessType.NotifyingInductionCompletee, sentOn, SystemUser.SystemUserId);

        await eventPublisher.PublishSingleEventAsync(
            new EmailSentEvent
            {
                PersonId = personId,
                Email = EventModels.Email.FromModel(sentEmail)
            },
            processContext);

        txn.Complete();
    }
}
