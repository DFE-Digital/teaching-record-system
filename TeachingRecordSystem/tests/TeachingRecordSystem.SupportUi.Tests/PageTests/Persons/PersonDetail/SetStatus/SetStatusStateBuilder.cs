using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.SetStatus;

public class SetStatusStateBuilder
{
    public DeactivateReasonOption? DeactivateReason { get; set; }
    public string? DeactivateReasonDetail { get; set; }
    public ReactivateReasonOption? ReactivateReason { get; set; }
    public string? ReactivateReasonDetail { get; set; }
    public bool? UploadEvidence { get; set; }
    public Guid? EvidenceFileId { get; set; }
    public string? EvidenceFileName { get; set; }
    public string? EvidenceFileSizeDescription { get; set; }

    private bool Initialized { get; set; }

    public SetStatusStateBuilder WithInitializedState()
    {
        Initialized = true;

        return this;
    }

    public SetStatusStateBuilder WithDeactivateReasonChoice(DeactivateReasonOption option, string? detailText = null)
    {
        DeactivateReason = option;
        DeactivateReasonDetail = detailText;
        return this;
    }

    public SetStatusStateBuilder WithReactivateReasonChoice(ReactivateReasonOption option, string? detailText = null)
    {
        ReactivateReason = option;
        ReactivateReasonDetail = detailText;
        return this;
    }

    public SetStatusStateBuilder WithUploadEvidenceChoice(bool uploadEvidence, Guid? evidenceFileId = null, string? evidenceFileName = "evidence.jpeg", string evidenceFileSizeDescription = "5MB")
    {
        UploadEvidence = uploadEvidence;
        if (evidenceFileId.HasValue)
        {
            EvidenceFileId = evidenceFileId;
            EvidenceFileName = evidenceFileName;
            EvidenceFileSizeDescription = evidenceFileSizeDescription;
        }
        return this;
    }

    public SetStatusState Build()
    {
        return new SetStatusState
        {
            DeactivateReason = DeactivateReason,
            DeactivateReasonDetail = DeactivateReasonDetail,
            ReactivateReason = ReactivateReason,
            ReactivateReasonDetail = ReactivateReasonDetail,
            UploadEvidence = UploadEvidence,
            EvidenceFileId = EvidenceFileId,
            EvidenceFileName = EvidenceFileName,
            EvidenceFileSizeDescription = EvidenceFileSizeDescription,

            Initialized = Initialized
        };
    }
}
