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
                TrnRequest = null
            },
            processContext);

        return supportTask;
    }

    public async Task ResolveRecordMatchingSupportTaskAsync(NotConnectingOutcomeOptions options, ProcessContext processContext)
    {
        var supportTask = options.SupportTask;
        ThrowIfSupportTaskIsClosed(supportTask);

        var data = supportTask.GetData<OneLoginUserRecordMatchingData>();

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
    }

    public async Task ResolveRecordMatchingSupportTaskAsync(NoMatchesOutcomeOptions options, ProcessContext processContext)
    {
        var supportTask = options.SupportTask;
        ThrowIfSupportTaskIsClosed(supportTask);

        var data = supportTask.GetData<OneLoginUserRecordMatchingData>();

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

        var firstVerifiedOrStatedName = data.VerifiedOrStatedNames!.First();
        var name = $"{firstVerifiedOrStatedName.First()} {firstVerifiedOrStatedName.LastOrDefault()}";
        await oneLoginService.EnqueueRecordNotFoundEmailAsync(supportTask.OneLoginUser!.EmailAddress!, name, processContext);
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

        var firstVerifiedOrStatedName = data.VerifiedOrStatedNames!.First();
        var name = $"{firstVerifiedOrStatedName.First()} {firstVerifiedOrStatedName.LastOrDefault()}";
        await oneLoginService.EnqueueRecordMatchedEmailAsync(supportTask.OneLoginUser!.EmailAddress!, name, processContext);

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
    }
}
