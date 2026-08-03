using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.Services.RoutesToProfessionalStatus;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[TeachingRecordSystem.WebCommon.FormFlow.Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class CheckAnswersModel(
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    EvidenceUploadManager evidenceUploadManager,
    RoutesToProfessionalStatusService routesToProfessionalStatusService)
    : AddRoutePostStatusPageModel(AddRoutePage.CheckAnswers, linkGenerator, referenceDataCache, evidenceUploadManager)
{
    public RouteDetailViewModel RouteDetail { get; set; } = null!;

    public ChangeReasonOption? ChangeReason { get; set; }
    public ChangeReasonDetailsState ChangeReasonDetail { get; set; } = new();

    public override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        var pagesInOrder = Enum.GetValues<AddRoutePage>()
            .Except([AddRoutePage.Route, AddRoutePage.Status, AddRoutePage.CheckAnswers])
            .OrderBy(p => p);

        foreach (var page in pagesInOrder)
        {
            var pageRequired = page.FieldRequirementForPage(RouteType, Status);

            if (pageRequired == FieldRequirement.Mandatory &&
                !JourneyInstance!.State.IsComplete(page) &&
                // if the route has an implicit exemption, don't show the induction exemption page
                (page != AddRoutePage.InductionExemption ||
                 RouteType.InductionExemptionReason is null ||
                 !RouteType.InductionExemptionReason.RouteImplicitExemption))
            {
                context.Result = Redirect(LinkGenerator.RoutesToProfessionalStatus.AddRoute.AddRoutePage(page, PersonId, JourneyInstance.InstanceId, fromCheckAnswers: true));
                return;
            }
        }

        var hasImplicitExemption = RouteType.InductionExemptionReason?.RouteImplicitExemption ?? false;
        ChangeReason = JourneyInstance!.State.ChangeReason;
        ChangeReasonDetail = JourneyInstance!.State.ChangeReasonDetail;
        RouteDetail = new RouteDetailViewModel
        {
            RouteToProfessionalStatusType = RouteType,
            Status = Status,
            HoldsFrom = JourneyInstance!.State.HoldsFrom,
            TrainingStartDate = JourneyInstance!.State.TrainingStartDate,
            TrainingEndDate = JourneyInstance!.State.TrainingEndDate,
            TrainingSubjectIds = JourneyInstance!.State.TrainingSubjectIds,
            TrainingAgeSpecialismType = JourneyInstance!.State.TrainingAgeSpecialismType,
            TrainingAgeSpecialismRangeFrom = JourneyInstance!.State.TrainingAgeSpecialismRangeFrom,
            TrainingAgeSpecialismRangeTo = JourneyInstance!.State.TrainingAgeSpecialismRangeTo,
            TrainingCountryId = JourneyInstance!.State.TrainingCountryId,
            TrainingProviderId = JourneyInstance!.State.TrainingProviderId,
            DegreeTypeId = JourneyInstance!.State.DegreeTypeId,
            HasImplicitExemption = hasImplicitExemption,
            IsExemptFromInduction = JourneyInstance!.State.IsExemptFromInduction,
            FromCheckAnswers = true,
            JourneyInstanceId = JourneyInstance!.InstanceId,
            PersonId = PersonId
        };
    }

    public async Task OnGetAsync()
    {
        RouteDetail.IsExemptFromInduction = JourneyInstance!.State.IsExemptFromInduction;
        RouteDetail.TrainingProvider = RouteDetail.TrainingProviderId is not null ? (await ReferenceDataCache.GetTrainingProviderByIdAsync(RouteDetail.TrainingProviderId!.Value))?.Name : null;
        RouteDetail.TrainingCountry = RouteDetail.TrainingCountryId is not null ? (await ReferenceDataCache.GetTrainingCountryByIdAsync(RouteDetail.TrainingCountryId))?.Name : null;
        RouteDetail.DegreeType = RouteDetail.DegreeTypeId is not null ? (await ReferenceDataCache.GetDegreeTypeByIdAsync(RouteDetail.DegreeTypeId!.Value))?.Name : null;
        RouteDetail.TrainingSubjects = await SubjectDisplayHelper.GetFormattedSubjectNamesAsync(RouteDetail.TrainingSubjectIds, ReferenceDataCache);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await routesToProfessionalStatusService.CreateRouteToProfessionalStatusAsync(
            new CreateRouteToProfessionalStatusOptions
            {
                PersonId = PersonId,
                RouteToProfessionalStatusTypeId = RouteType.RouteToProfessionalStatusTypeId,
                Status = Status,
                CreatedBy = User.GetUserId(),
                HoldsFrom = JourneyInstance!.State.HoldsFrom,
                TrainingStartDate = JourneyInstance.State.TrainingStartDate,
                TrainingEndDate = JourneyInstance.State.TrainingEndDate,
                TrainingSubjectIds = JourneyInstance.State.TrainingSubjectIds,
                TrainingAgeSpecialismType = JourneyInstance.State.TrainingAgeSpecialismType,
                TrainingAgeSpecialismRangeFrom = JourneyInstance.State.TrainingAgeSpecialismRangeFrom,
                TrainingAgeSpecialismRangeTo = JourneyInstance.State.TrainingAgeSpecialismRangeTo,
                TrainingCountryId = JourneyInstance.State.TrainingCountryId,
                TrainingProviderId = JourneyInstance.State.TrainingProviderId,
                DegreeTypeId = JourneyInstance.State.DegreeTypeId,
                IsExemptFromInduction = JourneyInstance.State.IsExemptFromInduction,
                ChangeReason = JourneyInstance.State.ChangeReason?.GetDisplayName(),
                ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail.ChangeReasonDetail,
                EvidenceFile = JourneyInstance.State.ChangeReasonDetail.Evidence.UploadedEvidenceFile?.ToEventModel(),
                AdditionalInformation = JourneyInstance.State.ChangeReasonDetail.AdditionalInformation
            });

        await JourneyInstance!.CompleteAsync();

        TempData.SetFlashNotificationBanner("Route to professional status added");

        return await ContinueAsync();
    }
}
