using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;

public class EditAlertDetailsState
{
    public string? CurrentDetails { get; set; }

    public string? Details { get; set; }

    public AlertChangeDetailsReasonOption? ChangeReason { get; set; }

    public bool? ProvideAdditionalInformation { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public string? AdditionalInformation { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();
}
