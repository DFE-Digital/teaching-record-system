using Optional;
using ProfessionalStatusStatus = TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos.ProfessionalStatusStatus;
using TrainingAgeSpecialismType = TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos.TrainingAgeSpecialismType;

namespace TeachingRecordSystem.Api.V3.VNext.Requests;

public record SetProfessionalStatusRequest
{
    public required Guid RouteTypeId { get; init; }
    public required ProfessionalStatusStatus Status { get; init; }
    public DateOnly? AwardedDate { get; init; }
    public DateOnly? TrainingStartDate { get; init; }
    public DateOnly? TrainingEndDate { get; init; }
    public Option<string[]> TrainingSubjectReferences { get; init; }
    public SetProfessionalStatusRequestAgeRangeSpecialism? AgeRangeSpecialism { get; init; }
    public string? TrainingCountryReference { get; init; }
    public string? TrainingProviderUkprn { get; init; }
    public Guid? DegreeTypeId { get; init; }
    public Guid? InductionExemptionReasonId { get; init; }
}

public record SetProfessionalStatusRequestAgeRangeSpecialism
{
    public TrainingAgeSpecialismType Type { get; init; }
    public int? From { get; init; }
    public int? To { get; init; }
}
