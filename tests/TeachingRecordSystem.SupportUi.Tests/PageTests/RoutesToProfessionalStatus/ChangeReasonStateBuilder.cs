using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus;

public class ChangeReasonStateBuilder
{
    private string? _changeReasonDetail;
    private readonly EvidenceUploadModel _evidence = new();
    private string? _additionalInormation;

    private ProvideMoreInformationOption? _provideAdditionalInformation;

    public ChangeReasonStateBuilder WithChangeReasonDetail(string detail)
    {
        _changeReasonDetail = detail;
        return this;
    }

    public ChangeReasonStateBuilder WithAdditionalInformation(ProvideMoreInformationOption provideAdditionalInformation, string? additionalInformation)
    {
        _provideAdditionalInformation = provideAdditionalInformation;
        _additionalInormation = additionalInformation;
        return this;
    }

    public ChangeReasonStateBuilder WithValidChangeReasonDetail()
    {
        _provideAdditionalInformation = ProvideMoreInformationOption.Yes;
        _changeReasonDetail = "Some free text reason detail";
        _evidence.UploadEvidence = false;
        _additionalInormation = "some additional information";
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
            ProvideAdditionalInformation = _provideAdditionalInformation,
            Evidence = _evidence,
            AdditionalInformation = _additionalInormation,
        };
    }
}
