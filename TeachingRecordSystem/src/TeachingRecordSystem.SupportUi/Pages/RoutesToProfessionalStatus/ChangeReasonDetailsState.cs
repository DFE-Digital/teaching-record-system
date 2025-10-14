using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;

public class ChangeReasonDetailsState
{
    public bool? HasAdditionalReasonDetail { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();

    public bool IsComplete =>
        ChangeReasonDetail != null ||
        HasAdditionalReasonDetail != null ||
        Evidence.IsComplete;
}
