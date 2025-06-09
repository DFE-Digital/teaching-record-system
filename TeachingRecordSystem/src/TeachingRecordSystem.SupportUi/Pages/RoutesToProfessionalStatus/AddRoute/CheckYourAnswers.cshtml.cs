using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class CheckYourAnswersModel(TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    IClock clock) :
    AddRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    public RouteDetailViewModel RouteDetail { get; set; } = null!;

    public string BackLink =>
        LinkGenerator.RouteAddPage(PreviousPage(AddRoutePage.CheckYourAnswers) ?? AddRoutePage.Status, PersonId, JourneyInstance!.InstanceId);

    public async Task OnGetAsync()
    {
        RouteDetail.IsExemptFromInduction = JourneyInstance!.State.IsExemptFromInduction;
        RouteDetail.TrainingProvider = RouteDetail.TrainingProviderId is not null ? (await ReferenceDataCache.GetTrainingProviderByIdAsync(RouteDetail.TrainingProviderId!.Value))?.Name : null;
        RouteDetail.TrainingCountry = RouteDetail.TrainingCountryId is not null ? (await ReferenceDataCache.GetTrainingCountryByIdAsync(RouteDetail.TrainingCountryId))?.Name : null;
        RouteDetail.DegreeType = RouteDetail.DegreeTypeId is not null ? (await ReferenceDataCache.GetDegreeTypeByIdAsync(RouteDetail.DegreeTypeId!.Value))?.Name : null;
        RouteDetail.TrainingSubjects = RouteDetail.TrainingSubjectIds is not null ?
            RouteDetail.TrainingSubjectIds
                .Join((await ReferenceDataCache.GetTrainingSubjectsAsync()), id => id, subject => subject.TrainingSubjectId, (_, subject) => subject.Name)
                .OrderByDescending(name => name)
                .ToArray() : null;
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
            Route.RouteToProfessionalStatusTypeId,
            Status,
            JourneyInstance!.State.AwardedDate,
            JourneyInstance!.State.TrainingStartDate,
            JourneyInstance!.State.TrainingEndDate,
            JourneyInstance!.State.TrainingSubjectIds,
            JourneyInstance!.State.TrainingAgeSpecialismType,
            JourneyInstance!.State.TrainingAgeSpecialismRangeFrom,
            JourneyInstance!.State.TrainingAgeSpecialismRangeTo,
            JourneyInstance!.State.TrainingCountryId,
            JourneyInstance!.State.TrainingProviderId,
            JourneyInstance!.State.DegreeTypeId,
            JourneyInstance!.State.IsExemptFromInduction,
            User.GetUserId(),
            clock.UtcNow,
            out var @event);

        dbContext.Qualifications.Add(professionalStatus);
        await dbContext.AddEventAndBroadcastAsync(@event);
        await dbContext.SaveChangesAsync();

        await JourneyInstance!.CompleteAsync();

        TempData.SetFlashSuccess("Route to professional status added");

        return Redirect(LinkGenerator.PersonQualifications(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!(JourneyInstance!.State.RouteToProfessionalStatusId.HasValue && JourneyInstance!.State.Status.HasValue))
        {
            context.Result = new BadRequestResult();
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        Route = await ReferenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId.Value);
        Status = JourneyInstance!.State.Status!.Value;

        var hasImplicitExemption = Route.InductionExemptionReason?.RouteImplicitExemption ?? false;
        RouteDetail = new RouteDetailViewModel
        {
            RouteToProfessionalStatusType = Route,
            Status = JourneyInstance!.State.Status.Value,
            AwardedDate = JourneyInstance!.State.AwardedDate,
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

        await next();
    }
}
