using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.Notify;

namespace TeachingRecordSystem.Core.Jobs;

public class SendEytsAwardedEmailJob
{
    private const string EytsAwardedEmailConfirmationTemplateId = "f85babdb-049b-4f32-9579-2a812acc0a2b";
    private const string LinkToAccessYourQualificationsServicePersonalisationKey = "link to access your teaching qualifications service";
    private readonly INotificationSender _notificationSender;
    private readonly TrsDbContext _dbContext;
    private readonly IGetAnIdentityApiClient _identityApiClient;
    private readonly IClock _clock;
    private readonly AccessYourTeachingQualificationsOptions _accessYourTeachingQualificationsOptions;

    public SendEytsAwardedEmailJob(
        INotificationSender notificationSender,
        TrsDbContext dbContext,
        IGetAnIdentityApiClient identityApiClient,
        IOptions<AccessYourTeachingQualificationsOptions> accessYourTeachingQualificationsOptions,
        IClock clock)
    {
        _notificationSender = notificationSender;
        _dbContext = dbContext;
        _identityApiClient = identityApiClient;
        _clock = clock;
        _accessYourTeachingQualificationsOptions = accessYourTeachingQualificationsOptions.Value;
    }

    public async Task Execute(Guid eytsAwardedEmailsJobId, Guid personId)
    {
        var item = await _dbContext.EytsAwardedEmailsJobItems.SingleAsync(i => i.EytsAwardedEmailsJobId == eytsAwardedEmailsJobId && i.PersonId == personId);

        if (!item.Personalization.ContainsKey(LinkToAccessYourQualificationsServicePersonalisationKey))
        {
            var request = new CreateTrnTokenRequest
            {
                Trn = item.Trn,
                Email = item.EmailAddress
            };

            var tokenResponse = await _identityApiClient.CreateTrnToken(request);
            item.Personalization[LinkToAccessYourQualificationsServicePersonalisationKey] = $"{_accessYourTeachingQualificationsOptions.BaseAddress}{_accessYourTeachingQualificationsOptions.StartUrlPath}?trn_token={tokenResponse.TrnToken}";
        }

        await _notificationSender.SendEmail(EytsAwardedEmailConfirmationTemplateId, item.EmailAddress, item.Personalization);
        item.EmailSent = true;

        _dbContext.AddEvent(new EytsAwardedEmailSentEvent
        {
            EventId = Guid.NewGuid(),
            EytsAwardedEmailsJobId = eytsAwardedEmailsJobId,
            PersonId = personId,
            EmailAddress = item.EmailAddress,
            CreatedUtc = _clock.UtcNow,
            RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId
        });

        await _dbContext.SaveChangesAsync();
    }
}
