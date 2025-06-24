using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;

namespace TeachingRecordSystem.SupportUi.Pages.Shared;

public class RouteDetailModel()
{
    public required RouteToProfessionalStatusType RouteToProfessionalStatusType { get; set; }
    public RouteToProfessionalStatusStatus Status { get; set; }
    public DateOnly? HoldsFrom { get; set; }
    public DateOnly? TrainingStartDate { get; set; }
    public DateOnly? TrainingEndDate { get; set; }
    public Guid[]? TrainingSubjectIds { get; set; }
    public TrainingAgeSpecialismType? TrainingAgeSpecialismType { get; set; }
    public int? TrainingAgeSpecialismRangeFrom { get; set; }
    public int? TrainingAgeSpecialismRangeTo { get; set; }
    public string? TrainingAgeSpecialismRange => TrainingAgeSpecialismRangeFrom is not null ? $"From {TrainingAgeSpecialismRangeFrom} to {TrainingAgeSpecialismRangeTo}" : null;
    public string? TrainingCountryId { get; set; }
    public Guid? TrainingProviderId { get; set; }
    public Guid? DegreeTypeId { get; set; }
    public string? DegreeType { get; set; }
    public bool? IsExemptFromInduction { get; set; }
    public bool? HasImplicitExemption { get; set; }
    public string? TrainingProvider { get; set; }
    public string? TrainingCountry { get; set; }
    public string[]? TrainingSubjects { get; set; }
    public JourneyInstanceId JourneyInstanceId { get; set; }

    public FieldRequirement StartDateRequired => QuestionDriverHelper.FieldRequired(RouteToProfessionalStatusType.TrainingStartDateRequired, Status.GetStartDateRequirement());
    public FieldRequirement EndDateRequired => QuestionDriverHelper.FieldRequired(RouteToProfessionalStatusType.TrainingEndDateRequired, Status.GetEndDateRequirement());
    public FieldRequirement AwardDateRequired => QuestionDriverHelper.FieldRequired(RouteToProfessionalStatusType.HoldsFromRequired, Status.GetHoldsFromRequirement());
    public FieldRequirement DegreeTypeRequired => QuestionDriverHelper.FieldRequired(RouteToProfessionalStatusType.DegreeTypeRequired, Status.GetDegreeTypeRequirement());
    public FieldRequirement TrainingProviderRequired => QuestionDriverHelper.FieldRequired(RouteToProfessionalStatusType.TrainingProviderRequired, Status.GetTrainingProviderRequirement());
    public FieldRequirement AgeSpecialismRequired => QuestionDriverHelper.FieldRequired(RouteToProfessionalStatusType.TrainingAgeSpecialismTypeRequired, Status.GetAgeSpecialismRequirement());
    public FieldRequirement TrainingCountryRequired => QuestionDriverHelper.FieldRequired(RouteToProfessionalStatusType.TrainingCountryRequired, Status.GetCountryRequirement());
    public FieldRequirement HasInductionExemptionRequired => QuestionDriverHelper.FieldRequired(RouteToProfessionalStatusType.InductionExemptionRequired, Status.GetInductionExemptionRequirement());
    public FieldRequirement TrainingSubjectsRequired => QuestionDriverHelper.FieldRequired(RouteToProfessionalStatusType.TrainingSubjectsRequired, Status.GetSubjectsRequirement());

    public bool FromCheckAnswers { get; set; }
}
