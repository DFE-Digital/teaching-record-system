using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Link;

public class EditAlertLinkState
{
    public string? CurrentLink { get; set; }

    public bool? AddLink { get; set; }

    public string? Link { get; set; }

    public AlertChangeLinkReasonOption? ChangeReason { get; set; }

    public bool? HasAdditionalReasonDetail { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();
}
