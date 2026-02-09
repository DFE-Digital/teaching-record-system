using System.Diagnostics;
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

        var updateTaskResult = await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserIdVerificationData>
            {
                SupportTask = supportTask,
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
        Debug.Assert(updateTaskResult is UpdateSupportTaskResult.Ok);

        var name = $"{data.StatedFirstName} {data.StatedLastName}";
        var reason = options.RejectReason is OneLoginIdVerificationRejectReason.AnotherReason
            ? options.RejectionAdditionalDetails!
            : options.RejectReason.GetDisplayName()!;
        await oneLoginService.EnqueueNotVerifiedEmailAsync(supportTask.OneLoginUser!.EmailAddress!, name, reason, processContext);
    }

    public async Task ResolveVerificationSupportTaskAsync(VerifiedOnlyWithMatchesOutcomeOptions options, ProcessContext processContext)
    {
        var supportTask = options.SupportTask;
        ThrowIfSupportTaskIsClosed(supportTask);

        var data = supportTask.GetData<OneLoginUserIdVerificationData>();

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

        var updateTaskResult = await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserIdVerificationData>
            {
                SupportTask = supportTask.SupportTaskReference,
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
        Debug.Assert(updateTaskResult is UpdateSupportTaskResult.Ok);

        var name = $"{data.StatedFirstName} {data.StatedLastName}";
        await oneLoginService.EnqueueRecordNotFoundEmailAsync(supportTask.OneLoginUser!.EmailAddress!, name, processContext);
    }

    public async Task ResolveVerificationSupportTaskAsync(VerifiedOnlyWithoutMatchesOutcomeOptions options, ProcessContext processContext)
    {
        var supportTask = options.SupportTask;
        ThrowIfSupportTaskIsClosed(supportTask);

        var data = supportTask.GetData<OneLoginUserIdVerificationData>();

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

        var updateTaskResult = await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserIdVerificationData>
            {
                SupportTask = supportTask.SupportTaskReference,
                UpdateData = data => data with
                {
                    Verified = true,
                    Outcome = OneLoginUserIdVerificationOutcome.VerifiedOnlyWithoutMatches
                },
                Status = SupportTaskStatus.Closed
            },
            processContext);
        Debug.Assert(updateTaskResult is UpdateSupportTaskResult.Ok);

        var name = $"{data.StatedFirstName} {data.StatedLastName}";
        await oneLoginService.EnqueueRecordNotFoundEmailAsync(supportTask.OneLoginUser!.EmailAddress!, name, processContext);
    }

    public async Task ResolveVerificationSupportTaskAsync(VerifiedAndConnectedOutcomeOptions options, ProcessContext processContext)
    {
        var supportTask = options.SupportTask;
        ThrowIfSupportTaskIsClosed(supportTask);

        var data = supportTask.GetData<OneLoginUserIdVerificationData>();

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
        await oneLoginService.EnqueueRecordMatchedEmailAsync(supportTask.OneLoginUser!.EmailAddress!, name, processContext);

        var updateTaskResult = await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserIdVerificationData>
            {
                SupportTask = supportTask.SupportTaskReference,
                UpdateData = data => data with
                {
                    Verified = true,
                    PersonId = options.MatchedPersonId,
                    Outcome = OneLoginUserIdVerificationOutcome.VerifiedAndConnected
                },
                Status = SupportTaskStatus.Closed
            },
            processContext);
        Debug.Assert(updateTaskResult is UpdateSupportTaskResult.Ok);
    }

    private void ThrowIfSupportTaskIsClosed(SupportTask supportTask)
    {
        if (supportTask.Status is SupportTaskStatus.Closed)
        {
            throw new InvalidOperationException("Support task is closed.");
        }
    }
}
