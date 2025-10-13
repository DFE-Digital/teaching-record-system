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

    public bool? HasAdditionalReasonDetail { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();

    public bool IsComplete =>
        !string.IsNullOrWhiteSpace(Details) &&
        ChangeReason.HasValue &&
        HasAdditionalReasonDetail is bool hasDetail &&
        (!hasDetail || ChangeReasonDetail is not null) &&
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
