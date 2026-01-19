using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

public record ResolveOneLoginUserMatchingState : IRegisterJourney, IJourneyWithSavedState
{
    public static Guid NotMatchedPersonIdSentinel => Guid.Empty;

    public static JourneyDescriptor Journey { get; } = new(
        JourneyNames.ResolveOneLoginUserMatching,
        typeof(ResolveOneLoginUserMatchingState),
        ["supportTaskReference"],
        appendUniqueKey: true);

    public SavedJourneyState? SavedJourneyState { get; set; }

    public required IReadOnlyCollection<MatchPersonResult> MatchedPersons { get; set; }

    public bool? Verified { get; set; }

    public Guid? MatchedPersonId { get; set; }

    public OneLoginIdVerificationRejectReason? RejectReason { get; set; }

    public string? RejectionAdditionalDetails { get; set; }

    public OneLoginUserNotConnectingReason? NotConnectingReason { get; set; }

    public string? NotConnectingAdditionalDetails { get; set; }
}

[UsedImplicitly]
public class ResolveOneLoginUserMatchingStateFactory(OneLoginService oneLoginService) :
    IJourneyStateFactory<ResolveOneLoginUserMatchingState>
{
    public Task<ResolveOneLoginUserMatchingState> CreateAsync(CreateJourneyStateContext context)
    {
        var supportTask = context.HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        return CreateAsync(supportTask);
    }

    public async Task<ResolveOneLoginUserMatchingState> CreateAsync(SupportTask supportTask)
    {
        Debug.Assert(supportTask.SupportTaskType is SupportTaskType.OneLoginUserIdVerification or SupportTaskType.OneLoginUserRecordMatching);
        var requestData = supportTask.GetData<IOneLoginUserMatchingData>();

        var suggestedMatches = await oneLoginService.GetSuggestedPersonMatchesAsync(new(
            Names: requestData!.VerifiedOrStatedNames!,
            DatesOfBirth: requestData.VerifiedOrStatedDatesOfBirth!,
            NationalInsuranceNumber: requestData.StatedNationalInsuranceNumber,
            Trn: requestData.StatedTrn,
            TrnTokenTrnHint: requestData.TrnTokenTrn));

        return supportTask.ResolveJourneySavedState?.GetState<ResolveOneLoginUserMatchingState>() is { } existingState ?
            existingState with { MatchedPersons = suggestedMatches, SavedJourneyState = supportTask.ResolveJourneySavedState } :
            new ResolveOneLoginUserMatchingState { MatchedPersons = suggestedMatches, Verified = requestData.Verified };
    }
}
