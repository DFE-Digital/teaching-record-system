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

    public bool? UploadEvidence { get; set; }

    public Guid? EvidenceFileId { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    public bool IsComplete =>
        !string.IsNullOrWhiteSpace(Details) &&
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

        Details = CurrentDetails = alertInfo.Alert.Details;
        Initialized = true;
    }
}
