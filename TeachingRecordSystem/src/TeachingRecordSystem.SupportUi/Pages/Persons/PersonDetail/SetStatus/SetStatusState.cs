using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

public class SetStatusState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.SetStatus,
        typeof(SetStatusState),
        requestDataKeys: ["personId"],
        appendUniqueKey: true);

    public DeactivateReasonOption? DeactivateReason { get; set; }
    public string? DeactivateReasonDetail { get; set; }
    public ReactivateReasonOption? ReactivateReason { get; set; }
    public string? ReactivateReasonDetail { get; set; }
    public EvidenceUploadModel Evidence { get; set; } = new();

    public bool Initialized { get; set; }

    public bool IsComplete => (DeactivateReason is not null || ReactivateReason is not null) &&
            (DeactivateReason != DeactivateReasonOption.AnotherReason || DeactivateReasonDetail is not null) &&
            (ReactivateReason != ReactivateReasonOption.AnotherReason || ReactivateReasonDetail is not null) &&
            Evidence.IsComplete;

    public void EnsureInitialized()
    {
        if (Initialized)
        {
            return;
        }

        Initialized = true;
    }
}
