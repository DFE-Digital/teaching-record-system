using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

[Journey(JourneyNames.ResolveTpsPotentialDuplicate), RequireJourneyInstance]
public class KeepRecordSeparateModel(TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator, EvidenceUploadManager evidenceController) : ResolveTeacherPensionsPotentialDuplicatePageModel(dbContext)
{
    [BindProperty]
    public string? Reason { get; set; }

    [BindProperty]
    public KeepingRecordSeparateReason? KeepSeparateReason { get; set; }

    public void OnGet()
    {
        KeepSeparateReason = JourneyInstance!.State.KeepSeparateReason;
    }

    public bool? FromCheckDetails { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (KeepSeparateReason == KeepingRecordSeparateReason.AnotherReason && string.IsNullOrEmpty(Reason))
        {
            ModelState.AddModelError($"{nameof(Reason)}.{Reason}", "Enter Reason");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.Reason = Reason;
            state.KeepSeparateReason = KeepSeparateReason;
        });

        return Redirect(linkGenerator.SupportTasks.TeacherPensions.Resolve.ConfirmKeepRecordSeparateReason(SupportTaskReference!, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceController.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.SupportTasks.TeacherPensions.Index());
    }
}
