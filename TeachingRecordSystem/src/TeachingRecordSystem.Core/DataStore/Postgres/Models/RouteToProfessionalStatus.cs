namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class RouteToProfessionalStatus
{
    public static Guid AssessmentOnlyRouteId { get; } = new("57B86CEF-98E2-4962-A74A-D47C7A34B838");

    public required Guid RouteToProfessionalStatusId { get; init; }
    public required string Name { get; init; }
    public required ProfessionalStatusType ProfessionalStatusType { get; init; }
    public required bool IsActive { get; set; }
    public required FieldRequirement TrainingStartDateRequired { get; init; }
    public required FieldRequirement TrainingEndDateRequired { get; init; }
    public required FieldRequirement AwardDateRequired { get; init; }
    public required FieldRequirement InductionExemptionRequired { get; init; }
    public required FieldRequirement TrainingProviderRequired { get; init; }
    public required FieldRequirement DegreeTypeRequired { get; init; }
    public required FieldRequirement TrainingCountryRequired { get; init; }
    public required FieldRequirement TrainingAgeSpecialismTypeRequired { get; init; }
    public required FieldRequirement TrainingSubjectsRequired { get; init; }

}
