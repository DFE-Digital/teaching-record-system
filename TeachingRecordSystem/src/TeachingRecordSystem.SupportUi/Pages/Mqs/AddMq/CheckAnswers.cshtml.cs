using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsDbContext dbContext,
    SupportUiLinkGenerator linkGenerator,
    IClock clock,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    public JourneyInstance<AddMqState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public Guid ProviderId { get; set; }

    public string? ProviderName { get; set; }

    public MandatoryQualificationSpecialism Specialism { get; set; }

    public DateOnly StartDate { get; set; }

    public MandatoryQualificationStatus Status { get; set; }

    public DateOnly? EndDate { get; set; }

    public AddMqReasonOption AddReason { get; set; }

    public string? AddReasonDetail { get; set; }

    [BindProperty]
    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var qualification = MandatoryQualification.Create(
            PersonId,
            ProviderId,
            Specialism,
            Status,
            StartDate,
            EndDate,
            AddReason.GetDisplayName(),
            AddReasonDetail,
            evidenceFile: EvidenceFile?.ToEventModel(),
            User.GetUserId(),
            clock.UtcNow,
            out var createdEvent);

        dbContext.MandatoryQualifications.Add(qualification);
        await dbContext.AddEventAndBroadcastAsync(createdEvent);
        await dbContext.SaveChangesAsync();

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Mandatory qualification added");

        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.Mqs.AddMq.Provider(PersonId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
        ProviderId = JourneyInstance.State.ProviderId.Value;
        ProviderName = MandatoryQualificationProvider.GetById(ProviderId).Name;
        Specialism = JourneyInstance.State.Specialism.Value;
        StartDate = JourneyInstance.State.StartDate.Value;
        Status = JourneyInstance.State.Status.Value;
        EndDate = JourneyInstance.State.EndDate;
        AddReason = JourneyInstance.State.AddReason!.Value;
        AddReasonDetail = JourneyInstance.State.AddReasonDetail;
        EvidenceFile = JourneyInstance.State.Evidence.UploadedEvidenceFile;

        await next();
    }
}
