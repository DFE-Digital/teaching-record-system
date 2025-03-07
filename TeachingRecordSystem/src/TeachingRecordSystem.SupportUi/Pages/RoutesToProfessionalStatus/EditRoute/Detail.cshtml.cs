using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Pages.Shared;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), ActivatesJourney, RequireJourneyInstance, CheckProfessionalStatusExistsFilterFactory()]
public class DetailModel(
    TrsLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache) : PageModel
{
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }

    public RouteDetailViewModel RouteDetail { get; set; } = new();
    public string? PersonName { get; set; }
    public Guid PersonId { get; private set; }

    public string BackLink => linkGenerator.PersonQualifications(PersonId);

    [FromRoute]
    public Guid QualificationId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public async Task OnGetAsync()
    {
        var routeToProfessionalStatus = await referenceDataCache.GetRouteToProfessionalStatusByIdAsync(RouteDetail.RouteToProfessionalStatusId);
        RouteDetail.EndDateRequired = QuestionDriverHelper.FieldRequired(routeToProfessionalStatus.TrainingEndDateRequired, JourneyInstance!.State.Status.GetEndDateRequirement());
        RouteDetail.RouteToProfessionalStatusName = routeToProfessionalStatus?.Name;
        RouteDetail.ExemptionReason = RouteDetail.InductionExemptionReasonId is not null ? (await referenceDataCache.GetInductionExemptionReasonByIdAsync(RouteDetail.InductionExemptionReasonId!.Value))?.Name : null;
        RouteDetail.TrainingProvider = RouteDetail.TrainingProviderId is not null ? (await referenceDataCache.GetTrainingProviderByIdAsync(RouteDetail.TrainingProviderId!.Value))?.Name : null;
        RouteDetail.TrainingCountry = RouteDetail.TrainingCountryId is not null ? (await referenceDataCache.GetTrainingCountryByIdAsync(RouteDetail.TrainingCountryId))?.Name : null;
        RouteDetail.TrainingSubjects = RouteDetail.TrainingSubjectIds is not null ?
            RouteDetail.TrainingSubjectIds
                .Join((await referenceDataCache.GetTrainingSubjectsAsync()), id => id, subject => subject.TrainingSubjectId, (_, subject) => subject.Name)
                .OrderByDescending(name => name)
                .ToArray() : null;
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
            await JourneyInstance.UpdateStateAsync(state => state.EnsureInitialized(context.HttpContext.GetCurrentProfessionalStatusFeature()));
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        RouteDetail = new RouteDetailViewModel()
        {
            QualificationType = JourneyInstance!.State.QualificationType,
            RouteToProfessionalStatusId = JourneyInstance!.State.RouteToProfessionalStatusId,
            Status = JourneyInstance!.State.Status,
            AwardedDate = JourneyInstance!.State.AwardedDate,
            TrainingStartDate = JourneyInstance!.State.TrainingStartDate,
            TrainingEndDate = JourneyInstance!.State.TrainingEndDate,
            TrainingSubjectIds = JourneyInstance!.State.TrainingSubjectIds,
            TrainingAgeSpecialismType = JourneyInstance!.State.TrainingAgeSpecialismType,
            TrainingAgeSpecialismRangeFrom = JourneyInstance!.State.TrainingAgeSpecialismRangeFrom,
            TrainingAgeSpecialismRangeTo = JourneyInstance!.State.TrainingAgeSpecialismRangeTo,
            TrainingCountryId = JourneyInstance!.State.TrainingCountryId,
            TrainingProviderId = JourneyInstance!.State.TrainingProviderId,
            InductionExemptionReasonId = JourneyInstance!.State.InductionExemptionReasonId,
            QualificationId = QualificationId,
            JourneyInstance = JourneyInstance
        };

        await next();
    }
}
