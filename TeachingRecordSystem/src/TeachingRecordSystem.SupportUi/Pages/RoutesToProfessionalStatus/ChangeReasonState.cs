namespace TeachingRecordSystem.SupportUi.Pages.Routes;

public class ChangeReasonState
{
    public ChangeReasonOption? ChangeReason;
    public string? ChangeReasonDetail { get; set; }

    public bool? UploadEvidence { get; set; }

    public Guid? EvidenceFileId { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }
}
