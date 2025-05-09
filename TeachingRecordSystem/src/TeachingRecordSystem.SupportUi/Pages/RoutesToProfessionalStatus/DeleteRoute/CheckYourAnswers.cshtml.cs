using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

[Journey(JourneyNames.DeleteRouteToProfessionalStatus), RequireJourneyInstance, CheckProfessionalStatusExistsFilterFactory()]
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

    public string BackLink => linkGenerator.RouteChangeReason(QualificationId, JourneyInstance!.InstanceId);

    [FromRoute]
    public Guid QualificationId { get; set; }

    public async Task OnGetAsync()
    {
        RouteDetail.TrainingProvider = RouteDetail.TrainingProviderId is not null ? (await referenceDataCache.GetTrainingProviderByIdAsync(RouteDetail.TrainingProviderId!.Value))?.Name : null;
        RouteDetail.TrainingCountry = RouteDetail.TrainingCountryId is not null ? (await referenceDataCache.GetTrainingCountryByIdAsync(RouteDetail.TrainingCountryId))?.Name : null;
        RouteDetail.DegreeType = RouteDetail.DegreeTypeId is not null ? (await referenceDataCache.GetDegreeTypeByIdAsync(RouteDetail.DegreeTypeId!.Value))?.Name : null;
        RouteDetail.TrainingSubjects = RouteDetail.TrainingSubjectIds is not null ?
            RouteDetail.TrainingSubjectIds
                .Join((await referenceDataCache.GetTrainingSubjectsAsync()), id => id, subject => subject.TrainingSubjectId, (_, subject) => subject.Name)
                .OrderByDescending(name => name)
                .ToArray() : null;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var professionalStatus = HttpContext.GetCurrentProfessionalStatusFeature().ProfessionalStatus;


        //if (deletedEvent is not null)
        //{
        //    await dbContext.AddEventAndBroadcastAsync(deletedEvent);
        //    await dbContext.SaveChangesAsync();
        //}

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
            context.Result = Redirect(linkGenerator.DeleteRouteChangeReason(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        var routeInfo = context.HttpContext.GetCurrentProfessionalStatusFeature();
        RouteDetail = new RouteDetailViewModel()
        {
            RouteToProfessionalStatus = routeInfo.ProfessionalStatus.Route,
            AwardedDate = routeInfo.ProfessionalStatus.AwardedDate,
            DegreeTypeId = routeInfo.ProfessionalStatus.DegreeTypeId,
            IsExemptFromInduction = routeInfo.ProfessionalStatus.ExemptFromInduction,
            Status = routeInfo.ProfessionalStatus.Status,
            QualificationId = routeInfo.ProfessionalStatus.QualificationId,
            TrainingAgeSpecialismType = routeInfo.ProfessionalStatus.TrainingAgeSpecialismType,
            TrainingAgeSpecialismRangeFrom = routeInfo.ProfessionalStatus.TrainingAgeSpecialismRangeFrom,
            TrainingAgeSpecialismRangeTo = routeInfo.ProfessionalStatus.TrainingAgeSpecialismRangeTo,
            TrainingCountryId = routeInfo.ProfessionalStatus.TrainingCountryId,
            TrainingEndDate = routeInfo.ProfessionalStatus.TrainingEndDate,
            TrainingProviderId = routeInfo.ProfessionalStatus.TrainingProviderId,
            TrainingStartDate = routeInfo.ProfessionalStatus.TrainingStartDate,
            TrainingSubjectIds = routeInfo.ProfessionalStatus.TrainingSubjectIds
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
