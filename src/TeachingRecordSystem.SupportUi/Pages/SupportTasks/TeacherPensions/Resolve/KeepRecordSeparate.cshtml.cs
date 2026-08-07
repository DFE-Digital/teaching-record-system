using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

[Journey(JourneyNames.ResolveTpsPotentialDuplicate)]
public class KeepRecordSeparateModel(
    ResolveTeacherPensionsPotentialDuplicateJourneyCoordinator journey,
    TrsDbContext dbContext,
    SupportTaskService supportTaskService,
    TimeProvider timeProvider,
    IFeatureProvider featureProvider,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceController) : ResolveTeacherPensionsPotentialDuplicatePageModel(journey, dbContext)
{
    public static class Actions
    {
        public const string SaveAndComeBackLater = nameof(SaveAndComeBackLater);
        public const string Cancel = nameof(Cancel);
    }

    [BindProperty]
    public string? Reason { get; set; }

    [BindProperty]
    public KeepingRecordSeparateReason? KeepSeparateReason { get; set; }

    public void OnGet()
    {
        Journey.State.ApplySavedModelStateValues(nameof(KeepRecordSeparateModel), ModelState);
        KeepSeparateReason = Journey.State.KeepSeparateReason;
        Reason = Journey.State.Reason;
    }

    public async Task<IActionResult> OnPostAsync(string? action)
    {
        if (action is Actions.Cancel)
        {
            return await CancelAsync();
        }

        if (action is Actions.SaveAndComeBackLater)
        {
            return await HandleSaveAndReturnAsync();
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
                state.ClearSavedModelStateValues(nameof(KeepRecordSeparateModel));
            });
    }

    private async Task<IActionResult> HandleSaveAndReturnAsync()
    {
        var supportTask = GetSupportTask();

        var savedJourneyState = this.CreateSavedJourneyState(
            nameof(KeepRecordSeparateModel),
            Journey.State,
            excludeKeys: ["Action", nameof(SupportTaskReference)]);

        var processContext = new ProcessContext(ProcessType.TeacherPensionsSupportTaskSaving, timeProvider.UtcNow, User.GetUserId());

        await supportTaskService.SaveProgressAsync(
            new()
            {
                SupportTaskReference = supportTask.SupportTaskReference,
                SavedJourneyState = savedJourneyState
            },
            processContext);

        Journey.DeleteInstance();

        if (featureProvider.IsEnabled("SupportTaskDashboard"))
        {
            return Redirect(linkGenerator.SupportTasks.SupportTaskDetail.Index(supportTask.SupportTaskReference));
        }

        return Redirect(Journey.State.CompletionUrl);
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceController.DeleteUploadedFileAsync(Journey.State.Evidence.UploadedEvidenceFile);
        Journey.DeleteInstance();

        return Redirect(Journey.State.CompletionUrl);
    }

    public override void OnPageHandlerExecuting(Microsoft.AspNetCore.Mvc.Filters.PageHandlerExecutingContext context)
    {
        BackLink = Journey.GetBackLink() ?? Journey.State.CompletionUrl;
    }
}
