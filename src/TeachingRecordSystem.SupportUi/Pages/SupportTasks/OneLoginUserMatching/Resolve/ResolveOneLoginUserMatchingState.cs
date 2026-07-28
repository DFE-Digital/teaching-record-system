using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

public record ResolveOneLoginUserMatchingState : IJourneyWithSavedState
{
    public static Guid NotMatchedPersonIdSentinel => Guid.Empty;

    public required IReadOnlyCollection<MatchPersonResult> MatchedPersons { get; init; }
    public required string CompletionUrl { get; init; }

    public SavedJourneyState? SavedJourneyState { get; set; }
    public bool? Verified { get; set; }
    public Guid? MatchedPersonId { get; set; }
    public OneLoginIdVerificationRejectReason? RejectReason { get; set; }
    public string? RejectionAdditionalDetails { get; set; }
    public OneLoginUserNotConnectingReason? NotConnectingReason { get; set; }
    public string? NotConnectingAdditionalDetails { get; set; }
}
