using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.Notify;

namespace TeachingRecordSystem.Core.Jobs;

public class SendInductionCompletedEmailJob(
    INotificationSender notificationSender,
    TrsDbContext dbContext,
    IGetAnIdentityApiClient identityApiClient,
    IOptions<AccessYourTeachingQualificationsOptions> accessYourTeachingQualificationsOptions,
    IClock clock)
{
    private const string InductionCompletedEmailConfirmationTemplateId = "8029faa8-8409-4423-a717-c142dfd2ba86";
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

        await notificationSender.SendEmailAsync(InductionCompletedEmailConfirmationTemplateId, item.EmailAddress, item.Personalization);
        item.EmailSent = true;

        dbContext.AddEvent(new InductionCompletedEmailSentEvent
        {
            EventId = Guid.NewGuid(),
            InductionCompletedEmailsJobId = inductionCompletedEmailsJobId,
            PersonId = personId,
            EmailAddress = item.EmailAddress,
            CreatedUtc = clock.UtcNow,
            RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId
        });

        await dbContext.SaveChangesAsync();
    }
}
