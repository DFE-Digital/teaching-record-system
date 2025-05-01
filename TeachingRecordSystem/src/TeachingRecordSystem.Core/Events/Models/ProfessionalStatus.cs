namespace TeachingRecordSystem.Core.Events.Models;

public record ProfessionalStatus
{
    public required Guid RouteToProfessionalStatusId { get; init; }
    public required ProfessionalStatusStatus Status { get; init; }
    public required DateOnly? AwardedDate { get; init; }
    public required DateOnly? TrainingStartDate { get; init; }
    public required DateOnly? TrainingEndDate { get; init; }
    public required Guid[] TrainingSubjectIds { get; init; }
    public required TrainingAgeSpecialismType? TrainingAgeSpecialismType { get; init; }
    public required int? TrainingAgeSpecialismRangeFrom { get; init; }
    public required int? TrainingAgeSpecialismRangeTo { get; init; }
    public required string? TrainingCountryId { get; init; }
    public required Guid? TrainingProviderId { get; init; }
    public bool? ExemptFromInduction { get; set; }
    public required Guid? DegreeTypeId { get; init; }

    public static ProfessionalStatus FromModel(DataStore.Postgres.Models.ProfessionalStatus model) => new()
    {
        RouteToProfessionalStatusId = model.RouteToProfessionalStatusId,
        Status = model.Status,
        AwardedDate = model.AwardedDate,
        TrainingStartDate = model.TrainingStartDate,
        TrainingEndDate = model.TrainingEndDate,
        TrainingSubjectIds = model.TrainingSubjectIds,
        TrainingAgeSpecialismType = model.TrainingAgeSpecialismType,
        TrainingAgeSpecialismRangeFrom = model.TrainingAgeSpecialismRangeFrom,
        TrainingAgeSpecialismRangeTo = model.TrainingAgeSpecialismRangeTo,
        TrainingCountryId = model.TrainingCountryId,
        TrainingProviderId = model.TrainingProviderId,
        ExemptFromInduction = model.ExemptFromInduction,
        DegreeTypeId = model.DegreeTypeId
    };
}
