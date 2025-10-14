using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.SetStatus;

public class SetStatusStateBuilder
{
    public DeactivateReasonOption? DeactivateReason { get; set; }
    public string? DeactivateReasonDetail { get; set; }
    public ReactivateReasonOption? ReactivateReason { get; set; }
    public string? ReactivateReasonDetail { get; set; }
    public EvidenceUploadModel Evidence { get; set; } = new();

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

    public SetStatusStateBuilder WithUploadEvidenceChoice(bool uploadEvidence, Guid? evidenceFileId = null, string? evidenceFileName = null, string? evidenceFileSizeDescription = null)
    {
        Evidence.UploadEvidence = uploadEvidence;
        if (uploadEvidence)
        {
            Evidence.UploadedEvidenceFile = new()
            {
                FileId = evidenceFileId ?? Guid.NewGuid(),
                FileName = evidenceFileName ?? "evidence.jpeg",
                FileSizeDescription = evidenceFileSizeDescription ?? "5MB"
            };
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
            Evidence = Evidence,
            Initialized = Initialized
        };
    }
}
