namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;

public class ChangeReasonDetailsState
{
    public bool? HasAdditionalReasonDetail { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public bool? UploadEvidence { get; set; }

    public Guid? EvidenceFileId { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    public bool IsComplete => UploadEvidence == false || UploadEvidence == true && !string.IsNullOrWhiteSpace(EvidenceFileName);
}
