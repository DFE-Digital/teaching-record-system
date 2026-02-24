using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

[Journey(JourneyNames.DeleteRouteToProfessionalStatus), RequireJourneyInstance, CheckRouteToProfessionalStatusExistsFilterFactory()]
public class CheckAnswersModel(
    SupportUiLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    EvidenceUploadManager evidenceController,
    IClock clock) : PageModel
{
    public JourneyInstance<DeleteRouteState>? JourneyInstance { get; set; }

    public RouteDetailViewModel RouteDetail { get; set; } = null!;

    public string? PersonName { get; set; }
    public Guid PersonId { get; private set; }

    public ChangeReasonOption? ChangeReason { get; set; }
    public ChangeReasonDetailsState ChangeReasonDetail { get; set; } = new();

    public string BackLink => linkGenerator.RoutesToProfessionalStatus.DeleteRoute.Reason(QualificationId, JourneyInstance!.InstanceId);

    [FromRoute]
    public Guid QualificationId { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.RoutesToProfessionalStatus.DeleteRoute.Reason(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        var routeInfo = context.HttpContext.GetCurrentProfessionalStatusFeature();
        RouteDetail = new RouteDetailViewModel()
        {
            RouteToProfessionalStatusType = routeInfo.RouteToProfessionalStatus.RouteToProfessionalStatusType!,
            HoldsFrom = routeInfo.RouteToProfessionalStatus.HoldsFrom,
            DegreeTypeId = routeInfo.RouteToProfessionalStatus.DegreeTypeId,
            IsExemptFromInduction = routeInfo.RouteToProfessionalStatus.ExemptFromInduction,
            Status = routeInfo.RouteToProfessionalStatus.Status,
            QualificationId = routeInfo.RouteToProfessionalStatus.QualificationId,
            TrainingAgeSpecialismType = routeInfo.RouteToProfessionalStatus.TrainingAgeSpecialismType,
            TrainingAgeSpecialismRangeFrom = routeInfo.RouteToProfessionalStatus.TrainingAgeSpecialismRangeFrom,
            TrainingAgeSpecialismRangeTo = routeInfo.RouteToProfessionalStatus.TrainingAgeSpecialismRangeTo,
            TrainingCountryId = routeInfo.RouteToProfessionalStatus.TrainingCountryId,
            TrainingEndDate = routeInfo.RouteToProfessionalStatus.TrainingEndDate,
            TrainingProviderId = routeInfo.RouteToProfessionalStatus.TrainingProviderId,
            TrainingStartDate = routeInfo.RouteToProfessionalStatus.TrainingStartDate,
            TrainingSubjectIds = routeInfo.RouteToProfessionalStatus.TrainingSubjectIds
        };

        ChangeReason = JourneyInstance!.State.ChangeReason;
        ChangeReasonDetail = JourneyInstance!.State.ChangeReasonDetail;
    }

    public async Task OnGetAsync()
    {
        RouteDetail.TrainingProvider = RouteDetail.TrainingProviderId is not null ? (await referenceDataCache.GetTrainingProviderByIdAsync(RouteDetail.TrainingProviderId!.Value))?.Name : null;
        RouteDetail.TrainingCountry = RouteDetail.TrainingCountryId is not null ? (await referenceDataCache.GetTrainingCountryByIdAsync(RouteDetail.TrainingCountryId))?.Name : null;
        RouteDetail.DegreeType = RouteDetail.DegreeTypeId is not null ? (await referenceDataCache.GetDegreeTypeByIdAsync(RouteDetail.DegreeTypeId!.Value))?.Name : null;
        RouteDetail.TrainingSubjects = await SubjectDisplayHelper.GetFormattedSubjectNamesAsync(RouteDetail.TrainingSubjectIds, referenceDataCache);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var professionalStatus = HttpContext.GetCurrentProfessionalStatusFeature().RouteToProfessionalStatus;
        var allRoutes = await referenceDataCache.GetRouteToProfessionalStatusTypesAsync(activeOnly: false);

        // ... or adapt the current all-in-one method
        professionalStatus.Delete(
            allRoutes,
            ChangeReason!.GetDisplayName(),
            ChangeReasonDetail.ChangeReasonDetail,
            ChangeReasonDetail.Evidence.UploadedEvidenceFile?.ToEventModel(),
            User.GetUserId(),
            clock.UtcNow,
            out var deletedEvent);
        if (deletedEvent is not null)
        {
            dbContext.AddEventWithoutBroadcast(deletedEvent);
            await dbContext.SaveChangesAsync();
        }

        await JourneyInstance!.CompleteAsync();

        TempData.SetFlashSuccess("Route to professional status deleted");

        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceController.DeleteUploadedFileAsync(JourneyInstance!.State.ChangeReasonDetail.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }
}
