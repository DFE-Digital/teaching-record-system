using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.DeleteAlert;

public class DeleteAlertState
{
    public DeleteAlertReasonOption? DeleteReason { get; set; }

    public bool? ProvideAdditionalInformation { get; set; }

    public string? DeleteReasonDetail { get; set; }

    public string? AdditionalInformation { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();
}
