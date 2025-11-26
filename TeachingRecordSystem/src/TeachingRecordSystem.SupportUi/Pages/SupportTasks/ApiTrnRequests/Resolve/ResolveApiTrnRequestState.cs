using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.PersonMatching;
using TeachingRecordSystem.SupportUi.Services;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;

public class ResolveApiTrnRequestState : IRegisterJourney
{
    public static Guid CreateNewRecordPersonIdSentinel => Guid.Empty;

    public static JourneyDescriptor Journey { get; } = new(
        JourneyNames.ResolveApiTrnRequest,
        typeof(ResolveApiTrnRequestState),
        ["supportTaskReference"],
        appendUniqueKey: true);

    public required IReadOnlyCollection<Guid> MatchedPersonIds { get; init; }
    public TrnRequestMatchResultOutcome MatchOutcome { get; set; }
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

public class ResolveApiTrnRequestStateFactory(IPersonMatchingService personMatchingService) : IJourneyStateFactory<ResolveApiTrnRequestState>
{
    public Task<ResolveApiTrnRequestState> CreateAsync(CreateJourneyStateContext context)
    {
        var supportTask = context.HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        return CreateAsync(supportTask);
    }

    public async Task<ResolveApiTrnRequestState> CreateAsync(SupportTask supportTask)
    {
        Debug.Assert(supportTask.SupportTaskType is SupportTaskType.ApiTrnRequest);
        var requestData = supportTask.TrnRequestMetadata!;

        var matchResult = await personMatchingService.MatchFromTrnRequestAsync(requestData);

        var state = new ResolveApiTrnRequestState
        {
            MatchOutcome = matchResult.Outcome,
            MatchedPersonIds = matchResult.Outcome switch
            {
                TrnRequestMatchResultOutcome.DefiniteMatch => [matchResult.PersonId],
                TrnRequestMatchResultOutcome.PotentialMatches => matchResult.PotentialMatchesPersonIds,
                _ => []
            }
        };

        return state;
    }
}
