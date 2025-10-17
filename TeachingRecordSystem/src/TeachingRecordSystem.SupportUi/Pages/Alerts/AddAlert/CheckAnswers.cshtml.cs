using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

[Journey(JourneyNames.AddAlert), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsDbContext dbContext,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager,
    IClock clock) : PageModel
{
    public JourneyInstance<AddAlertState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string? PersonName { get; set; }

    public Guid AlertTypeId { get; set; }

    public string? AlertTypeName { get; set; }

    public string? Details { get; set; }

    public string? Link { get; set; }

    public Uri? LinkUri { get; set; }

    public DateOnly StartDate { get; set; }

    public AddAlertReasonOption AddReason { get; set; }

    public string? AddReasonDetail { get; set; }

    [BindProperty]
    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.Alerts.AddAlert.Reason(PersonId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
        AlertTypeId = JourneyInstance.State.AlertTypeId!.Value;
        AlertTypeName = JourneyInstance.State.AlertTypeName;
        Details = JourneyInstance.State.Details;
        Link = JourneyInstance.State.Link;
        LinkUri = TrsUriHelper.TryCreateWebsiteUri(Link, out var linkUri) ? linkUri : null;
        StartDate = JourneyInstance.State.StartDate!.Value;
        AddReason = JourneyInstance.State.AddReason!.Value;
        AddReasonDetail = JourneyInstance.State.AddReasonDetail;
        EvidenceFile = JourneyInstance.State.Evidence.UploadedEvidenceFile;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var now = clock.UtcNow;

        var alert = Alert.Create(
            AlertTypeId,
            PersonId,
            Details,
            Link,
            StartDate,
            endDate: null,
            AddReason.GetDisplayName(),
            AddReasonDetail,
            evidenceFile: EvidenceFile?.ToEventModel(),
            User.GetUserId(),
            now,
            out var createdEvent);

        dbContext.Alerts.Add(alert);
        await dbContext.AddEventAndBroadcastAsync(createdEvent);
        await dbContext.SaveChangesAsync();

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Alert added");

        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }
}
