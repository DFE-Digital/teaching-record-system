using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

[JourneyCoordinator(JourneyNames.ResolveTpsPotentialDuplicate, routeValueKeys: ["supportTaskReference"])]
public class ResolveTeacherPensionsPotentialDuplicateJourneyCoordinator(
    TrnRequestService trnRequestService,
    SupportUiLinkGenerator linkGenerator) :
    JourneyCoordinator<ResolveTeacherPensionsPotentialDuplicateState>
{
    public override Task<ResolveTeacherPensionsPotentialDuplicateState> GetStartingStateAsync()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        var completionUrl = this.GetReturnUrlOrDefault(linkGenerator.SupportTasks.TeacherPensions.Index());

        return CreateStateAsync(trnRequestService, supportTask, completionUrl);
    }

    public static async Task<ResolveTeacherPensionsPotentialDuplicateState> CreateStateAsync(
        TrnRequestService trnRequestService,
        SupportTask supportTask,
        string completionUrl)
    {
        Debug.Assert(supportTask.SupportTaskType is SupportTaskType.TeacherPensionsPotentialDuplicate);
        var requestData = supportTask.TrnRequestMetadata!;

        var matchResult = await trnRequestService.MatchPersonsAsync(requestData, excludePersonIds: supportTask.PersonId!.Value);

        return new ResolveTeacherPensionsPotentialDuplicateState
        {
            CompletionUrl = completionUrl,
            MatchedPersons = matchResult.Outcome switch
            {
                MatchPersonsResultOutcome.DefiniteMatch => [new MatchPersonsResultPerson(matchResult.PersonId, matchResult.MatchedAttributes)],
                MatchPersonsResultOutcome.PotentialMatches => matchResult.Matches.ToArray(),
                _ => []
            }
        };
    }
}
