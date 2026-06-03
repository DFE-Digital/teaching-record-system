using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
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
    }

    public override async Task ExecuteAsync(Guid emailId)
    {
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
    }
}
