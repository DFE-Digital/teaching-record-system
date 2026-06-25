using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

public partial class OneLoginUserMatchingSupportTaskService
{
    public async Task<SupportTask> CreateRecordMatchingSupportTaskAsync(
        CreateOneLoginUserRecordMatchingSupportTaskOptions options,
        ProcessContext processContext)
    {
        var trnRequest = options.TrnRequestId is not null
            ? (options.ClientApplicationUserId, options.TrnRequestId)
            : ((Guid ApplicationUserId, string RequestId)?)null;

        var supportTask = await supportTaskService.CreateSupportTaskAsync(
            new CreateSupportTaskOptions
            {
                SupportTaskType = SupportTaskType.OneLoginUserRecordMatching,
                Data = new OneLoginUserRecordMatchingData
                {
                    OneLoginUserSubject = options.OneLoginUserSubject,
                    OneLoginUserEmail = options.OneLoginUserEmail,
                    VerifiedNames = options.VerifiedNames,
                    VerifiedDatesOfBirth = options.VerifiedDatesOfBirth,
                    StatedNationalInsuranceNumber = options.StatedNationalInsuranceNumber,
                    StatedTrn = options.StatedTrn,
                    ClientApplicationUserId = options.ClientApplicationUserId,
                    TrnTokenTrn = options.TrnTokenTrn
                },
                PersonId = null,
                OneLoginUserSubject = options.OneLoginUserSubject,
                TrnRequest = trnRequest
            },
            processContext);

        return supportTask;
    }

    public async Task<ResolveRecordMatchingSupportTaskResult> ResolveRecordMatchingSupportTaskAsync(
        NotConnectingOutcomeOptions options,
        ProcessContext processContext)
    {
        var supportTask = options.SupportTask;
        ThrowIfSupportTaskIsClosed(supportTask);

        var data = supportTask.GetData<OneLoginUserRecordMatchingData>();

        var applicationUser = await GetApplicationUserAsync(supportTask);
        var appContent = applicationUser.AppContent;
        var recordMatchingPolicy = applicationUser.RecordMatchingPolicy;

        bool emailSent = false;

        if (recordMatchingPolicy == RecordMatchingPolicy.Deferred && appContent?.OneLoginNotConnectedEmailTemplateId is { } templateId)
        {
            var firstVerifiedOrStatedName = data.VerifiedOrStatedNames!.First();
            var name = $"{firstVerifiedOrStatedName.First()} {firstVerifiedOrStatedName.LastOrDefault()}";
            var reason = options.NotConnectingReason is OneLoginUserNotConnectingReason.AnotherReason
                ? options.NotConnectingAdditionalDetails!
                : options.NotConnectingReason.GetDisplayName()!;

            await oneLoginService.EnqueueNotConnectedEmailAsync(
                supportTask.OneLoginUser!.EmailAddress!,
                name,
                reason,
                templateId,
                processContext);

            emailSent = true;
        }

        await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserRecordMatchingData>
            {
                SupportTaskReference = supportTask.SupportTaskReference,
                UpdateData = data => data with
                {
                    Outcome = OneLoginUserRecordMatchingOutcome.NotConnecting,
                    NotConnectingReason = options.NotConnectingReason,
                    NotConnectingAdditionalDetails = options.NotConnectingAdditionalDetails
                },
                Status = SupportTaskStatus.Closed
            },
            processContext);

        return new() { EmailSent = emailSent };
    }

    public async Task<ResolveRecordMatchingSupportTaskResult> ResolveRecordMatchingSupportTaskAsync(NoMatchesOutcomeOptions options, ProcessContext processContext)
    {
        var supportTask = options.SupportTask;
        ThrowIfSupportTaskIsClosed(supportTask);

        var data = supportTask.GetData<OneLoginUserRecordMatchingData>();

        var applicationUser = await dbContext.ApplicationUsers.SingleAsync(u => u.UserId == data.ClientApplicationUserId);
        var appContent = applicationUser.AppContent;

        await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserRecordMatchingData>
            {
                SupportTaskReference = supportTask.SupportTaskReference,
                UpdateData = data => data with
                {
                    Outcome = OneLoginUserRecordMatchingOutcome.NoMatches
                },
                Status = SupportTaskStatus.Closed
            },
            processContext);

        var emailTemplateId = applicationUser.RecordMatchingPolicy is RecordMatchingPolicy.Deferred
            ? null
            : appContent?.OneLoginCannotFindRecordEmailTemplateId;

        if (emailTemplateId is not null)
        {
            var firstVerifiedOrStatedName = data.VerifiedOrStatedNames!.First();
            var name = $"{firstVerifiedOrStatedName.First()} {firstVerifiedOrStatedName.LastOrDefault()}";

            await oneLoginService.EnqueueRecordNotFoundEmailAsync(
                supportTask.OneLoginUser!.EmailAddress!,
                name,
                emailTemplateId,
                appContent?.SupportEmailAddressNotifyId,
                processContext);

            return new() { EmailSent = true };
        }

        return new() { EmailSent = false };
    }

    public async Task ResolveRecordMatchingSupportTaskAsync(ConnectedOutcomeOptions options, ProcessContext processContext)
    {
        var supportTask = options.SupportTask;
        ThrowIfSupportTaskIsClosed(supportTask);

        var data = supportTask.GetData<OneLoginUserRecordMatchingData>();

        await oneLoginService.SetUserMatchedAsync(
            new SetUserMatchedOptions
            {
                OneLoginUserSubject = supportTask.OneLoginUserSubject!,
                MatchedPersonId = options.MatchedPersonId,
                MatchRoute = OneLoginUserMatchRoute.SupportUi,
                MatchedAttributes = options.MatchedAttributes
            },
            processContext);

        await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserRecordMatchingData>
            {
                SupportTaskReference = supportTask.SupportTaskReference,
                UpdateData = data => data with
                {
                    PersonId = options.MatchedPersonId,
                    Outcome = OneLoginUserRecordMatchingOutcome.Connected
                },
                Status = SupportTaskStatus.Closed
            },
            processContext);

        if (supportTask.TrnRequestId is not null)
        {
            await trnRequestService.ResolveTrnRequestWithMatchedPersonAsync(
                supportTask.TrnRequestApplicationUserId!.Value,
                supportTask.TrnRequestId,
                (options.MatchedPersonId, options.Trn),
                options.MatchedAttributes.Select(kvp => kvp.Key).ToArray(),
                processContext);
        }
        else
        {
            var appContent = await GetAppContentAsync(data.ClientApplicationUserId);

            var firstVerifiedOrStatedName = data.VerifiedOrStatedNames!.First();
            var name = $"{firstVerifiedOrStatedName.First()} {firstVerifiedOrStatedName.LastOrDefault()}";

            await oneLoginService.EnqueueRecordMatchedEmailAsync(
                supportTask.OneLoginUser!.EmailAddress!,
                name,
                appContent?.OneLoginRecordMatchedEmailTemplateId,
                appContent?.SupportEmailAddressNotifyId, processContext);
        }
    }
}
