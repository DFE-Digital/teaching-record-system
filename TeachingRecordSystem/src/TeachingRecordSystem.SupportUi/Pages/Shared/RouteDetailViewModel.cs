using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.Pages.Shared;

public class RouteDetailViewModel()
{
    public Guid QualificationId { get; set; }
    public QualificationType? QualificationType { get; set; }
    public required RouteToProfessionalStatus RouteToProfessionalStatus { get; set; }
    public ProfessionalStatusStatus Status { get; set; }
    public DateOnly? AwardedDate { get; set; }
    public DateOnly? TrainingStartDate { get; set; }
    public DateOnly? TrainingEndDate { get; set; }
    public Guid[]? TrainingSubjectIds { get; set; }
    public TrainingAgeSpecialismType? TrainingAgeSpecialismType { get; set; }
    public int? TrainingAgeSpecialismRangeFrom { get; set; }
    public int? TrainingAgeSpecialismRangeTo { get; set; }
    public string? TrainingAgeSpecialismRange => TrainingAgeSpecialismRangeFrom is not null ? $"From {TrainingAgeSpecialismRangeFrom} to {TrainingAgeSpecialismRangeTo}" : null;
    public string? TrainingCountryId { get; set; }
    public Guid? TrainingProviderId { get; set; }
    public Guid? InductionExemptionReasonId { get; set; }
    public Guid? DegreeTypeId { get; set; }
    public string? DegreeType { get; set; }
    public string? ExemptionReason { get; set; }
    public string? TrainingProvider { get; set; }
    public string? TrainingCountry { get; set; }
    public string[]? TrainingSubjects { get; set; }
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }

    public FieldRequirement StartDateRequired => QuestionDriverHelper.FieldRequired(RouteToProfessionalStatus.TrainingStartDateRequired, Status.GetStartDateRequirement());
    public FieldRequirement EndDateRequired => QuestionDriverHelper.FieldRequired(RouteToProfessionalStatus.TrainingEndDateRequired, Status.GetEndDateRequirement());
    public FieldRequirement AwardDateRequired => QuestionDriverHelper.FieldRequired(RouteToProfessionalStatus.AwardDateRequired, Status.GetAwardDateRequirement());
    public FieldRequirement DegreeTypeRequired => QuestionDriverHelper.FieldRequired(RouteToProfessionalStatus.DegreeTypeRequired, Status.GetDegreeTypeRequirement());

    public bool FromCheckAnswers { get; set; }
}
