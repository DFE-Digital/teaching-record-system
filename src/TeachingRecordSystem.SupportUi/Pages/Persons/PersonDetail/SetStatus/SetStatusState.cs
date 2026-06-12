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

    public PersonDeactivateReason? DeactivateReason { get; set; }
    public string? DeactivateReasonDetail { get; set; }
    public string? DeactivateAdditionalInformation { get; set; }
    public string? ReactivateAdditionalInformation { get; set; }

    public PersonReactivateReason? ReactivateReason { get; set; }
    public string? ReactivateReasonDetail { get; set; }
    public EvidenceUploadModel Evidence { get; set; } = new();

    public ProvideMoreInformationOption? ProvideMoreInformation { get; set; }

    public bool Initialized { get; set; }

    public bool IsComplete
    {
        get
        {
            var isDeactivate = DeactivateReason is not null;
            var isReactivate = ReactivateReason is not null;

            var reasonSelected = isDeactivate || isReactivate;

            var reasonDetailComplete =
                isDeactivate
                    ? DeactivateReason != PersonDeactivateReason.AnotherReason ||
                      !string.IsNullOrWhiteSpace(DeactivateReasonDetail)
                    : isReactivate && (ReactivateReason != PersonReactivateReason.AnotherReason ||
                                       !string.IsNullOrWhiteSpace(ReactivateReasonDetail));

            var additionalInformationComplete =
                ProvideMoreInformation != ProvideMoreInformationOption.Yes ||
                (
                    isDeactivate
                        ? !string.IsNullOrWhiteSpace(DeactivateAdditionalInformation)
                        : !string.IsNullOrWhiteSpace(ReactivateAdditionalInformation)
                );

            return Evidence.IsComplete
                   && reasonSelected
                   && reasonDetailComplete
                   && additionalInformationComplete;
        }
    }

    public void EnsureInitialized()
    {
        if (Initialized)
        {
            return;
        }

        Initialized = true;
    }
}
