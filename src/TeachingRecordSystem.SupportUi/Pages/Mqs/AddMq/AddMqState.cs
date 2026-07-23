using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

public class AddMqState
{
    public Guid? ProviderId { get; set; }

    public MandatoryQualificationSpecialism? Specialism { get; set; }

    public DateOnly? StartDate { get; set; }

    public MandatoryQualificationStatus? Status { get; set; }

    public DateOnly? EndDate { get; set; }

    public AddMqReasonOption? AddReason { get; set; }

    public bool? ProvideAdditionalInformation { get; set; }

    public string? AddReasonDetail { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();

    public string? AdditionalInformation { get; set; }
}
