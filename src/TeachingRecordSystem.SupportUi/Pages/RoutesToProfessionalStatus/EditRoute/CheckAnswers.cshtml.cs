using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Optional;
using TeachingRecordSystem.Core.Services.RoutesToProfessionalStatus;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[TeachingRecordSystem.WebCommon.FormFlow.Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance, CheckRouteToProfessionalStatusExistsFilterFactory()]
public class CheckYourAnswersModel(
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    EvidenceUploadManager evidenceController,
    RoutesToProfessionalStatusService routesToProfessionalStatusService) : PageModel
{
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }

    public RouteDetailViewModel RouteDetail { get; set; } = null!;

    public string? PersonName { get; set; }
    public Guid PersonId { get; private set; }

    public ChangeReasonOption? ChangeReason { get; set; }
    public ChangeReasonDetailsState ChangeReasonDetail { get; set; } = new();

    public string BackLink => linkGenerator.RoutesToProfessionalStatus.EditRoute.Reason(QualificationId, JourneyInstance!.InstanceId);

    [FromRoute]
    public Guid QualificationId { get; set; }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var route = await referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId);
        var status = JourneyInstance!.State.Status;

        var pagesInOrder = Enum.GetValues<AddRoutePage>()
            .Except([AddRoutePage.Route, AddRoutePage.Status, AddRoutePage.CheckAnswers])
            .OrderBy(p => p);

        foreach (var page in pagesInOrder)
        {
            var pageRequired = page.FieldRequirementForPage(route, status);

            if (pageRequired == FieldRequirement.Mandatory &&
                !JourneyInstance!.State.IsComplete(page) &&
                // if the route has an implicit exemption, don't show the induction exemption page
                (page != AddRoutePage.InductionExemption ||
                 route.InductionExemptionReason is null ||
                 !route.InductionExemptionReason.RouteImplicitExemption))
            {
                context.Result = Redirect(linkGenerator.RoutesToProfessionalStatus.EditRoute.EditRoutePage(page, QualificationId, JourneyInstance.InstanceId, fromCheckAnswers: true));
                return;
            }
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        ChangeReason = JourneyInstance.State.ChangeReason;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail;
        var hasImplicitExemption = route.InductionExemptionReason?.RouteImplicitExemption ?? false;
        RouteDetail = new RouteDetailViewModel
        {
            RouteToProfessionalStatusType = route,
            Status = JourneyInstance.State.Status,
            HoldsFrom = JourneyInstance.State.HoldsFrom,
            TrainingStartDate = JourneyInstance.State.TrainingStartDate,
            TrainingEndDate = JourneyInstance.State.TrainingEndDate,
            TrainingSubjectIds = JourneyInstance.State.TrainingSubjectIds,
            TrainingAgeSpecialismType = JourneyInstance.State.TrainingAgeSpecialismType,
            TrainingAgeSpecialismRangeFrom = JourneyInstance.State.TrainingAgeSpecialismRangeFrom,
            TrainingAgeSpecialismRangeTo = JourneyInstance.State.TrainingAgeSpecialismRangeTo,
            TrainingCountryId = JourneyInstance.State.TrainingCountryId,
            TrainingProviderId = JourneyInstance.State.TrainingProviderId,
            QualificationId = QualificationId,
            DegreeTypeId = JourneyInstance.State.DegreeTypeId,
            HasImplicitExemption = hasImplicitExemption,
            IsExemptFromInduction = JourneyInstance.State.IsExemptFromInduction,
            FromCheckAnswers = true,
            JourneyInstanceId = JourneyInstance.InstanceId
        };

        await next();
    }

    public async Task OnGetAsync()
    {
        RouteDetail.IsExemptFromInduction = JourneyInstance!.State.IsExemptFromInduction;
        RouteDetail.TrainingProvider = RouteDetail.TrainingProviderId is not null ? (await referenceDataCache.GetTrainingProviderByIdAsync(RouteDetail.TrainingProviderId!.Value))?.Name : null;
        RouteDetail.TrainingCountry = RouteDetail.TrainingCountryId is not null ? (await referenceDataCache.GetTrainingCountryByIdAsync(RouteDetail.TrainingCountryId))?.Name : null;
        RouteDetail.DegreeType = RouteDetail.DegreeTypeId is not null ? (await referenceDataCache.GetDegreeTypeByIdAsync(RouteDetail.DegreeTypeId!.Value))?.Name : null;
        RouteDetail.TrainingSubjects = await SubjectDisplayHelper.GetFormattedSubjectNamesAsync(RouteDetail.TrainingSubjectIds, referenceDataCache);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await routesToProfessionalStatusService.UpdateRouteToProfessionalStatusAsync(
            new UpdateRouteToProfessionalStatusOptions
            {
                QualificationId = QualificationId,
                UpdatedBy = User.GetUserId(),
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
                ExemptFromInduction = Option.Some(RouteDetail.IsExemptFromInduction),
                ChangeReason = ChangeReason?.GetDisplayName(),
                ChangeReasonDetail = ChangeReasonDetail.ChangeReasonDetail,
                EvidenceFile = ChangeReasonDetail.Evidence.UploadedEvidenceFile?.ToEventModel(),
                AdditionalInformation = ChangeReasonDetail.AdditionalInformation
            });

        await JourneyInstance!.CompleteAsync();

        TempData.SetFlashNotificationBanner("Route to professional status updated");

        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceController.DeleteUploadedFileAsync(JourneyInstance!.State.ChangeReasonDetail.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }
}
