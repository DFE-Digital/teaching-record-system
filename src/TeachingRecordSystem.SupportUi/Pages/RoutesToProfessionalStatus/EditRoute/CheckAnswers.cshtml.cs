using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Optional;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.RoutesToProfessionalStatus;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus)]
public class CheckYourAnswersModel(
    EditRouteJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    TimeProvider timeProvider,
    RoutesToProfessionalStatusService routesToProfessionalStatusService) : PageModel
{
    public string PageCaption => journey.PageCaption;

    public string? BackLink { get; set; }

    public RouteDetailViewModel RouteDetail { get; set; } = null!;

    public ChangeReasonOption? ChangeReason { get; set; }

    public ChangeReasonDetailsState ChangeReasonDetail { get; set; } = new();

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

        await routesToProfessionalStatusService.UpdateRouteToProfessionalStatusAsync(
            new UpdateRouteToProfessionalStatusOptions
            {
                QualificationId = journey.QualificationId,
                RouteToProfessionalStatusTypeId = Option.Some(RouteDetail.RouteToProfessionalStatusType.RouteToProfessionalStatusTypeId),
                Status = Option.Some(RouteDetail.Status),
                HoldsFrom = Option.Some(RouteDetail.HoldsFrom),
                TrainingStartDate = Option.Some(RouteDetail.TrainingStartDate),
                TrainingEndDate = Option.Some(RouteDetail.TrainingEndDate),
                TrainingSubjectIds = Option.Some(RouteDetail.TrainingSubjectIds ?? []),
                TrainingAgeSpecialismType = Option.Some(RouteDetail.TrainingAgeSpecialismType),
                TrainingAgeSpecialismRangeFrom = Option.Some(RouteDetail.TrainingAgeSpecialismRangeFrom),
                TrainingAgeSpecialismRangeTo = Option.Some(RouteDetail.TrainingAgeSpecialismRangeTo),
                TrainingCountryId = Option.Some(RouteDetail.TrainingCountryId),
                TrainingProviderId = Option.Some(RouteDetail.TrainingProviderId),
                DegreeTypeId = Option.Some(RouteDetail.DegreeTypeId),
                ExemptFromInduction = Option.Some(RouteDetail.IsExemptFromInduction)
            },
            new ProcessContext(
                ProcessType.RouteToProfessionalStatusUpdating,
                timeProvider.UtcNow,
                User.GetUserId(),
                new ChangeReasonWithDetailsAndEvidence
                {
                    Reason = ChangeReason?.GetDisplayName(),
                    Details = ChangeReasonDetail.ChangeReasonDetail,
                    EvidenceFile = ChangeReasonDetail.Evidence.UploadedEvidenceFile?.ToEventModel(),
                    AdditionalInformation = ChangeReasonDetail.AdditionalInformation
                }));

        journey.DeleteInstance();

        TempData.SetFlashNotificationBanner("Route to professional status updated");

        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(journey.PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var checkAnswersUrl = linkGenerator.RoutesToProfessionalStatus.EditRoute.CheckAnswers(journey.InstanceId);

        // The detail page lets the user come straight here, so this is what asks for the reason for the
        // change — and for anything the route needs that its answers don't cover yet.
        if (await journey.GetUnansweredQuestionUrlAsync(returnUrl: checkAnswersUrl) is string unansweredQuestionUrl)
        {
            context.Result = Redirect(unansweredQuestionUrl);
            return;
        }

        var routeType = await journey.GetRouteTypeAsync();
        var state = journey.State;

        ChangeReason = state.ChangeReason;
        ChangeReasonDetail = state.ChangeReasonDetail;

        RouteDetail = new RouteDetailViewModel
        {
            RouteToProfessionalStatusType = routeType,
            Status = state.Status,
            HoldsFrom = state.HoldsFrom,
            TrainingStartDate = state.TrainingStartDate,
            TrainingEndDate = state.TrainingEndDate,
            TrainingSubjectIds = state.TrainingSubjectIds,
            TrainingAgeSpecialismType = state.TrainingAgeSpecialismType,
            TrainingAgeSpecialismRangeFrom = state.TrainingAgeSpecialismRangeFrom,
            TrainingAgeSpecialismRangeTo = state.TrainingAgeSpecialismRangeTo,
            TrainingCountryId = state.TrainingCountryId,
            TrainingProviderId = state.TrainingProviderId,
            DegreeTypeId = state.DegreeTypeId,
            IsExemptFromInduction = state.IsExemptFromInduction,
            HasImplicitExemption = routeType.InductionExemptionReason?.RouteImplicitExemption ?? false,
            InstanceId = journey.InstanceId,
            ReturnUrl = checkAnswersUrl,
            TrainingProvider = state.TrainingProviderId is Guid trainingProviderId
                ? (await referenceDataCache.GetTrainingProviderByIdAsync(trainingProviderId))?.Name
                : null,
            TrainingCountry = state.TrainingCountryId is string trainingCountryId
                ? (await referenceDataCache.GetTrainingCountryByIdAsync(trainingCountryId))?.Name
                : null,
            DegreeType = state.DegreeTypeId is Guid degreeTypeId
                ? (await referenceDataCache.GetDegreeTypeByIdAsync(degreeTypeId))?.Name
                : null,
            TrainingSubjects = await SubjectDisplayHelper.GetFormattedSubjectNamesAsync(state.TrainingSubjectIds, referenceDataCache)
        };

        BackLink = journey.GetBackLink() ?? linkGenerator.RoutesToProfessionalStatus.EditRoute.Detail(journey.InstanceId);

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
