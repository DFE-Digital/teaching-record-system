using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Api.Services.AccessYourQualifications;
using TeachingRecordSystem.Api.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Api.Services.GetAnIdentityApi;
using TeachingRecordSystem.Api.Services.Notify;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events;

namespace TeachingRecordSystem.Api.Jobs;

public class SendEytsAwardedEmailJob
{
    private const string EytsAwardedEmailConfirmationTemplateId = "f85babdb-049b-4f32-9579-2a812acc0a2b";
    private const string LinkToAccessYourQualificationsServicePersonalisationKey = "link to access your teaching qualifications service";
    private readonly INotificationSender _notificationSender;
    private readonly TrsDbContext _dbContext;
    private readonly IGetAnIdentityApiClient _identityApiClient;
    private readonly IClock _clock;
    private readonly AccessYourQualificationsOptions _accessYourQualificationsOptions;

    public SendEytsAwardedEmailJob(
        INotificationSender notificationSender,
        TrsDbContext dbContext,
        IGetAnIdentityApiClient identityApiClient,
        IOptions<AccessYourQualificationsOptions> accessYourQualificationsOptions,
        IClock clock)
    {
        _notificationSender = notificationSender;
        _dbContext = dbContext;
        _identityApiClient = identityApiClient;
        _clock = clock;
        _accessYourQualificationsOptions = accessYourQualificationsOptions.Value;
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
            item.Personalization[LinkToAccessYourQualificationsServicePersonalisationKey] = $"{_accessYourQualificationsOptions.BaseAddress}/qualifications/start?trn_token={tokenResponse.TrnToken}";
        }

        await _notificationSender.SendEmail(EytsAwardedEmailConfirmationTemplateId, item.EmailAddress, item.Personalization);
        item.EmailSent = true;

        _dbContext.AddEvent(new EytsAwardedEmailSentEvent
        {
            EytsAwardedEmailsJobId = eytsAwardedEmailsJobId,
            PersonId = personId,
            EmailAddress = item.EmailAddress,
            CreatedUtc = _clock.UtcNow,
        });

        await _dbContext.SaveChangesAsync();
    }
}
