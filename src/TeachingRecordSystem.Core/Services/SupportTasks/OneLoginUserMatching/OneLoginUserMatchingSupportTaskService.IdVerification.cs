using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

public partial class OneLoginUserMatchingSupportTaskService
{
    public async Task<SupportTask> CreateVerificationSupportTaskAsync(
        CreateOneLoginUserIdVerificationSupportTaskOptions options,
        ProcessContext processContext)
    {
        var supportTask = await supportTaskService.CreateSupportTaskAsync(
            new CreateSupportTaskOptions
            {
                SupportTaskType = SupportTaskType.OneLoginUserIdVerification,
                Data = new OneLoginUserIdVerificationData
                {
                    OneLoginUserSubject = options.OneLoginUserSubject,
                    StatedNationalInsuranceNumber = options.StatedNationalInsuranceNumber,
                    StatedTrn = options.StatedTrn,
                    ClientApplicationUserId = options.ClientApplicationUserId,
                    TrnTokenTrn = options.TrnTokenTrn,
                    StatedFirstName = options.StatedFirstName,
                    StatedLastName = options.StatedLastName,
                    StatedDateOfBirth = options.StatedDateOfBirth,
                    EvidenceFileId = options.EvidenceFileId,
                    EvidenceFileName = options.EvidenceFileName
                },
                PersonId = null,
                OneLoginUserSubject = options.OneLoginUserSubject,
                TrnRequest = null
            },
            processContext);

        return supportTask;
    }

    public async Task ResolveVerificationSupportTaskAsync(NotVerifiedOutcomeOptions options, ProcessContext processContext)
    {
        var supportTask = options.SupportTask;
        ThrowIfSupportTaskIsClosed(supportTask);

        var data = supportTask.GetData<OneLoginUserIdVerificationData>();

        var appContent = await GetAppContentAsync(data.ClientApplicationUserId);

        await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserIdVerificationData>
            {
                SupportTaskReference = supportTask.SupportTaskReference,
                UpdateData = data => data with
                {
                    Verified = false,
                    Outcome = OneLoginUserIdVerificationOutcome.NotVerified,
                    RejectReason = options.RejectReason,
                    RejectionAdditionalDetails = options.RejectionAdditionalDetails
                },
                Status = SupportTaskStatus.Closed
            },
            processContext);

        var name = $"{data.StatedFirstName} {data.StatedLastName}";
        var reason = options.RejectReason is OneLoginIdVerificationRejectReason.AnotherReason
            ? options.RejectionAdditionalDetails!
            : options.RejectReason.GetDisplayName()!;

        await oneLoginService.EnqueueNotVerifiedEmailAsync(
            supportTask.OneLoginUser!.EmailAddress!,
            name,
            reason,
            appContent?.OneLoginNotVerifiedEmailTemplateId,
            processContext);
    }

    public async Task ResolveVerificationSupportTaskAsync(VerifiedOnlyWithMatchesOutcomeOptions options, ProcessContext processContext)
    {
        var supportTask = options.SupportTask;
        ThrowIfSupportTaskIsClosed(supportTask);

        var data = supportTask.GetData<OneLoginUserIdVerificationData>();

        var applicationUser = await GetApplicationUserAsync(supportTask);
        var recordMatchingPolicy = applicationUser.RecordMatchingPolicy;
        var appContent = applicationUser.AppContent;

        await oneLoginService.SetUserVerifiedAsync(
            new SetUserVerifiedOptions
            {
                OneLoginUserSubject = supportTask.OneLoginUserSubject!,
                VerificationRoute = OneLoginUserVerificationRoute.Support,
                VerifiedDatesOfBirth = [data.StatedDateOfBirth],
                VerifiedNames = [[data.StatedFirstName, data.StatedLastName]],
                CoreIdentityClaimVc = null
            },
            processContext);

        if (recordMatchingPolicy == RecordMatchingPolicy.Deferred && appContent?.OneLoginNotConnectedEmailTemplateId is { } templateId)
        {
            var name = $"{data.StatedFirstName} {data.StatedLastName}";
            var reason = options.NotConnectingReason is OneLoginUserNotConnectingReason.AnotherReason
                ? options.NotConnectingAdditionalDetails!
                : options.NotConnectingReason.GetDisplayName()!;

            await oneLoginService.EnqueueNotConnectedEmailAsync(
                supportTask.OneLoginUser!.EmailAddress!,
                name,
                reason,
                templateId,
                processContext);
        }

        await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserIdVerificationData>
            {
                SupportTaskReference = supportTask.SupportTaskReference,
                UpdateData = data => data with
                {
                    Verified = true,
                    Outcome = OneLoginUserIdVerificationOutcome.VerifiedOnlyWithMatches,
                    NotConnectingReason = options.NotConnectingReason,
                    NotConnectingAdditionalDetails = options.NotConnectingAdditionalDetails
                },
                Status = SupportTaskStatus.Closed
            },
            processContext);
    }

    public async Task ResolveVerificationSupportTaskAsync(VerifiedOnlyWithoutMatchesOutcomeOptions options, ProcessContext processContext)
    {
        var supportTask = options.SupportTask;
        ThrowIfSupportTaskIsClosed(supportTask);

        var data = supportTask.GetData<OneLoginUserIdVerificationData>();

        var appContent = await GetAppContentAsync(data.ClientApplicationUserId);

        await oneLoginService.SetUserVerifiedAsync(
            new SetUserVerifiedOptions
            {
                OneLoginUserSubject = supportTask.OneLoginUserSubject!,
                VerificationRoute = OneLoginUserVerificationRoute.Support,
                VerifiedDatesOfBirth = [data.StatedDateOfBirth],
                VerifiedNames = [[data.StatedFirstName, data.StatedLastName]],
                CoreIdentityClaimVc = null
            },
            processContext);

        await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserIdVerificationData>
            {
                SupportTaskReference = supportTask.SupportTaskReference,
                UpdateData = data => data with
                {
                    Verified = true,
                    Outcome = OneLoginUserIdVerificationOutcome.VerifiedOnlyWithoutMatches
                },
                Status = SupportTaskStatus.Closed
            },
            processContext);

        var name = $"{data.StatedFirstName} {data.StatedLastName}";

        await oneLoginService.EnqueueRecordNotFoundEmailAsync(
            supportTask.OneLoginUser!.EmailAddress!,
            name,
            appContent?.OneLoginCannotFindRecordEmailTemplateId,
            appContent?.SupportEmailAddressNotifyId,
            processContext);
    }

    public async Task ResolveVerificationSupportTaskAsync(VerifiedAndConnectedOutcomeOptions options, ProcessContext processContext)
    {
        var supportTask = options.SupportTask;
        ThrowIfSupportTaskIsClosed(supportTask);

        var data = supportTask.GetData<OneLoginUserIdVerificationData>();

        var appContent = await GetAppContentAsync(data.ClientApplicationUserId);

        await oneLoginService.SetUserVerifiedAndMatchedAsync(
            new SetUserVerifiedAndMatchedOptions
            {
                OneLoginUserSubject = supportTask.OneLoginUserSubject!,
                VerificationRoute = OneLoginUserVerificationRoute.Support,
                VerifiedDatesOfBirth = [data.StatedDateOfBirth],
                VerifiedNames = [[data.StatedFirstName, data.StatedLastName]],
                CoreIdentityClaimVc = null,
                MatchedPersonId = options.MatchedPersonId,
                MatchRoute = OneLoginUserMatchRoute.SupportUi,
                MatchedAttributes = options.MatchedAttributes
            },
            processContext);

        var name = $"{data.StatedFirstName} {data.StatedLastName}";
        await oneLoginService.EnqueueRecordMatchedEmailAsync(
            supportTask.OneLoginUser!.EmailAddress!,
            name,
            appContent?.OneLoginRecordMatchedEmailTemplateId,
            appContent?.SupportEmailAddressNotifyId, processContext);

        await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserIdVerificationData>
            {
                SupportTaskReference = supportTask.SupportTaskReference,
                UpdateData = data => data with
                {
                    Verified = true,
                    PersonId = options.MatchedPersonId,
                    Outcome = OneLoginUserIdVerificationOutcome.VerifiedAndConnected
                },
                Status = SupportTaskStatus.Closed
            },
            processContext);
    }

    private void ThrowIfSupportTaskIsClosed(SupportTask supportTask)
    {
        if (supportTask.Status is SupportTaskStatus.Closed)
        {
            throw new InvalidOperationException("Support task is closed.");
        }
    }
}
