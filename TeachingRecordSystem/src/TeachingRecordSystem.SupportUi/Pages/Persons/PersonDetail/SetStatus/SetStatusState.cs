using TeachingRecordSystem.Core.Services.Persons;
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

    public ProvideMoreInformationOption? ProvideMoreInformation { get; set; }

    public bool Initialized { get; set; }

    public bool IsComplete =>
        Evidence.IsComplete &&
        (
            (
                DeactivateReason is not null &&
                (
                    ProvideMoreInformation != ProvideMoreInformationOption.Yes ||
                    DeactivateReasonDetail is not null
                )
            ) ||
            (
                ReactivateReason is not null &&
                (
                    ProvideMoreInformation != ProvideMoreInformationOption.Yes ||
                    ReactivateReasonDetail is not null
                )
            )
        );

    public void EnsureInitialized()
    {
        if (Initialized)
        {
            return;
        }

        Initialized = true;
    }
}
