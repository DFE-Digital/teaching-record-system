using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests.Resolve;

[JourneyCoordinator(JourneyNames.ResolveTrnRequest, routeValueKeys: ["supportTaskReference"])]
public class ResolveTrnRequestJourneyCoordinator(TrnRequestService trnRequestService) :
    JourneyCoordinator<ResolveTrnRequestState>
{
    public override Task<ResolveTrnRequestState> GetStartingStateAsync()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        return CreateStateAsync(trnRequestService, supportTask);
    }

    public static async Task<ResolveTrnRequestState> CreateStateAsync(
        TrnRequestService trnRequestService,
        SupportTask supportTask)
    {
        Debug.Assert(supportTask.SupportTaskType is SupportTaskType.TrnRequest);
        var requestData = supportTask.TrnRequestMetadata!;

        var matchResult = await trnRequestService.MatchPersonsAsync(requestData);

        return new ResolveTrnRequestState
        {
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
