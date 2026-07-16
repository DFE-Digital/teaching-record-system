using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.SupportTasks.TeacherPensions;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

[Journey(JourneyNames.ResolveTpsPotentialDuplicate), RequireJourneyInstance]
public class ConfirmKeepRecordSeparateReasonModel(
    TrsDbContext dbContext,
    TrnRequestService trnRequestService,
    TeacherPensionsSupportTaskService teacherPensionsSupportTaskService,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceController,
    TimeProvider timeProvider) : ResolveTeacherPensionsPotentialDuplicatePageModel(dbContext)
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

        var processContext = new ProcessContext(ProcessType.TeacherPensionsDuplicateSupportTaskResolvingWithoutMerge, timeProvider.UtcNow, User.GetUserId());

        await trnRequestService.ResolveTrnRequestWithMatchedPersonAsync(trnRequest, supportTask.PersonId!.Value, processContext);

        await teacherPensionsSupportTaskService.ResolveWithoutMergeAsync(
            new()
            {
                SupportTaskReference = SupportTaskReference,
                Comments = Reason
            },
            processContext);

        TempData.SetFlashNotificationBanner(
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
