namespace TeachingRecordSystem.Core.Events.Models;

public record RouteToProfessionalStatus
{
    public required Guid QualificationId { get; init; }
    public required Guid? SourceApplicationUserId { get; init; }
    public required string? SourceApplicationReference { get; init; }
    public required Guid RouteToProfessionalStatusTypeId { get; init; }
    public required RouteToProfessionalStatusStatus Status { get; init; }
    public required DateOnly? HoldsFrom { get; init; }
    public required DateOnly? TrainingStartDate { get; init; }
    public required DateOnly? TrainingEndDate { get; init; }
    public required Guid[] TrainingSubjectIds { get; init; }
    public required TrainingAgeSpecialismType? TrainingAgeSpecialismType { get; init; }
    public required int? TrainingAgeSpecialismRangeFrom { get; init; }
    public required int? TrainingAgeSpecialismRangeTo { get; init; }
    public required string? TrainingCountryId { get; init; }
    public required Guid? TrainingProviderId { get; init; }
    public required bool? ExemptFromInduction { get; set; }
    public required bool? ExemptFromInductionDueToQtsDate { get; init; }
    public required Guid? DegreeTypeId { get; init; }

    public static RouteToProfessionalStatus FromModel(DataStore.Postgres.Models.RouteToProfessionalStatus model) => new()
    {
        QualificationId = model.QualificationId,
        SourceApplicationUserId = model.SourceApplicationUserId,
        SourceApplicationReference = model.SourceApplicationReference,
        RouteToProfessionalStatusTypeId = model.RouteToProfessionalStatusTypeId,
        Status = model.Status,
        HoldsFrom = model.HoldsFrom,
        TrainingStartDate = model.TrainingStartDate,
        TrainingEndDate = model.TrainingEndDate,
        TrainingSubjectIds = model.TrainingSubjectIds,
        TrainingAgeSpecialismType = model.TrainingAgeSpecialismType,
        TrainingAgeSpecialismRangeFrom = model.TrainingAgeSpecialismRangeFrom,
        TrainingAgeSpecialismRangeTo = model.TrainingAgeSpecialismRangeTo,
        TrainingCountryId = model.TrainingCountryId,
        TrainingProviderId = model.TrainingProviderId,
        ExemptFromInduction = model.ExemptFromInduction,
        ExemptFromInductionDueToQtsDate = model.ExemptFromInductionDueToQtsDate,
        DegreeTypeId = model.DegreeTypeId
    };
}
