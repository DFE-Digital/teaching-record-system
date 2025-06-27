using Optional;
using RouteToProfessionalStatusStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos.RouteToProfessionalStatusStatus;
using TrainingAgeSpecialismType = TeachingRecordSystem.Core.ApiSchema.V3.V20250425.Dtos.TrainingAgeSpecialismType;

namespace TeachingRecordSystem.Api.V3.V20250627.Requests;

public record SetRouteToProfessionalStatusRequest
{
    public required Guid RouteToProfessionalStatusTypeId { get; init; }
    public required RouteToProfessionalStatusStatus Status { get; init; }
    public DateOnly? HoldsFrom { get; init; }
    public DateOnly? TrainingStartDate { get; init; }
    public DateOnly? TrainingEndDate { get; init; }
    public Option<string[]> TrainingSubjectReferences { get; init; }
    public SetRouteToProfessionalStatusRequestTrainingAgeSpecialism? TrainingAgeSpecialism { get; init; }
    public string? TrainingCountryReference { get; init; }
    public string? TrainingProviderUkprn { get; init; }
    public Guid? DegreeTypeId { get; init; }
    public bool? IsExemptFromInduction { get; init; }
}

public record SetRouteToProfessionalStatusRequestTrainingAgeSpecialism
{
    public TrainingAgeSpecialismType Type { get; init; }
    public int? From { get; init; }
    public int? To { get; init; }
}
