using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[JourneyCoordinator(JourneyNames.ResolveOneLoginUserMatching, routeValueKeys: ["supportTaskReference"])]
public class ResolveOneLoginUserMatchingJourneyCoordinator(
    OneLoginService oneLoginService,
    SupportUiLinkGenerator linkGenerator) :
    JourneyCoordinator<ResolveOneLoginUserMatchingState>
{
    public override Task<ResolveOneLoginUserMatchingState> GetStartingStateAsync()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        return CreateStateAsync(oneLoginService, supportTask);
    }

    /// <summary>
    /// Gets the URL of the journey's first question, which depends on the type of support task being
    /// resolved and whether any matching records were found.
    /// </summary>
    public string GetFirstStepUrl()
    {
        var resolveLinkGenerator = linkGenerator.SupportTasks.OneLoginUserMatching.Resolve;

        // Record matching tasks have already had the person's identity verified, so they skip
        // straight to picking a record.
        if (HttpContext.GetCurrentSupportTaskFeature().SupportTask.SupportTaskType is not SupportTaskType.OneLoginUserRecordMatching)
        {
            return resolveLinkGenerator.Verify(InstanceId);
        }

        return State.MatchedPersons.Count > 0 ?
            resolveLinkGenerator.Matches(InstanceId) :
            resolveLinkGenerator.NoMatches(InstanceId);
    }

    /// <summary>
    /// Gets the URL of the support task list page that this journey was started from.
    /// </summary>
    public string GetListPageUrl() =>
        HttpContext.GetCurrentSupportTaskFeature().SupportTask.SupportTaskType is SupportTaskType.OneLoginUserIdVerification ?
            linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification() :
            linkGenerator.SupportTasks.OneLoginUserMatching.RecordMatching();

    public static async Task<ResolveOneLoginUserMatchingState> CreateStateAsync(
        OneLoginService oneLoginService,
        SupportTask supportTask)
    {
        Debug.Assert(supportTask.SupportTaskType is SupportTaskType.OneLoginUserIdVerification or SupportTaskType.OneLoginUserRecordMatching);
        var requestData = supportTask.GetData<IOneLoginUserMatchingData>();
        var emailAddress = supportTask.OneLoginUser!.EmailAddress;

        IReadOnlyCollection<MatchPersonResult> suggestedMatches;

        if (string.IsNullOrWhiteSpace(requestData.StatedTrn))
        {
            suggestedMatches = [];
        }
        else
        {
            var matchOptions = new GetSuggestedPersonMatchesOptions(
                Names: requestData.VerifiedOrStatedNames!,
                DatesOfBirth: requestData.VerifiedOrStatedDatesOfBirth!,
                NationalInsuranceNumber: requestData.StatedNationalInsuranceNumber,
                EmailAddress: emailAddress,
                Trn: requestData.StatedTrn,
                TrnTokenTrnHint: requestData.TrnTokenTrn);

            suggestedMatches = await oneLoginService.GetSuggestedPersonMatchesAsync(matchOptions);

            var matchResult = oneLoginService.MatchPerson(matchOptions, suggestedMatches);
            if (matchResult is not null)
            {
                suggestedMatches = [matchResult];
            }
        }

        return supportTask.ResolveJourneySavedState?.GetState<ResolveOneLoginUserMatchingState>() is { } existingState ?
            existingState with
            {
                MatchedPersons = suggestedMatches,
                SavedJourneyState = supportTask.ResolveJourneySavedState
            } :
            new ResolveOneLoginUserMatchingState
            {
                MatchedPersons = suggestedMatches,
                Verified = supportTask.SupportTaskType is SupportTaskType.OneLoginUserRecordMatching ? true : null
            };
    }
}
