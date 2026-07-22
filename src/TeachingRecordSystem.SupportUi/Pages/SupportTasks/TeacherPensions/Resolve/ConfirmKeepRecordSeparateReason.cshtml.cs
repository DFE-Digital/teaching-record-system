using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.SupportTasks.TeacherPensions;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

[Journey(JourneyNames.ResolveTpsPotentialDuplicate)]
public class ConfirmKeepRecordSeparateReasonModel(
    ResolveTeacherPensionsPotentialDuplicateJourneyCoordinator journey,
    TrsDbContext dbContext,
    TeacherPensionsSupportTaskService teacherPensionsSupportTaskService,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceController,
    TimeProvider timeProvider) : ResolveTeacherPensionsPotentialDuplicatePageModel(journey, dbContext)
{
    [BindProperty]
    public bool Cancel { get; set; }

    public string? Reason { get; set; }

    public KeepingRecordSeparateReason? KeepSeparateReason { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            await evidenceController.DeleteUploadedFileAsync(Journey.State.Evidence.UploadedEvidenceFile);
            Journey.DeleteInstance();

            return Redirect(linkGenerator.SupportTasks.TeacherPensions.Index());
        }

        // Conditionally override the value in Reason.
        // if KeepSeparateReason is AnotherReason - the event will contain the reason provided from the user
        // if RecordDoesNotMatch, override reason using the display name from the enum
        if (Journey.State.KeepSeparateReason == KeepingRecordSeparateReason.RecordDoesNotMatch)
        {
            Reason = Journey.State.KeepSeparateReason.GetDisplayName()!;
        }
        else if (Journey.State.KeepSeparateReason == KeepingRecordSeparateReason.AnotherReason)
        {
            Reason = Journey.State.Reason;
        }

        var processContext = new ProcessContext(ProcessType.TeacherPensionsDuplicateSupportTaskResolvingWithoutMerge, timeProvider.UtcNow, User.GetUserId());

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

        Journey.DeleteInstance();

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

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        BackLink = Journey.GetBackLink();

        Reason = Journey.State.Reason;
        KeepSeparateReason = Journey.State.KeepSeparateReason;
    }
}
