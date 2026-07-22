using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[JourneyCoordinator(JourneyNames.ResolveOneLoginUserMatching, routeValueKeys: ["supportTaskReference"])]
public class ResolveOneLoginUserMatchingJourneyCoordinator(OneLoginService oneLoginService) :
    JourneyCoordinator<ResolveOneLoginUserMatchingState>
{
    public override Task<ResolveOneLoginUserMatchingState> GetStartingStateAsync()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        return CreateStateAsync(oneLoginService, supportTask);
    }

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
            suggestedMatches = await oneLoginService.GetSuggestedPersonMatchesAsync(new(
                Names: requestData.VerifiedOrStatedNames!,
                DatesOfBirth: requestData.VerifiedOrStatedDatesOfBirth!,
                NationalInsuranceNumber: requestData.StatedNationalInsuranceNumber,
                EmailAddress: emailAddress,
                Trn: requestData.StatedTrn,
                TrnTokenTrnHint: requestData.TrnTokenTrn));

            var matchResult = oneLoginService.MatchPerson(suggestedMatches);
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
