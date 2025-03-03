using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Events.Models;

public record ProfessionalStatus
{
    public required ProfessionalStatusType ProfessionalStatusType { get; set; }
    public required RouteToProfessionalStatus Route { get; init; }
    public required ProfessionalStatusStatus Status { get; init; }
    public required DateOnly? AwardedDate { get; init; }
    public required DateOnly? TrainingStartDate { get; init; }
    public required DateOnly? TrainingEndDate { get; init; }
    public required Guid[] TrainingSubjectIds { get; init; }
    public required TrainingAgeSpecialismType? TrainingAgeSpecialismType { get; init; }
    public required int? TrainingAgeSpecialismRangeFrom { get; init; }
    public required int? TrainingAgeSpecialismRangeTo { get; init; }
    public required Country? TrainingCountry { get; init; }
    public required TrainingProvider? TrainingProvider { get; init; }
    public required InductionExemptionReason? InductionExemptionReason { get; init; }

    public static ProfessionalStatus FromModel(DataStore.Postgres.Models.ProfessionalStatus model, string? providerNameHint = null) => new()
    {
        ProfessionalStatusType = model.ProfessionalStatusType,
        Route = model.Route,
        Status = model.Status,
        AwardedDate = model.AwardedDate,
        TrainingStartDate = model.TrainingStartDate,
        TrainingEndDate = model.TrainingEndDate,
        TrainingSubjectIds = model.TrainingSubjectIds,
        TrainingAgeSpecialismType = model.TrainingAgeSpecialismType,
        TrainingAgeSpecialismRangeFrom = model.TrainingAgeSpecialismRangeFrom,
        TrainingAgeSpecialismRangeTo = model.TrainingAgeSpecialismRangeTo,
        TrainingCountry = model.TrainingCountry,
        TrainingProvider = model.TrainingProviderId is not null ? new TrainingProvider
        {
            IsActive = model.TrainingProvider.IsActive,
            Name = model.TrainingProvider.Name,
            TrainingProviderId = model.TrainingProvider.TrainingProviderId,
            Ukprn = model.TrainingProvider.Ukprn
        } : null,
        InductionExemptionReason = model.InductionExemptionReason
    };
}
