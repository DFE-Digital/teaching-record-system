using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus;

public class ChangeReasonStateBuilder
{
    private string? _changeReasonDetail;
    private readonly EvidenceUploadModel _evidence = new();

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
        _evidence.UploadEvidence = false;
        return this;
    }

    public ChangeReasonStateBuilder WithFileUploadChoice(bool uploadFile)
    {
        _evidence.UploadEvidence = uploadFile;
        if (uploadFile)
        {
            _evidence.UploadedEvidenceFile = new()
            {
                FileId = Guid.NewGuid(),
                FileName = "evidence.jpeg",
                FileSizeDescription = "5MB"
            };
        }
        return this;
    }

    public ChangeReasonDetailsState Build()
    {
        return new ChangeReasonDetailsState
        {
            ChangeReasonDetail = _changeReasonDetail,
            HasAdditionalReasonDetail = _hasAdditionalReasonDetail,
            Evidence = _evidence
        };
    }
}
