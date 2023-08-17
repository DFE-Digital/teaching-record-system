using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.Core.Services.AccessYourQualifications;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.Notify;

namespace TeachingRecordSystem.Core.Jobs;

public class SendInternationalQtsAwardedEmailJob
{
    private const string InternationalQtsAwardedEmailConfirmationTemplateId = "f4200027-de67-4a55-808a-b37ae2653660";
    private const string LinkToAccessYourQualificationsServicePersonalisationKey = "link to access your teaching qualifications service";
    private readonly INotificationSender _notificationSender;
    private readonly TrsDbContext _dbContext;
    private readonly IGetAnIdentityApiClient _identityApiClient;
    private readonly IClock _clock;
    private readonly AccessYourQualificationsOptions _accessYourQualificationsOptions;

    public SendInternationalQtsAwardedEmailJob(
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

    public async Task Execute(Guid internationalQtsAwardedEmailsJobId, Guid personId)
    {
        var item = await _dbContext.InternationalQtsAwardedEmailsJobItems.SingleAsync(i => i.InternationalQtsAwardedEmailsJobId == internationalQtsAwardedEmailsJobId && i.PersonId == personId);

        if (!item.Personalization.ContainsKey(LinkToAccessYourQualificationsServicePersonalisationKey))
        {
            var request = new CreateTrnTokenRequest
            {
                Trn = item.Trn,
                Email = item.EmailAddress
            };

            var tokenResponse = await _identityApiClient.CreateTrnToken(request);
            item.Personalization[LinkToAccessYourQualificationsServicePersonalisationKey] = $"{_accessYourQualificationsOptions.BaseAddress}{_accessYourQualificationsOptions.StartUrlPath}?trn_token={tokenResponse.TrnToken}";
        }

        await _notificationSender.SendEmail(InternationalQtsAwardedEmailConfirmationTemplateId, item.EmailAddress, item.Personalization);
        item.EmailSent = true;

        _dbContext.AddEvent(new InternationalQtsAwardedEmailSentEvent
        {
            InternationalQtsAwardedEmailsJobId = internationalQtsAwardedEmailsJobId,
            PersonId = personId,
            EmailAddress = item.EmailAddress,
            CreatedUtc = _clock.UtcNow,
        });

        await _dbContext.SaveChangesAsync();
    }
}
