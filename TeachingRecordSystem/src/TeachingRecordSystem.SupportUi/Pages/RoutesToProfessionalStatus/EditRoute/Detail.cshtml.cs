using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), ActivatesJourney, RequireJourneyInstance, CheckRouteToProfessionalStatusExistsFilterFactory()]
public class DetailModel(
    TrsLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache) : PageModel
{
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }

    public RouteDetailViewModel RouteDetail { get; set; } = null!;
    public string? PersonName { get; set; }
    public Guid PersonId { get; private set; }

    public string BackLink => FromInductions ? linkGenerator.PersonInduction(PersonId) : linkGenerator.PersonQualifications(PersonId);

    [FromRoute]
    public Guid QualificationId { get; set; }

    [FromQuery]
    public bool FromInductions { get; set; }

    public async Task OnGetAsync()
    {
        RouteDetail.IsExemptFromInduction = JourneyInstance!.State.IsExemptFromInduction;
        RouteDetail.TrainingProvider = RouteDetail.TrainingProviderId is not null ? (await referenceDataCache.GetTrainingProviderByIdAsync(RouteDetail.TrainingProviderId!.Value))?.Name : null;
        RouteDetail.TrainingCountry = RouteDetail.TrainingCountryId is not null ? (await referenceDataCache.GetTrainingCountryByIdAsync(RouteDetail.TrainingCountryId))?.Name : null;
        RouteDetail.DegreeType = RouteDetail.DegreeTypeId is not null ? (await referenceDataCache.GetDegreeTypeByIdAsync(RouteDetail.DegreeTypeId!.Value))?.Name : null;
        RouteDetail.TrainingSubjects = await SubjectDisplayHelper.GetFormattedSubjectNamesAsync(RouteDetail.TrainingSubjectIds, referenceDataCache);
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!JourneyInstance!.State.Initialized)
        {
            await JourneyInstance.UpdateStateAsync(state => state.EnsureInitialized(context.HttpContext.GetCurrentProfessionalStatusFeature().RouteToProfessionalStatus));
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        var route = await referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId);

        var hasImplicitExemption = route.InductionExemptionReasonId.HasValue ?
            (await referenceDataCache.GetInductionExemptionReasonByIdAsync(route.InductionExemptionReasonId!.Value)).RouteImplicitExemption
            : false;

        RouteDetail = new RouteDetailViewModel()
        {
            RouteToProfessionalStatusType = route,
            Status = JourneyInstance!.State.Status,
            HoldsFrom = JourneyInstance!.State.HoldsFrom,
            TrainingStartDate = JourneyInstance!.State.TrainingStartDate,
            TrainingEndDate = JourneyInstance!.State.TrainingEndDate,
            TrainingSubjectIds = JourneyInstance!.State.TrainingSubjectIds,
            TrainingAgeSpecialismType = JourneyInstance!.State.TrainingAgeSpecialismType,
            TrainingAgeSpecialismRangeFrom = JourneyInstance!.State.TrainingAgeSpecialismRangeFrom,
            TrainingAgeSpecialismRangeTo = JourneyInstance!.State.TrainingAgeSpecialismRangeTo,
            TrainingCountryId = JourneyInstance!.State.TrainingCountryId,
            TrainingProviderId = JourneyInstance!.State.TrainingProviderId,
            IsExemptFromInduction = JourneyInstance!.State.IsExemptFromInduction,
            QualificationId = QualificationId,
            DegreeTypeId = JourneyInstance!.State.DegreeTypeId,
            HasImplicitExemption = hasImplicitExemption,
            JourneyInstanceId = JourneyInstance.InstanceId
        };

        await next();
    }
}
