using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Pages.Routes;
using TeachingRecordSystem.SupportUi.Pages.Routes.EditRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus;

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
    private ChangeReasonState? _changeReasonDetail;

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

    public EditRouteStateBuilder WithDefaultChangeReasonNoUploadFileDetail()
    {
        _changeReasonDetail = new ChangeReasonStateBuilder().WithValidChangeReason().Build();
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
        return new EditRouteState(new Mock<IFileService>().Object)
        {
            QualificationType = _qualificationType ?? default,
            RouteToProfessionalStatusId = _routeToProfessionalStatusId ?? default,
            Status = _status ?? default,
            AwardedDate = _awardedDate ?? default,
            TrainingStartDate = _trainingStartDate ?? default,
            TrainingEndDate = _trainingEndDate ?? default,
            TrainingSubjectIds = _trainingSubjectIds ?? default,
            TrainingAgeSpecialismType = _trainingAgeSpecialismType ?? default,
            TrainingAgeSpecialismRangeFrom = _trainingAgeSpecialismRangeFrom ?? default,
            TrainingAgeSpecialismRangeTo = _trainingAgeSpecialismRangeTo ?? default,
            TrainingCountryId = _trainingCountryId ?? default,
            TrainingProviderId = _trainingProviderId ?? default,
            InductionExemptionReasonId = _inductionExemptionReasonId ?? default,
            ChangeReasonDetail = _changeReasonDetail ?? null
        };
    }
}
