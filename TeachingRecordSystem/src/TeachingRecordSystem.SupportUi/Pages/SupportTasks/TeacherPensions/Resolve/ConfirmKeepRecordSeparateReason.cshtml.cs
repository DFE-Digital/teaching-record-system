using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

[Journey(JourneyNames.ResolveTpsPotentialDuplicate), RequireJourneyInstance]
public class ConfirmKeepRecordSeparateReasonModel(
    TrsDbContext dbContext,
    TrnRequestService trnRequestService,
    SupportTaskService supportTaskService,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceController,
    IClock clock) : ResolveTeacherPensionsPotentialDuplicatePageModel(dbContext)
{
    public string? Reason { get; set; }

    public KeepingRecordSeparateReason? KeepSeparateReason { get; set; }

    public void OnGet()
    {
        Reason = JourneyInstance!.State.Reason;
        KeepSeparateReason = JourneyInstance!.State.KeepSeparateReason;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Conditionally override the value in Reason.
        // if KeepSeparateReason is AnotherReason - the event will contain the reason provided from the user
        // if RecordDoesNotMatch, override reason using the display name from the enum
        if (JourneyInstance!.State.KeepSeparateReason == KeepingRecordSeparateReason.RecordDoesNotMatch)
        {
            Reason = JourneyInstance!.State.KeepSeparateReason.GetDisplayName()!;
        }
        else if (JourneyInstance!.State.KeepSeparateReason == KeepingRecordSeparateReason.AnotherReason)
        {
            Reason = JourneyInstance!.State.Reason;
        }

        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var trnRequest = supportTask.TrnRequestMetadata!;

        var processContext = new ProcessContext(ProcessType.TeacherPensionsDuplicateSupportTaskResolvingWithoutMerge, clock.UtcNow, User.GetUserId());

        var person = await DbContext.Persons.FindAsync(supportTask.PersonId) ?? throw new InvalidOperationException($"Person with ID {supportTask.PersonId} not found.");
        await trnRequestService.ResolveTrnRequestWithMatchedPersonAsync(trnRequest, (person.PersonId, person.Trn), processContext);

        await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<TeacherPensionsPotentialDuplicateData>
            {
                SupportTask = SupportTaskReference,
                UpdateData = data => data with
                {
                    ResolvedAttributes = null,
                    SelectedPersonAttributes = null
                },
                Status = SupportTaskStatus.Closed,
                Comments = Reason
            },
            processContext);

        TempData.SetFlashSuccess(
            "Teachers’ Pensions duplicate task completed",
            "The records were not merged.");

        await JourneyInstance!.CompleteAsync();
        return Redirect(linkGenerator.SupportTasks.TeacherPensions.Index());
    }

    public string GetReason()
    {
        if (KeepSeparateReason == KeepingRecordSeparateReason.AnotherReason)
        {
            return Reason!;
        }
        else if (KeepSeparateReason == KeepingRecordSeparateReason.RecordDoesNotMatch)
        {
            return KeepSeparateReason.Value.GetDisplayName()!;
        }
        return string.Empty;
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceController.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.SupportTasks.TeacherPensions.Index());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (JourneyInstance!.State.KeepSeparateReason is null ||
            JourneyInstance!.State.KeepSeparateReason == KeepingRecordSeparateReason.AnotherReason && string.IsNullOrEmpty(JourneyInstance!.State.Reason))
        {
            context.Result = Redirect(linkGenerator.SupportTasks.TeacherPensions.Index());
            return;
        }
        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
