using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests.Resolve;

[JourneyCoordinator(JourneyNames.ResolveTrnRequest, routeValueKeys: ["supportTaskReference"])]
public class ResolveTrnRequestJourneyCoordinator(
    TrnRequestService trnRequestService,
    SupportUiLinkGenerator linkGenerator) :
    JourneyCoordinator<ResolveTrnRequestState>
{
    public override Task<ResolveTrnRequestState> GetStartingStateAsync()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        var completionUrl = this.GetReturnUrlOrDefault(linkGenerator.SupportTasks.TrnRequests.Index());

        return CreateStateAsync(trnRequestService, supportTask, completionUrl);
    }

    public static async Task<ResolveTrnRequestState> CreateStateAsync(
        TrnRequestService trnRequestService,
        SupportTask supportTask,
        string completionUrl)
    {
        Debug.Assert(supportTask.SupportTaskType is SupportTaskType.TrnRequest);
        var requestData = supportTask.TrnRequestMetadata!;

        var matchResult = await trnRequestService.MatchPersonsAsync(requestData);

        return new ResolveTrnRequestState
        {
            CompletionUrl = completionUrl,
            MatchOutcome = matchResult.Outcome,
            MatchedPersons = matchResult.Outcome switch
            {
                MatchPersonsResultOutcome.DefiniteMatch => [new MatchPersonsResultPerson(matchResult.PersonId, matchResult.MatchedAttributes)],
                MatchPersonsResultOutcome.PotentialMatches => matchResult.Matches.ToArray(),
                _ => []
            }
        };
    }
}
