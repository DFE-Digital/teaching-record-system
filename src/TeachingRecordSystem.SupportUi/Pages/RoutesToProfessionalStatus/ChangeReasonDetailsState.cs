using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;

public class ChangeReasonDetailsState
{
    public ProvideMoreInformationOption? ProvideAdditionalInformation { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public string? AdditionalInformation { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();

    public bool IsComplete =>

        (ProvideAdditionalInformation == ProvideMoreInformationOption.No && AdditionalInformation is null) ||
        (ProvideAdditionalInformation == ProvideMoreInformationOption.Yes && AdditionalInformation is not null) ||
        Evidence.IsComplete;
}
