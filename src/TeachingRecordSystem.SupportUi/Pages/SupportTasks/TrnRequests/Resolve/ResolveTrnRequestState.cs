using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi.Services;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests.Resolve;

public class ResolveTrnRequestState : IRegisterJourney
{
    public static Guid CreateNewRecordPersonIdSentinel => Guid.Empty;

    public static JourneyDescriptor Journey { get; } = new(
        JourneyNames.ResolveTrnRequest,
        typeof(ResolveTrnRequestState),
        ["supportTaskReference"],
        appendUniqueKey: true);

    public required IReadOnlyCollection<MatchPersonsResultPerson> MatchedPersons { get; init; } = [];
    public MatchPersonsResultOutcome MatchOutcome { get; set; }
    public Guid? PersonId { get; set; }
    public bool PersonAttributeSourcesSet { get; set; }
    public PersonAttributeSource? FirstNameSource { get; set; }
    public PersonAttributeSource? MiddleNameSource { get; set; }
    public PersonAttributeSource? LastNameSource { get; set; }
    public PersonAttributeSource? DateOfBirthSource { get; set; }
    public PersonAttributeSource? EmailAddressSource { get; set; }
    public PersonAttributeSource? NationalInsuranceNumberSource { get; set; }
    public PersonAttributeSource? GenderSource { get; set; }
    public string? Comments { get; set; }
}

public class ResolveApiTrnRequestStateFactory(TrnRequestService trnRequestService) : IJourneyStateFactory<ResolveTrnRequestState>
{
    public Task<ResolveTrnRequestState> CreateAsync(CreateJourneyStateContext context)
    {
        var supportTask = context.HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        return CreateAsync(supportTask);
    }

    public async Task<ResolveTrnRequestState> CreateAsync(SupportTask supportTask)
    {
        Debug.Assert(supportTask.SupportTaskType is SupportTaskType.TrnRequest);
        var requestData = supportTask.TrnRequestMetadata!;

        var matchResult = await trnRequestService.MatchPersonsAsync(requestData);

        var state = new ResolveTrnRequestState
        {
            MatchOutcome = matchResult.Outcome,
            MatchedPersons = matchResult.Outcome switch
            {
                MatchPersonsResultOutcome.DefiniteMatch => [new MatchPersonsResultPerson(matchResult.PersonId, matchResult.MatchedAttributes)],
                MatchPersonsResultOutcome.PotentialMatches => matchResult.Matches.ToArray(),
                _ => []
            }
        };

        return state;
    }
}
