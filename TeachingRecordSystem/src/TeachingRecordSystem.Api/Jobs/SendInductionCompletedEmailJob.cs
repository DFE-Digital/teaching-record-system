﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.Api.Services.AccessYourQualifications;
using TeachingRecordSystem.Api.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Api.Services.GetAnIdentityApi;
using TeachingRecordSystem.Api.Services.Notify;

namespace TeachingRecordSystem.Api.Jobs;

public class SendInductionCompletedEmailJob
{
    private const string InductionCompletedEmailConfirmationTemplateId = "8029faa8-8409-4423-a717-c142dfd2ba86";
    private const string LinkToAccessYourQualificationsServicePersonalisationKey = "link to access your teaching qualifications service";
    private readonly INotificationSender _notificationSender;
    private readonly TrsContext _dbContext;
    private readonly IGetAnIdentityApiClient _identityApiClient;
    private readonly IClock _clock;
    private readonly AccessYourQualificationsOptions _accessYourQualificationsOptions;

    public SendInductionCompletedEmailJob(
        INotificationSender notificationSender,
        TrsContext dbContext,
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

    public async Task Execute(Guid inductionCompletedEmailsJobId, Guid personId)
    {
        var item = await _dbContext.InductionCompletedEmailsJobItems.SingleAsync(i => i.InductionCompletedEmailsJobId == inductionCompletedEmailsJobId && i.PersonId == personId);

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

        await _notificationSender.SendEmail(InductionCompletedEmailConfirmationTemplateId, item.EmailAddress, item.Personalization);
        item.EmailSent = true;

        _dbContext.AddEvent(new InductionCompletedEmailSentEvent
        {
            InductionCompletedEmailsJobId = inductionCompletedEmailsJobId,
            PersonId = personId,
            EmailAddress = item.EmailAddress,
            CreatedUtc = _clock.UtcNow,
        });

        await _dbContext.SaveChangesAsync();
    }
}
