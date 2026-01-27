using System.Diagnostics;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

public partial class OneLoginUserMatchingSupportTaskService
{
    public async Task ResolveSupportTaskAsync(NotConnectingOutcomeOptions options, ProcessContext processContext)
    {
        var supportTask = options.SupportTask;
        ThrowIfSupportTaskIsClosed(supportTask);

        var data = supportTask.GetData<OneLoginUserRecordMatchingData>();

        var updateTaskResult = await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserRecordMatchingData>
            {
                SupportTask = supportTask.SupportTaskReference,
                UpdateData = data => data with
                {
                    Outcome = OneLoginUserRecordMatchingOutcome.NotConnecting,
                    NotConnectingReason = options.NotConnectingReason,
                    NotConnectingAdditionalDetails = options.NotConnectingAdditionalDetails
                },
                Status = SupportTaskStatus.Closed
            },
            processContext);
        Debug.Assert(updateTaskResult is UpdateSupportTaskResult.Ok);

        var firstVerifiedOrStatedName = data.VerifiedOrStatedNames!.First();
        var name = $"{firstVerifiedOrStatedName.First()} {firstVerifiedOrStatedName.LastOrDefault()}";
        await oneLoginService.EnqueueRecordNotFoundEmailAsync(supportTask.OneLoginUser!.EmailAddress!, name, processContext);
    }

    public async Task ResolveSupportTaskAsync(NoMatchesOutcomeOptions options, ProcessContext processContext)
    {
        var supportTask = options.SupportTask;
        ThrowIfSupportTaskIsClosed(supportTask);

        var data = supportTask.GetData<OneLoginUserRecordMatchingData>();

        var updateTaskResult = await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserRecordMatchingData>
            {
                SupportTask = supportTask.SupportTaskReference,
                UpdateData = data => data with
                {
                    Outcome = OneLoginUserRecordMatchingOutcome.NoMatches
                },
                Status = SupportTaskStatus.Closed
            },
            processContext);
        Debug.Assert(updateTaskResult is UpdateSupportTaskResult.Ok);

        var firstVerifiedOrStatedName = data.VerifiedOrStatedNames!.First();
        var name = $"{firstVerifiedOrStatedName.First()} {firstVerifiedOrStatedName.LastOrDefault()}";
        await oneLoginService.EnqueueRecordNotFoundEmailAsync(supportTask.OneLoginUser!.EmailAddress!, name, processContext);
    }

    public async Task ResolveSupportTaskAsync(ConnectedOutcomeOptions options, ProcessContext processContext)
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

        var firstVerifiedOrStatedName = data.VerifiedOrStatedNames!.First();
        var name = $"{firstVerifiedOrStatedName.First()} {firstVerifiedOrStatedName.LastOrDefault()}";
        await oneLoginService.EnqueueRecordMatchedEmailAsync(supportTask.OneLoginUser!.EmailAddress!, name, processContext);

        var updateTaskResult = await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserRecordMatchingData>
            {
                SupportTask = supportTask.SupportTaskReference,
                UpdateData = data => data with
                {
                    PersonId = options.MatchedPersonId,
                    Outcome = OneLoginUserRecordMatchingOutcome.Connected
                },
                Status = SupportTaskStatus.Closed
            },
            processContext);
        Debug.Assert(updateTaskResult is UpdateSupportTaskResult.Ok);
    }
}
