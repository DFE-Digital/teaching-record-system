using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Routes.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance]
public class CheckYourAnswersModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    IFileService fileService) : PageModel
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

    public ChangeReasonState ChangeReasonDetail { get; set; } = new(fileService);

    [FromRoute]
    public Guid QualificationId { get; set; }
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }

    public string BackLink { get; set; }

    public void OnGet()
    {
    }

    public override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        JourneyInstance!.State.EnsureInitialized(context.HttpContext.GetCurrentProfessionalStatusFeature());

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        ChangeReasonDetail = JourneyInstance!.State.ChangeReasonDetail;

        QualificationType = JourneyInstance!.State.QualificationType;
        RouteToProfessionalStatusId = JourneyInstance!.State.RouteToProfessionalStatusId;
        Status = JourneyInstance!.State.Status;
        AwardedDate = JourneyInstance!.State.AwardedDate;
        TrainingStartDate = JourneyInstance!.State.TrainingStartDate;
        TrainingEndDate = JourneyInstance!.State.TrainingEndDate;
        TrainingSubjectIds = JourneyInstance!.State.TrainingSubjectIds;
        TrainingAgeSpecialismType = JourneyInstance!.State.TrainingAgeSpecialismType;
        TrainingAgeSpecialismRangeFrom = JourneyInstance!.State.TrainingAgeSpecialismRangeFrom;
        TrainingAgeSpecialismRangeTo = JourneyInstance!.State.TrainingAgeSpecialismRangeTo;
        TrainingCountryId = JourneyInstance!.State.TrainingCountryId;
        TrainingProviderId = JourneyInstance!.State.TrainingProviderId;
        InductionExemptionReasonId = JourneyInstance!.State.InductionExemptionReasonId;

        return next();
    }
}
