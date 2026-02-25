using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.Notify;

namespace TeachingRecordSystem.Core.Jobs;

public class SendInductionCompletedEmailJob(
    INotificationSender notificationSender,
    TrsDbContext dbContext,
    IGetAnIdentityApiClient identityApiClient,
    IOptions<AccessYourTeachingQualificationsOptions> accessYourTeachingQualificationsOptions,
    TimeProvider timeProvider)
{
    private const string LinkToAccessYourQualificationsServicePersonalisationKey = "link to access your teaching qualifications service";

    private readonly AccessYourTeachingQualificationsOptions _accessYourTeachingQualificationsOptions = accessYourTeachingQualificationsOptions.Value;

    public async Task ExecuteAsync(Guid inductionCompletedEmailsJobId, Guid personId)
    {
        var item = await dbContext.InductionCompletedEmailsJobItems.SingleAsync(i => i.InductionCompletedEmailsJobId == inductionCompletedEmailsJobId && i.PersonId == personId);

        if (!item.Personalization.ContainsKey(LinkToAccessYourQualificationsServicePersonalisationKey))
        {
            var request = new CreateTrnTokenRequest
            {
                Trn = item.Trn,
                Email = item.EmailAddress
            };

            var tokenResponse = await identityApiClient.CreateTrnTokenAsync(request);
            item.Personalization[LinkToAccessYourQualificationsServicePersonalisationKey] = $"{_accessYourTeachingQualificationsOptions.BaseAddress}{_accessYourTeachingQualificationsOptions.StartUrlPath}?trn_token={tokenResponse.TrnToken}";
        }

        await notificationSender.SendEmailAsync(EmailTemplateIds.InductionCompletedEmailConfirmation, item.EmailAddress, item.Personalization);
        item.EmailSent = true;

        dbContext.AddEventWithoutBroadcast(new InductionCompletedEmailSentEvent
        {
            EventId = Guid.NewGuid(),
            InductionCompletedEmailsJobId = inductionCompletedEmailsJobId,
            PersonId = personId,
            EmailAddress = item.EmailAddress,
            CreatedUtc = timeProvider.UtcNow,
            RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId
        });

        await dbContext.SaveChangesAsync();
    }
}
