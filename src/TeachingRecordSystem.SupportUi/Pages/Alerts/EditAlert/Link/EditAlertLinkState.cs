using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Link;

public class EditAlertLinkState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.EditAlertLink,
        typeof(EditAlertLinkState),
        requestDataKeys: ["alertId"],
        appendUniqueKey: true);

    public bool Initialized { get; set; }

    public string? CurrentLink { get; set; }

    public bool? AddLink { get; set; }

    public string? Link { get; set; }

    public AlertChangeLinkReasonOption? ChangeReason { get; set; }

    public bool? HasAdditionalReasonDetail { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();

    public bool IsComplete =>
        AddLink is bool addLink &&
        (!addLink || Link is not null) &&
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

        Link = CurrentLink = alertInfo.Alert.ExternalLink;
        Initialized = true;
    }
}
