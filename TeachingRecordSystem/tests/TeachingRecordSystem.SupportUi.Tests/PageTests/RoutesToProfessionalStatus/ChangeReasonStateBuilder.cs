using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Pages.Routes;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus;

public class ChangeReasonStateBuilder
{
    private ChangeReasonOption? _changeReason;
    private string? _changeReasonDetail;
    private bool? _uploadEvidence;
    private Guid? _evidenceFileId;
    private string? _evidenceFileName;
    private string? _evidenceFileSizeDescription;

    public ChangeReasonStateBuilder WithValidChangeReason()
    {
        _changeReason = ChangeReasonOption.AnotherReason;
        _uploadEvidence = false;
        return this;
    }

    public ChangeReasonState Build()
    {
        return new ChangeReasonState(new Mock<IFileService>().Object) {
            ChangeReason = _changeReason,
            ChangeReasonDetail = _changeReasonDetail,
            UploadEvidence = _uploadEvidence,
            EvidenceFileId = _evidenceFileId,
            EvidenceFileName = _evidenceFileName,
            EvidenceFileSizeDescription = _evidenceFileSizeDescription
        };
    }
}
