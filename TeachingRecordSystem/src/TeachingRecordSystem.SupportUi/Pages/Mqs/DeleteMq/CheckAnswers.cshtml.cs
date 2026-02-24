using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;

[Journey(JourneyNames.DeleteMq), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsDbContext dbContext,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceController,
    IClock clock) : PageModel
{
    public JourneyInstance<DeleteMqState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public string? ProviderName { get; set; }

    public MandatoryQualificationSpecialism? Specialism { get; set; }

    public MandatoryQualificationStatus? Status { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public MqDeletionReasonOption? DeletionReason { get; set; }

    public string? DeletionReasonDetail { get; set; }

    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.Mqs.DeleteMq.Index(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        var qualificationInfo = context.HttpContext.GetCurrentMandatoryQualificationFeature();
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        ProviderName = qualificationInfo.MandatoryQualification.Provider?.Name;
        Specialism = qualificationInfo.MandatoryQualification.Specialism;
        Status = qualificationInfo.MandatoryQualification.Status;
        StartDate = qualificationInfo.MandatoryQualification.StartDate;
        EndDate = qualificationInfo.MandatoryQualification.EndDate;
        DeletionReason = JourneyInstance.State.DeletionReason;
        DeletionReasonDetail = JourneyInstance.State.DeletionReasonDetail;
        EvidenceFile = JourneyInstance.State.Evidence.UploadedEvidenceFile;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var qualification = HttpContext.GetCurrentMandatoryQualificationFeature().MandatoryQualification;

        qualification.Delete(
            DeletionReason!.GetDisplayName(),
            DeletionReasonDetail,
            EvidenceFile?.ToEventModel(),
            User.GetUserId(),
            clock.UtcNow,
            out var deletedEvent);

        dbContext.AddEventWithoutBroadcast(deletedEvent);
        await dbContext.SaveChangesAsync();

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Mandatory qualification deleted");

        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceController.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }
}
