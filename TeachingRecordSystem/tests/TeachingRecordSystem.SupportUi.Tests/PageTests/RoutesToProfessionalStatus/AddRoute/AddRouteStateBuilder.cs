using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.AddRoute;

public class AddRouteStateBuilder()
{
    private Guid? _routeToProfessionalStatusId;
    private RouteToProfessionalStatusStatus? _status;
    private DateOnly? _awardedDate;
    private DateOnly? _trainingStartDate;
    private DateOnly? _trainingEndDate;
    private Guid[] _trainingSubjectIds = [];
    private TrainingAgeSpecialismType? _trainingAgeSpecialismType;
    private int? _trainingAgeSpecialismRangeFrom;
    private int? _trainingAgeSpecialismRangeTo;
    private string? _trainingCountryId;
    private Guid? _trainingProviderId;
    private Guid? _degreeTypeId;
    private bool? _isExemptFromInduction;

    public AddRouteStateBuilder WithStatusWhereAllFieldsApply()
    {
        _status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingAgeSpecialismTypeRequired != FieldRequirement.NotApplicable
                && s.AwardDateRequired != FieldRequirement.NotApplicable
                && s.TrainingCountryRequired != FieldRequirement.NotApplicable
                && s.DegreeTypeRequired != FieldRequirement.NotApplicable
                && s.TrainingEndDateRequired != FieldRequirement.NotApplicable
                && s.InductionExemptionRequired != FieldRequirement.NotApplicable
                && s.TrainingProviderRequired != FieldRequirement.NotApplicable
                && s.TrainingStartDateRequired != FieldRequirement.NotApplicable
                && s.TrainingSubjectsRequired != FieldRequirement.NotApplicable)
            .RandomOne()
            .Value;
        return this;
    }

    public AddRouteStateBuilder WithAwardedStatusFields(IClock clock)
    {
        _trainingStartDate = clock.Today.AddYears(-1);
        _trainingEndDate = clock.Today.AddDays(-1);
        _awardedDate = clock.Today;
        _status = RouteToProfessionalStatusStatus.Awarded;
        return this;
    }

    public AddRouteStateBuilder WithRouteToProfessionalStatusId(Guid routeToProfessionalStatusId)
    {
        _routeToProfessionalStatusId = routeToProfessionalStatusId;
        return this;
    }

    public AddRouteStateBuilder WithStatus(RouteToProfessionalStatusStatus status)
    {
        _status = status;
        return this;
    }

    public AddRouteStateBuilder WithAwardedDate(DateOnly awardedDate)
    {
        _awardedDate = awardedDate;
        return this;
    }

    public AddRouteStateBuilder WithTrainingStartDate(DateOnly trainingStartDate)
    {
        _trainingStartDate = trainingStartDate;
        return this;
    }

    public AddRouteStateBuilder WithTrainingEndDate(DateOnly trainingEndDate)
    {
        _trainingEndDate = trainingEndDate;
        return this;
    }

    public AddRouteStateBuilder WithTrainingSubjectIds(Guid[] trainingSubjectIds)
    {
        _trainingSubjectIds = trainingSubjectIds;
        return this;
    }

    public AddRouteStateBuilder WithTrainingAgeSpecialismType(TrainingAgeSpecialismType trainingAgeSpecialismType)
    {
        _trainingAgeSpecialismType = trainingAgeSpecialismType;
        return this;
    }

    public AddRouteStateBuilder WithTrainingAgeSpecialismRangeFrom(int trainingAgeSpecialismRangeFrom)
    {
        _trainingAgeSpecialismRangeFrom = trainingAgeSpecialismRangeFrom;
        return this;
    }

    public AddRouteStateBuilder WithTrainingAgeSpecialismRangeTo(int trainingAgeSpecialismRangeTo)
    {
        _trainingAgeSpecialismRangeTo = trainingAgeSpecialismRangeTo;
        return this;
    }

    public AddRouteStateBuilder WithTrainingCountryId(string trainingCountryId)
    {
        _trainingCountryId = trainingCountryId;
        return this;
    }

    public AddRouteStateBuilder WithTrainingProviderId(Guid trainingProviderId)
    {
        _trainingProviderId = trainingProviderId;
        return this;
    }

    public AddRouteStateBuilder WithDegreeTypeId(Guid degreeTypeId)
    {
        _degreeTypeId = degreeTypeId;
        return this;
    }

    public AddRouteStateBuilder WithInductionExemption(bool? isExempt)
    {
        _isExemptFromInduction = isExempt;
        return this;
    }

    public AddRouteState Build()
    {
        return new AddRouteState()
        {
            Initialized = true,
            RouteToProfessionalStatusId = _routeToProfessionalStatusId,
            Status = _status,
            AwardedDate = _awardedDate,
            TrainingStartDate = _trainingStartDate,
            TrainingEndDate = _trainingEndDate,
            TrainingSubjectIds = _trainingSubjectIds,
            TrainingAgeSpecialismType = _trainingAgeSpecialismType,
            TrainingAgeSpecialismRangeFrom = _trainingAgeSpecialismRangeFrom,
            TrainingAgeSpecialismRangeTo = _trainingAgeSpecialismRangeTo,
            TrainingCountryId = _trainingCountryId,
            TrainingProviderId = _trainingProviderId,
            IsExemptFromInduction = _isExemptFromInduction,
            DegreeTypeId = _degreeTypeId,
        };
    }
}
