using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;

public class EditAlertDetailsState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.EditAlertDetails,
        typeof(EditAlertDetailsState),
        requestDataKeys: ["alertId"],
        appendUniqueKey: true);

    public bool Initialized { get; set; }

    public string? CurrentDetails { get; set; }

    public string? Details { get; set; }

    public AlertChangeDetailsReasonOption? ChangeReason { get; set; }

    public bool? ProvideAdditionalInformation { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public string? AdditionalInformation { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();

    public bool IsComplete =>
        !string.IsNullOrWhiteSpace(Details) &&
        ChangeReason.HasValue &&
        (ChangeReason.HasValue && ChangeReason == AlertChangeDetailsReasonOption.AnotherReason && !string.IsNullOrEmpty(ChangeReasonDetail) ||
         ChangeReason != AlertChangeDetailsReasonOption.AnotherReason && string.IsNullOrEmpty(ChangeReasonDetail)) &&
        (ProvideAdditionalInformation == true && !string.IsNullOrEmpty(AdditionalInformation) ||
         ProvideAdditionalInformation == false && string.IsNullOrEmpty(AdditionalInformation)) &&
        Evidence.IsComplete;

    public void EnsureInitialized(CurrentAlertFeature alertInfo)
    {
        if (Initialized)
        {
            return;
        }

        Details = CurrentDetails = alertInfo.Alert.Details;
        Initialized = true;
    }
}
