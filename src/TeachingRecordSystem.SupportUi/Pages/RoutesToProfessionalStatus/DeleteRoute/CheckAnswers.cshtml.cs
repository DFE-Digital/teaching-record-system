using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.RoutesToProfessionalStatus;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

[Journey(JourneyNames.DeleteRouteToProfessionalStatus)]
public class CheckAnswersModel(
    DeleteRouteJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    RoutesToProfessionalStatusService routesToProfessionalStatusService) : PageModel
{
    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string PageCaption => journey.PageCaption;

    public string? BackLink { get; set; }

    // The URL a change link brings the user back to once they've answered the question again.
    public string ReturnUrl { get; set; } = null!;

    public RouteDetailViewModel RouteDetail { get; set; } = null!;

    public Guid PersonId { get; private set; }

    public ChangeReasonOption? ChangeReason { get; set; }

    public ChangeReasonDetailsState ChangeReasonDetail { get; set; } = new();

    [FromRoute]
    public Guid QualificationId { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        await routesToProfessionalStatusService.DeleteRouteToProfessionalStatusAsync(
            new DeleteRouteToProfessionalStatusOptions
            {
                QualificationId = QualificationId,
                DeletedBy = User.GetUserId(),
                DeletionReason = ChangeReason!.GetDisplayName(),
                DeletionReasonDetail = ChangeReasonDetail.ChangeReasonDetail,
                EvidenceFile = ChangeReasonDetail.Evidence.UploadedEvidenceFile?.ToEventModel(),
                AdditionalInformation = ChangeReasonDetail.AdditionalInformation
            });

        journey.DeleteInstance();

        TempData.SetFlashNotificationBanner("Route to professional status deleted");

        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ReturnUrl = linkGenerator.RoutesToProfessionalStatus.DeleteRoute.CheckAnswers(journey.InstanceId);
        BackLink = journey.GetBackLink();

        PersonId = context.HttpContext.GetCurrentPersonFeature().PersonId;

        var route = context.HttpContext.GetCurrentProfessionalStatusFeature().RouteToProfessionalStatus;
        RouteDetail = new RouteDetailViewModel()
        {
            RouteToProfessionalStatusType = route.RouteToProfessionalStatusType!,
            HoldsFrom = route.HoldsFrom,
            DegreeTypeId = route.DegreeTypeId,
            IsExemptFromInduction = route.ExemptFromInduction,
            Status = route.Status,
            QualificationId = route.QualificationId,
            TrainingAgeSpecialismType = route.TrainingAgeSpecialismType,
            TrainingAgeSpecialismRangeFrom = route.TrainingAgeSpecialismRangeFrom,
            TrainingAgeSpecialismRangeTo = route.TrainingAgeSpecialismRangeTo,
            TrainingCountryId = route.TrainingCountryId,
            TrainingEndDate = route.TrainingEndDate,
            TrainingProviderId = route.TrainingProviderId,
            TrainingStartDate = route.TrainingStartDate,
            TrainingSubjectIds = route.TrainingSubjectIds
        };

        RouteDetail.TrainingProvider = RouteDetail.TrainingProviderId is Guid trainingProviderId
            ? (await referenceDataCache.GetTrainingProviderByIdAsync(trainingProviderId))?.Name
            : null;
        RouteDetail.TrainingCountry = RouteDetail.TrainingCountryId is string trainingCountryId
            ? (await referenceDataCache.GetTrainingCountryByIdAsync(trainingCountryId))?.Name
            : null;
        RouteDetail.DegreeType = RouteDetail.DegreeTypeId is Guid degreeTypeId
            ? (await referenceDataCache.GetDegreeTypeByIdAsync(degreeTypeId))?.Name
            : null;
        RouteDetail.TrainingSubjects = await SubjectDisplayHelper.GetFormattedSubjectNamesAsync(RouteDetail.TrainingSubjectIds, referenceDataCache);

        ChangeReason = journey.State.ChangeReason;
        ChangeReasonDetail = journey.State.ChangeReasonDetail;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
