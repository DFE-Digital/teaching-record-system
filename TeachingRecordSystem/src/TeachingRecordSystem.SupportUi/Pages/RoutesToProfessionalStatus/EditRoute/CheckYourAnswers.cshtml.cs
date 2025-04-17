using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
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
        RouteDetail.IsExemptFromInduction = JourneyInstance!.State.IsExemptFromInduction;
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
        var professionalStatusType = (await referenceDataCache.GetRouteToProfessionalStatusByIdAsync(RouteDetail.RouteToProfessionalStatus.RouteToProfessionalStatusId)).ProfessionalStatusType;
        professionalStatus.Update(
            s =>
            {
                s.Status = RouteDetail.Status;
                s.RouteToProfessionalStatusId = RouteDetail.RouteToProfessionalStatus.RouteToProfessionalStatusId;
                s.AwardedDate = RouteDetail.AwardedDate;
                s.TrainingStartDate = RouteDetail.TrainingStartDate;
                s.TrainingEndDate = RouteDetail.TrainingEndDate;
                s.TrainingSubjectIds = RouteDetail.TrainingSubjectIds ?? [];
                s.TrainingAgeSpecialismType = RouteDetail.TrainingAgeSpecialismType;
                s.TrainingAgeSpecialismRangeFrom = RouteDetail.TrainingAgeSpecialismRangeFrom;
                s.TrainingAgeSpecialismRangeTo = RouteDetail.TrainingAgeSpecialismRangeTo;
                s.TrainingCountryId = RouteDetail.TrainingCountryId;
                s.TrainingProviderId = RouteDetail.TrainingProviderId;
                s.ExemptFromInduction = RouteDetail.IsExemptFromInduction;
                s.DegreeTypeId = RouteDetail.DegreeTypeId;
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
        var route = await referenceDataCache.GetRouteToProfessionalStatusByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId);
        var status = JourneyInstance!.State.Status;

        if (!IsComplete(route, status) || !JourneyInstance!.State.ChangeReasonIsComplete)
        {
            context.Result = Redirect(linkGenerator.RouteDetail(QualificationId, JourneyInstance.InstanceId));
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
            QualificationType = JourneyInstance!.State.QualificationType,
            RouteToProfessionalStatus = route,
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
            QualificationId = QualificationId,
            DegreeTypeId = JourneyInstance!.State.DegreeTypeId,
            HasImplicitExemption = hasImplicitExemption,
            IsExemptFromInduction = JourneyInstance!.State.IsExemptFromInduction,
            FromCheckAnswers = true,
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

    private bool IsComplete(RouteToProfessionalStatus route, ProfessionalStatusStatus status)
    {
        return (QuestionDriverHelper.FieldRequired(route.TrainingStartDateRequired, status.GetStartDateRequirement()) != FieldRequirement.Mandatory
            || JourneyInstance!.State.TrainingStartDate is not null) &&
            (QuestionDriverHelper.FieldRequired(route.TrainingEndDateRequired, status.GetEndDateRequirement()) != FieldRequirement.Mandatory
            || JourneyInstance!.State.TrainingEndDate is not null) &&
            (QuestionDriverHelper.FieldRequired(route.AwardDateRequired, status.GetAwardDateRequirement()) != FieldRequirement.Mandatory
            || JourneyInstance!.State.AwardedDate is not null) &&
            (QuestionDriverHelper.FieldRequired(route.DegreeTypeRequired, status.GetDegreeTypeRequirement()) != FieldRequirement.Mandatory
            || JourneyInstance!.State.DegreeTypeId is not null) &&
            (QuestionDriverHelper.FieldRequired(route.TrainingAgeSpecialismTypeRequired, status.GetAgeSpecialismRequirement()) != FieldRequirement.Mandatory
            || (JourneyInstance!.State.TrainingAgeSpecialismRangeFrom is not null && JourneyInstance!.State.TrainingAgeSpecialismRangeTo is not null) || JourneyInstance!.State.TrainingAgeSpecialismType is not null) &&
            (QuestionDriverHelper.FieldRequired(route.TrainingCountryRequired, status.GetCountryRequirement()) != FieldRequirement.Mandatory
            || JourneyInstance!.State.TrainingCountryId is not null) &&
            (QuestionDriverHelper.FieldRequired(route.TrainingSubjectsRequired, status.GetSubjectsRequirement()) != FieldRequirement.Mandatory
            || !JourneyInstance!.State.TrainingSubjectIds.IsNullOrEmpty()) &&
            (QuestionDriverHelper.FieldRequired(route.TrainingProviderRequired, status.GetTrainingProviderRequirement()) != FieldRequirement.Mandatory
            || JourneyInstance!.State.TrainingProviderId is not null) &&
            (QuestionDriverHelper.FieldRequired(route.InductionExemptionRequired, status.GetInductionExemptionRequirement()) != FieldRequirement.Mandatory
            || JourneyInstance!.State.IsExemptFromInduction is not null);
    }
}
