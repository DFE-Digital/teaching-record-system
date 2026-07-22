using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

[Journey(JourneyNames.ResolveTpsPotentialDuplicate)]
public class KeepRecordSeparateModel(
    ResolveTeacherPensionsPotentialDuplicateJourneyCoordinator journey,
    TrsDbContext dbContext,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceController) : ResolveTeacherPensionsPotentialDuplicatePageModel(journey, dbContext)
{
    [BindProperty]
    public string? Reason { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public KeepingRecordSeparateReason? KeepSeparateReason { get; set; }

    public void OnGet()
    {
        KeepSeparateReason = Journey.State.KeepSeparateReason;
        Reason = Journey.State.Reason;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            await evidenceController.DeleteUploadedFileAsync(Journey.State.Evidence.UploadedEvidenceFile);
            Journey.DeleteInstance();

            return Redirect(linkGenerator.SupportTasks.TeacherPensions.Index());
        }

        if (KeepSeparateReason == KeepingRecordSeparateReason.AnotherReason && string.IsNullOrEmpty(Reason))
        {
            ModelState.AddModelError($"{nameof(Reason)}.{Reason}", "Enter Reason");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        return Journey.AdvanceTo(
            linkGenerator.SupportTasks.TeacherPensions.Resolve.ConfirmKeepRecordSeparateReason(Journey.InstanceId),
            state =>
            {
                state.Reason = Reason;
                state.KeepSeparateReason = KeepSeparateReason;
            });
    }

    public override void OnPageHandlerExecuting(Microsoft.AspNetCore.Mvc.Filters.PageHandlerExecutingContext context)
    {
        BackLink = Journey.GetBackLink() ?? linkGenerator.SupportTasks.TeacherPensions.Index();
    }
}
