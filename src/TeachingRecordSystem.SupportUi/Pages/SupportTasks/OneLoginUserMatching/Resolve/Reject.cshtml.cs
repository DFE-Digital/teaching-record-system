using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserMatching)]
public class Reject(
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

    private readonly InlineValidator<Reject> _validator = new()
    {
        v => v.RuleFor(m => m.Reason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(m => m.AdditionalDetails)
            .NotNull().WithMessage("Enter additional detail")
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount).WithMessage($"Additional detail must be {UiDefaults.ReasonDetailsMaxCharacterCount} characters or less")
            .When(m => m.Reason is OneLoginIdVerificationRejectReason.AnotherReason)
    };

    private SupportTask? _supportTask;

    [FromRoute]
    public required string SupportTaskReference { get; init; }

    [BindProperty]
    public OneLoginIdVerificationRejectReason? Reason { get; set; }

    [BindProperty]
    public string? AdditionalDetails { get; set; }

    public string? BackLink { get; set; }

    public void OnGet()
    {
        Reason = journey.State.RejectReason;
        AdditionalDetails = journey.State.RejectionAdditionalDetails;
        journey.State.ApplySavedModelStateValues(nameof(Reject), ModelState);
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
            linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.ConfirmReject(journey.InstanceId),
            state =>
            {
                state.RejectReason = Reason;
                state.RejectionAdditionalDetails = Reason is OneLoginIdVerificationRejectReason.AnotherReason ? AdditionalDetails : null;
                state.ClearSavedModelStateValues(nameof(Reject));
            });
    }

    private async Task<IActionResult> HandleSaveAndReturnAsync()
    {
        var savedJourneyState = this.CreateSavedJourneyState(
            nameof(Reject),
            journey.State,
            excludeKeys: ["Action", nameof(SupportTaskReference)]);

        var processContext = new ProcessContext(
            ProcessType.OneLoginUserIdVerificationSupportTaskSaving,
            timeProvider.UtcNow,
            User.GetUserId());

        await supportTaskService.SaveProgressAsync(
            new()
            {
                SupportTaskReference = _supportTask!.SupportTaskReference,
                SavedJourneyState = savedJourneyState
            },
            processContext);

        journey.DeleteInstance();

        if (featureProvider.IsEnabled("SupportTaskDashboard"))
        {
            return Redirect(linkGenerator.SupportTasks.SupportTaskDetail.Index(_supportTask.SupportTaskReference));
        }

        return Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification());
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        _supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        BackLink = journey.GetBackLink();
    }
}
