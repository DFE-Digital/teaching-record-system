using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance, CheckProfessionalStatusExistsFilterFactory()]
public class CheckYourAnswersModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    IFileService fileService) : PageModel
{
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }

    public string? PersonName { get; set; }
    public Guid PersonId { get; private set; }
    public QualificationType? QualificationType { get; set; }
    public Guid RouteToProfessionalStatusId { get; set; }
    public ProfessionalStatusStatus Status { get; set; }
    public DateOnly? AwardedDate { get; set; }
    public DateOnly? TrainingStartDate { get; set; }
    public DateOnly? TrainingEndDate { get; set; }
    public Guid[]? TrainingSubjectIds { get; set; }
    public TrainingAgeSpecialismType? TrainingAgeSpecialismType { get; set; }
    public int? TrainingAgeSpecialismRangeFrom { get; set; }
    public int? TrainingAgeSpecialismRangeTo { get; set; }
    public string? TrainingAgeSpecialismRange => TrainingAgeSpecialismRangeFrom is not null ? $"From {TrainingAgeSpecialismRangeFrom} to {TrainingAgeSpecialismRangeTo}" : null;
    public string? TrainingCountryId { get; set; }
    public Guid? TrainingProviderId { get; set; }
    public Guid? InductionExemptionReasonId { get; set; }

    public ChangeReasonOption? ChangeReason;
    public ChangeReasonDetailsState ChangeReasonDetail { get; set; } = new();

    [FromRoute]
    public Guid QualificationId { get; set; }

    public string BackLink { get; set; } // CML TODO when I have a page to go back to
    public string? ExemptionReason { get; set; }
    public string? TrainingProvider { get; set; }
    public string? TrainingCountry { get; set; }
    public string[]? TrainingSubjects { get; set; }
    public string? RouteToProfessionalStatusName { get; set; }

    public async Task OnGetAsync()
    {
        RouteToProfessionalStatusName = (await referenceDataCache.GetRouteToProfessionalStatusByIdAsync(RouteToProfessionalStatusId))?.Name!;
        ExemptionReason = InductionExemptionReasonId is not null ? (await referenceDataCache.GetInductionExemptionReasonByIdAsync(InductionExemptionReasonId!.Value))?.Name : null;
        TrainingProvider = TrainingProviderId is not null ? (await referenceDataCache.GetTrainingProviderByIdAsync(TrainingProviderId!.Value))?.Name : null;
        TrainingCountry = TrainingCountryId is not null ? (await referenceDataCache.GetTrainingCountryByIdAsync(TrainingCountryId))?.Name : null;
        TrainingSubjects = TrainingSubjectIds is not null ?
            TrainingSubjectIds
                .Join((await referenceDataCache.GetTrainingSubjectsAsync()), id => id, subject => subject.TrainingSubjectId, (_, subject) => subject.Name)
                .OrderByDescending(name => name)
                .ToArray() : null;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // save the changes

        // broadcast event

        await JourneyInstance!.CompleteAsync();

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
            //context.Result = Redirect(linkGenerator.RouteEditPage(QualificationId, JourneyInstance.InstanceId)); // CML TODo go back to the start when i have that page
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        ChangeReason = JourneyInstance!.State.ChangeReason;
        ChangeReasonDetail = JourneyInstance!.State.ChangeReasonDetail!;

        QualificationType = JourneyInstance!.State.QualificationType;
        RouteToProfessionalStatusId = JourneyInstance!.State.RouteToProfessionalStatusId;
        Status = JourneyInstance!.State.Status;
        AwardedDate = JourneyInstance!.State.AwardedDate;
        TrainingStartDate = JourneyInstance!.State.TrainingStartDate;
        TrainingEndDate = JourneyInstance!.State.TrainingEndDate;
        TrainingSubjectIds = JourneyInstance!.State.TrainingSubjectIds;
        TrainingAgeSpecialismType = JourneyInstance!.State.TrainingAgeSpecialismType;
        TrainingAgeSpecialismRangeFrom = JourneyInstance!.State.TrainingAgeSpecialismRangeFrom;
        TrainingAgeSpecialismRangeTo = JourneyInstance!.State.TrainingAgeSpecialismRangeTo;
        TrainingCountryId = JourneyInstance!.State.TrainingCountryId;
        TrainingProviderId = JourneyInstance!.State.TrainingProviderId;
        InductionExemptionReasonId = JourneyInstance!.State.InductionExemptionReasonId;

        await next();
    }

    public async Task<string?> GetEvidenceFileUrlAsync()
    {
        return ChangeReasonDetail.EvidenceFileId is not null ?
            await fileService.GetFileUrlAsync(ChangeReasonDetail.EvidenceFileId!.Value, FileUploadDefaults.FileUrlExpiry) :
            null;
    }
}
