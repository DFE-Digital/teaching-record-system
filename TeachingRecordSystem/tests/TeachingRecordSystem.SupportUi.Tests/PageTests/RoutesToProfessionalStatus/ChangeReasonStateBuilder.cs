using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus;

public class ChangeReasonStateBuilder
{
    //private ChangeReasonOption? _changeReason;
    private string? _changeReasonDetail;
    private bool? _uploadEvidence;
    private Guid? _evidenceFileId;
    private string? _evidenceFileName;
    private string? _evidenceFileSizeDescription;

    public ChangeReasonStateBuilder WithValidChangeReasonDetail()
    {
        _changeReasonDetail = "Some free text reason detail";
        _uploadEvidence = false;
        return this;
    }

    public ChangeReasonDetailsState Build()
    {
        return new ChangeReasonDetailsState {
            ChangeReasonDetail = _changeReasonDetail,
            UploadEvidence = _uploadEvidence,
            EvidenceFileId = _evidenceFileId,
            EvidenceFileName = _evidenceFileName,
            EvidenceFileSizeDescription = _evidenceFileSizeDescription
        };
    }
}
