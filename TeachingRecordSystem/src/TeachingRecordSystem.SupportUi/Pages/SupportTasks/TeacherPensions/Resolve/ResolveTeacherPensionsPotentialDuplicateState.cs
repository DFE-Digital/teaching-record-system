using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.PersonMatching;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

public class ResolveTeacherPensionsPotentialDuplicateState : IRegisterJourney
{
    public static Guid CreateNewRecordPersonIdSentinel => Guid.Empty;

    public static JourneyDescriptor Journey { get; } = new(
        JourneyNames.ResolveTpsPotentialDuplicate,
        typeof(ResolveTeacherPensionsPotentialDuplicateState),
        ["supportTaskReference"],
        appendUniqueKey: true);

    public required IReadOnlyCollection<Guid> MatchedPersonIds { get; init; }
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

    public enum PersonAttributeSource
    {
        ExistingRecord = 0,
        TrnRequest = 1
    }
}

public class ResolveTeacherPensionsPotentialDuplicateStateFactory(IPersonMatchingService personMatchingService) : IJourneyStateFactory<ResolveTeacherPensionsPotentialDuplicateState>
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

        var matchResult = await personMatchingService.MatchFromTrnRequestAsync(requestData);

        var state = new ResolveTeacherPensionsPotentialDuplicateState
        {
            MatchedPersonIds = matchResult.Outcome switch
            {
                TrnRequestMatchResultOutcome.DefiniteMatch => [matchResult.PersonId],
                TrnRequestMatchResultOutcome.PotentialMatches => matchResult.PotentialMatchesPersonIds.Where(x => x != supportTask.PersonId).ToArray(),
                _ => []
            }
        };

        return state;
    }
}
