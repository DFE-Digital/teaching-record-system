using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Pages.Shared;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance, CheckProfessionalStatusExistsFilterFactory()]
public class CheckYourAnswersModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    IFileService fileService,
    IClock clock) : PageModel
{
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }

    public RouteDetailViewModel RouteDetail { get; set; } = new();

    public string? PersonName { get; set; }
    public Guid PersonId { get; private set; }

    public ChangeReasonOption? ChangeReason;
    public ChangeReasonDetailsState ChangeReasonDetail { get; set; } = new();

    public string BackLink => linkGenerator.RouteChangeReason(QualificationId, JourneyInstance!.InstanceId);

    [FromRoute]
    public Guid QualificationId { get; set; }

    public async Task OnGetAsync()
    {
        var routeToProfessionalStatus = await referenceDataCache.GetRouteToProfessionalStatusByIdAsync(RouteDetail.RouteToProfessionalStatusId);
        RouteDetail.EndDateRequired = QuestionDriverHelper.FieldRequired(routeToProfessionalStatus.TrainingEndDateRequired, JourneyInstance!.State.Status.GetEndDateRequirement());
        RouteDetail.RouteToProfessionalStatusName = (await referenceDataCache.GetRouteToProfessionalStatusByIdAsync(RouteDetail.RouteToProfessionalStatusId))?.Name!;
        RouteDetail.ExemptionReason = RouteDetail.InductionExemptionReasonId is not null ? (await referenceDataCache.GetInductionExemptionReasonByIdAsync(RouteDetail.InductionExemptionReasonId!.Value))?.Name : null;
        RouteDetail.TrainingProvider = RouteDetail.TrainingProviderId is not null ? (await referenceDataCache.GetTrainingProviderByIdAsync(RouteDetail.TrainingProviderId!.Value))?.Name : null;
        RouteDetail.TrainingCountry = RouteDetail.TrainingCountryId is not null ? (await referenceDataCache.GetTrainingCountryByIdAsync(RouteDetail.TrainingCountryId))?.Name : null;
        RouteDetail.DegreeType = RouteDetail.DegreeTypeId is not null ? (await referenceDataCache.GetDegreeTypeByIdAsync(RouteDetail.DegreeTypeId!.Value))?.Name : null;
        RouteDetail.TrainingSubjects = RouteDetail.TrainingSubjectIds is not null ?
            RouteDetail.TrainingSubjectIds
                .Join((await referenceDataCache.GetTrainingSubjectsAsync()), id => id, subject => subject.TrainingSubjectId, (_, subject) => subject.Name)
                .OrderByDescending(name => name)
                .ToArray() : null;
        RouteDetail.FromCheckAnswers = true;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var professionalStatus = HttpContext.GetCurrentProfessionalStatusFeature().ProfessionalStatus;
        var professionalStatusType = (await referenceDataCache.GetRouteToProfessionalStatusByIdAsync(RouteDetail.RouteToProfessionalStatusId)).ProfessionalStatusType;
        professionalStatus.Update(
            s =>
            {
                s.Status = RouteDetail.Status;
                s.RouteToProfessionalStatusId = RouteDetail.RouteToProfessionalStatusId;
                s.AwardedDate = RouteDetail.AwardedDate;
                s.TrainingStartDate = RouteDetail.TrainingStartDate;
                s.TrainingEndDate = RouteDetail.TrainingEndDate;
                s.TrainingSubjectIds = RouteDetail.TrainingSubjectIds ?? [];
                s.TrainingAgeSpecialismType = RouteDetail.TrainingAgeSpecialismType;
                s.TrainingAgeSpecialismRangeFrom = RouteDetail.TrainingAgeSpecialismRangeFrom;
                s.TrainingAgeSpecialismRangeTo = RouteDetail.TrainingAgeSpecialismRangeTo;
                s.TrainingCountryId = RouteDetail.TrainingCountryId;
                s.TrainingProviderId = RouteDetail.TrainingProviderId;
                s.InductionExemptionReasonId = RouteDetail.InductionExemptionReasonId;
            },
            changeReason: ChangeReason?.GetDisplayName(),
            ChangeReasonDetail.ChangeReasonDetail,
            evidenceFile: JourneyInstance!.State.ChangeReasonDetail.EvidenceFileId is Guid fileId ?
                new EventModels.File()
                {
                    FileId = fileId,
                    Name = JourneyInstance.State.ChangeReasonDetail.EvidenceFileName!
                } :
                null,
            User.GetUserId(),
            clock.UtcNow,
            out var updatedEvent);

        if (updatedEvent is not null)
        {
            await dbContext.AddEventAndBroadcastAsync(updatedEvent);
            await dbContext.SaveChangesAsync();
        }

        await JourneyInstance!.CompleteAsync();

        TempData.SetFlashSuccess("Route to professional status updated");

        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        JourneyInstance!.State.EnsureInitialized(context.HttpContext.GetCurrentProfessionalStatusFeature());
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.RouteDetail(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        ChangeReason = JourneyInstance!.State.ChangeReason;
        ChangeReasonDetail = JourneyInstance!.State.ChangeReasonDetail!;

        RouteDetail = new RouteDetailViewModel
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

    public async Task<string?> GetEvidenceFileUrlAsync()
    {
        return ChangeReasonDetail.EvidenceFileId is not null ?
            await fileService.GetFileUrlAsync(ChangeReasonDetail.EvidenceFileId!.Value, FileUploadDefaults.FileUrlExpiry) :
            null;
    }
}
