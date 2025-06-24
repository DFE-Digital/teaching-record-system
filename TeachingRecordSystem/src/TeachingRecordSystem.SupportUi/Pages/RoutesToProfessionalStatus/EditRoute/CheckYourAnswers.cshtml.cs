using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance, CheckRouteToProfessionalStatusExistsFilterFactory()]
public class CheckYourAnswersModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    IFileService fileService,
    IClock clock) : PageModel
{
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }

    public RouteDetailViewModel RouteDetail { get; set; } = null!;

    public string? PersonName { get; set; }
    public Guid PersonId { get; private set; }

    public ChangeReasonOption? ChangeReason;
    public ChangeReasonDetailsState ChangeReasonDetail { get; set; } = new();
    public string? UploadedEvidenceFileUrl { get; set; }

    public string BackLink => linkGenerator.RouteEditChangeReason(QualificationId, JourneyInstance!.InstanceId);

    [FromRoute]
    public Guid QualificationId { get; set; }

    public async Task OnGetAsync()
    {
        RouteDetail.IsExemptFromInduction = JourneyInstance!.State.IsExemptFromInduction;
        RouteDetail.TrainingProvider = RouteDetail.TrainingProviderId is not null ? (await referenceDataCache.GetTrainingProviderByIdAsync(RouteDetail.TrainingProviderId!.Value))?.Name : null;
        RouteDetail.TrainingCountry = RouteDetail.TrainingCountryId is not null ? (await referenceDataCache.GetTrainingCountryByIdAsync(RouteDetail.TrainingCountryId))?.Name : null;
        RouteDetail.DegreeType = RouteDetail.DegreeTypeId is not null ? (await referenceDataCache.GetDegreeTypeByIdAsync(RouteDetail.DegreeTypeId!.Value))?.Name : null;
        RouteDetail.TrainingSubjects = await SubjectDisplayHelper.GetFormattedSubjectNamesAsync(RouteDetail.TrainingSubjectIds, referenceDataCache);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var professionalStatus = HttpContext.GetCurrentProfessionalStatusFeature().RouteToProfessionalStatus;
        var allRoutes = await referenceDataCache.GetRouteToProfessionalStatusTypesAsync(activeOnly: false);

        professionalStatus.Update(
            allRoutes,
            r =>
            {
                r.Status = RouteDetail.Status;
                r.RouteToProfessionalStatusTypeId = RouteDetail.RouteToProfessionalStatusType.RouteToProfessionalStatusTypeId;
                r.HoldsFrom = RouteDetail.HoldsFrom;
                r.TrainingStartDate = RouteDetail.TrainingStartDate;
                r.TrainingEndDate = RouteDetail.TrainingEndDate;
                r.TrainingSubjectIds = RouteDetail.TrainingSubjectIds ?? [];
                r.TrainingAgeSpecialismType = RouteDetail.TrainingAgeSpecialismType;
                r.TrainingAgeSpecialismRangeFrom = RouteDetail.TrainingAgeSpecialismRangeFrom;
                r.TrainingAgeSpecialismRangeTo = RouteDetail.TrainingAgeSpecialismRangeTo;
                r.TrainingCountryId = RouteDetail.TrainingCountryId;
                r.TrainingProviderId = RouteDetail.TrainingProviderId;
                r.ExemptFromInduction = RouteDetail.IsExemptFromInduction;
                r.DegreeTypeId = RouteDetail.DegreeTypeId;
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
        var route = await referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId);
        var status = JourneyInstance!.State.Status;

        if (!IsComplete(route, status) || !JourneyInstance!.State.ChangeReasonIsComplete)
        {
            context.Result = Redirect(linkGenerator.RouteEditDetail(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        ChangeReason = JourneyInstance!.State.ChangeReason;
        ChangeReasonDetail = JourneyInstance!.State.ChangeReasonDetail;
        var hasImplicitExemption = route.InductionExemptionReason?.RouteImplicitExemption ?? false;
        RouteDetail = new RouteDetailViewModel
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
            QualificationId = QualificationId,
            DegreeTypeId = JourneyInstance!.State.DegreeTypeId,
            HasImplicitExemption = hasImplicitExemption,
            IsExemptFromInduction = JourneyInstance!.State.IsExemptFromInduction,
            FromCheckAnswers = true,
            JourneyInstanceId = JourneyInstance.InstanceId
        };

        UploadedEvidenceFileUrl = ChangeReasonDetail.EvidenceFileId is not null ?
            await fileService.GetFileUrlAsync(ChangeReasonDetail.EvidenceFileId!.Value, FileUploadDefaults.FileUrlExpiry) :
            null;
        await next();
    }


    private bool IsComplete(RouteToProfessionalStatusType route, RouteToProfessionalStatusStatus status)
    {
        var errors = NotCompleteError.None |
            ((QuestionDriverHelper.FieldRequired(route.TrainingEndDateRequired, status.GetEndDateRequirement()) == FieldRequirement.Mandatory
                && JourneyInstance!.State.TrainingEndDate is null) ? NotCompleteError.StartDate : NotCompleteError.None) |
            ((QuestionDriverHelper.FieldRequired(route.TrainingEndDateRequired, status.GetEndDateRequirement()) == FieldRequirement.Mandatory
                && JourneyInstance!.State.TrainingEndDate is null) ? NotCompleteError.EndDate : NotCompleteError.None) |
            ((QuestionDriverHelper.FieldRequired(route.HoldsFromRequired, status.GetHoldsFromRequirement()) == FieldRequirement.Mandatory
                && JourneyInstance!.State.HoldsFrom is null) ? NotCompleteError.AwardDate : NotCompleteError.None) |
            ((QuestionDriverHelper.FieldRequired(route.DegreeTypeRequired, status.GetDegreeTypeRequirement()) == FieldRequirement.Mandatory
                && JourneyInstance!.State.DegreeTypeId is null) ? NotCompleteError.DegreeType : NotCompleteError.None) |
            ((QuestionDriverHelper.FieldRequired(route.TrainingAgeSpecialismTypeRequired, status.GetAgeSpecialismRequirement()) == FieldRequirement.Mandatory
                && (JourneyInstance!.State.TrainingAgeSpecialismRangeFrom is null || JourneyInstance!.State.TrainingAgeSpecialismRangeTo is null)
                && JourneyInstance!.State.TrainingAgeSpecialismType is null) ? NotCompleteError.AgeSpecialism : NotCompleteError.None) |
            ((QuestionDriverHelper.FieldRequired(route.TrainingCountryRequired, status.GetCountryRequirement()) == FieldRequirement.Mandatory
                && JourneyInstance!.State.TrainingCountryId is null) ? NotCompleteError.Country : NotCompleteError.None) |
            ((QuestionDriverHelper.FieldRequired(route.TrainingSubjectsRequired, status.GetSubjectsRequirement()) == FieldRequirement.Mandatory
                && !JourneyInstance!.State.TrainingSubjectIds.IsNullOrEmpty()) ? NotCompleteError.Subject : NotCompleteError.None) |
            ((QuestionDriverHelper.FieldRequired(route.TrainingProviderRequired, status.GetTrainingProviderRequirement()) == FieldRequirement.Mandatory
                && JourneyInstance!.State.TrainingProviderId is null) ? NotCompleteError.Provider : NotCompleteError.None) |
            ((QuestionDriverHelper.FieldRequired(route.InductionExemptionRequired, status.GetInductionExemptionRequirement()) == FieldRequirement.Mandatory
                && JourneyInstance!.State.IsExemptFromInduction is null) ? NotCompleteError.InductionExemption : NotCompleteError.None);

        return errors == NotCompleteError.None;
    }

    enum NotCompleteError
    {
        None = 0,
        StartDate = 1,
        EndDate = 1 << 1,
        AwardDate = 1 << 2,
        DegreeType = 1 << 3,
        AgeSpecialism = 1 << 4,
        Country = 1 << 5,
        Subject = 1 << 6,
        Provider = 1 << 7,
        InductionExemption = 1 << 8
    }
}
