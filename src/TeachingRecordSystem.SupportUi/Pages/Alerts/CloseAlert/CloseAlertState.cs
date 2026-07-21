using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.CloseAlert;

public class CloseAlertState
{
    public DateOnly? EndDate { get; set; }

    public CloseAlertReasonOption? ChangeReason { get; set; }

    public bool? HasAdditionalReasonDetail { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();
}
