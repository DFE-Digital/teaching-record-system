using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Routes.EditRoute;

[Journey(JourneyNames.EditRoute), RequireJourneyInstance]
public class CheckYourAnswersModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    IClock clock,
    IFileService fileService)
{
    public string? PersonName { get; set; }

    public QualificationType QualificationType { get; set; }
    public Guid RouteToProfessionalStatusId { get; set; }
    public ProfessionalStatusStatus Status { get; set; }
    public DateOnly? AwardedDate { get; set; }
    public DateOnly? TrainingStartDate { get; set; }
    public DateOnly? TrainingEndDate { get; set; }
    public Guid[] TrainingSubjectIds { get; set; } = [];
    public TrainingAgeSpecialismType? TrainingAgeSpecialismType { get; set; }
    public int? TrainingAgeSpecialismRangeFrom { get; set; }
    public int? TrainingAgeSpecialismRangeTo { get; set; }
    public string? TrainingCountryId { get; set; }
    public Guid? TrainingProviderId { get; set; }
    public Guid? InductionExemptionReasonId { get; set; }

    public ChangeReasonState ChangeReasonDetail { get; set; } = new();

    [FromRoute]
    public Guid QualificationId { get; set; }
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }

    public string BackLink { get; set; }

    public void OnGet()
    {
    }
}
