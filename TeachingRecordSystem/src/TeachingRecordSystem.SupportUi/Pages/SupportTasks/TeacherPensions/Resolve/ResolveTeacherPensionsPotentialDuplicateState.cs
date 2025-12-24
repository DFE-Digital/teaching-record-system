using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;
using TeachingRecordSystem.SupportUi.Services;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

public class ResolveTeacherPensionsPotentialDuplicateState : IRegisterJourney
{
    public static Guid CreateNewRecordPersonIdSentinel => Guid.Empty;

    public static JourneyDescriptor Journey { get; } = new(
        JourneyNames.ResolveTpsPotentialDuplicate,
        typeof(ResolveTeacherPensionsPotentialDuplicateState),
        ["supportTaskReference"],
        appendUniqueKey: true);

    public required IReadOnlyCollection<MatchPersonResult> MatchedPersons { get; init; }
    public Guid? PersonId { get; set; }
    public bool PersonAttributeSourcesSet { get; set; }
    public PersonAttributeSource? FirstNameSource { get; set; }
    public PersonAttributeSource? MiddleNameSource { get; set; }
    public PersonAttributeSource? LastNameSource { get; set; }
    public PersonAttributeSource? DateOfBirthSource { get; set; }
    public PersonAttributeSource? NationalInsuranceNumberSource { get; set; }
    public PersonAttributeSource? GenderSource { get; set; }
    public string? Reason { get; set; }
    public string? MergeComments { get; set; }
    public PersonAttributeSource? TRNSource { get; set; }
    public KeepingRecordSeparateReason? KeepSeparateReason { get; set; }
    public Guid? TeachersPensionPersonId { get; set; }
    public EvidenceUploadModel Evidence { get; set; } = new();
}

public class ResolveTeacherPensionsPotentialDuplicateStateFactory(TrnRequestService trnRequestService) : IJourneyStateFactory<ResolveTeacherPensionsPotentialDuplicateState>
{
    public Task<ResolveTeacherPensionsPotentialDuplicateState> CreateAsync(CreateJourneyStateContext context)
    {
        var supportTask = context.HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        return CreateAsync(supportTask);
    }

    public async Task<ResolveTeacherPensionsPotentialDuplicateState> CreateAsync(SupportTask supportTask)
    {
        Debug.Assert(supportTask.SupportTaskType is SupportTaskType.TeacherPensionsPotentialDuplicate);
        var requestData = supportTask.TrnRequestMetadata!;

        var matchResult = await trnRequestService.MatchPersonsAsync(requestData, excludePersonIds: supportTask.PersonId!.Value);

        var state = new ResolveTeacherPensionsPotentialDuplicateState
        {
            MatchedPersons = matchResult.Outcome switch
            {
                MatchPersonsResultOutcome.DefiniteMatch => [new MatchPersonResult(matchResult.PersonId, matchResult.MatchedAttributes)],
                MatchPersonsResultOutcome.PotentialMatches => matchResult.Matches.ToArray(),
                _ => []
            }
        };

        return state;
    }
}
