using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class CheckAnswersModel(
    SupportUiLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    EvidenceUploadManager evidenceUploadManager,
    IClock clock)
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
        var person = await dbContext.Persons
            .Where(p => p.PersonId == PersonId)
            .Include(p => p.Qualifications)
            .SingleAsync();

        var allRoutes = await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync(activeOnly: false);

        var professionalStatus = RouteToProfessionalStatus.Create(
            person,
            allRoutes,
            RouteType.RouteToProfessionalStatusTypeId,
            sourceApplicationUserId: null,
            sourceApplicationReference: null,
            status: Status,
            holdsFrom: JourneyInstance!.State.HoldsFrom,
            trainingStartDate: JourneyInstance.State.TrainingStartDate,
            trainingEndDate: JourneyInstance.State.TrainingEndDate,
            trainingSubjectIds: JourneyInstance.State.TrainingSubjectIds,
            trainingAgeSpecialismType: JourneyInstance.State.TrainingAgeSpecialismType,
            trainingAgeSpecialismRangeFrom: JourneyInstance.State.TrainingAgeSpecialismRangeFrom,
            trainingAgeSpecialismRangeTo: JourneyInstance.State.TrainingAgeSpecialismRangeTo,
            trainingCountryId: JourneyInstance.State.TrainingCountryId,
            trainingProviderId: JourneyInstance.State.TrainingProviderId,
            degreeTypeId: JourneyInstance.State.DegreeTypeId,
            isExemptFromInduction: JourneyInstance.State.IsExemptFromInduction,
            createdBy: User.GetUserId(),
            now: clock.UtcNow,
            changeReason: JourneyInstance.State.ChangeReason?.GetDisplayName(),
            changeReasonDetail: JourneyInstance.State.ChangeReasonDetail.ChangeReasonDetail,
            evidenceFile: JourneyInstance.State.ChangeReasonDetail.Evidence.UploadedEvidenceFile?.ToEventModel(),
            @event: out var @event);

        dbContext.Qualifications.Add(professionalStatus);
        dbContext.AddEventWithoutBroadcast(@event);
        await dbContext.SaveChangesAsync();

        await JourneyInstance!.CompleteAsync();

        TempData.SetFlashSuccess("Route to professional status added");

        return await ContinueAsync();
    }
}
