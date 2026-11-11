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
        string completionUrl,
        CancellationToken cancellationToken = default)
    {
        Debug.Assert(supportTask.SupportTaskType is SupportTaskType.TeacherPensionsPotentialDuplicate);
        var requestData = supportTask.TrnRequestMetadata!;

        var matchResult = await trnRequestService.MatchPersonsAsync(requestData, excludePersonIds: [supportTask.PersonId!.Value], cancellationToken);
        var matchedPersons = matchResult switch
        {
            MatchPersonsResult.DefiniteMatch definiteMatch => [new MatchPersonsResultPerson(definiteMatch.PersonId, definiteMatch.MatchedAttributes)],
            MatchPersonsResult.PotentialMatches potentialMatches => potentialMatches.Matches.ToArray(),
            MatchPersonsResult.NoMatches => []
        };

        return supportTask.ResolveJourneySavedState?.GetState<ResolveTeacherPensionsPotentialDuplicateState>() is { } existingState ?
            existingState with
            {
                MatchedPersons = matchedPersons,
                CompletionUrl = completionUrl,
                SavedJourneyState = supportTask.ResolveJourneySavedState
            } :
            new ResolveTeacherPensionsPotentialDuplicateState
            {
                MatchedPersons = matchedPersons,
                CompletionUrl = completionUrl
            };
    }
}
