using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.ReopenAlert;

public class ReopenAlertState
{
    public ReopenAlertReasonOption? ChangeReason { get; set; }

    public bool? HasAdditionalReasonDetail { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();
}
