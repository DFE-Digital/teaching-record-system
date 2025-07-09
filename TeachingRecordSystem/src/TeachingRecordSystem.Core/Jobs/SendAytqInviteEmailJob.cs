using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.Notify;

namespace TeachingRecordSystem.Core.Jobs;

public class SendAytqInviteEmailJob(
    INotificationSender notificationSender,
    TrsDbContext dbContext,
    IGetAnIdentityApiClient identityApiClient,
    IOptions<AccessYourTeachingQualificationsOptions> aytqOptions,
    IClock clock) :
    SendEmailJob(dbContext, notificationSender, clock)
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
            var trnTokenResponse = await identityApiClient.CreateTrnTokenAsync(
                new CreateTrnTokenRequest
                {
                    Trn = email.Metadata[JobMetadataKeys.Trn].ToString() ?? throw new InvalidOperationException("TRN is missing from email metadata."),
                    Email = email.EmailAddress
                });

            email.Personalization[MagicLinkPersonalizationKey] =
                $"{aytqOptions.Value.BaseAddress}{aytqOptions.Value.StartUrlPath}?trn_token={trnTokenResponse.TrnToken}";

            await DbContext.SaveChangesAsync();
        }

        await SendEmailAsync(email);
    }
}
