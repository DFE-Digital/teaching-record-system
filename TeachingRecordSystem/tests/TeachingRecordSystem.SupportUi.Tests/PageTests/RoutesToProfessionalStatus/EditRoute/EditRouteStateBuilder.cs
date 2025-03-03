using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.EditRoute;

public class EditRouteStateBuilder
{
    private QualificationType? _qualificationType;
    private Guid? _routeToProfessionalStatusId;
    private ProfessionalStatusStatus? _status;
    private DateOnly? _awardedDate;
    private DateOnly? _trainingStartDate;
    private DateOnly? _trainingEndDate;
    private Guid[]? _trainingSubjectIds;
    private TrainingAgeSpecialismType? _trainingAgeSpecialismType;
    private int? _trainingAgeSpecialismRangeFrom;
    private int? _trainingAgeSpecialismRangeTo;
    private string? _trainingCountryId;
    private Guid? _trainingProviderId;
    private Guid? _inductionExemptionReasonId;
    private ChangeReasonOption? _changeReasonOption;
    private ChangeReasonDetailsState _changeReasonDetail = new();


    public async Task<EditRouteStateBuilder> WithPopulatedReferenceFieldsAsync(ReferenceDataCache referenceDataCache)
    {
        _routeToProfessionalStatusId = (await referenceDataCache.GetRoutesToProfessionalStatusAsync()).RandomOne().RouteToProfessionalStatusId;
        _trainingCountryId = (await referenceDataCache.GetTrainingCountriesAsync()).RandomOne().CountryId;
        _trainingProviderId = (await referenceDataCache.GetTrainingProvidersAsync()).RandomOne().TrainingProviderId;
        _trainingSubjectIds = (await referenceDataCache.GetTrainingSubjectsAsync()).RandomSelection(1).Select(x => x.TrainingSubjectId).ToArray();
        _trainingAgeSpecialismType = TrainingAgeSpecialismType.KeyStage1;
        return this;
    }
    public EditRouteStateBuilder WithAwardedStatusFields(IClock clock)
    {
        _trainingStartDate = clock.Today.AddYears(-1);
        _trainingEndDate = clock.Today.AddDays(-1);
        _awardedDate = clock.Today;
        _status = ProfessionalStatusStatus.Awarded;
        return this;
    }

    public EditRouteStateBuilder WithQualificationType(QualificationType qualificationType)
    {
        _qualificationType = qualificationType;
        return this;
    }

    public EditRouteStateBuilder WithRouteToProfessionalStatusId(Guid routeToProfessionalStatusId)
    {
        _routeToProfessionalStatusId = routeToProfessionalStatusId;
        return this;
    }

    public EditRouteStateBuilder WithStatus(ProfessionalStatusStatus status)
    {
        _status = status;
        return this;
    }

    public EditRouteStateBuilder WithAwardedDate(DateOnly awardedDate)
    {
        _awardedDate = awardedDate;
        return this;
    }

    public EditRouteStateBuilder WithTrainingStartDate(DateOnly trainingStartDate)
    {
        _trainingStartDate = trainingStartDate;
        return this;
    }

    public EditRouteStateBuilder WithTrainingEndDate(DateOnly trainingEndDate)
    {
        _trainingEndDate = trainingEndDate;
        return this;
    }

    public EditRouteStateBuilder WithTrainingSubjectIds(Guid[] trainingSubjectIds)
    {
        _trainingSubjectIds = trainingSubjectIds;
        return this;
    }

    public EditRouteStateBuilder WithTrainingAgeSpecialismType(TrainingAgeSpecialismType trainingAgeSpecialismType)
    {
        _trainingAgeSpecialismType = trainingAgeSpecialismType;
        return this;
    }

    public EditRouteStateBuilder WithTrainingAgeSpecialismRangeFrom(int trainingAgeSpecialismRangeFrom)
    {
        _trainingAgeSpecialismRangeFrom = trainingAgeSpecialismRangeFrom;
        return this;
    }

    public EditRouteStateBuilder WithTrainingAgeSpecialismRangeTo(int trainingAgeSpecialismRangeTo)
    {
        _trainingAgeSpecialismRangeTo = trainingAgeSpecialismRangeTo;
        return this;
    }

    public EditRouteStateBuilder WithTrainingCountryId(string trainingCountryId)
    {
        _trainingCountryId = trainingCountryId;
        return this;
    }

    public EditRouteStateBuilder WithTrainingProviderId(Guid trainingProviderId)
    {
        _trainingProviderId = trainingProviderId;
        return this;
    }

    public EditRouteStateBuilder WithInductionExemptionReasonId(Guid inductionExemptionReasonId)
    {
        _inductionExemptionReasonId = inductionExemptionReasonId;
        return this;
    }

    public EditRouteStateBuilder WithChangeReasonOption(ChangeReasonOption reason)
    {
        _changeReasonOption = reason;
        return this;
    }

    public EditRouteStateBuilder WithValidChangeReasonOption()
    {
        _changeReasonOption = ChangeReasonOption.AnotherReason;
        return this;
    }

    public EditRouteStateBuilder WithChangeReasonDetail(string detail, bool fileUpload)
    {
        _changeReasonDetail = new ChangeReasonStateBuilder().WithChangeReasonDetail(detail).WithFileUploadChoice(fileUpload).Build();
        return this;
    }

    public EditRouteStateBuilder WithDefaultChangeReasonNoUploadFileDetail()
    {
        _changeReasonDetail = new ChangeReasonStateBuilder().WithValidChangeReasonDetail().Build();
        return this;
    }

    public EditRouteStateBuilder WithChangeReasonDetail(Action<ChangeReasonStateBuilder> buildChangeReasonDetail)
    {
        var builder = new ChangeReasonStateBuilder();
        buildChangeReasonDetail(builder);
        _changeReasonDetail = builder.Build();
        return this;
    }

    public EditRouteState Build()
    {
        if(!_routeToProfessionalStatusId.HasValue || !_status.HasValue)
        {
            throw new InvalidOperationException("RouteToProfessionalStatusId and Status must be set");
        }
        return new EditRouteState()
        {
            Initialized = true,
            RouteToProfessionalStatusId = _routeToProfessionalStatusId!.Value,
            Status = _status!.Value,
            AwardedDate = _awardedDate,
            TrainingStartDate = _trainingStartDate,
            TrainingEndDate = _trainingEndDate,
            TrainingSubjectIds = _trainingSubjectIds,
            TrainingAgeSpecialismType = _trainingAgeSpecialismType,
            TrainingAgeSpecialismRangeFrom = _trainingAgeSpecialismRangeFrom,
            TrainingAgeSpecialismRangeTo = _trainingAgeSpecialismRangeTo,
            TrainingCountryId = _trainingCountryId,
            TrainingProviderId = _trainingProviderId,
            InductionExemptionReasonId = _inductionExemptionReasonId,
            ChangeReason = _changeReasonOption,
            QualificationType = _qualificationType,
            ChangeReasonDetail = new ChangeReasonDetailsState()
            {
                ChangeReasonDetail = _changeReasonDetail.ChangeReasonDetail,
                EvidenceFileId = _changeReasonDetail.EvidenceFileId,
                EvidenceFileName = _changeReasonDetail.EvidenceFileName,
                EvidenceFileSizeDescription = _changeReasonDetail.EvidenceFileSizeDescription,
                HasAdditionalReasonDetail = _changeReasonDetail.HasAdditionalReasonDetail,
                UploadEvidence = _changeReasonDetail.UploadEvidence
            }
        };
    }
}
