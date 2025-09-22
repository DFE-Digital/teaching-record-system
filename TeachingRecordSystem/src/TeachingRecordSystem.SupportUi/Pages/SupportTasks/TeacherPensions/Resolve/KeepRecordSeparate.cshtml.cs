using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

[Journey(JourneyNames.ResolveTpsPotentialDuplicate), RequireJourneyInstance]
public class KeepRecordSeparate(TrsDbContext dbContext, TrsLinkGenerator linkGenerator) : ResolveTeacherPensionsPotentialDuplicatePageModel(dbContext)
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

        return Redirect(linkGenerator.TeacherPensionsConfirmKeepRecordSeparateReason(SupportTaskReference!, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.TeacherPensions());
    }
}
