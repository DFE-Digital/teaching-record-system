using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus)]
public class DetailModel(
    EditRouteJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache) : PageModel
{
    public string PageCaption => journey.PageCaption;

    public string BackLink =>
        journey.State.FromInductions
            ? linkGenerator.Persons.PersonDetail.Induction(journey.PersonId)
            : linkGenerator.Persons.PersonDetail.Qualifications(journey.PersonId);

    public string CheckAnswersUrl => linkGenerator.RoutesToProfessionalStatus.EditRoute.CheckAnswers(journey.InstanceId);

    public RouteDetailViewModel RouteDetail { get; set; } = null!;

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

        return journey.AdvanceTo(CheckAnswersUrl);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var routeType = await journey.GetRouteTypeAsync();
        var state = journey.State;

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

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
