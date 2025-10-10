using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

[Journey(JourneyNames.SetStatus), RequireJourneyInstance]
[AllowDeactivatedPerson]
public class CheckAnswersModel(
    SupportUiLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    IClock clock,
    EvidenceUploadManager evidenceController)
    : CommonJourneyPage(dbContext, linkGenerator, evidenceController)
{
    public DeactivateReasonOption? DeactivateReason { get; set; }
    public string? DeactivateReasonDetail { get; set; }
    public ReactivateReasonOption? ReactivateReason { get; set; }
    public string? ReactivateReasonDetail { get; set; }
    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public string BackLink => LinkGenerator.Persons.PersonDetail.SetStatus.Reason(PersonId, TargetStatus, JourneyInstance!.InstanceId);
    public string ChangeReasonLink => LinkGenerator.Persons.PersonDetail.SetStatus.Reason(PersonId, TargetStatus, JourneyInstance!.InstanceId, fromCheckAnswers: true);

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        var state = JourneyInstance!.State;

        if (!state.IsComplete)
        {
            context.Result = Redirect(LinkGenerator.Persons.PersonDetail.SetStatus.Reason(PersonId, TargetStatus, JourneyInstance.InstanceId));
            return;
        }

        DeactivateReason = state.DeactivateReason;
        DeactivateReasonDetail = state.DeactivateReasonDetail;
        ReactivateReason = state.ReactivateReason;
        ReactivateReasonDetail = state.ReactivateReasonDetail;
        EvidenceFile = state.Evidence.UploadedEvidenceFile;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var now = clock.UtcNow;

        Person!.SetStatus(
            TargetStatus,
            TargetStatus == PersonStatus.Deactivated
                ? DeactivateReason!.GetDisplayName()
                : ReactivateReason!.GetDisplayName(),
            TargetStatus == PersonStatus.Deactivated
                ? DeactivateReasonDetail
                : ReactivateReasonDetail,
            EvidenceFile?.ToEventModel(),
            User.GetUserId(),
            now,
            out var @event);

        if (@event is not null)
        {
            await DbContext.AddEventAndBroadcastAsync(@event);
            await DbContext.SaveChangesAsync();
        }

        await JourneyInstance!.CompleteAsync();

        var action = TargetStatus == PersonStatus.Deactivated ? "deactivated" : "reactivated";
        TempData.SetFlashSuccess($"{PersonName}\u2019s record has been {action}");

        return Redirect(LinkGenerator.Persons.PersonDetail.Index(PersonId));
    }
}
