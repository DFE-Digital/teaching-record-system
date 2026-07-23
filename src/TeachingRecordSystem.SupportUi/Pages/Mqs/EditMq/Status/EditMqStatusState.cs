using System.Text.Json.Serialization;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

public class EditMqStatusState
{
    public MandatoryQualificationStatus? CurrentStatus { get; set; }

    public MandatoryQualificationStatus? Status { get; set; }

    public DateOnly? CurrentEndDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public MqChangeStatusReasonOption? StatusChangeReason { get; set; }

    public MqChangeEndDateReasonOption? EndDateChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();

    [JsonIgnore]
    public bool IsEndDateChange => EndDate != CurrentEndDate;

    [JsonIgnore]
    public bool IsStatusChange => Status != CurrentStatus;

    public string? AdditionalInformation { get; set; }
}
