using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;

public class DeleteMqState
{
    public MqDeletionReasonOption? DeletionReason { get; set; }

    public string? DeletionReasonDetail { get; set; }

    public string? AdditionalInformation { get; set; }

    public bool? ProvideAdditionalInformation { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();
}
