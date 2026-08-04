using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserMatching)]
public class NotConnecting(
    ResolveOneLoginUserMatchingJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    SupportTaskService supportTaskService,
    TimeProvider timeProvider,
    IFeatureProvider featureProvider) : PageModel
{
    public static class Actions
    {
        public const string SaveAndComeBackLater = nameof(SaveAndComeBackLater);
        public const string Cancel = nameof(Cancel);
    }

    private InlineValidator<NotConnecting> _validator = new()
    {
        v => v.RuleFor(x => x.Reason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(x => x.AdditionalDetails)
            .NotNull().WithMessage("Enter additional detail")
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount).WithMessage($"Additional detail must be {UiDefaults.ReasonDetailsMaxCharacterCount} characters or less")
            .When(x => x.Reason is OneLoginUserNotConnectingReason.AnotherReason)
    };

    private SupportTask? _supportTask;

    [FromRoute]
    public required string SupportTaskReference { get; init; }

    [BindProperty]
    public OneLoginUserNotConnectingReason? Reason { get; set; }

    [BindProperty]
    public string? AdditionalDetails { get; set; }

    public string? BackLink { get; set; }

    public void OnGet()
    {
        Reason = journey.State.NotConnectingReason;
        AdditionalDetails = journey.State.NotConnectingAdditionalDetails;
        journey.State.ApplySavedModelStateValues(nameof(NotConnecting), ModelState);
    }

    public async Task<IActionResult> OnPostAsync(string? action)
    {
        if (action is Actions.Cancel)
        {
            journey.DeleteInstance();

            return Redirect(journey.State.CompletionUrl);
        }

        if (action is Actions.SaveAndComeBackLater)
        {
            return await HandleSaveAndReturnAsync();
        }

        await this.ThrowIfInvalidAsync(_validator);

        return journey.AdvanceTo(
            linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.ConfirmNotConnecting(journey.InstanceId),
            state =>
            {
                state.NotConnectingReason = Reason;
                state.NotConnectingAdditionalDetails = Reason is OneLoginUserNotConnectingReason.AnotherReason ? AdditionalDetails : null;
                state.ClearSavedModelStateValues(nameof(NotConnecting));
            });
    }

    private async Task<IActionResult> HandleSaveAndReturnAsync()
    {
        var savedJourneyState = this.CreateSavedJourneyState(
            nameof(NotConnecting),
            journey.State,
            excludeKeys: ["Action", nameof(SupportTaskReference)]);

        var processType = _supportTask!.SupportTaskType is SupportTaskType.OneLoginUserIdVerification ?
            ProcessType.OneLoginUserIdVerificationSupportTaskSaving :
            ProcessType.OneLoginUserRecordMatchingSupportTaskSaving;

        var processContext = new ProcessContext(processType, timeProvider.UtcNow, User.GetUserId());

        await supportTaskService.SaveProgressAsync(
            new()
            {
                SupportTaskReference = _supportTask.SupportTaskReference,
                SavedJourneyState = savedJourneyState
            },
            processContext);

        journey.DeleteInstance();

        if (featureProvider.IsEnabled("SupportTaskDashboard"))
        {
            return Redirect(linkGenerator.SupportTasks.SupportTaskDetail.Index(_supportTask.SupportTaskReference));
        }

        return Redirect(journey.State.CompletionUrl);
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        _supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        BackLink = journey.GetBackLink();
    }
}
