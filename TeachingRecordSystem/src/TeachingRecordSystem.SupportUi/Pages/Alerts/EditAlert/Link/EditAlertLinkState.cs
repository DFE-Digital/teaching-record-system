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

    public bool? UploadEvidence { get; set; }

    public Guid? EvidenceFileId { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    public bool IsComplete =>
        AddLink.HasValue &&
        (!AddLink.Value || (AddLink.Value && !string.IsNullOrWhiteSpace(Link))) &&
        ChangeReason.HasValue &&
        HasAdditionalReasonDetail.HasValue &&
        (!HasAdditionalReasonDetail.Value || (HasAdditionalReasonDetail.Value && !string.IsNullOrWhiteSpace(ChangeReasonDetail))) &&
        UploadEvidence.HasValue &&
        (!UploadEvidence.Value || (UploadEvidence.Value && EvidenceFileId.HasValue));

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
