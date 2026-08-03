using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

public class SetStatusState
{
    public PersonDeactivateReason? DeactivateReason { get; set; }
    public string? DeactivateReasonDetail { get; set; }
    public string? DeactivateAdditionalInformation { get; set; }
    public string? ReactivateAdditionalInformation { get; set; }
    public PersonReactivateReason? ReactivateReason { get; set; }
    public string? ReactivateReasonDetail { get; set; }
    public EvidenceUploadModel Evidence { get; set; } = new();
    public ProvideMoreInformationOption? ProvideMoreInformation { get; set; }
}
