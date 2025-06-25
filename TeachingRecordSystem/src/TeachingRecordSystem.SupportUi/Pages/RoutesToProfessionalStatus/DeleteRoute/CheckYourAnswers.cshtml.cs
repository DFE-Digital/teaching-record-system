using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

[Journey(JourneyNames.DeleteRouteToProfessionalStatus), RequireJourneyInstance, CheckRouteToProfessionalStatusExistsFilterFactory()]
public class CheckYourAnswersModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    IFileService fileService,
    IClock clock) : PageModel
{
    public JourneyInstance<DeleteRouteState>? JourneyInstance { get; set; }

    public RouteDetailViewModel RouteDetail { get; set; } = null!;

    public string? PersonName { get; set; }
    public Guid PersonId { get; private set; }

    public ChangeReasonOption? ChangeReason;
    public ChangeReasonDetailsState ChangeReasonDetail { get; set; } = new();

    public string BackLink => linkGenerator.RouteDeleteChangeReason(QualificationId, JourneyInstance!.InstanceId);

    [FromRoute]
    public Guid QualificationId { get; set; }

    public async Task OnGetAsync()
    {
        RouteDetail.TrainingProvider = RouteDetail.TrainingProviderId is not null ? (await referenceDataCache.GetTrainingProviderByIdAsync(RouteDetail.TrainingProviderId!.Value))?.Name : null;
        RouteDetail.TrainingCountry = RouteDetail.TrainingCountryId is not null ? (await referenceDataCache.GetTrainingCountryByIdAsync(RouteDetail.TrainingCountryId))?.Name : null;
        RouteDetail.DegreeType = RouteDetail.DegreeTypeId is not null ? (await referenceDataCache.GetDegreeTypeByIdAsync(RouteDetail.DegreeTypeId!.Value))?.Name : null;
        RouteDetail.TrainingSubjects = await SubjectDisplayHelper.GetFormattedSubjectNamesAsync(RouteDetail.TrainingSubjectIds, referenceDataCache);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!JourneyInstance!.State.Completed)
        {
            return Redirect(linkGenerator.RouteDeleteChangeReason(QualificationId, JourneyInstance!.InstanceId, fromCheckAnswers: true));
        }
        var professionalStatus = HttpContext.GetCurrentProfessionalStatusFeature().RouteToProfessionalStatus;
        var allRoutes = await referenceDataCache.GetRouteToProfessionalStatusTypesAsync(activeOnly: false);

        // ... or adapt the current all-in-one method
        professionalStatus.Delete(
            allRoutes,
            ChangeReason!.GetDisplayName(),
            ChangeReasonDetail.ChangeReasonDetail,
            ChangeReasonDetail.EvidenceFileId is Guid fileId
                ? new EventModels.File()
                {
                    FileId = fileId,
                    Name = ChangeReasonDetail.EvidenceFileName!
                }
                : null,
            User.GetUserId(),
            clock.UtcNow,
            out var deletedEvent);
        if (deletedEvent is not null)
        {
            await dbContext.AddEventAndBroadcastAsync(deletedEvent);
            await dbContext.SaveChangesAsync();
        }

        await JourneyInstance!.CompleteAsync();

        TempData.SetFlashSuccess("Route to professional status deleted");

        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!JourneyInstance!.State.ChangeReasonIsComplete)
        {
            context.Result = Redirect(linkGenerator.RouteDeleteChangeReason(QualificationId, JourneyInstance.InstanceId));
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

        await next();
    }

    public async Task<string?> GetEvidenceFileUrlAsync()
    {
        return ChangeReasonDetail.EvidenceFileId is not null ?
            await fileService.GetFileUrlAsync(ChangeReasonDetail.EvidenceFileId!.Value, FileUploadDefaults.FileUrlExpiry) :
            null;
    }

}
