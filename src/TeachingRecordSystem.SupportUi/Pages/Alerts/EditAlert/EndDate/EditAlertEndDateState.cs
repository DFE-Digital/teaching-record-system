using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.EndDate;

public class EditAlertEndDateState
{
    public DateOnly? CurrentEndDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public AlertChangeEndDateReasonOption? ChangeReason { get; set; }

    public bool? HasAdditionalReasonDetail { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();
}
