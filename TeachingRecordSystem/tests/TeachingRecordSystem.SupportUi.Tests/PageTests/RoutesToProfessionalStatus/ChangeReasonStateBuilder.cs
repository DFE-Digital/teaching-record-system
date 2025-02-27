using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus;

public class ChangeReasonStateBuilder
{
    private string? _changeReasonDetail;
    private bool? _uploadEvidence;
    private Guid? _evidenceFileId;
    private string? _evidenceFileName;
    private string? _evidenceFileSizeDescription;
    private bool? _hasAdditionalReasonDetail;

    public ChangeReasonStateBuilder WithChangeReasonDetail(string detail)
    {
        _changeReasonDetail = detail;
        return this;
    }

    public ChangeReasonStateBuilder WithValidChangeReasonDetail()
    {
        _hasAdditionalReasonDetail = true;
        _changeReasonDetail = "Some free text reason detail";
        _uploadEvidence = false;
        return this;
    }

    public ChangeReasonStateBuilder WithFileUploadChoice(bool uploadFile)
    {
        _uploadEvidence = uploadFile;
        if (uploadFile)
        {
            _evidenceFileId = Guid.NewGuid();
            _evidenceFileName = "evidence.jpeg";
            _evidenceFileSizeDescription = "5MB";
        }
        return this;
    }

    public ChangeReasonDetailsState Build()
    {
        return new ChangeReasonDetailsState
        {
            ChangeReasonDetail = _changeReasonDetail,
            HasAdditionalReasonDetail = _hasAdditionalReasonDetail,
            UploadEvidence = _uploadEvidence,
            EvidenceFileId = _evidenceFileId,
            EvidenceFileName = _evidenceFileName,
            EvidenceFileSizeDescription = _evidenceFileSizeDescription
        };
    }
}
