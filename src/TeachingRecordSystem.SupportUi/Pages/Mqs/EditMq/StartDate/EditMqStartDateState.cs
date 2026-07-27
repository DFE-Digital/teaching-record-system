using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;

public class EditMqStartDateState
{
    public DateOnly? CurrentStartDate { get; set; }

    public DateOnly? StartDate { get; set; }

    public MqChangeStartDateReasonOption? ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public string? AdditionalInformation { get; set; }

    public bool? ProvideAdditionalInformation { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();
}
